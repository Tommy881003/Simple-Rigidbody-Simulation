using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomSphere : CustomCollider
{
    protected override void Reset()
    {
        base.Reset();
        type = colliderType.Sphere;
        float xx = transform.lossyScale.x * transform.lossyScale.x;
        float yy = transform.lossyScale.y * transform.lossyScale.y;
        float zz = transform.lossyScale.z * transform.lossyScale.z;
        localInertiaTensor = new Matrix3x3();
        localInertiaTensor.SetRow(0, new Vector4(mass * yy * zz / 5f, 0, 0));
        localInertiaTensor.SetRow(1, new Vector4(0, mass * xx * zz / 5f, 0));
        localInertiaTensor.SetRow(2, new Vector4(0, 0, mass * xx * yy / 5f));
    }
}
