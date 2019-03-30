using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/*
 * NOT DIRECTLY AI RELATED
 * 
 * This class is implementing textboxes (used for the trailer videos as well as the GUI and tutorial during simulation.)
 * 
 * Each line of the textbox consists of multiple, easy accessible strings allowing for quick replacement
 * Additionally there are self-updating lines and the user of this class only needs to provide "System.Funcs" or "System.Actions" for that.
 * This is the way one can provide Code inside a variable or argument in C#.
 * The system also allows keybaord shortcuts and buttons.
 * 
 * Eventually the result is drawn (including buttons) through Unitys native GUI functions.
 * 
 * Look into the "GUIControler" for the usage.
 * 
 * The whole class has been written by myself (though with the purpose of reusing it some day).
 * Will also be documentated better in the future.
 * */



public class TextFieldScript : MonoBehaviour {

    // Whether this textfield is active (showing) or not
    public bool active = true;

    // List of strings showing all currently displayed lines so they don't have to be computed every step
    List<string> linesDisplayed = new List<string>();

    // Actual list of data
    List<List<string>> linesStructure = new List<List<string>>();

    // List of text rectangles for drawing the text on the screen
    List<Rect> linesRectangles = new List<Rect>();
    Rect startRect;
    Rect surroundingRect;


    // The line where the mouse is in (-1 if none)
    public int mouseInLine = -1;
    public Rect mouseInRect;

    // Background rectangle styles
    private GUIStyle backgroundStyle;
    private GUIStyle labelTextStyle;


    // Whether the GUI functions have not been initialized yet
    bool initGUI = true;


    // Font and text defaults
    bool richtText = true;
    Color textCol = Color.white;
    int fontSize = 15;
    FontStyle fontStyle = FontStyle.Bold; //.Normal;


    float reEvaluationTimer = 0;


    // Lists for special lines
    List<TimedLine> timedLines = new List<TimedLine>();
    List<ActiveLine> activeLines = new List<ActiveLine>();
    List<AutoUpdateElement> autoUpdateElements = new List<AutoUpdateElement>();


    // action sample
    System.Action activeTriggerAction = ()=>{};

    // Button related
    bool buttonUpdate = true;
    public bool showAllButtons = true;
    Rect tempButtonRect;
    int activeButtonMinX;
    int activeButtonOffset;



    /// /// /// Init functions /// /// ///

    public void InitializeTextField(int x, int y, int line_width, int line_height, int padding, int lines)
    {
        startRect = new Rect(x + padding, y + padding, line_width, line_height);
        surroundingRect = new Rect(x, y, line_width + padding*2, line_height * lines + padding*2);
    }

    public void InitializeTextField(int x, int y, int line_width, int line_height, int padding, int lines, bool richtText, Color textCol, int fontSize, FontStyle fontStyle)
    {
        InitializeTextField(x, y, line_width, line_height, padding, lines);
        this.richtText = richtText;
        this.textCol = textCol;
        this.fontSize = fontSize;
        this.fontStyle = fontStyle;
    }


    public void ChangePosition(Vector2 vec)
    {
        shiftPosition(vec.x - surroundingRect.x, vec.y - surroundingRect.y);
    }
    public void ChangePosition(Vector2 vec, float fac)
    {
        shiftPosition((vec.x - surroundingRect.x) * fac, (vec.y - surroundingRect.y) * fac);
    }

    private void shiftPosition(float relx, float rely)
    {
        startRect.x += relx;
        startRect.y += rely;
        surroundingRect.x += relx;
        surroundingRect.y += rely;

        for (int ind = 0; ind < linesRectangles.Count; ind++)
            linesRectangles[ind] = new Rect(linesRectangles[ind].x + relx, linesRectangles[ind].y + rely, linesRectangles[ind].width, linesRectangles[ind].height);
    }


    /// /// /// Text functions /// /// ///

    public int AddTextLine(string str)
    {
        int line = addTextLine(str);
        recomputeTextLine(line);
        return (line);
    }

    // Timed text displaying
    public int AddTextLine(string str, float steplength)
    {
        int line = addTextLine(str);

        timedLines.Add(new TimedLine(line, 0, 0, steplength));
        recomputeTextLine(line, 0);

        return (line);
    }

    // Timed text with a start delay
    public int AddTextLine(string str, float steplength, float delay)
    {
        int line = addTextLine(str);

        timedLines.Add(new TimedLine(line, 0, delay, steplength));
        recomputeTextLine(line, 0);

        return (line);
    }



    // Add another element to a line
    public int AddTextElement(int line, string str)
    {
        linesStructure[line].Add(str);

        recomputeTextLine(line);

        return (linesStructure[line].Count-1);
    }

    // Insert line inbetween
    public int InsertTextLine(int line, string str)
    {

        linesStructure.Insert(line, new List<string>());
        linesStructure[line].Add(str);

        linesDisplayed.Insert(line, "");
        linesRectangles.Add(new Rect(startRect.x, startRect.y + (linesRectangles.Count-1) * startRect.height, startRect.width, startRect.height));
        recomputeTextLine(line);

        return (line);
    }

    // Remove
    public int RemoveTextLine(int line)
    {
        linesDisplayed.RemoveAt(line);
        linesStructure.RemoveAt(line);
        linesRectangles.RemoveAt(linesRectangles.Count-1);

        return (linesDisplayed.Count-1);
    }

    // Replace
    public void ReplaceTextLine(int line, string str)
    {
        linesStructure[line].Clear();
        linesStructure[line] = new List<string>();
        linesStructure[line].Add(str);

        recomputeTextLine(line);
    }

    // Replace element
    public void ReplaceTextLineElement(int line, int el, string str)
    {
        linesStructure[line][el] = str;

        recomputeTextLine(line);
    }

    public void HideTextLine(int line)
    {
        if (linesRectangles[line].y > -5000)
            linesRectangles[line] = new Rect(linesRectangles[line].x, linesRectangles[line].y-10000, linesRectangles[line].width, linesRectangles[line].height);
    }
    public void ShowTextLine(int line)
    {
        if (linesRectangles[line].y < -5000)
            linesRectangles[line] = new Rect(linesRectangles[line].x, linesRectangles[line].y + 10000, linesRectangles[line].width, linesRectangles[line].height);
    }

    public int Clear()
    {
        linesDisplayed.Clear();
        linesStructure.Clear();
        linesRectangles.Clear();
        timedLines.Clear();

        return (linesDisplayed.Count);
    }



    public void InitializedActiveLineSettings(int minX, int buttonOffset)
    {
        tempButtonRect = new Rect(minX, 0, 0, startRect.height-2);
        activeButtonMinX = minX;
        activeButtonOffset = buttonOffset;
    }
    public void InitializedActiveLineSettings(int minX, int buttonOffset, System.Action triggerAction)
    {
        InitializedActiveLineSettings(minX, buttonOffset);
        activeTriggerAction = triggerAction;
    }


    // Self updating lines
    public int AddAutoUpdateLine(string str, System.Func<string> evaluation)
    {
        return(AddAutoUpdateLine(str, evaluation, false));
    }

    public int AddAutoUpdateLine(string str, System.Func<string> evaluation, bool buttonTriggered)
    {
        int el, line = AddTextLine(str);

        if (active)
            el = AddTextElement(line, evaluation());
        else
            el = AddTextElement(line, "");

        autoUpdateElements.Add(new AutoUpdateElement(line, el, evaluation, buttonTriggered));

        return (line);
    }


    // Active/interactive lines
    public int AddActiveLine(string str, string[] buttonStrings, System.Action[] buttonActions, System.Func<string> evaluation, params KeyCode[] keys)
    {
        return AddActiveLine(str, "", "", buttonStrings, buttonActions, evaluation, 0, keys);
    }
    public int AddActiveLine(string str, string toolTipEN, string toolTipDE, string[] buttonStrings, System.Action[] buttonActions, System.Func<string> evaluation, params KeyCode[] keys)
    {
        return AddActiveLine(str, toolTipEN, toolTipDE, buttonStrings, buttonActions, evaluation, 0, keys);
    }

    public int AddActiveLine(string str, string toolTipEN, string toolTipDE, string[] buttonStrings, System.Action[] buttonActions, System.Func<string> evaluation, int startedPos, params KeyCode[] keys)
    {
        int el, line = AddTextLine(str);

        if (active)
            el = AddTextElement(line, evaluation());
        else
            el = AddTextElement(line, "");


        int[] buttonWidth = new int[buttonStrings.Length];
        int w = (int) Mathf.Ceil((surroundingRect.width-tempButtonRect.x + 7) / buttonWidth.Length);

        for (int i = 0; i < buttonWidth.Length; i++)
            buttonWidth[i] = w;

        if (keys.Length > 0)
        {
            AddTextElement(line, " | "+keys[0].ToString());
        }

        foreach(ActiveLine al in activeLines)
            if (Enumerable.SequenceEqual(al.keys, keys))
                 Global.ShowMessage("Error! The same shortcut letter tries to be added twice: " + keys[0].ToString());


        activeLines.Add(new ActiveLine(line, buttonActions, buttonStrings, buttonWidth, startedPos, keys, toolTipEN, toolTipDE));
        autoUpdateElements.Add(new AutoUpdateElement(line, el, evaluation, true));


        return (line);
    }





    // INTERNAL

    private int addTextLine(string str)
    {
        int line = linesStructure.Count;
        linesStructure.Add(new List<string>());
        linesStructure[line].Add(str);

        return (line);
    }

    private string recomputeTextLine(int line)
    {
        string res = "";
        foreach (string str in linesStructure[line])
        {
            res += str;
        }

        if (line > (linesDisplayed.Count - 1))
        {
            // New line
            linesDisplayed.Add(res);
            linesRectangles.Add(new Rect(startRect.x, startRect.y + line * startRect.height, startRect.width, startRect.height));
        }
        else linesDisplayed[line] = res;

        return(res);
    }

    private bool recomputeTextLine(int line, int limit)
    {
        recomputeTextLine(line);
        string str = linesDisplayed[line];
        linesDisplayed[line] = linesDisplayed[line].Substring(0, limit);
        return(!str.Equals(linesDisplayed[line]));
    }




    void Update()
    {
        // Timed Lines

        foreach (TimedLine tl in timedLines.ToArray())
            if (tl.HandleCounter(Time.deltaTime))
            {
                if (linesStructure[tl.line][0].Length > tl.position)
                if (linesStructure[tl.line][0][tl.position].ToString().Equals("<"))
                {
                    while(!linesStructure[tl.line][0][tl.position].ToString().Equals(">"))
                        tl.position++;
                    tl.position++;
                    while (!linesStructure[tl.line][0][tl.position].ToString().Equals(">"))
                        tl.position++;
                    tl.position++;
                }

                if (!recomputeTextLine(tl.line, tl.position))
                    timedLines.Remove(tl);
            }


        // Auto Update Elements

        if (active)
        {
            if (reEvaluationTimer <= 0)
            {
                reEvaluationTimer = 0.1f;

                foreach (AutoUpdateElement ael in autoUpdateElements)
                    if ((!ael.buttonTriggered) || buttonUpdate)
                        ReplaceTextLineElement(ael.line, ael.element, ael.evaluation());

                buttonUpdate = false;
            }
            else reEvaluationTimer -= Time.deltaTime;



            foreach (ActiveLine al in activeLines)
            {
                if (al.keys.Length >= 1)
                {
                    if (al.keys.Length == 1)
                    {
                        if (Input.GetKeyDown(al.keys[0]))
                        {
                            al.buttonActions[(al.startedPos == 0) ? 1 : 0]();
                            reEvaluationTimer = 0;
                            buttonUpdate = true;
                            activeTriggerAction();

                            al.startedPos = (al.startedPos == 0) ? 1 : 0;
                        }
                    }
                    else if (al.keys.Length == al.buttonActions.Length)
                    {
                        for(int b = 0; b < al.keys.Length; b++)
                            if (Input.GetKeyDown(al.keys[b]))
                            {
                                al.buttonActions[b]();
                                reEvaluationTimer = 0;
                                buttonUpdate = true;
                                activeTriggerAction();

                                al.startedPos = (al.startedPos == 0) ? 1 : 0;
                            }
                    }
                    else if (al.keys.Length == al.buttonActions.Length+1)
                    {
                        if (Input.GetKey(al.keys[0]))
                            for (int b = 0; b < al.buttonActions.Length; b++)
                                if (Input.GetKeyDown(al.keys[b+1]))
                                {
                                    al.buttonActions[b]();
                                    reEvaluationTimer = 0;
                                    buttonUpdate = true;
                                    activeTriggerAction();

                                    al.startedPos = (al.startedPos == 0) ? 1 : 0;
                                }
                    }
                }

            }
        }

    }



    // GUI Draw
    void OnGUI()
    {
        mouseInLine = -1;

        if (initGUI)
        {
            backgroundStyle = new GUIStyle(GUI.skin.button);

            backgroundStyle.normal = backgroundStyle.active;
            backgroundStyle.hover = backgroundStyle.active;
            
            labelTextStyle = new GUIStyle(GUI.skin.label);
            labelTextStyle.richText = richtText;
            labelTextStyle.normal.textColor = textCol;
            labelTextStyle.fontSize = fontSize;
            labelTextStyle.fontStyle = fontStyle;

            initGUI = false;
        }


        // Get cursor position
        Vector2 invMouse = new Vector2(Input.mousePosition.x, Screen.height-Input.mousePosition.y);


        if (active)
        {
            GUI.Box(surroundingRect, GUIContent.none, backgroundStyle);

            for (int ind = 0; ind < linesDisplayed.Count; ind++)
            {
                GUI.Label(linesRectangles[ind], linesDisplayed[ind], labelTextStyle);
                if (linesRectangles[ind].Contains(invMouse))
                {
                    mouseInLine = ind;
                    mouseInRect = linesRectangles[ind];
                }
            }


            // Active Lines

            foreach (ActiveLine al in activeLines)
            {
                if ((al.line == mouseInLine) || showAllButtons)
                {
                    tempButtonRect.x = activeButtonMinX;
                    tempButtonRect.y = linesRectangles[al.line].y + 2;
                    int i = 0;
                    foreach (string str in al.buttonStrings)
                    {
                        tempButtonRect.width = al.buttonWidth[i] - activeButtonOffset;
                        if (GUI.Button(tempButtonRect, str))
                        {
                            al.buttonActions[i]();
                            reEvaluationTimer = 0;
                            buttonUpdate = true; activeTriggerAction();
                            al.startedPos = (al.startedPos == 0) ? 1 : 0;
                        }
                        tempButtonRect.x += al.buttonWidth[i];
                        i++;
                    }
                }
            }


            foreach (ActiveLine al in activeLines)
            {
                // Show tooltip
                if (al.line == mouseInLine)
                    if (al.toolTipDE != "")
                    {
                        float height;
                        if (al.toolTipDE.Contains("\n"))
                            height = 56;
                        else height = 36;

                        if (GlobalSettings.language.Equals("de"))
                            GUI.Button(new Rect(invMouse.x, invMouse.y + 22, 800, height), al.toolTipDE);
                        else GUI.Button(new Rect(invMouse.x, invMouse.y + 22, 800, height), al.toolTipEN);
                        if (GlobalSettings.language.Equals("de"))
                            GUI.Button(new Rect(invMouse.x, invMouse.y + 22, 800, height), al.toolTipDE);
                        else GUI.Button(new Rect(invMouse.x, invMouse.y + 22, 800, height), al.toolTipEN);
                    }
            }

        }
    }
}



// Class holding a line which is dispalyed over time
class TimedLine
{
    public int line;
    public int position;
    public double timecounter;
    public float steplength;

    public TimedLine(int line, int position, float timecounter, float steplength)
    {
        this.line = line;
        this.position = position;
        this.timecounter = timecounter;
        this.steplength = steplength;
    }

    public void ResetCounter()
    {
        timecounter = steplength;
    }

    public bool CheckCounter()
    {
        return (timecounter <= 0);
    }


    public bool HandleCounter(float delta)
    {
        timecounter -= delta;
        if (timecounter <= 0)
        {
            position++;
            timecounter = steplength;
            return (true);
        }
        return (false);
    }
}


// Class with control buttons and keybaord shrotcuts
class ActiveLine
{
    public int line;
    public System.Action[] buttonActions;
    public string[] buttonStrings;
    public int[] buttonWidth;
    public KeyCode[] keys;
    public int startedPos;
    public string toolTipEN;
    public string toolTipDE;

    public ActiveLine(int line, System.Action[] buttonActions, string[] buttonStrings, int[] buttonWidth, int startedPos, KeyCode[] keys, string toolTipEN, string toolTipDE)
    {
        this.line = line;
        this.buttonActions = buttonActions;
        this.buttonStrings = buttonStrings;
        this.buttonWidth = buttonWidth;
        this.startedPos = startedPos;
        this.keys = keys;
        this.toolTipDE = toolTipDE;
        this.toolTipEN = toolTipEN;
    }
}


// Self updating line
class AutoUpdateElement
{
    public int line;
    public int element;
    public System.Func<string> evaluation;
    public bool buttonTriggered;

    public AutoUpdateElement(int line, int el, System.Func<string> evaluation, bool buttonTriggered)
    {
        this.line = line;
        this.element = el;
        this.evaluation = evaluation;
        this.buttonTriggered = buttonTriggered;
    }
}
