using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CustomCollider : MonoBehaviour
{
    public float mass;
    public Vector3 position;
    [HideInInspector]
    public Vector3 localCentroid;
    public Matrix4x4 localInertiaTensor;

    protected virtual void Reset() { }
}
