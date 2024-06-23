using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{

    Rigidbody rb;
    Collider collider;

    public delegate void Distraction(Throwable dist);
    public static Distraction causeDistraction;

    // Start is called before the first frame update
    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }

    public void pickUp()
    {
        collider.enabled = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void interact(Vector3 throwDir)
    {
        causeDistraction.Invoke(this);
        collider.enabled = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(throwDir.normalized * 10, ForceMode.Impulse);
    }
}
