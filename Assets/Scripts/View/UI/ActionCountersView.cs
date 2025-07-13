using Core.Events;
using TMPro;
using UnityEngine;

namespace View.UI
{
    /// <summary>
    /// Отображает количество доступных действий игрока (отмена, добавление, подсказка) в UI.
    /// </summary>
    public class ActionCountersView : MonoBehaviour
    {
        [Header("UI Dependencies")]
        [SerializeField] private TextMeshProUGUI undoCountText;
        [SerializeField] private TextMeshProUGUI addNumbersCountText;
        [SerializeField] private TextMeshProUGUI hintCountText;

        [Header("Event Listening")]
        [SerializeField] private GameEvents gameEvents;

        private void OnEnable()
        {
            if (gameEvents)
            {
                gameEvents.onCountersChanged.AddListener(UpdateCountersUI);
            }
        }

        private void OnDisable()
        {
            if (gameEvents != null)
            {
                gameEvents.onCountersChanged.RemoveListener(UpdateCountersUI);
            }
        }

        /// <summary>
        /// Обновляет текстовые поля с количеством действий.
        /// </summary>
        /// <param name="data">Кортеж с количеством отмен, добавлений и подсказок.</param>
        private void UpdateCountersUI((int undo, int add, int hint) data)
        {
            if (data.undo == -1) // -1 означает бесконечность
            {
                if (undoCountText) undoCountText.text = "∞";
                if (addNumbersCountText) addNumbersCountText.text = "∞";
                if (hintCountText) hintCountText.text = "∞";
            }
            else
            {
                if (undoCountText) undoCountText.text = data.undo.ToString();
                if (addNumbersCountText) addNumbersCountText.text = data.add.ToString();
                if (hintCountText) hintCountText.text = data.hint.ToString();
            }
        }
    }
}