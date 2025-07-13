using Model;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, передающее данные одной ячейки (CellData).
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Cell Data Event")]
    public class CellDataEvent : BaseGameEvent<CellData>
    {
    }
}