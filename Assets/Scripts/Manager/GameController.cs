using Mirror;
using UnityEngine;

public class GameController : Mirror.NetworkBehaviour
{
    [SyncVar] private int playerCounter = 0;

    private GameObject GameOverUI;
    private GameObject gameUI;
    private GameObject mainCanva;
    private GameOverMenu gameOverMenu;
    private GameObject DeathUI;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        playerCounter = 0;

        mainCanva = GameObject.Find("Canvas");
        GameOverUI = mainCanva.transform.Find("GameOverMenu").gameObject;
        gameUI = mainCanva.transform.Find("UI").gameObject;
        gameOverMenu = mainCanva.GetComponent<GameOverMenu>();
        DeathUI = mainCanva.transform.Find("DeathMenu").gameObject;
    }

    public void AddPlayer()
    {
        playerCounter++;
    }

    [Server]
    public void OnPlayerDeath() 
    {
        playerCounter--;

        if(playerCounter <= 0)
        {
            RpcEndGame();// clients

            if (isServer && connectionToClient != null) // host 
            {
                EndGame();
            }
            else // client 
            {
                RpcEndGame();
            }
        }
    }

    [ClientRpc]
    private void RpcEndGame()
    {
        EndGame();
    }
    private void EndGame()
    {
        DeathUI.SetActive(false);
        gameUI.SetActive(false);
        GameOverUI.SetActive(true);
        gameOverMenu.OnMenuShow();

        PauseMenu.GameIsPaused = true;
        Time.timeScale = 0f;
    }
}
