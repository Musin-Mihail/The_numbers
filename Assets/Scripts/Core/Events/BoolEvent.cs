using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, передающее значение типа bool.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Bool Event")]
    public class BoolEvent : BaseGameEvent<bool>
    {
    }
}