using UnityEngine;
using System.Collections;

/*
 * PARTIALLY AI RELATED
 * 
 * The WingSet is an important element of the whole simulation.
 * It makes up for Unity's lack of a handle for real air-friction (Luftwiderstand).
 * 
 * technically it keeps track of the positions of every bone, calculates the surface of the triangels between them
 * and then computes how much air has been pushed out of the way and in what direction this happened.
 * 
 * At this occasion it also saves some values about the chromosomes which have "caused" those forces due to their values.
 * This provides a possibility to assign a fitness to each chromosome in particular.
 * 
 * Last but not least, the loop down below handles the muscle actions, causing the wings to actually move
 * and ontains the abortion-criteria of every sample based on how many wing-flaps it already performed.
 * When this criteria triggers, all data is gathered and sent to the "PopulationControler" to evaluate the sample (and also replace it with a new sample based on a different or mutated Phenotype).
 */



public class WingSet
{
    public bool running = true;

    public GameObject body;
    public Phenotype phenotype;
    public Rigidbody[] segments;
    public HingeJoint[] joints;
    public PolygonSet polyset = null;

    int wingParts;
    int[] muscleFlapDelays;
    int maxFlapSteps;
    int currentFlapPosition = 0;
    public int totalFlapPosition = 0;
    int segmentsNumber;
    int polgyonsNumber = 0;

    bool flapDir = true;

    // polyPositions holds the current positions of all polygon-corners which represent the wing-membrane
    // It is calculated and updated from the rigidbodies of all wing bones
    Vector3[] polyPositions = null;
    // The last poly positions are saved and are the base for calculating the air-resistance
    Vector3[] lastPolyPositions = null;

    // This array holds the vertical (y) component of all forces added to the wing segments
    // It is used to compute the performance of particular chromosomes
    float[] levitationSum;




    public WingSet(GameObject body, Phenotype phenotype, Rigidbody[] wingSegments, HingeJoint[] wingJoints)
    {
        this.body = body;
        this.phenotype = phenotype;
        this.segments = wingSegments;

        // Note on the joints array. The HingeJoint components could be recieved through the GetComponent<Joint> function from the wingSegments[n].gameObject, however, that would be significantly slower.
        // So this array holds the joints separately
        this.joints = wingJoints;


        segmentsNumber = segments.Length;
        levitationSum = new float[segmentsNumber];

        int len = 0; int polpos = 0;
        for (int i = 0; i < segmentsNumber; i++)
        {
            if (segments[i] != null)
            {
                len++;
                polpos++;

                if (polpos == 3) polgyonsNumber++;
            }
            else polpos = 0;

            levitationSum[i] = 0;
        }

        polyPositions = new Vector3[len];
        lastPolyPositions = new Vector3[len];

        for (int i = 0; i < polyPositions.Length; i++)
        {
            polyPositions[i] = new Vector3(-1000, -1000, -1000);
        }


        maxFlapSteps = phenotype.maxFlapSteps;
        totalFlapPosition = -Mathf.FloorToInt(maxFlapSteps / 2f);

        wingParts = phenotype.TypeChromosomes.Count + 1;

        // Paralel wing
        muscleFlapDelays = new int[wingParts];
        // Different wing
        //muscleFlapDelays = new int[2 * segs];

        //AddComponentMenu object "ActiveChromosome" which holds the joint and actual KeyValuePair like muscleFlapDelays, etc.
        //This allows to actually evaluate chromosomes by saving how much Auftrieb they created and so on


        muscleFlapDelays[0] = Mathf.RoundToInt(phenotype.rootChromL.muscleFlapStep * maxFlapSteps);

        for (int i = 1; i < wingParts; i++)
        {
            Chromosome chrom = phenotype.TypeChromosomes[i - 1];
            muscleFlapDelays[i] = Mathf.RoundToInt(chrom.muscleFlapStep * maxFlapSteps);
        }
    }




    public void UpdatePhysics()
    {
        if (flapDir) currentFlapPosition++;
        if (!flapDir) currentFlapPosition--;

        if (currentFlapPosition >= maxFlapSteps)
        {
            currentFlapPosition = 0;
            flapDir = false;
        }
        if (currentFlapPosition < 0)
        {
            currentFlapPosition = 0;
            flapDir = true;
        }

        totalFlapPosition++;
        if (totalFlapPosition == 0) currentFlapPosition = 0;


        // This is a core function of the whole wing simulation
        // First it reads the current positions from the rigidbodys.
        // Then it calculates their movement relative to the last time
        // This diference is the base for the applied force which represents the air-resistance
        // This force is applied but also saved for using when evaluating the phenotype

        int i = 0, r = 0, polys = 0;
        for (int ind = 0; ind < segmentsNumber; ind++)
        {
            // Update the positions of all polygon coordinates
            if (segments[ind] != null)
            {
                lastPolyPositions[i] = polyPositions[i];
                polyPositions[i] = segments[ind].position + segments[ind].transform.rotation * (new Vector3(0, 0, segments[ind].transform.localScale.z / 2));

                if (totalFlapPosition > 1)
                if ((polyPositions[i] - lastPolyPositions[i]).sqrMagnitude > GlobalSettings.destructionMoveLimit)
                {
                    PopulationControler.RemoveSample(body.gameObject, body.gameObject.GetComponent<DragonScript>());
                    return;
                }

                if (r >= 2)
                {
                    if (totalFlapPosition > 1)
                    {
                        Vector3 area = GlobalSettings.standardWingDesignInverting[polys] * Vector3.Cross(polyPositions[i - 1] - polyPositions[i - 2], polyPositions[i] - polyPositions[i - 2]);

                        Vector3 distA = lastPolyPositions[i] - polyPositions[i];
                        Vector3 distB = lastPolyPositions[i - 1] - polyPositions[i - 1];
                        Vector3 distC = lastPolyPositions[i - 2] - polyPositions[i - 2];

                        levitationSum[ind] += -distA.y;
                        levitationSum[ind - 1] += -distB.y;
                        levitationSum[ind - 2] += -distC.y;

                        segments[ind].AddForce(area * distA.sqrMagnitude * GlobalSettings.AirDensity);
                        segments[ind - 1].AddForce(area * distB.sqrMagnitude * GlobalSettings.AirDensity);
                        segments[ind - 2].AddForce(area * distC.sqrMagnitude * GlobalSettings.AirDensity);
                    }

                    polys++;
                }

                i++;
                r++;
            }
            else r = 0;




            // Calculate and apply the physic forces

        }


        // This loop inverts the target velocity of all wing msucles ("joints") at the right time
        // It is the main mechanism causing the wings to acutally flap up and down.
        if (running)
        if (totalFlapPosition > 0)
        {
            for (int ind = 0; ind < wingParts; ind++)
            {

                // Switches direction when the coutner reaches the desired delay
                if (currentFlapPosition == muscleFlapDelays[ind])
                {
                    JointMotor muscle = joints[ind].motor;
                    muscle.targetVelocity *= -1;
                    joints[ind].motor = muscle;

                    muscle = joints[ind + wingParts].motor;
                    muscle.targetVelocity *= -1;
                    joints[ind + wingParts].motor = muscle;
                }


                /*
                // This disabled section was the first concept for achiving flaping wings. They would change direction when reaching a desired angle instead of a desired time.
                // It would be far smother as it doesn't require hard limits (joints[ind].useLimits would set to FALSE) and no counter either.
                // However, it doesn't work out because Unity's engine gives no manual control over the current velocity. joints[ind].velocity is readonly...
                // So the wings cannot be controled properly...
                
                JointMotor motor = joints[ind].motor;
                if (motor.targetVelocity > 0)
                {
                    if (joints[ind].angle >= joints[ind].limits.max)
                        motor.targetVelocity = -GlobalSettings.dragonMaxMuscleVelocity;
                } else
                    if (joints[ind].angle <= joints[ind].limits.min)
                        motor.targetVelocity = GlobalSettings.dragonMaxMuscleVelocity;
                joints[ind].motor = motor;
                */

            }


            // AI RELATED PART ///////////////////////////////////////////////////

            // Check how many times the wings have moved up and down
            if (totalFlapPosition > (GlobalSettings.requiredEvaluationFlaps * 2 - 0.5f) * maxFlapSteps)
            {
                // If it is enough, call fitness function for evaluation
                if (GlobalSettings.taskMode.Equals("Learning"))
                {
                        // The following code calculates the current sum of the surface of all wing-segments.
                        float totalWingArea = GetCurrentWingArea();

                        // Calclulates the individual performance of chromosomes
                        float[] performanceOfChromosomes = GetChromosomePerformance();

                        running = false;

                        // Send data to the controler
                        PopulationControler.EvaluateSample(body.gameObject, body.gameObject.GetComponent<DragonScript>(), totalWingArea, performanceOfChromosomes);
                }

            }
            ///////////////////////////////////////////////////////////////////////
        }

    }


    public float GetCurrentWingArea()
    {
        float area = 0;


        int i = 0, r = 0;
        for (int ind = 0; ind < segmentsNumber; ind++)
        {
            // Update the positions of all polygon coordinates
            if (segments[ind] != null)
            {
                polyPositions[i] = segments[ind].position + segments[ind].transform.rotation * (new Vector3(0, 0, segments[ind].transform.localScale.z / 2));

                if (r >= 2)
                    area += Vector3.Cross(polyPositions[i - 1] - polyPositions[i - 2], polyPositions[i] - polyPositions[i - 2]).magnitude / 2;

                i++;
                r++;
            }
            else r = 0;
        }

        return (area);
    }

    public float GetCurrentLevitation()
    {
        float currentTotalLevitation = 0;

        int i = 0, r = 0, polys = 0;
        for (int ind = 0; ind < segmentsNumber; ind++)
        {
            if (segments[ind] != null)
            {
                if (r >= 2)
                {
                    if (totalFlapPosition > 1)
                    {
                        Vector3 area = GlobalSettings.standardWingDesignInverting[polys] * Vector3.Cross(polyPositions[i - 1] - polyPositions[i - 2], polyPositions[i] - polyPositions[i - 2]);

                        currentTotalLevitation += (((lastPolyPositions[i] - polyPositions[i]).y + (lastPolyPositions[i - 1] - polyPositions[i - 1]).y + (lastPolyPositions[i - 2] - polyPositions[i - 2]).y) * area.y * (area.magnitude / 2)) / 10;
                    }
                    polys++;
                }
                i++;
                r++;
            }
            else r = 0;
        }

        return (currentTotalLevitation);
    }




    public float[] GetChromosomePerformance()
    {
        float[] chromPerformance = new float[phenotype.TypeChromosomes.Count + 1];

        for (int i = 0; i < chromPerformance.Length; i++)
            chromPerformance[i] = 0;

        for (int ind = 0; ind < segmentsNumber; ind++)
        {
            if (segments[ind] == null)
            {
                chromPerformance[int.Parse(segments[ind - 3].name)] += levitationSum[ind - 3];
                chromPerformance[int.Parse(segments[ind - 2].name)] += levitationSum[ind - 2];
                chromPerformance[int.Parse(segments[ind - 1].name)] += levitationSum[ind - 1];
            }
        }

        return (chromPerformance);
    }


    public void UpdateVisualisation()
    {
        if (polyset != null) polyset.EditPolygons(polyPositions);
    }


    public void RenderVisualisation(bool render)
    {
        if (render)
        { if (polyset == null) polyset = WingSetFactory.VisualizeWingMembrane(segments); }
        else if (polyset != null) { polyset.Destroy(); polyset = null; }
    }


    public void Destroy()
    {
        foreach (Rigidbody bone in segments)
            if (bone != null)
                MonoBehaviour.Destroy(bone.gameObject);

        if (polyset != null)
            polyset.Destroy();
    }

}

