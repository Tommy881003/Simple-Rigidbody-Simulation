using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum broadphaseType
{
    Dummy,
    AABBTree
}

public enum collisionDetectionType
{
    GJK,
    SAT
}

public class CollisionManager : MonoBehaviour
{
    public static CollisionManager instance = null;
    public static List<CustomCollider> colliders;
    private Broadphase broadphase = null;
    private CollisionDetection collisionDetection = null;
    private List<CollisionPair> pairs;
    private List<CollisionContact> contacts;
    public broadphaseType bType;
    public collisionDetectionType cType;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            colliders = new List<CustomCollider>();
        }
        else
            Destroy(this);
    }

    private void Start()
    {
        switch (bType)
        {
            case broadphaseType.Dummy:
                broadphase = Dummy.CreateInstance();
                break;
            case broadphaseType.AABBTree:
                break;
        }

        switch (cType)
        {
            case collisionDetectionType.GJK:
                collisionDetection = GJKwithEPA.CreateInstance();
                break;
            case collisionDetectionType.SAT:
                break;
        }
    }

    private void FixedUpdate()
    {
        broadphase.UpdateAABB();
        pairs = broadphase.ComputePossibleCollisions();
        contacts = collisionDetection.CalculateCollision(pairs);
        foreach(CollisionContact contact in contacts)
        {
            contact.a.haveContact = true;
            contact.b.haveContact = true;
        }
    }
}
