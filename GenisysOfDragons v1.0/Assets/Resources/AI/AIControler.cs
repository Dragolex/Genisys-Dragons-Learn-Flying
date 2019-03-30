using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * AI RELATED (CORE)
 * 
 * This script is the core of the genetic algorithm as it handles the creation/computation of new Phenotypes.
 * 
 * Therefore it takes the already evaluated and sorted phenotypes (as an EvaluatedPhenotype structure) from the Lists belonging to the "PopulationControler"
 * And applies certain "rules" on them.
 * The rules describe how new Phenotypes are computed.
 * The rules are chosen by random (with individually defined probability), however, their application follows the Phenotype's overall fitness and, more important, the fitness of their Chromosomes!
 * 
 * Besides the creation rules, there are also rules for Mutations which are applied in a similar way, directly to the resulting Phenotype.
 * 
 * To read more about the whole process, look into the dcoumentation. tehre's a whole sectiond edicated to this.
 */


public class AIControler : MonoBehaviour {


    void Start() {
        if (Global.GetCurrentScene() == Global.mainAIScene)
        {
                print("Initialisizing Learning Mode");

                // Create the first population
                PopulationControler.ResizePopulation(GlobalSettings.startPopulationSize);

                LoadRules();

                
                // Add a random phenotype to the evaluated list for the case that the evry first sample cannot be evaluated successfully.
                float[] em = new float[20];
                for(int i = 0; i < em.Length; i++)
                    em[i] = 1;
                PopulationControler.evaluatedPhenotypesGood.Add(new EvaluatedPhenotype(PhenotypeFactory.CreateRandom(), -100, 1, em));
        }
    }





    // Simple delegate taking a varying number of Phenotypes for crossing them
    public delegate Phenotype crossRule(params EvaluatedPhenotype[] P);

    // Simple delegate taking a single Phenotypes for mutating it
    public delegate Phenotype mutateRule(Phenotype P);


    // Lists for rules
    List<CrossingRule> CrossingRules = new List<CrossingRule>(); // 7
    List<MutationRule> MutationRules = new List<MutationRule>(); // 11
    int totalCrossRuleProbability, totalMutateRuleProbability;

    
    // Initialize the ruels to the list
    public void LoadRules()
    {
        crossRule rule;
        int probability;
        int numOfPhenotypes;
        bool posOrNeg;
        CrossingRules.Clear();


        ////// Crossing-Rules (Generation rules) //////

        posOrNeg = true; // Rules using only positive fitness

        // Random crossing (Phenotypes are chosed randomly from P0 and P1)
        rule = (EvaluatedPhenotype[] P) => { return (P[0] * P[1]); };
        numOfPhenotypes = 2;
        probability = 12;

        CrossingRules.Add(new CrossingRule(rule, probability, numOfPhenotypes, posOrNeg));


        // Maximizing cross (The Chromosome with better fitness is chosen)
        rule = (EvaluatedPhenotype[] P) => { return (P[0] + P[1]); };
        numOfPhenotypes = 2;
        probability = 24;

        CrossingRules.Add(new CrossingRule(rule, probability, numOfPhenotypes, posOrNeg));


        // Split Cross (Chosing half of the chromosomes from P0 and half from P1)
        rule = (EvaluatedPhenotype[] P) => { return (P[0] / P[1]); };
        numOfPhenotypes = 2;
        probability = 8;

        CrossingRules.Add(new CrossingRule(rule, probability, numOfPhenotypes, posOrNeg));


        // Maximum from a range of Phenotypes (Chosing best chromosome-fitness from all
        rule = (EvaluatedPhenotype[] P) => { return (EvaluatedPhenotype.MAX_PHENOTYPE(P)); };
        numOfPhenotypes = -1;
        probability = 20;

        CrossingRules.Add(new CrossingRule(rule, probability, numOfPhenotypes, posOrNeg));

        


        posOrNeg = false; // Rules using negative/minimum fitness

        // Minimizing cross (The Chromosome with lower fitness is chosen)
        rule = (EvaluatedPhenotype[] P) => { return (P[0] - P[1]); };
        numOfPhenotypes = 2;
        probability = 1;

        CrossingRules.Add(new CrossingRule(rule, probability, numOfPhenotypes, posOrNeg));


        // Minimum from a range of Phenotypes (Chosing best chromosome-fitness from all
        rule = (EvaluatedPhenotype[] P) => { return (EvaluatedPhenotype.MIN_PHENOTYPE(P)); };
        numOfPhenotypes = -1;
        probability = 1;

        CrossingRules.Add(new CrossingRule(rule, probability, numOfPhenotypes, posOrNeg));


        // Minimizing cross (The Chromosome with lower fitness is chosen) with Inversion
        rule = (EvaluatedPhenotype[] P) => { return (EvaluatedPhenotype.INVERT_FLAPSTEPS(P[0] - P[1])); };
        numOfPhenotypes = 2;
        probability = 2;

        CrossingRules.Add(new CrossingRule(rule, probability, numOfPhenotypes, posOrNeg));



        totalCrossRuleProbability = 0;
        foreach(CrossingRule aRule in CrossingRules)
        {
            if (aRule.posOrNeg || GlobalSettings.UseNegativeRules)
                totalCrossRuleProbability += aRule.probabilityCount;
        }




        ////// Mutation Rules //////


        mutateRule mRule;
        int probabilityCount;
        MutationRules.Clear();

        // Mutate timesteps with 30% chance
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.MUTATE_FLAPSTEPS(P, 0.3f)); };
        probabilityCount = 3;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));


        // Mutate Limits with 20% chance
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.MUTATE_LIMITS(P, 0.2f)); };
        probabilityCount = 2;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));


        // Invert all timesteps
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.INVERT_FLAPSTEPS(P)); };
        probabilityCount = 1;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));


        // Invert timesteps with 30% chance
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.INVERT_FLAPSTEPS(P, 0.3f)); };
        probabilityCount = 2;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));


        // Invert timesteps with 30% chance
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.INVERT_FLAPSTEPS(P, 0.3f)); };
        probabilityCount = 2;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));


        // Randomize length of a full wing flap
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.MUTATE_FLAPLENGTH(P)); };
        probabilityCount = 4;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));


        // Randomize strength distribution
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.MUTATE_STRENGTHDISTRIBUTION(P)); };
        probabilityCount = 3;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));


        // Mutate length of wings egments with 10% probability
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.MUTATE_BONELENGTH(P, 0.1f)); };
        probabilityCount = 3;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));

        
        // Mutate length of wings egments with 40% probability
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.MUTATE_BONELENGTH(P, 0.4f)); };
        probabilityCount = 1;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));


        // Randomizes everything with 50% probability
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.MUTATE_FULL(P, 0.5f)); };
        probabilityCount = 1;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));


        // Randomizes everything with 5% probability
        mRule = (Phenotype P) => { return (EvaluatedPhenotype.MUTATE_FULL(P, 0.05f)); };
        probabilityCount = 5;
        MutationRules.Add(new MutationRule(mRule, probabilityCount));




        totalMutateRuleProbability = 0;
        foreach (MutationRule aRule in MutationRules)
        {
             totalMutateRuleProbability += aRule.probabilityCount;
        }

    }



    // This function is where the core of the genetic algorithm happens.
    // Like described, it choses random rules for generation and for mutationa nd applies them.
    // So it basically performs the typical patterns of genetic algorithms called "crossing", "combination" and "mutation".
    public int GeneratePhenotypes(List<Phenotype> readyPhenotypes, List<int> freeIndices)
    {
        int repeat;
        for (repeat = 0; repeat < Random.value * 3; repeat++)
        {
            float chosenRuleVal = Mathf.Floor(Random.value * (totalCrossRuleProbability));

            int pos = 0;
            CrossingRule chosenRule = null;
            foreach (CrossingRule aRule in CrossingRules)
            {
                pos += aRule.probabilityCount;
                if (pos >= chosenRuleVal)
                {
                    chosenRule = aRule;
                    break;
                }
            }
            if (chosenRule == null)
                Debug.LogError("No rule has been chosen!");


            bool choseBad = (Random.value <= GlobalSettings.UseBadSampleProbability);
            if (choseBad && (PopulationControler.evaluatedPhenotypesBad.Count == 0)) choseBad = false;


            List<EvaluatedPhenotype> phenList;

            if (choseBad)
                phenList = PopulationControler.evaluatedPhenotypesBad;
            else
                phenList = PopulationControler.evaluatedPhenotypesGood;

            int phenLimit = phenList.Count;


            int phensToChose;
            if (chosenRule.numOfPhenotypes >= 0) // Rule with fixed number of phenotypes as a base
                phensToChose = chosenRule.numOfPhenotypes;
            else phensToChose = Mathf.Max(2,Mathf.CeilToInt(Random.value * (phenLimit / 2))); // Rule with unlimited phenotypes


            EvaluatedPhenotype[] phenotypesParam = new EvaluatedPhenotype[phensToChose];
            int el = Mathf.FloorToInt(Random.value * Random.value * (phenLimit - 1));


            if (phenLimit == 0)
                Debug.LogError("Trying to generate new phenotype with empty lists.");

            if (phenLimit < 2)
            {
                phenotypesParam[0] = phenList[0];
                phenotypesParam[1] = phenList[0];

                if (phenList[0].ReduceLife()) phenList.Remove(phenList[0]);
            }
            else
            {
                for (int i = 0; i < phensToChose; i++)
                    phenotypesParam[i] = phenList[Mathf.Min(phenLimit - 1, el + i)];

                foreach(EvaluatedPhenotype ph in phenotypesParam)
                    if (phenList[0].ReduceLife())
                        phenList.Remove(ph);
            }


                


            // Execute the rule and generate the new phenotype
            Phenotype newPhenotype = chosenRule.rule(phenotypesParam);



            // Find mutation(s)

            while (Random.value < GlobalSettings.MutationRate)
            {
                chosenRuleVal = Mathf.Floor(Random.value * (totalMutateRuleProbability));

                pos = 0;
                MutationRule mutChosenRule = null;

                foreach (MutationRule aRule in MutationRules)
                {
                    pos += aRule.probabilityCount;
                    if (pos >= chosenRuleVal)
                    {
                        mutChosenRule = aRule;
                        break;
                    }
                }

                if (mutChosenRule == null)
                {
                    Debug.Log(pos);
                    Debug.Log(totalMutateRuleProbability);
                    Debug.Log(chosenRuleVal);

                    Debug.LogError("No mutation rule has been chosen!");
                }


                // Apply mutation
                newPhenotype = mutChosenRule.rule(newPhenotype);
            }

            readyPhenotypes.Add(newPhenotype);
        }



        // The variance is being reduced every time a bit until reaching a limit
        GlobalSettings.RandomVariance -= 0.0005f;
        if (GlobalSettings.RandomVariance <= 0.1f)
            GlobalSettings.RandomVariance = 0.1f;


        return (repeat);
    }



    // Two datastructures for the the rules

    class CrossingRule
    {
        public crossRule rule;
        public int probabilityCount;
        public int numOfPhenotypes;
        public bool posOrNeg;

        public CrossingRule(crossRule rule, int probabilityCount, int numOfPhenotypes, bool posOrNeg)
        {
            this.rule = rule;
            this.probabilityCount = probabilityCount;
            this.numOfPhenotypes = numOfPhenotypes;
            this.posOrNeg = posOrNeg;
        }
    }


    class MutationRule
    {
        public mutateRule rule;
        public int probabilityCount;

        public MutationRule(mutateRule rule, int probabilityCount)
        {
            this.rule = rule;
            this.probabilityCount = probabilityCount;
        }
    }


}

