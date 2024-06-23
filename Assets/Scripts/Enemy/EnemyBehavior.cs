using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField]
    PlayerController playerController;

    [SerializeField]
    Transform player;


    [SerializeField]
    Vector2Int viewGrid;
    [SerializeField]
    float fovScale;
    [SerializeField]
    float lockTime;
    [SerializeField]
    float distractedTime;
    [SerializeField]
    float reloadTime;
    [SerializeField]
    GameObject impact;

    float currentLock = 0;
    float rayLockContrib;
    Vector3[] offsets;

    AudioSource audioSource;
    Transform eyes;
    StateMachine<States, Commands> sm;

    States currentState => sm.currentState;

    Queue<Commands> triggerQueue;

    public bool Distracted
    {
        set
        {
            if (!value)
            {
                triggerQueue.Enqueue(Commands.LockIn);
                return;
            }
            triggerQueue.Enqueue(Commands.Distract);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        eyes = transform.Find("Eyes");
        offsets = new Vector3[viewGrid.x*viewGrid.y];
        Vector2 botLeft = (Vector2)(-viewGrid+Vector2Int.one) * 0.5f*fovScale;

        for(int x = 0; x < viewGrid.x; x++)
        {
            for(int y = 0; y < viewGrid.y; y++)
            {
                offsets[x * viewGrid.y + y] = fovScale*(new Vector2(x,y)) + botLeft;
            }
        }
        rayLockContrib = 1 /lockTime/ (viewGrid.x * viewGrid.y);
        triggerQueue = new Queue<Commands>();
        CreateStateMachine();
    }

    void CreateStateMachine()
    {
        sm = new StateMachine<States, Commands>(States.Active, Commands.None, States.Reloading, States.Distracted);
        sm.Addtransition(States.Active, States.Distracted, Commands.Distract);
        sm.Addtransition(States.Distracted, States.Active, Commands.LockIn);
        sm.Addtransition(States.Active, States.Reloading, Commands.Reload);
        sm.Addtransition(States.Reloading, States.Active, Commands.LockIn);

        sm.stateEnterActions[States.Reloading] += () => { StartCoroutine(ReloadRoutine()); };


        sm.stateUpdateActions[States.Active] += (t) => Commands.None;

        sm.stateEnterActions[States.Distracted] += WarningShot;
        sm.stateUpdateActions[States.Distracted] += (t) =>
        {
            if(sm.timeInState > distractedTime)
            {
                return Commands.LockIn;
            }
            return Commands.None;
        };
        
    }
    
    void Update()
    {
        UpdateStateMachine();
        switch (currentState)
        {
            case States.Active:
                LookForPlayer();
                break;
            case States.Distracted:
                break;
            case States.Reloading:

                break;
            default:
                break;
        }
        

    }
    IEnumerator ReloadRoutine()
    {
        currentLock = 0.2f;
        while (true)
        {
            playerController.UpdateObservation(currentLock);
            
            if (sm.timeInState >= reloadTime)
            {
                triggerQueue.Enqueue(Commands.LockIn);
                yield break;
            }

            yield return null;
        }

    }
    void UpdateStateMachine()
    {
        if (sm.Update(Time.deltaTime, triggerQueue))
        {
            triggerQueue.Clear();
        }
    }
    int LookForPlayer()
    {
        int hits = 0;
        Vector3 toPlayer = player.position - eyes.position;
        foreach(Vector3 o in offsets)
        {
            Ray look = new Ray(eyes.position, (Quaternion.LookRotation(toPlayer.normalized) * o / toPlayer.magnitude + toPlayer.normalized) * toPlayer.magnitude * 1.2f);
            if (Physics.Raycast(look, out RaycastHit hit, toPlayer.magnitude*1.1f))
            {
                if (hit.collider.transform.Equals(player))
                {
                    hits++;
                }
            }
        }
        if(hits > 0)
        {
            currentLock += hits * rayLockContrib * Time.deltaTime;
        }
        else
        {
            currentLock = Mathf.Lerp(0,currentLock, Mathf.Exp(-0.5f*Time.deltaTime));
        }
        playerController.UpdateObservation(currentLock);
        
        if(currentLock >= 1) 
        {
            TakeShot(hits);
        }

        transform.LookAt(player);
        return hits;
    }
    void TakeShot(int hits)
    {
        
        audioSource.Play();
        triggerQueue.Enqueue(Commands.Reload);

        float hitProb = 0.8f*((float)hits) / viewGrid.x / viewGrid.y+0.1f;

        if (UnityEngine.Random.value < hitProb)
        {
            playerController.TakeHit();
        }
        else
        {
            currentLock = 0.2f;
            playerController.UpdateObservation(currentLock);
            Vector3 toPlayer = player.position - eyes.position;
            Vector3 o = UnityEngine.Random.insideUnitCircle * fovScale * viewGrid.y * (1 + UnityEngine.Random.value);
            Physics.Raycast(eyes.position, (Quaternion.LookRotation(toPlayer.normalized) * o / toPlayer.magnitude + toPlayer.normalized), out RaycastHit info);
            Instantiate(impact, info.point, Quaternion.identity);
        }
    }
    void WarningShot()
    {
        print("Shot at dibris");
    }
    private void OnDrawGizmos()
    {
        if(eyes == null || player == null)
        {
            return;
        }
        Vector3 toPlayer = player.position - eyes.position;
        foreach(Vector3 o in offsets)
        {
            Gizmos.DrawRay(eyes.position, (Quaternion.LookRotation(toPlayer.normalized)*o/toPlayer.magnitude+toPlayer.normalized)*toPlayer.magnitude*1.2f);
        }
       
    }

    enum States
    {
        Active,
        Distracted,
        Reloading,
    }
    enum Commands
    {
        None,
        Distract,
        LockIn,
        Attack,
        Reload,
    }
}

public class StateMachine<S, C> where S: Enum where C : Enum
{
    public S currentState;
    public float timeInState;
    S defaultState;

    Dictionary<S, Dictionary<C,S>> stateTransitions;

    
    public Dictionary<S, Action> stateEnterActions;
    public Dictionary<S, Func<float, C>> stateUpdateActions;
    public Dictionary<S, Action> stateExitActions;



    public StateMachine(S _defaultState, C nullCommand, params S[] states)
    {
        stateTransitions = new Dictionary<S, Dictionary<C, S>>();
        stateEnterActions = new Dictionary<S, Action>();
        stateUpdateActions = new Dictionary<S, Func<float, C>>();
        stateExitActions = new Dictionary<S, Action>();

        defaultState = _defaultState;
        timeInState = 0;
        currentState = defaultState;

        stateEnterActions[defaultState] = () => { };
        stateUpdateActions[defaultState] = (t) => nullCommand;
        stateExitActions[defaultState] = () => { timeInState = 0; };
        foreach(S s in states)
        {
            stateEnterActions[s] = () => { };
            stateUpdateActions[s] = (t) => nullCommand;
            stateExitActions[s] = () => { timeInState = 0; };
        }
    }
    

    public bool Update(float t, Queue<C> trigs)
    {
        if(trigs.Count > 0)
        {
            foreach(C trig in trigs) 
            {
                if (stateTransitions[currentState]?.ContainsKey(trig) ?? false)
                {
                    stateExitActions[currentState]?.Invoke();
                    currentState = stateTransitions[currentState][trig];
                    stateEnterActions[currentState]?.Invoke();

                    return true;
                }
            }
        }
        timeInState += t;
        C command = stateUpdateActions[currentState](t);
        if (stateTransitions[currentState]?.ContainsKey(command) ?? false)
        {
            stateExitActions[currentState]?.Invoke();
            currentState = stateTransitions[currentState][command];
            stateEnterActions[currentState]?.Invoke();

            return true;
        }
        return false;
    }
    public void Addtransition(S fromState, S toState, C trigger) 
    {
        if (!stateTransitions.ContainsKey(fromState))
        {
            stateTransitions[fromState] = new Dictionary<C, S>();
        }
        stateTransitions[fromState][trigger] = toState;
    }
        
}


