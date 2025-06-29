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
        [SerializeField] private WindowSwiper windowSwiper;
        [SerializeField] private ConfirmationDialog confirmationDialog;
        [SerializeField] private Toggle topLineToggle;

        private GameController _gameController;
        private GridModel _gridModel;

        private void Awake()
        {
            _gridModel = new GridModel();
            var gridDataProvider = new GridDataProvider(_gridModel);
            var calculatingMatches = new MatchValidator(gridDataProvider);
            _gameController = new GameController(_gridModel, calculatingMatches);
            view.Initialize(_gridModel, headerNumberDisplay, windowSwiper);
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
                GameEvents.OnRequestNewGame += () => confirmationDialog.Show("Начать новую игру?", StartNewGameInternal);
            }
            else
            {
                GameEvents.OnRequestNewGame += StartNewGameInternal;
            }
        }

        private void OnDestroy()
        {
            if (topLineToggle)
            {
                topLineToggle.onValueChanged.RemoveListener(GameEvents.RaiseToggleTopLine);
            }

            if (confirmationDialog)
            {
                GameEvents.OnRequestNewGame -= () => confirmationDialog.Show("Начать новую игру?", StartNewGameInternal);
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