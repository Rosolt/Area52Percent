﻿/* CowBrain.cs

    Simple AI that randomly selects a destination on a flat plane to travel to.
    The destination updates once the agent is within 1 meter of it.
   
   Assumptions:
     This component is attached to a GameObject with Collider, NavMeshAgent and Rigidbody components.
 */

using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(NavMeshAgent))]
public class CowBrain : MonoBehaviour
{
    [SerializeField] private NavMeshAgent cowAgent;
    [SerializeField] private Camera cowCam;
    [SerializeField] private RigidbodyFirstPersonController rbFpController;
    [SerializeField] private int minX;
    [SerializeField] private int maxX;
    [SerializeField] private int minZ;
    [SerializeField] private int maxZ;
    [SerializeField] private float wanderTime;
    [SerializeField] private float maxWanderTime;
    [SerializeField] private bool aiControlled;

    // Start is called before the first frame update
    void Start()
    {
        cowAgent = GetComponent<NavMeshAgent>();
        cowCam = GetComponentInChildren<Camera>();
        rbFpController = GetComponent<RigidbodyFirstPersonController>();
        if (aiControlled)
        {
            SetPlayerControlled(false);
            cowAgent.destination = new Vector3(Random.Range(minX, maxX), 0f, Random.Range(minZ, maxZ));
        }
        maxWanderTime = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        if (aiControlled)
        {
            if (wanderTime < maxWanderTime)
            {
                wanderTime += Time.deltaTime;
            }
            if (cowAgent.enabled && (cowAgent.remainingDistance < 1f || wanderTime >= maxWanderTime))
            {
                cowAgent.destination = new Vector3(Random.Range(minX, maxX), 0f, Random.Range(minZ, maxZ));
                wanderTime = 0f;
            }
        }
    }

    public void SetPlayerControlled(bool control)
    {
        if (control)
        {
            aiControlled = false;
            if (cowCam)
                cowCam.enabled = true;
            if (rbFpController)
                rbFpController.enabled = true;
        }
        else
        {
            aiControlled = true;
            if (cowCam)
                cowCam.enabled = false;
            if (rbFpController)
                rbFpController.enabled = false;
        }
    }
}
