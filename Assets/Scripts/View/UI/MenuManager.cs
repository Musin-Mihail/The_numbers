using UnityEngine;

namespace View.UI
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject windowMenu;

        private void Awake()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(true);
            }
        }

        public void ShowMenu()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(true);
            }
        }

        public void HideMenu()
        {
            if (windowMenu)
            {
                windowMenu.SetActive(false);
            }
        }

        public void ToggleMenu()
        {
            if (!windowMenu) return;

            var isMenuActive = windowMenu.activeSelf;
            if (isMenuActive)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
        }
    }
}