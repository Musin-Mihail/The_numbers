using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerController : MonoBehaviour
{
    private void Start()
    {
        var canvasScaler = GetComponent<CanvasScaler>();
        canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
    }
}