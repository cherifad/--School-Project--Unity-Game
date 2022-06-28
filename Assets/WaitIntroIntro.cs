using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaitIntroIntro : MonoBehaviour
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

        if (scene.name == "IntroIntro")
        {
            yield return new WaitForSeconds(11.5f);
            SceneManager.LoadScene("intro");
        }
        
    }
}
