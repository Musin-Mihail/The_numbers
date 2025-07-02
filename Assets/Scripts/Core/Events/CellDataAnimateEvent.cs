using Model;
using UnityEngine;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Events/Cell Data Animate Event")]
    public class CellDataAnimateEvent : BaseGameEvent<(CellData, bool)>
    {
    }
}