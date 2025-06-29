using Core;
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class ActionCountersView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI undoCountText;
        [SerializeField] private TextMeshProUGUI addNumbersCountText;
        [SerializeField] private TextMeshProUGUI hintCountText;

        private void Start()
        {
            GameEvents.OnCountersChanged += UpdateCountersUI;
            GameEvents.RaiseCountersChanged(5, 5, 5);
        }

        private void OnDestroy()
        {
            GameEvents.OnCountersChanged -= UpdateCountersUI;
        }

        private void UpdateCountersUI(int undo, int add, int hint)
        {
            if (undo == -1)
            {
                if (undoCountText) undoCountText.text = "∞";
                if (addNumbersCountText) addNumbersCountText.text = "∞";
                if (hintCountText) hintCountText.text = "∞";
            }
            else
            {
                if (undoCountText) undoCountText.text = undo.ToString();
                if (addNumbersCountText) addNumbersCountText.text = add.ToString();
                if (hintCountText) hintCountText.text = hint.ToString();
            }
        }
    }
}