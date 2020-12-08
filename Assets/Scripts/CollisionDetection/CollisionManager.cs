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

    public static Vector3 gravity = new Vector3(0, -9.81f, 0);

    private Broadphase broadphase = null;
    private CollisionDetection collisionDetection = null;
    private List<CollisionPair> pairs;
    private List<CollisionContact> contacts;
    public broadphaseType bType;
    public collisionDetectionType cType;
    [Range(0f, 1f)]
    public float beta = 1;
    [Range(1,20)]
    public int resolveInteration = 10;

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
        //contact.a.gameObject.transform.position += contact.contactNormal * contact.penetrationDepth * 0.5f;
        //contact.b.gameObject.transform.position -= contact.contactNormal * contact.penetrationDepth * 0.5f;

        Debug.DrawLine(contact.a.gameObject.transform.position - Vector3.down * 0.1f,contact.a.gameObject.transform.position + Vector3.down * 0.1f,Color.yellow);
        Debug.DrawLine(contact.a.gameObject.transform.position - Vector3.left * 0.1f,contact.a.gameObject.transform.position + Vector3.left * 0.1f,Color.yellow);
        Debug.DrawLine(contact.a.gameObject.transform.position - Vector3.back * 0.1f,contact.a.gameObject.transform.position + Vector3.back * 0.1f,Color.yellow);
        
        Debug.DrawLine(contact.globalContactA - Vector3.down * 0.2f,contact.globalContactA + Vector3.down * 0.2f,Color.yellow);
        Debug.DrawLine(contact.globalContactA - Vector3.left * 0.2f,contact.globalContactA + Vector3.left * 0.2f,Color.yellow);
        Debug.DrawLine(contact.globalContactA - Vector3.back * 0.2f,contact.globalContactA + Vector3.back * 0.2f,Color.yellow);

        Debug.DrawLine(contact.b.gameObject.transform.position - Vector3.down * 0.1f,contact.b.gameObject.transform.position + Vector3.down * 0.1f,Color.cyan);
        Debug.DrawLine(contact.b.gameObject.transform.position - Vector3.left * 0.1f,contact.b.gameObject.transform.position + Vector3.left * 0.1f,Color.cyan);
        Debug.DrawLine(contact.b.gameObject.transform.position - Vector3.back * 0.1f,contact.b.gameObject.transform.position + Vector3.back * 0.1f,Color.cyan);
        
        Debug.DrawLine(contact.globalContactB - Vector3.down * 0.2f,contact.globalContactB + Vector3.down * 0.2f,Color.cyan);
        Debug.DrawLine(contact.globalContactB - Vector3.left * 0.2f,contact.globalContactB + Vector3.left * 0.2f,Color.cyan);
        Debug.DrawLine(contact.globalContactB - Vector3.back * 0.2f,contact.globalContactB + Vector3.back * 0.2f,Color.cyan);
        
        //法向量碰撞處理
        //碰撞限制初始化

        Vector3 rA = contact.globalContactA - contact.a.gameObject.transform.position;
        Vector3 rB = contact.globalContactB - contact.b.gameObject.transform.position;

        /*  J = [j_va, j_wa, j_vb, j_wb]
         *    = [normal, rA × normal, -normal, - (rb × normal)]
         *
         *      ┌ Ma 0  0  0  ┐
         *  M = │ 0  Ia 0  0  │ Ma, Mb are mass. Ia, Ib are inertia tensor.
         *      │ 0  0  Mb 0  │
         *      └ 0  0  0  Ib ┘
         *
         *  Effective mass = 1 / (J × M^(-1) × J^(T))
         */

        Vector3 j_va = contact.contactNormal;
        Vector3 j_wa = Vector3.Cross(rA, contact.contactNormal);
        Vector3 j_vb = -contact.contactNormal;
        Vector3 j_wb = -Vector3.Cross(rB, contact.contactNormal);

        float k = (contact.a._rigidbody.inverseMass * Vector3.Dot(j_va,j_va)) +
                  Vector3.Dot(j_wa, contact.a._rigidbody.localInverseInertiaTensor.Transform(j_wa)) +
                  (contact.b._rigidbody.inverseMass * Vector3.Dot(j_vb,j_vb)) +
                  Vector3.Dot(j_wb, contact.b._rigidbody.localInverseInertiaTensor.Transform(j_wb));

        float effectiveMass = 1.0f / k;

        float totalImpulse = 0;

        //碰撞限制解析

        float jv =  Vector3.Dot(j_va, contact.a._rigidbody.linearVelocity) +
                    Vector3.Dot(j_wa, contact.a._rigidbody.angularVelocity * Mathf.Deg2Rad) +
                    Vector3.Dot(j_vb, contact.b._rigidbody.linearVelocity) +
                    Vector3.Dot(j_wb, contact.b._rigidbody.angularVelocity * Mathf.Deg2Rad);

        /*float resistution = (contact.a._rigidbody.resistution + contact.b._rigidbody.resistution) * 0.5f;
        Vector3 relativeVelocity =  -contact.a._rigidbody.linearVelocity
                                    -Vector3.Cross(contact.a._rigidbody.angularVelocity * Mathf.Deg2Rad, rA)
                                    +contact.b._rigidbody.linearVelocity
                                    +Vector3.Cross(contact.b._rigidbody.angularVelocity * Mathf.Deg2Rad, rB);
        float closeVelocity = Vector3.Dot(relativeVelocity, contact.contactNormal);
        
        float b = -(beta / Time.fixedDeltaTime) * contact.penetrationDepth + resistution * closeVelocity;*/

        float tempLambda = effectiveMass * (-(jv /*+ b*/));
        float oldTotalImpulse = totalImpulse;

        totalImpulse = Mathf.Clamp(totalImpulse + tempLambda, 0, float.PositiveInfinity);

        float lambda = totalImpulse - oldTotalImpulse;

        if(!contact.a._rigidbody.isStatic)
        {
            contact.a._rigidbody.linearVelocity += contact.a._rigidbody.inverseMass * j_va * lambda;
            contact.a._rigidbody.angularVelocity += contact.a._rigidbody.localInverseInertiaTensor.Transform(j_wa) * lambda;
        }
        if (!contact.b._rigidbody.isStatic)
        {
            contact.b._rigidbody.linearVelocity += contact.b._rigidbody.inverseMass * j_vb * lambda;
            contact.b._rigidbody.angularVelocity += contact.b._rigidbody.localInverseInertiaTensor.Transform(j_wb) * lambda;
        }

        //========================
        /*
        //切向量碰撞處理1
        j_va = contact.contactTangent1;
        j_wa = Vector3.Cross(rA, contact.contactTangent1);
        j_vb = -contact.contactTangent1;
        j_wb = -Vector3.Cross(rB, contact.contactTangent1);

        jv = Vector3.Dot(j_va, contact.a._rigidbody.linearVelocity) +
             Vector3.Dot(j_wa, contact.a._rigidbody.angularVelocity * Mathf.Deg2Rad) +
             Vector3.Dot(j_vb, contact.b._rigidbody.linearVelocity) +
             Vector3.Dot(j_wb, contact.b._rigidbody.angularVelocity * Mathf.Deg2Rad);

        float friction = 1 - ((contact.a._rigidbody.friction + contact.b._rigidbody.friction) * 0.5f);
        float maxFrictionForce = friction * lambda;
        relativeVelocity = -contact.a._rigidbody.linearVelocity
                                   - Vector3.Cross(contact.a._rigidbody.angularVelocity * Mathf.Deg2Rad, rA)
                                   + contact.b._rigidbody.linearVelocity
                                   + Vector3.Cross(contact.b._rigidbody.angularVelocity * Mathf.Deg2Rad, rB);
        closeVelocity = Vector3.Dot(relativeVelocity, contact.contactTangent1);
        b = friction * closeVelocity;

        float lambdaT = Mathf.Clamp(effectiveMass * (-(jv + b)), -maxFrictionForce, maxFrictionForce);

        if (!contact.a._rigidbody.isStatic)
        {
            contact.a._rigidbody.linearVelocity += contact.a._rigidbody.inverseMass * j_va * lambdaT;
            contact.a._rigidbody.angularVelocity += contact.a._rigidbody.localInverseInertiaTensor.Transform(j_wa) * lambdaT;
        }
        if (!contact.b._rigidbody.isStatic)
        {
            contact.b._rigidbody.linearVelocity += contact.b._rigidbody.inverseMass * j_vb * lambdaT;
            contact.b._rigidbody.angularVelocity += contact.b._rigidbody.localInverseInertiaTensor.Transform(j_wb) * lambdaT;
        }   
        //========================

        //切向量碰撞處理2
        j_va = contact.contactTangent2;
        j_wa = Vector3.Cross(rA, contact.contactTangent2);
        j_vb = -contact.contactTangent2;
        j_wb = -Vector3.Cross(rB, contact.contactTangent2);

        jv = Vector3.Dot(j_va, contact.a._rigidbody.linearVelocity) +
             Vector3.Dot(j_wa, contact.a._rigidbody.angularVelocity * Mathf.Deg2Rad) +
             Vector3.Dot(j_vb, contact.b._rigidbody.linearVelocity) +
             Vector3.Dot(j_wb, contact.b._rigidbody.angularVelocity * Mathf.Deg2Rad);

        relativeVelocity = -contact.a._rigidbody.linearVelocity
                                   - Vector3.Cross(contact.a._rigidbody.angularVelocity * Mathf.Deg2Rad, rA)
                                   + contact.b._rigidbody.linearVelocity
                                   + Vector3.Cross(contact.b._rigidbody.angularVelocity * Mathf.Deg2Rad, rB);
        closeVelocity = Vector3.Dot(relativeVelocity, contact.contactTangent2);
        b = friction * closeVelocity;
        lambdaT = Mathf.Clamp(effectiveMass * (-(jv + b)), -maxFrictionForce, maxFrictionForce);

        if (!contact.a._rigidbody.isStatic)
        {
            contact.a._rigidbody.linearVelocity += contact.a._rigidbody.inverseMass * j_va * lambdaT;
            contact.a._rigidbody.angularVelocity += contact.a._rigidbody.localInverseInertiaTensor.Transform(j_wa) * lambdaT;
        }
        if (!contact.b._rigidbody.isStatic)
        {
            contact.b._rigidbody.linearVelocity += contact.b._rigidbody.inverseMass * j_vb * lambdaT;
            contact.b._rigidbody.angularVelocity += contact.b._rigidbody.localInverseInertiaTensor.Transform(j_wb) * lambdaT;
        }
        //========================
        */
    }
}
