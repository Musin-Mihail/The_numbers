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

        private void Awake()
        {
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

            var gameController = new GameController();
            ServiceProvider.Register(gameController);

            ServiceProvider.Register(view);
            ServiceProvider.Register(headerNumberDisplay);
        }

        private void Start()
        {
            if (topLineToggle)
            {
                gameEvents.onToggleTopLine.Raise(topLineToggle.isOn);
                topLineToggle.onValueChanged.AddListener(gameEvents.onToggleTopLine.Raise);
            }
            else
            {
                gameEvents.onToggleTopLine.Raise(true);
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
            if (topLineToggle)
            {
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
            gameController.StartNewGame();

            if (topLineToggle)
            {
                gameEvents.onToggleTopLine.Raise(topLineToggle.isOn);
            }
        }
    }
}