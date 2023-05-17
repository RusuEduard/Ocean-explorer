using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public string mainMenuScene;

    public GameObject pauseMenu;
    public bool isPaused;
    // Update is called once per frame

    private void Start() {
        ResumeGame();    
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        this.isPaused = true;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        this.isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Exit()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}
