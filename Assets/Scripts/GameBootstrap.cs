using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GeneratingPlayingField view;
    [SerializeField] private TopLineController topLineController;
    [SerializeField] private CanvasSwiper canvasSwiper;

    private GameController _gameController;
    private GridModel _gridModel;

    private void Awake()
    {
        _gridModel = new GridModel();
        var calculatingMatches = new CalculatingMatches(_gridModel);
        _gameController = new GameController(_gridModel, calculatingMatches);
        view.Initialize(_gridModel, topLineController, canvasSwiper, _gameController);
        SubscribeToEvents();
    }

    private void OnEnable()
    {
        if (_gameController != null)
        {
            SubscribeToEvents();
        }
    }

    private void OnDisable()
    {
        if (_gameController != null)
        {
            UnsubscribeFromEvents();
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