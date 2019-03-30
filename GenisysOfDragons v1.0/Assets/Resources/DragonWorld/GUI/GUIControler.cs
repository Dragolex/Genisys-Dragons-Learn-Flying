using UnityEngine;
using System.Collections;


/*
 * NOT DIRECTLY AI RELATED
 * 
 * This large script handles the whole GUI and everything seen on the screen in 2D.
 * That includes buttons and information-displays (using instances of "TextFieldObj" with "TextFieldScript")
 * as well as the observation bar at the bottom of the screen, using a "Surface".
 */


public class GUIControler : MonoBehaviour {

    // For buttons and text
    Rect btLineRect = new Rect(0, 0, 0, 0);
    int btLineDist;
    // textfields
    TextFieldScript titleTF, worldDataTF, visualisationTF, AIsettingsTF, observationTF, observationSurfaceTF, tutorialTF, testingModeTF, coverMessageTextfield = null;

    
    // Whether the GUI functions have not been initialized yet
    bool initGUI = true;
    GUIStyle styleAlwaysActive;


    // For fps
    float currentFPS = 0;
    int currentFPSCounter = 0;
    float currentFPSSum = 0;

    // Foreign Objects (Static)
    GameObject TextFieldObj;


    // Tutorial
    float tutorialTFletterTime = 0.04f;


    // Foreign Objects (Changing)
    public GameObject sampleToObserve = null;
    public DragonVisualisation sampleVisScriptToObserve;


    // Sample observation
    Surface observationSurface;
    float observationSurfaceProgressPosition = 0;
    readonly Rect observationSurfRect = new Rect(300 - 10, Screen.height - 120 - 10, Screen.width - 300, 120);
    readonly int observationSurfScaleOffset = 20;
    float observationSurfVScale = 5;
    float observationCurrentLevitation = 0;


    // Textfield lines
    int TFautoSpottingLine;
    int shiftGUIPos = 0;


    // Other variables
    bool pressedPause = false;
    public int additionalButtonsY = 0;




    void Start () {

        if (Global.GetCurrentScene() == Global.mainAIScene)
        {
            TextFieldObj = (GameObject)Resources.Load("DragonWorld/GUI/TextFieldObj");

            // Some coordinates for the GUI
            int fy = 10;
            int fw = 400;
            int btx = 275;
            int fh = 25;
            int offs = 7;


            // Create the title textfield with displaying the FPS and running time
            titleTF = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
            titleTF.InitializeTextField(10, fy, fw, 30, offs, 4, true, Color.cyan, 23, FontStyle.Bold);

            titleTF.AddTextLine("--- GENISYS OF DRAGONS");
            titleTF.AddAutoUpdateLine("     Current FPS: ", () => { return (currentFPS.ToString()); });
            titleTF.AddAutoUpdateLine("     Total Running Time: ", () => { return (Time.realtimeSinceStartup.ToString("F2")); });
            titleTF.AddAutoUpdateLine("     Tested Samples: ", () => { return (PopulationControler.testedSamples.ToString()); });


            fy += 4*30 + 3*offs;


            // Create the Textfield with the World Data
            worldDataTF = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
            worldDataTF.InitializeTextField(10, fy, fw, fh, offs, 6);
            worldDataTF.InitializedActiveLineSettings(btx, offs, () => { Global.mainCameraScr.doubleClick = 0; });

            worldDataTF.AddTextLine("<color=#00ffffff>--- Simulation</color>     ");


            worldDataTF.AddActiveLine("     Time Scale: ", "Scale of the whole simulation speed.", "Geschwindigkeitsskalierung der gesamten Simulation.",
                                                           new string[] { "++", "--" }, new System.Action[] {()=>{Time.timeScale += 0.05f;},
                                                                                                             ()=>{Time.timeScale = Mathf.Max(0.0f, Time.timeScale - 0.05f);} },
                                                                                                             ()=>{ return (Time.timeScale.ToString("F2")); },
                                                                                                             KeyCode.T, KeyCode.UpArrow, KeyCode.DownArrow);


            worldDataTF.AddActiveLine("     Physic Speed: ", "Scale of the physics update speed.", "Geschwindigkeitsskalierung der Physik.",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{Time.fixedDeltaTime = Mathf.Max(0.0025f, Time.fixedDeltaTime - 0.001f);},
                                                                                                               ()=>{Time.fixedDeltaTime += 0.001f;} },
                                                                                                               () => { return ((1 / Time.fixedDeltaTime).ToString("F2")); },
                                                                                                               KeyCode.Z, KeyCode.UpArrow, KeyCode.DownArrow);

            /*
            worldDataTF.AddActiveLine("     Wind Speed: ", "Speed of Wind added to the simulation (the moving, blue walls).", "Geschwindigkeit des Windes in der Simulation (siehe die blauen Wände).",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{GlobalSettings.windSpeed = new Vector3(GlobalSettings.windSpeed.x, GlobalSettings.windSpeed.y, Mathf.Max(0.0f, GlobalSettings.windSpeed.z - 0.5f));},
                                                                                                               ()=>{GlobalSettings.windSpeed += new Vector3(0,0,0.5f);} },
                                                                                                               () => { return (GlobalSettings.windSpeed.z.ToString("F2")); },
                                                                                                               KeyCode.J, KeyCode.UpArrow, KeyCode.DownArrow);
            */

            worldDataTF.AddActiveLine("     Air Density: ", "Density of the air. Influences the friction of wing membranes.", "Dichte der Luft. Beeinflusst den Widerstand der Flügelmembranen.",
                                                            new string[] { "++", "--" }, new System.Action[] {()=>{GlobalSettings.AirDensity = Mathf.Min(5, GlobalSettings.AirDensity + 0.1f);},
                                                                                                              ()=>{GlobalSettings.AirDensity = Mathf.Max(0, GlobalSettings.AirDensity - 0.1f);} },
                                                                                                              () => { return (GlobalSettings.AirDensity.ToString("F2")); },
                                                                                                              KeyCode.I, KeyCode.UpArrow, KeyCode.DownArrow);


            worldDataTF.AddActiveLine("     Total muscle strength: ", "Sum of the all muscle strength within one wing-pair.\nThe value is fixed here for equality, however the strength-distribution is set by the AI!", "Summe der Muskelkraft welche einem Flügelpaar zur verfügung steht.\nDieser Wert ist Zwecks Gleichheit konstant, aber die Verteilung entscheidet die KI!",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{GlobalSettings.dragonAvailableForce = Mathf.Min(10000, GlobalSettings.dragonAvailableForce + 50);},
                                                                                                               ()=>{GlobalSettings.dragonAvailableForce = Mathf.Max(50, GlobalSettings.dragonAvailableForce-50);} },
                                                                                                               () => { return (GlobalSettings.dragonAvailableForce.ToString()); },
                                                                                                               KeyCode.S, KeyCode.UpArrow, KeyCode.DownArrow);

            worldDataTF.AddActiveLine("     Max muscle velocity: ", "Fixed maximum muscle velocity. A value required by Unity's engine.\nCould be interpreted as muscle intertia.", "Fixes Limit der Muskelgeschwindigkeit in Unity's Engine.\nKann als Muskelträgheit interpretiert werden.",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{GlobalSettings.dragonMaxMuscleVelocity = Mathf.Min(500, GlobalSettings.dragonMaxMuscleVelocity + 2);},
                                                                                                               ()=>{GlobalSettings.dragonMaxMuscleVelocity = Mathf.Max(1, GlobalSettings.dragonMaxMuscleVelocity-2);} },
                                                                                                               () => { return (GlobalSettings.dragonMaxMuscleVelocity.ToString()); },
                                                                                                               KeyCode.V, KeyCode.UpArrow, KeyCode.DownArrow);




            // Create the textfield with visualisation data and settings
            fy += 6 * fh + 3*offs;
            visualisationTF = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
            visualisationTF.InitializeTextField(10, fy, fw, fh, offs, 6);
            visualisationTF.InitializedActiveLineSettings(btx, offs, () => { Global.mainCameraScr.doubleClick = 0; });

            visualisationTF.AddTextLine("<color=#00ffffff>--- Rendering</color>     ");
            visualisationTF.AddActiveLine("     MODE: ", "Mode of the program: Learn-Simulation or presentation/testing.", "Modus des Programms: Lern-Simulation oder Vorführung bzw. Testen.",
                                                               new string[] { "Learn", "Test" }, new System.Action[] {()=>{visualisationTF.ShowTextLine(TFautoSpottingLine); AIsettingsTF.active = true; testingModeTF.active = false; Global.envControlerScr.ChangeTaskMode("Learning"); },
                                                                                                                      ()=>{visualisationTF.HideTextLine(TFautoSpottingLine); AIsettingsTF.active = false; testingModeTF.active = true; Global.envControlerScr.ChangeTaskMode("Testing");} },
                                                                                                                      () => { return (GlobalSettings.taskMode); },
                                                                                                                      KeyCode.M);

            visualisationTF.AddActiveLine("     Rendering Phenotypes: ", "Whether the samples and the environment is being rendered or not.\nDisable this to allow far larger population!", "Ob die Samples und die sonstige Umgebung gerendert wird, oder nicht.\nDeaktivieren um eine deutlich größere Population zu ermöglichen!",
                                                                         new string[] { "Yes", "No" }, new System.Action[] {()=>{GlobalSettings.SetBodyAndBonesRendering(true); visualisationTF.showAllButtons = true; worldDataTF.showAllButtons = true; AIsettingsTF.showAllButtons = true;},
                                                                                                                            ()=>{GlobalSettings.SetBodyAndBonesRendering(false); visualisationTF.showAllButtons = false; worldDataTF.showAllButtons = false; AIsettingsTF.showAllButtons = false;} },
                                                                                                                            () => { return ((GlobalSettings.renderingBodyAndBones == true) ? "Yes" : "No"); },
                                                                                                                            KeyCode.N);

            visualisationTF.AddActiveLine("     Rendering Membranes: ", "Whether the membrane of wings is being rendered or not.\nDisable this to allow larger population!", "Ob die Flügelmembranen gerendert werd, oder nicht.\nDeaktivieren um eine größere Population zu ermöglichen!",
                                                                        new string[] { "Yes", "No" }, new System.Action[] {()=>{GlobalSettings.SetMembraneRendering(true);},
                                                                                                                           ()=>{GlobalSettings.SetMembraneRendering(false);} },
                                                                                                                           () => { return ((GlobalSettings.renderingMembranes == true) ? "Yes" : "No"); },
                                                                                                                           KeyCode.B);

            visualisationTF.AddActiveLine("     Physic Interpolation: ", "Slightly improves the visualisation at cost of computation power.", "Verbessert die Darstellung etwas, auf Kosten von Leistung.",
                                                                         new string[] { "Yes", "No" }, new System.Action[] {()=>{GlobalSettings.SetPhysInterpolation(true);},
                                                                                                                            ()=>{GlobalSettings.SetPhysInterpolation(false);} },
                                                                                                                            () => { return ((GlobalSettings.physInterpolate == true) ? "Yes" : "No"); },
                                                                                                                            KeyCode.I);

            TFautoSpottingLine = visualisationTF.AddActiveLine("     Spotting Best: ", "If activated, the camera will automatically swap to focus \non the best sample of the current population.", "Sofern aktiviert, folgt die Kamera \ndem besten Sample der momentanen Population.",
                                                                                       new string[] { "Yes", "No" }, new System.Action[] {()=>{GlobalSettings.autoSpotting = true; if (Global.envControlerScr.bestSample != null) Global.mainCameraScr.GotoWatchObject(Global.envControlerScr.bestSample);},
                                                                                                                                          ()=>{GlobalSettings.autoSpotting = false;} },
                                                                                                                                          () => { return ((GlobalSettings.autoSpotting == true) ? "Yes" : "No"); },
                                                                                                                                          KeyCode.O);


            fy += 6 * fh + 3 * offs;



            // Create the textfield with AI data and settings
            AIsettingsTF = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
            AIsettingsTF.InitializeTextField(10, fy, fw, fh, offs, 6);
            AIsettingsTF.InitializedActiveLineSettings(btx, offs, () => { Global.mainCameraScr.doubleClick = 0; });

            AIsettingsTF.AddTextLine("<color=#00ffffff>--- Genectic Settings</color>     ");

            AIsettingsTF.AddActiveLine("     Population Size: ", "This number determines the speed of the whole genetic algorithm.\nAdding more samples costs much performance!", "Diese Zahl bestimmt die Fortschrittsgeschwindigkeit des gesamten Algorithmus.\nEine Erhöhung erfordert zunehmend Performance!",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{PopulationControler.ResizePopulation(GlobalSettings.populationSize + (int)(Mathf.Ceil(Mathf.Sqrt((float)GlobalSettings.populationSize))));},
                                                                                                               ()=>{PopulationControler.ResizePopulation(GlobalSettings.populationSize + (int)Mathf.Min(-1, -Mathf.Floor(Mathf.Sqrt((float)GlobalSettings.populationSize))));} },
                                                                                                               () => { return (GlobalSettings.populationSize.ToString()); },
                                                                                                               KeyCode.H, KeyCode.UpArrow, KeyCode.DownArrow);

            AIsettingsTF.AddActiveLine("     Req. flaps to evaluate: ", "How many complete beats of the wings, a sample has to perform until its evaluation.\nHigher might be more accurate.", "Anzahl der Flügelschläge, die ein Sample zur Evaluation ausführen muss.\nEin höherer Wert kann genauere Resultate bringen.",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{GlobalSettings.requiredEvaluationFlaps = Mathf.Min(20, GlobalSettings.requiredEvaluationFlaps + 1);},
                                                                                                               ()=>{GlobalSettings.requiredEvaluationFlaps = Mathf.Max(1, GlobalSettings.requiredEvaluationFlaps - 1);} },
                                                                                                               () => { return (GlobalSettings.requiredEvaluationFlaps.ToString()); },
                                                                                                               KeyCode.K, KeyCode.UpArrow, KeyCode.DownArrow);

            AIsettingsTF.AddActiveLine("     Mutation Rate: ", "Probability of a mutation when a new phenotype is computed. Attention, it is repetitive!\n50% means: 1/2 is mutated once. 1/4 is mutated twice. 1/8 is mutated three times and so on.", "Wahrscheinlichkeit für Mutationen bei der Generierung neuer Phenotypen.\nAchtung, Wert ist additiv. 50% heisst 1/2 Wahrsch. 1 Mutation. 1/4 2fach Mutation. 1/8: 3fach usw.",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{GlobalSettings.MutationRate = Mathf.Min(0.9f, GlobalSettings.MutationRate + 0.05f);},
                                                                                                               ()=>{GlobalSettings.MutationRate = Mathf.Max(0, GlobalSettings.MutationRate - 0.05f);} },
                                                                                                               () => { return ((GlobalSettings.MutationRate * 100).ToString("F0") + "%"); },
                                                                                                               KeyCode.X, KeyCode.UpArrow, KeyCode.DownArrow);
            
            AIsettingsTF.AddActiveLine("     Good/Bad Samples Ratio: ", "Probability ratio of chosing a bad sample when creating \na new phenotype. Can provide better results due to variation.", "Wahrscheinlichkeit der Auswahl eines schlechten Samples zur Generierung \ndes neuen phenotyps. Kann wegen höherer Varianz isngesamt besser sein.",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{GlobalSettings.UseBadSampleProbability = Mathf.Min(1, GlobalSettings.UseBadSampleProbability + 0.025f);},
                                                                                                               ()=>{GlobalSettings.UseBadSampleProbability = Mathf.Max(0, GlobalSettings.UseBadSampleProbability - 0.025f);} },
                                                                                                               () => { return ("1/"+GlobalSettings.UseBadSampleProbability.ToString("F3")); },
                                                                                                               KeyCode.Y, KeyCode.UpArrow, KeyCode.DownArrow);

            AIsettingsTF.AddActiveLine("     Use Bad Rules too: ", "Whether the creation of new Phenotypes based on evaluated ones\nwill take suboptimal rules into account too. Can provide better results due to variance.", "Ob die Generierung von neuen Phenotypen auf Basis von evaluierten Samples\nauch schlechte Regeln einbezieht. Kann wegen höherer Varianz besser sein.",
                                                            new string[] { "Yes", "No" }, new System.Action[] {()=>{GlobalSettings.UseNegativeRules = true; Global.AIControlerScr.LoadRules();},
                                                                                                               ()=>{GlobalSettings.UseNegativeRules = false; Global.AIControlerScr.LoadRules();} },
                                                                                                               () => { return ((GlobalSettings.UseNegativeRules == true) ? "Yes" : "No"); },
                                                                                                               KeyCode.O);


            


            // Create the textfield for the TestMode
            testingModeTF = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
            testingModeTF.InitializeTextField(10, fy, fw, fh, offs, 4);
            testingModeTF.InitializedActiveLineSettings(btx, offs, () => { Global.mainCameraScr.doubleClick = 0; });
            testingModeTF.active = false;


            testingModeTF.AddActiveLine("     Applied gravity: ", "Gravity applied to the dragon. The start value is aproximated from its performance\nto stay floating. However, not very accurate possible, so change at wish.", "Schwerkraft des Drachens. Wurde automatisch anhand der Leistung geschätzt.\num den Auftrieb auszugleichen. Leider nicht sehr genau möglich. Kann daher verändert werden.",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{Global.envControlerScr.customGravityOnBestSample = new Vector3(Global.envControlerScr.customGravityOnBestSample.x, Mathf.Min(50, Global.envControlerScr.customGravityOnBestSample.y+0.2f), Global.envControlerScr.customGravityOnBestSample.z);},
                                                                                                               ()=>{Global.envControlerScr.customGravityOnBestSample = new Vector3(Global.envControlerScr.customGravityOnBestSample.x, Mathf.Max(-50, Global.envControlerScr.customGravityOnBestSample.y-0.2f), Global.envControlerScr.customGravityOnBestSample.z);} },
                                                                                                               () => { return (Global.envControlerScr.customGravityOnBestSample.y.ToString()); },
                                                                                                               KeyCode.G, KeyCode.UpArrow, KeyCode.DownArrow);
            
            testingModeTF.AddActiveLine("     Body drag/friction: ", "Natural drag of the body. \nDefault is fairly high to improve stability of simulation.", "Bewegungswiderstand des Drachens.\nStandardmäßig relativ hoch zwecks der Stabilität der Simulation.",
                                                             new string[] { "++", "--" }, new System.Action[] {()=>{Global.envControlerScr.bestSample.GetComponent<Rigidbody>().drag = Mathf.Min(5, Global.envControlerScr.bestSample.GetComponent<Rigidbody>().drag + 0.05f);},
                                                                                                               ()=>{Global.envControlerScr.bestSample.GetComponent<Rigidbody>().drag = Mathf.Max(0, Global.envControlerScr.bestSample.GetComponent<Rigidbody>().drag - 0.05f);} },
                                                                                                               () => { return (Global.envControlerScr.bestSample.GetComponent<Rigidbody>().drag.ToString()); },
                                                                                                               KeyCode.B, KeyCode.UpArrow, KeyCode.DownArrow);

            testingModeTF.AddAutoUpdateLine("     Current horizontal speed: ", () => { return (Global.envControlerScr.bestSample.GetComponent<Rigidbody>().velocity.z.ToString("F2")); });
            testingModeTF.AddAutoUpdateLine("     Current vertical speed: ", () => { return (Global.envControlerScr.bestSample.GetComponent<Rigidbody>().velocity.y.ToString("F2")); });







            // Create textfield for observation through GUI

            observationTF = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));

            observationTF.InitializeTextField(10, (int) observationSurfRect.y+1, (int) observationSurfRect.x-29, 22, 7, 5);
            observationTF.AddTextLine("<color=#00ffffff>--- Observing Sample</color>");

            observationTF.AddAutoUpdateLine("     <color=#00ff00ff>Global Best Height:</color> ", () => { if (sampleToObserve == null) return (""); else return (Global.envControlerScr.bestSampleHeight.ToString("F4")); });
            observationTF.AddAutoUpdateLine("     <color=#ff00ffff>Current Height:</color> ", () => { if (sampleToObserve == null) return (""); else return (sampleToObserve.transform.position.y.ToString("F4")); });

            observationTF.AddAutoUpdateLine("     <color=#ffff00ff>Current Levitation:</color> ", () => { if (sampleToObserve == null) return (""); else return (observationCurrentLevitation.ToString("F2")); });
            observationTF.AddAutoUpdateLine("     <color=#ffffffff>Current Wing Area:</color> ", () => { if ((sampleToObserve == null) || (sampleVisScriptToObserve == null)) return (""); else return (sampleVisScriptToObserve.dragonScr.wingSet.GetCurrentWingArea().ToString("F2")); });


            // Create Textfield for the observation surface
            observationSurfaceTF = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
            observationSurfaceTF.InitializeTextField((int)observationSurfRect.x, (int)observationSurfRect.y, (int)observationSurfRect.width, (int)observationSurfRect.height, 0, 1);



            if (GlobalSettings.tutorialState == 0)
            {
                // Create textfield for the tutorial
                tutorialTF = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
                int li = Mathf.FloorToInt((Screen.height - 150) / 35);

                tutorialTF.InitializeTextField(430, 10, Screen.width - 454, 35, 7, li, true, Color.white, 23, FontStyle.Bold);

                // Begin with tutorial page 1
                startTutorialPage1(tutorialTF, li);
            }


            additionalButtonsY = Screen.height - 165;
        }
            else additionalButtonsY = 10;

	}
	


	// Update is called once per frame
	void Update () {
        currentFPSCounter++;
        currentFPSSum += (1.0f / Time.deltaTime);
        if (currentFPSCounter >= 15)
        {
            currentFPS = Mathf.Floor(currentFPSSum / currentFPSCounter);
            currentFPSSum = 0;
            currentFPSCounter = 0;
        }

        // tutorialState is only 100 when the tutorial has been finished. However, it contineus to increase to avoid isntantly quitting the whole program when pressing escape
        if (GlobalSettings.tutorialState >= 100)
            GlobalSettings.tutorialState++;

        // Tutorial buttons
        //Continue
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // End tutorial
                if (GlobalSettings.tutorialState == 3)
                    Destroy(tutorialTF);

                // Start page three
                if (GlobalSettings.tutorialState == 2)
                    startTutorialPage3(tutorialTF, Mathf.FloorToInt((Screen.height - 150) / 35));

                // Start page two
                if (GlobalSettings.tutorialState == 1)
                    startTutorialPage2(tutorialTF, Mathf.FloorToInt((Screen.height - 150) / 35));
            }
        // Change language
            if (Input.GetKeyDown(KeyCode.L))
            {
                GlobalSettings.tutorialState--;

                if (GlobalSettings.language.Equals("de"))
                    GlobalSettings.language = "en";
                else GlobalSettings.language = "de";

                // Start page three
                if (GlobalSettings.tutorialState == 2)
                    startTutorialPage3(tutorialTF, Mathf.FloorToInt((Screen.height - 150) / 35));

                // Start page two
                if (GlobalSettings.tutorialState == 1)
                    startTutorialPage2(tutorialTF, Mathf.FloorToInt((Screen.height - 150) / 35));

                // Start page one
                if (GlobalSettings.tutorialState == 0)
                    startTutorialPage1(tutorialTF, Mathf.FloorToInt((Screen.height - 150) / 35));
            }
        // END
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (tutorialTF != null)
                    Destroy(tutorialTF);

                GlobalSettings.tutorialState = 100;
            }




        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            return; // Currently disabled
            // Part of the videos for youtube and the media night 
            //addVideoMessageField();
        }


        if (coverMessageTextfield != null)
        {
            coverMessageTextfield.ChangePosition(new Vector2(-50, -125), 0.1f);

            worldDataTF.ChangePosition(new Vector2(10, -700), 0.02f);
            visualisationTF.ChangePosition(new Vector2(10, -700), 0.02f);
            observationTF.ChangePosition(new Vector2(Screen.width - 348, -500), 0.02f);

            shiftGUIPos += 2;
        }



        // OBSERVATION and surface drawing

        if ((sampleToObserve != null) && (sampleVisScriptToObserve != null))
        {
            if (observationSurface == null)
            {
                observationSurface = Surface.Create((int)observationSurfRect.width, (int)observationSurfRect.height);
                observationSurfaceProgressPosition = 0;
                observationSurfaceTF.active = true;
            }


            if (observationSurfaceProgressPosition == 0)
            {
                Surface.SetTarget(observationSurface);

                    Surface.Clear();
                    
                    Surface.DrawLine(0, Mathf.FloorToInt(observationSurfRect.height / 2), (int) observationSurfRect.width, Mathf.FloorToInt(observationSurfRect.height / 2), new Color(0.5f, 0.5f, 0.5f, 1));


                    float referenceHeight = Mathf.Max(Global.envControlerScr.bestSampleHeight, sampleVisScriptToObserve.localBestHeight);

                    if (Mathf.Abs(sampleVisScriptToObserve.localBestHeight) > 1)
                    {
                        observationSurfVScale = (observationSurfRect.height / 2 - observationSurfScaleOffset) / Mathf.Min(100,referenceHeight);
                        //observationSurfVScale *= 0.85f;
                    }
                    

                    float ybest = referenceHeight;
                    ybest = -ybest * observationSurfVScale + Mathf.RoundToInt(observationSurfRect.height / 2) -2;
                    Surface.DrawLine(0, (int) ybest, (int)observationSurfRect.width, (int) ybest, Color.green);

                    Surface.DrawRect(0, 0, (int) observationSurfRect.width, (int) observationSurfRect.height, Color.clear, true);

                Surface.ResetTarget();
            }


            Surface.SetTarget(observationSurface);

                // Draw height graph
                float ypos = sampleToObserve.transform.position.y;
                ypos = -ypos * observationSurfVScale + Mathf.RoundToInt(observationSurfRect.height / 2);
                Surface.DrawLine(observationSurfaceProgressPosition+1, ypos, observationSurfaceProgressPosition + 5, ypos+1, Color.magenta);

                if (sampleVisScriptToObserve != null)
                    if (sampleVisScriptToObserve.dragonScr != null)
                        observationCurrentLevitation = sampleVisScriptToObserve.dragonScr.wingSet.GetCurrentLevitation()/3;

                // Draw levitation graph
                ypos = -observationCurrentLevitation + Mathf.RoundToInt(observationSurfRect.height / 2);
                Surface.DrawLine(observationSurfaceProgressPosition+1, ypos, observationSurfaceProgressPosition + 3, ypos+1, Color.yellow);

            Surface.ResetTarget();


            observationSurfaceProgressPosition += (currentFPS/(0.5f*GlobalSettings.requiredEvaluationFlaps)) * Time.deltaTime;
            if (observationSurfaceProgressPosition > (observationSurfRect.width - 4))
            {
                observationSurfaceProgressPosition = 0;
                observationSurfVScale = (observationSurfRect.height / 2 - observationSurfScaleOffset) / Mathf.Min(100, Global.envControlerScr.bestSampleHeight);
            }
        }
	}




    void StartButtonLine(int offsx, int offsy, int sep, int height)
    {
        btLineRect.x = offsx;
        btLineRect.y = offsy;
        btLineRect.height = height;
        btLineDist = sep;
    }

    bool DrawInlineButton(int width, string str, bool sep)
    {
         return(DrawInlineButton(width, str, sep, false));
    }
    bool DrawInlineButton(int width, string str, bool sep, bool keptDown)
    {
        if (sep) btLineRect.x += btLineDist;
        btLineRect.width = width;

        bool ret;
        if (keptDown)
            ret = GUI.Button(btLineRect, str, styleAlwaysActive);
        else
        ret = GUI.Button(btLineRect, str, GUI.skin.button);

        btLineRect.x += width + btLineDist;

        if (ret) Global.mainCameraScr.doubleClick = 0; // prevent double click to trigger

        return(ret);
    }





    // Switches to a certain object to observe through the GUI
    public void SwitchObjectToObserve(GameObject obj)
    {
        if (obj == null) return;
        if (Global.currentScene == Global.cliffScene) return;

        if (sampleToObserve != obj)
            observationSurfaceProgressPosition = 0;

        sampleToObserve = obj;
        sampleVisScriptToObserve = (DragonVisualisation)obj.GetComponent(typeof(DragonVisualisation));
    }


    // Tutorial functions

    void startTutorialPage1(TextFieldScript tf, int lines)
    {
        tf.Clear();

        if (GlobalSettings.language.Equals("de"))
        {
            tf.AddTextLine("   WILLKOMMEN!", tutorialTFletterTime, 1);
            tf.AddTextLine("");

            tf.AddTextLine("   Zum Überspringen dieses Tutorials, <color=#00ff00ff>ESCAPE</color> drücken! Für Englisch: <color=#00ff00ff>L</color>.", tutorialTFletterTime, 1.5f);
            tf.AddTextLine("");
            tf.AddTextLine("");

            tf.AddTextLine("   Dieses Software Projekt stellt ein visualisiertes Anwendungsbeispiel", tutorialTFletterTime, 3);
            tf.AddTextLine("   für einen <color=#00ff00ff>genetischen Algorithmus</color> dar.", tutorialTFletterTime, 5f);
            tf.AddTextLine("   Dabei wird eine bestimmte Anzahl an Flügelpaaren, <color=#00ff00ff>samples</color> genannt", tutorialTFletterTime, 7);
            tf.AddTextLine("   (z.B. eines Vogels oder Drachens), als Teil der <color=#00ff00ff>Population</color> generiert.", tutorialTFletterTime, 9.5f);
            tf.AddTextLine("   Diese wird mittels Physik simuliert und auf Flugtauglichkeit getestet.", tutorialTFletterTime, 13f);

            tf.AddTextLine("   Anhand der Resultate, werden nach den typischen Regeln", tutorialTFletterTime, 16.5f);
            tf.AddTextLine("   von genetischen Algorithmen, der <color=#00ff00ff>Selektion</color>, <color=#00ff00ff>Kreuzung</color>, <color=#00ff00ff>Mutation</color> usw.", tutorialTFletterTime, 19f);
            tf.AddTextLine("   neue Flügelpaare generiert welche die nächste Population bilden.", tutorialTFletterTime, 22.5f);
            tf.AddTextLine("   Der Vorgang wird fortwährend bis zu einem Wunsch-Zustand <color=#00ff00ff>wiederholt</color>.", tutorialTFletterTime, 26f);
            tf.AddTextLine("");
            tf.AddTextLine("   Für mehr Informationen, lesen Sie bitte die dazugehörige <color=#00ff00ff>Dokumentation</color>.", tutorialTFletterTime, 29f);
            tf.AddTextLine("");

            for (int i = 19; i < lines; i++)
                tf.AddTextLine("");

            tf.AddTextLine("   <color=#00ff00ff>Weiter</color> --- 'STEUERUNG im Raum'  |  <color=#00ff00ff>ENTER</color> Drücken");
        }
        else
        if (GlobalSettings.language.Equals("en"))
        {
            tf.AddTextLine("   WELCOME!", tutorialTFletterTime, 1);
            tf.AddTextLine("");

            tf.AddTextLine("   To skip this tutorial, press <color=#00ff00ff>ESCAPE</color>! For German text, press <color=#00ff00ff>L</color>.", tutorialTFletterTime, 1.5f);
            tf.AddTextLine("");
            tf.AddTextLine("");

            tf.AddTextLine("   This softwareproject shows a visualized example of a <color=#00ff00ff>genetic algorithm</color>.", tutorialTFletterTime, 3);
            tf.AddTextLine("   Therefore it creates a certain number of wing-pairs as <color=#00ff00ff>samples</color>", tutorialTFletterTime, 7);
            tf.AddTextLine("   (like of a bird or dragon) which make up the so called <color=#00ff00ff>Population</color>.", tutorialTFletterTime, 9.5f);
            tf.AddTextLine("   Their physics are being simulated and examined for their airworthiness.", tutorialTFletterTime, 13f);
            tf.AddTextLine("");

            tf.AddTextLine("   The results are used with the typical rules of genetic algorithms,", tutorialTFletterTime, 16.5f);
            tf.AddTextLine("   like <color=#00ff00ff>selection</color>, <color=#00ff00ff>crossing</color>, <color=#00ff00ff>mutation</color> etc. to generate new wing-pairs,", tutorialTFletterTime, 19f);
            tf.AddTextLine("   and create a new population for the next generation this way.", tutorialTFletterTime, 22.5f);
            tf.AddTextLine("   The process is being <color=#00ff00ff>repeated</color> until a desired result is achieved.", tutorialTFletterTime, 26f);
            tf.AddTextLine("");
            tf.AddTextLine("   For more information, please read the <color=#00ff00ff>documentation</color>.", tutorialTFletterTime, 29f);
            tf.AddTextLine("");

            for (int i = 18; i < lines; i++)
                tf.AddTextLine("");

            tf.AddTextLine("   <color=#00ff00ff>Continue</color> --- 'CONTROLS: Position in space'  |  Press <color=#00ff00ff>ENTER</color>");
        }

        GlobalSettings.tutorialState++;
    }


    void startTutorialPage2(TextFieldScript tf, int lines)
    {
        tf.Clear();

        if (GlobalSettings.language.Equals("de"))
        {
            tf.AddTextLine("   STEUERUNG im Raum", tutorialTFletterTime, 0);
            tf.AddTextLine("");

            tf.AddTextLine("   Zum Überspringen dieses Tutorials, <color=#00ff00ff>ESCAPE</color> drücken! Für Englisch: <color=#00ff00ff>L</color>.", tutorialTFletterTime, 0.5f);
            tf.AddTextLine("");
            tf.AddTextLine("");

            tf.AddTextLine("   Abhängig vom Blickfeld nach Oben:      <color=#00ff00ff>W</color>-Taste", tutorialTFletterTime, 4f);
            tf.AddTextLine("   Abhängig vom Blickfeld nach Links:      <color=#00ff00ff>A</color>-Taste", tutorialTFletterTime, 4f);
            tf.AddTextLine("   Abhängig vom Blickfeld nach Unten:     <color=#00ff00ff>S</color>-Taste", tutorialTFletterTime, 4f);
            tf.AddTextLine("   Abhängig vom Blickfeld nach Rechts:   <color=#00ff00ff>D</color>-Taste", tutorialTFletterTime, 4f);
            tf.AddTextLine("");

            tf.AddTextLine("   Entlang des Blickfelds vorwärts:     <color=#00ff00ff>E</color>-Taste oder Scrollrad", tutorialTFletterTime, 9f);
            tf.AddTextLine("   Entlang des Blickfelds rückwärts:   <color=#00ff00ff>Q</color>-Taste oder Scrollrad", tutorialTFletterTime, 9f);

            for (int i = 13; i < lines; i++)
                tf.AddTextLine("");

            tf.AddTextLine("   <color=#00ff00ff>Weiter</color> --- 'STEUERUNG des Blickfelds'  |  <color=#00ff00ff>ENTER</color> Drücken");
        }
        else
        if (GlobalSettings.language.Equals("en"))
        {
            tf.AddTextLine("   CONTROLS: Position in space", tutorialTFletterTime, 0);
            tf.AddTextLine("");

            tf.AddTextLine("   To skip this tutorial, press <color=#00ff00ff>ESCAPE</color>! For German text, press <color=#00ff00ff>L</color>.", tutorialTFletterTime, 0.5f);
            tf.AddTextLine("");
            tf.AddTextLine("");

            tf.AddTextLine("   Depending on camera direction move up:        <color=#00ff00ff>W</color>-Key.", tutorialTFletterTime, 4f);
            tf.AddTextLine("   Depending on camera direction move left:       <color=#00ff00ff>A</color>-Key.", tutorialTFletterTime, 4f);
            tf.AddTextLine("   Depending on camera direction move down:   <color=#00ff00ff>S</color>-Key.", tutorialTFletterTime, 4f);
            tf.AddTextLine("   Depending on camera direction move right:     <color=#00ff00ff>D</color>-Key.", tutorialTFletterTime, 4f);
            tf.AddTextLine("");

            tf.AddTextLine("   Forwards along camera direction:      <color=#00ff00ff>E</color>-Key or scrolling wheel.", tutorialTFletterTime, 9f);
            tf.AddTextLine("   Backwards along camera direction:   <color=#00ff00ff>Q</color>-Key or scrolling wheel.", tutorialTFletterTime, 9f);

            for (int i = 13; i < lines; i++)
                tf.AddTextLine("");

            tf.AddTextLine("   <color=#00ff00ff>Continue</color> --- 'CONTROLS: Camera direction'  |  Press <color=#00ff00ff>ENTER</color>");
        }

        GlobalSettings.tutorialState++;
    }


    void startTutorialPage3(TextFieldScript tf, int lines)
    {
        tf.Clear();

        if (GlobalSettings.language.Equals("de"))
        {
            tf.AddTextLine("   STEUERUNG des Blickfelds", tutorialTFletterTime, 0);
            tf.AddTextLine("");

            tf.AddTextLine("   Zum Überspringen dieses Tutorials, <color=#00ff00ff>ESCAPE</color> drücken! Für Englisch: <color=#00ff00ff>L</color>.",tutorialTFletterTime, 0.5f);
            tf.AddTextLine("");
            tf.AddTextLine("");

            tf.AddTextLine("   Um die manuelle Steuerung des Sichtfelds zu aktivieren:", tutorialTFletterTime, 4f);
            tf.AddTextLine("   Außerhalb von Buttons, mit der Maus <color=#00ff00ff>Doppelt Klicken</color>.", tutorialTFletterTime, 7f);
            tf.AddTextLine("   Bewegung ist weiterhin mittels <color=#00ff00ff>WASD</color> möglich.", tutorialTFletterTime, 10f);
            tf.AddTextLine("   Steuermodus durch <color=#00ff00ff>Doppel klick</color> oder <color=#00ff00ff>Escape</color> beenden.", tutorialTFletterTime, 13f);
            tf.AddTextLine("");

            tf.AddTextLine("   Flügel-Knochen und Körper verändern die Farbe beim Zeigen mit der <color=#00ff00ff>Maus</color>.", tutorialTFletterTime, 16f);
            tf.AddTextLine("   Durch Festhalten der <color=#00ff00ff>Linken Maustaste</color>, können diese Elemente bewegt werden.", tutorialTFletterTime, 20f);
            tf.AddTextLine("");
            tf.AddTextLine("   Hinweis: Nach dem Ändern mancher Werte links, ist ein <color=#00ff00ff>Restart</color> empfehlenswert!", tutorialTFletterTime, 24f);
            tf.AddTextLine("");
            tf.AddTextLine("   Kurzes Klicken auf den Körper von 'Samples' erlaubt deren Beobachtung.", tutorialTFletterTime, 26.5f);

            for (int i = 17; i < lines; i++)
                tf.AddTextLine("");

            tf.AddTextLine("   <color=#00ff00ff>Ende</color>  |  <color=#00ff00ff>ENTER</color> Drücken");
        }
        else
        if (GlobalSettings.language.Equals("en"))
        {
            tf.AddTextLine("   CONTROLS: Camera direction", tutorialTFletterTime, 0);
            tf.AddTextLine("");

            tf.AddTextLine("   To skip this tutorial, press <color=#00ff00ff>ESCAPE</color>! For German text, press <color=#00ff00ff>L</color>.", tutorialTFletterTime, 0.5f);
            tf.AddTextLine("");
            tf.AddTextLine("");

            tf.AddTextLine("   To activate the manual camera control: <color=#00ff00ff>doubleclick</color> mouse outside buttons.", tutorialTFletterTime, 4f);
            tf.AddTextLine("   Movement still via <color=#00ff00ff>WASD</color> possible.", tutorialTFletterTime, 7f);
            tf.AddTextLine("   End manual camera control by <color=#00ff00ff>doubleclick</color> again or <color=#00ff00ff>escape</color>.", tutorialTFletterTime, 9f);
            tf.AddTextLine("");

            tf.AddTextLine("   Wing-Bones and the body change color under the cursor (in manual mode).", tutorialTFletterTime, 13f);
            tf.AddTextLine("   By keeping the <color=#00ff00ff>left mousebutton</color> down, one can drag those elements.", tutorialTFletterTime, 15.5f);
            tf.AddTextLine("");
            tf.AddTextLine("   Note, when changing some of the settings on the left, a <color=#00ff00ff>restart</color> is recommended.", tutorialTFletterTime, 19f);
            tf.AddTextLine("   For changing numeric values by <color=#00ff00ff>shortcut</color>, press the displayed letter and an arrow key.", tutorialTFletterTime, 23f);
            tf.AddTextLine("   Quick click onto a body allows the 'observation' of that test sample.", tutorialTFletterTime, 27f);

            for (int i = 17; i < lines; i++)
                tf.AddTextLine("");

            tf.AddTextLine("   <color=#00ff00ff>End</color>  |  Press <color=#00ff00ff>ENTER</color>");
        }

        GlobalSettings.tutorialState++;
    }


    // For the video trailers

    void addVideoMessageField()
    {
        float speed = 0.035f;

        coverMessageTextfield = (TextFieldScript)Instantiate(TextFieldObj).GetComponent(typeof(TextFieldScript));
        coverMessageTextfield.InitializeTextField(-50, -125 + 2000, Screen.width + 100, 70, 10, 25, false, new Color(1f, 1f, 1), 50, FontStyle.Bold);


        // For Youtube videos
        if (GlobalSettings.language == "de") // 
        {
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("             Genisys Of Dragons", speed, 0.5f);
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("             Willst du mehr erfahren und dem kleinen Drachen helfen?", speed, 1.25f);
            coverMessageTextfield.AddTextLine("             Dann warte ab - Dieses Software Projekt ist in Kürze Open Source!", speed, 3f);
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("             Vielen Dank!", speed, 5f);
        }
        else
        {
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("             Genisys Of Dragons", speed, 0.5f);
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("             Do you want to see more and help the little dragon too?", speed, 1.25f);
            coverMessageTextfield.AddTextLine("             Then stay tuned - This software project will be Open Source soon!", speed, 3f);
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("");
            coverMessageTextfield.AddTextLine("             Thank you for watching!", speed, 5f);
        }

        /* // Medianight VIDEO
        coverMessageTextfield.AddTextLine("");
        coverMessageTextfield.AddTextLine("");
        coverMessageTextfield.AddTextLine("");
        coverMessageTextfield.AddTextLine("             Genisys Of Dragons", speed, 0.25f);
        coverMessageTextfield.AddTextLine("");
        coverMessageTextfield.AddTextLine("");
        coverMessageTextfield.AddTextLine("");
        coverMessageTextfield.AddTextLine("             Willst du mehr erfahren und dem Drachen helfen?", speed, 0.7f);
        coverMessageTextfield.AddTextLine("             Dann komm zu dem Stand über künstliche Inteligenz!", speed, 1.5f);
        coverMessageTextfield.AddTextLine("");
        coverMessageTextfield.AddTextLine("             Standord:  Am Grünen Ei", speed, 2f);
        coverMessageTextfield.AddTextLine("             Entwickler:  Alexander Georgescu", speed, 2f);


        coverMessageTextfield.AddTextLine("");
        coverMessageTextfield.AddTextLine("             Vielen Dank!", speed, 2.5f);
        */
    }


    // GUI Draw
    void OnGUI()
    {
        if (initGUI)
        {
            // Modified GUI Styles

            GUI.skin.button.fontSize = 18;
            GUI.skin.button.fontStyle = FontStyle.Bold;

            GUI.skin.label.fontSize = 15;
            GUI.skin.label.fontStyle = FontStyle.Bold;


            styleAlwaysActive = new GUIStyle(GUI.skin.button);

            styleAlwaysActive.normal = styleAlwaysActive.hover;
            styleAlwaysActive.hover = styleAlwaysActive.hover;

            initGUI = false;
        }

        
        if ((GUI.Button(new Rect(10, additionalButtonsY, 133, 30), "Restart (R)")) || (Input.GetKeyDown(KeyCode.R)))
        {
            Global.mainCameraScr.doubleClick = 0;
            PopulationControler.Init();
            Global.envControlerScr.SwitchScene("MainScene");
        }

        if ((GUI.Button(new Rect(129 + 21, additionalButtonsY, 133, 30), "Quit (Esc)")) || ((Input.GetKeyDown(KeyCode.Escape)) && (GlobalSettings.tutorialState >= 110)))
        {
            Global.mainCameraScr.doubleClick = 0;
            Application.Quit();

            Global.ShowMessage("");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!pressedPause)
            {
                if (Time.timeScale != 0) Time.timeScale = 0;
                else Time.timeScale = 1.5f;

                pressedPause = true;
            }
        }
        if (pressedPause)
        if (Input.GetKeyUp(KeyCode.P))
            pressedPause = false;


        if ((GUI.Button(new Rect(264 + 26, additionalButtonsY, 133, 30), "Cliffscene (F)")) || (Input.GetKeyDown(KeyCode.F)))
        {
            if (Global.GetCurrentScene() == Global.mainAIScene)
                Global.envControlerScr.SwitchScene("CliffSimulation");
            else if (Global.GetCurrentScene() == Global.cliffScene)
                Global.envControlerScr.SwitchScene("MainScene");
        }


        if (observationSurface != null)
            Surface.Draw(observationSurface, observationSurfRect.x, observationSurfRect.y);

        Global.envControlerScr.DrawSceneChangeGUI();
    }

}
