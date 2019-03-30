using UnityEngine;
using System.Collections;


/*
 * NOT AI RELATED
 * 
 * This little helper script is attached to the similar named object and serves as manual spots for the camera.
 * Generally one can alternate with the <Spacebar> key through those "spots" afetr tehy have been palced in the Unity editor and asigned a correct indexNumber.
 * The spots are also used by the script "DragonControler" for the demonstration trailers.
 */


public class CameraSpotScript : MonoBehaviour {

    // This variable is to be set through the editor
    public int indexNumber;

    public Quaternion startLookAt;

    public bool attached = false;
    GameObject AttachedObject;

    Vector3 attachedVector;


    void Start()
    {
        // Make invisible ingame
        this.GetComponent<MeshRenderer>().enabled = false;
    }

    void Update()
    {
        // Follow an object
        if (attached)
            this.gameObject.transform.position = AttachedObject.transform.position + attachedVector;
    }

    public void AttachToObject(GameObject obj, bool attach)
    {
        attached = attach;
        AttachedObject = obj;

        if (attach)
            attachedVector = this.gameObject.transform.position - obj.transform.position;
    }

}
