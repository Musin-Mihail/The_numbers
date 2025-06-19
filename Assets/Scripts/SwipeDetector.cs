using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipeDetector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private CanvasSwiper canvasSwiper;
    [SerializeField] private ScrollRect scrollRect;
    private Vector2 _dragStartPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStartPosition = eventData.position;
        scrollRect?.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        scrollRect?.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canvasSwiper)
        {
            scrollRect?.OnEndDrag(eventData);
            return;
        }

        var dragVector = eventData.position - _dragStartPosition;

        if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y) && Mathf.Abs(dragVector.x) > canvasSwiper.swipeThreshold)
        {
            if (dragVector.x < 0)
            {
                canvasSwiper.SwitchToCanvas2();
            }
            else
            {
                canvasSwiper.SwitchToCanvas1();
            }
        }
        else
        {
            scrollRect?.OnEndDrag(eventData);
        }
    }
}