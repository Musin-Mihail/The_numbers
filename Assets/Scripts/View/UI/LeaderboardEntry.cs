using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG.Utils.LB;

namespace View.UI
{
    /// <summary>
    /// Управляет отображением данных одной строки в таблице лидеров.
    /// </summary>
    public class LeaderboardEntry : MonoBehaviour
    {
        [Header("UI Элементы игрока")]
        [SerializeField] private GameObject playerInfoContainer;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private Image photoImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Стилизация текущего игрока")]
        [Tooltip("Множитель размера шрифта для строки текущего игрока.")]
        [SerializeField] private float fontSizeMultiplier = 1.2f;

        [Header("UI Элемент разделителя")]
        [SerializeField] private GameObject separatorContainer;

        private float _defaultRankFontSize;
        private float _defaultNameFontSize;
        private float _defaultScoreFontSize;

        private void Awake()
        {
            if (rankText) _defaultRankFontSize = rankText.fontSize;
            if (nameText) _defaultNameFontSize = nameText.fontSize;
            if (scoreText) _defaultScoreFontSize = scoreText.fontSize;
        }

        /// <summary>
        /// Заполняет UI элементы данными игрока и применяет стили.
        /// </summary>
        /// <param name="playerData">Данные игрока от Yandex Games.</param>
        /// <param name="isCurrentPlayer">True, если это запись текущего игрока.</param>
        public void Populate(LBPlayerData playerData, bool isCurrentPlayer)
        {
            playerInfoContainer.SetActive(true);
            separatorContainer.SetActive(false);

            rankText.text = playerData.rank.ToString();
            nameText.text = playerData.name;
            scoreText.text = playerData.score.ToString();

            if (photoImage && !string.IsNullOrEmpty(playerData.photo))
            {
                StartCoroutine(ImageDownloader.LoadImage(photoImage, playerData.photo));
            }

            if (isCurrentPlayer)
            {
                if (rankText) rankText.fontSize = _defaultRankFontSize * fontSizeMultiplier;
                if (nameText) nameText.fontSize = _defaultNameFontSize * fontSizeMultiplier;
                if (scoreText) scoreText.fontSize = _defaultScoreFontSize * fontSizeMultiplier;
            }
            else
            {
                if (rankText) rankText.fontSize = _defaultRankFontSize;
                if (nameText) nameText.fontSize = _defaultNameFontSize;
                if (scoreText) scoreText.fontSize = _defaultScoreFontSize;
            }
        }

        /// <summary>
        /// Превращает эту строку в разделитель "---".
        /// </summary>
        public void SetAsSeparator()
        {
            playerInfoContainer.SetActive(false);
            separatorContainer.SetActive(true);
        }
    }
}