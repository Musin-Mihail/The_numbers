using System;
using System.Collections;
using Core.Events;
using Core.Platform;
using DataProviders;
using Gameplay;
using Model;
using UnityEngine;
using UnityEngine.UI;
using View.Grid;
using View.UI;
using YG;

namespace Core
{
    /// <summary>
    /// Основной класс для инициализации игры. Отвечает за создание и регистрацию всех
    /// основных сервисов, моделей и контроллеров при запуске сцены.
    /// </summary>
    [RequireComponent(typeof(GameManager))]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Зависимости сцены")]
        [SerializeField] private GridView view;
        [SerializeField] private HeaderNumberDisplay headerNumberDisplay;
        [SerializeField] private ConfirmationDialog confirmationDialog;
        [SerializeField] private Toggle topLineToggle;

        [Header("Экраны UI")]
        [SerializeField] private GameObject loadingScreen;

        [Header("Каналы событий")]
        [SerializeField] private GameEvents gameEvents;

        [Header("Настройки таблицы лидеров")]
        [SerializeField] private string leaderboardName = "TotalScore";

        private const int MaxLoadAttempts = 3;
        private const float LoadAttemptDelay = 1.0f;

        private Action _requestNewGameAction;
        private GameManager _gameManager;
        private GameController _gameController;

        /// <summary>
        /// Вызывается при запуске. Инициализирует все системы игры.
        /// </summary>
        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
            if (loadingScreen) loadingScreen.SetActive(true);
            if (gameEvents && gameEvents.onYandexSDKInitialized != null)
            {
                gameEvents.onYandexSDKInitialized.Reset();
            }

            if (!gameEvents)
            {
                Debug.LogError("ОШИБКА: 'GameEvents' не назначен в инспекторе!", this);
                return;
            }

            ServiceProvider.Clear();
            ServiceProvider.Register(gameEvents);

            RegisterModelsAndHistory();
            var platformServices = RegisterPlatformServices();
            var gameplayLogic = RegisterGameplayLogic();
            RegisterViews();

            _gameController = RegisterGameController(platformServices, gameplayLogic.matchValidator);
        }

        /// <summary>
        /// Регистрирует модели данных и историю действий.
        /// </summary>
        private void RegisterModelsAndHistory()
        {
            ServiceProvider.Register(new GridModel());
            ServiceProvider.Register(new StatisticsModel());
            ServiceProvider.Register(new ActionCountersModel());
            ServiceProvider.Register(new ActionHistory());
        }

        /// <summary>
        /// Регистрирует сервисы для взаимодействия с платформой (Yandex Games).
        /// </summary>
        /// <returns>Интерфейс платформенных сервисов.</returns>
        private IPlatformServices RegisterPlatformServices()
        {
            var gridModel = ServiceProvider.GetService<GridModel>();
            var statisticsModel = ServiceProvider.GetService<StatisticsModel>();
            var actionCountersModel = ServiceProvider.GetService<ActionCountersModel>();

            var yandexSaveLoadService = new YandexSaveLoadService(gridModel, statisticsModel, actionCountersModel, gameEvents);
            ServiceProvider.Register<ISaveLoadService>(yandexSaveLoadService);

            var yandexLeaderboardService = new YandexLeaderboardService(leaderboardName);
            ServiceProvider.Register<ILeaderboardService>(yandexLeaderboardService);

            var yandexPlatformService = new YandexPlatformService();
            ServiceProvider.Register<IPlatformServices>(yandexPlatformService);

            return yandexPlatformService;
        }

        /// <summary>
        /// Регистрирует основную игровую логику, такую как провайдер данных сетки и валидатор совпадений.
        /// </summary>
        /// <returns>Кортеж с провайдером данных и валидатором.</returns>
        private (IGridDataProvider gridDataProvider, MatchValidator matchValidator) RegisterGameplayLogic()
        {
            var gridModel = ServiceProvider.GetService<GridModel>();
            var gridDataProvider = new GridDataProvider(gridModel);
            ServiceProvider.Register<IGridDataProvider>(gridDataProvider);

            var matchValidator = new MatchValidator(gridDataProvider);
            ServiceProvider.Register(matchValidator);

            return (gridDataProvider, matchValidator);
        }

        /// <summary>
        /// Регистрирует компоненты представления (View).
        /// </summary>
        private void RegisterViews()
        {
            ServiceProvider.Register(view);
            ServiceProvider.Register(headerNumberDisplay);
        }

        /// <summary>
        /// Создает и регистрирует главный игровой контроллер.
        /// </summary>
        /// <param name="platformServices">Сервисы платформы.</param>
        /// <param name="matchValidator">Валидатор совпадений.</param>
        /// <returns>Созданный игровой контроллер.</returns>
        private GameController RegisterGameController(IPlatformServices platformServices, MatchValidator matchValidator)
        {
            var gameController = new GameController(
                ServiceProvider.GetService<GridModel>(),
                matchValidator,
                gameEvents,
                ServiceProvider.GetService<ActionHistory>(),
                ServiceProvider.GetService<ActionCountersModel>(),
                ServiceProvider.GetService<StatisticsModel>(),
                _gameManager,
                view,
                platformServices
            );
            ServiceProvider.Register(gameController);
            return gameController;
        }

        /// <summary>
        /// Подписывается на событие инициализации Yandex SDK.
        /// </summary>
        private void OnEnable()
        {
            YG2.onGetSDKData += OnYandexSDKInitialized;
        }

        /// <summary>
        /// Отписывается от события инициализации Yandex SDK.
        /// </summary>
        private void OnDisable()
        {
            YG2.onGetSDKData -= OnYandexSDKInitialized;
        }

        /// <summary>
        /// Вызывается после успешной инициализации Yandex SDK. Запускает загрузку с повторными попытками.
        /// </summary>
        private void OnYandexSDKInitialized()
        {
            gameEvents.onYandexSDKInitialized.Raise();
            if (!_gameManager) return;
            StartCoroutine(LoadGameWithRetries());
        }

        /// <summary>
        /// Корутина для загрузки игры с несколькими попытками.
        /// </summary>
        private IEnumerator LoadGameWithRetries()
        {
            var saveLoadService = ServiceProvider.GetService<ISaveLoadService>();
            var attempts = 0;
            var loadSuccess = false;

            while (attempts < MaxLoadAttempts && !loadSuccess)
            {
                attempts++;
                Debug.Log($"Попытка загрузки данных #{attempts}");

                var loadFinished = false;
                saveLoadService.LoadGame(success =>
                {
                    loadSuccess = success;
                    loadFinished = true;
                });

                yield return new WaitUntil(() => loadFinished);

                if (!loadSuccess && attempts < MaxLoadAttempts)
                {
                    Debug.LogWarning($"Загрузка не удалась. Повторная попытка через {LoadAttemptDelay} сек.");
                    yield return new WaitForSeconds(LoadAttemptDelay);
                }
            }

            Debug.Log(loadSuccess ? "Данные успешно загружены. Отображение сохраненного состояния." : "Сохраненные данные не найдены или не удалось их загрузить. Игра ожидает начала новой сессии от пользователя.");

            FinalizeGameSetup();
        }

        private void FinalizeGameSetup()
        {
            Debug.Log("Завершение настройки игры и активация UI.");
            SetupListeners();
            if (loadingScreen) loadingScreen.SetActive(false);
        }

        private void SetupListeners()
        {
            if (topLineToggle)
            {
                gameEvents.onToggleTopLine.AddListener(UpdateTopLineToggleState);
                topLineToggle.onValueChanged.AddListener(gameEvents.onToggleTopLine.Raise);
            }

            if (confirmationDialog)
            {
                _requestNewGameAction = () => confirmationDialog.Show("Начать новую игру?", StartNewGameFromButton, new Vector2(0, 350));
                gameEvents.onRequestNewGame.AddListener(_requestNewGameAction);
                gameEvents.onRequestRefillCounters.AddListener(HandleRequestRefillCounters);
                gameEvents.onRequestDisableCounters.AddListener(HandleRequestDisableCounters);
            }
            else
            {
                gameEvents.onRequestNewGame.AddListener(StartNewGameFromButton);
            }
        }

        /// <summary>
        /// Обновляет состояние переключателя верхней строки.
        /// </summary>
        /// <param name="isVisible">Новое состояние видимости.</param>
        private void UpdateTopLineToggleState(bool isVisible)
        {
            if (topLineToggle)
            {
                topLineToggle.isOn = isVisible;
            }
        }

        /// <summary>
        /// Обрабатывает запрос на отключение счетчиков, показывая диалог подтверждения.
        /// </summary>
        private void HandleRequestDisableCounters()
        {
            confirmationDialog.Show("Отключить ограничения за плату?", () => { gameEvents.onDisableCountersConfirmed.Raise(); }, new Vector2(0, 350));
        }

        /// <summary>
        /// Обрабатывает запрос на пополнение счетчиков, показывая диалог подтверждения.
        /// </summary>
        private void HandleRequestRefillCounters()
        {
            confirmationDialog.Show("Посмотреть рекламу, чтобы пополнить счетчики?", () => { gameEvents.onShowRewardedAdForRefill.Raise(); }, new Vector2(0, 350));
        }

        /// <summary>
        /// Вызывается при уничтожении объекта. Отписывается от всех событий.
        /// </summary>
        private void OnDestroy()
        {
            if (!gameEvents) return;
            if (topLineToggle)
            {
                gameEvents.onToggleTopLine.RemoveListener(UpdateTopLineToggleState);
                topLineToggle.onValueChanged.RemoveListener(gameEvents.onToggleTopLine.Raise);
            }

            if (confirmationDialog)
            {
                if (_requestNewGameAction != null)
                {
                    gameEvents.onRequestNewGame.RemoveListener(_requestNewGameAction);
                }

                gameEvents.onRequestRefillCounters.RemoveListener(HandleRequestRefillCounters);
                gameEvents.onRequestDisableCounters.RemoveListener(HandleRequestDisableCounters);
            }
            else
            {
                gameEvents.onRequestNewGame.RemoveListener(StartNewGameFromButton);
            }

            _gameController?.Dispose();
            ServiceProvider.Clear();
        }

        /// <summary>
        /// Вызывается по нажатию кнопки "Новая игра".
        /// </summary>
        private void StartNewGameFromButton()
        {
            _gameController.StartNewGame(false);
            if (topLineToggle)
            {
                gameEvents.onToggleTopLine.Raise(topLineToggle.isOn);
            }
        }
    }
}