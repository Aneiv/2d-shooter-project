using DG.Tweening;
using Mirror;
using Mirror.Examples.Common.Controllers.Player;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Player : Mirror.NetworkBehaviour, IHealth
{
    public int maxHp = 80;
    [SyncVar] public bool isAlive;
    [SyncVar] public string playerName;
    [SyncVar(hook = nameof(OnScoreChanged))] public int currentScore = 0;
    [SyncVar] public int currentHp;
    [SyncVar(hook = nameof(OnCoinsChanged))] public int currentNumberOfCoins = 0;
    private Animator playerAnimator;

    private GameOverMenu gameOverMenu;
    public GameObject mainCanva;
    public GameObject GameOverUI;
    private GameObject DeathUI;

    public GameObject gameUI;
    public HealthBar healthBar;
    public TMP_Text totalScoreText;
    public TMP_Text totalCoinsTextUI;
    public TMP_Text totalCoinsTextPause;
    public GameObject coinUI;

    private GameController gameController;
    void Start()
    {
        isAlive = true;
        //object assign
        mainCanva = GameObject.Find("Canvas");
        GameOverUI = mainCanva.transform.Find("GameOverMenu").gameObject;
        gameUI = mainCanva.transform.Find("UI").gameObject;
        DeathUI = mainCanva.transform.Find("DeathMenu").gameObject;

        healthBar = mainCanva.transform.Find("UI/TopRightElements/Player_Health_Bar").GetComponent<HealthBar>();
        totalScoreText = mainCanva.transform.Find("UI/TopRightElements/Score/ScoreText").GetComponent<TMP_Text>();
        totalCoinsTextUI = mainCanva.transform.Find("UI/TopRightElements/Coins/CoinsText").GetComponent<TMP_Text>();
        totalCoinsTextPause = mainCanva.transform.Find("PauseMenu/Coins/CoinsText").GetComponent<TMP_Text>();
        coinUI = mainCanva.transform.Find("UI/TopRightElements/Coins").gameObject;

        currentHp = maxHp;
        healthBar.SetMaxHealth(maxHp);
        playerAnimator = GetComponent<Animator>();
        totalScoreText.text = currentScore.ToString();
        gameOverMenu = mainCanva.GetComponent<GameOverMenu>();

        // add player
        GameObject gc = GameObject.FindGameObjectWithTag("GameController");
        if (gc != null) {
            gameController = gc.GetComponent<GameController>();
            if (gameController != null) {
                gameController.AddPlayer();
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        //get player name from LanLobbyManager
        string chosenName = LanLobbyManager.Instance.GetPlayerName();
        CmdSetPlayerName(chosenName); //send request to server to change username for player instance
        Debug.Log("Playing as: " + chosenName);

    }
    [Command]
    private void CmdSetPlayerName(string newName)
    {
        playerName = newName;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        //Debug.Log("Player took: " + damage.ToString() + " dmg");
        if (currentHp - damage > 0)
        {
            currentHp -= damage;

        }
        else
        {
            Die();
        }
        TakeDamageRpc();
    }

    [Server]
    public void Die()
    {
        if (!isAlive) return;
        isAlive = false;

        if (isServer && connectionToClient == NetworkServer.localConnection) // host death
        {
            OnGameOver();
        }
        else // client death
        {
            TargetOnGameOver(connectionToClient);
        }
        RpcOnPlayerDeath();

        //NetworkServer.Destroy(gameObject);
        gameController.OnPlayerDeath();
    }

    [ClientRpc]
    private void RpcOnPlayerDeath()
    {
        DisablePlayer();
    }

    private void DisablePlayer()
    {
        var sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
            sprite.enabled = false;

        var col = GetComponent<PolygonCollider2D>();
        if(col != null) 
            col.enabled = false;

        var dragInputSystem = GetComponent<DragWithInputSystem>();
        if (dragInputSystem != null)
            dragInputSystem.enabled = false;

        var shootSystem = GetComponent<PlayerShoot>();
        if (shootSystem != null)
            shootSystem.enabled = false;

        foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    [TargetRpc]
    private void TargetOnGameOver(NetworkConnectionToClient conn)
    {
        OnGameOver();
    }

    private void OnGameOver()
    {
        GameOverUI.SetActive(false);
        gameUI.SetActive(false);
        DeathUI.SetActive(true);
    }


    [ClientRpc]
    private void TakeDamageRpc()
    {
        //Damage received animation
        playerAnimator.SetTrigger("DamageReceived");

        //run only on correct player instance
        if (isLocalPlayer)
        {
            healthBar.SetHealth(currentHp);
        }
    }

    private void OnScoreChanged(int oldScore, int newScore)
    {
        if (isLocalPlayer)
        {
            DisplayNumberAnimation(totalScoreText, oldScore, newScore, 0.6f);
            totalScoreText.text = newScore.ToString(); //update score value
        }
    }

    private void OnCoinsChanged(int oldCoins, int newCoins)
    {
        if (isLocalPlayer)
        {
            CoinTextUI coinTextUI = coinUI.GetComponent<CoinTextUI>();
            if (coinTextUI != null)
            {
                coinTextUI.showCoins();
                DisplayNumberAnimation(totalCoinsTextUI, oldCoins, newCoins, 0.2f);
            }
            totalCoinsTextUI.text = newCoins.ToString();//update score value
            totalCoinsTextPause.text = newCoins.ToString();//update score value
        }
    }

    [Server]
    public void AddToScore(int score)
    {
        currentScore += score;
    }

    [Server]
    public void AddToCoins(int coinsNumber)
    {
        currentNumberOfCoins += coinsNumber;
    }

    private void DisplayNumberAnimation(TMP_Text numberText, int currentScore, int targetScore, float animationPace)
    {
        numberText.text = currentScore.ToString(); //make display old value on UI to simulate animation for value increase
        DOVirtual.Int(currentScore, targetScore, animationPace, (x) =>
        {
            currentScore = x;
            numberText.text = currentScore.ToString();
        })
        .SetEase(Ease.Linear);
    }
}
