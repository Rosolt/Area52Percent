/* SC_CowBrain.cs

    Simple AI that randomly selects a destination on a flat plane to travel to.
    The destination updates once the agent is within 1 meter of it.
   
   Assumptions:
     This component is attached to a GameObject with Collider, NavMeshAgent and Rigidbody components.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class SC_CowBrain : MonoBehaviour
{
    // Private variables
    private bool seekingFood = true;

    // Protected variables
    protected GameObject[] fields; // Tagged as Field
    protected RigidbodyFirstPersonController rbFpController;
    protected NavMeshAgent m_Agent;
    protected Camera m_Cam;
    protected AudioSource m_AudioSource;
    protected Animator m_Animator;
    protected Vector3 currentDestination; // Keeps track of destination while agent disabled
    protected float wanderTime = 0.0f;
    protected bool wandering = true;

    // Public variables
    [Header("Public")]
    public float maxSpeed = 8.0f;
    public float maxWanderTime = 10.0f;
    public float milk = 10.0f;
    public bool tugWhenGrappled = false;

    // Serialized private variables
    [Header("Private")]
    [SerializeField] private AudioClip cowMoo = null; // Set up in inspector

    // Serialized protected variables
    [Header("Protected")]
    [SerializeField] protected float fieldRadius = 5.0f;
    [SerializeField] protected int wanderRadius = 100;
    [SerializeField] protected float idleTime = 3.0f;
    [SerializeField] protected float recoveryTime = 3.0f;
    [SerializeField] protected bool aiControlled = true;

    // Get mass of rigidbody
    public float GetMass()
    {
        return GetComponent<Rigidbody>().mass;
    }

    // Get local scale of transform
    public float GetSize()
    {
        return transform.localScale.x;
    }

    // Set rigidbody mass
    public void SetMass(float _mass)
    {
        GetComponent<Rigidbody>().mass = _mass;
    }

    // Set transform local scale
    public void SetSize(float _size)
    {
        transform.localScale = Vector3.one * _size;
    }

    // Set max speed and agent speed
    public void SetMaxSpeed(float _maxSpeed)
    {
        maxSpeed = _maxSpeed;

        if (!m_Agent)
            return;

        if (wandering || seekingFood)
            m_Agent.speed = maxSpeed;
    }

    // Awake is called after all objects are initialized
    private void Awake()
    {
        // Retrieve components
        m_Agent = GetComponent<NavMeshAgent>();
        m_Cam = GetComponentInChildren<Camera>();
        m_AudioSource = GetComponent<AudioSource>();
        m_Animator = GetComponentInChildren<Animator>();
        rbFpController = GetComponent<RigidbodyFirstPersonController>();
        fields = GameObject.FindGameObjectsWithTag("Field");

        // If AI controlled, disable player controls and set destination
        if (aiControlled)
        {
            SetPlayerControlled(false);
            m_Agent.destination = Random.insideUnitSphere * wanderRadius;
            currentDestination = m_Agent.destination;
        }
        else
        {
            SetPlayerControlled(true);
        }

        // Cow immediately seeks food after spawning
        SeekFood();
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_Agent.enabled && m_Agent.isOnNavMesh && aiControlled)
        {
            if (!seekingFood)
            {
                // Seek food after done wandering
                if (wandering && wanderTime < maxWanderTime)
                {
                    wanderTime += Time.deltaTime;
                }
                else
                {
                    SeekFood();
                }
            }
            if (m_Agent.remainingDistance <= fieldRadius)
            {
                Wander();

                // Play cow moo audio clip
                if (m_AudioSource && cowMoo)
                    m_AudioSource.PlayOneShot(cowMoo);
            }
        }

        // Set animator parameter
        if (m_Animator)
            m_Animator.SetFloat("speed", m_Agent.velocity.magnitude);
    }

    // Satisfy hunger on contact with field
    private void OnCollisionStay(Collision collision)
    {
        if (seekingFood && collision.gameObject.tag == "Field")
        {
            SatisfyHunger();
        }
    }

    // Allow agent to be knocked over
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody && (collision.rigidbody.velocity.magnitude * collision.rigidbody.mass) > (GetComponent<Rigidbody>().velocity.magnitude * GetComponent<Rigidbody>().mass))
        {
            if (m_Agent && m_Agent.enabled)
            {
                m_Agent.enabled = false;
                if (!GetComponent<NavMeshObstacle>())
                    gameObject.AddComponent<NavMeshObstacle>();
                StartCoroutine(Recover());
            }
        }
    }

    // Re-enable agent after a set period of time
    public IEnumerator Recover()
    {
        yield return new WaitForSeconds(recoveryTime);
        
        if (this)
        {
            // Smoothly rotate object upright
            while (transform.localEulerAngles.z > 1f && transform.localEulerAngles.z < 359f)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                Quaternion deltaQuat = Quaternion.FromToRotation(transform.up, Vector3.up);

                Vector3 axis;
                float angle;
                deltaQuat.ToAngleAxis(out angle, out axis);

                float dampenFactor = 2f; // this value requires tuning
                rb.AddTorque(-rb.angularVelocity * dampenFactor, ForceMode.Acceleration);

                float adjustFactor = 1f; // this value requires tuning
                rb.AddTorque(axis.normalized * angle * adjustFactor, ForceMode.Acceleration);
                
                yield return null;
            }

            // Remove obstacle component and re-enable agent
            if (GetComponent<NavMeshObstacle>())
                Destroy(GetComponent<NavMeshObstacle>());
            
            m_Agent.enabled = true;
            if (!m_Agent.hasPath)
                m_Agent.destination = currentDestination;
        }
    }

    // Set destination as the closest field
    private void SeekFood()
    {
        if (!m_Agent.enabled)
            return;
        
        seekingFood = true;
        wandering = false;

        // Find the closest field
        float minDist = Mathf.Infinity;
        Vector3 targetArea = Vector3.zero;
        foreach (GameObject field in fields)
        {
            float dist = Vector3.Distance(field.transform.position, transform.position);
            if (dist < minDist)
            {
                targetArea = field.transform.position;
                minDist = dist;
            }
        }
        m_Agent.destination = targetArea;
        currentDestination = m_Agent.destination;
        m_Agent.stoppingDistance = fieldRadius;
    }

    // Stop seeking food
    private void SatisfyHunger()
    {
        seekingFood = false;
        StartCoroutine(Idle());
    }

    // Choose a random destination
    protected void Wander()
    {
        if (!m_Agent.enabled)
            return;
        
        wandering = true;
        m_Agent.destination = Random.insideUnitSphere * wanderRadius;
        currentDestination = m_Agent.destination;
        m_Agent.stoppingDistance = 0f;
        wanderTime = 0f;
    }

    // Wait for a set amount of seconds before moving again
    protected IEnumerator Idle()
    {
        m_Agent.speed = 1.0f;

        yield return new WaitForSeconds(idleTime);

        m_Agent.speed = maxSpeed;
        Wander();
    }

    // Toggle between AI controlled and Player controlled
    protected void SetPlayerControlled(bool control)
    {
        if (control)
        {
            aiControlled = false;
            if (m_Cam)
                m_Cam.enabled = true;
            if (rbFpController)
                rbFpController.enabled = true;
        }
        else
        {
            aiControlled = true;
            if (m_Cam)
                m_Cam.enabled = false;
            if (rbFpController)
                rbFpController.enabled = false;
        }
    }
}
