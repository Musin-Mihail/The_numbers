using TMPro;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int number;
    public int line;
    public int column;
    public TextMeshProUGUI text;
    public GameObject indicator;
    private bool _selected;

    public void OnSelectingCell()
    {
        _selected = true;
        indicator.SetActive(_selected);
        ActionBus.SelectingCell(this);
    }

    public void OnDeselectingCell()
    {
        _selected = false;
        indicator.SetActive(_selected);
    }

    public void DisableCell()
    {
        gameObject.SetActive(false);
    }
}