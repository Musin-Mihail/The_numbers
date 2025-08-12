using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, передающее значение типа string.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/String Event")]
    public class StringEvent : BaseGameEvent<string>
    {
    }
}