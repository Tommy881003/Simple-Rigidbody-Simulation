using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GJKwithEPA : CollisionDetection
{
    private static GJKwithEPA instance = null;
    private static Vector3 first, second, third, last;
    private static Vector3 searchDir;
    private static int simplexCount = 0;
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
            currentContact = new CollisionContact();
            currentContact.a = pair.a;
            currentContact.b = pair.b;
            if (GJK(pair.a, pair.b))
                contacts.Add(currentContact);
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

        for(int i = 0; i < 64; i++)
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

        if((first + normal).sqrMagnitude < normal.sqrMagnitude)
        {
            normal = -normal;
            Vector3 temp = second;
            second = third;
            third = temp;
            sVect = second - first;
            tVect = third - first;
        }

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
            2. The origin is not within the simplex, will reduce the simplex to at least a triangle.
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
