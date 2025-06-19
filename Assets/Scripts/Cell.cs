using System;
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

    public event Action<Cell> OnCellClicked;

    private void Awake()
    {
        if (!backgroundImage)
        {
            backgroundImage = GetComponent<Image>();
        }

        IsActive = true;
    }

    public void HandleClick()
    {
        if (!IsActive) return;

        OnCellClicked?.Invoke(this);
    }

    public void SetSelected(bool isSelected)
    {
        _selected = isSelected;
        indicator.SetActive(_selected);
    }

    public void OnDeselectingCell()
    {
        SetSelected(false);
    }

    public void DisableCell()
    {
        IsActive = false;
        SetDisabledSprite();
        if (_selected)
        {
            OnDeselectingCell();
        }
    }

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