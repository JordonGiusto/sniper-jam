using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    int health;


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

    [SerializeField]
    AudioSource heart;
    [SerializeField]
    AudioSource breathing;
    [SerializeField]
    AudioSource impact;
    [SerializeField]
    AudioSource shot;

    public GameObject heartContainer;

    float hr = 1;
    float Heartbeat
    {
        get => hr;
        set
        {
            hr = value;
            heart.outputAudioMixerGroup.audioMixer.SetFloat("Speed", hr/60);
            heart.outputAudioMixerGroup.audioMixer.SetFloat("Pitch", 60/hr);
            heart.volume = 0.1f*hr / 60;



        }
    }

    Animator animator;

    public Camera cam;

    bool crouched;

    public TextMeshProUGUI interactText;

    public float interactDistance, holdDistance;

    public bool aimedDownSights = false;
    
    Throwable heldObject = null;

    public GameObject gun;
    bool hasGun = false;

    float hitLevel = 0f;
    float enemyLockLevel = 0f;

    float camXRotation;

    bool reloaded = false;

    // Start is called before the first frame update
    void Start()
    {
        Heartbeat = 60;

        gun.SetActive(false);
        sj =  new Sniperjam();
        sj.Enable();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentSpeed = Vector3.zero;


        camXRotation = cam.transform.rotation.eulerAngles.x;

        sunbscribeEvents();


        Cursor.lockState = CursorLockMode.Locked;

        
    }



    // Update is called once per frame
    void Update()
    {
        movementUpdate();
       
        aimUpdate();

        interactUpdate();

        timeSinceFired += Time.deltaTime;
        UIUpdate();
        
    }

    private void UIUpdate()
    {
        hitLevel -= 0.08f*Time.deltaTime;
        hitLevel = hitLevel < 0 ? 0 : hitLevel;
        Heartbeat = 60 + 50*(hitLevel) + 30 * enemyLockLevel;

        PPController.Singleton.volume.weight = 1 - Mathf.Exp(-2 * (enemyLockLevel + hitLevel));
        PPController.Singleton.vignette.color.SetValue(new UnityEngine.Rendering.ColorParameter(Color.Lerp(Color.black, Color.red, hitLevel), true));


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



        camXRotation = Mathf.Clamp(camXRotation - mouseSense * currentLookInput.y * (aimedDownSights ? .5f : 1f), -80, 80);




        cam.transform.localRotation = Quaternion.Euler(camXRotation, cam.transform.localRotation.eulerAngles.y, cam.transform.localRotation.eulerAngles.z);




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
            animator.ResetTrigger("unADS");
            animator.SetTrigger("ADS");
            reloaded = true;

        }
        else if(true)
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
        if (!reloaded) return;
        timeSinceFired = 0;
        reloaded = false;
        animator.SetTrigger("fire");

        shot.Play();

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.transform.parent.TryGetComponent(out EnemyBehavior e))
            {
                e.die();
                StartCoroutine(enterWinSceenAfterSeconds(3));
            }

        }


    }


    IEnumerator enterWinSceenAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        unsubscribeEvents();
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene("WinScreen");
    }
    
    public void UpdateObservation(float lockLevel)
    {
        enemyLockLevel = lockLevel;
    }

    void unsubscribeEvents()
    {
        sj.Player.Move.performed -= handleMoveInput;
        sj.Player.Move.canceled -= handleMoveInput;

        sj.Player.Look.performed -= handleLookInput;
        sj.Player.Look.canceled -= handleLookInput;

        sj.Player.Crouch.performed -= handleCrouchInput;
        sj.Player.Crouch.canceled -= handleCrouchInput;

        sj.Player.Aim.canceled -= handleADSInput;
        sj.Player.Aim.performed -= handleADSInput;

        sj.Player.Grab.performed -= handleGrabInput;

        sj.Player.Drop.performed -= handleDropInput;


        sj.Player.Fire.performed -= handleFireInput;
    }
    void sunbscribeEvents()
    {
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
    }

    public void TakeHit()
    {
        
        hitLevel = 1;
        health -= 1;

        if(heartContainer.transform.childCount > 0)
        {
            Destroy(heartContainer.transform.GetChild(0).gameObject);
        }

        if(health <= 0)
        {
            unsubscribeEvents();
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("LoseScreen");
        }

        breathing.Play();
        impact.Play();
    }

}
