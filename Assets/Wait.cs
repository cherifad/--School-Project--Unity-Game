using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Wait : MonoBehaviour
{
    void Start()
    {

        // Démarre la fonction Wait (coroutine)
        // Wait veut dire attendre
        StartCoroutine(Wait2());

        
    }

    private IEnumerator Wait2()
    {
        yield return new WaitForSeconds(8f);

        SceneManager.LoadScene("SampleScene");
    }

}
