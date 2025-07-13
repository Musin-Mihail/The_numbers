using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, уведомляющее об изменении счетчиков действий.
    /// Передает кортеж с количеством отмен, добавлений и подсказок.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Counters Changed Event")]
    public class CountersChangedEvent : BaseGameEvent<(int undo, int add, int hint)>
    {
    }
}