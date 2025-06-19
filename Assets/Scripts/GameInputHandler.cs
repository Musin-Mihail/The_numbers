using UnityEngine;

public class GameInputHandler : MonoBehaviour
{
    private GameController _gameController;

    public void Initialize(GameController gameController)
    {
        _gameController = gameController;
    }

    public void SubscribeToCell(Cell cell)
    {
        if (cell)
        {
            cell.OnCellClicked += HandleCellClick;
        }
    }

    public void UnsubscribeFromCell(Cell cell)
    {
        if (cell)
        {
            cell.OnCellClicked -= HandleCellClick;
        }
    }

    private void HandleCellClick(Cell cell)
    {
        _gameController?.HandleCellSelection(cell);
    }
}