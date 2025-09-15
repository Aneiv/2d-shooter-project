using DG.Tweening;
using Mirror;
using TMPro;
using UnityEngine;

public class Player : Mirror.NetworkBehaviour, IHealth
{
    public int maxHp = 80;
    [SyncVar(hook = nameof(OnScoreChanged))] public int currentScore = 0;
    [SyncVar] private int currentHp;
    [SyncVar(hook = nameof(OnCoinsChanged))] private int currentNumberOfCoins = 0;
    private Animator playerAnimator;
    private GameOverMenu gameOverMenu;
    public GameObject mainCanva;
    public GameObject GameOverUI;
    public GameObject gameUI;
    public HealthBar healthBar;
    public TMP_Text totalScoreText;
    public TMP_Text totalCoinsTextUI;
    public TMP_Text totalCoinsTextPause;
    public GameObject coinUI;

    void Start()
    {
        //object assign
        mainCanva = GameObject.Find("Canvas");
        GameOverUI = mainCanva.transform.Find("GameOverMenu").gameObject;
        gameUI = mainCanva.transform.Find("UI").gameObject;
        healthBar = mainCanva.transform.Find("UI/Player_Health_Bar").GetComponent<HealthBar>();
        totalScoreText = mainCanva.transform.Find("UI/Score/ScoreText").GetComponent<TMP_Text>();
        totalCoinsTextUI = mainCanva.transform.Find("UI/Coins/CoinsText").GetComponent<TMP_Text>();
        totalCoinsTextPause = mainCanva.transform.Find("PauseMenu/Coins/CoinsText").GetComponent<TMP_Text>();
        coinUI = mainCanva.transform.Find("UI/Coins").gameObject;

        currentHp = maxHp;
        healthBar.SetMaxHealth(maxHp);
        playerAnimator = GetComponent<Animator>();
        totalScoreText.text = currentScore.ToString();
        gameOverMenu = mainCanva.GetComponent<GameOverMenu>();
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

    public void Die()
    {
        OnGameOver();
        DieRpc();
    }
    [ClientRpc]
    private void DieRpc()
    {
        Destroy(gameObject);
    }


    [ClientRpc]
    private void TakeDamageRpc()
    {
        if (isLocalPlayer) healthBar.SetHealth(currentHp);
        //Damage received animation
        playerAnimator.SetTrigger("DamageReceived");
    }

    [Server]
    public void AddScore(int amount)
    {
        currentScore += amount;
    }
    private void OnScoreChanged(int oldScore, int newScore)
    {
        if (isLocalPlayer)
            currentScore = newScore;
    }

    [Server]
    public void AddCoins(int amount)
    {
        currentNumberOfCoins += amount;
    }
    private void OnCoinsChanged(int oldCoins, int newCoins)
    {
        if (isLocalPlayer)
            currentNumberOfCoins = newCoins;
    }

    public void OnGameOver()
    {
        gameUI.SetActive(false);
        GameOverUI.SetActive(true);
        Time.timeScale = 0f;
        PauseMenu.GameIsPaused = true;
        gameOverMenu.OnMenuShow();
    }
    public void AddToScore(int score)
    {
        DisplayNumberAnimation(totalScoreText, currentScore, currentScore + score, 0.6f);
        currentScore += score;
        totalScoreText.text = currentScore.ToString(); //update score value
    }
    public void AddToCoins(int coinsNumber)
    {
        CoinTextUI coinTextUI = coinUI.GetComponent<CoinTextUI>();
        if (coinTextUI != null)
        {
            coinTextUI.showCoins();
            DisplayNumberAnimation(totalCoinsTextUI, currentNumberOfCoins, currentNumberOfCoins + coinsNumber, 0.2f);
        }
        currentNumberOfCoins += coinsNumber;
        totalCoinsTextUI.text = currentNumberOfCoins.ToString();//update score value
        totalCoinsTextPause.text = currentNumberOfCoins.ToString();//update score value
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
