using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation : MonoBehaviour
{
    Animator playerAnimator;

    // Start is called before the first frame update
    void Start()
    {
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        playerAnimator.SetFloat("xAxis", Input.GetAxisRaw("Horizontal"));//xAxis = 0quand on bouge pas, =-1 quand on vas à gauche et =1 à droite
        playerAnimator.SetFloat("zAxis", Input.GetAxisRaw("Vertical"));//xAxis = 0quand on bouge pas, =-1 quand on recule et =1 à avance

        //si il court
        playerAnimator.SetBool("isRunning", Input.GetAxisRaw("Vertical") > 0 && Input.GetKey(KeyCode.LeftShift));
    }
}