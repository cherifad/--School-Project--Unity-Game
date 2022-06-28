using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Wait : MonoBehaviour
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
        var temps = 9f;
        Scene scene = SceneManager.GetActiveScene();

        if (scene.name == "IntroIntro")
        {
            yield return new WaitForSeconds(11.5f);
            SceneManager.LoadScene("intro");
        }
        if (scene.name == "Final")
        {
            yield return new WaitForSeconds(30f);
            SceneManager.LoadScene("FinJeu");
        }
        else
        {
            if (scene.name == "LavaReveal")
                temps = 7.5f;
            if (scene.name == "KeyGrabe")
                temps = 9f;
            if (scene.name == "intro")
            { 
                temps = 24f;
                Debug.Log("yo");
            }


            yield return new WaitForSeconds(temps);

            SceneManager.LoadScene("SampleScene");
        }
    }
}
