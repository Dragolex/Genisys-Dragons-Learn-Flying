using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * The GlobalSettings holds a lot of globally available, static variables.
 * 
 * It provides some functions to change important settings which requrie sideeffects.
 * Most settings are changed at runtime through the "GUIScript" though.
 */


public class GlobalSettings : MonoBehaviour {

    // Main variable determinating whether phenotypes are currently drawn or not
    static public bool renderingBodyAndBones { get; private set; }

    // Whether physic-object-rendering works with interpolation (slower but smoother when rendered)
    static public bool physInterpolate { get; private set; }

    // Whether wing mebranes are being rendered or not
    static public bool renderingMembranes { get; private set; }
    // The number of membranes is limited because tehy cannot be efficiently removed and readded to the memmory
    static public int maxRenderedMembranes = 200;

    // Point of origin for the whole scene
    static public Vector3 originPoint { get; private set; }


    // The number of phenotypes to be tested currently
    static public int populationSize = 0;

    // The number of phenotypes at the start
    static public int startPopulationSize = 12;



    // Mode of operation
    static public string taskMode = "Learning"; // or "Testing"

    // Language for the tutorial and when taking the videos
    static public string language = "de"; // or "en";



    static public Color attractionColor = new Color(0, 0.75f, 1, 1);
    static public Color focusColor = new Color(0, 1, 0, 1);




    // SIMULATION RELATED ///////////////////////

    static public int requiredEvaluationFlaps = 3; // How many flaps a sample has to perform until its evaluated
    static public float AirDensity = 1f; // Force factor the pushed air provides when a wing moves
    static public Vector3 windSpeed = new Vector3(0, 0, 10); // Wind currently disabled
    static public readonly float standardFixedPhysicDeltaTime = 1f/75f; // Ammount of time a physics-update-step takes
    static public float dragonAvailableForce = 1500; // Total of strength a dragon has. This is a fixed value as it results in fairness between all samples. 
    static public int dragonMaxMuscleVelocity = 75; // maximum speed of wing movement (prevents the break of the simulation)
    
    static public sbyte[] standardWingDesignIndices;
    static public sbyte[] standardWingDesignInverting;


    // AI RELATED ///////////////////////

    static public float MutationRate = 0.4f;
    static public bool UseNegativeRules = true;
    static public float UseBadSampleProbability = 0.025f;
    static public float RandomVariance = 0.3f;
    static public float destructionMoveLimit = 20;
    static public int phenotypeLife = 10;



    // Other settings

    static public int sampleCellWidth = 50;
    static public int sampleCellHeight = 20;

    static public bool autoSpotting = false;
    static public string Bar { get; private set; }

    static public int tutorialState = 0; // Current tutorial step






    static public void Init()
    {
        populationSize = 0;

        renderingBodyAndBones = true;
        physInterpolate = false;
        renderingMembranes = true;

        GameObject basePoint = GameObject.Find("Basepoint");
        if (basePoint != null)
            originPoint = basePoint.transform.position;
        else originPoint = Vector3.zero;


        // Initializing physik values
        Time.timeScale = 1.5f;
        Time.fixedDeltaTime = standardFixedPhysicDeltaTime;




        // The following data requries some explaining...
        // Initially, the phenotypes were meant not to have a predefined number and arangement of chromosomes but be completly random.
        // That would have allowed wings with multiple sets of multi-jointts, spreading out like the branches of a tree for example.
        // The DragonFactory actually supports that without problems and the AI would work too - however, the creation of branch-like wings would require
        // a method to form triangles between those bones without leaving gaps and without overlapping.
        // Unfortunately that turned out to be a lot harder than I expected and I had to give up.
        // Therefore the following hard-coded set of indices tells the WingSetFactory and the WingSet how to connect the number of wing-bones into a set of polygons for achieving a perfect wing.
        //
        // This was a slight drawback for the diversity of the whole system. However, this also has a tad of natural background.
        // Complex Elements like multijoints for additional fingers, etc. happen extremly rare in nature. Hence why about all mammals, from pigs to whales have a similar number of bones in their limbs.


        List<sbyte> indices = new List<sbyte>();

        indices.Add(0); // Root
        indices.Add(1);
        indices.Add(2);

        indices.Add(-1);


        indices.Add(1);
        indices.Add(12);
        indices.Add(2);
        indices.Add(11);
        indices.Add(3);

        indices.Add(-1);


        indices.Add(2);
        indices.Add(3);
        indices.Add(4);

        indices.Add(-1);


        indices.Add(3); // Root
        indices.Add(11);
        indices.Add(8);
        indices.Add(12);
        indices.Add(9);
        indices.Add(12);
        indices.Add(10);

        indices.Add(-1);


        indices.Add(3); // Root
        indices.Add(8);
        indices.Add(5);
        indices.Add(9);
        indices.Add(6);
        indices.Add(10);
        indices.Add(7);

        indices.Add(-1);


        standardWingDesignIndices = indices.ToArray();


        standardWingDesignInverting = new sbyte[indices.Count];
        for (int i = 0; i < indices.Count; i++)
            standardWingDesignInverting[i] = 1;
        
        standardWingDesignInverting[2] = -1;
        standardWingDesignInverting[6] = -1;
        standardWingDesignInverting[11] = -1;
        standardWingDesignInverting[13] = -1;
        standardWingDesignInverting[15] = -1;
        standardWingDesignInverting[16] = -1;
        standardWingDesignInverting[18] = -1;
        standardWingDesignInverting[19] = -1;
        standardWingDesignInverting[20] = -1;
        standardWingDesignInverting[22] = -1;
        standardWingDesignInverting[24] = -1;
        standardWingDesignInverting[25] = -1;
        standardWingDesignInverting[27] = -1;
        standardWingDesignInverting[29] = -1;
    }



    // Global rendering setting function
    static public void SetBodyAndBonesRendering(bool render)
    {
        GlobalSettings.renderingBodyAndBones = render;

        if (Global.envControlerScr.floorLevels.Count != 0)
        {
            foreach (FloorLevel floor in Global.envControlerScr.floorLevels)
                Global.envControlerScr.SetQuadRender(floor.floor, render);

            foreach (GameObject wall in Global.envControlerScr.wallVerticalWind)
                Global.envControlerScr.SetQuadRender(wall, render);

            Global.envControlerScr.SetQuadRender(Global.envControlerScr.floorOfBest, render);

            Global.envControlerScr.SetQuadRender(Global.envControlerScr.wallBack, render);
            Global.envControlerScr.SetQuadRender(Global.envControlerScr.wallFront, render);
            Global.envControlerScr.SetQuadRender(Global.envControlerScr.wallLeft, render);
            Global.envControlerScr.SetQuadRender(Global.envControlerScr.wallRight, render);
        }


        foreach (GameObject body in PopulationControler.population)
            SetBodyAndBonesRendering(body, render);

        if (!render)
            Global.envControlerScr.bestSample = null;
    }


    // Renderings etting function for a certain object
    static public void SetBodyAndBonesRendering(GameObject body, bool render)
    {
        // Set render for main body
        body.GetComponent<Renderer>().enabled = render;
        body.GetComponent<MeshRenderer>().enabled = render;


        // Set render for wing bones
        DragonScript scr = body.gameObject.GetComponent<DragonScript>();
            foreach (Rigidbody obj in scr.wingSet.segments)
            {
                if (obj != null)
                {
                    if (render && GlobalSettings.physInterpolate)
                        obj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                    else obj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;

                    obj.GetComponent<Renderer>().enabled = render;
                    obj.GetComponent<MeshRenderer>().enabled = render;
                }
            }


        // Attach DragonVisualisation Script
        if (render)
        { if (body.GetComponent<DragonVisualisation>() == null) body.AddComponent<DragonVisualisation>(); }
        else if (body.GetComponent<DragonVisualisation>() != null) Destroy(body.GetComponent<DragonVisualisation>());
    }




    static public void SetPhysInterpolation(bool interpolate)
    {
        GlobalSettings.physInterpolate = interpolate;

        foreach (GameObject body in PopulationControler.population)
            SetPhysInterpolation(body, interpolate);
    }

    static public void SetPhysInterpolation(GameObject body, bool interpolate)
    {
        if (GlobalSettings.renderingBodyAndBones && interpolate)
            body.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        else body.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;

        DragonScript scr = body.gameObject.GetComponent<DragonScript>();
        foreach (Rigidbody obj in scr.wingSet.segments)
        {
            if (obj != null)
            {
                if (GlobalSettings.renderingBodyAndBones && interpolate)
                    obj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
                else obj.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
            }
        }
    }



    static public void SetMembraneRendering(bool rendering)
    {
        GlobalSettings.renderingMembranes = rendering;

        foreach (GameObject body in PopulationControler.population)
            SetMembraneRendering(body, rendering);
    }
    static public void SetMembraneRendering(GameObject body, bool rendering)
    {
        body.gameObject.GetComponent<DragonScript>().wingSet.RenderVisualisation(rendering);
    }

}
