using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text textBox;
    public Color showColor = Color.blue;

    public void SetText(string t)
    {
        textBox.text = t;
    }

    void Start()
    {
        textBox.color = new Color(0, 0, 0, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textBox.color = showColor; //Or however you do your color
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textBox.color = new Color(0, 0, 0, 0);
    }
}