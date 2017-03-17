using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject textBoxObj;
    private Text textB;
    public Color showColor = Color.blue;

    public void SetText(string t)
    {
        if(textB == null)
        {
            textB = textBoxObj.GetComponentInChildren<Text>();
        }
        textB.text = t;
    }

    void Start()
    {
        textBoxObj.SetActive(false);
        textB = textBoxObj.GetComponentInChildren<Text>();
        textB.color = new Color(0, 0, 0, 0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textB.color = showColor; //Or however you do your color
        textBoxObj.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textB.color = new Color(0, 0, 0, 0);
        textBoxObj.SetActive(false);
    }
}