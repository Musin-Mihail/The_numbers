using System;

public static class ActionBus
{
    public static event Action<Cell> OnSelectingCell;
    public static event Action<int, int> OnCheckLines;

    public static void SelectingCell(Cell cell)
    {
        OnSelectingCell?.Invoke(cell);
    }

    public static void CheckLines(int line1, int line2)
    {
        OnCheckLines?.Invoke(line1, line2);
    }
}