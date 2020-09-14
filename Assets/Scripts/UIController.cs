using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController ui; //singleton

    [Header("Texts")]
    public Text Title; //title text
    public Text Best; //best score text
    public Text Score; //score text
    public Text Funds; //funds text
    public Text Tip; //tip text

    [Header("GameObject")]
    public GameObject postProcessing; //post processing gameobject
    public GameObject restartPanel; //restart panel
    public GameObject startPanel; //start panel
    public GameObject startButton; //start button

    [Header("Flags")]
    public bool touchEnabled; //is touch enabled

    private void Awake() //on wake
    {
        ui = this; //set singleton
        Application.targetFrameRate = 30; //set target fps
    }

    private void Start() //on start
    {
        touchEnabled = false; //disable touch
        setStartScene(); //call start scene method

        if(PlayerPrefs.HasKey("PostProcessing"))
        {
            postProcessing.SetActive(PlayerPrefs.GetInt("PostProcessing") == 1);
        }
    }

    public void setStartScene() //start scene method
    {
        Title.gameObject.SetActive(true); //enable title text
        Best.gameObject.SetActive(true); //enable best score text
        Funds.gameObject.SetActive(true); //enable funds text
        Score.gameObject.SetActive(false); //disable score text
        Tip.gameObject.SetActive(true); //enable tip text
        restartPanel.SetActive(false); //disable restart panel
        startPanel.SetActive(true); //enable start panel
        startButton.SetActive(true); //enable start button
    }

    public void setIngameScene() //ingame scene method
    {
        Title.gameObject.SetActive(false); //disable title text
        Best.gameObject.SetActive(false); //disable best score text
        Funds.gameObject.SetActive(false); //disable funds text
        Score.gameObject.SetActive(true); //enable score text
        Tip.gameObject.SetActive(false); //disable tip text
        restartPanel.SetActive(false); //disable restart panel
        startPanel.SetActive(false); //disable start panel
        startButton.SetActive(false); //disable start button
    }

    public void setEndgameScene() //endgame scene method
    {
        Title.gameObject.SetActive(false); //disable title text
        Best.gameObject.SetActive(true); //enable best score text
        Funds.gameObject.SetActive(false); //disable funds text
        Score.gameObject.SetActive(true); //enable score text
        Tip.gameObject.SetActive(false); //disable tip text
        restartPanel.SetActive(true); //enable restart panel
        startPanel.SetActive(false); //disable start panel
        startButton.SetActive(false); //disable start button
    }

    public void reloadScene() //reload scene method
    {
        SceneManager.LoadScene(0); //reload scene
    }

    public void togglePostProcessing() //toggle post proecessing method
    {
        if(postProcessing.activeSelf) //if postprocessing is enabled
        {
            postProcessing.SetActive(false); //disable post processing
            PlayerPrefs.SetInt("PostProcessing", 0);
        }
        else //if post processing is disabled
        {
            postProcessing.SetActive(true); //enable post processing
            PlayerPrefs.SetInt("PostProcessing", 1);
        }
    }

    public void enableTouch() //enable touch method
    {
        touchEnabled = true; //enable touch
        setIngameScene(); //set ingame scene

    }
}