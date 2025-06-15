using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Этот скрипт управляет переключением между двумя Canvas с помощью свайпа (прокрутки) в сторону.
/// Он плавно анимирует перемещение Canvas.
/// </summary>
public class CanvasSwiper : MonoBehaviour
{
    public RectTransform canvas1;
    public RectTransform canvas2;

    private readonly float animationSpeed = 10f;

    private readonly float swipeThreshold = 100f;

    private Vector2 _touchStartPos;
    private bool _isSwiping = false;
    private int _currentCanvasIndex = 1;

    private Vector2 _canvas1TargetPosition;
    private Vector2 _canvas2TargetPosition;

    private float _canvasWidth;

    private void Start()
    {
        if (canvas1 == null || canvas2 == null)
        {
            Debug.LogError("Ошибка: Один или оба Canvas не назначены в инспекторе! Пожалуйста, перетащите ваши Canvas в соответствующие поля компонента CanvasSwiper.");
            enabled = false;
            return;
        }

        _canvasWidth = canvas1.rect.width;
        canvas1.anchoredPosition = Vector2.zero;
        canvas2.anchoredPosition = new Vector2(_canvasWidth, 0);
        _canvas1TargetPosition = canvas1.anchoredPosition;
        _canvas2TargetPosition = canvas2.anchoredPosition;
    }

    private void Update()
    {
        if (!canvas1 || !canvas2) return;
        HandleInput();
        AnimateCanvasPositions();
    }

    /// <summary>
    /// Обрабатывает ввод пользователя (клик мыши или касание экрана).
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            _isSwiping = true;
            _touchStartPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && _isSwiping)
        {
            _isSwiping = false;
            Vector2 touchEndPos = Input.mousePosition;
            var swipeDistance = touchEndPos.x - _touchStartPos.x;

            if (Mathf.Abs(swipeDistance) > swipeThreshold)
            {
                if (swipeDistance < 0 && _currentCanvasIndex == 1)
                {
                    SwitchToCanvas2();
                }
                else if (swipeDistance > 0 && _currentCanvasIndex == 2)
                {
                    SwitchToCanvas1();
                }
            }
        }
    }

    /// <summary>
    /// Запускает переход к первому Canvas.
    /// </summary>
    private void SwitchToCanvas1()
    {
        if (_currentCanvasIndex == 2)
        {
            _currentCanvasIndex = 1;
            _canvas1TargetPosition = Vector2.zero;
            _canvas2TargetPosition = new Vector2(_canvasWidth, 0);
        }
    }

    /// <summary>
    /// Запускает переход ко второму Canvas.
    /// </summary>
    private void SwitchToCanvas2()
    {
        if (_currentCanvasIndex == 1)
        {
            _currentCanvasIndex = 2;
            _canvas1TargetPosition = new Vector2(-_canvasWidth, 0);
            _canvas2TargetPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Плавно анимирует позиции Canvas к их целевым значениям.
    /// </summary>
    private void AnimateCanvasPositions()
    {
        canvas1.anchoredPosition = Vector2.Lerp(canvas1.anchoredPosition, _canvas1TargetPosition, Time.deltaTime * animationSpeed);
        canvas2.anchoredPosition = Vector2.Lerp(canvas2.anchoredPosition, _canvas2TargetPosition, Time.deltaTime * animationSpeed);
    }
}