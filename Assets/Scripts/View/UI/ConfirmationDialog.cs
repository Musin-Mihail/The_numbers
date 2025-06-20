using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class ConfirmationDialog : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private Action _onYesAction;

        private void Awake()
        {
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(OnNoClicked);
        }

        public void Show(string message, Action onYes)
        {
            messageText.text = message;
            _onYesAction = onYes;
            gameObject.SetActive(true);
        }

        private void OnYesClicked()
        {
            _onYesAction?.Invoke();
            gameObject.SetActive(false);
        }

        private void OnNoClicked()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            yesButton.onClick.RemoveListener(OnYesClicked);
            noButton.onClick.RemoveListener(OnNoClicked);
        }
    }
}