using System.Collections;
using UnityEngine;

public class CameraFollowManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] GameObject targetObject; //object to follow
    [SerializeField] float lerpMultiplier; //speed of follow adjustment

    [Header("Flags")]
    public bool follow; //is follow enabled

    private void Start() //on start
    {
        StartCoroutine(followObject()); //start follow coroutine
    }

    IEnumerator followObject() //follow coroutine
    {
        while (true) //infinity loop almost like fixedupdate
        {
            if(follow) //if follow enabled
            {
                transform.position = 
                    Vector3.Lerp(transform.position, targetObject.transform.position, lerpMultiplier); //follow object
            }

            yield return new WaitForFixedUpdate(); //wait for fixedupdate
        }
    }
}
