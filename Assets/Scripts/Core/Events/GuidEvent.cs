using System;
using UnityEngine;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Events/Guid Event")]
    public class GuidEvent : BaseGameEvent<Guid>
    {
    }
}