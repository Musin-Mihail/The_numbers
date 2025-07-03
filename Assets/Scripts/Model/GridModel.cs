using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Events;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Model
{
    public class GridModel
    {
        private readonly List<List<CellData>> _cells = new();
        public IReadOnlyList<IReadOnlyList<CellData>> Cells => _cells;
        private readonly Dictionary<Guid, CellData> _cellDataMap = new();
        private readonly List<CellData> _activeCellsCache = new();
        private bool _isCacheDirty = true;

        private readonly GameEvents _gameEvents;

        public GridModel()
        {
            _gameEvents = ServiceProvider.GetService<GameEvents>();
        }

        public List<CellData> GetAllCellData()
        {
            return _cellDataMap.Values.ToList();
        }

        public void RestoreState(List<CellDataSerializable> savedCells)
        {
            ClearField();
            var maxLine = -1;
            if (savedCells == null || savedCells.Count == 0) return;

            var lines = savedCells.GroupBy(c => c.line)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var lineGroup in lines)
            {
                var lineIndex = lineGroup.Key;
                while (_cells.Count <= lineIndex)
                {
                    _cells.Add(new List<CellData>());
                }

                var lineData = new List<CellData>();
                foreach (var savedCell in lineGroup.OrderBy(c => c.column))
                {
                    var cellData = new CellData(savedCell.number, savedCell.line, savedCell.column);
                    cellData.SetActive(savedCell.isActive);
                    lineData.Add(cellData);
                    _cellDataMap[cellData.Id] = cellData;
                }

                _cells[lineIndex] = lineData;
                if (lineIndex > maxLine) maxLine = lineIndex;
            }

            while (_cells.Count <= maxLine)
            {
                _cells.Add(new List<CellData>());
            }

            _isCacheDirty = true;
        }


        private static void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public List<CellData> GetAllActiveCellData()
        {
            if (!_isCacheDirty) return _activeCellsCache;
            _activeCellsCache.Clear();
            _activeCellsCache.AddRange(_cellDataMap.Values.Where(cell => cell.IsActive));
            _activeCellsCache.Sort((a, b) =>
            {
                var lineComparison = a.Line.CompareTo(b.Line);
                return lineComparison != 0 ? lineComparison : a.Column.CompareTo(b.Column);
            });
            _isCacheDirty = false;
            return _activeCellsCache;
        }

        public CellData GetCellDataById(Guid id)
        {
            _cellDataMap.TryGetValue(id, out var data);
            return data;
        }

        public void SetCellActiveState(CellData data, bool isActive)
        {
            if (data.IsActive == isActive) return;
            data.SetActive(isActive);
            _isCacheDirty = true;
            _gameEvents?.onCellUpdated.Raise(data);
        }

        public void ClearField()
        {
            _cells.Clear();
            _cellDataMap.Clear();
            _isCacheDirty = true;
            _gameEvents?.onGridCleared.Raise();
        }

        public void CreateLine(int lineIndex)
        {
            var newLine = new List<CellData>();
            for (var i = 0; i < GameConstants.QuantityByWidth; i++)
            {
                var cellData = new CellData(Random.Range(1, 10), lineIndex, i);
                newLine.Add(cellData);
                _cellDataMap[cellData.Id] = cellData;
                _gameEvents?.onCellAdded.Raise((cellData, true));
            }

            _cells.Add(newLine);
            _isCacheDirty = true;
        }

        public bool IsLineEmpty(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= _cells.Count) return false;
            return _cells[lineIndex].All(cellData => !cellData.IsActive);
        }

        public void RemoveLine(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= _cells.Count) return;
            foreach (var cellData in _cells[lineIndex])
            {
                _cellDataMap.Remove(cellData.Id);
                _gameEvents?.onCellRemoved.Raise(cellData.Id);
            }

            _cells.RemoveAt(lineIndex);
            for (var i = lineIndex; i < _cells.Count; i++)
            {
                foreach (var cell in _cells[i])
                {
                    cell.Line = i;
                    _gameEvents?.onCellUpdated.Raise(cell);
                }
            }

            _isCacheDirty = true;
        }

        public void RestoreLine(int lineIndex, List<CellData> lineData)
        {
            if (lineIndex < 0 || lineIndex > _cells.Count) return;

            for (var i = lineIndex; i < _cells.Count; i++)
            {
                foreach (var cell in _cells[i])
                {
                    cell.Line++;
                    _gameEvents?.onCellUpdated.Raise(cell);
                }
            }

            _cells.Insert(lineIndex, lineData);
            foreach (var cellData in lineData)
            {
                _cellDataMap[cellData.Id] = cellData;
                _gameEvents?.onCellAdded.Raise((cellData, true));
            }

            _isCacheDirty = true;
        }

        public List<int> GetNumbersForTopLine(int numberLine)
        {
            var topNumbers = new List<int>(new int[GameConstants.QuantityByWidth]);
            if (numberLine < 0) return topNumbers;
            var activeCells = GetAllActiveCellData();
            numberLine = Mathf.Min(numberLine, _cells.Count);
            for (var col = 0; col < GameConstants.QuantityByWidth; col++)
            {
                for (var line = numberLine - 1; line >= 0; line--)
                {
                    var cellData = activeCells.LastOrDefault(d => d.Column == col && d.Line == line);
                    if (cellData == null) continue;
                    topNumbers[col] = cellData.Number;
                    break;
                }
            }

            return topNumbers;
        }

        public void AppendActiveNumbersToGrid()
        {
            var numbersToAdd = GetAllActiveCellData().Select(cell => cell.Number).ToList();
            if (numbersToAdd.Count == 0) return;
            Shuffle(numbersToAdd);

            var lineIndex = _cells.Count > 0 ? _cells.Count - 1 : 0;
            var columnIndex = 0;
            if (_cells.Count > 0 && _cells[lineIndex] != null)
            {
                columnIndex = _cells[lineIndex].Count;
            }

            if (_cells.Count == 0)
            {
                _cells.Add(new List<CellData>());
            }

            foreach (var number in numbersToAdd)
            {
                if (columnIndex >= GameConstants.QuantityByWidth)
                {
                    columnIndex = 0;
                    lineIndex++;
                    if (_cells.Count <= lineIndex)
                    {
                        _cells.Add(new List<CellData>());
                    }
                }

                var newCellData = new CellData(number, lineIndex, columnIndex);
                _cells[lineIndex].Add(newCellData);
                _cellDataMap[newCellData.Id] = newCellData;
                _gameEvents.onCellAdded.Raise((newCellData, false));
                columnIndex++;
            }

            _isCacheDirty = true;
        }
    }
}