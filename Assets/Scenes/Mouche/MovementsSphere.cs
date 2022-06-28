using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementsSphere : MonoBehaviour
{
    public float speed = 6f;

    public float jumpspeed = 8f;

    public float gravity = 20f;

    private Vector3 moveD = Vector3.zero;
    CharacterController Cac;
    //Animation animations;
    Animator PlayerAnimator;

    void Start()
    {
        Cac = GetComponent<CharacterController>();
        //animations = gameObject.GetComponent<Animation>();
        PlayerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Cac.isGrounded)
        {
            moveD = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveD = transform.TransformDirection(moveD);
            moveD *= speed;
            /*Debug.Log(moveD);*/

            if (Input.GetButton("Jump"))
            {
                moveD.y = jumpspeed;
            }
            /*if (Input.GetKey(KeyCode.K))
            {
                transform.Translate(0, 0, 100 * Time.deltaTime);
                PlayerAnimator.SetBool("isWalking", Input.GetKey(KeyCode.Z));
            }*/

        }

        moveD.y -= gravity * Time.deltaTime;
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * speed * 100);

        Cac.Move(moveD * Time.deltaTime);
    }
}

