using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * NOT AI RELATED
 * 
 * This script and its datastructures provide a flexible system for having polygons drawn on the screens and udpate them in relatime.
 * It's not very flexible yet and has some weird shading issues, but suffices for the membranes of the wings.
 */


public class PolygonSet
{
    GameObject polygonObject;
    public Mesh mesh;
    MeshRenderer renderer;


    public PolygonSet()
    {
        initPolygonSet(PolygonFactory.yellowSurface);
    }
    public PolygonSet(GameObject obj)
    {
        initPolygonSet(obj, PolygonFactory.yellowSurface);
    }
    public PolygonSet(Vector3[] polys)
    {
        initPolygonSet(PolygonFactory.yellowSurface);
        AddPolygonStrip(polys);
    }
    public PolygonSet(Vector3[] polys, Color col)
    {
        initPolygonSet(PolygonFactory.yellowSurface);
        AddPolygonStrip(polys);
        SetColor(col);
    }
    public PolygonSet(Material material)
    {
        initPolygonSet(material);
    }
    public PolygonSet(GameObject obj, Material material)
    {
        initPolygonSet(obj, material);
    }

    public PolygonSet(Material material, Vector3[] polys)
    {
        initPolygonSet(material);
        AddPolygonStrip(polys);
    }
    public PolygonSet(Material material, Vector3[] polys, Color col)
    {
        initPolygonSet(material);
        AddPolygonStrip(polys);
        SetColor(col);
    }
    public PolygonSet(GameObject obj, Material material, Vector3[] polys)
    {
        initPolygonSet(obj, material);
        AddPolygonStrip(polys);
    }
    public PolygonSet(GameObject obj, Material material, Vector3[] polys, Color col)
    {
        initPolygonSet(obj, material);
        AddPolygonStrip(polys);
        SetColor(col);
    }



    void initPolygonSet(Material material)
    {
        polygonObject = new GameObject(); //"FreeMesh", typeof(MeshFilter), typeof(MeshRenderer));

        MeshFilter mf = polygonObject.AddComponent<MeshFilter>();
        mesh = new Mesh();
        mf.mesh = mesh;

        renderer = polygonObject.AddComponent<MeshRenderer>();
        renderer.material = material;
    }
    void initPolygonSet(GameObject obj, Material material)
    {
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf == null) mf = obj.AddComponent<MeshFilter>();
        mesh = new Mesh();
        mf.mesh = mesh;

        renderer = obj.GetComponent<MeshRenderer>();
        if (renderer == null) renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = material;

        polygonObject = null;
    }





    public void AddPolygonStrip(Vector3[] polys)
    {
        int len = polys.Length - 2;

        if (mesh.subMeshCount == 1)
        {
            //mf.mesh.uv = uv;

            int[] tri = new int[mesh.triangles.Length + len*3];
            mesh.triangles.CopyTo(tri, 0);
            tri = calculateTriangles(tri, mesh.triangles.Length, mesh.vertices.Length);

            Vector3[] vert = new Vector3[mesh.vertices.Length + polys.Length];
            mesh.vertices.CopyTo(vert, 0);
            polys.CopyTo(vert, mesh.vertices.Length);
            mesh.vertices = vert;

            mesh.triangles = tri;

            mesh.RecalculateNormals();
        }
    }

    public void SetColor(Color col)
    {
        Color[] colArray = new Color[mesh.vertices.Length];
        for (int ind = 0; ind < colArray.Length; ind++)
            colArray[ind] = col;

        mesh.colors = colArray;
    }


    public void EditPolygons(Vector3[] polys)
    {
        if (mesh.vertices.Length != polys.Length)
        {
            int[] triangles = calculateTriangles(polys.Length - 2);

            //mf.mesh.uv = uv;
            mesh.vertices = polys;
            mesh.triangles = triangles;
        }
        else
            mesh.vertices = polys;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }



    public void Destroy()
    {
        if (polygonObject != null)
            MonoBehaviour.Destroy(polygonObject);
    }






    static int[] calculateTriangles(int len)
    {
        return (calculateTriangles(len, 0));
    }
    static int[] calculateTriangles(int len, int startVal)
    {
        int[] triangles = new int[len * 3];

        int sub = 0;
        for (int i = 0; i < len * 3; i++)
        {
            triangles[i] = startVal + i - sub;

            if (i != 0)
                if (((i + 1) % 3) == 0)
                    sub += 2;
        }

        return (triangles);
    }

    static int[] calculateTriangles(int[] triangles, int startPos, int startVal)
    {
        int sub = 0;
        int len = triangles.Length;
        for (int i = startPos; i < len; i++)
        {
            triangles[i] = startVal + i - startPos - sub;

            if (i != startPos)
                if (((i + 1) % 3) == 0)
                    sub += 2;
        }

        return (triangles);
    }

}




public class PolygonFactory : MonoBehaviour {

    static public Material yellowSurface = (Material)Resources.Load("Other/YellowSurface");

	void Start () {
        PolygonFactory.yellowSurface = (Material)Resources.Load("Other/YellowSurface");
	}


    public static Mesh CreatePolygonStrip(Vector3[] polys, GameObject obj)
    {
        if (obj.GetComponent<MeshFilter>() != null)
        {
            MeshRenderer mre = obj.AddComponent<MeshRenderer>();
            mre.material = yellowSurface;
            Material[] mat = new Material[80];

            for(int ind = 0; ind < 80; ind++)
                mat[ind] = yellowSurface;
            
            mre.materials = mat;


            return(AddPolygonStrip(polys, obj.GetComponent<MeshFilter>().mesh, mre));
        }

        int len = polys.Length - 2;

        Mesh mesh = new Mesh();
        (obj.AddComponent<MeshFilter>()).mesh = mesh;

        MeshRenderer mr = obj.AddComponent<MeshRenderer>();
        mr.material = yellowSurface;
        mr.materials = new Material[] { yellowSurface };

        int[] triangles = calculateTriangles(len);

        //mf.mesh.uv = uv;
        mesh.vertices = polys;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return(mesh);

    }

    public static void EditPolygonStrip(Vector3[] polys, Mesh mesh)
    {
        if (mesh.vertices.Length != polys.Length)
        {
            int[] triangles = calculateTriangles(polys.Length-2);

            //mf.mesh.uv = uv;
            mesh.vertices = polys;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        else
            mesh.vertices = polys;
    }


    public static Mesh AddPolygonStrip(Vector3[] polys, Mesh mesh, MeshRenderer mr)
    {
        int len = polys.Length - 2;
        mesh.subMeshCount++;

        Material[] mat = new Material[mesh.subMeshCount];
        mr.materials.CopyTo(mat, 0);
        mat[mesh.subMeshCount - 1] = yellowSurface;
        mr.materials = mat;


        int[] triangles = calculateTriangles(len, mesh.vertices.Length);

        Vector3[] nv = new Vector3[mesh.vertices.Length + polys.Length];
        mesh.vertices.CopyTo(nv, 0);
        polys.CopyTo(nv, mesh.vertices.Length);


        //mf.mesh.uv = uv;
        mesh.vertices = nv;
        mesh.SetTriangles(triangles, mesh.subMeshCount-1);
        mesh.RecalculateNormals();

        return (mesh);
    }





    //public static Mesh ExtendPolygonStrip(Vector3[] polys)
    //{
    //    return (ExtendPolygonStrip(polys, (this.gameObject.GetComponent<MeshFilter>()).mesh));
    //}
    public static Mesh ExtendPolygonStrip(Vector3[] polys, Mesh mesh)
    {
        int len = polys.Length - 2;

        int[] tri = new int[mesh.triangles.Length + len*3];
        mesh.triangles.CopyTo(tri, 0);
        calculateTriangles(len, mesh.vertices.Length).CopyTo(tri, mesh.triangles.Length);

        Vector3[] nv = new Vector3[mesh.vertices.Length + polys.Length];
        mesh.vertices.CopyTo(nv, 0);
        polys.CopyTo(nv, mesh.vertices.Length);

        //mf.mesh.uv = uv;
        mesh.vertices = nv;
        mesh.triangles = tri;
        mesh.RecalculateNormals();

        return (mesh);
    }

    public static Mesh AddPolygonStripPoint(Vector3 point, Mesh mesh)
    {
        if (mesh.subMeshCount == 1)
        {
            int[] tri = new int[mesh.triangles.Length + 3];
            mesh.triangles.CopyTo(tri, 0);
            tri[mesh.triangles.Length] = mesh.vertices.Length - 2;
            tri[mesh.triangles.Length + 1] = mesh.vertices.Length - 1;
            tri[mesh.triangles.Length + 2] = mesh.vertices.Length;

            Vector3[] nv = new Vector3[mesh.vertices.Length + 1];
            mesh.vertices.CopyTo(nv, 0);
            nv[mesh.vertices.Length] = point;

            //mf.mesh.uv = uv;
            mesh.vertices = nv;
            mesh.triangles = tri;
            mesh.RecalculateNormals();
        }
        else
        {
            int[] tri = new int[mesh.GetTriangles(mesh.subMeshCount - 1).Length + 3];
            mesh.GetTriangles(mesh.subMeshCount - 1).CopyTo(tri, 0);
            tri[mesh.GetTriangles(mesh.subMeshCount - 1).Length] = mesh.vertices.Length - 2;
            tri[mesh.GetTriangles(mesh.subMeshCount - 1).Length + 1] = mesh.vertices.Length - 1;
            tri[mesh.GetTriangles(mesh.subMeshCount - 1).Length + 2] = mesh.vertices.Length;

            Vector3[] nv = new Vector3[mesh.vertices.Length + 1];
            mesh.vertices.CopyTo(nv, 0);
            nv[mesh.vertices.Length] = point;

            //mf.mesh.uv = uv;
            mesh.vertices = nv;
            mesh.SetTriangles(tri,mesh.subMeshCount-1);
            mesh.RecalculateNormals();
        }

        return (mesh);
    }


    public static void SetMeshColor(Mesh mesh, Color col)
    {
        Color[] colArray = new Color[mesh.vertices.Length];
        for(int ind = 0; ind < colArray.Length; ind++)
            colArray[ind] = col;

        mesh.colors = colArray;
    }


    static int[] calculateTriangles(int len)
    {
        return(calculateTriangles(len, 0));
    }
    static int[] calculateTriangles(int len, int startVal)
    {
        int[] triangles = new int[len * 3];

        int sub = 0;
        for (int i = 0; i < len * 3; i++)
        {
            triangles[i] = startVal + i - sub;

            if (i != 0)
                if (((i + 1) % 3) == 0)
                    sub += 2;
        }

        return (triangles);
    }


	
	// Update is called once per frame
	void Update () {
	
	}
}
