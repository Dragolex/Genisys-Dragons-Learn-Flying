using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * AI RELATED
 * 
 * This class represents joints and segments of the wing.
 * When created as non-root chromosomes, a set of ranges is applied, providing the randomized start values of each chromosome.
 * 
 * See the "Phenotype" class for usage.
*/


public class Chromosome
{
    // Number of other wing-segments which can branch off from this bone (0 for an end)
    public int numOfJoints;

    // Length
    public float boneLength;
    // Thickness
    public float boneThickness;
    // Offset for not overlapping entirely
    public Vector3 boneOffset;
    // Rotation relative to the bone it's achored on.
    public Quaternion boneAddRot;

    // Limits for the muscle movement
    public int muscleMinLimit;
    public int muscleMaxLimit;

    // Force the muscle can apply to move the segment
    // For the calculation of this force (as the function below initializes it with 0) look into "calculateMuscles()" in the script "Phenotype".
    public float muscleForce;

    // factor between 0 and 1 which tells when during a complete wing flap, the direction change of the muscle-forces happens. that#s required to avoid that all elements move in sync and don#t produce any levitation on average.
    public float muscleFlapStep;

    // Color (only used when rendering)
    public Color col;


    // Creation
    public Chromosome(int joints, float angle, float angleRange, float length, float lengthRange, int muscleLimit, int muscleLimitRange, float muscleFlapStep)
    {
        numOfJoints = joints;

        boneLength = length + Random.Range(-lengthRange, lengthRange);
        boneThickness = 0.1f;

        boneAddRot = Quaternion.Euler(Vector3.up * (angle + Random.Range(-angleRange, angleRange)));
        boneOffset = boneAddRot * new Vector3(0, 0, 0.2f);


        this.muscleMinLimit = -muscleLimit + Random.Range(-muscleLimitRange, muscleLimitRange);
        this.muscleMaxLimit = muscleLimit + Random.Range(-muscleLimitRange, muscleLimitRange);
        if (muscleLimit < 0) muscleMinLimit += 4 * muscleLimit;


        this.muscleFlapStep = (muscleFlapStep - (muscleFlapStep / 2) + Random.value * muscleFlapStep) % 1;

        muscleForce = 0;
    }


    

    // ROOT chromosomes
    public Chromosome(int joints, Vector3 boneOffset, Quaternion boneAddRot, float length, float muscleFlapStep)
    {
        numOfJoints = joints;

        boneLength = length;
        boneThickness = 0.4f;

        this.boneAddRot = boneAddRot;
        this.boneOffset = boneOffset;

        muscleMinLimit = 0;
        muscleMaxLimit = 0;

        this.muscleFlapStep = muscleFlapStep;

        muscleForce = GlobalSettings.dragonAvailableForce;
    }

}

