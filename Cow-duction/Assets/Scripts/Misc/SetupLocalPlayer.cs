using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class SetupLocalPlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(isLocalPlayer)
        {
            if ((gameObject.GetComponent("SC_Cow Abduction") as SC_CowAbduction) != null)
            {
                GetComponent<SC_CowAbduction>().enabled = true;
                GetComponent<SC_SpaceshipMovement>().enabled = true;
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
