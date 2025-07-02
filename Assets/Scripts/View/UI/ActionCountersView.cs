using Core.Events;
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class ActionCountersView : MonoBehaviour
    {
        [Header("UI Dependencies")]
        [SerializeField] private TextMeshProUGUI undoCountText;
        [SerializeField] private TextMeshProUGUI addNumbersCountText;
        [SerializeField] private TextMeshProUGUI hintCountText;

        [Header("Event Listening")]
        [SerializeField] private CountersChangedEvent onCountersChanged;

        private void OnEnable()
        {
            if (onCountersChanged)
            {
                onCountersChanged.AddListener(UpdateCountersUI);
            }
        }

        private void Start()
        {
            UpdateCountersUI((5, 5, 5));
        }

        private void OnDisable()
        {
            if (onCountersChanged)
            {
                onCountersChanged.RemoveListener(UpdateCountersUI);
            }
        }

        private void UpdateCountersUI((int undo, int add, int hint) data)
        {
            if (data.undo == -1)
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