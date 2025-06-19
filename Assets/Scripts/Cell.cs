using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public int number;
    public int line;
    public int column;
    public TextMeshProUGUI text;
    public GameObject indicator;
    public RectTransform targetRectTransform;
    public Image backgroundImage;
    public Sprite activeSprite;
    public Sprite disabledSprite;
    private bool _selected;
    public bool IsActive { get; private set; }

    private void Awake()
    {
        if (!backgroundImage)
        {
            backgroundImage = GetComponent<Image>();
        }

        IsActive = true;
    }

    public void OnSelectingCell()
    {
        if (!IsActive) return;
        _selected = true;
        indicator.SetActive(_selected);
        ActionBus.SelectingCell(this);
    }

    public void OnDeselectingCell()
    {
        _selected = false;
        indicator.SetActive(_selected);
    }

    /// <summary>
    /// "Отключает" ячейку визуально, не деактивируя сам GameObject.
    /// Вызывается при успешном совпадении пары.
    /// </summary>
    public void DisableCell()
    {
        IsActive = false;
        SetDisabledSprite();
        if (_selected)
        {
            OnDeselectingCell();
        }
    }

    /// <summary>
    /// "Включает" ячейку, когда она достается из пула объектов.
    /// Возвращает ей первоначальный вид.
    /// </summary>
    public void EnableCell()
    {
        gameObject.SetActive(true);
        IsActive = true;
        SetActiveSprite();

        OnDeselectingCell();
    }

    public void SetActiveSprite()
    {
        text.enabled = true;
        if (backgroundImage && activeSprite)
        {
            backgroundImage.sprite = activeSprite;
        }
    }

    public void SetDisabledSprite()
    {
        text.enabled = false;
        if (backgroundImage && disabledSprite)
        {
            backgroundImage.sprite = disabledSprite;
        }
    }
}