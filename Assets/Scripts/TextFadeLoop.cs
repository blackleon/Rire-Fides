using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextFadeLoop : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] float fadeMultiplier; //amount of change
    [SerializeField] float minimumAlpha; //minimum alpha value
    [SerializeField] float stayDuration; //time spent on minimum and 1 values

    private void Start() //on start
    {
        StartCoroutine(fadeLoop()); //Start fade loop coroutine
    }

    IEnumerator fadeLoop() //fade loop coroutine
    {
        Color fadeColor = new Color(0f, 0f, 0f, fadeMultiplier); //fade color
        bool goingDown = true; //is reducing alpha
        while(true) //infinity loop almost like fixedupdate
        {
            if(goingDown) //if alpha is being reduced
            {
                if(GetComponent<Text>().color.a > minimumAlpha) //if alpha is more than minimum
                {
                    GetComponent<Text>().color -= fadeColor; //decrease alpha
                }
                else //if alpha is less or equal
                {
                    GetComponent<Text>().color = new Color(
                        GetComponent<Text>().color.r, 
                        GetComponent<Text>().color.g, 
                        GetComponent<Text>().color.b, minimumAlpha); //set alpha to minimum
                    yield return new WaitForSeconds(stayDuration); //wait for a while
                    goingDown = false; //change direction
                }

            }
            else //if alpha is being increased
            {
                if (GetComponent<Text>().color.a < 1f) //if alpha is less than 1
                {
                    GetComponent<Text>().color += fadeColor; //increase alpha
                }
                else //if more or equeal
                {
                    GetComponent<Text>().color = new Color(
                        GetComponent<Text>().color.r, 
                        GetComponent<Text>().color.g, 
                        GetComponent<Text>().color.b, 1f); //set alpha to 1
                    yield return new WaitForSeconds(stayDuration); //wait for a while
                    goingDown = true; //change direction
                }
            }

            yield return new WaitForFixedUpdate(); //wait for fixed update
        }
    }
}