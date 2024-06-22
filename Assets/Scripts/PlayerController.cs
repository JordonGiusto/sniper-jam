using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxAcceleration, maxSpeed;

    Vector3 currentSpeed;
    // Start is called before the first frame update
    void Start()
    {
        currentSpeed = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
