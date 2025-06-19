using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CellAnimator), typeof(Image))]
public class Cell : MonoBehaviour
{
    public TextMeshProUGUI text;
    [SerializeField] private GameObject indicator;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Sprite disabledSprite;
    public RectTransform targetRectTransform { get; private set; }
    public CellAnimator Animator { get; private set; }
    private Image _backgroundImage;
    public event Action<Cell> OnCellClicked;
    public Guid DataId { get; private set; }
    public int line { get; private set; }
    public int column { get; private set; }
    public bool IsActive { get; private set; }
    public int Number { get; private set; }
    private bool _selected;

    private void Awake()
    {
        _backgroundImage = GetComponent<Image>();
        Animator = GetComponent<CellAnimator>();
        targetRectTransform = GetComponent<RectTransform>();
    }

    public void UpdateFromData(CellData data)
    {
        DataId = data.Id;
        line = data.Line;
        column = data.Column;

        if (Number != data.Number)
        {
            Number = data.Number;
            text.text = Number.ToString();
        }

        if (IsActive != data.IsActive)
        {
            IsActive = data.IsActive;
            SetVisualState(IsActive);
        }
    }

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
        text.enabled = isActive;
        if (_backgroundImage)
        {
            _backgroundImage.sprite = isActive ? activeSprite : disabledSprite;
        }
    }
}