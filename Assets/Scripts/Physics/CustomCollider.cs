using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum colliderType
{
    Other,
    Box,
    Sphere
}

[RequireComponent(typeof(MeshFilter))]
public class CustomCollider : MonoBehaviour
{
    public float mass = 1;
    [HideInInspector]
    public Vector3 localCentroid = Vector3.zero;
    [HideInInspector]
    public Matrix3x3 localInertiaTensor;
    [HideInInspector]
    public Mesh mesh;
    [HideInInspector]
    public CustomRigidbody _rigidbody = null;
    [HideInInspector]
    public List<Vector3> localNormals = new List<Vector3>();
    [HideInInspector]
    public colliderType type = colliderType.Other;

    public virtual void Calculate() 
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;

        int[] tris = mesh.triangles;
        Vector3[] verts = mesh.vertices;

        Matrix3x3 scaleMatrix = new Matrix3x3();
        scaleMatrix.SetRow(0, new Vector3(transform.localScale.x, 0, 0));
        scaleMatrix.SetRow(1, new Vector3(0, transform.localScale.y, 0));
        scaleMatrix.SetRow(2, new Vector3(0, 0, transform.localScale.z));

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 center = scaleMatrix.Transform(((verts[tris[i + 0]] + verts[tris[i + 1]] + verts[tris[i + 2]]) / 3))
                             + transform.position;
            Vector3 a = scaleMatrix.Transform(verts[tris[i + 0]] - verts[tris[i + 1]]);
            Vector3 b = scaleMatrix.Transform(verts[tris[i + 0]] - verts[tris[i + 2]]);
            Vector3 normal = Vector3.Cross(a, b).normalized;
            if(!localNormals.Contains(normal) && !localNormals.Contains(-normal))
            {
                localNormals.Add(normal);
                //Debug.DrawLine(center, center + normal * 0.2f, Color.yellow, 3);
            }
        }
    }

    private void Start()
    {
        CollisionManager.colliders.Add(this);
    }
}
