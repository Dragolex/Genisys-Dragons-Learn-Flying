using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * This script is attached to every sample (even in learning mode)
 * 
 * It holds the sample#s indey, handles destruction of the wing when the whole sample is destroyed and
 * it triggers the individual physics update.
 * 
 * In a first realisation, it was also checking for the numebr of taken wingflaps, however,t hat is outdated and disabled.
 */


public class DragonScript : MonoBehaviour {

    public int index = -1;


    // The wingSet Obejct related to this testsample
    public WingSet wingSet = null;


    void OnDestroy()
    {
        if (index == -1) return;

        // Destroy the wing membrane
        WingSetFactory.DestroyWing(wingSet);
    }


    /*
    void OnJointBreak(float breakForce)
    {
        Global.ShowMessage(breakForce.ToString());

        Destroy(this);
    }*/


    // Physics update
    void FixedUpdate()
    {
        if (index == -1) return;

        // Update and apply physics
        wingSet.UpdatePhysics();


        /*
        This was a first way of detecting the numebr of wingflaps depending on its actual movement.
        A new method has been implemented directly in the WingSet.
        
        float lastDirectionChange = 0;
        public byte movingDirection = 0;
        public byte directionChanges = 0;
         
        if (checkSelf)
        {
            switch (movingDirection)
            {
                case 0: // Not found a movement direction yet
                    if (Mathf.Abs(transform.position.y - lastDirectionChange) > GlobalSettings.directionChangeThreshold)
                        movingDirection = (byte)((transform.position.y > lastDirectionChange) ? 2 : 1);
                    break;
                case 1: // Currently moving upwards
                    if (transform.position.y < lastDirectionChange)
                        lastDirectionChange = transform.position.y;
                    else if ((transform.position.y - lastDirectionChange) > GlobalSettings.directionChangeThreshold)
                    {
                        movingDirection = 2;
                        directionChanges++;

                        if (directionChanges == GlobalSettings.directionChangeLimit)
                        {
                            PopulationControler.EvaluateSample(gameObject, this); // Call fitness function
                            directionChanges = 0;
                        }
                    }
                    break;
                case 2: // Currently moving downwards
                    if (transform.position.y > lastDirectionChange)
                        lastDirectionChange = transform.position.y;
                    else if ((lastDirectionChange - transform.position.y) > GlobalSettings.directionChangeThreshold)
                    {
                        movingDirection = 1;
                        directionChanges++;

                        if (directionChanges == GlobalSettings.directionChangeLimit)
                        {
                            PopulationControler.EvaluateSample(gameObject, this); // Call fitness fucntion
                            directionChanges = 0;
                        }
                    }
                    break;
            }
        }
        */

    }

}
