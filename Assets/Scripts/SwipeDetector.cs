using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Этот компонент отслеживает свайпы и передает события прокрутки компоненту ScrollRect.
/// Он должен быть размещен на UI-элементе (например, невидимой панели),
/// который находится на Canvas и покрывает область, где возможен свайп.
/// </summary>
public class SwipeDetector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CanvasSwiper canvasSwiper;
    public ScrollRect scrollRect;
    private Vector2 _dragStartPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStartPosition = eventData.position;
        if (scrollRect)
        {
            scrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (scrollRect)
        {
            scrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var dragEndPosition = eventData.position;
        var dragVector = dragEndPosition - _dragStartPosition;
        if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y))
        {
            if (Mathf.Abs(dragVector.x) > canvasSwiper.swipeThreshold)
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
        }

        if (scrollRect)
        {
            scrollRect.OnEndDrag(eventData);
        }
    }
}