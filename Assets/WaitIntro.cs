using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitIntro : MonoBehaviour
{
    GameObject player;
    void Start()
    {
        // Démarre la fonction Wait (coroutine)
        // Wait veut dire attendre
        StartCoroutine(Wait2());


        // Exécution en parallèle
        print("S'affiche avant que Wait soit finie : " + Time.time);
        Destroy(player);
    }

    private IEnumerator Wait2()
    {

        Scene scene = SceneManager.GetActiveScene();

        if (scene.name == "Intro")
        {
            yield return new WaitForSeconds(24f);
            SceneManager.LoadScene("SampleScene");
        }

    }
}
