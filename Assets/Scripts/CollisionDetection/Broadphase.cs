using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CollisionPair
{
    public CustomCollider a;
    public CustomCollider b;
}

public abstract class Broadphase : MonoBehaviour
{
    /*增加一個新的AABB*/
    public abstract void Add(Bounds aabb);

    /*更新AABB之間的碰撞關係*/
    public abstract void FixedUpdate();

    /*算出所有可能的碰撞序對*/
    public abstract List<CollisionPair> ComputePossibleCollisions();

    /*給出一個collider可能撞到的其他collider*/
    public abstract List<CustomCollider> QueryPossibleCollisions(CustomCollider collider);
}
