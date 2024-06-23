using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float maxAcceleration, maxSpeed, crouchSpeed;

    public float relaodTime;
    float timeSinceFired;

    public GameObject scope;


    public Sniperjam sj;

    Rigidbody rb;

    Vector3 currentSpeed;
    Vector2 currentMoveInput;
    Vector2 currentLookInput;

    public float mouseSense;


    Animator animator;

    public Camera cam;

    bool crouched;

    public TextMeshProUGUI interactText;

    public float interactDistance, holdDistance;

    public bool aimedDownSights = false;
    
    Throwable heldObject = null;

    public GameObject gun;
    bool hasGun = false;

    // Start is called before the first frame update
    void Start()
    {
        gun.SetActive(false);
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

        sj.Player.Aim.canceled += handleADSInput;
        sj.Player.Aim.performed += handleADSInput;

        sj.Player.Grab.performed += handleGrabInput;

        sj.Player.Drop.performed += handleDropInput;


        sj.Player.Fire.performed += handleFireInput;


        Cursor.lockState = CursorLockMode.Locked;

        
    }



    // Update is called once per frame
    void Update()
    {
        movementUpdate();
       
        aimUpdate();

        interactUpdate();

        timeSinceFired += Time.deltaTime;
        
    }

    void interactUpdate()
    {
        if(heldObject != null)
        {
            heldObject.transform.position = cam.transform.position + cam.transform.forward * holdDistance;
            
        }
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if(heldObject == null && Physics.Raycast(ray, out RaycastHit hit, interactDistance) && hit.collider.TryGetComponent(out Interactable i))
        {
            if(i.TryGetComponent(out Throwable t))
            {
                interactText.text = i.text + ": press E to pick up";
            }
            if (i.CompareTag("gun"))
            {
                interactText.text = i.text + ": press E equip";

            }
        }
        else
        {
            interactText.text = "";
        }
    }

    private void aimUpdate()
    {
        transform.Rotate(Vector3.up, mouseSense * currentLookInput.x * (aimedDownSights ? .4f : 1f));

        float currentCamRotation = (cam.transform.localRotation.eulerAngles.x + 90) % 360;

        float targetCamRotation = Mathf.Clamp(currentCamRotation - mouseSense * currentLookInput.y * (aimedDownSights ? .5f : 1f), 10, 170);




        cam.transform.rotation = Quaternion.Euler(targetCamRotation - 90, cam.transform.rotation.eulerAngles.y, cam.transform.eulerAngles.z);




    }

    private void movementUpdate()
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


    private void handleMoveInput(InputAction.CallbackContext context)
    {
        currentMoveInput = context.ReadValue<Vector2>();
    }
    private void handleLookInput(InputAction.CallbackContext context)
    {
        currentLookInput = context.ReadValue<Vector2>();
    }

    private void handleCrouchInput(InputAction.CallbackContext context)
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

    private void handleADSInput(InputAction.CallbackContext context)
    {
        if(!hasGun) return;
        if (context.performed)
        {
            animator.SetTrigger("ADS");

        }
        else
        {
            animator.SetTrigger("unADS");

        }
    }

    private void handleGrabInput(InputAction.CallbackContext context)
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if(hit.collider.TryGetComponent(out Throwable i))
            {
                heldObject = i;
                i.pickUp();
            }
            else if (hit.collider.CompareTag("gun"))
            {
                gun.SetActive(true);
                hasGun = true;
                Destroy(hit.collider.gameObject);
            }
      
        }
        
    }

    private void handleDropInput(InputAction.CallbackContext context)
    {
        if(heldObject == null) return;
        heldObject.interact(cam.transform.forward);
        heldObject = null;
    }

    private void handleFireInput(InputAction.CallbackContext context)
    {
        if (!aimedDownSights || !hasGun)
        {
            return;
        }
        if(timeSinceFired < relaodTime) return;

        timeSinceFired = 0;



        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent(out EnemyBehavior e))
            {
                print("hit enemy");
            }

        }


    }

}
