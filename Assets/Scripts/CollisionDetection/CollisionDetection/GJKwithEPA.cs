﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GJKwithEPA : CollisionDetection
{
    private static GJKwithEPA instance = null;
    
    /*For GJK only*/
    private static int simplexCount = 0;
    private static int maxGJKLoop = 64;

    /*For EPA only*/
    private struct CSOFace
    {
        public readonly CSOVertex a;
        public readonly CSOVertex b;
        public readonly CSOVertex c;
        public readonly Vector3 normal;
        public readonly float distance;
        public CSOFace(CSOVertex A, CSOVertex B, CSOVertex C)
        {
            a = A;
            b = B;
            c = C;
            normal = Vector3.Cross(b.vertCSO - a.vertCSO, c.vertCSO - a.vertCSO).normalized;
            distance = Vector3.Dot(a.vertCSO,normal);
        }
    };
    private static float epaThreshold = 0.0001f;
    private static int maxEPALoop = 64;
    private static int maxEPAFaces = 64;
    private static int maxEPALooseEdges = 64;

    /*For both GJK and EPA algorithm*/
    private struct CSOVertex
    {
        public readonly Vector3 vertA;
        public readonly Vector3 vertB;
        public readonly Vector3 vertCSO;
        public CSOVertex(Vector3 a, Vector3 b)
        {
            vertA = a;
            vertB = b;
            vertCSO = vertA + vertB;
        }
    };

    private static CSOVertex first,second,third,last;

    private static Vector3 searchDir;

    private static CollisionContact currentContact;

    public static GJKwithEPA CreateInstance()
    {
        if (instance == null)
        {
            instance = new GJKwithEPA();
            return instance;
        }
        else
            return null;
    }

    public override List<CollisionContact> CalculateCollision(List<CollisionPair> pairs)
    {
        List<CollisionContact> contacts = new List<CollisionContact>();
        foreach(CollisionPair pair in pairs)
        {
            if (pair.a._rigidbody == null || pair.b._rigidbody == null ||
                pair.a._rigidbody == pair.b._rigidbody)
                continue;

            currentContact = new CollisionContact();
            currentContact.a = pair.a;
            currentContact.b = pair.b;

            if (GJK(pair.a, pair.b))
            {
                EPA(pair.a, pair.b);
                contacts.Add(currentContact);
            }    
        }
        return contacts;
    }

    private static bool GJK(CustomCollider colliderA, CustomCollider colliderB)
    {
        /*Find the first point of the simplex*/
        searchDir = colliderA.transform.position - colliderB.transform.position;
        third = new CSOVertex(-Support(colliderA, -searchDir),Support(colliderB, searchDir));

        /*Find the second point of the simplex*/
        searchDir = -third.vertCSO;
        second = new CSOVertex(-Support(colliderA, -searchDir),Support(colliderB, searchDir));

        /*If the new simplex point have negative dot value, then the CSO must NOT contain the origin*/
        if (Vector3.Dot(second.vertCSO, searchDir) < 0) 
            return false;

        searchDir = Vector3.Cross(Vector3.Cross(third.vertCSO - second.vertCSO, -second.vertCSO), third.vertCSO - second.vertCSO);
        if (searchDir == Vector3.zero)
        {
            searchDir = Vector3.Cross(third.vertCSO - second.vertCSO, Vector3.right); //normal with x-axis
            if (searchDir == Vector3.zero)
                searchDir = Vector3.Cross(third.vertCSO - second.vertCSO, Vector3.forward); //normal with z-axis
        }
        simplexCount = 2;

        /*run through the iteration.*/
        for(int i = 0; i < maxGJKLoop; i++)
        {
            first = new CSOVertex(-Support(colliderA, -searchDir),Support(colliderB, searchDir));
            if (Vector3.Dot(first.vertCSO, searchDir) < 0)
                return false;
            simplexCount++;
            if (simplexCount == 3)
                UpdateTriangle();
            else if(TetrahedralContainOrigin())
                return true;
        }

        return false;
    }

    private static void UpdateTriangle()
    {
        Vector3 sVect = second.vertCSO - first.vertCSO;
        Vector3 tVect = third.vertCSO - first.vertCSO;
        Vector3 nFirst = -first.vertCSO;
        Vector3 normal = Vector3.Cross(sVect, tVect);

        /*Try reducing the simplex such that it contains the closest point with the lowest dimension.*/
        if(Vector3.Dot(Vector3.Cross(sVect, normal),nFirst) > 0)
        {
            /*The closest point is on edge (first,second).*/
            third = first;
            simplexCount = 2;
            searchDir = Vector3.Cross(Vector3.Cross(sVect, nFirst), sVect);
            return;
        }
        else if(Vector3.Dot(Vector3.Cross(normal, tVect), nFirst) > 0)
        {
            /*The closest point is on edge (first,third).*/
            second = first;
            simplexCount = 2;
            searchDir = Vector3.Cross(Vector3.Cross(tVect, nFirst), tVect);
            return;
        }
        else
        {
            simplexCount = 3;
            /*The closest point is within the triangle.*/
            if (Vector3.Dot(normal,nFirst) > 0)
            {
                /*The origin is above the triangle.*/
                last = third; third = second; second = first;
                searchDir = normal;
                return;
            }
            else
            {
                /*The origin is beneath the triangle.*/
                last = second; second = first;
                searchDir = -normal;
                return;
            }
        }
    }

    private static bool TetrahedralContainOrigin()
    {
        /*First is the top of the tetrahedral.*/
        Vector3 sVect = second.vertCSO - first.vertCSO;
        Vector3 tVect = third.vertCSO - first.vertCSO;
        Vector3 lVect = last.vertCSO - first.vertCSO;
        Vector3 nFirst = -first.vertCSO;
        Vector3 FST = Vector3.Cross(sVect, tVect);
        Vector3 FTL = Vector3.Cross(tVect, lVect);
        Vector3 FLS = Vector3.Cross(lVect, sVect);

        /*  There's only two possible case in this function :
            1. The origin is within the simplex, will go through EPA, simplexCount doesn't matter in this case.
            2. The origin is not within the simplex, will reduce the simplex to a triangle.
            In both case, change the simplexCount = 3 fits its needs.
        */
        simplexCount = 3;

        if(Vector3.Dot(FST, nFirst) > 0)
        {
            last = third; third = second; second = first;
            searchDir = FST;
            return false;
        }
        else if(Vector3.Dot(FTL, nFirst) > 0)
        {
            second = first;
            searchDir = FTL;
            return false;
        }
        else if(Vector3.Dot(FLS, nFirst) > 0)
        {
            third = last; last = second; second = first;
            searchDir = FLS;
            return false;
        }
        return true;
    }

    private static void EPA(CustomCollider colliderA, CustomCollider colliderB)
    {
        CSOFace[] faces = new CSOFace[maxEPAFaces];

        faces[0] = new CSOFace(first,second,third);
        faces[1] = new CSOFace(first,third,last);
        faces[2] = new CSOFace(first,last,second);
        faces[3] = new CSOFace(second,last,third);

        int numFaces = 4;
        int closestFace = 0;

        for (int iterations = 0; iterations < maxEPALoop; iterations++)
        {
            float min_dist = faces[0].distance;
            closestFace = 0;
            for (int i = 1; i < numFaces; i++)
            {
                float dist = faces[i].distance;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    closestFace = i;
                }
            }

            searchDir = faces[closestFace].normal;
            CSOVertex newVert =  new CSOVertex(-Support(colliderA, -searchDir), Support(colliderB,searchDir));

            if (Vector3.Dot(newVert.vertCSO, searchDir) - min_dist < epaThreshold)
            {
                CalculateContactInfo(colliderA, colliderB, faces[closestFace]);
                return;
            }

            CSOVertex[,] looseEdges = new CSOVertex[maxEPALooseEdges,2];
            int looseEdgeNum = 0;

            for(int i = 0; i < numFaces; i++)
            {
                if (Vector3.Dot(faces[i].normal, newVert.vertCSO - faces[i].a.vertCSO) > 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        CSOVertex[] currentEdge = new CSOVertex[2];
                        if(j == 0)
                        {
                            currentEdge[0] = faces[i].a;
                            currentEdge[1] = faces[i].b;
                        }
                        else if(j == 1)
                        {
                            currentEdge[0] = faces[i].b;
                            currentEdge[1] = faces[i].c;
                        }
                        else
                        {
                            currentEdge[0] = faces[i].c;
                            currentEdge[1] = faces[i].a;
                        }
                        
                        bool foundEdge = false;
                        for (int k = 0; k < looseEdgeNum; k++)
                        {
                            if (looseEdges[k,1].vertCSO == currentEdge[0].vertCSO && looseEdges[k,0].vertCSO == currentEdge[1].vertCSO)
                            {
                                looseEdges[k,0] = looseEdges[looseEdgeNum - 1,0];
                                looseEdges[k,1] = looseEdges[looseEdgeNum - 1,1];
                                looseEdgeNum--;
                                foundEdge = true;
                                k = looseEdgeNum; 
                            }
                        }

                        if (!foundEdge)
                        {
                            if (looseEdgeNum >= maxEPALooseEdges) break;
                            looseEdges[looseEdgeNum,0] = currentEdge[0];
                            looseEdges[looseEdgeNum,1] = currentEdge[1];
                            looseEdgeNum++;
                        }
                    }

                    faces[i] = faces[numFaces - 1];
                    numFaces--;
                    i--;
                }
            }

            for (int i = 0; i < looseEdgeNum; i++)
            {
                if (numFaces >= maxEPAFaces) break;
                faces[numFaces] = new CSOFace(looseEdges[i,0],looseEdges[i,1],newVert);

                float bias = 0.0001f;
                if (faces[numFaces].distance + bias < 0) //Check the counter-clockwiseness of the vertices
                    faces[numFaces] = new CSOFace(looseEdges[i,1],looseEdges[i,0],newVert);
                numFaces++;
            }

            if(iterations == maxEPALoop - 1)
            {
                CalculateContactInfo(colliderA, colliderB, faces[closestFace]);
                return;
            }
        }
    }

    private static void CalculateContactInfo(CustomCollider a, CustomCollider b, CSOFace face)
    {
        currentContact.contactNormal = face.normal;
        currentContact.penetrationDepth = face.distance;

        Vector3 closestPoint = face.normal * face.distance;

        /*  Calculate the barycentric coordinate of the closest point. We're using the Cramer's rule to solve coordinates.  */
        /*  The method of calulating coordinates is based on this website :
            https://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates
         */
        Vector3 v0 = face.b.vertCSO - face.a.vertCSO, v1 = face.c.vertCSO - face.a.vertCSO, v2 = closestPoint - face.a.vertCSO;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;

        currentContact.globalContactA = u * -face.a.vertA + v * -face.b.vertA + w * -face.c.vertA;
        currentContact.localContactA = a.gameObject.transform.worldToLocalMatrix.MultiplyPoint3x4(currentContact.globalContactA);
        currentContact.globalContactB = u * face.a.vertB + v * face.b.vertB + w * face.c.vertB;
        currentContact.localContactB = b.gameObject.transform.worldToLocalMatrix.MultiplyPoint3x4(currentContact.globalContactB);
        

        /*  The method of calulating orthonormal basis is based on this website :
            http://allenchou.net/2013/12/game-physics-contact-generation-epa/
         */
        currentContact.contactTangent1 = currentContact.contactNormal.x >= 0.55735f ?
                                         new Vector3(currentContact.contactNormal.y, -currentContact.contactNormal.x, 0).normalized:
                                         new Vector3(0, currentContact.contactNormal.z, -currentContact.contactNormal.y).normalized;
        currentContact.contactTangent2 = Vector3.Cross(currentContact.contactNormal, currentContact.contactTangent1);

        return;
    }

    private static Vector3 Support(CustomCollider collider, Vector3 vector)
    {
        Vector3 support = new Vector3();
        float dotValue = -10000000;

        Matrix4x4 worldMatrix = collider.gameObject.transform.localToWorldMatrix;

        Vector3[] verts = collider.mesh.vertices;

        foreach (Vector3 v in verts)
        {
            Vector3 globalV = worldMatrix.MultiplyPoint3x4(v);
            if (Vector3.Dot(vector, globalV) > dotValue)
            {
                dotValue = Vector3.Dot(vector, globalV);
                support = globalV;
            }
        }
        return support;
    }
}
