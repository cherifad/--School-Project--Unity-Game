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
        string tooltip = string.Format("<b>{0}</b>\n<b>{1}</b>", item.title,item.description);
        gameObject.SetActive(true);
    }
}
    