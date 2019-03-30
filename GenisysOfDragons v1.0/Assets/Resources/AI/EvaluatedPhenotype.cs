using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * AI RELATED (CORE)
 * 
 * The "EvaluatedPhenotye" is an important element of the whole AI.
 * Basically it is a normal Phenotype but with an attached fitness value and a fitness-array for its chromosomes.
 * Those values have been provided through the EvaluatePhenotype function found in "PopulationControler".
 * 
 * However, for the easier realisation of flexible RULES for creating
 * new Phenotypes based on the existing ones (-> the core of the genetic algorithm), this class provides a couple of 
 * operators and special functions in capital letters.
 * 
 * For the usage and to see more about rules, look into "AIControler".
 */


public struct EvaluatedPhenotype
{
    // Phenotype this evaluation is made for
    public Phenotype phenotype;

    // What height the sample has reached with the atatched phenotype
    public float reachedHeight;
    
    // Computed fitness of the whole phenotype including its chromosomes
    public float totalFitness;

    // Lifes say how often the same phenotype can be reused for generating new phenotypes.
    public int life;


    // Wing surface
    float wingSurface;

    // Array for accessing the assumed fitness of each chromosome
    float[] fitnessOfChromosomes;

    

    // Constructor
    public EvaluatedPhenotype(Phenotype phenotype, float reachedHeight, float wingSurface, float[] fitnessOfChromosomes)
    {
        this.reachedHeight = reachedHeight; // Height in this case
        this.phenotype = phenotype;
        this.wingSurface = wingSurface;
        this.fitnessOfChromosomes = fitnessOfChromosomes;
        this.life = GlobalSettings.phenotypeLife;
        this.totalFitness = 0;
        this.totalFitness = CalculateTotalFitness();
    }

    // This fucntion calculates the actual fitness of the phenotype
    public float CalculateTotalFitness()
    {
        int len = fitnessOfChromosomes.Length;

        float fitness = 0;
        for (int i = 0; i < len; i++)
            fitness += fitnessOfChromosomes[i];

        fitness = reachedHeight * (wingSurface / 10) + fitness;

        return (fitness);
    }

    public bool ReduceLife()
    {
        life--;
        return(life <= 0);
    }

    // Add to the sorted lists of OpulatioNControler when there'sroom in them
    // If the good list is too full with better phenotypes, it is being added to the "badlist".
    // For the idea behind the "bad list", please look into the documentation.
    // By the way, this function basically performs the typical pattern of genetic algorithms called "selection".
    public bool AddToList(List<EvaluatedPhenotype> goodList, List<EvaluatedPhenotype> badList)
    {
        if (this.reachedHeight > PopulationControler.TotalBestPhenotype.reachedHeight)
            PopulationControler.TotalBestPhenotype = this;

        int listLen = goodList.Count;
        int limit = Mathf.RoundToInt(GlobalSettings.populationSize / 3);
        bool firstListFull = false;

        int i = 0;
        do
        {
            if (i >= listLen)
            {
                if (listLen >= limit)
                    firstListFull = true;
                else goodList.Add(this);

                break;
            }

            if (totalFitness > goodList[i].totalFitness)
            {
                goodList.Insert(i + 1, this);
                if (goodList.Count > limit)
                    goodList.RemoveAt(limit);
            }

            i++;
        } while (i < listLen);


        if (firstListFull)
        {
            listLen = badList.Count;
            limit = Mathf.RoundToInt(GlobalSettings.populationSize / 6);

            i = 0;
            do
            {
                if (i >= listLen)
                {
                    if (listLen != limit)
                        badList.Add(this);
                    break;
                }
                if (totalFitness <= goodList[i].totalFitness)
                {
                    badList.Insert(i + 1, this);

                    if (badList.Count > limit)
                        badList.RemoveAt(limit);
                }

                i++;
            } while (i < listLen);
        }

        return (!firstListFull);
    }


    // The following capitalized functions are required for generating new phenotypes. Look into the script "AIControler" for their ussage.
    // Their names are mostly self-explanatory though.

    // Mutation functions

    static public Phenotype MUTATE_FLAPSTEPS(Phenotype P, float prob)
    {
        if (Random.value <= prob)
        {
            P.rootChromL.muscleFlapStep = (P.rootChromL.muscleFlapStep * VARIANCE_FACTOR()) % 1;
            P.rootChromR.muscleFlapStep = P.rootChromL.muscleFlapStep;
        }

        foreach (Chromosome chrom in P.TypeChromosomes)
            if (Random.value <= prob)
                chrom.muscleFlapStep = (chrom.muscleFlapStep * VARIANCE_FACTOR()) % 1;

        return (P);
    }

    static public Phenotype MUTATE_LIMITS(Phenotype P, float prob)
    {
        if (Random.value <= prob)
        {
            P.rootChromL.muscleMinLimit = (int) (P.rootChromL.muscleMinLimit * VARIANCE_FACTOR());
            P.rootChromR.muscleMinLimit = P.rootChromL.muscleMinLimit;

            P.rootChromL.muscleMaxLimit = (int) (P.rootChromL.muscleMaxLimit * VARIANCE_FACTOR());
            P.rootChromR.muscleMaxLimit = P.rootChromL.muscleMaxLimit;
        }

        foreach (Chromosome chrom in P.TypeChromosomes)
            if (Random.value <= prob)
            {
                chrom.muscleMinLimit = (int) (chrom.muscleMinLimit * VARIANCE_FACTOR());
                chrom.muscleMaxLimit = (int) (chrom.muscleMaxLimit * VARIANCE_FACTOR());
            }

        return (P);
    }


    static public Phenotype INVERT_FLAPSTEPS(Phenotype P)
    {
        return (INVERT_FLAPSTEPS(P, 1));
    }
    static public Phenotype INVERT_FLAPSTEPS(Phenotype P, float prob)
    {
        if (Random.value <= prob)
        {
            P.rootChromL.muscleFlapStep = 1 - P.rootChromL.muscleFlapStep;
            P.rootChromR.muscleFlapStep = P.rootChromL.muscleFlapStep;
        }

        foreach (Chromosome chrom in P.TypeChromosomes)
            if (Random.value <= prob)
                chrom.muscleFlapStep = 1 - chrom.muscleFlapStep;

        return (P);
    }


    static public Phenotype MUTATE_FLAPLENGTH(Phenotype P)
    {
        P.maxFlapSteps = Mathf.RoundToInt(P.maxFlapSteps * VARIANCE_FACTOR());

        return (P);
    }

    static public Phenotype MUTATE_STRENGTHDISTRIBUTION(Phenotype P)
    {
        P.muscleStrengthDistribution = P.muscleStrengthDistribution * VARIANCE_FACTOR();

        return (P);
    }


    static public Phenotype MUTATE_BONELENGTH(Phenotype P, float prob)
    {
        if (Random.value <= prob)
        {
            P.rootChromL.boneLength = (P.rootChromL.boneLength * VARIANCE_FACTOR());
            P.rootChromR.boneLength = P.rootChromL.boneLength;
        }

        foreach (Chromosome chrom in P.TypeChromosomes)
            if (Random.value <= prob)
                chrom.boneLength = (chrom.boneLength * VARIANCE_FACTOR());

        return (P);
    }

    static public Phenotype MUTATE_FULL(Phenotype P, float prob)
    {
        P = MUTATE_FLAPSTEPS(P, prob);

        if (Random.value <= prob)
            P = MUTATE_FLAPLENGTH(P);

        if (Random.value <= prob)
            P = MUTATE_STRENGTHDISTRIBUTION(P);

        P = MUTATE_LIMITS(P, prob);

        P = MUTATE_BONELENGTH(P, prob);

        return (P);
    }



    // For generation rules

    // Randomly combine (cross) the two old phenotypes to a new phenotype
    public static Phenotype operator *(EvaluatedPhenotype P1, EvaluatedPhenotype P2)
    {

        Chromosome rootL, rootR;

        if (Random.value > 0.5f)
        {
            rootL = P1.phenotype.rootChromL;
            rootR = P1.phenotype.rootChromR;
        }
        else
        {
            rootL = P2.phenotype.rootChromL;
            rootR = P2.phenotype.rootChromR;
        }

        List<Chromosome> minChromosomes = new List<Chromosome>();

        int ind = 1;
        foreach (Chromosome chrom in P1.phenotype.TypeChromosomes)
        {
            if (Random.value > 0.5f)
                minChromosomes.Add(chrom);
            else minChromosomes.Add(P2.phenotype.TypeChromosomes[ind - 1]);

            ind++;
        }

        float strengthDistr;
        int maxFlapSteps;

        if (Random.value > 0.5f)
            strengthDistr = P1.phenotype.muscleStrengthDistribution;
        else
            strengthDistr = P2.phenotype.muscleStrengthDistribution;

        if (Random.value > 0.5f)
            maxFlapSteps = P1.phenotype.maxFlapSteps;
        else
            maxFlapSteps = P2.phenotype.maxFlapSteps;


        return (new Phenotype(rootL, rootR, minChromosomes, strengthDistr, maxFlapSteps));

    }

    // Create a new phenotype based each half on a base phenotype
    public static Phenotype operator /(EvaluatedPhenotype P1, EvaluatedPhenotype P2)
    {
        Chromosome rootL, rootR;

        if (Random.value > 0.5f)
        {
            EvaluatedPhenotype P3 = P1;
            P1 = P2;
            P2 = P3;
        }

        rootL = P1.phenotype.rootChromL;
        rootR = P1.phenotype.rootChromR;

        List<Chromosome> newChromosomes = new List<Chromosome>();

        int half = P1.phenotype.TypeChromosomes.Count;

        int ind = 1;
        foreach (Chromosome chrom in P1.phenotype.TypeChromosomes)
        {
            if (ind > half)
                newChromosomes.Add(chrom);
            else newChromosomes.Add(P2.phenotype.TypeChromosomes[ind - 1]);

            ind++;
        }


        float strengthDistr = P2.phenotype.muscleStrengthDistribution;
        int maxFlapSteps = P1.phenotype.maxFlapSteps;

        return (new Phenotype(rootL, rootR, newChromosomes, strengthDistr, maxFlapSteps));

    }

    // Create new Phenotype with "best" Chromosomes, based on the array "fitnessOfChromosomes"
    public static Phenotype operator +(EvaluatedPhenotype P1, EvaluatedPhenotype P2)
    {
        Chromosome rootL, rootR;

        if (P1.fitnessOfChromosomes[0] > P2.fitnessOfChromosomes[0])
        {
            rootL = P1.phenotype.rootChromL;
            rootR = P1.phenotype.rootChromR;
        }
        else
        {
            rootL = P2.phenotype.rootChromL;
            rootR = P2.phenotype.rootChromR;
        }

        List<Chromosome> maxChromosomes = new List<Chromosome>();

        int ind = 1;
        foreach (Chromosome chrom in P1.phenotype.TypeChromosomes)
        {
            if (P1.fitnessOfChromosomes[ind] > P2.fitnessOfChromosomes[ind])
                maxChromosomes.Add(chrom);
            else maxChromosomes.Add(P2.phenotype.TypeChromosomes[ind - 1]);

            ind++;
        }

        float strengthDistr;
        int maxFlapSteps;

        if (P1.totalFitness > P2.totalFitness)
        {
            strengthDistr = P1.phenotype.muscleStrengthDistribution;
            maxFlapSteps = P1.phenotype.maxFlapSteps;
        }
        else
        {
            strengthDistr = P2.phenotype.muscleStrengthDistribution;
            maxFlapSteps = P2.phenotype.maxFlapSteps;
        }

        Phenotype newPhen = new Phenotype(rootL, rootR, maxChromosomes, strengthDistr, maxFlapSteps);

        return (newPhen);
    }

    // Create new Phenotype with "lowest" Chromosomes, based on the array "fitnessOfChromosomes"
    public static Phenotype operator -(EvaluatedPhenotype P1, EvaluatedPhenotype P2)
    {
        Chromosome rootL, rootR;

        if (P1.fitnessOfChromosomes[0] <= P2.fitnessOfChromosomes[0])
        {
            rootL = P1.phenotype.rootChromL;
            rootR = P1.phenotype.rootChromR;
        }
        else
        {
            rootL = P2.phenotype.rootChromL;
            rootR = P2.phenotype.rootChromR;
        }

        List<Chromosome> minChromosomes = new List<Chromosome>();

        int ind = 1;
        foreach (Chromosome chrom in P1.phenotype.TypeChromosomes)
        {
            if (P1.fitnessOfChromosomes[ind] <= P2.fitnessOfChromosomes[ind])
                minChromosomes.Add(chrom);
            else minChromosomes.Add(P2.phenotype.TypeChromosomes[ind - 1]);

            ind++;
        }

        float strengthDistr;
        int maxFlapSteps;

        if (P1.totalFitness <= P2.totalFitness)
        {
            strengthDistr = P1.phenotype.muscleStrengthDistribution;
            maxFlapSteps = P1.phenotype.maxFlapSteps;
        }
        else
        {
            strengthDistr = P2.phenotype.muscleStrengthDistribution;
            maxFlapSteps = P2.phenotype.maxFlapSteps;
        }

        Phenotype newPhen = new Phenotype(rootL, rootR, minChromosomes, strengthDistr, maxFlapSteps);

        return (newPhen);
    }


    // Create new Phenotype with "best" Chromosomes, based on the array "fitnessOfChromosomes"
    static public Phenotype MAX_PHENOTYPE(EvaluatedPhenotype[] P)
    {
        EvaluatedPhenotype chosen;

        chosen = P[0];
        foreach (EvaluatedPhenotype ePhen in P)
        {
            if (ePhen.fitnessOfChromosomes[0] > chosen.fitnessOfChromosomes[0])
                chosen = ePhen;
        }

        Chromosome rootL = chosen.phenotype.rootChromL;
        Chromosome rootR = chosen.phenotype.rootChromR;


        List<Chromosome> minChromosomes = new List<Chromosome>();

        int len = P[0].phenotype.TypeChromosomes.Count;

        for (int ind = 1; ind <= len; ind++)
        {
            chosen = P[0];
            foreach (EvaluatedPhenotype ePhen in P)
            {
                if (ePhen.fitnessOfChromosomes[ind] > chosen.fitnessOfChromosomes[ind])
                    chosen = ePhen;
            }

            minChromosomes.Add(chosen.phenotype.TypeChromosomes[ind - 1]);
        }

        float strengthDistr;
        int maxFlapSteps;

        chosen = P[0];
        foreach (EvaluatedPhenotype ePhen in P)
        {
            if (ePhen.totalFitness > chosen.totalFitness)
                chosen = ePhen;
        }

        strengthDistr = chosen.phenotype.muscleStrengthDistribution;
        maxFlapSteps = chosen.phenotype.maxFlapSteps;


        Phenotype newPhen = new Phenotype(rootL, rootR, minChromosomes, strengthDistr, maxFlapSteps);

        return (newPhen);
    }


    // Create new Phenotype with "lowest" Chromosomes, based on the array "fitnessOfChromosomes"
    static public Phenotype MIN_PHENOTYPE(EvaluatedPhenotype[] P)
    {
        EvaluatedPhenotype chosen;

        chosen = P[0];
        foreach (EvaluatedPhenotype ePhen in P)
        {
            if (ePhen.fitnessOfChromosomes[0] <= chosen.fitnessOfChromosomes[0])
                chosen = ePhen;
        }

        Chromosome rootL = chosen.phenotype.rootChromL;
        Chromosome rootR = chosen.phenotype.rootChromR;


        List<Chromosome> minChromosomes = new List<Chromosome>();

        int len = P[0].phenotype.TypeChromosomes.Count;

        for (int ind = 1; ind <= len; ind++)
        {
            chosen = P[0];
            foreach (EvaluatedPhenotype ePhen in P)
            {
                if (ePhen.fitnessOfChromosomes[ind] <= chosen.fitnessOfChromosomes[ind])
                    chosen = ePhen;
            }

            minChromosomes.Add(chosen.phenotype.TypeChromosomes[ind - 1]);
        }

        float strengthDistr;
        int maxFlapSteps;

        chosen = P[0];
        foreach (EvaluatedPhenotype ePhen in P)
        {
            if (ePhen.totalFitness <= chosen.totalFitness)
                chosen = ePhen;
        }

        strengthDistr = chosen.phenotype.muscleStrengthDistribution;
        maxFlapSteps = chosen.phenotype.maxFlapSteps;


        Phenotype newPhen = new Phenotype(rootL, rootR, minChromosomes, strengthDistr, maxFlapSteps);

        return (newPhen);
    }


    static private float VARIANCE_FACTOR()
    {
        return (1 - GlobalSettings.RandomVariance + Random.value * 2 * GlobalSettings.RandomVariance);
    }

}


