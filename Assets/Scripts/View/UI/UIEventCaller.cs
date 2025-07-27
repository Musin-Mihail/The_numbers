using Core.Events;
using UnityEngine;

namespace View.UI
{
    /// <summary>
    /// Компонент-посредник, который вызывается из UnityEvents (например, с кнопок в инспекторе)
    /// и транслирует эти вызовы в систему игровых событий (ScriptableObject Events).
    /// </summary>
    public class UIEventCaller : MonoBehaviour
    {
        [Header("Вызов событий")]
        [SerializeField] private GameEvents gameEvents;

        public void RaiseRequestNewGame() => gameEvents?.onRequestNewGame.Raise();
        public void RaiseUndoLastAction() => gameEvents?.onUndoLastAction.Raise();
        public void RaiseAddExistingNumbers() => gameEvents?.onAddExistingNumbers.Raise();
        public void RaiseShowMenu() => gameEvents?.onShowMenu.Raise();
        public void RaiseHideMenu() => gameEvents?.onHideMenu.Raise();
        public void RaiseRequestHint() => gameEvents?.onRequestHint.Raise();
        public void RaiseRequestDisableCounters() => gameEvents?.onRequestDisableCounters.Raise();
        public void RaiseShowStatistics() => gameEvents?.onShowStatistics.Raise();
        public void RaiseHideStatistics() => gameEvents?.onHideStatistics.Raise();
        public void RaiseShowRules() => gameEvents?.onShowRules.Raise();
        public void RaiseHideRules() => gameEvents?.onHideRules.Raise();

        /// <summary>
        /// Вызывает событие для скрытия окна настроек.
        /// </summary>
        public void RaiseHideOptions() => gameEvents?.onHideOptions.Raise();

        /// <summary>
        /// Вызывает событие для полного сброса игры с очисткой статистики.
        /// </summary>
        public void RaiseRequestHardReset() => gameEvents?.onRequestHardReset.Raise();

        /// <summary>
        /// Показывает окно настроек и одновременно отмечает обновление как просмотренное.
        /// Используйте этот метод для кнопки, которая открывает окно с информацией об обновлении.
        /// </summary>
        public void RaiseShowOptionsAndMarkUpdateSeen()
        {
            gameEvents?.onShowOptions.Raise();
            gameEvents?.onRequestMarkUpdateSeen.Raise();
        }
    }
}