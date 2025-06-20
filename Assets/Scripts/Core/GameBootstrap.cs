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
        }

        private void UnsubscribeFromEvents()
        {
            _gameController.OnMatchFound -= view.HandleMatchFound;
            _gameController.OnInvalidMatch -= view.HandleInvalidMatch;
        }

        public void StartNewGame() => _gameController.StartNewGame();
        public void AddExistingNumbersAsNewLines() => _gameController.AddExistingNumbersAsNewLines();
    }
}