using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : Broadphase
{
    private static Dummy instance = null;

    public static Dummy CreateInstance()
    {
        if (instance == null)
        {
            instance = new Dummy();
            return instance;
        }
        else
            return null;
    }

    public override void Add(Bounds aabb)
    {
        return;
    }

    public override void UpdateAABB()
    {
        return;
    }

    public override List<CollisionPair> ComputePossibleCollisions()
    {
        List<CollisionPair> pairs = new List<CollisionPair>();
        int length = CollisionManager.colliders.Count;

        for (int i = 0; i < length - 1; i++)
            for (int j = i + 1; j < length; j++)
                pairs.Add(new CollisionPair
                {
                    a = CollisionManager.colliders[i],
                    b = CollisionManager.colliders[j]
                });
        return pairs;
    }

    public override List<CustomCollider> QueryPossibleCollisions(CustomCollider collider)
    {
        return null;
    }
}
