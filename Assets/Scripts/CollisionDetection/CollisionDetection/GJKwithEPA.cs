using System.Collections;
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
    private static float epaThreshold = 0.0001f;
    private static int maxEPALoop = 32;
    private static int maxEPAFaces = 64;
    private static int maxEPALooseEdges = 64;

    /*For both GJK and EPA algorithm*/
    private static Vector3 first, second, third, last;
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
        third = Support(colliderB, searchDir) - Support(colliderA, -searchDir);

        /*Find the second point of the simplex*/
        searchDir = -third;
        second = Support(colliderB, searchDir) - Support(colliderA, -searchDir);

        /*If the new simplex point have negative dot value, then the CSO must NOT contain the origin*/
        if (Vector3.Dot(second, searchDir) < 0) 
            return false;

        searchDir = Vector3.Cross(Vector3.Cross(third - second, -second), third - second);
        if (searchDir == Vector3.zero)
        {
            searchDir = Vector3.Cross(third - second, Vector3.right); //normal with x-axis
            if (searchDir == Vector3.zero)
                searchDir = Vector3.Cross(third - second, Vector3.forward); //normal with z-axis
        }
        simplexCount = 2;

        /*run through the iteration.*/
        for(int i = 0; i < maxGJKLoop; i++)
        {
            first = Support(colliderB, searchDir) - Support(colliderA, -searchDir);
            if (Vector3.Dot(first, searchDir) < 0)
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
        Vector3 sVect = second - first;
        Vector3 tVect = third - first;
        Vector3 nFirst = -first;
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
        Vector3 sVect = second - first;
        Vector3 tVect = third - first;
        Vector3 lVect = last - first;
        Vector3 nFirst = -first;
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
        Vector3[,] faces = new Vector3[maxEPAFaces, 4];
        Vector3 a = first; Vector3 b = second; Vector3 c = third; Vector3 d = last;

        faces[0,0] = a;
        faces[0,1] = b;
        faces[0,2] = c;
        faces[0,3] = Vector3.Cross(b - a, c - a).normalized;
        faces[1,0] = a;
        faces[1,1] = c;
        faces[1,2] = d;
        faces[1,3] = Vector3.Cross(c - a, d - a).normalized;
        faces[2,0] = a;
        faces[2,1] = d;
        faces[2,2] = b;
        faces[2,3] = Vector3.Cross(d - a, b - a).normalized;
        faces[3,0] = b;
        faces[3,1] = d;
        faces[3,2] = c;
        faces[3,3] = Vector3.Cross(d - b, c - b).normalized;

        int numFaces = 4;
        int closestFace = 0;

        for (int iterations = 0; iterations < maxEPALoop; iterations++)
        {
            float min_dist = Vector3.Dot(faces[0, 0], faces[0, 3]);
            closestFace = 0;
            for (int i = 1; i < numFaces; i++)
            {
                float dist = Vector3.Dot(faces[i, 0], faces[i, 3]);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    closestFace = i;
                }
            }

            searchDir = faces[closestFace,3];
            Vector3 newVert = Support(colliderB,searchDir) - Support(colliderA, -searchDir);

            if (Vector3.Dot(newVert, searchDir) - min_dist < epaThreshold)
            {
                Vector3[] face = new Vector3[4];
                face[0] = faces[closestFace, 0];
                face[1] = faces[closestFace, 1];
                face[2] = faces[closestFace, 2];
                face[3] = faces[closestFace, 3];
                CalculateContactInfo(colliderA, colliderB, face);
                return;
            }

            Vector3[,] looseEdges = new Vector3[maxEPALooseEdges,2];
            int looseEdgeNum = 0;

            for(int i = 0; i < numFaces; i++)
            {
                if (Vector3.Dot(faces[i,3], newVert - faces[i,0]) > 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Vector3[] currentEdge = new Vector3[2];
                        currentEdge[0] = faces[i, j];
                        currentEdge[1] = faces[i, (j + 1) % 3];
                        bool foundEdge = false;
                        for (int k = 0; k < looseEdgeNum; k++)
                        {
                            if (looseEdges[k,1] == currentEdge[0] && looseEdges[k,0] == currentEdge[1])
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

                    faces[i,0] = faces[numFaces - 1,0];
                    faces[i,1] = faces[numFaces - 1,1];
                    faces[i,2] = faces[numFaces - 1,2];
                    faces[i,3] = faces[numFaces - 1,3];
                    numFaces--;
                    i--;
                }
            }

            for (int i = 0; i < looseEdgeNum; i++)
            {
                if (numFaces >= maxEPAFaces) break;
                faces[numFaces,0] = looseEdges[i,0];
                faces[numFaces,1] = looseEdges[i,1];
                faces[numFaces,2] = newVert;
                faces[numFaces,3] = Vector3.Cross(looseEdges[i,0] - looseEdges[i,1], looseEdges[i,0] - newVert).normalized;

                float bias = 0.000001f;
                if (Vector3.Dot(faces[numFaces,0], faces[numFaces,3]) + bias < 0)
                {
                    Vector3 temp = faces[numFaces,0];
                    faces[numFaces,0] = faces[numFaces,1];
                    faces[numFaces,1] = temp;
                    faces[numFaces,3] = -faces[numFaces,3];
                }
                numFaces++;
            }

            if(iterations == maxEPALoop - 1)
            {
                Vector3[] _face = new Vector3[4];
                _face[0] = faces[closestFace, 0];
                _face[1] = faces[closestFace, 1];
                _face[2] = faces[closestFace, 2];
                _face[3] = faces[closestFace, 3];
                CalculateContactInfo(colliderA, colliderB, _face);
                return;
            }
        }
    }

    private static void CalculateContactInfo(CustomCollider a, CustomCollider b, Vector3[] face)
    {
        currentContact.globalContactA = Support(a, -searchDir);
        currentContact.localContactA = a.gameObject.transform.worldToLocalMatrix.MultiplyPoint3x4(currentContact.globalContactA);
        currentContact.globalContactB = Support(b, searchDir);
        currentContact.localContactB = b.gameObject.transform.worldToLocalMatrix.MultiplyPoint3x4(currentContact.globalContactB);
        currentContact.contactNormal = face[3];
        currentContact.penetrationDepth = Vector3.Dot(face[0],face[3]);

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
