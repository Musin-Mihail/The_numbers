using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, уведомляющее об изменении статистики (счет, множитель).
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Statistics Changed Event")]
    public class StatisticsChangedEvent : BaseGameEvent<(long score, int multiplier)>
    {
    }
}