using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CollisionContact
{
    public CustomCollider a;
    public CustomCollider b;

    public Vector3 localContactA;
    public Vector3 localContactB;
    public Vector3 globalContactA;
    public Vector3 globalContactB;

    public Vector3 contactNormal;
    public Vector3 contactTangent1;
    public Vector3 contactTangent2;
    public float penetrationDepth;
}

public abstract class CollisionDetection
{
    public abstract List<CollisionContact> CalculateCollision(List<CollisionPair> pairs);
}
