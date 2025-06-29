using System;
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
        [SerializeField] private GridView view;
        [SerializeField] private HeaderNumberDisplay headerNumberDisplay;
        [SerializeField] private ConfirmationDialog confirmationDialog;
        [SerializeField] private Toggle topLineToggle;

        private GameController _gameController;
        private GridModel _gridModel;
        private Action _requestNewGameAction;

        private void Awake()
        {
            _gridModel = new GridModel();
            var gridDataProvider = new GridDataProvider(_gridModel);
            var calculatingMatches = new MatchValidator(gridDataProvider);
            _gameController = new GameController(_gridModel, calculatingMatches, gridDataProvider);
            view.Initialize(_gridModel, headerNumberDisplay);
        }

        private void Start()
        {
            if (topLineToggle)
            {
                GameEvents.RaiseToggleTopLine(topLineToggle.isOn);
                topLineToggle.onValueChanged.AddListener(GameEvents.RaiseToggleTopLine);
            }
            else
            {
                GameEvents.RaiseToggleTopLine(true);
            }

            if (confirmationDialog)
            {
                _requestNewGameAction = () => confirmationDialog.Show("Начать новую игру?", StartNewGameInternal);
                GameEvents.OnRequestNewGame += _requestNewGameAction;
                GameEvents.OnRequestRefillCounters += HandleRequestRefillCounters;
            }
            else
            {
                GameEvents.OnRequestNewGame += StartNewGameInternal;
            }
        }

        private void HandleRequestRefillCounters()
        {
            confirmationDialog.Show("Добавить количество к cчетчикам?", () => { GameEvents.RaiseRefillCountersConfirmed(); });
        }

        private void OnDestroy()
        {
            if (topLineToggle)
            {
                topLineToggle.onValueChanged.RemoveListener(GameEvents.RaiseToggleTopLine);
            }

            if (confirmationDialog)
            {
                if (_requestNewGameAction != null)
                {
                    GameEvents.OnRequestNewGame -= _requestNewGameAction;
                }

                GameEvents.OnRequestRefillCounters -= HandleRequestRefillCounters;
            }
            else
            {
                GameEvents.OnRequestNewGame -= StartNewGameInternal;
            }
        }

        private void StartNewGameInternal()
        {
            _gameController.StartNewGame();
        }
    }
}