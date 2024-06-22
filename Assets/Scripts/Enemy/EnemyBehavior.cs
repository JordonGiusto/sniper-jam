using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class EnemyBehavior : MonoBehaviour
{
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


    float currentLock = 0;
    float rayLockContrib;
    Vector3[] offsets;
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
        sm = new StateMachine<States, Commands>(States.Active);
        sm.Addtransition(States.Active, States.Distracted, Commands.Distract);
        sm.Addtransition(States.Distracted, States.Active, Commands.LockIn);
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
            default:
                break;
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
        PPController.Singleton.volume.weight = 1-Mathf.Exp(-2*currentLock);
        if(currentLock >= 1) 
        {
            print("Bam");
            PPController.Singleton.vignette.color.SetValue(new UnityEngine.Rendering.ColorParameter(Color.red, true));
            //PPController.Singleton.volume.profile.Reset();

        }
        

        return hits;
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
        Distracted
    }
    enum Commands
    {
        None,
        Distract,
        LockIn,
        Attack,
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



    public StateMachine(S _defaultState)
    {
        stateTransitions = new Dictionary<S, Dictionary<C, S>>();
        stateEnterActions = new Dictionary<S, Action>();
        stateUpdateActions = new Dictionary<S, Func<float, C>>();
        stateExitActions = new Dictionary<S, Action>();

        defaultState = _defaultState;
        timeInState = 0;
        currentState = defaultState;
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
        if (stateTransitions[fromState] == null)
        {
            stateTransitions[fromState] = new Dictionary<C, S>();
        }
        stateTransitions[fromState][trigger] = toState;
    }
        
}


