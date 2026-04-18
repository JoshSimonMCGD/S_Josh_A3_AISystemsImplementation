using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class NewMonoBehaviourScript : MonoBehaviour
{

    public enum State { Idle, Patrol, Search, Chase, Hunt, Flee }

    [Header("Scene References")]
    public GameObject character;

    public Transform[] waypoints;

    [Header("Config Values")]
    public float waypointThreshold = 0.5f;
    public float idleThreshold = 1.0f;
    public float searchThreshold = 3.0f;
    public float viewRadius = 10f;
    public float viewAngle = 60f;
    public float huntTime= 8.0f;
    public float huntDistance = 2f;
    public float huntThreshold = 5.0f;
    private int waypointIndex = 0;
    private WolfBehavior wolfBehavior;
    Vector3 fleeLocation = Vector3.zero;
    public float fleeHealthThreshold = 0.25f;
    public float fleeThreshold = 1.5f;

    int GetRandomWaypointIndex()   // Randomizes patrol
    {
        if (waypoints == null || waypoints.Length == 0)
            return 0;

        return UnityEngine.Random.Range(0, waypoints.Length);
    }

    NavMeshAgent agent;

    bool viewEnabled = false;
    bool canSeePlayer = false;
    bool soundHeard = false;
    bool hasFled = false;

    Vector3 soundLocation = Vector3.zero;

    float idleTime = 0.0f;
    float searchTime = 0.0f;

    Animator anim;

    State state;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        anim = GetComponent<Animator>();


        wolfBehavior = GetComponent<WolfBehavior>();    // reference for Wolf audio listener
        if (wolfBehavior != null)
        {
            wolfBehavior.OnSoundTriggered.AddListener(OnWolfHowlHeard);
        }

        state = State.Idle;
        idleTime = Time.time;
    }

    private void Update()
    {
        if (wolfBehavior != null && wolfBehavior.GetHealthPercent() <= fleeHealthThreshold && state != State.Flee)  //Flee trigger referencing Wolf HP
        {
            EnterFlee();
        }

        switch (state)
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrol:
                Patrol();
                break;
            case State.Search:
                Search();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Hunt:
                Hunt();
                break;
            case State.Flee:
                Flee();
                break;
        }
    }

    void EnterFlee()    // Flee state
    {
        hasFled = true;
        state = State.Flee;

        int randomIndex = GetRandomWaypointIndex();
        fleeLocation = waypoints[randomIndex].position;
    }

    void Flee()
    {
        agent.SetDestination(fleeLocation);

        float distance = Vector3.Distance(transform.position, fleeLocation);
        if (distance <= fleeThreshold)
        {
            state = State.Idle;
            idleTime = Time.time;
        }
    }

    void OnWolfHowlHeard(WolfBehavior sourceWolf)
    {
        if (sourceWolf == null)
            return;

        soundHeard = true;
        soundLocation = sourceWolf.transform.position;
    }
    void EnterHunt()     //Hunt state is new and not on the documents. It was a design of in the moment inspiration
    {
        state = State.Hunt;
        huntTime = Time.time;
        soundHeard = false;
    }
    void Idle()
    {
        viewEnabled = true;
        canSeePlayer = InViewCone();
        // Idle time is set when the npc enters Idle
        float timeElapsed = Time.time - idleTime;
        if (timeElapsed >= idleThreshold)
        {
            state = State.Patrol;
            waypointIndex = GetRandomWaypointIndex();
        }

        if (soundHeard)
        {
            EnterHunt();
        }
        
    }

    void Patrol()
    {
        Vector3 waypoint = waypoints[waypointIndex].position;

        agent.SetDestination(waypoint);

        viewEnabled = true;
        canSeePlayer = InViewCone();


        float distance = Vector3.Distance(transform.position, waypoint);
        if (Vector3.Distance(transform.position, waypoint) < waypointThreshold)
        {
            waypointIndex = GetRandomWaypointIndex();
            // Example of leaky state code. Idle state needs the time when entered, but that is set in Patrol, and must be set every time
            state = State.Idle;
            idleTime = Time.time;
        }
        if (canSeePlayer)
        {
            state = State.Chase;
        }

        if (soundHeard)
        {
            EnterHunt();
        }
        
    }

    void Search()  // Not used
    {
        agent.SetDestination(transform.position + transform.forward + transform.right);
        float elapsedSearchTime = Time.time - searchTime;
        if(elapsedSearchTime > searchThreshold)
        {
            state = State.Patrol;
        }

        if (canSeePlayer)
        {
            state = State.Chase;
        }       

        if (!canSeePlayer)
        {
            state = State.Patrol;
        }
    }

    void Chase()
    {
        agent.SetDestination(character.transform.position);

        canSeePlayer = true;
        if (!canSeePlayer)
        {
            state = State.Search;
            searchTime = Time.time;
        }
    }

    void Hunt()     // Goes to sound source for an alloted amount of time
    {
        agent.SetDestination(soundLocation);

        float distance = Vector3.Distance(transform.position, soundLocation);
        
        if(distance <= huntDistance)
        {
            float timeElapsed = Time.time - huntTime;
            if(timeElapsed >= huntThreshold)
            {
                state = State.Patrol;
            }
        }
        else
        {
            huntTime = Time.time;
        }

        if (canSeePlayer)
        {
            state = State.Chase;
        }
    }


    bool InViewCone()
    {
        if (Vector3.Distance(transform.position, character.transform.position) > viewRadius)
        return false;

        Vector3 npcToCharacter = character.transform.position - transform.position;
        if (Vector3.Angle(transform.forward, npcToCharacter) > 0.5f * viewAngle)
        return false;

        Vector3 toCharacterDir = npcToCharacter.normalized;
        if (Physics.Raycast(transform.position, toCharacterDir, out RaycastHit ray, viewRadius))
        {
            return ray.transform == character.transform;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Transform waypoint in waypoints)
        {
            Gizmos.DrawWireSphere(waypoint.position, 0.5f);
        }

        if (viewEnabled)
        {
            Handles.color = new Color(0f, 1f, 1f, 0.25f);

            if (canSeePlayer) Handles.color = new Color(1f, 0f, 0f, 0.25f);

            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, viewAngle/2, viewRadius);
            Handles.DrawSolidArc(transform.position, Vector3.up, transform.forward, -viewAngle/2, viewRadius);
        }
    }
}
