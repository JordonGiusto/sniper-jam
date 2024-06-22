using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float maxAcceleration, maxSpeed;


    Rigidbody rb;

    Vector3 currentSpeed;
    Vector2 currentMoveInput;
    Vector2 currentLookInput;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        movementUpdate();
       
        aimUpdate();
        
    }


    public void aimUpdate()
    {

    }

    public void movementUpdate()
    {
        Vector3 transformedInput = transform.TransformDirection(new Vector3(currentMoveInput.x, 0, currentMoveInput.y));

        Vector2 targetVelo = new Vector2(transformedInput.x, transformedInput.z) * maxSpeed;

        Vector2 v2CurrentSpeed = new Vector2(currentSpeed.x, currentSpeed.z);
        Vector2 acceleration = Vector2.ClampMagnitude(targetVelo - v2CurrentSpeed, maxAcceleration * Time.deltaTime);

        v2CurrentSpeed += acceleration;
        currentSpeed.x = v2CurrentSpeed.x;
        currentSpeed.z = v2CurrentSpeed.y;


        rb.velocity = currentSpeed;
    }


    public void handleMoveInput(InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();
    }
    public void handleLookInput(InputAction.CallbackContext context)
    {
        currentLookInput = context.ReadValue<Vector2>();
    }
}
