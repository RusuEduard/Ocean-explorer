using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public string openLevel;
    public string trainingLevel;
    public GameObject optionsMenu;
    public GameObject resultsMenu;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        Loader.Load(Loader.Scene.Explore);
    }

    public void StartTrainig()
    {
        Loader.Load(Loader.Scene.Train);
    }

    public void OpenOptions()
    {
        optionsMenu.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
    }

    public void OpenTrainingResults()
    {
        resultsMenu.SetActive(true);
    }

    public void CloseTrainingMenu()
    {
        resultsMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting");
    }
}
