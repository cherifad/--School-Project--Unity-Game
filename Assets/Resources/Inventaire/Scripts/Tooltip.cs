using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    private Text tooltip;
    void Start()
    {
        tooltip = GetComponentInChildren<Text>();
        tooltip.gameObject.SetActive(false);
    }
    public void GenerateTooltip(Item item)
    {
        string text = string.Format("<b>{0}</b>\n{1}", item.title,item.description);
        tooltip.text = text;
        Debug.Log($"{tooltip.gameObject}");
        tooltip.gameObject.SetActive(true);
    }
}
    