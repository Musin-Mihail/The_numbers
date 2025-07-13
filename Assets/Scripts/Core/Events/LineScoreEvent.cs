using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, передающее информацию о начислении очков за линию.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Line Score Event")]
    public class LineScoreEvent : BaseGameEvent<(int lineIndex, int score)>
    {
    }
}