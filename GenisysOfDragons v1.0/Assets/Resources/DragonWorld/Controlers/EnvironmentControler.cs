using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
 * NOT AI RELATED!
 * 
 * This script handles some elements of visualisation of the environment in the "mainAIScene".
 * Basically it enables the translcuent grids ("floorlevels") whicha re meant to show whetehr a sample has ascended or descended.
 * 
 * It also handles the change of taskmode via the GUI button -> From learning to demonstration/test and back.
 */



public class EnvironmentControler : MonoBehaviour {

    // Foreign objects
    GameObject BaseQuad;

    GameObject CompleteDragonModel;
    GameObject completeDragon;


    public List<FloorLevel> floorLevels = new List<FloorLevel>();
    public GameObject floorOfBest, wallLeft, wallBack, wallRight, wallFront;

    public List<GameObject> wallVerticalWind = new List<GameObject>();

    public GameObject bestSample = null;
    public float bestSampleHeight = -1000;


    int testfieldHeight;

    DragonScript testSampleScript = null;

    Vector3 standardFloorScale;
    Vector2 standardFloorTextureScale;

    float sceneChangeTimer = 0;
    float sceneChangeDuration = 2f;
    string sceneChangeTo = "";


    Texture2D fadeTexture;
    Rect screenRect;

    public bool justMovedToWatchNew = false;

    bool renderPhTemp, renderMemTemp, inLearningMode = true;

    public Vector3 customGravityOnBestSample;


    // Initializes various environment variables and elements
	void Awake () {

        // Initialise static objects
        Global.Init();
        PopulationControler.Init();
        GlobalSettings.Init();
        DragonFactory.Init();


        CompleteDragonModel = (GameObject)Resources.Load("Dragon/CompleteDragon/TheDragon");


        // Screen fade
        sceneChangeTimer = sceneChangeDuration;
        screenRect = new Rect(0, 0, Screen.width, Screen.height);
        fadeTexture = new Texture2D(1, 1);



        if (Global.GetCurrentScene() == Global.mainAIScene)
        {

            // Create Floors
            BaseQuad = (GameObject)Resources.Load("DragonWorld/Surroundings/BaseQuad");
            standardFloorScale = BaseQuad.transform.localScale;
            standardFloorTextureScale = BaseQuad.GetComponent<Renderer>().sharedMaterial.mainTextureScale;


            // Local settings
            int numOfFloor = 5;
            int floorDist = 5;
            float floorAlpha = 0.225f;
            float wallAlpha = 0.1f;


            testfieldHeight = numOfFloor * floorDist;
            Color col, midcol = Color.white;


            GameObject floor;

            for (int pos = -numOfFloor*floorDist; pos <= numOfFloor*floorDist; pos += floorDist)
            {
                if (pos < 0)
                    col = Color.Lerp(Color.white, Color.red, (float)((-pos) / (float)(testfieldHeight)) * 1.6f);
                else if (pos > 0)
                    col = Color.Lerp(Color.white, GlobalSettings.focusColor, (float)((pos) / (float)(testfieldHeight)) * 1.6f);
                else col = midcol;
                col.a = floorAlpha;

                floor = createQuad(new Vector3(0, pos, 0), Quaternion.Euler(new Vector3(90, 0, 0)), col, standardFloorScale);

                col.a = 1;
                floorLevels.Add(new FloorLevel(floor, pos, col));
                // *Mathf.Min(1f, ((float)(numOfFloor * floorDist) / (float)(Mathf.Abs(pos))));
            }


            // Vertical floors simulating wind
            wallVerticalWind.Add(createQuad(new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(0, 0, 0)), new Color(0, 0, 1, 0.5f), new Vector3(standardFloorScale.x, 2 * testfieldHeight, 1), new Vector2(1, 1)));

            // Floor around best testsample
            floorOfBest = createQuad(new Vector3(0, 0, 0), Quaternion.Euler(new Vector3(90, 0, 0)), new Color(0, 1, 0, 0.85f), standardFloorScale * 0.9f);

            // Walls

            wallBack = createQuad(new Vector3(0, 0, -floorLevels[1].floor.transform.localScale.y / 2), Quaternion.Euler(new Vector3(0, 0, 0)), new Color(1, 1, 1, wallAlpha), new Vector3(standardFloorScale.x, 2 * testfieldHeight, 1), new Vector2(1, 1));
            wallFront = createQuad(new Vector3(0, 0, floorLevels[1].floor.transform.localScale.y / 2), Quaternion.Euler(new Vector3(0, 0, 0)), new Color(1, 1, 1, wallAlpha), new Vector3(standardFloorScale.x, 2 * testfieldHeight, 1), new Vector2(1, 1));
            wallLeft = createQuad(new Vector3(-standardFloorScale.x / 2, 0, 0), Quaternion.Euler(new Vector3(0, 90, 0)), new Color(1, 1, 1, wallAlpha), new Vector3(floorLevels[1].floor.transform.localScale.y, 2 * testfieldHeight, 1), new Vector2(1, 1));
            wallRight = createQuad(new Vector3(+standardFloorScale.x / 2, 0, 0), Quaternion.Euler(new Vector3(0, 90, 0)), new Color(1, 1, 1, wallAlpha), new Vector3(floorLevels[1].floor.transform.localScale.y, 2 * testfieldHeight, 1), new Vector2(1, 1));
            
        }
        else if (Global.GetCurrentScene() == Global.cliffScene)
        {
            GlobalSettings.SetPhysInterpolation(true);
        }


        Debug.Log("Environment Started");
	}


    void Start()
    {
        if (Global.GetCurrentScene() == Global.mainAIScene)
        {
            renderPhTemp = GlobalSettings.renderingBodyAndBones;
            renderMemTemp = GlobalSettings.renderingMembranes;

            ChangeTaskMode("Learning");
        }
        else
        if (Global.GetCurrentScene() == Global.cliffScene)
            {
                print("Initialisizing Cliff Scene Mode");

                PopulationControler.Init();

                // Generate a standard-phenotype for a demonstrationd ragon
                Phenotype mainDragonType = PhenotypeFactory.CreateForDemoDragon();


                // Activate some graphical effects (not used during the phenotypes for the genetic algorithm)
                mainDragonType.SetDependantBonethickness(0.75f, 0.2f); // Sets bone thicknes depending on the distance from the origin
                mainDragonType.SetDependantBonecolor(new Color(0.3f, 0.6f, 0.2f), new Color(0.5f, 0.8f, 0.4f)); // Sets bone color depending on the distance from the origin


                GameObject theDragon = GameObject.Find("TheDragon");
                GameObject dragonBody = null, dragonHead = null;


                foreach (Transform child in theDragon.transform)
                {
                    if (child.gameObject.name == "MainBody")
                        dragonBody = child.gameObject;

                    if (child.gameObject.name == "Head")
                        dragonHead = child.gameObject;
                }


                PopulationControler.population.Add(dragonBody);

                DragonFactory.AttachWing(dragonBody, mainDragonType);


                //Disable the wing movement
                foreach (HingeJoint joint in dragonBody.GetComponent<DragonScript>().wingSet.joints)
                    joint.useMotor = false;

                dragonBody.GetComponent<DragonScript>().wingSet.running = false;
                dragonBody.GetComponent<DragonScript>().index = 0;
                theDragon.GetComponent<DragonControler>().PrepareComponents();
                theDragon.GetComponent<DragonControler>().SwitchGravity(true);

                

                // Set rendering settings
                GlobalSettings.SetBodyAndBonesRendering(true);
                GlobalSettings.SetMembraneRendering(true);

                // Make the camera keep looking at the head
                Global.mainCameraScr.SetCameraFocusObject(dragonHead);

                bestSample = dragonBody;
                inLearningMode = false;
            }

    }





    GameObject createQuad(Vector3 position, Quaternion rotation, Color col, Vector3 scale)
    {
        GameObject floor = (GameObject)Instantiate(BaseQuad, position, rotation);
        floor.GetComponent<Renderer>().material.color = col;
        foreach (Transform backFace in floor.transform)
            backFace.gameObject.GetComponent<Renderer>().material.color = col;
        floor.transform.localScale = scale;

        return (floor);
    }
    GameObject createQuad(Vector3 position, Quaternion rotation, Color col, Vector3 scale, Vector2 texScale)
    {
        GameObject floor = createQuad(position, rotation, col, scale);

        floor.GetComponent<Renderer>().material.mainTextureScale = texScale;
        foreach (Transform backFace in floor.transform)
            backFace.gameObject.GetComponent<Renderer>().material.mainTextureScale = texScale;

        return (floor);
    }

    void modifyQuad(GameObject floor, Vector3 position)
    {
        floor.transform.position = position;
    }
    void modifyQuad(GameObject floor, Vector3 position, Vector3 scale)
    {
        floor.transform.position = position;
        floor.transform.localScale = scale;
    }

    void modifyQuad(GameObject floor, Vector3 position, Vector3 scale, Vector3 texScale)
    {
        floor.transform.position = position;
        floor.transform.localScale = scale;

        floor.GetComponent<Renderer>().material.mainTextureScale = texScale;
        foreach (Transform backFace in floor.transform)
            backFace.gameObject.GetComponent<Renderer>().material.mainTextureScale = texScale;
    }

    public void SetQuadRender(GameObject floor, bool render)
    {
        floor.GetComponent<Renderer>().enabled = render;
        foreach (Transform backFace in floor.transform)
            backFace.gameObject.GetComponent<Renderer>().enabled = render;
    }



    public void ResizeFloors(float scale)
    {
        foreach (FloorLevel floorLevel in floorLevels)
        {
            //if (floorLevel.level == 0)
              //  floorLevel.floor.transform.localScale = standardFloorScale * scale * 1.1f;
            //else
            floorLevel.floor.transform.localScale = standardFloorScale * scale;
            floorLevel.floor.GetComponent<Renderer>().material.mainTextureScale = standardFloorTextureScale * Mathf.Round(scale);
            foreach (Transform backFace in floorLevel.floor.transform)
                backFace.gameObject.GetComponent<Renderer>().material.mainTextureScale = standardFloorTextureScale * Mathf.Round(scale);
        }

        Vector3 currentFloorScale = floorLevels[1].floor.transform.localScale;


        foreach (GameObject windWall in wallVerticalWind)
            Destroy(windWall);
        wallVerticalWind.Clear();


        float stepPosition = (-Mathf.Round(0.5f * scale) * standardFloorScale.z * GlobalSettings.sampleCellHeight);

        for (int i = Mathf.FloorToInt(scale/2); i >= 0; i--)
        {
            wallVerticalWind.Add(createQuad(new Vector3(0, 0, stepPosition), Quaternion.Euler(new Vector3(0, 0, 0)), new Color(0, 0, 1, 0.5f), new Vector3(currentFloorScale.x, 2 * testfieldHeight, 1), new Vector2(Mathf.Round(scale), 1)));
            stepPosition += 2 * standardFloorScale.z * GlobalSettings.sampleCellHeight;
        }

        modifyQuad(wallBack, new Vector3(0, 0, -currentFloorScale.y / 2), new Vector3(currentFloorScale.x, 2 * testfieldHeight, 1), new Vector2(Mathf.Round(scale),1));
        modifyQuad(wallFront, new Vector3(0, 0, currentFloorScale.y / 2), new Vector3(currentFloorScale.x, 2 * testfieldHeight, 1), new Vector2(Mathf.Round(scale), 1));
        modifyQuad(wallLeft, new Vector3(-currentFloorScale.x / 2, 0, 0), new Vector3(currentFloorScale.y, 2 * testfieldHeight, 1), new Vector2(Mathf.Round(scale), 1));
        modifyQuad(wallRight, new Vector3(+currentFloorScale.x / 2, 0, 0), new Vector3(currentFloorScale.y, 2 * testfieldHeight, 1), new Vector2(Mathf.Round(scale), 1));    
    }


    public void ResizeFloorsFitToPopulation()
    {
        Global.envControlerScr.ResizeFloors(Mathf.Sqrt(GlobalSettings.populationSize) + ((GlobalSettings.populationSize > 1) ? 1 : 0));
        if (GlobalSettings.populationSize >= 2)
            Global.mainCameraScr.GotoWatchObjectDistance(35 + Mathf.Sqrt(GlobalSettings.populationSize) * 35);
    }





    // Frame Update
	void Update () {

        if (Global.currentScene == Global.mainAIScene)
        {
            
            // Check for best phenotype so far
            if (GlobalSettings.renderingBodyAndBones)
            {
                GameObject oldBestBody = bestSample;

                if (inLearningMode)
                {
                    bestSample = PopulationControler.GetCurrentBestSample(bestSample, bestSampleHeight);
                    if (bestSample == null)
                        bestSample = PopulationControler.GetCurrentBestSample(null, -1000);

                    if (bestSample != null)
                        if (bestSample.transform.position.y > bestSampleHeight)
                            bestSampleHeight = bestSample.transform.position.y;
                }


                // New best phenotype found
                if ((bestSample != oldBestBody) && (bestSample != null))
                {
                    if (GlobalSettings.autoSpotting)
                        Global.mainCameraScr.GotoWatchObject(bestSample);

                    floorOfBest.transform.position = bestSample.transform.position;
                    Global.GUIControlerScr.SwitchObjectToObserve(bestSample);
                    justMovedToWatchNew = true;
                }
                else
                    if (bestSample != null)
                        floorOfBest.transform.position = new Vector3(floorOfBest.transform.position.x, bestSampleHeight, floorOfBest.transform.position.z);
                
            

                // Move wind wall
                foreach (GameObject windWall in wallVerticalWind)
                {
                    windWall.transform.position -= GlobalSettings.windSpeed * Time.deltaTime;
                    if (windWall.transform.position.z < -floorLevels[1].floor.transform.localScale.y / 2)
                        windWall.transform.position = new Vector3(0, 0, floorLevels[1].floor.transform.localScale.y / 2);
                }

            }

        }



        // Scene Blending
        if (sceneChangeTimer > 0)
        {
            sceneChangeTimer -= Time.deltaTime;

            if (sceneChangeTimer <= 0.15)
            {
                if (!sceneChangeTo.Equals(""))
                    SceneManager.LoadScene(sceneChangeTo);
            }
        }


        // Scene Change
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Global.GetCurrentScene() == Global.mainAIScene)
                SwitchScene("CliffSimulation");
            else if (Global.GetCurrentScene() == Global.cliffScene)
                SwitchScene("MainScene");
        }


	}


    public void SwitchScene(string sceneName)
    {
        sceneChangeTimer = sceneChangeDuration;
        sceneChangeTo = sceneName;
    }


    // See GUI event of GUIScript
    public void DrawSceneChangeGUI()
    {
        if (sceneChangeTimer > 0)
        {
            if (sceneChangeTo.Equals(""))
                GUI.color = Color.Lerp(Color.clear, Color.black, (sceneChangeTimer * 1.25f) / sceneChangeDuration);
            else GUI.color = Color.Lerp(Color.black, Color.clear, (sceneChangeTimer * 1.25f) / sceneChangeDuration);

            GUI.DrawTexture(screenRect, fadeTexture, ScaleMode.StretchToFill);
            GUI.DrawTexture(screenRect, fadeTexture, ScaleMode.StretchToFill);
        }
    }




    public void ChangeTaskMode(string mode)
    {
        GlobalSettings.taskMode = mode;

        switch(mode)
        {
            case "Learning":
                Debug.Log("Learning Mode");

                // Destroy the full dragon and recreate population
                if (inLearningMode == false)
                {
                    Destroy(completeDragon);
                    Destroy(bestSample.gameObject);

                    completeDragon = null;

                    bestSample = null;

                    PopulationControler.AddPossibleSamples();

                    GlobalSettings.SetBodyAndBonesRendering(renderPhTemp);
                    GlobalSettings.SetMembraneRendering(renderMemTemp);

                    ResizeFloorsFitToPopulation();
                }

                renderPhTemp = GlobalSettings.renderingBodyAndBones;
                renderMemTemp = GlobalSettings.renderingMembranes;

                inLearningMode = true;

                if (bestSample == null)
                    Global.mainCameraScr.GotoWatchObject(PopulationControler.population[0]);
                else Global.mainCameraScr.GotoWatchObject(bestSample);

                Global.mainCameraScr.GotoWatchObjectDistance(35 + Mathf.Sqrt(GlobalSettings.populationSize) * 35);


                testSampleScript = null;
            break;

            case "Testing":
                if (completeDragon != null) return;

                Debug.Log("Testing Mode");

                renderPhTemp = GlobalSettings.renderingBodyAndBones;
                renderMemTemp = GlobalSettings.renderingMembranes;

                // Enable rendering of the new phenotype
                    GlobalSettings.SetBodyAndBonesRendering(true);
                    GlobalSettings.SetMembraneRendering(true);
                //

                // Save the current population for future use
                PopulationControler.SaveAndDestroyPopulation();


                // Copy the phenotype of the best dragon
                    Phenotype bestPhenotype;

                    if (bestSample == null)
                        bestPhenotype = PopulationControler.GetBestPhenotype();
                    else bestPhenotype = bestSample.GetComponent<DragonScript>().wingSet.phenotype;

                    if (bestPhenotype == null) bestPhenotype = PhenotypeFactory.CreateRandom();

                    bestPhenotype.SetDependantBonethickness(0.6f, 0.1f); // Sets bone thicknes depending on the distance from the origin
                    bestPhenotype.SetDependantBonecolor(new Color(0.3f, 1, 0.3f), new Color(0.25f, 0.25f, 1)); // Sets bone color depending on the distance from the origin

                    bestSample = DragonFactory.CreateDragon(bestPhenotype, GlobalSettings.originPoint, Quaternion.Euler(20, 0, 0), 0);
                //                    

                // Calculate optimal gravity for floating
                    float curMax;
                    if (Global.GUIControlerScr.sampleToObserve != null)
                        curMax = Mathf.Max(bestSampleHeight, Global.GUIControlerScr.sampleVisScriptToObserve.localBestHeight);
                    else curMax = bestSampleHeight;

                    customGravityOnBestSample = new Vector3(0, -(curMax / (bestPhenotype.maxFlapSteps * (GlobalSettings.requiredEvaluationFlaps + 0.5f))) * 75, 0);
                //


                ResizeFloors(1);
                inLearningMode = false;

                // Make everything visible
                GlobalSettings.SetBodyAndBonesRendering(bestSample, true);

                // Make body invisible (leaves wings visible)
                bestSample.gameObject.GetComponent<MeshRenderer>().enabled = false;

                // Set interpolation
                GlobalSettings.SetPhysInterpolation(bestSample, true);
                
                // Reenable membrane
                testSampleScript = bestSample.GetComponent<DragonScript>();
                testSampleScript.wingSet.RenderVisualisation(true);


                Global.mainCameraScr.GotoWatchObject(bestSample);
                floorOfBest.transform.position = bestSample.transform.position;
                Global.GUIControlerScr.SwitchObjectToObserve(bestSample);
                justMovedToWatchNew = true;
                Global.mainCameraScr.GotoWatchObjectDistance(70);


                // Create the new surrounding full dragon model

                completeDragon = (GameObject)Instantiate(CompleteDragonModel, bestSample.transform.position, bestSample.transform.rotation);
                completeDragon.GetComponent<DragonControler>().followerObject = bestSample;
                

            break;
        }
    }


    
    void FixedUpdate()
    {
        // This gravity  force is only active in testing mode
        if (inLearningMode == false)
            bestSample.GetComponent<Rigidbody>().AddForce(customGravityOnBestSample);
    }


}


// DATASTRUCTURE for FLOORLEVELS ///////////////////////////////

public struct FloorLevel
{
    public GameObject floor;
    public int level;
    public Color col;

    public FloorLevel(GameObject floor, int level, Color col)
    {
        this.floor = floor;
        this.level = level;
        this.col = col;
    }
}