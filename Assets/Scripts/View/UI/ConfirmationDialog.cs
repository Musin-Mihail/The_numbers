using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    /// <summary>
    /// Управляет модальным окном для подтверждения действий пользователя.
    /// </summary>
    public class ConfirmationDialog : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private TextMeshProUGUI yesButtonText;
        [SerializeField] private TextMeshProUGUI noButtonText;
        [SerializeField] private RectTransform panel;

        private Action _onYesAction;
        private Action _onNoAction;

        private void Awake()
        {
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(OnNoClicked);
        }

        /// <summary>
        /// Показывает диалоговое окно с заданным сообщением и действиями.
        /// </summary>
        /// <param name="message">Сообщение для пользователя.</param>
        /// <param name="yesText">Текст для кнопки подтверждения.</param>
        /// <param name="noText">Текст для кнопки отмены.</param>
        /// <param name="onYes">Действие, выполняемое при нажатии "Да".</param>
        /// <param name="onNo">Действие, выполняемое при нажатии "Нет".</param>
        /// <param name="newSize">Новый размер панели диалога.</param>
        public void Show(string message, string yesText, string noText, Action onYes, Action onNo, Vector2 newSize)
        {
            panel.sizeDelta = newSize;
            messageText.text = message;

            if (yesButtonText) yesButtonText.text = yesText;
            if (noButtonText) noButtonText.text = noText;

            _onYesAction = onYes;
            _onNoAction = onNo;

            yesButton.gameObject.SetActive(!string.IsNullOrEmpty(yesText));
            noButton.gameObject.SetActive(!string.IsNullOrEmpty(noText));

            gameObject.SetActive(true);
        }

        private void OnYesClicked()
        {
            _onYesAction?.Invoke();
            gameObject.SetActive(false);
        }

        private void OnNoClicked()
        {
            _onNoAction?.Invoke();
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            yesButton.onClick.RemoveListener(OnYesClicked);
            noButton.onClick.RemoveListener(OnNoClicked);
        }
    }
}