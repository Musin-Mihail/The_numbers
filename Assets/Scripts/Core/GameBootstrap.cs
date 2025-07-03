using System;
using Core.Events;
using DataProviders;
using Gameplay;
using Model;
using UnityEngine;
using UnityEngine.UI;
using View.Grid;
using View.UI;

namespace Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Scene Dependencies")]
        [SerializeField] private GridView view;
        [SerializeField] private HeaderNumberDisplay headerNumberDisplay;
        [SerializeField] private ConfirmationDialog confirmationDialog;
        [SerializeField] private Toggle topLineToggle;

        [Header("Event Channels")]
        [SerializeField] private GameEvents gameEvents;

        private Action _requestNewGameAction;
        private GameManager _gameManager;

        private void Awake()
        {
            if (!gameEvents)
            {
                Debug.LogError("ОШИБКА: 'GameEvents' не назначен в инспекторе для объекта " + gameObject.name + "! Пожалуйста, назначьте ScriptableObject 'GameEvents'.", this);
                return;
            }

            ServiceProvider.Clear();

            ServiceProvider.Register(gameEvents);
            var gridModel = new GridModel();
            ServiceProvider.Register(gridModel);
            var statisticsModel = new StatisticsModel();
            ServiceProvider.Register(statisticsModel);
            var actionCountersModel = new ActionCountersModel();
            ServiceProvider.Register(actionCountersModel);
            var actionHistory = new ActionHistory();
            ServiceProvider.Register(actionHistory);

            var gridDataProvider = new GridDataProvider(gridModel);
            ServiceProvider.Register<IGridDataProvider>(gridDataProvider);
            var matchValidator = new MatchValidator(gridDataProvider);
            ServiceProvider.Register(matchValidator);

            ServiceProvider.Register(view);
            ServiceProvider.Register(headerNumberDisplay);

            _gameManager = gameObject.AddComponent<GameManager>();
            ServiceProvider.Register(_gameManager);

            var gameController = new GameController();
            ServiceProvider.Register(gameController);
        }

        private void Start()
        {
            if (!_gameManager) return;

            SetupListeners();

            if (_gameManager.LoadGame()) return;
            var gameController = ServiceProvider.GetService<GameController>();
            gameController.StartNewGame(true);
            if (topLineToggle)
            {
                gameEvents.onToggleTopLine.Raise(topLineToggle.isOn);
            }
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
            if (topLineToggle != null)
            {
                topLineToggle.isOn = isVisible;
            }
        }

        private void HandleRequestDisableCounters()
        {
            confirmationDialog.Show("Отключить счётчики?", () => { gameEvents.onDisableCountersConfirmed.Raise(); });
        }

        private void HandleRequestRefillCounters()
        {
            confirmationDialog.Show("Добавить количество к cчетчикам?", () => { gameEvents.onRefillCountersConfirmed.Raise(); });
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

            ServiceProvider.Clear();
        }

        private void StartNewGameInternal()
        {
            var gameController = ServiceProvider.GetService<GameController>();
            gameController.StartNewGame(false);
            if (topLineToggle)
            {
                gameEvents.onToggleTopLine.Raise(topLineToggle.isOn);
            }
        }
    }
}