using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBox : CustomCollider
{
    protected override void Reset()
    {
        float xx = transform.lossyScale.x * transform.lossyScale.x;
        float yy = transform.lossyScale.y * transform.lossyScale.y;
        float zz = transform.lossyScale.z * transform.lossyScale.z;
        localCentroid = position;
        localInertiaTensor = new Matrix4x4();
        localInertiaTensor.SetRow(0, new Vector4(mass * yy * zz / 12f, 0, 0, 0));
        localInertiaTensor.SetRow(1, new Vector4(0, mass * xx * zz / 12f, 0, 0));
        localInertiaTensor.SetRow(2, new Vector4(0, 0, mass * xx * yy / 12f, 0));
        localInertiaTensor.SetRow(3, new Vector4(0, 0, 0, 1));
    }
}
