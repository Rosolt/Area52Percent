/* GameManager.cs
   
   Handles loading scenes and resetting game.

   Assumptions:
     All private components may be found in the scene.
     Prefabs, parameter menu and sliders are set up through the inspector.
     Slider names correspond to one of the cases in the switch statements.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    // Private variables
    private GameObject spaceship;
    private SC_AlienUIManager uiManager;
    private SC_SpaceshipMovement spaceshipMovement;
    private SC_CowAbduction cowAbduction;
    private SC_HudReticleFollowCursor hudReticle;
    private SC_RadarCamera radarCamera;
    private SC_CowSpawner cowSpawner;
    private List<SC_CowBrain> cowBrains;
    private List<SC_FarmerBrain> farmerBrains;
    private GameObject[] sliders = null;

    // Serialized private variables
    [Header("Private")]
    [SerializeField] private SC_CowBrain cowBase = null; // Set up in inspector
    [SerializeField] private SC_FarmerBrain farmerBase = null; // Set up in inspector
    [SerializeField] private GameObject parameterMenu = null; // Set up in inspector
    [SerializeField] private GameObject spaceshipSliders = null; // Set up in inspector
    [SerializeField] private GameObject cowSliders = null; // Set up in inspector
    [SerializeField] private GameObject farmerSliders = null; // Set up in inspector
    [SerializeField] private bool saveParameters = true;

    // Awake is called after all objects are initialized
    void Awake()
    {
        // Find components
        spaceship = GameObject.FindWithTag("UFO");
        uiManager = spaceship.GetComponentInChildren<SC_AlienUIManager>();
        spaceshipMovement = spaceship.GetComponentInChildren<SC_SpaceshipMovement>();
        cowAbduction = spaceship.GetComponentInChildren<SC_CowAbduction>();
        hudReticle = spaceship.GetComponentInChildren<SC_HudReticleFollowCursor>();
        radarCamera = spaceship.GetComponentInChildren<SC_RadarCamera>();
        cowSpawner = GameObject.Find("CowSpawner").GetComponent<SC_CowSpawner>();

        // Set up gameobjects before disabling them
        if (parameterMenu && !parameterMenu.activeSelf)
            parameterMenu.SetActive(true);
        if (spaceshipSliders && !spaceshipSliders.activeSelf)
            spaceshipSliders.SetActive(true);
        if (cowSliders && !cowSliders.activeSelf)
            cowSliders.SetActive(true);
        if (farmerSliders && !farmerSliders.activeSelf)
            farmerSliders.SetActive(true);
        sliders = GameObject.FindGameObjectsWithTag("ParameterSlider");

        // Find multiple instances of components (brains)
        FindFarmerBrains();
        StartCoroutine(FindCowBrains());

        // Load all parameters
        LoadParameters();
    }

    // Add farmer brains to list
    private void FindFarmerBrains()
    {
        farmerBrains = new List<SC_FarmerBrain>();

        GameObject[] farmers = GameObject.FindGameObjectsWithTag("Farmer");
        for (int f = 0; f < farmers.Length; f++)
        {
            farmerBrains.Add(farmers[f].GetComponent<SC_FarmerBrain>());
        }
    }

    // Apply parameter changes to farmers
    private void UpdateFarmerBrains(string parameter)
    {
        foreach (SC_FarmerBrain farmer in farmerBrains)
        {
            switch (parameter)
            {
                case "FarmerLockOnDistance":
                    farmer.lockOnDistance = farmerBase.lockOnDistance;
                    break;
                case "FarmerLockOnSpeed":
                    farmer.lockOnSpeed = farmerBase.lockOnSpeed;
                    break;
                case "FarmerNormalSpeed":
                    farmer.SetMaxSpeed(farmer.maxSpeed);
                    break;
                case "FarmerAimSpeed":
                    farmer.aimSpeed = farmerBase.aimSpeed;
                    break;
                case "FarmerProjectileSpeed":
                    farmer.projectileSpeed = farmerBase.projectileSpeed;
                    break;
                case "FarmerProjectileDamage":
                    farmer.projectileDamage = farmerBase.projectileDamage;
                    break;
                case "FarmerProjectileKnockback":
                    farmer.projectileKnockback = farmerBase.projectileKnockback;
                    break;
                case "FarmerProjectileLife":
                    farmer.projectileLife = farmerBase.projectileLife;
                    break;
                case "FarmerFireRate":
                    farmer.fireRate = farmerBase.fireRate;
                    break;
                case "FarmerStartingAmmo":
                    farmer.startingAmmo = farmerBase.startingAmmo;
                    break;
                default:
                    break;
            }
        }
    }

    // Continuously add cow brains to list
    private IEnumerator FindCowBrains()
    {
        GameObject[] cows;
        while (true)
        {
            cowBrains = new List<SC_CowBrain>();

            cows = GameObject.FindGameObjectsWithTag("Cow");
            for (int c = 0; c < cows.Length; c++)
            {
                cowBrains.Add(cows[c].GetComponent<SC_CowBrain>());
            }
            
            yield return new WaitUntil(() => cows.Length < cowSpawner.GetCowAmount());
        }
    }

    // Apply parameter changes to cows
    private void UpdateCowBrains()
    {
        foreach (SC_CowBrain cow in cowBrains)
        {
            cow.SetMass(cowBase.GetMass());
            cow.SetSize(cowBase.GetSize());
            cow.SetMaxSpeed(cowBase.maxSpeed);
            cow.milk = cowBase.milk;
            cow.maxWanderTime = cowBase.maxWanderTime;
        }
    }

    // Apply toggle settings
    public void SetBool(Toggle toggle)
    {
        // TO DO: Parameters may need tweaking based on playtesting
        switch(toggle.name)
        {
            case "GodMode":
                List<ParameterSlider> godModeParameters = new List<ParameterSlider>();
                List<Slider> godModeSliders = new List<Slider>();
                foreach (GameObject slider in sliders)
                {
                    switch (slider.name)
                    {
                        case "FarmerProjectileDamage":    // 0
                        case "FarmerProjectileKnockback": // 1
                        case "FuelDepletionRate":         // 2
                            godModeParameters.Add(slider.GetComponent<ParameterSlider>());
                            godModeSliders.Add(slider.GetComponent<Slider>());
                            break;
                        default:
                            break;
                    }
                }
                if (toggle.isOn)
                {
                    godModeParameters[0].SetFloatValue(0.0f);
                    godModeParameters[1].SetFloatValue(0.0f);
                    godModeParameters[2].SetFloatValue(0.0f);

                    SetFloat(godModeSliders[0]);
                    SetFloat(godModeSliders[1]);
                    SetFloat(godModeSliders[2]);
                }
                else
                {
                    foreach (ParameterSlider ps in godModeParameters)
                    {
                        ps.ResetValue();
                    }
                }
                break;
            case "JoyRide":
                List<ParameterSlider> joyRideSliders = new List<ParameterSlider>();
                foreach (GameObject slider in sliders)
                {
                    switch (slider.name)
                    {
                        case "SpaceshipSize":     // 0
                        case "FuelDepletionRate": // 1
                        case "HorizontalSpeed":   // 2
                        case "VerticalSpeed":     // 3
                        case "RotationSpeed":     // 4
                            joyRideSliders.Add(slider.GetComponent<ParameterSlider>());
                            break;
                        default:
                            break;
                    }
                }
                if (toggle.isOn)
                {
                    joyRideSliders[0].SetFloatValue(0.5f);
                    joyRideSliders[1].SetFloatValue(0.0f);
                    joyRideSliders[2].SetFloatValue(30.0f);
                    joyRideSliders[3].SetFloatValue(20.0f);
                    joyRideSliders[4].SetFloatValue(0.5f);
                }
                else
                {
                    foreach (ParameterSlider ps in joyRideSliders)
                    {
                        ps.ResetValue();
                    }
                }
                uiManager.unlimitedTime = toggle.isOn;
                break;
            case "InvertLook":
                spaceshipMovement.invertLook = toggle.isOn;
                break;
            default:
                break;
        }
    }

    // Set float value given slider name and value then save it in PlayerPrefs
    public void SetFloat(Slider slider)
    {
        SetFloat(slider.name, slider.value);

        if (saveParameters)
        {
            ParameterSlider ps = slider.GetComponent<ParameterSlider>();
            if (ps)
                ps.SaveValue();
        }
    }
    
    // Set float value given name and value
    public void SetFloat(string name, float value)
    {
        if (!saveParameters)
        {
            Undo.RecordObject(cowBase, "Modify Cow Prefab");
            Undo.RecordObject(farmerBase, "Modify Farmer Prefab");
        }
        switch(name)
        {
            // Movement
            case "HorizontalSpeed":
                spaceshipMovement.horizontalSpeed = value;
                break;
            case "VerticalSpeed":
                spaceshipMovement.verticalSpeed = value;
                break;
            case "RotationSpeed":
                spaceshipMovement.rotationForce = value;
                break;
            case "GroundSpeed":
                spaceshipMovement.groundSpeed = value;
                break;
            case "MaxRotation":
                spaceshipMovement.maxRotation = value;
                break;
            case "MaxHeight":
                spaceshipMovement.maxHeight = value;
                break;
            case "MinHeight":
                spaceshipMovement.minHeight = value;
                break;
            // Camera
            case "MainCameraFieldOfView":
                Camera.main.fieldOfView = value;
                break;
            case "RadarCameraHeight":
                radarCamera.height = value;
                break;
            // Grapple
            case "GrappleFireRate":
                cowAbduction.grappleTime = value;
                break;
            case "GrappleReelSpeed":
                cowAbduction.captureSpeed = value;
                break;
            case "GrappleMaxLength":
                cowAbduction.maxCaptureLength = value;
                break;
            case "GrappleJointAmount":
                cowAbduction.numberOfJoints = (int)value;
                break;
            // Fuel and Ability
            case "FuelDepletionRate":
                uiManager.fuelDepletionRate = value;
                break;
            case "FuelWarnAmount":
                uiManager.fuelWarnAmount = value;
                break;
            case "AbilityRegenerationRate":
                uiManager.cooldownRegenerationRate = value;
                break;
            case "AbilityActiveTime":
                uiManager.abilityActiveTime = value;
                break;
            // Sensitivity
            case "ReticleAimSensitivity":
                hudReticle.joystickSensitivity = value;
                break;
            // Spaceship
            case "SpaceshipSize":
                spaceship.transform.localScale = Vector3.one * value;
                break;
            // Cows
            case "CowSpawnRate":
                cowSpawner.spawnRate = value;
                break;
            case "CowMaxAmount":
                cowSpawner.maxCowAmount = (int)value;
                break;
            case "CowRandomFactor":
                cowSpawner.randomFactor = value;
                break;
            case "CowMass":
                if (!saveParameters)
                    Undo.RecordObject(cowBase.GetComponent<Rigidbody>(), "Modify cow mass");
                cowBase.SetMass(value);
                UpdateCowBrains();
                break;
            case "CowSize":
                if (!saveParameters)
                    Undo.RecordObject(cowBase.GetComponent<Transform>(), "Modify cow size");
                cowBase.SetSize(value);
                UpdateCowBrains();
                break;
            case "CowSpeed":
                cowBase.SetMaxSpeed(value);
                UpdateCowBrains();
                break;
            case "CowMilk":
                cowBase.milk = value;
                UpdateCowBrains();
                break;
            case "CowWanderTime":
                cowBase.maxWanderTime = value;
                UpdateCowBrains();
                break;
            // Farmers
            case "FarmerLockOnDistance":
                farmerBase.lockOnDistance = value;
                UpdateFarmerBrains(name);
                break;
            case "FarmerLockOnSpeed":
                farmerBase.lockOnSpeed = value;
                UpdateFarmerBrains(name);
                break;
            case "FarmerNormalSpeed":
                farmerBase.SetMaxSpeed(value);
                UpdateFarmerBrains(name);
                break;
            case "FarmerAimSpeed":
                farmerBase.aimSpeed = value;
                UpdateFarmerBrains(name);
                break;
            case "FarmerProjectileSpeed":
                farmerBase.projectileSpeed = value;
                UpdateFarmerBrains(name);
                break;
            case "FarmerProjectileDamage":
                farmerBase.projectileDamage = value;
                UpdateFarmerBrains(name);
                break;
            case "FarmerProjectileKnockback":
                farmerBase.projectileKnockback = value;
                UpdateFarmerBrains(name);
                break;
            case "FarmerProjectileLife":
                farmerBase.projectileLife = value;
                UpdateFarmerBrains(name);
                break;
            case "FarmerFireRate":
                farmerBase.fireRate = value;
                UpdateFarmerBrains(name);
                break;
            case "FarmerStartingAmmo":
                farmerBase.startingAmmo = (int)value;
                UpdateFarmerBrains(name);
                break;
            // Misc
            case "TimeScaleFactor":
                uiManager.timeScaleFactor = value;
                break;
            case "TimeMax":
                uiManager.SetTimeMax(value);
                break;
            default:
                break;
        }

        // Undo prefab modification if not saving parameters
        // Note: Subsequent cow spawns will use default values
        if (!saveParameters)
            Undo.PerformUndo();
    }

    // Loop through sliders and load parameters
    public void LoadParameters()
    {
        for (int s = 0; s < sliders.Length; s++)
        {
            ParameterSlider ps = sliders[s].GetComponent<ParameterSlider>();
            if (ps)
            {
                float value = 0.0f;
                switch (sliders[s].name)
                {
                    // Movement
                    case "HorizontalSpeed":
                        value = spaceshipMovement.horizontalSpeed;
                        break;
                    case "VerticalSpeed":
                        value = spaceshipMovement.verticalSpeed;
                        break;
                    case "RotationSpeed":
                        value = spaceshipMovement.rotationForce;
                        break;
                    case "GroundSpeed":
                        value = spaceshipMovement.groundSpeed;
                        break;
                    case "MaxRotation":
                        value = spaceshipMovement.maxRotation;
                        break;
                    case "MaxHeight":
                        value = spaceshipMovement.maxHeight;
                        break;
                    case "MinHeight":
                        value = spaceshipMovement.minHeight;
                        break;
                    // Camera
                    case "MainCameraFieldOfView":
                        value = Camera.main.fieldOfView;
                        break;
                    case "RadarCameraHeight":
                        value = radarCamera.height;
                        break;
                    // Grapple
                    case "GrappleFireRate":
                        value = cowAbduction.grappleTime;
                        break;
                    case "GrappleReelSpeed":
                        value = cowAbduction.captureSpeed;
                        break;
                    case "GrappleMaxLength":
                        value = cowAbduction.maxCaptureLength;
                        break;
                    case "GrappleJointAmount":
                        value = cowAbduction.numberOfJoints;
                        break;
                    // Fuel and Ability
                    case "FuelDepletionRate":
                        value = uiManager.fuelDepletionRate;
                        break;
                    case "FuelWarnAmount":
                        value = uiManager.fuelWarnAmount;
                        break;
                    case "AbilityRegenerationRate":
                        value = uiManager.cooldownRegenerationRate;
                        break;
                    case "AbilityActiveTime":
                        value = uiManager.abilityActiveTime;
                        break;
                    // Sensitivity
                    case "ReticleAimSensitivity":
                        value = hudReticle.joystickSensitivity;
                        break;
                    // Spaceship
                    case "SpaceshipSize":
                        value = spaceship.transform.localScale.x; // Assume uniform scale
                        break;
                    // Cows
                    case "CowSpawnRate":
                        value = cowSpawner.spawnRate;
                        break;
                    case "CowMaxAmount":
                        value = cowSpawner.maxCowAmount;
                        break;
                    case "CowRandomFactor":
                        value = cowSpawner.randomFactor;
                        break;
                    case "CowMass":
                        value = cowBase.GetMass();
                        break;
                    case "CowSize":
                        value = cowBase.GetSize();
                        break;
                    case "CowSpeed":
                        value = cowBase.maxSpeed;
                        break;
                    case "CowMilk":
                        value = cowBase.milk;
                        break;
                    case "CowWanderTime":
                        value = cowBase.maxWanderTime;
                        break;
                    // Farmers
                    case "FarmerLockOnDistance":
                        value = farmerBase.lockOnDistance;
                        break;
                    case "FarmerLockOnSpeed":
                        value = farmerBase.lockOnSpeed;
                        break;
                    case "FarmerNormalSpeed":
                        value = farmerBase.maxSpeed;
                        break;
                    case "FarmerAimSpeed":
                        value = farmerBase.aimSpeed;
                        break;
                    case "FarmerProjectileSpeed":
                        value = farmerBase.projectileSpeed;
                        break;
                    case "FarmerProjectileDamage":
                        value = farmerBase.projectileDamage;
                        break;
                    case "FarmerProjectileKnockback":
                        value = farmerBase.projectileKnockback;
                        break;
                    case "FarmerProjectileLife":
                        value = farmerBase.projectileLife;
                        break;
                    case "FarmerFireRate":
                        value = farmerBase.fireRate;
                        break;
                    case "FarmerStartingAmmo":
                        value = farmerBase.startingAmmo;
                        break;
                    // Misc
                    case "TimeScaleFactor":
                        value = uiManager.timeScaleFactor;
                        break;
                    case "TimeMax":
                        value = uiManager.timeMax;
                        break;
                }
                ps.SetDefaultValue(value);
                ps.LoadValue();
                ps.UpdateTextByValue();
            }
        }

        // Hide menu and sliders when done
        parameterMenu.SetActive(false);
        cowSliders.SetActive(false);
        farmerSliders.SetActive(false);
    }

    // Load the active scene
    public IEnumerator ResetGame()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        while(!async.isDone)
            yield return async;

        Awake();

        // Reset gameplay parameters to default values
        uiManager.ResetGame();

        cowAbduction.ResetGame();

        spaceshipMovement.ResetGame();
    }
}
