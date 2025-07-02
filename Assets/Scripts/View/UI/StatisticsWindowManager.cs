using Core;
using UnityEngine;

namespace View.UI
{
    public class StatisticsWindowManager : MonoBehaviour
    {
        [SerializeField] private GameObject statisticsWindow;

        private void Awake()
        {
            GameEvents.OnShowStatistics += ShowStatisticsWindow;
            GameEvents.OnHideStatistics += HideStatisticsWindow;

            if (statisticsWindow)
            {
                statisticsWindow.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnShowStatistics -= ShowStatisticsWindow;
            GameEvents.OnHideStatistics -= HideStatisticsWindow;
        }

        private void ShowStatisticsWindow()
        {
            if (statisticsWindow)
            {
                statisticsWindow.SetActive(true);
            }
        }

        private void HideStatisticsWindow()
        {
            if (statisticsWindow)
            {
                statisticsWindow.SetActive(false);
            }
        }
    }
}