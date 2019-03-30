using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * NOT AI RELATED but required for SIMULATION
 * 
 * This core script following the "factory pattern" handles the creation of samples into the world, based on a provided phenotype.
 * The process is highly recursive and constructs the wings step by step based on the phenotype's chromosomes.
 */


public class DragonFactory : MonoBehaviour {


    // Prefabs
    static GameObject DragonBody, DragonBone;
    
    // Temporary variables
    //static GameObject currentBody;
    static public int nextChrom = 0;
    static public int segmentPos = 0;
    static public bool leftWing = true;


    // Get the prefabs into variables
    static public void Init()
    {
        DragonBody = (GameObject)Resources.Load("Dragon/SimplifiedDragon/DragonBody");
        DragonBone = (GameObject)Resources.Load("Dragon/SimplifiedDragon/DragonBone");
    }


    // Use the prefab to instantiate an actual body of the sample (the block at the middle)
    static public GameObject CreateDragon(Phenotype type, Vector3 startPosition, Quaternion startRotation, int index)
    {
        // Create new body
        GameObject body = (GameObject)Instantiate(DragonBody, startPosition, startRotation);

        DragonScript scr = body.gameObject.GetComponent<DragonScript>();
        scr.index = index;

        // Create and atatch a wing
        AttachWing(body, type);

        return (body);
    }


    // Starts constructing the wing
    static public void AttachWing(GameObject body, Phenotype type)
    {
        int wingParts = type.TypeChromosomes.Count + 1;

        // Create wing segments
        Rigidbody[] wingSegments = new Rigidbody[2 * wingParts];
        HingeJoint[] wingJoints = new HingeJoint[2 * wingParts];

        segmentPos = 0;
        nextChrom = -1;
        leftWing = true;
        attachNextSegment(body, type, type.rootChromL, 0, wingSegments, wingJoints); // Left wing
        nextChrom = -1;
        leftWing = false;
        attachNextSegment(body, type, type.rootChromR, 0, wingSegments, wingJoints); // Right wing


        // Add the wing to the global set via the WingFactory
        WingSetFactory.AddWing(body, type, wingSegments, wingJoints);


        // Attach the second bone of the wings, to the body (the second bone is always one parallel to the body, simulating the length where the wing is connected to the body)
        addBoneJoint(body, wingSegments[1].gameObject, type.rootChromL.boneOffset, type.rootChromL.boneOffset.magnitude, 0, 0, type.rootChromL.muscleForce / 3);
        addBoneJoint(body, wingSegments[wingParts + 1].gameObject, type.rootChromR.boneOffset, type.rootChromR.boneOffset.magnitude, 0, 0, type.rootChromR.muscleForce / 3);

    }


    // Attaches the wing segments recursively
    static public void attachNextSegment(GameObject obj, Phenotype type, Chromosome chrom, int el, Rigidbody[] segments, HingeJoint[] joints)
    {
        el++;
        nextChrom++;

        if (chrom.numOfJoints == 0)
        {
            obj = attachBone(obj, chrom.boneOffset, chrom.boneAddRot, chrom.boneLength, chrom.boneThickness, chrom, el, joints);
            obj.name = (nextChrom).ToString();

            segments[segmentPos++] = obj.GetComponent<Rigidbody>();
        }
        else
        {
            if (chrom.numOfJoints == 1)
            {
                obj = attachBone(obj, chrom.boneOffset, chrom.boneAddRot, chrom.boneLength, chrom.boneThickness, chrom, el, joints);
                obj.name = (nextChrom - 1).ToString();
                segments[segmentPos++] = obj.GetComponent<Rigidbody>();

                attachNextSegment(obj, type, type.TypeChromosomes[nextChrom], el, segments, joints);
            }
            else
            {
                obj = attachBone(obj, chrom.boneOffset, chrom.boneAddRot, chrom.boneLength, chrom.boneThickness, chrom, el, joints);
                obj.name = (nextChrom).ToString();
                segments[segmentPos++] = obj.GetComponent<Rigidbody>();

                for (int b = chrom.numOfJoints; b > 0; b--)
                    attachNextSegment(obj, type, type.TypeChromosomes[nextChrom], el, segments, joints);
            }
        }


        // Normally bones are uncolored when not visible
        if (el > 0)
            if (GlobalSettings.renderingBodyAndBones) ColorizeBone(obj, chrom);
    }




    // Attaches a bone
    static GameObject attachBone(GameObject rootBone, Vector3 rootOffset, Quaternion newAttachDirection, float newLength, float newThickness, Chromosome chrom, int el, HingeJoint[] joints)
    {

        Quaternion newDirection = rootBone.transform.rotation * newAttachDirection;
        Vector3 hingePoint = rootBone.transform.position + rootBone.transform.rotation * (new Vector3(0, 0, rootBone.transform.localScale.z / 2) + rootOffset);

        Vector3 newOrig = hingePoint + newDirection * (new Vector3(0, 0, newLength / 2));
        GameObject newBone = (GameObject)Instantiate(DragonBone, newOrig, newDirection);

        newBone.transform.localScale = new Vector3(newThickness, newThickness, newLength);

        joints[segmentPos] = addBoneJoint(rootBone, newBone, rootOffset, newLength, chrom.muscleMinLimit, chrom.muscleMaxLimit, chrom.muscleForce);

        return (newBone);
    }


    // Conenct with a joint
    static HingeJoint addBoneJoint(GameObject objA, GameObject objB, Vector3 offset, float lengthOfB, int minLimit, int maxLimit, float force)
    {

        // Create a new "hinge joint"
        HingeJoint hinge = objA.AddComponent<HingeJoint>();

        // Attach the other body
        hinge.connectedBody = objB.GetComponent<Rigidbody>();
        // Other settings
        hinge.enableCollision = false;
        hinge.enablePreprocessing = true;
        hinge.autoConfigureConnectedAnchor = true;

        hinge.anchor = (Vector3.forward / 2) + (offset * 0.5f) / lengthOfB;
        hinge.axis = (Quaternion.Inverse(objA.transform.localRotation) * objB.transform.localRotation) * (leftWing ? Vector3.right : Vector3.left);


        //Add normal joint and use motor

        JointLimits limits = new JointLimits();
        limits.min = minLimit;
        limits.max = maxLimit;
        limits.bounciness = 0f;

        JointMotor motor = new JointMotor();
        motor.freeSpin = !true;
        motor.force = force;
        motor.targetVelocity = -GlobalSettings.dragonMaxMuscleVelocity;

        hinge.limits = limits;
        hinge.motor = motor;
        hinge.useMotor = true;
        hinge.useLimits = true;


        // Break force is currently disabled due to undesired effects
        //hinge.breakForce = 500;
        //hinge.breakTorque = 2000;



        /*
        // Old variant using springs instead of motors and limtis. That was promising smoother results, but the engine doesn't seem to be stable enough for the purpose of this project. 
        
        JointSpring hingeSpring = hinge.spring;
        hingeSpring.spring = force;
        hingeSpring.damper = 0.5f;
        hingeSpring.targetPosition = 0;
        hinge.spring = hingeSpring;
        hinge.useSpring = true;
        */

        return (hinge);
    }


    static void ColorizeBone(GameObject obj, Chromosome chrom)
    {
        obj.GetComponent<Renderer>().material.color = chrom.col;
    }

}
