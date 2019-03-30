using UnityEngine;
using System.Collections;

/*
 * NOT AI RELATED
 * 
 * This script is representing the cursor.
 * For its actual control, see "MainCameraScript".
 */

public class CursorScript : MonoBehaviour {

    GameObject mainCam;

	// Use this for initialization
	void Start () {
        mainCam = GameObject.Find("MainCamera");
	}
	
	// Update is called once per frame
	void Update () {
         Vector3 mouseViewVector = mainCam.transform.TransformDirection(new Vector3(0, 0, 1));
         this.transform.position = mainCam.transform.position + 0.5f * mouseViewVector;
	}
}
