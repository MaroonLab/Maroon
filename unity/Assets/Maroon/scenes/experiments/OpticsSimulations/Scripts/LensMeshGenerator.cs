﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maroon.Physics { }

[ExecuteInEditMode]
public class LensMeshGenerator : MonoBehaviour
{
    // Start is called before the first frame update


    MeshFilter testmeshfilter;
    //[Range(0.0f, 4.0f)]
    //public float testfloat;
    //[Range(1.01f, 20.0f)]

    public Vector3 offset_vec = new Vector3(1.0f, 0, 0);
    [Range(0, 3)]
    public float cylinderthickness = 0.0f;
    private float cylinderthickness_old = 0.0f;
    private float circradius = 3.0f;
    //[Range(-1, 1)]
    [Range(-1, 1)]
    public float radcalc = 0;

    [Range(-1, 1)]
    
    public float radcalc2 = 0;
    private float circradius2 = 3.0f;

    [Range(0.2f, 0.99f)]
    public float lensRadius =0.99f;
    private float lensRadius_old;

    private float radcalc_old = 0;
    private float radcalc_old2 = 0;


    // not to change ingame
    [Range(3, 50)]
    public int sectionPoints = 10;
    [Range(3, 50)]
    public int domeSegments = 10;
    [HideInInspector]
    public OpticsLens thisLensLens;

    [SerializeField]
    public float Testfloat
    {
        get => Testfloat;
        set
        {
            cylinderthickness = value;
        }
    }

    [SerializeField]
    public float radcalcset
    {
        get => radcalcset;
        set
        {
            radcalc = value;
        }
    }

    [SerializeField]
    public float radcalc2set
    {
        get => radcalc2set;
        set
        {
            radcalc2 = value;
        }
    }

    [SerializeField]
    public float lensradset //0.2f -0.99f
    {
        get => lensradset;
        set
        {
            lensRadius = value;
        }
    }




    void Start()
    {
        if(gameObject.GetComponent<MeshFilter>() != null)
        {
            testmeshfilter = gameObject.GetComponent<MeshFilter>();
        }
        else
        {
            testmeshfilter = gameObject.AddComponent<MeshFilter>();
        }

        if(gameObject.GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }

    }



    float calcRadiusFromTopedge(float edgeinput, float lensradius)
    {

        // tan alpha = lensradius/ edgeinput
        float edgeinp_sav = edgeinput;

        // clamp forbidden values
        if (edgeinput >= 0.0f && edgeinput < 0.01f) edgeinput = 0.01f;
        if (edgeinput < 0.0f && edgeinput > -0.01f) edgeinput = -0.01f;
        if (edgeinput > 0.999f) edgeinput = 0.999f;
        if (edgeinput < -0.999f) edgeinput = -0.999f;

        float alpha = Mathf.Atan(lensradius / edgeinput) * Mathf.Rad2Deg; // todo there is most likely a problem there
        //2ndtriangle = 90 - (180 - 90 - alpha)

        // segmentr = 90 - 2ndtriangle

        float radiusangle =  Mathf.Abs(alpha)*2 -90.0f;
        // radius = lensradius / sin ( segmentr)

        float radius = lensradius / Mathf.Sin((radiusangle + 90) *Mathf.Deg2Rad); 
        if (edgeinp_sav >= 0.0f) radius = radius * -1.0f;
        return radius;
    }



    float calcSectionAngle(float radius, float lensradius)
    {
        //return Mathf.Rad2Deg *Mathf.Atan2(1.0f, radius); // hardcoded, lenssize could be bigger/smaller
        float angle=  Mathf.Rad2Deg* Mathf.Asin((lensradius/ radius));
        return angle;

    }


    //gets points of arc, starting at (0,0) and ending at (x, y(1.0))
    Vector3[] getSectionPoints(int numpoints, float sectionangle, float radius, float circrad)
    {
        Vector3 radvec = new Vector3(radius, 0.0f, 0.0f);

        //radvec = Quaternion.Euler(0, sectionangle / 10.0f, 0)*radvec; // what axis? 

        Vector3[] points = new Vector3[numpoints];
        //iterate over all points
        for(int i = 0; i < numpoints; i++)
        {
            //i+1/numpoints
            points[i] = Quaternion.Euler(0, sectionangle * (i + 1) / numpoints, 0) * radvec;
            points[i].x = points[i].x - circrad;
        }


        // return list of points in array

        //afterwards rotate whole section and connect them together

        return points;
    }

    List<Vector3[]> getDomeVertices(Vector3[] slice, int numberofribs)
    {
        


        float rotAngle = 360 / numberofribs;

        // numberofribs - 1
        List<Vector3[]> dome = new List<Vector3[]>();
        dome.Add(slice);
        //create copies and rotate around the rib
        for (int i = 0; i < numberofribs -1; i++)
        {

            // new list, rotate
            Vector3[] rotated = (Vector3[])slice.Clone();

            //rotate
            for( int j = 0; j< rotated.Length; j++)
            {
                rotated[j]= Quaternion.Euler(rotAngle* (i+1), 0, 0) * rotated[j];
            }

            dome.Add(rotated);
        }

        return dome;
    }


    //makemesh function
    // also return vertex positions of outer ring
    (List<Vector3>, List<int>) makeFacesAndVerts(List<Vector3[]> dome)
    {
        Vector3 startVec = new Vector3(0.0f, 0.0f, 0.0f);

        //startVec.x = 3.0f;

        int dcount = dome[0].Length;

        List<Vector3> vertBuffer = new List<Vector3>();
        List<Vector3> normBuffer = new List<Vector3>();
        List<int> trisBuffer = new List<int>();
        vertBuffer.Add(startVec);
        //filled up list of verts
        for(int i = 0; i < dome.Count; i++)
        {
            vertBuffer.AddRange(dome[i]);
            //add normals here
        }
        for(int i = 0; i < dome.Count; i++)
        {
            //make middle tri
            trisBuffer.Add(0);
            trisBuffer.Add((i + 1)%dome.Count * dcount + 1);
            trisBuffer.Add(i * dcount + 1);
            
            for(int j = 0; j < dcount-1; j++)
            {
                //fill the rest of the triangles
                trisBuffer.Add((i + 1) % dome.Count * dcount + j + 1);
                trisBuffer.Add(i * dcount + 2 + j);
                trisBuffer.Add(i * dcount + 1 + j);


                trisBuffer.Add((i + 1) % dome.Count * dcount + j + 2);
                trisBuffer.Add(i * dcount + 2 + j);
                trisBuffer.Add((i + 1) % dome.Count * dcount + j + 1);

            }
        }
        return (vertBuffer, trisBuffer);
    } 


    (float, float) calcLensPos(float leftdome, float rightdome, float additionalthickness)
    {
        // calculate the minimum safe distance between the 2 midpoints
        // additionalthickness is thickness @ edge of lens

        float domediff = leftdome - rightdome;
        float lensdistance = 0.0f;
        float avgoffset = (leftdome + rightdome) / 2.0f;

        if(domediff > 0.0f)
        {
            // then outer edge meets
            // domediff is the difference we have to separate the 2 midpoints from each other to get the outer edge to be flush
            // add the additional thickness
            lensdistance = domediff + additionalthickness;
        }
        else
        {
            // inner edges meet
            // domes just need to be moved so that the outer edges are symmetrical.

            // domediff is the value how much outer edges are separated. additionalthickness - (-domediff) is the amount we need to separate the inner edges to reach the desired outer edge separation
            if(-domediff > additionalthickness)
            {
                lensdistance = 0.0f;
            }
            else
            {
                lensdistance = additionalthickness + domediff;
            }
        }
        return (lensdistance, avgoffset);
    }

    (List<Vector3>,List<int>) makeDomeConnection(List<Vector3> stackedDomeVerts)
    {
        //domeSegments, sectionpoints
        //stackeddomeVerts

        List<int> domeConnects = new List<int>();
        List<Vector3> dupliRingVerts = new List<Vector3>();
        int startfirst = sectionPoints;
        int startsecond = sectionPoints + sectionPoints*domeSegments + 1;

        int firstlenscurrvert = startfirst;
        int secondlenscurrvert = startsecond;

        dupliRingVerts.Add(stackedDomeVerts[firstlenscurrvert]);
        dupliRingVerts.Add(stackedDomeVerts[secondlenscurrvert]);


        for(int i = 0; i < domeSegments; i++)
        {
            firstlenscurrvert += sectionPoints;
            secondlenscurrvert += sectionPoints;

            if(! (i == domeSegments - 1) )
            {
                dupliRingVerts.Add(stackedDomeVerts[firstlenscurrvert]);
                dupliRingVerts.Add(stackedDomeVerts[secondlenscurrvert]);

                domeConnects.Add(2 * i);
                domeConnects.Add(2 * i + 2);
                domeConnects.Add(2 * i + 1);
                

                domeConnects.Add(2 * i + 1);
                domeConnects.Add(2 * i + 2);
                domeConnects.Add(2 * i + 3);
                
            }
            else
            {
                domeConnects.Add(2 * i);
                domeConnects.Add(0);
                domeConnects.Add(2 * i + 1);
                

                domeConnects.Add(2 * i + 1);
                domeConnects.Add(0);
                domeConnects.Add(1);
                
            }
        }
        //return dupliringverts, return domeconnects
        return (dupliRingVerts, domeConnects);
    }

    public OpticsLens getOpticsLens(float distance, float offset, float rad1, float rad2)
    {
        OpticsLens toReturn;

        if (rad1 < 0) toReturn.leftLeftSegment = true;
        else toReturn.leftLeftSegment = false;
        if (rad2 < 0) toReturn.rightLeftSegment = true;
        else toReturn.rightLeftSegment = false;

        toReturn.leftCircle.radius = Mathf.Abs(rad1);
        toReturn.rightCircle.radius = Mathf.Abs(rad2);
        toReturn.radius =  0.5f;
        toReturn.leftCircle.midpoint = new Vector2(-distance / 2.0f - offset - rad1, 0.0f);
        toReturn.rightCircle.midpoint = new Vector2(+distance / 2.0f - offset - rad2, 0.0f);
        toReturn.innerRefractiveidx = 1.0f;
        toReturn.outerRefractiveidx = 1.0f;
        toReturn.leftbound = 0.0f;
        toReturn.rightbound = 0.0f;
        return toReturn;
    }

    // Update is called once per frame
    void Update()
    {
        //check for lens update.
        if (Mathf.Approximately(radcalc, radcalc_old) && Mathf.Approximately(radcalc2, radcalc_old2) && Mathf.Approximately(cylinderthickness, cylinderthickness_old) && Mathf.Approximately(lensRadius, lensRadius_old))
        {
            return;
        }
        else
        {
            radcalc_old = radcalc;
            radcalc_old2 = radcalc2;
            lensRadius_old = lensRadius;
            cylinderthickness_old = cylinderthickness;
        }
        Mesh mymesh = new Mesh();

        (float lensdist, float averageoffset) = calcLensPos(radcalc*lensRadius, radcalc2*lensRadius, cylinderthickness);

        float radiuss = calcRadiusFromTopedge(radcalc*lensRadius, lensRadius);
        float radiuss2 = calcRadiusFromTopedge(radcalc2*lensRadius, lensRadius);
        circradius = radiuss;
        circradius2 = radiuss2;

        thisLensLens = getOpticsLens(lensdist, averageoffset, radiuss, radiuss2);

        float sectionangle = calcSectionAngle(circradius, lensRadius); //circradius
        float sectionangle2 = calcSectionAngle(circradius2, lensRadius);


        var pts = getSectionPoints(sectionPoints, sectionangle, circradius, circradius); //circradiuss
        var pts2 = getSectionPoints(sectionPoints, sectionangle2, circradius2, circradius2);


        // first vertex is 0 0 0 
        var domeL = getDomeVertices(pts, domeSegments);
        (List<Vector3> verts, List<int> tris) = makeFacesAndVerts(domeL);
        //Debug.Log("vertct trict " + verts.Count + " " + tris.Count);

        var domeL2 = getDomeVertices(pts2, domeSegments);
        (List<Vector3> verts2, List<int> tris2) = makeFacesAndVerts(domeL2);

        List<Vector3> lensmeshverts = new List<Vector3>();
        List<int> lensmeshtris = new List<int>();
        lensmeshtris.AddRange(tris);

        float leftlensdisplacement = -lensdist / 2.0f - averageoffset;
        float rightlensdisplacement = +lensdist / 2.0f - averageoffset;

        Vector3 leftlensmidpoint = new Vector3(leftlensdisplacement - circradius, 0.0f, 0.0f);
        Vector3 rightlensmidpoint = new Vector3(rightlensdisplacement - circradius2, 0.0f, 0.0f);
        List<Vector3> normals1 = new List<Vector3>(verts.Count);
        List<Vector3> normals2 = new List<Vector3>(verts.Count);

        List<Vector3> lensmeshnormals = new List<Vector3>();

        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] += new Vector3(leftlensdisplacement, 0.0f, 0.0f);
            normals1.Add(-(verts[i] - leftlensmidpoint).normalized);
        }
        lensmeshverts.AddRange(verts);
        for (int i = 0; i < verts2.Count; i++)
        {
            verts2[i] += new Vector3(rightlensdisplacement, 0.0f, 0.0f);
            normals2.Add( -(verts2[i] - rightlensmidpoint).normalized);
        }

        lensmeshverts.AddRange(verts2);


        //List<int> tris = new List<int>();
        //Debug.Log(tris.Count);

        for(int i = 0; i < tris2.Count; i++)
        {
            tris2[i] += verts.Count;
            //flip normals of 2nd
            if(i%3== 0)
            {
                int temppoly = tris2[i + 1];
                tris2[i + 1] = tris2[i + 2];
                tris2[i + 2] = temppoly; 
            }
        }
        lensmeshtris.AddRange(tris2);

        (var additionalverts, var additionaltris)=makeDomeConnection(lensmeshverts);

        //make normalvectors for the additionalvertices

        Vector3 norm_firstdome = new Vector3(additionalverts[0].x, 0.0f, 0.0f);
        Vector3 norm_seconddome = new Vector3(additionalverts[1].x, 0.0f, 0.0f);
        List<Vector3> additionalnormals = new List<Vector3>();
        for(int i = 0; i < additionalverts.Count; i+=2)
        {
            additionalnormals.Add(additionalverts[i] - norm_firstdome);
            additionalnormals.Add(additionalverts[i + 1] - norm_seconddome);
        }

        for(int i = 0; i< additionaltris.Count; i++) {
            additionaltris[i] += lensmeshverts.Count;
        }
        lensmeshverts.AddRange(additionalverts);
        lensmeshtris.AddRange(additionaltris); //connect the 2 lenscaps

        lensmeshnormals.AddRange(normals1);
        lensmeshnormals.AddRange(normals2);
        lensmeshnormals.AddRange(additionalnormals);

        //Debug.Log("vertct trict " + lensmeshverts.Count + " " + lensmeshtris.Count);
        //adding  first verts 
        // Set vertices and triangles to the mesh
        //mymesh.vertices = vertices;
        //mymesh.triangles = triangles;
        //mymesh.SetVertices(verts);
        //mymesh.SetTriangles(tris, 0);
        

        //swap tris of second half

        List<int> invertedtris = new List<int>(lensmeshtris);

        for(int i= 0; i < invertedtris.Count; i += 3)
        {
            int temp = invertedtris[i];
            invertedtris[i] = invertedtris[i+1];
            invertedtris[i + 1] = temp;
        }

        List<int> duplicatedtris = new List<int>();
        duplicatedtris.AddRange(lensmeshtris);
        duplicatedtris.AddRange(invertedtris);

        //List<Vector3> duplicatednormals = new List<Vector3>();
        //duplicatednormals.AddRange(lensmeshnormals);



        mymesh.SetVertices(lensmeshverts);
        mymesh.SetTriangles(lensmeshtris, 0);
        mymesh.SetNormals(lensmeshnormals);
        if(testmeshfilter == null) // not particularly beautiful, but it works
        {
            //add mesh filter if not added already
            testmeshfilter = gameObject.GetComponent<MeshFilter>();

        }
        else
        {
            testmeshfilter.mesh = mymesh;
            mymesh.RecalculateNormals();
        }
    }
}
