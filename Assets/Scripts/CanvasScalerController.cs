using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Этот скрипт автоматически настраивает Canvas Scaler в зависимости от ориентации экрана.
/// Повесьте этот скрипт на тот же GameObject, где находится ваш Canvas.
/// </summary>
[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerController : MonoBehaviour
{
    private CanvasScaler _canvasScaler;

    private void Start()
    {
        _canvasScaler = GetComponent<CanvasScaler>();
        _canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
    }
}