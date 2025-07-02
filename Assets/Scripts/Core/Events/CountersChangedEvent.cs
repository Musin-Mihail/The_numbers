using UnityEngine;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Events/Counters Changed Event")]
    public class CountersChangedEvent : BaseGameEvent<(int undo, int add, int hint)>
    {
    }
}