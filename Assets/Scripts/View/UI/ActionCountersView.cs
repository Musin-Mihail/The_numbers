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
        [SerializeField] private GameObject undoCount;
        [SerializeField] private GameObject hintCount;
        [SerializeField] private TextMeshProUGUI undoCountText;
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
            if (gameEvents)
            {
                gameEvents.onCountersChanged.RemoveListener(UpdateCountersUI);
            }
        }

        /// <summary>
        /// Обновляет текстовые поля с количеством действий.
        /// Если счетчик бесконечен (-1), соответствующий GameObject отключается.
        /// Если счетчик равен 0, отображается "+".
        /// </summary>
        /// <param name="data">Кортеж с количеством отмен и подсказок.</param>
        private void UpdateCountersUI((int undo, int hint) data)
        {
            var areCountersInfinite = data.undo == -1;

            if (areCountersInfinite)
            {
                if (undoCount) undoCount.SetActive(false);
                if (hintCount) hintCount.SetActive(false);
            }
            else
            {
                if (undoCountText)
                {
                    undoCountText.gameObject.SetActive(true);
                    undoCountText.text = data.undo == 0 ? "+" : data.undo.ToString();
                }

                if (hintCountText)
                {
                    hintCountText.gameObject.SetActive(true);
                    hintCountText.text = data.hint == 0 ? "+" : data.hint.ToString();
                }
            }
        }
    }
}