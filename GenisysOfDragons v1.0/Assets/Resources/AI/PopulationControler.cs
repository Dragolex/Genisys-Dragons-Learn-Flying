using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * AI RELATED
 * 
 * The populationcontroler is one of the core scripts required for the AI and the genetic algorithm to work.
 * It provides the evaluation function which triggers the computation of a simulated samples fitness.
 * 
 * Additionally it keeps track of the populations current state and forces the creation of new phenotypes when there are empty spots or when the population is being resized.
 * 
 * For a detailed description of how the creation process and mechanism actually works, please read the documentation.
 */


public class PopulationControler : MonoBehaviour {

    // List of dragon bodies
    public static List<GameObject> population = new List<GameObject>();


    // List holding the best evaluated (tested) phenotypes
    public static List<EvaluatedPhenotype> evaluatedPhenotypesGood = new List<EvaluatedPhenotype>();

    // List holding the worst evaluated (tested) phenotypes (used is rare cases to enhance variance of the results)
    public static List<EvaluatedPhenotype> evaluatedPhenotypesBad = new List<EvaluatedPhenotype>();

    public static EvaluatedPhenotype TotalBestPhenotype;

 
    static List<Phenotype> readyPhenotypes = new List<Phenotype>();

    static List<Vector3> populationPositions = new List<Vector3>();
    static List<int> freeIndices = new List<int>();

    public static int testedSamples = 0;


    // This init function resets all the datastructures (required for switching scenes)
    public static void Init()
    {
        population.Clear();
        evaluatedPhenotypesGood.Clear();
        evaluatedPhenotypesBad.Clear();
        readyPhenotypes.Clear();
        populationPositions.Clear();
        freeIndices.Clear();
    }



    public static void EvaluateSample(GameObject body, DragonScript scr, float wingSurface, float[] performanceOfChromosomes)
    {
        EvaluatedPhenotype evaluationData = new EvaluatedPhenotype(scr.wingSet.phenotype, body.transform.position.y, wingSurface, performanceOfChromosomes);

        evaluationData.AddToList(evaluatedPhenotypesGood, evaluatedPhenotypesBad);

        RemoveSample(body, scr);
    }

    public static void RemoveSample(GameObject body, DragonScript scr)
    {
        freeIndices.Add(scr.index);

        population.Remove(body);
        Destroy(body);

        testedSamples++;

        Global.AIControlerScr.GeneratePhenotypes(readyPhenotypes, freeIndices);

        AddPossibleSamples();
    }


    /*
     * This core function checks for available slots (stored in the list "freeIndices") for new samples
     * If there are prepared phenotypes for those new samples (stored in the list "readyPhenotypes") it uses them for creation
     * If there are no prepared phenotypes (like at the start of the first population or when resizing the population) and random is allowed, it creates random phenotypes first to use them
     * */
    public static void AddPossibleSamples()
    {
        AddPossibleSamples(false);
    }

    public static void AddPossibleSamples(bool randomAllowed)
    {
        if (freeIndices.Count > 0) // If there are free slots
            if (readyPhenotypes.Count > 0) // If there are phenotypes to use
            {
                int range = Mathf.Min(freeIndices.Count, readyPhenotypes.Count);

                for (int i = 0; i < range; i++)
                {
                    // Create dragons with the stored phenotypes

                    Vector3 position = populationPositions[freeIndices[i]];
                    GameObject body = DragonFactory.CreateDragon(readyPhenotypes[i], position, Quaternion.Euler(20, 0, 0), freeIndices[i]);
                    population.Add(body);

                    GlobalSettings.SetBodyAndBonesRendering(body, GlobalSettings.renderingBodyAndBones);
                }

                freeIndices.RemoveRange(0, range);
                readyPhenotypes.RemoveRange(0, range);
            }
            else
                if (randomAllowed)
                {
                    int range = freeIndices.Count;

                    for (int i = 0; i < range; i++)
                    {
                        // Create dragons with the new, random phenotypes
                        Phenotype phenotype = PhenotypeFactory.CreateRandom();
                        Vector3 position = populationPositions[freeIndices[i]];
                        GameObject body = DragonFactory.CreateDragon(phenotype, position, Quaternion.Euler(20, 0, 0), freeIndices[i]);
                        population.Add(body);

                        GlobalSettings.SetBodyAndBonesRendering(body, GlobalSettings.renderingBodyAndBones);
                    }
                    freeIndices.Clear();
                }
    }



    public static void ResizePopulation(int newSize)
    {
        newSize = Mathf.Max(1, newSize);
        int dif = newSize-GlobalSettings.populationSize;


        if (dif >= 1) // Increase number
        {
            // Add new available positions for samples to the list and add add their indexes to the list of unused (free) indeces
            for (int i = GlobalSettings.populationSize; i < newSize; i++) // To create
            {
                populationPositions.Add(calcSamplePosition(i));
                freeIndices.Add(i);
            }

            // Force the adding of samples (fills all freeIndices)
            AddPossibleSamples(true);
        }


        if (dif < 0) // Decrease number
        {
            // Removes all samples with a higher index than the new population size allows
            foreach (GameObject rem in population.ToArray())
            {
                int ind = rem.GetComponent<DragonScript>().index;

                if (ind >= (newSize))
                {
                    population.Remove(rem);
                    Destroy(rem);
                }
            }

            // Remove unavailable free indices
            foreach (int ind in freeIndices.ToArray())
            {
                if (ind >= newSize)
                    freeIndices.Remove(ind);
            }

            // Remove the unavailable positions as well
            populationPositions.RemoveRange(newSize, populationPositions.Count - newSize);

            Global.envControlerScr.bestSample = null;
        }


        GlobalSettings.populationSize = newSize;

        for (int i = freeIndices.Count - 1; i >= 0; i--)
        {
            if (freeIndices[i] >= GlobalSettings.populationSize)
                freeIndices.RemoveAt(i);
        }

        if (Global.mainCameraScr.cameraFocusObject == null)
            Global.mainCameraScr.SetCameraFocusObject(population[0]);

        Global.envControlerScr.ResizeFloorsFitToPopulation();
    }


    // Get the best phenotype at any time.
    public static Phenotype GetBestPhenotype()
    {
        return (TotalBestPhenotype.phenotype);
    }


    // Turns the population into readyPhenotypes to "save" them when the have to be destroyed for switching into TestMode.
    public static void SaveAndDestroyPopulation()
    {
        freeIndices.Clear();

        for (int i = 0; i < population.Count; i++)
        {
            DragonScript scr = population[i].GetComponent<DragonScript>();
            readyPhenotypes.Add(scr.wingSet.phenotype);
            Destroy(population[i]);
            freeIndices.Add(i);
        }

        population.Clear();
    }


    public static GameObject GetCurrentBestSample(GameObject curBestBody, float curBestHeight)
    {
        foreach (GameObject obj in population)
        {
            if (obj.transform.position.y > curBestHeight)
            {
                curBestHeight = obj.transform.position.y;
                curBestBody = obj;
            }
        }

        return (curBestBody);
    }




    static Vector3 calcSamplePosition(int pos)
    {
        int dir = 0, sq = 0;
        int xx = 0, yy = 0;
        for (; pos > 0; pos--)
        {
            switch (dir)
            {
                case 0: xx++; if (xx > sq) dir++; break;
                case 1: yy++; if (yy > sq) dir++; break;
                case 2: xx--; if (xx < -sq) dir++; break;
                case 3: yy--; if (yy < -sq) { dir = 0; sq++; } break;
            }
        }

        return (GlobalSettings.originPoint + new Vector3(xx * GlobalSettings.sampleCellWidth, 0, yy * GlobalSettings.sampleCellHeight)); // *40 * 18
    }


}
