using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Этот компонент отслеживает свайпы с помощью интерфейсов событий UI.
/// Он должен быть размещен на UI-элементе (например, невидимой панели), 
/// который находится на Canvas.
/// </summary>
public class SwipeDetector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CanvasSwiper canvasSwiper;
    private Vector2 _dragStartPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStartPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var dragEndPosition = eventData.position;
        var swipeDistance = dragEndPosition.x - _dragStartPosition.x;

        if (!(Mathf.Abs(swipeDistance) > canvasSwiper.swipeThreshold)) return;

        if (swipeDistance < 0)
        {
            canvasSwiper.SwitchToCanvas2();
        }
        else
        {
            canvasSwiper.SwitchToCanvas1();
        }
    }
}