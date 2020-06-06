using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SAT : CollisionDetection
{
    public override List<CollisionContact> CalculateCollision(List<CollisionPair> pairs)
    {
        List<CollisionContact> contacts = new List<CollisionContact>();
        
        foreach (CollisionPair pair in pairs)
        {

        }

        return contacts;
    }
}
