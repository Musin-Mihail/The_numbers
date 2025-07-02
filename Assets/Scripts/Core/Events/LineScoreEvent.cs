using UnityEngine;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Events/Line Score Event")]
    public class LineScoreEvent : BaseGameEvent<(int lineIndex, int score)>
    {
    }
}