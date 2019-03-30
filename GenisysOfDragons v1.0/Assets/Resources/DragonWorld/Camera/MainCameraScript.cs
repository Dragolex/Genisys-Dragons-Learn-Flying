using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * NOT AI RELATED
 * 
 * This large script handles everything related to the camera.
 * Looking backwards, it might have been smarter to use mroe than one camera and split all this into multiple objects and scripts, however, it works.
 *
 * Things it handles:
 * - Camera positions with "CameraSpots"
 * - Manual camera mvoement via keybaord and mouse
 * - Doubleclick switch
 * - Cursor and scrollwheel
 * - Object dragging and pulling (see tutorial)
 */




public class MainCameraScript : MonoBehaviour
{

    // Settings
    float movSpeed = 1.5f;
    float rotSpeed = 4f;
    float shiftSpeedStart = 8f;
    float shiftSpeedLimit = 20f;
    float shiftSpeed = 8f;

    Color mouseHoverColor = Color.yellow;
    Color mouseMovingColor = Color.red;


    // Arrays and variables for positions
    public List<CameraSpotScript> cameraSpots = new List<CameraSpotScript>();
    public GameObject cameraFocusObject = null;
    CameraSpotScript lockedSpot = null;


    // Mouse moving variables
    RaycastHit mouseTargetHit = new RaycastHit();
    Ray mouseTargetRay = new Ray();
    bool moveTargetAttached = false;
    float moveTargetDistance;
    public Rigidbody moveTarget;
    public Rigidbody moveTargetTouched;
    Color moveTargetColor;
    Color moveTargetHoverColor;


    // Data variables

      //public
    public bool moveView = false;
    public float doubleClick = 0;

      //local
    GameObject cursorObj;

    Vector3 desiredCameraPos, relativeFocusVector = Vector3.zero;
    Quaternion desiredLookAt;

    public int currentSpot = 0;
    float posChangeFactor;



    // Use this for initialization
    void Awake()
    {

        // Prepare start positions for the camera depending on the CameraSpots
        GameObject basepoint = GameObject.Find("Basepoint");

        Vector3 position = new Vector3(0, 0, 0); // position with offset
        if (basepoint == null) position += GlobalSettings.originPoint;
        else position += basepoint.transform.position;


        GameObject[] camSpots = (GameObject[])GameObject.FindGameObjectsWithTag("CameraSpotTag");
        int maxFound = -1; CameraSpotScript spot; int ind = 0;

        
        while (ind < (camSpots.Length))
        {
            spot = camSpots[ind].GetComponent<CameraSpotScript>();
            if (spot.indexNumber == (maxFound+1))
            {
                spot.startLookAt = Quaternion.LookRotation(position - spot.transform.position);
                cameraSpots.Add(spot);
                maxFound = spot.indexNumber;
                ind = -1;
            }
            ind++;
	    }

        if (camSpots.Length != cameraSpots.Count)
            Debug.LogError("The Index of a CameraSpot is probably missing in the scene!");




        // Set the first camera spot
        GotoCameraSpot(currentSpot);


        // Handle cursor
        cursorObj = GameObject.Find("Cursor");
        cursorObj.SetActive(false);

    }


    // Set a desired focus
    public void SetCameraFocusObject(GameObject obj)
    {
        if (obj != null)
        {
            cameraFocusObject = obj;
            relativeFocusVector = desiredCameraPos - obj.transform.position;
        }
    }


    public void GotoCameraSpot(int spotNum)
    {
        CameraSpotScript spot = cameraSpots[spotNum];
        currentSpot = spotNum;

        posChangeFactor = 0.25f;

        desiredCameraPos = spot.transform.position;
        desiredLookAt = spot.startLookAt;

        if ((cameraFocusObject != null) && (relativeFocusVector == Vector3.zero))
            relativeFocusVector = desiredCameraPos - cameraFocusObject.transform.position;

        lockedSpot = null; // Unlock from spot
    }

    public void GotoWatchObject(GameObject obj)
    {
        cameraFocusObject = obj;
        desiredCameraPos = obj.transform.position + relativeFocusVector*1.25f;
    }
    public void GotoWatchObjectDistance(GameObject obj, float distance)
    {
        cameraFocusObject = obj;
        desiredCameraPos = obj.transform.position + relativeFocusVector.normalized * distance;
    }
    public void GotoWatchObjectDistance(float distance)
    {
        desiredCameraPos = cameraFocusObject.transform.position + relativeFocusVector.normalized * distance;
    }

    public void LockToCameraSpot()
    {
        lockedSpot = cameraSpots[currentSpot];
    }
    public void LockToCameraSpot(int spotNum)
    {
        if (spotNum < 0)
            lockedSpot = null;
        else lockedSpot = cameraSpots[spotNum]; // Lock to spot
    }


    // Update is called once per frame
    void Update()
    {

        // If locked to a spot
        if (lockedSpot != null)
        {
            desiredCameraPos = lockedSpot.transform.position + new Vector3(0, 3f, 0);
            desiredLookAt = lockedSpot.startLookAt;
        }



        // Switch Camera spots on keypress
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpotLoopNext();

            if (Global.GetCurrentScene() == Global.mainAIScene)
            {
                if (Global.envControlerScr.justMovedToWatchNew)
                {
                    Global.envControlerScr.justMovedToWatchNew = false;
                    GotoWatchObjectDistance(35 + Mathf.Sqrt(GlobalSettings.populationSize) * 35);
                }
            }


        }

        // Turn at a chosen obejct
        if (!moveView)
            if (cameraFocusObject != null)
                desiredLookAt = Quaternion.LookRotation(cameraFocusObject.transform.position - this.transform.position);



        // Update Camera position and direction with smooth transition

        this.transform.position = Vector3.Slerp(this.transform.position, desiredCameraPos, posChangeFactor * Time.deltaTime * movSpeed);
        this.transform.rotation = Quaternion.Slerp(transform.rotation, desiredLookAt, Time.deltaTime * rotSpeed);
        //this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, desiredLookAt, Time.deltaTime * 50); //This method of rotating has issues
        



        // MOUSE ACTIONS ////////////////////////////

        // Check mosuebutton down
        if (Input.GetMouseButton(0))
        {
            // Check doubleclick for locking view
            if (doubleClick == 0)
                doubleClick = 1;
            else
                if (doubleClick >= 2)
                {
                    if (doubleClick < Time.timeScale * 30)
                    {
                        if (Cursor.lockState.Equals(CursorLockMode.None)) Cursor.lockState = CursorLockMode.Locked;
                        else Cursor.lockState = CursorLockMode.None;

                        Cursor.visible = !Cursor.visible;
                        cursorObj.SetActive(!cursorObj.activeSelf);
                        moveView = !moveView;

                        if (!moveView)
                        {
                            Global.GUIControlerScr.SwitchObjectToObserve(Global.envControlerScr.bestSample);
                            if (Global.GUIControlerScr.sampleToObserve != null)
                                Global.mainCameraScr.SetCameraFocusObject(Global.GUIControlerScr.sampleToObserve);
                            else Global.mainCameraScr.SetCameraFocusObject(Global.envControlerScr.bestSample);
                        }

                    }
                    doubleClick = 0;
                }
        }
        else if (doubleClick == 1) doubleClick = 2;
        if (doubleClick >= 2) doubleClick += Time.deltaTime*150;



        // COLOR CHANGE when hovering objects with mouse and chosing MOVEMENT-TARGET ////////////////////////////
        if (GlobalSettings.renderingBodyAndBones)
        {
            if ((moveView) && (!moveTargetAttached))
            {
                mouseTargetRay.origin = this.transform.position;
                mouseTargetRay.direction = this.transform.rotation * Vector3.forward;

                if (Physics.Raycast(mouseTargetRay, out mouseTargetHit, 150, -1))
                {
                    if (moveTargetTouched != null)
                        if (mouseTargetHit.rigidbody != moveTargetTouched)
                        {
                            moveTargetTouched.GetComponent<Renderer>().material.color = moveTargetHoverColor;
                            moveTargetTouched = null;
                        }


                    if (mouseTargetHit.rigidbody != null)
                        if (mouseTargetHit.rigidbody.tag == "PhenotypeElementTag")
                            if (!Input.GetMouseButtonDown(0))
                            {
                                if (moveTargetTouched != null)
                                {
                                    moveTargetTouched.GetComponent<Renderer>().material.color = moveTargetHoverColor;
                                    moveTargetTouched = null;
                                }
                                moveTargetTouched = mouseTargetHit.rigidbody;
                                moveTargetHoverColor = moveTargetTouched.GetComponent<Renderer>().material.color;
                                moveTargetTouched.GetComponent<Renderer>().material.color = mouseHoverColor;
                            }
                            else
                            {
                                if (moveTargetTouched != null)
                                {
                                    moveTargetTouched.GetComponent<Renderer>().material.color = moveTargetHoverColor;
                                    moveTargetTouched = null;
                                }
                                

                                moveTarget = mouseTargetHit.rigidbody;

                                // Moving object at distance
                                moveTargetAttached = true;
                                moveTargetDistance = (this.transform.position - moveTarget.gameObject.transform.position).magnitude;


                                Global.GUIControlerScr.SwitchObjectToObserve(moveTarget.gameObject);


                                // Save and Change color

                                moveTargetColor = moveTarget.GetComponent<Renderer>().material.color;
                                moveTarget.GetComponent<Renderer>().material.color = mouseMovingColor;
                            }
                }
                else
                {
                    if (moveTargetTouched != null)
                    {
                        moveTargetTouched.GetComponent<Renderer>().material.color = moveTargetHoverColor;
                        moveTargetTouched = null;
                    }
                }
            }
        }



        // ESCAPE ////////////////////////////
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cursorObj.SetActive(false);
            moveView = false;

            Global.GUIControlerScr.SwitchObjectToObserve(Global.envControlerScr.bestSample);
            Global.mainCameraScr.SetCameraFocusObject(Global.GUIControlerScr.sampleToObserve);


            if (moveTargetAttached)
            {
                moveTargetAttached = false;
                if (moveTarget != null) moveTarget.GetComponent<Renderer>().material.color = moveTargetColor;
            }
        }




        // MOVE CAMERA ////////////////////////////

        shiftSpeed = Mathf.Min(shiftSpeedLimit, Mathf.Max(shiftSpeedStart, shiftSpeed *= 1 - Time.deltaTime));

        if (Input.GetKey(KeyCode.W))
        { desiredCameraPos += Time.deltaTime * shiftSpeed * (desiredLookAt * (new Vector3(0, 1, 0))); shiftSpeed *= 1 + shiftSpeed * Time.deltaTime; }
        if (Input.GetKey(KeyCode.A))
        { desiredCameraPos += Time.deltaTime * shiftSpeed * (desiredLookAt * (new Vector3(-1, 0, 0))); shiftSpeed *= 1 + shiftSpeed * Time.deltaTime; }
        if (Input.GetKey(KeyCode.S))
        { desiredCameraPos += Time.deltaTime * shiftSpeed * (desiredLookAt * (new Vector3(0, -1, 0))); shiftSpeed *= 1 + shiftSpeed * Time.deltaTime; }
        if (Input.GetKey(KeyCode.D))
        { desiredCameraPos += Time.deltaTime * shiftSpeed * (desiredLookAt * (new Vector3(1, 0, 0))); shiftSpeed *= 1 + shiftSpeed * Time.deltaTime; }

        if (Input.GetKey(KeyCode.E))
        { desiredCameraPos += Time.deltaTime * shiftSpeed * (desiredLookAt * (new Vector3(0, 0, 1))); shiftSpeed *= 1 + shiftSpeed * Time.deltaTime; }
        if (Input.GetKey(KeyCode.Q))
        { desiredCameraPos += Time.deltaTime * shiftSpeed * (desiredLookAt * (new Vector3(0, 0, -1))); shiftSpeed *= 1 + shiftSpeed * Time.deltaTime; }



        // MOVE rigid bodies by mouse when clicking ////////////////////////////

        if (moveTargetAttached)
        {
            if ((Input.GetMouseButtonUp(0)) || (moveTarget == null))
            {
                if (moveTarget != null) moveTarget.GetComponent<Renderer>().material.color = moveTargetColor;
                moveTarget = null;
                moveTargetAttached = false;
            }
            else
            {
                moveTargetDistance += Input.GetAxis("Mouse ScrollWheel") * 3;

                Vector3 mouseViewVector = this.transform.TransformDirection(Vector3.forward);
                Vector3 targetPosition = this.transform.position + moveTargetDistance * mouseViewVector;


                // Apply force
                Vector3 force = (targetPosition - moveTarget.transform.position) * Time.deltaTime * 1000;
                moveTarget.AddForce(force - moveTarget.velocity * Time.deltaTime * 75);

                //moveTargetColor = moveTarget.GetComponent<Renderer>().material.color;
                //moveTarget.GetComponent<Renderer>().material.color = mouseMovingColor;
            }
        }
        else
        {
            float wheel = Input.GetAxis("Mouse ScrollWheel");

            if (wheel > 0)
            { desiredCameraPos += Time.deltaTime * shiftSpeedLimit * shiftSpeed * (desiredLookAt * (new Vector3(0, 0, 1))); shiftSpeed *= 1 + shiftSpeedLimit * shiftSpeed * Time.deltaTime; }
            if (wheel < 0)
            { desiredCameraPos += Time.deltaTime * shiftSpeedLimit * shiftSpeed * (desiredLookAt * (new Vector3(0, 0, -1))); shiftSpeed *= 1 + shiftSpeedLimit * shiftSpeed * Time.deltaTime; }
        }




        // VIEWPOINT MOVING ////////////////////////////
        if (moveView)
        {
            float mouseSense = 1.25f;

            float xdif = Input.GetAxis("Mouse X");
            float ydif = Input.GetAxis("Mouse Y");

            desiredLookAt *= Quaternion.AngleAxis(xdif * mouseSense , Vector3.up);
            desiredLookAt *= Quaternion.AngleAxis(ydif * mouseSense , Vector3.left);

        }


        posChangeFactor = Mathf.Min(1, posChangeFactor + 0.65f*Time.deltaTime);


    }

    public void SpotLoopNext()
    {
        currentSpot += 1;
        if (currentSpot >= cameraSpots.Count) currentSpot = 0;

        GotoCameraSpot(currentSpot);
    }


    Vector3 VectorSqrt(Vector3 invec)
    {
        if (invec.x < 0) invec.x = -Mathf.Sqrt(-invec.x);
        else invec.x = Mathf.Sqrt(invec.x);

        if (invec.y < 0) invec.y = -Mathf.Sqrt(-invec.y);
        else invec.y = Mathf.Sqrt(invec.y);

        if (invec.z < 0) invec.z = -Mathf.Sqrt(-invec.z);
        else invec.z = Mathf.Sqrt(invec.z);

        return (invec);
    }

}
