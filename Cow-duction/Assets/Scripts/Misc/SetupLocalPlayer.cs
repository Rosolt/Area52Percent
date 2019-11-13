using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class SetupLocalPlayer : NetworkBehaviour
{
    [SyncVar]
    public string pname = "player";

    [SyncVar]
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        if(isLocalPlayer)
        {
            if ((gameObject.GetComponent("SC_Cow Abduction") as SC_CowAbduction) != null)
            {
                GetComponent<SC_CowAbduction>().enabled = true;
                GetComponent<SC_SpaceshipMovement>().enabled = true;

                Renderer[] rends = GetComponentsInChildren<Renderer>();
                foreach (Renderer r in rends)
                {
                    r.material.color = Color.blue;
                }
            }
            else
            {
                GetComponent<SC_CowBrain>().enabled = true;
                GetComponent<SC_FarmerBrain>().enabled = true;
                GetComponent<FirstPersonController>().enabled = true;
            }
        }
    }

}
