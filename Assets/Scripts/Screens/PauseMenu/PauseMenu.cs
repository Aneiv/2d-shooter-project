using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : Mirror.NetworkBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject pauseMenuMultiplayerUI;
    public GameObject gameUI;

    public void PauseButtonAction()
    {
        if (GameIsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {
        if (isServer)
        {
            ResumeRpc();
        }
    }
    public void Pause()
    {
        if (isServer)
        {
            PauseRpc();
        }
    }

    [ClientRpc]
    private void PauseRpc()
    {
        //Debug.Log("Pause...");
        if (isServer)
        {
            pauseMenuUI.SetActive(true);
            gameUI.SetActive(false);
        }
        else
        {
            pauseMenuMultiplayerUI.SetActive(true);
            gameUI.SetActive(false);
        }
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
    [ClientRpc]
    private void ResumeRpc()
    {
        if (isServer)
        {
            pauseMenuUI.SetActive(false);
            gameUI.SetActive(true);
        }
        else
        {
            pauseMenuMultiplayerUI.SetActive(false);
            gameUI.SetActive(true);
        }
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void LoadMenu()
    {
        GameIsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
