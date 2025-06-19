using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public Guid DataId { get; private set; }
    public TextMeshProUGUI text;
    public GameObject indicator;
    public RectTransform targetRectTransform;
    public Image backgroundImage;
    public Sprite activeSprite;
    public Sprite disabledSprite;
    public CellAnimator Animator { get; private set; }
    public event Action<Cell> OnCellClicked;
    private bool _selected;

    private void Awake()
    {
        if (!backgroundImage) backgroundImage = GetComponent<Image>();
        Animator = GetComponent<CellAnimator>();
        if (!Animator) Debug.LogError("Компонент CellAnimator не найден!", this);
    }

    public void UpdateFromData(CellData data)
    {
        DataId = data.Id;
        text.text = data.Number.ToString();
        line = data.Line;
        column = data.Column;

        if (IsActive != data.IsActive)
        {
            IsActive = data.IsActive;
            if (IsActive)
            {
                SetActiveSprite();
            }
            else
            {
                SetDisabledSprite();
            }
        }
    }

    public int line;
    public int column;
    public int Number => int.Parse(text.text);
    public bool IsActive { get; private set; }

    public void HandleClick()
    {
        if (!IsActive) return;
        OnCellClicked?.Invoke(this);
    }

    public void SetSelected(bool isSelected)
    {
        if (_selected == isSelected) return;
        _selected = isSelected;
        indicator.SetActive(_selected);
    }

    public void SetVisualState(bool isActive)
    {
        if (isActive)
        {
            SetActiveSprite();
        }
        else
        {
            SetDisabledSprite();
        }
    }

    private void SetActiveSprite()
    {
        text.enabled = true;
        if (backgroundImage && activeSprite)
        {
            backgroundImage.sprite = activeSprite;
        }
    }

    private void SetDisabledSprite()
    {
        text.enabled = false;
        if (backgroundImage && disabledSprite)
        {
            backgroundImage.sprite = disabledSprite;
        }
    }
}