using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GeneratingPlayingField view;
    [SerializeField] private TopLineController topLineController;
    [SerializeField] private WindowSwiper windowSwiper;

    private GameController _gameController;
    private GridModel _gridModel;
    private ActiveCellsManager _activeCellsManager;

    private void Awake()
    {
        _gridModel = new GridModel();
        _activeCellsManager = new ActiveCellsManager(_gridModel);
        var calculatingMatches = new CalculatingMatches(_activeCellsManager);

        _gameController = new GameController(_gridModel, calculatingMatches, _activeCellsManager);

        view.Initialize(_gridModel, topLineController, windowSwiper, _gameController, _activeCellsManager);
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        if (_gameController != null)
        {
            UnsubscribeFromEvents();
        }

        _activeCellsManager?.UnsubscribeEvents();
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