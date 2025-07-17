using System;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using Core.Platform;
using Core.Shop;
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
    /// основных сервисов, моделей и контроллеров, а также за внедрение зависимостей.
    /// </summary>
    [RequireComponent(typeof(GameManager))]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Зависимости сцены")]
        [SerializeField] private GridView gridView;
        [SerializeField] private HeaderNumberDisplay headerNumberDisplay;
        [SerializeField] private ConfirmationDialog confirmationDialog;
        [SerializeField] private Toggle topLineToggle;
        [SerializeField] private LeaderboardUpdater leaderboardUpdater;
        [SerializeField] private ShopManager shopManager;

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
        private bool _isNewUser;

        private readonly List<IDisposable> _disposableServices = new();

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

            var gridModel = new GridModel();
            var statisticsModel = new StatisticsModel();
            var actionCountersModel = new ActionCountersModel();
            var actionHistory = new ActionHistory();
            ServiceProvider.Register(gridModel);
            ServiceProvider.Register(statisticsModel);
            ServiceProvider.Register(actionCountersModel);
            ServiceProvider.Register(actionHistory);

            var yandexSaveLoadService = new YandexSaveLoadService(gridModel, statisticsModel, actionCountersModel, gameEvents);
            var yandexLeaderboardService = new YandexLeaderboardService(leaderboardName);
            var yandexPlatformService = new YandexPlatformService();
            ServiceProvider.Register<ISaveLoadService>(yandexSaveLoadService);
            ServiceProvider.Register<ILeaderboardService>(yandexLeaderboardService);
            ServiceProvider.Register<IPlatformServices>(yandexPlatformService);
            _disposableServices.Add(yandexPlatformService);

            var gridDataProvider = new GridDataProvider(gridModel);
            var matchValidator = new MatchValidator(gridDataProvider);
            ServiceProvider.Register<IGridDataProvider>(gridDataProvider);
            ServiceProvider.Register(matchValidator);

            ServiceProvider.Register(gridView);
            ServiceProvider.Register(headerNumberDisplay);

            _gameController = new GameController(
                gridModel,
                matchValidator,
                gameEvents,
                actionHistory,
                actionCountersModel,
                statisticsModel,
                _gameManager,
                gridView,
                yandexPlatformService
            );
            ServiceProvider.Register(_gameController);
            _disposableServices.Add(_gameController);

            InjectDependencies();
        }

        /// <summary>
        /// Внедряет зависимости в компоненты MonoBehaviour, которые не могут получить их через конструктор.
        /// </summary>
        private void InjectDependencies()
        {
            var saveLoadService = ServiceProvider.GetService<ISaveLoadService>();
            var leaderboardService = ServiceProvider.GetService<ILeaderboardService>();
            var actionCountersModel = ServiceProvider.GetService<ActionCountersModel>();

            _gameManager.Initialize(saveLoadService);
            leaderboardUpdater.Initialize(leaderboardService, gameEvents);
            shopManager.Initialize(gameEvents, actionCountersModel);
            gridView.Initialize(gameEvents, ServiceProvider.GetService<GridModel>(), headerNumberDisplay);
        }

        /// <summary>
        /// Подписывается на события.
        /// </summary>
        private void OnEnable()
        {
            YG2.onGetSDKData += OnYandexSDKInitialized;
            YG2.onDefaultSaves += OnDefaultSavesReceived;
        }

        /// <summary>
        /// Отписывается от событий.
        /// </summary>
        private void OnDisable()
        {
            YG2.onGetSDKData -= OnYandexSDKInitialized;
            YG2.onDefaultSaves -= OnDefaultSavesReceived;
        }

        /// <summary>
        /// Вызывается, когда плагин определяет, что сохранений нет.
        /// </summary>
        private void OnDefaultSavesReceived()
        {
            _isNewUser = true;
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
        /// В случае неудачи - предлагает игроку выбор: повторить или начать новую игру.
        /// </summary>
        private IEnumerator LoadGameWithRetries()
        {
            if (_isNewUser)
            {
                Debug.Log("Плагин не нашел сохранений (onDefaultSaves). Запуск новой игры.");
                StartNewGameAndFinalize();
                yield break;
            }

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

            if (loadSuccess)
            {
                Debug.Log("Данные успешно загружены. Отображение сохраненного состояния.");
                gridView.FullRedraw();
                FinalizeGameSetup();
            }
            else
            {
                Debug.LogError("Не удалось загрузить данные после нескольких попыток. Показ диалога ошибки.");
                ShowLoadErrorDialog();
            }
        }

        /// <summary>
        /// Показывает диалог с ошибкой загрузки и вариантами действий.
        /// </summary>
        private void ShowLoadErrorDialog()
        {
            confirmationDialog.Show(
                "Не удалось загрузить данные. Проверьте интернет-соединение и попробуйте снова.",
                "Попробовать снова",
                "Новая игра",
                OnRetryLoad,
                OnStartNewGameWithWarning,
                new Vector2(0, 450)
            );
        }

        /// <summary>
        /// Обработчик для кнопки "Попробовать снова" в диалоге ошибки загрузки.
        /// </summary>
        private void OnRetryLoad()
        {
            Debug.Log("Игрок выбрал повторную попытку загрузки.");
            if (loadingScreen) loadingScreen.SetActive(true);
            StartCoroutine(LoadGameWithRetries());
        }

        /// <summary>
        /// Обработчик для кнопки "Новая игра" в диалоге ошибки загрузки.
        /// </summary>
        private void OnStartNewGameWithWarning()
        {
            Debug.Log("Игрок выбрал начать новую игру после ошибки загрузки.");
            StartNewGameAndFinalize();
        }

        /// <summary>
        /// Запускает новую игру со сбросом прогресса и финализирует настройку.
        /// </summary>
        private void StartNewGameAndFinalize()
        {
            _gameController.StartNewGame(true);
            if (topLineToggle)
            {
                gameEvents.onToggleTopLine.Raise(topLineToggle.isOn);
            }

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
                _requestNewGameAction = () => confirmationDialog.Show("Начать новую игру?", "Да", "Нет", StartNewGameFromButton, null, new Vector2(0, 350));
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
            confirmationDialog.Show("Отключить ограничения за плату?", "Да", "Нет", () => { gameEvents.onDisableCountersConfirmed.Raise(); }, null, new Vector2(0, 350));
        }

        /// <summary>
        /// Обрабатывает запрос на пополнение счетчиков, показывая диалог подтверждения.
        /// </summary>
        private void HandleRequestRefillCounters()
        {
            confirmationDialog.Show("Посмотреть рекламу, чтобы пополнить счетчики?", "Да", "Нет", () => { gameEvents.onShowRewardedAdForRefill.Raise(); }, null, new Vector2(0, 350));
        }

        /// <summary>
        /// Вызывается при уничтожении объекта. Отписывается от всех событий и освобождает ресурсы.
        /// </summary>
        private void OnDestroy()
        {
            if (gameEvents)
            {
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
            }

            foreach (var service in _disposableServices)
            {
                service.Dispose();
            }

            _disposableServices.Clear();
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