using Core.Events;
using UnityEngine;

namespace View.UI
{
    public class StatisticsWindowManager : MonoBehaviour
    {
        [SerializeField] private GameObject statisticsWindow;

        [Header("Event Listening")]
        [SerializeField] private VoidEvent onShowStatistics;
        [SerializeField] private VoidEvent onHideStatistics;

        private void OnEnable()
        {
            if (onShowStatistics) onShowStatistics.AddListener(ShowStatisticsWindow);
            if (onHideStatistics) onHideStatistics.AddListener(HideStatisticsWindow);
        }

        private void Start()
        {
            if (statisticsWindow)
            {
                statisticsWindow.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (onShowStatistics) onShowStatistics.RemoveListener(ShowStatisticsWindow);
            if (onHideStatistics) onHideStatistics.RemoveListener(HideStatisticsWindow);
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