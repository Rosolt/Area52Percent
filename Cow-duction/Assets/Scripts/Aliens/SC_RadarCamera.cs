/*  SC_RadarCamera.cs

    Causes an object to follow another object at a set height difference.

    Assumptions:
        This component belongs to a camera pointing downward.
        There is an object in the scene named "UFO".
 */

using UnityEngine;

public class SC_RadarCamera : MonoBehaviour
{
    private GameObject followObject;

    public float height = 15.0f;

    // Awake is called after all objects are initialized
    void Awake()
    {
        followObject = GameObject.Find("UFO");
    }

    // FixedUpdate is called in fixed intervals
    void FixedUpdate()
    {
        if (followObject) {
            Vector3 followPosition = followObject.transform.position;
            
            followPosition.y += height;
            transform.position = followPosition;

            transform.localEulerAngles = new Vector3(90f, 0, -followObject.transform.localEulerAngles.y);
        }
    }
}
