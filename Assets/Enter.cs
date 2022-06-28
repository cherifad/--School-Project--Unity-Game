using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enter : MonoBehaviour
{
    public GameObject player;
    private GameObject textcle;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<GameObject>();
        textcle = GameObject.FindGameObjectWithTag("Text-clé");
        textcle.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(Inventory.IdItems.Find(x => x == 0) != null)
        {
            return;
        } else
        {
            textcle.SetActive(true);
        }
    }
        // Update is called once per frame
        void Update()
    {
        
    }
}
