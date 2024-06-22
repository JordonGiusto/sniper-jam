using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string text;

    Rigidbody rb;
    Collider collider;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }


    



    // Update is called once per frame
    void Update()
    {
        
    }
}
