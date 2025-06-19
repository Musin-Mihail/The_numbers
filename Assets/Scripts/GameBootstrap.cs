using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GeneratingPlayingField view;
    [SerializeField] private CellPool cellPool;
    [SerializeField] private TopLineController topLineController;
    [SerializeField] private CanvasSwiper canvasSwiper;

    private GameController _gameController;
    private GridModel _gridModel;

    private void Awake()
    {
        _gridModel = new GridModel(
            cellPool,
            cell => view.SubscribeToCell(cell),
            cell => view.UnsubscribeFromCell(cell)
        );
        var calculatingMatches = new CalculatingMatches(_gridModel);
        _gameController = new GameController(_gridModel, calculatingMatches);
        view.Initialize(_gameController, _gridModel, topLineController, canvasSwiper);
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