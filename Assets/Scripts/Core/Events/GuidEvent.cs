using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, передающее один идентификатор (Guid).
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Guid Event")]
    public class GuidEvent : BaseGameEvent<Guid>
    {
    }
}