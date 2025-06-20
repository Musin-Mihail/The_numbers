using DataProviders;
using Gameplay;
using Model;
using UnityEngine;
using View.Grid;
using View.UI;

namespace Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GridView view;
        [SerializeField] private HeaderNumberDisplay headerNumberDisplay;
        [SerializeField] private WindowSwiper windowSwiper;
        [SerializeField] private ConfirmationDialog confirmationDialog;

        private GameController _gameController;
        private GridModel _gridModel;

        private void Awake()
        {
            _gridModel = new GridModel();
            var gridDataProvider = new GridDataProvider(_gridModel);
            var calculatingMatches = new MatchValidator(gridDataProvider);
            _gameController = new GameController(_gridModel, calculatingMatches);
            view.Initialize(_gridModel, headerNumberDisplay, windowSwiper, _gameController);
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            if (_gameController != null)
            {
                UnsubscribeFromEvents();
            }
        }

        private void OnEnable()
        {
            if (_gameController != null)
            {
                SubscribeToEvents();
            }
        }

        private void SubscribeToEvents()
        {
            _gameController.OnMatchFound += view.HandleMatchFound;
            _gameController.OnInvalidMatch += view.HandleInvalidMatch;
            _gameController.OnActionUndone += HandleActionUndone;
        }

        private void UnsubscribeFromEvents()
        {
            _gameController.OnMatchFound -= view.HandleMatchFound;
            _gameController.OnInvalidMatch -= view.HandleInvalidMatch;
            _gameController.OnActionUndone -= HandleActionUndone;
        }

        private void HandleActionUndone()
        {
            if (windowSwiper)
            {
                windowSwiper.SwitchToWindowGame();
            }
        }

        private void StartNewGameInternal() => _gameController.StartNewGame();

        public void RequestNewGame()
        {
            if (confirmationDialog)
            {
                confirmationDialog.Show("Начать новую игру?", StartNewGameInternal);
            }
            else
            {
                Debug.LogWarning("ConfirmationDialog не назначен в GameBootstrap. Новая игра начнется немедленно.");
                StartNewGameInternal();
            }
        }

        public void AddExistingNumbersAsNewLines() => _gameController.AddExistingNumbersAsNewLines();
        public void UndoLastMove() => _gameController.UndoLastAction();
    }
}