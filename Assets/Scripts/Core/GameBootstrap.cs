using System;
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
    [RequireComponent(typeof(GameManager))]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene Dependencies")]
        [SerializeField] private GridView view;
        [SerializeField] private HeaderNumberDisplay headerNumberDisplay;
        [SerializeField] private ConfirmationDialog confirmationDialog;
        [SerializeField] private Toggle topLineToggle;

        [Header("Event Channels")]
        [SerializeField] private GameEvents gameEvents;

        [Header("Leaderboard Settings")]
        [SerializeField] private string leaderboardName = "TotalScore";

        private Action _requestNewGameAction;
        private GameManager _gameManager;
        private GameController _gameController;

        private void Awake()
        {
            _gameManager = GetComponent<GameManager>();
            if (gameEvents && gameEvents.onYandexSDKInitialized)
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

        private void RegisterModelsAndHistory()
        {
            ServiceProvider.Register(new GridModel());
            ServiceProvider.Register(new StatisticsModel());
            ServiceProvider.Register(new ActionCountersModel());
            ServiceProvider.Register(new ActionHistory());
        }

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

        private (IGridDataProvider gridDataProvider, MatchValidator matchValidator) RegisterGameplayLogic()
        {
            var gridModel = ServiceProvider.GetService<GridModel>();
            var gridDataProvider = new GridDataProvider(gridModel);
            ServiceProvider.Register<IGridDataProvider>(gridDataProvider);

            var matchValidator = new MatchValidator(gridDataProvider);
            ServiceProvider.Register(matchValidator);

            return (gridDataProvider, matchValidator);
        }

        private void RegisterViews()
        {
            ServiceProvider.Register(view);
            ServiceProvider.Register(headerNumberDisplay);
        }

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

        private void OnEnable()
        {
            YG2.onGetSDKData += OnYandexSDKInitialized;
        }

        private void OnDisable()
        {
            YG2.onGetSDKData -= OnYandexSDKInitialized;
        }

        private void OnYandexSDKInitialized()
        {
            gameEvents.onYandexSDKInitialized.Raise();
            if (!_gameManager) return;
            SetupListeners();
            var saveLoadService = ServiceProvider.GetService<ISaveLoadService>();
            saveLoadService.LoadGame(loadSuccess =>
            {
                if (loadSuccess) return;
                _gameController.StartNewGame(true);
                if (topLineToggle)
                {
                    gameEvents.onToggleTopLine.Raise(topLineToggle.isOn);
                }
            });
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
                _requestNewGameAction = () => confirmationDialog.Show("Начать новую игру?", StartNewGameInternal);
                gameEvents.onRequestNewGame.AddListener(_requestNewGameAction);
                gameEvents.onRequestRefillCounters.AddListener(HandleRequestRefillCounters);
                gameEvents.onRequestDisableCounters.AddListener(HandleRequestDisableCounters);
            }
            else
            {
                gameEvents.onRequestNewGame.AddListener(StartNewGameInternal);
            }
        }

        private void UpdateTopLineToggleState(bool isVisible)
        {
            if (topLineToggle)
            {
                topLineToggle.isOn = isVisible;
            }
        }

        private void HandleRequestDisableCounters()
        {
            confirmationDialog.Show("Отключить ограничения за плату?", () => { gameEvents.onDisableCountersConfirmed.Raise(); });
        }

        private void HandleRequestRefillCounters()
        {
            confirmationDialog.Show("Посмотреть рекламу, чтобы пополнить счетчики?", () => { gameEvents.onShowRewardedAdForRefill.Raise(); });
        }

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
                gameEvents.onRequestNewGame.RemoveListener(StartNewGameInternal);
            }

            _gameController?.Dispose();
            ServiceProvider.Clear();
        }

        private void StartNewGameInternal()
        {
            _gameController.StartNewGame(false);
            if (topLineToggle)
            {
                gameEvents.onToggleTopLine.Raise(topLineToggle.isOn);
            }
        }
    }
}