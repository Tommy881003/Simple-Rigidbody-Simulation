using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRigidbody : MonoBehaviour
{
    private float mass;
    private float inverseMass;

    private Matrix3x3 localInverseInertiaTensor;
    private Matrix3x3 globalInverseInertiaTensor;

    [HideInInspector]
    public Vector3 globalCentroid;
    [HideInInspector]
    public Vector3 localCentroid;

    [ReadOnly]
    public Vector3 position;
    [HideInInspector]
    public Matrix3x3 orientation;

    [HideInInspector]
    public Vector3 linearVelocity;
    [HideInInspector]
    public Vector3 angularVelocity;

    [HideInInspector]
    public Vector3 forceAccumulator;
    [HideInInspector]
    public Vector3 torqueAccumulator;

    private List<CustomCollider> colliders = new List<CustomCollider>();

    /*根據質心更新位置*/
    private void UpdateGlobalCentroidFromPosition()
    {
        globalCentroid = position + orientation.Transform(localCentroid);
    }

    /*根據位置更新質心*/
    private void UpdatePositionFromGlobalCentroid()
    {
        position = globalCentroid - orientation.Transform(localCentroid);
    }

    /*更新碰撞器以及剛體的物理參數*/
    private void AddColliders()
    {
        /*參數歸零*/
        mass = 0;
        localCentroid = Vector3.zero;
        localInverseInertiaTensor = Matrix3x3.zero;

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

        Matrix3x3 localInertiaTensor = Matrix3x3.zero;

        /*更新旋轉慣量矩陣*/
        foreach (CustomCollider cc in ccs)
        {
            Vector3 distance = localCentroid - cc.localCentroid;
            localInertiaTensor += (cc.localInertiaTensor + 
                                   cc.mass * (Vector3.Dot(distance,distance) * Matrix3x3.identity - 
                                   Matrix3x3.OuterProduct(distance, distance)));
        }

        localInverseInertiaTensor = localInertiaTensor.inverse;
    }

    /*點轉換(物體坐標系到世界座標系)*/
    public Vector3 LocalToGlobal(Vector3 point)
    {
        return orientation.Transform(point) + position;
    }

    /*點轉換(世界坐標系到物體座標系)*/
    public Vector3 GlobalToLocal(Vector3 point)
    {
        return orientation.inverse.Transform(point - position);
    }

    /*向量轉換(物體坐標系到世界座標系)*/
    public Vector3 LocalToGlobalVec(Vector3 vector)
    {
        return orientation.Transform(vector);
    }

    /*向量轉換(世界坐標系到物體座標系)*/
    public Vector3 GlobalToLocalVec(Vector3 vector)
    {
        return orientation.inverse.Transform(vector);
    }

    /*施力*/
    public void AddForce(Vector3 force, Vector3 at)
    {
        forceAccumulator += force;
        torqueAccumulator += Vector3.Cross((at - globalCentroid), force);
    }

    /*修正因為更新旋轉矩陣造成的浮點數誤差*/
    public void RecalculateOrientation()
    {
        Quaternion q = orientation.toAffine4x4.rotation;
        orientation = Matrix3x3.Rotate(q.normalized);
    }

    private void Reset()
    {
        AddColliders();
    }
}
