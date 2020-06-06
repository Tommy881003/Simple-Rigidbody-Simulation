using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private Rigidbody rb;
    public Vector3 velocity;

    private static float timeSpeed = 0.1f;

    private TestScript instance = null;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            Time.timeScale = timeSpeed;
            Time.fixedDeltaTime *= 0.5f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
    }
}
