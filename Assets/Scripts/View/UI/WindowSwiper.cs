using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class WindowSwiper : MonoBehaviour
    {
        private enum WindowState
        {
            Menu,
            Game
        }

        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private RectTransform windowMenu;
        [SerializeField] private RectTransform windowGame;
        [SerializeField] private float animationSpeed = 10f;
        public float swipeThreshold = 100f;

        private WindowState _currentWindowIndex = WindowState.Menu;
        private Vector2 _windowMenuTargetPosition;
        private Vector2 _windowGameTargetPosition;
        private float _canvasWidth;

        private void Start()
        {
            if (!windowMenu || !windowGame || !canvasScaler)
            {
                Debug.LogError("Ошибка: CanvasScaler или один из Window не назначены в инспекторе!", this);
                enabled = false;
                return;
            }

            _canvasWidth = canvasScaler.referenceResolution.x;
            windowMenu.anchoredPosition = Vector2.zero;
            windowGame.anchoredPosition = new Vector2(_canvasWidth, 0);
            _windowMenuTargetPosition = windowMenu.anchoredPosition;
            _windowGameTargetPosition = windowGame.anchoredPosition;
        }

        private void Update()
        {
            if (!windowMenu || !windowGame) return;
            AnimateCanvasPositions();
        }

        public void SwitchToWindowMenu()
        {
            if (_currentWindowIndex == WindowState.Menu) return;
            _currentWindowIndex = WindowState.Menu;
            _windowMenuTargetPosition = Vector2.zero;
            _windowGameTargetPosition = new Vector2(_canvasWidth, 0);
        }

        public void SwitchToWindowGame()
        {
            if (_currentWindowIndex == WindowState.Game) return;
            _currentWindowIndex = WindowState.Game;
            _windowMenuTargetPosition = new Vector2(-_canvasWidth, 0);
            _windowGameTargetPosition = Vector2.zero;
        }

        private void AnimateCanvasPositions()
        {
            windowMenu.anchoredPosition = Vector2.Lerp(windowMenu.anchoredPosition, _windowMenuTargetPosition, Time.deltaTime * animationSpeed);
            windowGame.anchoredPosition = Vector2.Lerp(windowGame.anchoredPosition, _windowGameTargetPosition, Time.deltaTime * animationSpeed);
        }
    }
}