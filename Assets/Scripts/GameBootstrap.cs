using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GeneratingPlayingField view;
    [SerializeField] private TopLineController topLineController;
    [SerializeField] private CanvasSwiper canvasSwiper;
    [SerializeField] private GameInputHandler inputHandler;

    private GameController _gameController;
    private GridModel _gridModel;

    private void Awake()
    {
        _gridModel = new GridModel(view);
        var calculatingMatches = new CalculatingMatches(_gridModel);
        _gameController = new GameController(_gridModel, calculatingMatches);
        view.Initialize(_gridModel, topLineController, canvasSwiper, inputHandler);
        inputHandler.Initialize(_gameController);
    }

    private void OnEnable()
    {
        _gameController.OnMatchFound += view.HandleMatchFound;
        _gameController.OnInvalidMatch += view.HandleInvalidMatch;
        _gameController.OnGridChanged += view.HandleGridChanged;
    }

    private void OnDisable()
    {
        _gameController.OnMatchFound -= view.HandleMatchFound;
        _gameController.OnInvalidMatch -= view.HandleInvalidMatch;
        _gameController.OnGridChanged -= view.HandleGridChanged;
    }

    public void StartNewGame()
    {
        _gameController.StartNewGame();
    }

    public void AddExistingNumbersAsNewLines()
    {
        _gameController.AddExistingNumbersAsNewLines();
    }
}