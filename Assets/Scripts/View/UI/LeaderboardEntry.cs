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

        [Header("UI Элемент разделителя")]
        [SerializeField] private GameObject separatorContainer;

        /// <summary>
        /// Заполняет UI элементы данными игрока.
        /// </summary>
        /// <param name="playerData">Данные игрока от Yandex Games.</param>
        public void Populate(LBPlayerData playerData)
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