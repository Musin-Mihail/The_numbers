using Model;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, передающее данные ячейки и флаг необходимости анимации.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Cell Data Animate Event")]
    public class CellDataAnimateEvent : BaseGameEvent<(CellData, bool)>
    {
    }
}