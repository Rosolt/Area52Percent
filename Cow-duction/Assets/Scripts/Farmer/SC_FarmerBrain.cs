/*  SC_FarmerBrain.cs

    Extends Cow Brain to add lock on and firing states.

    Assumptions:
        There is a GameObject in the scene named "UFO".
 */

using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;

public class SC_FarmerBrain : SC_CowBrain
{
    // Private variables
    private Transform targetTransform;
    private float fireCooldown;
    private int ammoCount;
    private bool lockedOn;
    private bool seekingAmmo;

    // Public variables
    [Header("Public")]
    public float lockOnDistance = 20.0f;
    public float lockOnSpeed = 5.0f;
    public float aimSpeed = 5.0f;
    public float projectileSpeed = 100.0f;
    public float projectileDamage = 5.0f;
    public float projectileKnockback = 5.0f;
    public float projectileLife = 5.0f;
    public float fireRate = 3.0f;
    public int startingAmmo = 5;

    // Serialized private variables
    [Header("Private")]
    [SerializeField] private Transform gunShotOrigin = null; // Set up in inspector
    [SerializeField] private GameObject projectilePrefab = null; // Set up in inspector
    [SerializeField] private AudioClip shotgunPump = null; // Set up in inspector
    [SerializeField] private AudioClip shotgunShot = null; // Set up in inspector

    // Set max speed and agent speed
    public new void SetMaxSpeed(float _maxSpeed)
    {
        base.SetMaxSpeed(_maxSpeed);
        
        if (!m_Agent)
            return;

        if (!lockedOn)
            m_Agent.speed = maxSpeed;
        
    }

    // Awake is called after all objects are initialized
    void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Cam = GetComponentInChildren<Camera>();
        rbFpController = GetComponent<RigidbodyFirstPersonController>();
        targetTransform = GameObject.Find("UFO").transform;
        m_Animator = GetComponentInChildren<Animator>();
        m_AudioSource = GetComponent<AudioSource>();
        fields = GameObject.FindGameObjectsWithTag("Field");
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
    }

    // Start is called before the first frame update
    void Start()
    {
        lockedOn = false;
        seekingAmmo = false;
        fireCooldown = fireRate;
        ammoCount = startingAmmo;
        wandering = true;
        Wander();
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_Agent.enabled && m_Agent.isOnNavMesh && aiControlled)
        {
            if (fireCooldown < fireRate)
            {
                fireCooldown += Time.deltaTime;
            }
            if (!lockedOn)
            {
                // Smoothly rotate to face forward
                m_Cam.transform.rotation = Quaternion.Slerp(m_Cam.transform.rotation, transform.rotation, lockOnSpeed * Time.deltaTime);
                m_Animator.transform.rotation = Quaternion.Slerp(m_Animator.transform.rotation, transform.rotation, lockOnSpeed * Time.deltaTime);

                if (!seekingAmmo)
                {
                    if (wandering && wanderTime < maxWanderTime)
                    {
                        wanderTime += Time.deltaTime;
                    }
                    if (m_Agent.remainingDistance <= 1f || wanderTime >= maxWanderTime)
                    {
                        Wander();
                    }
                    if (Vector3.Distance(transform.position, targetTransform.position) <= lockOnDistance)
                    {
                        LockOn();
                    }
                }
            }
            else if (!seekingAmmo)
            {
                // Set destination to UFO xz-position
                m_Agent.destination = new Vector3(targetTransform.position.x, 0, targetTransform.position.z);
                currentDestination = m_Agent.destination;
                
                // Smoothly rotate camera towards UFO
                Quaternion targetRotation = Quaternion.LookRotation(targetTransform.position - m_Cam.transform.position);
                m_Cam.transform.rotation = Quaternion.Slerp(m_Cam.transform.rotation, targetRotation, lockOnSpeed * Time.deltaTime);
                
                // Smoothly rotate farmer model towards UFO
                Vector3 farmerForward = new Vector3(m_Cam.transform.forward.x, 0, m_Cam.transform.forward.z);
                m_Animator.transform.forward = Vector3.Lerp(farmerForward, m_Animator.transform.forward, lockOnSpeed * Time.deltaTime);

                // Disengage if too far away from UFO or if UFO is invisible
                if ((Vector3.Distance(transform.position, targetTransform.position) > lockOnDistance) ||
                    (!targetTransform.GetComponentInChildren<MeshRenderer>().enabled))
                {
                    Disengage();
                }
                else if (fireCooldown >= fireRate)
                {
                    FireWeapon();
                }
            }
        }

        // Set animator parameter
        if (m_Animator)
            m_Animator.SetFloat("speed", m_Agent.velocity.magnitude);
    }

    // Refill ammo on collision with field
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Field")
        {
            if (seekingAmmo)
                RefillAmmo();
        }
    }

    // Move towards and aim at target
    private void LockOn()
    {
        // Do not lock on if seeking ammo
        if (seekingAmmo)
            return;

        // Do not lock on if UFO is invisible
        if (!targetTransform.GetComponentInChildren<MeshRenderer>().enabled)
        {
            Disengage();
            return;
        }
        
        // Do not lock on if UFO is not in line of sight
        Debug.DrawRay(m_Cam.transform.position, (targetTransform.position - m_Cam.transform.position), Color.yellow);
        if (Physics.Raycast(m_Cam.transform.position, (targetTransform.position - m_Cam.transform.position), out RaycastHit hit))
        {
            if (hit.transform != targetTransform)
            {
                Disengage();
                return;
            }
        }

        lockedOn = true;
        wandering = false;
        m_Agent.speed = aimSpeed;
        fireCooldown = 0.0f;

        // Set animator parameter
        if (m_Animator)
            m_Animator.SetBool("lockedOn", lockedOn);
        
        // Play gun pump audio clip
        if (m_AudioSource)
            m_AudioSource.PlayOneShot(shotgunPump);
    }

    // Go back to wandering state
    private void Disengage()
    {
        lockedOn = false;
        wandering = true;
        m_Agent.speed = maxSpeed;

        // Set animator parameter
        if (m_Animator)
            m_Animator.SetBool("lockedOn", lockedOn);
    }

    // Shoot a projectile from gunShotOrigin
    private void FireWeapon()
    {
        // Assume gunShotOrigin is parented to the camera
        GameObject projectileClone = Instantiate(projectilePrefab, gunShotOrigin.position, gunShotOrigin.rotation);
        projectileClone.GetComponent<Rigidbody>().AddForce(gunShotOrigin.forward * projectileSpeed, ForceMode.Impulse);

        // Apply farmer projectile parameters
        SC_Projectile projectile = projectileClone.GetComponent<SC_Projectile>();
        if (projectile)
        {
            projectile.SetProjectileDamage(projectileDamage);
            projectile.SetProjectileKnockback(projectileKnockback);
        }

        fireCooldown = 0.0f;
        ammoCount--;
        
        // Look for closest field if out of ammo
        if (ammoCount < 1)
        {
            Disengage();
            float minDist = Mathf.Infinity;
            Vector3 targetArea = targetTransform.position;
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
            seekingAmmo = true;
        }
        
        // Play gun shot audio clip
        if (m_AudioSource)
            m_AudioSource.PlayOneShot(shotgunShot);

        StartCoroutine(DestroyClone(projectileClone));
    }

    // Refill ammo to startingAmmo then wander
    private void RefillAmmo()
    {
        ammoCount = startingAmmo;
        seekingAmmo = false;
        Wander();
    }

    // Destroy the clone after a set amount of time
    private IEnumerator DestroyClone(GameObject clone)
    {
        yield return new WaitForSeconds(projectileLife);

        if (clone)
            Destroy(clone);
    }
}
