using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TooltipWindowManager : MonoBehaviour
{
    public TextMeshProUGUI tipText;
    public RectTransform tipWindow;
    public static Action<string, Vector2> OnMouseHover;
    public static Action OnMouseLoseFocus;
    private void OnEnable()
    {
        OnMouseHover += ShowTip;
        OnMouseLoseFocus += HideTip;
    }
    private void OnDisable()
    {

    }
    void Start()
    {
        HideTip();
    }
    private void ShowTip(string tip, Vector2 mousePos)
    {
        if(tipText == null)
        {
            return;
        }
        tipText.text = tip;
        tipWindow.sizeDelta = new Vector2(tipText.preferredWidth > 300 ? 300 : tipText.preferredWidth, tipText.preferredHeight);

        tipWindow.gameObject.SetActive(true);
        if(SceneManager.GetActiveScene().name == "WaveFunctionCollapse")
        {
            tipWindow.transform.position = Camera.main.ScreenToWorldPoint(new Vector2(mousePos.x - tipWindow.sizeDelta.x / 2, mousePos.y)).ZeroZ();
        }
        else
        {
            tipWindow.transform.position = new Vector2(mousePos.x - tipWindow.sizeDelta.x / 2, mousePos.y);
        }
    }

    private void HideTip()
    {
        if (tipText == null)
        {
            return;
        }
        tipText.text = default;
        tipWindow.gameObject.SetActive(false);
    }
}