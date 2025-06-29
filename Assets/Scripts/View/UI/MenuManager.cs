using Core;
using UnityEngine;

namespace View.UI
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject windowMenu;

        private void Awake()
        {
            GameEvents.OnShowMenu += ShowMenu;
            GameEvents.OnHideMenu += HideMenu;

            if (windowMenu)
            {
                windowMenu.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            GameEvents.OnShowMenu -= ShowMenu;
            GameEvents.OnHideMenu -= HideMenu;
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