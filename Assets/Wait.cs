using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Wait : MonoBehaviour
{
    GameObject player;
    Inventory inventory;
    void Start()
    {

        // Démarre la fonction Wait (coroutine)
        // Wait veut dire attendre
        StartCoroutine(Wait2());
        player = GameObject.FindGameObjectWithTag("Player");


        // Exécution en parallèle
        print("S'affiche avant que Wait soit finie : " + Time.time);
        Destroy(player);
    }

    private IEnumerator Wait2()
    {
        var temps = 9f;
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "LavaReveal")
            temps = 7.5f;
        if (scene.name == "KeyGrabe")
            temps = 9f;

        yield return new WaitForSeconds(temps);

        Vector3 pos = Save.PlayerPos;

        Debug.Log($"{pos}");

        SceneManager.LoadScene("SampleScene");

        Debug.Log($"{player.transform.position}");

        player.transform.position = pos;

        inventory.GiveItem(0);


        Debug.Log($"{player.transform.position}");
    }
}
