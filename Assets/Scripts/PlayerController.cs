using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float maxAcceleration, maxSpeed, crouchSpeed;


    public Sniperjam sj;

    Rigidbody rb;

    Vector3 currentSpeed;
    Vector2 currentMoveInput;
    Vector2 currentLookInput;

    public float mouseSense;


    Animator animator;

    public Camera cam;

    bool crouched;

    // Start is called before the first frame update
    void Start()
    {
        sj =  new Sniperjam();
        sj.Enable();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentSpeed = Vector3.zero;

        sj.Player.Move.performed += handleMoveInput;
        sj.Player.Move.canceled += handleMoveInput;
        sj.Player.Look.performed += handleLookInput;
        sj.Player.Look.canceled += handleLookInput;

        sj.Player.Crouch.performed += handleCrouchInput;
        sj.Player.Crouch.canceled += handleCrouchInput;


        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        movementUpdate();
       
        aimUpdate();
        
    }


    public void aimUpdate()
    {
        transform.Rotate(Vector3.up, mouseSense * currentLookInput.x);

        float currentCamRotation = (cam.transform.localRotation.eulerAngles.x + 90) % 360;

        float targetCamRotation = Mathf.Clamp(currentCamRotation - mouseSense * currentLookInput.y, 10, 170);




        cam.transform.rotation = Quaternion.Euler(targetCamRotation - 90, cam.transform.rotation.eulerAngles.y, cam.transform.eulerAngles.z);

        print(targetCamRotation);



    }

    public void movementUpdate()
    {
        Vector3 transformedInput = transform.TransformDirection(new Vector3(currentMoveInput.x, 0, currentMoveInput.y));

        Vector2 targetVelo = new Vector2(transformedInput.x, transformedInput.z) * (crouched ? crouchSpeed : maxSpeed);

        Vector2 v2CurrentSpeed = new Vector2(currentSpeed.x, currentSpeed.z);
        Vector2 acceleration = Vector2.ClampMagnitude(targetVelo - v2CurrentSpeed, maxAcceleration * Time.deltaTime);

        v2CurrentSpeed += acceleration;
        currentSpeed.x = v2CurrentSpeed.x;
        currentSpeed.z = v2CurrentSpeed.y;
        currentSpeed.y = rb.velocity.y;


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

    public void handleCrouchInput(InputAction.CallbackContext context)
    {
        
        if (context.performed)
        {
            animator.SetTrigger("Crouch");
            crouched = true;
        }
        else
        {
            animator.SetTrigger("Uncrouch");
            crouched = false;
        }
    }

    public void handleADSInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

        }
        else
        {

        }
    }
}
