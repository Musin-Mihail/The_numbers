using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, передающее пару идентификаторов ячеек (Guid).
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Cell Pair Event")]
    public class CellPairEvent : BaseGameEvent<(Guid, Guid)>
    {
    }
}