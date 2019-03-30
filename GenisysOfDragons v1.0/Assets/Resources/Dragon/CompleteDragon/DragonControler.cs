using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * NOT AI RELATED!
 * 
 * This script is only required for demonstration purposes like during the CliffScene.
 * It handles things like the dragons actions like head movement, tailwag and so.
 * It also allows controls via keyboard and contains eevrything related to the video-scenes
 * 
 * Thsoe scenes have to be run "manually" as the <control> key of the keybaord controls each step of the dialogue, etc.
 * See the large SWITCH statement down below.
 * 
 */


public class DragonControler : MonoBehaviour {

    public GameObject followerObject = null;


    bool trailerAnim;

    bool swing = true;
    float swingPos = 0.5f;
    float swingStrength = 200;

    public List<HingeJoint> joints = new List<HingeJoint>();
    public List<float> springTargetPositions = new List<float>();

    Rigidbody body, jaw, head, tailClaw; //tailBase
    float tempForce1 = 0, tempForce2 = 0;


    float moveForce = 200;

    float wagPos = 0;
    float wagTime = 1.5f;

    Vector3 followerOffset;


    TextFieldScript titleTextfield, dragonTextfield, tamerTextfield;

    int dialogState = -1;
    bool dragonTextfieldMoving = false;
    bool tamerTextfieldMoving = false;

    Vector2 tamerTextfieldDesiredPos = new Vector2(10, Screen.height - 130);

    bool initGUI = true;

    float titleMoving = 0;

    float speakingSpeed = 0.05f;

    float dragonAccelerating = 0;
    bool dragonFalling = false;

    int titleDisplayTimer = 200;



    void Start () {

        // Check whether being in cliffScene (for the trailer animation) or in mainAIScene (for demonstrating/testing)

        trailerAnim = (Global.GetCurrentScene() == Global.cliffScene);

        PrepareComponents();


        if (trailerAnim)
        {
            // Create dialogue windows

            GameObject TextFieldObj = (GameObject)Resources.Load("DragonWorld/GUI/TextFieldObj");


            titleTextfield = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
            titleTextfield.InitializeTextField(-50, -125, Screen.width + 100, 200, 10, 8, false, new Color(1f, 1f, 1), 150, FontStyle.Bold);

            titleTextfield.AddTextLine("");
            titleTextfield.AddTextLine("");
            titleTextfield.AddTextLine("");
            titleTextfield.AddTextLine("       Genisys Of Dragons");


            dragonTextfield = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
            dragonTextfield.InitializeTextField(2000, 400, 600, 50, 10, 2, false, new Color(0.8f, 1, 0.8f), 35, FontStyle.BoldAndItalic);


            tamerTextfield = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
            tamerTextfield.InitializeTextField(10, Screen.height + 200, Screen.width - 40, 50, 10, 2, false, new Color(0.8f, 0.8f, 1), 35, FontStyle.Bold);


        }
        else SwitchGravity(false);


        followerOffset = new Vector3(0, -2.25f, -1);
    }


    public void PrepareComponents()
    {
        foreach (Transform child in this.transform) {

            if (child.gameObject.name == "MainBody")
                body = child.GetComponent<Rigidbody>();

            if (child.gameObject.name == "Jaw")
                jaw = child.GetComponent<Rigidbody>();

            if (child.gameObject.name == "Head")
                head = child.GetComponent<Rigidbody>();

            /*if (child.gameObject.name == "Tail 1")
            {
                tailBase = child.GetComponent<Rigidbody>();
            }*/

            if (child.gameObject.name == "TailClaw")
            {
                tailClaw = child.GetComponent<Rigidbody>();
            }
        }
    }

    void OnGUI()
    {
        if (initGUI)
        {
            GUI.skin.label.fontSize = 35;
            GUI.skin.label.fontStyle = FontStyle.Bold;            
        }
    }


	// Update is called once per frame
	void Update () {
        if (swing)
            swingPos += swingStrength * 0.005f * Time.deltaTime;

        titleDisplayTimer--;
        if (titleDisplayTimer == 0)
            titleMoving = 0.05f;


        // KEYBAORD CONTROL ////////////////////////////


         //if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.Z))

         if (Input.GetKeyDown(KeyCode.U))
             tempForce1 = 1;
         if (tempForce1 > 0)
             body.AddForce(new Vector3(0, tempForce1 * 2000 * Time.deltaTime, 0));
         tempForce1 -= Time.deltaTime;


         if (Input.GetKeyDown(KeyCode.J))
             jaw.AddTorque(new Vector3(2000 * Time.deltaTime, 0, 0));


         if (Input.GetKeyDown(KeyCode.G))
             tempForce2 = 0.5f;
         if (tempForce2 > 0)
         {
             head.AddForce(new Vector3(0, tempForce2 * 1100 * Time.deltaTime, 0));
             jaw.AddForce(new Vector3(0, tempForce2 * 300 * Time.deltaTime, 0));
         }
         tempForce2 -= Time.deltaTime;

         if (Input.GetKey(KeyCode.H))
             head.AddForce(new Vector3(0, -500*Time.deltaTime, 0));

         

         if (Input.GetKey(KeyCode.UpArrow))
         {
             body.AddForce(new Vector3(0, 0, moveForce*Time.deltaTime));
         }
         if (Input.GetKey(KeyCode.DownArrow))
         {
             body.AddForce(new Vector3(0, 0, -moveForce * Time.deltaTime));
         }
         if (Input.GetKey(KeyCode.RightArrow))
         {
             body.AddForce(new Vector3(moveForce * Time.deltaTime, 0, 0));
         }
         if (Input.GetKey(KeyCode.LeftArrow))
         {
             body.AddForce(new Vector3(-moveForce * Time.deltaTime, 0, 0));
         }



         if (trailerAnim)
         if (Input.GetKeyDown(KeyCode.L))
         {
             if (!Global.mainCameraScr.cameraSpots[Global.mainCameraScr.currentSpot].attached)
             {
                 Global.mainCameraScr.LockToCameraSpot(Global.mainCameraScr.currentSpot);
                 Global.mainCameraScr.cameraSpots[Global.mainCameraScr.currentSpot].AttachToObject(this.transform.gameObject, true);
             }
             else
             {
                 Global.mainCameraScr.LockToCameraSpot(-1);
                 Global.mainCameraScr.cameraSpots[Global.mainCameraScr.currentSpot].AttachToObject(null, false);
             }
         }


        // Tailwag
        
        wagPos += Time.deltaTime;

        if (wagPos < wagTime)
        {
            tailClaw.AddForce(new Vector3(wagPos, 0.2f, 0));
        }
        else if (wagPos < 2.1f * wagTime)
        {
            if (wagPos > 1.1f * wagTime)
            tailClaw.AddForce(new Vector3(- (wagPos - wagTime*1.1f), -0.2f, 0));
        }
        else wagPos = 0;



        // Dialogue ////////////////////////////

        if (trailerAnim)
        if (Input.GetKeyDown(KeyCode.LeftControl) == true)
        {

            switch(dialogState)
            {
                case -1:
                    Global.mainCameraScr.SpotLoopNext();
                    break;
                case 0:
                    Global.mainCameraScr.SpotLoopNext();
                    break;

                case 1:
                    titleMoving = 0.1f;
                    Global.mainCameraScr.SpotLoopNext();
                    break;

                case 2:
                    tamerTextfieldMoving = true;
                    if (GlobalSettings.language == "de") {tamerTextfield.AddTextLine("Hallo, mein kleiner Drache!", speakingSpeed, 0.5f);};
                    if (GlobalSettings.language == "en") {tamerTextfield.AddTextLine("Hallo there, little dragon!", speakingSpeed, 0.5f);};
                    break;

                case 3:
                    if (GlobalSettings.language == "de") tamerTextfield.AddTextLine("Du willst heute zum ersten mal fliegen, nicht wahr?", speakingSpeed / 1.25f);
                    if (GlobalSettings.language == "en") tamerTextfield.AddTextLine("You want to fly for the first time today, don't you?", speakingSpeed / 1.25f);
                    tempForce1 = 0.5f;
                    break;

                case 4:
                    tempForce2 = 0.6f;
                    dragonTextfieldMoving = true;
                    if (GlobalSettings.language == "de") dragonTextfield.AddTextLine("Raaaawr!!", speakingSpeed / 2, 0.2f);
                    if (GlobalSettings.language == "en") dragonTextfield.AddTextLine("Raaaawr!!", speakingSpeed / 2, 0.2f);
                    break;
                case 5:
                    tamerTextfield.RemoveTextLine(0);
                    tamerTextfield.RemoveTextLine(0);
                    dragonAccelerating = 350f;

                    if (GlobalSettings.language == "de") dragonTextfield.AddTextLine("Jaa! Komm, ich leg gleich los!!", speakingSpeed / 2f);
                    if (GlobalSettings.language == "en") dragonTextfield.AddTextLine("Yeees! C'mon, let's gooo!!", speakingSpeed / 2f);
                    break;

                case 6:
                    dragonTextfield.RemoveTextLine(0);
                    
                    // Lock to the back of the dragon
                    Global.mainCameraScr.SpotLoopNext();
                    Global.mainCameraScr.LockToCameraSpot(Global.mainCameraScr.currentSpot);
                    Global.mainCameraScr.cameraSpots[Global.mainCameraScr.currentSpot].AttachToObject(body.gameObject, true);

                    dragonAccelerating = 750f;
                    tempForce1 = 0.12f;
                    SwitchGravity(false);
                    break;

                case 7:
                    dragonTextfield.RemoveTextLine(0);
                    if (GlobalSettings.language == "de") dragonTextfield.AddTextLine("Ich fliege! Ich fliege..", speakingSpeed / 1.25f);
                    if (GlobalSettings.language == "de") dragonTextfield.AddTextLine("Ich fliiee...", speakingSpeed / 1.5f, 1.35f);
                    if (GlobalSettings.language == "de") tamerTextfield.AddTextLine("Äh.. mo..moment! Weisst du überhaupt wie...", speakingSpeed, 0.5f);

                    if (GlobalSettings.language == "en") dragonTextfield.AddTextLine("Im'm flying, I'm flying!", speakingSpeed / 1.25f);
                    if (GlobalSettings.language == "en") dragonTextfield.AddTextLine("I'm fly...", speakingSpeed / 1.5f, 1.35f);
                    if (GlobalSettings.language == "en") tamerTextfield.AddTextLine("Eep, moment.. do you even know how?..", speakingSpeed, 0.5f);
                    tempForce2 = 0.25f;
                    break;

                case 8:
                    Global.mainCameraScr.cameraSpots[Global.mainCameraScr.currentSpot].AttachToObject(null, false);
                    Global.mainCameraScr.LockToCameraSpot(-1);
                    Global.mainCameraScr.SpotLoopNext();

                    dragonTextfield.RemoveTextLine(0);
                    dragonTextfield.RemoveTextLine(0);

                    if (GlobalSettings.language == "de") dragonTextfield.AddTextLine("Uhm.. Uh Oh...", speakingSpeed * 1.5f, 1f);
                    if (GlobalSettings.language == "en") dragonTextfield.AddTextLine("Uhm.. Uh Oh...", speakingSpeed * 1.5f, 1f);
                    break;

                case 9:
                    dragonAccelerating = -2000f;
                    dragonFalling = true;
                    dragonTextfieldMoving = false;

                    dragonTextfield.RemoveTextLine(0);
                    if (GlobalSettings.language == "de") dragonTextfield.AddTextLine("Ahhh...", speakingSpeed);
                    if (GlobalSettings.language == "en") dragonTextfield.AddTextLine("Ahhh...", speakingSpeed);
                    break;

                case 10:
                    dragonFalling = false;
                    SwitchGravity(true);
                    tamerTextfield.RemoveTextLine(0);
                    dragonTextfield.RemoveTextLine(0);
                    if (GlobalSettings.language == "de") dragonTextfield.AddTextLine("*SPLASH*... ...", speakingSpeed / 2);
                    if (GlobalSettings.language == "en") dragonTextfield.AddTextLine("*SPLASH*... ...", speakingSpeed / 2);
                    break;

                case 11:
                    dragonFalling = false;
                    if (GlobalSettings.language == "de") dragonTextfield.AddTextLine("Aua... :(", speakingSpeed / 2, 0.2f);
                    if (GlobalSettings.language == "en") dragonTextfield.AddTextLine("Ouch... :(", speakingSpeed / 2, 0.2f);
                    break;

                case 12:
                    if (GlobalSettings.language == "de") tamerTextfield.AddTextLine("Du hast noch einiges zu lernen...", speakingSpeed / 1.25f);
                    if (GlobalSettings.language == "de") tamerTextfield.AddTextLine("Aber keine Sorge, Junger Drache - Ein genetischer Algorithmus wird dir helfen!", speakingSpeed / 1.65f, 1.25f);

                    if (GlobalSettings.language == "en") tamerTextfield.AddTextLine("You have a lot to learn...", speakingSpeed / 1.25f);
                    if (GlobalSettings.language == "en") tamerTextfield.AddTextLine("But don't worry, young dragon - A genetic algorithm will help you!", speakingSpeed / 1.65f, 1.25f);
                    break;

                case 13:
                    dragonTextfield.RemoveTextLine(0);
                    break;

                case 14:
                    dragonTextfield.RemoveTextLine(0);
                    break;

                case 15:
                    if (GlobalSettings.language == "de") dragonTextfield.AddTextLine("Na gut!", speakingSpeed);
                    if (GlobalSettings.language == "en") dragonTextfield.AddTextLine("A..alright!", speakingSpeed);
                    break;

                case 16:
                    Global.SwitchScene(Global.mainAIScene);
                    break;


                case 30:
                    Global.mainCameraScr.SpotLoopNext();
                    Global.mainCameraScr.SpotLoopNext();
                    Global.mainCameraScr.SpotLoopNext();
                    if (GlobalSettings.language == "de") tamerTextfield.AddTextLine("Nun bist du bereit.", speakingSpeed);
                    if (GlobalSettings.language == "de") tamerTextfield.AddTextLine("Zeig uns wie ein inteligenter Drache fliegen kann!", speakingSpeed, 2);

                    break;


                case 35:
                    Global.SwitchScene(Global.cliffScene);
                    break;


            }

            // Switch to next dialogue step
            dialogState++;
            Debug.Log("Next dialogState step: " + dialogState.ToString());


        }

        if (trailerAnim)
        {
            if (dragonTextfieldMoving)
            {
                Vector2 headPos = Global.mainCameraScr.gameObject.GetComponent<Camera>().WorldToScreenPoint(head.transform.position);
                headPos.x += 85;
                dragonTextfield.ChangePosition(headPos, 0.1f);
            }
            if (tamerTextfieldMoving)
            {
                tamerTextfield.ChangePosition(tamerTextfieldDesiredPos, 0.1f);
            }


            if (titleMoving > 0)
            {
                titleTextfield.ChangePosition(new Vector2(-50, titleMoving - 50));
                titleMoving += 0.25f + titleMoving * 0.05f;
            }


            if (dragonAccelerating != 0)
            {
                Vector3 accVec = new Vector3(0, 0, dragonAccelerating * Time.deltaTime);

                head.AddForce(accVec);
                head.AddForce(new Vector3(0, 100 * Time.deltaTime, 0));
                body.AddForce(accVec);
                head.velocity = body.velocity;
            }



            if (dragonFalling)
            {
                Vector3 accVec = new Vector3(0, -700 * Time.deltaTime * 20, 0);

                body.AddForce(accVec);
                head.AddForce(accVec / 3);
            }
        }

	}

    public void SwitchGravity(bool gravity)
    {
        body.useGravity = gravity;
        tailClaw.useGravity = gravity;
    }



    // Physics update
    void FixedUpdate()
    {
        // When following a foreign body (used in testing mode in the mainAIScene)
        if (followerObject != null)
        {
            body.transform.position = followerObject.transform.position + followerObject.transform.rotation*followerOffset;
            body.transform.rotation = followerObject.transform.rotation * Quaternion.AngleAxis(-22f, Vector3.right);
        }
    }
}
