using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * NOT DIRECTLY AI RELATED but required for SIMULATION
 * 
 * This factory basicly handles the existance of the "wings" of testsamples on the world.
 * For structural reasons, the instantiation of wing-elements remained in the "DragonFactory"
 * 
 * The keytasks here is the creation and attachment of a "WingSet", the script which handles physic calculations.
 */


public class WingSetFactory : MonoBehaviour {

    // Activated wings
    static public List<WingSet> activeWings = new List<WingSet>();


    // Adds a new wingset and keeps track of it
    static public void AddWing(GameObject body, Phenotype phenotype, Rigidbody[] wingSegments, HingeJoint[] wingJoints)
    {
        Rigidbody[] wingPolygons = createWingPolygons(wingSegments, GlobalSettings.standardWingDesignIndices);
        WingSet wingSet = new WingSet(body, phenotype, wingPolygons, wingJoints);

        body.GetComponent<DragonScript>().wingSet = wingSet;

        wingSet.RenderVisualisation(((activeWings.Count < GlobalSettings.maxRenderedMembranes) && GlobalSettings.renderingMembranes));

        activeWings.Add(wingSet);
    }


    static public void DestroyWing(GameObject body)
    {
        DragonScript scr = body.GetComponent<DragonScript>();

        activeWings.Remove(scr.wingSet);
        scr.wingSet.Destroy();
        scr.wingSet = null;
    }
    static public void DestroyWing(WingSet wingSet)
    {
        activeWings.Remove(wingSet);
        wingSet.Destroy();
        wingSet = null;
    }


    void FixedUpdate()
    {
        // Update the rendering of all wings if membranes are enabled but not the phenotypes
        if (GlobalSettings.renderingMembranes)
        if ((!GlobalSettings.renderingBodyAndBones) || (Global.currentScene == Global.cliffScene))
        {
            int len = activeWings.Count;
            for (int ind = 0; ind < len; ind++)
                activeWings[ind].UpdateVisualisation();
        }
    }


    static Rigidbody[] createWingPolygons(Rigidbody[] wing, sbyte[] indices)
    {
        int half = wing.Length / 2;
        int indlen = indices.Length;
        int i = 0;

        Rigidbody[] wingPolygons = new Rigidbody[indlen * 2];

        for (int ind = 0; ind < indlen; ind++)
        {
            wingPolygons[i] = wing[indices[ind]];
            wingPolygons[indlen + i] = wing[indices[ind] + half];
            i++;
            if (indices[ind+1] == -1)
            {
                wingPolygons[i] = null;
                wingPolygons[indlen + i] = null;
                i++;
                ind++;
            }
        }

        return (wingPolygons);
    }


    static public PolygonSet VisualizeWingMembrane(Rigidbody[] wingSegments)
    {
        List<Vector3> polys = new List<Vector3>();
        PolygonSet polyset = new PolygonSet();

        int len = wingSegments.Length;


        for (int ind = 0; ind < len; ind++)
        {
            if (wingSegments[ind] == null)
            {
                polyset.AddPolygonStrip(polys.ToArray());
                polys.Clear();
            }
            else polys.Add(wingSegments[ind].transform.position);
        }

        polyset.SetColor(new Color(0.7f, 0.7f, 0.3f, 0.75f));

        return (polyset);
    }

}
