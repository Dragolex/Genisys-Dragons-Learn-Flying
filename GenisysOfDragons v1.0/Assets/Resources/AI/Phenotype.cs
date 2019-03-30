using UnityEngine;
using System.Collections.Generic;


/*
 * AI RELATED
 * 
 * This class represents a candidate for the genetic algorithm
 * It holds the chromosomes required to instantiate the physic objects requried for testing this candidate.
 * It also contains a few basic variables independant of the chromosomes
 * 
 * See the documentation for more information about the principle
 * and see "AIControler" for the usage of this class.
*/



public class Phenotype {

    // Chromosome-Data //
    public Chromosome rootChromL; // Left root chromosome
    public Chromosome rootChromR; // Right root chromosome
    public List<Chromosome> TypeChromosomes = new List<Chromosome>(); // Main chromosome list
    //public float maxBoneSpan = -1; // This describes the longest row of connected bones (used for calculating the muscle (joint) strength of each segment realistically


    // Chromosome-Independant Settings //

    public float muscleStrengthDistribution; // factor describing how sharp the destribution is. See the "calculateMuscles()" function.
    public int maxFlapSteps; // This describes the number of steps needing to pass for a full flap of the wings



    // Constructors //

    public Phenotype(Chromosome rootChromL, Chromosome rootChromR, List<Chromosome> TypeChromosomes, float muscleStrengthDistribution, int totalFlapSteps)
    {
        this.rootChromL = rootChromL;
        this.rootChromR = rootChromR;
        this.TypeChromosomes = TypeChromosomes;
        this.muscleStrengthDistribution = muscleStrengthDistribution;
        this.maxFlapSteps = totalFlapSteps;

        calculateMuscles();
    }

    public Phenotype(Chromosome rootChromL, Chromosome rootChromR, List<Chromosome> TypeChromosomes, float muscleStrengthDistribution, float muscleStrengthDistributionRange, int totalFlapSteps, int totalFlapStepsRange)
    {
        this.rootChromL = rootChromL;
        this.rootChromR = rootChromR;
        this.TypeChromosomes = TypeChromosomes;
        this.muscleStrengthDistribution = muscleStrengthDistribution - muscleStrengthDistributionRange + Random.value * (2 * muscleStrengthDistributionRange);
        this.maxFlapSteps = totalFlapSteps - totalFlapStepsRange + Mathf.RoundToInt(Random.value * (2 * totalFlapStepsRange));

        calculateMuscles();
    }



    // Altering functions

    public void SetDependantBonethickness(float startThick, float endThick)
    {
        float[] rootdistances = GetRootDistances();
        float maxDist = rootdistances[0];
        int counter = 1;

        rootChromL.boneThickness = startThick;
        rootChromR.boneThickness = startThick;


        foreach (Chromosome chrom in TypeChromosomes)
        {
            chrom.boneThickness = Mathf.Lerp(startThick, endThick, rootdistances[counter] / maxDist);
            counter += 1;
        }
    }


    public void SetDependantBonecolor(Color startCol, Color endCol)
    {
        float[] rootdistances = GetRootDistances();
        float maxDist = rootdistances[0];
        int counter = 1;

        rootChromL.col = startCol;
        rootChromR.col = startCol;

        foreach (Chromosome chrom in TypeChromosomes)
        {
            chrom.col = Color.Lerp(startCol, endCol, rootdistances[counter] / maxDist);
            counter += 1;
        }
    }



    // This function calculates the muscle strength
    // The code asigns a strength depending on the distance from the body.
    // That menas, the most outer segments will have the weakest force while the clsoest have the most strength.
    // That is meant to make everything a bit clsoer to the nature and a bit mroe predictable.
    // For fairness reasons, the total sum of all muscle-strength is fixed to "GlobalSettings.dragonAvailableForce"!
    // The phenotype variable "muscleStrengthDistribution" tells how strong the diversity is focused on the body. The higehr the value, the faster strength decreases with distance.

    public void calculateMuscles()
    {
        float[] rootDistances = GetRootDistances();
        int chromLen = TypeChromosomes.Count;
        float maxDist = rootDistances[0] + (rootDistances[0] / TypeChromosomes.Count) * 1;


        float strengthSum = Mathf.Pow(1.1f, muscleStrengthDistribution);
        rootChromL.muscleForce = Mathf.Pow(1.1f, muscleStrengthDistribution);
        rootChromR.muscleForce = Mathf.Pow(1.1f, muscleStrengthDistribution);


        for (int i = 0; i < chromLen; i++)
        {
            TypeChromosomes[i].muscleForce = Mathf.Pow(((maxDist * 1.1f - rootDistances[i + 1])) / maxDist, muscleStrengthDistribution);
            strengthSum += TypeChromosomes[i].muscleForce;
        }

        strengthSum = GlobalSettings.dragonAvailableForce / strengthSum;
        rootChromL.muscleForce *= strengthSum;
        rootChromR.muscleForce *= strengthSum;


        for (int i = 0; i < chromLen; i++)
            TypeChromosomes[i].muscleForce *= strengthSum;
    }




    float[] GetRootDistances()
    {
        float[] rootdistances = new float[TypeChromosomes.Count+1];

        float dist, branchRoot = -1, maxDist = 0;
        int counter = 1;

        dist = rootChromL.boneLength;

        foreach (Chromosome chrom in TypeChromosomes)
        {
            if (chrom.numOfJoints > 1)
                    branchRoot = dist;

            rootdistances[counter] = dist;
            maxDist = Mathf.Max(maxDist, dist);
            dist += chrom.boneLength;

            if (chrom.numOfJoints == 0)
                if (branchRoot > -1)
                    dist = branchRoot;

            counter++;
        }

        rootdistances[0] = maxDist;
        return(rootdistances);
    }


    public int[] GetRootDistanceNum()
    {
        int[] rootdistances = new int[TypeChromosomes.Count + 1];

        int dist, branchRoot = -1, maxDist = 0;
        int counter = 1;

        dist = 0;//rootChromL.boneLength;

        foreach (Chromosome chrom in TypeChromosomes)
        {
            if (chrom.numOfJoints > 1)
                branchRoot = dist;

            rootdistances[counter] = dist;
            maxDist = Mathf.Max(maxDist, dist);
            dist += 1;//chrom.boneLength;

            if (chrom.numOfJoints == 0)
                if (branchRoot > -1)
                    dist = branchRoot;

            counter++;
        }

        rootdistances[0] = maxDist;
        return (rootdistances);
    }

}

