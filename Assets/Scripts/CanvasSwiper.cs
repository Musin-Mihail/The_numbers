using UnityEngine;

public class CanvasSwiper : MonoBehaviour
{
    public RectTransform canvas1;
    public RectTransform canvas2;
    public float animationSpeed = 10f;
    public float swipeThreshold = 100f;

    private int _currentCanvasIndex = 1;
    private Vector2 _canvas1TargetPosition;
    private Vector2 _canvas2TargetPosition;
    private float _screenWidth = 0;

    private void Start()
    {
        if (!canvas1 || !canvas2)
        {
            Debug.LogError("Ошибка: Один или оба Canvas не назначены в инспекторе!", this);
            enabled = false;
            return;
        }

        _screenWidth = Screen.width;
        canvas1.anchoredPosition = Vector2.zero;
        canvas2.anchoredPosition = new Vector2(_screenWidth, 0);
        _canvas1TargetPosition = canvas1.anchoredPosition;
        _canvas2TargetPosition = canvas2.anchoredPosition;
    }

    private void Update()
    {
        if (!canvas1 || !canvas2) return;
        AnimateCanvasPositions();
    }

    public void SwitchToCanvas1()
    {
        if (_currentCanvasIndex == 1) return;
        _currentCanvasIndex = 1;
        _canvas1TargetPosition = Vector2.zero;
        _canvas2TargetPosition = new Vector2(_screenWidth, 0);
    }

    public void SwitchToCanvas2()
    {
        if (_currentCanvasIndex == 2) return;
        _currentCanvasIndex = 2;
        _canvas1TargetPosition = new Vector2(-_screenWidth, 0);
        _canvas2TargetPosition = Vector2.zero;
    }

    private void AnimateCanvasPositions()
    {
        canvas1.anchoredPosition = Vector2.Lerp(canvas1.anchoredPosition, _canvas1TargetPosition, Time.deltaTime * animationSpeed);
        canvas2.anchoredPosition = Vector2.Lerp(canvas2.anchoredPosition, _canvas2TargetPosition, Time.deltaTime * animationSpeed);
    }
}