using System;
using UnityEngine;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Events/Pair Score Event")]
    public class PairScoreEvent : BaseGameEvent<(Guid cell1, Guid cell2, int score)>
    {
    }
}