using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * NOT AIR RELATED
 * 
 * Unity lacks functions of actually drawing primitives, etc. dorectly onto the screen.
 * Therefore I wrote this surface class based on the principles I've learned when I used the "Game Maker" in past.
 * 
 * The idea is to have so called "surfaces", a datastructure which can be set to be the current drawing-target through "SetTarget(surf)".
 * While a surface has a target, the static functions of "Surface" can be used to draw primitives onto it.
 * That's comparably efficient because the pixeldata is only updated back tot he graphic card when the "ResetTarget()" function is called.
 * 
 * Here it is only used for dispalying the observation-graph at the bottom of the screen.
 */


public class Surface {

    // Non-Static part
    Texture2D texture;
    Color32[] pixelSet;

    public Surface(int width, int height) // private
    {
         texture = new Texture2D(width, height);
    }


    // Static part

    //static Surface currentSurface;
    static Texture2D currentTexture;
    static Color32[] currentPixelSet;
    static int currentWidth;
    static int currentHeight;

    static Surface StandardSurface = null;

    //static List<Surface> surfaces = new List<Surface>();



    public static void SetTarget(Surface surf)
    {
        if (surf == null) return;

        //currentSurface = surf;
        currentTexture = surf.texture;
        currentPixelSet = currentTexture.GetPixels32();
        currentWidth = currentTexture.width;
        currentHeight = currentTexture.height;
    }

    public static void ResetTarget()
    {
        currentTexture.SetPixels32(currentPixelSet);
        currentTexture.Apply();

        SetTarget(Surface.StandardSurface);
    }



    public static void DrawLine(float x, float y, float tx, float ty, Color32 col)
    {
        DrawLine((int)x, (int)y, (int)tx, (int)ty, col);
    }
    public static void DrawLine(int x, int y, int tx, int ty, Color32 col)
    {
        x = Mathf.Clamp(x, 0, currentWidth - 1);
        y = Mathf.Clamp(y, 0, currentHeight - 1);
        tx = Mathf.Clamp(tx, 0, currentWidth - 1);
        ty = Mathf.Clamp(ty, 0, currentHeight - 1);

        tx -= 1;
        ty = (currentHeight - 1) - ty;
        y = (currentHeight - 1) - y;


        float dist = pointDistance(x, y, tx, ty);

        float adx = (tx - x)/dist;
        float ady = (ty - y)/dist;

        if (col.a == 255)
        {
            for (int p = 0; p < dist; p++)
                currentPixelSet[(int)((Mathf.Floor(y + p * ady) * currentWidth) + (Mathf.Floor(x + p * adx)))] = col;
        }

        if (col.a < 1)
            for (int p = 0; p < dist; p++)
            {
                int pos = (int)((Mathf.Round(y + p * ady) * currentWidth) + (Mathf.Round(x + p * adx)));
                currentPixelSet[pos] = new Color32((byte)(currentPixelSet[pos].r * currentPixelSet[pos].a + col.r * col.a), (byte)(currentPixelSet[pos].g * currentPixelSet[pos].a + col.g * col.a), (byte)(currentPixelSet[pos].b * currentPixelSet[pos].a + col.b * col.a), (byte)(currentPixelSet[pos].a + col.a));
            }
    }



    public static void DrawRect(float x, float y, float tx, float ty, Color32 col, bool lineOnly)
    {
        DrawRect((int) x, (int) y, (int) tx, (int) ty, col, lineOnly);
    }
    public static void DrawRect(int x, int y, int tx, int ty, Color32 col, bool lineOnly)
    {
        x = Mathf.Min(x, currentWidth - 1);
        tx -= 1;
        int aty = Mathf.Min(ty, currentHeight - 1) - 1;

        ty = (currentHeight - 1) - y;
        y = (currentHeight - 1) - aty;

        if (lineOnly)
        {

            for (int p = x; p < tx; p++)
                currentPixelSet[y*currentWidth + p] = col;

            for (int p = x; p < tx; p++)
                currentPixelSet[ty * currentWidth + p] = col;


            for (int p = y * currentWidth; p < ty*currentWidth; p += currentWidth)
                currentPixelSet[x + p] = col;

            for (int p = y * currentWidth; p < ty * currentWidth; p += currentWidth)
                currentPixelSet[tx + p] = col;

        }
        else
        {
            int targ = tx + ty * currentWidth;
            int lineAdd = currentWidth-tx + x-1;

            for (int p = x + y * currentWidth; p < targ; p++)
            {
                if ((p % currentWidth) > tx)
                    p += lineAdd;

                currentPixelSet[p] = col;
                //currentPixelSet[p] = new Color32((byte)(currentPixelSet[p].r * currentPixelSet[p].a + col.r * col.a), (byte)(currentPixelSet[p].g * currentPixelSet[p].a + col.g * col.a), (byte)(currentPixelSet[p].b * currentPixelSet[p].a + col.b * col.a), (byte)(currentPixelSet[p].a + col.a));
            }
        }
    }


    public static Surface Create(int width, int height)
    {
        return(Create(width, height, Color.clear));
    }

    public static Surface Create(int width, int height, Color col)
    {
        Surface currentSurface = new Surface(width, height);

        SetTarget(currentSurface);
        Clear(col);
        ResetTarget();

        return (currentSurface);
    }

    public static void Clear()
    {
        Clear(Color.clear);
    }
    public static void Clear(Color col)
    {
        int len = currentPixelSet.Length;
        for (int i = 0; i < len; i++)
            currentPixelSet[i] = col;
    }



    public static void Draw(Surface surf, float x, float y)
    {
        //surf.texture.SetPixel();

        Rect pos = new Rect(x, y, surf.texture.width, surf.texture.height);

        GUI.DrawTexture(pos, surf.texture);
    }



    // private static
    static private float pointDistance(int x, int y, int tx, int ty)
    {
        return (Mathf.Sqrt(((x - tx) * (x - tx) + (y - ty) * (y - ty))));
    }


}
