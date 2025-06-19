using System;

public class CellData
{
    public readonly Guid Id = Guid.NewGuid();
    public readonly int Number;
    public int Line;
    public readonly int Column;
    public bool IsActive { get; private set; } = true;

    public CellData(int number, int line, int column)
    {
        Number = number;
        Line = line;
        Column = column;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}