using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CI.QuickSave;

public class PersoClé : MonoBehaviour
{
    public GameObject key;
    private GameObject player;
    private static bool save;

    public static bool Save { get => save; set => save = value; }

    private void Start()
    {
        key = GetComponent<GameObject>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("key"))
        {
            Destroy(key);

            List<int> items = new List<int>();

            items = Inventory.IdItems;

            var writer = QuickSaveWriter.Create("Player");

            writer.Write("Vie", HealthBar.GetHealthBarValue())
                .Write("Items", items)
                .Write("Positiion", this.transform.position)
                .Write("Key", true)
                .Commit();

            Save = true;

            SceneManager.LoadScene("KeyGrabe");
        }

        if (other.gameObject.tag.Equals("triggerlava"))
        {
            SceneManager.LoadScene("LavaReveal");
        }

        if (other.gameObject.tag.Equals("final"))
        {
            SceneManager.LoadScene("Final");
        }

    }
    
}
