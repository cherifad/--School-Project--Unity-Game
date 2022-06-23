using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour {

	[SerializeField]private float Sensitivity;

	public bool isRotating;
	

	void Update () {
		if (Input.GetButton ("Fire1")) {
			isRotating = true;
			//`	HideMouse ();
			RotateOBJ ();
		} else {

			isRotating = false;
		}

		ReturnRotateOBJ ();
        HideMouse();    
	}

	public void RotateOBJ(){
		float mouseY = Input.GetAxis ("Mouse Y") * Sensitivity * Time.deltaTime;
		float mouseX = Input.GetAxis ("Mouse X") * Sensitivity * Time.deltaTime;

		transform.rotation = new Quaternion ( mouseY, mouseX, 0, 1) * transform.rotation;

	}

	public void ReturnRotateOBJ(){
		
		if (!isRotating) {
			transform.rotation = new Quaternion (0 * Time.deltaTime * 10, 0 * Time.deltaTime * 10, 0, 1)  ;
		}
	}
	public void HideMouse(){
        if (!isRotating)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false  ;
        }

	}

   
}
