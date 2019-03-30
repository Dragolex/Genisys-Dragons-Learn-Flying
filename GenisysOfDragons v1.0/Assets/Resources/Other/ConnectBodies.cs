using UnityEngine;
using System.Collections;

/*
 * TEST SCRIPT - OBSOLETE!
 */


public class ConnectBodies : MonoBehaviour {

    public GameObject connectedBody;

    PolygonFactory polygonFactory;

    Vector3[] vertices;

	// Use this for initialization
	void Start () {

        //polygonFactory = (PolygonFactory)this.GetComponent(typeof(PolygonFactory));

        ConnectCertainBodies(this.gameObject, connectedBody);
	
	}

    Mesh polyMesh;


    public void ConnectCertainBodies(GameObject objA, GameObject objB)
    {
        vertices = new Vector3[4];

        vertices[0] = -objA.transform.localScale / 4;
        vertices[1] = objA.transform.localScale / 4;
        vertices[2] = objB.transform.position - objA.transform.position - objB.transform.localScale / 4;
        vertices[3] = objB.transform.position - objA.transform.position + objB.transform.localScale / 4;

        polyMesh = PolygonFactory.CreatePolygonStrip(vertices, objA);


        Vector3 v = new Vector3(3,3,3);

        Vector3[] vertices2 = new Vector3[3];

        vertices2[0] = vertices[0] + v;
        vertices2[1] = vertices[1] + v;
        vertices2[2] = vertices[2] + v;

        MeshRenderer mr = objA.GetComponent<MeshRenderer>();
        PolygonFactory.AddPolygonStrip(vertices2, polyMesh, mr);

        PolygonFactory.AddPolygonStripPoint(vertices[3] + v, polyMesh);


        /*
        PolygonFactory.AddPolygonStripPoint(vertices[1], polyMesh);
        PolygonFactory.AddPolygonStripPoint(vertices[2], polyMesh);
        PolygonFactory.AddPolygonStripPoint(vertices[3], polyMesh);
        */

        /*
        PolygonFactory.ExtendPolygonStrip(vertices, polyMesh);
        */
    }






	// Update is called once per frame
	void Update () {

        //GameObject objA = this.gameObject;
        //GameObject objB = connectedBody;


        /*
        vertices[0] = -objA.transform.localScale / 4;
        vertices[1] = objA.transform.localScale / 4;
        vertices[2] = objB.transform.position - objA.transform.position - objB.transform.localScale / 4;
        vertices[3] = objB.transform.position - objA.transform.position + objB.transform.localScale / 4;
        */
         
        //PolygonFactory.EditPolygonStrip(vertices, polyMesh);

	}

}

