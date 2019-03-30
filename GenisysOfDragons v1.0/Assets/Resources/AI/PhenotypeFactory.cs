using UnityEngine;
using System.Collections.Generic;


/*
 * AI RELATED
 * 
 * The PhenotypeFactory creates new phenotypes with randomized chromosomes.
 * The values are fixed but note that each second value is a "range" factor providing pretty much variance.
 */


public class PhenotypeFactory {

    // Distance between the wing-roots - like the shoulder width of the dragon
    static float wingRootDist = 1.5f;



    // Sets the chromosomes for a non-randomized standard-phenotype (mostly for demonstration purpsoe)
    static public Phenotype CreateStandard()
    {
        Chromosome rootChromL = new Chromosome(2, new Vector3(-(wingRootDist + 0.3f), -0.3f, -1.5f), Global.quatLeft, 0.6f, 0.4f);
        Chromosome rootChromR = new Chromosome(2, new Vector3(wingRootDist + 0.3f, -0.3f, -1.5f), Global.quatRight, 0.6f, 0.4f);

        return (new Phenotype(rootChromL, rootChromR, getStandardChromosomes(), 1.5f, 0, 210, 0));
    }

    static public Phenotype CreateForDemoDragon()
    {
        Chromosome rootChromL = new Chromosome(2, new Vector3(-1f, 1.5f, 0.5f), Global.quatLeft, 1.25f, 0.4f);
        Chromosome rootChromR = new Chromosome(2, new Vector3(1f, 1.5f, 0.5f), Global.quatRight, 1.25f, 0.4f);

        return (new Phenotype(rootChromL, rootChromR, getStandardChromosomes(), 1.5f, 0, 210, 0));
    }

    static public Phenotype CreateRandom()
    {
        Chromosome rootChromL = new Chromosome(2, new Vector3(-(wingRootDist + 0.3f), -0.3f, -1.5f), Global.quatLeft, 0.6f , 0.4f);
        Chromosome rootChromR = new Chromosome(2, new Vector3(wingRootDist + 0.3f, -0.3f, -1.5f), Global.quatRight, 0.6f, 0.4f);

        return (new Phenotype(rootChromL, rootChromR, getRandomChromosomes(), 2f, 1.25f, 220, 80));
    }


    static List<Chromosome> getStandardChromosomes()
    {
        // Sets the chromosomes for a non-randomized standard-phenotype (mostly for demonstration purpose)

        List<Chromosome> chroms = new List<Chromosome>();

        
        chroms.Add(new Chromosome(0, 90, 0, 3f, 0, 0, 0, 0.35f)); // Parallel bone (anchor on body)
        chroms.Add(new Chromosome(1, 20, 0, 3.2f, 0, 45, 0, 0.4f));


        chroms.Add(new Chromosome(4, -65, 0, 4, 0, 40, 0, 0.1f));
        chroms.Add(new Chromosome(0, -30, 0, 1.5f, 0, 30, 0, 0.15f));

        chroms.Add(new Chromosome(1, 50, 0, 3f, 0, 15, 0, 0.1f));
        chroms.Add(new Chromosome(1, 12, 0, 3.5f, 0, 20, 0, 0.15f));
        chroms.Add(new Chromosome(0, 12, 0, 3.5f, 0, 20, 0, 0.2f));

        chroms.Add(new Chromosome(1, 85, 0, 4f, 0, -5, 0, 0.1f));
        chroms.Add(new Chromosome(1, 15, 0, 3f, 0, 20, 0, 0.2f));
        chroms.Add(new Chromosome(0, 10, 0, 2.5f, 0, 25, 0, 0.25f));

        chroms.Add(new Chromosome(1, 130, 0, 4.5f, 0, -10, 0, 0.15f));
        chroms.Add(new Chromosome(0, 10, 0, 3.5f, 0, 45, 0, 0.2f));

        return (chroms);
    }



    // RANDOM PHENOTYPE


    static List<Chromosome> getRandomChromosomes()
    {
        // Sets the chromosomes for a randomized phenotype

        List<Chromosome> chroms = new List<Chromosome>();


        chroms.Add(new Chromosome(0, 90, 0, 3f, 2, 0, 0, 0.35f)); // Parallel bone (anchor on body)
        chroms.Add(new Chromosome(1, 20, 30, 3.2f, 1.5f, 45, 20, 0.4f));


        chroms.Add(new Chromosome(4, -65, 50, 4, 2, 40, 20, 0.1f));
        chroms.Add(new Chromosome(0, -30, 40, 1.5f, 0.75f, 30, 20, 0.15f)); // First "finger"

        chroms.Add(new Chromosome(1, 50, 30, 3f, 1.5f, 15, 10, 0.1f)); // Second "finger" segment 1
        chroms.Add(new Chromosome(1, 12, 10, 3.5f, 2, 20, 10, 0.15f)); // Second "finger" segment 2
        chroms.Add(new Chromosome(0, 12, 10, 3.5f, 2, 20, 10, 0.2f)); // Second "finger" segment 3

        chroms.Add(new Chromosome(1, 85, 60, 4f, 2, -5, 10, 0.1f)); // Third "finger" segment 1
        chroms.Add(new Chromosome(1, 15, 10, 3f, 2, 20, 10, 0.2f)); // Third "finger" segment 2
        chroms.Add(new Chromosome(0, 10, 15, 2.5f, 1, 25, 20, 0.25f)); // Third "finger" segment 3

        chroms.Add(new Chromosome(1, 130, 70, 4.5f, 2.5f, -10, 20, 0.15f)); // Fourth "finger" segment 1
        chroms.Add(new Chromosome(0, 10, 20, 3.5f, 2, 45, 25, 0.2f)); // Fourth "finger" segment 2


        // IMPRTANT! For an explanation why this finger-structure seems hardcoded like that, please read in the script "GlobalSettings" before the definition of "standardWingDesignIndices".


        return (chroms);

    }



    static private float ranToHalf(float value)
    {
        return (Random.Range(0.5f * value, value));
    }


    static private float ranAround(float value)
    {
        return (Random.Range(0.65f * value, 1.35f * value));
    }

}

