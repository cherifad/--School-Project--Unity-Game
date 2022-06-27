using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Item
{
    public int id;
    public string title;
    public string description;
    public Sprite icon;
    public Item(int id, string title, string description)
    {
        this.id = id;
        this.icon = Resources.Load<Sprite>("Inventaire/Sprites/Items/" + title);
        this.description = description;
        this.title = title;
    }
    public Item(Item item)
    {
        this.id = item.id;
        this.icon = Resources.Load<Sprite>("Inventaire/Sprites/Items/" + item.title);
        this.description = item.description;
        this.title = item.title;
    }
}
