using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRigidbody : MonoBehaviour
{
    public float mass;
    private float inverseMass;

    private Matrix4x4 localInverseInertiaTensor;
    private Matrix4x4 globalInverseInertiaTensor;

    [HideInInspector]
    public Vector3 globalCentroid;
    [HideInInspector]
    public Vector3 localCentroid;

    [ReadOnly]
    public Vector3 position;
    [HideInInspector]
    public Matrix4x4 orientation;

    [HideInInspector]
    public Vector3 linearVelocity;
    [HideInInspector]
    public Vector3 angularVelocity;

    [HideInInspector]
    public Vector3 forceAccumulator;
    [HideInInspector]
    public Vector3 torqueAccumulator;

    private List<CustomCollider> colliders;

    private void UpdateGlobalCentroidFromPosition()
    {
        globalCentroid = position + (Vector3)(orientation * localCentroid);
    }

    private void UpdatePositionFromGlobalCentroid()
    {
        position = globalCentroid - (Vector3)(orientation * localCentroid);
    }

    private void AddColliders()
    {
        /*參數歸零*/
        mass = 0;
        localCentroid = Vector3.zero;
        localInverseInertiaTensor = Matrix4x4.zero;

        /*清除原本的碰撞器紀錄，並更新新的碰撞器*/
        colliders.Clear();
        CustomCollider[] ccs = gameObject.GetComponentsInChildren<CustomCollider>();

        /*更新質量和質量中心*/
        foreach(CustomCollider cc in ccs)
        {
            colliders.Add(cc);
            mass += cc.mass;
            localCentroid += cc.mass * cc.localCentroid;
        }

        inverseMass = 1 / mass;
        localCentroid *= inverseMass;

        Matrix4x4 localInertiaTensor = Matrix4x4.zero;

        foreach (CustomCollider cc in ccs)
        {
            Vector3 distance = localCentroid - cc.localCentroid;
        }
    }

    private void Reset()
    {
    }
}
