using UnityEngine;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Events/Statistics Changed Event")]
    public class StatisticsChangedEvent : BaseGameEvent<(long score, int multiplier)>
    {
    }
}