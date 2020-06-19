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
        foreach (CollisionContact contact in contacts)
            SolveContact(contact);
    }

    private void SolveContact(CollisionContact contact)
    {
        Debug.Log("Contact");
        /*碰撞限制初始化*/
        if (contact.a._rigidbody == null || contact.b._rigidbody == null || 
            contact.a._rigidbody == contact.b._rigidbody)
            return;

        Vector3 j_va = contact.contactNormal;
        Vector3 j_wa = Vector3.Cross(contact.localContactA, contact.contactNormal);
        Vector3 j_vb = -contact.contactNormal;
        Vector3 j_wb = -Vector3.Cross(contact.localContactB, contact.contactNormal);

        float k = contact.a._rigidbody.inverseMass +
                  Vector3.Dot(j_wa, contact.a._rigidbody.localInverseInertiaTensor.Transform(j_wa)) +
                  contact.b._rigidbody.inverseMass +
                  Vector3.Dot(j_wb, contact.b._rigidbody.localInverseInertiaTensor.Transform(j_wb));

        float effectiveMass = 1.0f / k;

        /*碰撞限制解析*/
        float jv = Vector3.Dot(j_va, contact.a._rigidbody.linearVelocity) +
                   Vector3.Dot(j_wa, contact.a._rigidbody.angularVelocity) +
                   Vector3.Dot(j_vb, contact.b._rigidbody.linearVelocity) +
                   Vector3.Dot(j_wb, contact.b._rigidbody.angularVelocity);

        float beta = 1f;
        float resistution = 1f;
        Vector3 relativeVelocity = -contact.a._rigidbody.linearVelocity
                                   -Vector3.Cross(contact.a._rigidbody.angularVelocity, contact.localContactA)
                                   +contact.b._rigidbody.linearVelocity
                                   +Vector3.Cross(contact.b._rigidbody.angularVelocity, contact.localContactB);
        float closeVelocity = Vector3.Dot(relativeVelocity, contact.contactNormal);
        float b = -(beta / Time.fixedDeltaTime) * contact.penetrationDepth + resistution * closeVelocity;

        float lambda = effectiveMass * (-(jv + b));

        contact.a._rigidbody.linearVelocity += contact.a._rigidbody.inverseMass * j_va * lambda;
        contact.a._rigidbody.angularVelocity += contact.a._rigidbody.localInverseInertiaTensor.Transform(j_wa) * lambda;
        contact.b._rigidbody.linearVelocity += contact.b._rigidbody.inverseMass * j_vb * lambda;
        contact.b._rigidbody.angularVelocity += contact.b._rigidbody.localInverseInertiaTensor.Transform(j_wb) * lambda;
    }
}
