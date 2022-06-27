using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDataBase : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    private void Awake()
    {
        BuildDatabase();
    }
    public Item GetItem(int id)
    {
        return items.Find(item => item.id == id);
    }
    public Item GetItem(string itemName)
    {
        return items.Find(item => item.title == itemName);
    }
    void BuildDatabase()
    {
        items = new List<Item>()
        {
            new Item(0,"Clef","La cle qui permet d'ouvrir une certaine porte"),
            new Item(1,"Pomme","Ce fruit permet de regagner de la vie"),
        };
    }
}
