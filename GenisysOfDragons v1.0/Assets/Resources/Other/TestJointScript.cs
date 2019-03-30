using UnityEngine;
using System.Collections;

/*
 * ANOTHER TEST SCRIPT - OBSOLETE!
*/

public class TestJointScript : MonoBehaviour {

    public GameObject otherObject;


	// Use this for initialization
	void Start () {

        GameObject primaryObject = gameObject;

        HingeJoint hinge = primaryObject.AddComponent<HingeJoint>();

        hinge.connectedBody = otherObject.GetComponent<Rigidbody>();
        hinge.enableCollision = false;
        hinge.enablePreprocessing = true;
        hinge.autoConfigureConnectedAnchor = true;

        hinge.anchor = (new Vector3(0, 0, -2.2f)) / primaryObject.transform.localScale.z;
        //hinge.axis = Vector3.forward;
        //hinge.axis = otherObject.transform.localRotation * Vector3.forward;

        //hinge.axis = (Quaternion.Inverse(primaryObject.transform.localRotation) * otherObject.transform.localRotation) * Vector3.forward;
        hinge.axis = calculateRelativeAxis(primaryObject.transform.localRotation, otherObject.transform.localRotation, Vector3.right);

        Global.ShowMessage(hinge.anchor.ToString());
        Global.ShowMessage(hinge.axis.ToString());


        
        //hinge.connectedAnchor = hingePoint - newOrig;
        //hinge.connectedAnchor = new Vector3(0, 0, 0);

        //hinge.connectedAnchor = (new Vector3(-2.4f, 0, 0)) / otherObject.transform.localScale.x;





        JointSpring hingeSpring = hinge.spring;
        hingeSpring.spring = 100;
        hingeSpring.damper = 0.5f;
        hingeSpring.targetPosition = 0;//chrom.springTargetPositions[0]; // start spring position

        hinge.spring = hingeSpring;
        hinge.useSpring = true;


	}
	
	// Update is called once per frame
	void Update () {
	
	}

    Vector3 calculateRelativeAxis(Quaternion rotA, Quaternion rotB, Vector3 direction)
    {
        return ((Quaternion.Inverse(rotA) * rotB) * direction);
    }


}
