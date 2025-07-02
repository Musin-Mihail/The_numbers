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

        private GameController _gameController;
        private GridModel _gridModel;
        private Action _requestNewGameAction;

        private void Awake()
        {
            _gridModel = new GridModel();
            var gridDataProvider = new GridDataProvider(_gridModel);
            var calculatingMatches = new MatchValidator(gridDataProvider);
            _gameController = new GameController(
                _gridModel,
                calculatingMatches,
                gameEvents
            );
            view.Initialize(
                _gridModel,
                headerNumberDisplay,
                gameEvents
            );
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
        }

        private void StartNewGameInternal()
        {
            _gameController.StartNewGame();
        }
    }
}