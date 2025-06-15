using System;

public static class ActionBus
{
    public static event Action<Cell> OnSelectingCell;

    public static void SelectingCell(Cell cell)
    {
        OnSelectingCell?.Invoke(cell);
    }
}