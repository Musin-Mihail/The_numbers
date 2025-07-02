using Core.Events;
using UnityEngine;

namespace View.UI
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject windowMenu;

        [Header("Event Listening")]
        [SerializeField] private VoidEvent onShowMenu;
        [SerializeField] private VoidEvent onHideMenu;
        [SerializeField] private VoidEvent onNewGameStarted;

        private void OnEnable()
        {
            if (onShowMenu) onShowMenu.AddListener(ShowMenu);
            if (onHideMenu) onHideMenu.AddListener(HideMenu);
            if (onNewGameStarted) onNewGameStarted.AddListener(HideMenu);
        }

        private void Start()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (onShowMenu) onShowMenu.RemoveListener(ShowMenu);
            if (onHideMenu) onHideMenu.RemoveListener(HideMenu);
            if (onNewGameStarted) onNewGameStarted.RemoveListener(HideMenu);
        }

        private void ShowMenu()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(true);
            }
        }

        private void HideMenu()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(false);
            }
        }
    }
}