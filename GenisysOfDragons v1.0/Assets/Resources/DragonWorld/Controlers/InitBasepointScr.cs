using UnityEngine;
using System.Collections;

/*
 * NOT AI RELATED
 * 
 * Makes the basepoints invisible in game.
 * The "basepoint" (a GameObject) determines the original orientation and position in the game and is palced through the Unity editor.
 */

public class InitBasepointScr : MonoBehaviour {

	void Start () {
        // Make invisible ingame
        this.GetComponent<MeshRenderer>().enabled = false;
	}

}
