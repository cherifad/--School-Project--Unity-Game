using CI.QuickSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestoreSave : MonoBehaviour
{
    public GameObject player;
    private GameObject key;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Application.persistentDataPath);
        player = GameObject.FindGameObjectWithTag("Player");
        key = GameObject.FindGameObjectWithTag("key");

        var reader = QuickSaveReader.Create("Player");

        float vie = 0f;
        List<int> items = new List<int>();
        Vector3 position = Vector3.zero;
        bool keyDestruct = false;
        
        if(PersoClé.Save)
        {
            if (reader != null)
            {
                if (reader.Exists("Vie") || reader.Exists("Items") || reader.Exists("Positiion") || reader.Exists("Key"))
                {
                    reader.Read<float>("Vie", r => vie = r)
                      .Read<List<int>>("Items", r => items = r)
                      .Read<Vector3>("Positiion", r => position = r)
                      .Read<bool>("Key", r => keyDestruct = r);
                }

                player.transform.position = position;

                items.Add(0);

                Debug.Log($"Longueur = {items.Count}, 1er element : {items[0]}");

                Inventory.IdItems = items;

                HealthBar.SetHealthBarValue(vie);

                if (keyDestruct)
                    Destroy(key);

                PersoClé.Save = false;

                Inventory.Trigger = true;
            }
        }
        
                
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
