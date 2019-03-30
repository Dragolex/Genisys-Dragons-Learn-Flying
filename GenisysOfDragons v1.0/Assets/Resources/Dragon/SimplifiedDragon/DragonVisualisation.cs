using UnityEngine;
using System.Collections;


/* 
 * NOT AI RELATED!
 * 
 * This script is only added at runtime when rendering is active.
 * It is not direectly influencing the behavior of any sample or the AI algorithm.
 * 
 * Its main purpose is to recolor the body depending on the performance and
 * it triggers the script which updates the visualisation of the individual wing membrane.
 */


public class DragonVisualisation : MonoBehaviour {

    Transform dragonTransform;
    Renderer dragonRenderer;
    Rigidbody dragonBody;
    public DragonScript dragonScr;

    public float localBestHeight = -1000;

    bool inAITest = true;


	// Use this for initialization
	void Start () {

        dragonTransform = (Transform)this.GetComponent(typeof(Transform));
        dragonRenderer = (Renderer)this.GetComponent<Renderer>();
        dragonBody = (Rigidbody)this.GetComponent(typeof(Rigidbody));
        dragonScr = (DragonScript)this.GetComponent(typeof(DragonScript)); 

        inAITest = (Global.GetCurrentScene() == Global.mainAIScene);

	}


    void FixedUpdate()
    {
        if (GlobalSettings.renderingMembranes)
            dragonScr.wingSet.UpdateVisualisation();
    }

	
	// Update is called once per frame
	void Update () {

        if (this.transform.position.y > localBestHeight)
            localBestHeight = this.transform.position.y;


        if (inAITest)
        {
            if (dragonBody != null)
            if (!(dragonBody.Equals(Global.mainCameraScr.moveTarget) || dragonBody.Equals(Global.mainCameraScr.moveTargetTouched)))
            {
                foreach (FloorLevel floorLevel in Global.envControlerScr.floorLevels)
                {
                    if (floorLevel.level < dragonTransform.position.y)
                        dragonRenderer.material.color = floorLevel.col;
                }

                if (((Global.envControlerScr.bestSample != null) && (Global.envControlerScr.bestSample.gameObject == this.gameObject)) || (Global.GUIControlerScr.sampleToObserve == this.gameObject))
                    dragonRenderer.material.color = GlobalSettings.focusColor; 
                 
            }
        }
	}
}
