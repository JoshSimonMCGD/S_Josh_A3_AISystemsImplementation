using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class NewMonoBehaviourScript : MonoBehaviour
{

public enum State { Idle, Patrol, Search, Chase }

[Header("Scene References")]
public GameObject character;

public Transform[] waypoints;

[Header("Config Values")]
public float waypointThreshold = 0.5f;
public float idleThreshold = 1.0f;
public float searchThreshold = 3.0f;
public float viewRadius = 10f;
public float viewAngle = 60f;
private int waypointIndex = 0;

NavMeshAgent agent;

bool viewEnabled = false;
bool canSeePlayer = false;

float idleTime = 0.0f;
float searchTime = 0.0f;

State state;

private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        state = State.Idle;
        idleTime = Time.time;
    }

private void Update()
    {
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
        }
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
            waypointIndex++;
            if (waypointIndex >= waypoints.Length) waypointIndex = 0;
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
            waypointIndex++;
            if (waypointIndex >= waypoints.Length) waypointIndex = 0;
            // Example of leaky state code. Idle state needs the time when entered, but that is set in Patrol, and must be set every time
            state = State.Idle;
            idleTime = Time.time;
        }
        if (canSeePlayer)
        {
            state = State.Chase;
        }
        
    }

    void Search()
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
