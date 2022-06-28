﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> characterItems = new List<Item>();
    public ItemDataBase itemDatabase;
    public UIInventory inventoryUI;

    private void Start()
    {
        inventoryUI.gameObject.SetActive(!inventoryUI.gameObject.activeSelf);
        GiveItem(1);
        GiveItem(1);
        GiveItem(1);
    }
    private void Update()
    {
        int bail = 1;
        if(Input.GetKeyDown(KeyCode.B))
        {
            inventoryUI.gameObject.SetActive(!inventoryUI.gameObject.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            if(Exist(bail))
            {
                RemoveItem(bail);
                HealthBar.SetHealthBarValue(HealthBar.GetHealthBarValue() + 0.01f);
            }            
        }
    }
    public void GiveItem(int id)
    {
        Item itemToAdd = itemDatabase.GetItem(id);
        characterItems.Add(itemToAdd);
        inventoryUI.AddNewItem(itemToAdd);
        Debug.Log("Added item: " + itemToAdd.title);
    }
    public void GiveItem(string itemName)
    {
        Item itemToAdd = itemDatabase.GetItem(itemName);
        characterItems.Add(itemToAdd);
        inventoryUI.AddNewItem(itemToAdd);
        Debug.Log("Added item: " + itemToAdd.title);
    }

    public bool Exist(int id)
    {
        return characterItems.Find(item => item.id == id) != null;
    }
    public Item CheckForItem(int id)
    {
        return characterItems.Find(item => item.id == id);
    }
    public void RemoveItem(int id)
    {
        Item itemToRemove = CheckForItem(id);
        if (itemToRemove != null)
        {
            characterItems.Remove(itemToRemove);
            inventoryUI.RemoveItem(itemToRemove);
            Debug.Log("Item removed: " + itemToRemove.title);
        }
    }
}
