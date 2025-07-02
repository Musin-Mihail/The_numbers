using System;
using UnityEngine;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Events/Cell Pair Event")]
    public class CellPairEvent : BaseGameEvent<(Guid, Guid)>
    {
    }
}