using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Игровое событие, передающее информацию о начислении очков за пару.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Pair Score Event")]
    public class PairScoreEvent : BaseGameEvent<(Guid cell1, Guid cell2, int score)>
    {
    }
}