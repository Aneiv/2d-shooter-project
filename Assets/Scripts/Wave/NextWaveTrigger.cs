using Mirror;
using UnityEngine;

public class NextWaveTrigger : Mirror.NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SyncVar]private int enemiesRemaining=0;
    private GameObject bulletsContainer;
    private Transform bulletsContainerTr;

    private BossHealthBar bossHealthBar; // UI hp bar
    private void Start()
    {
        bulletsContainer = GameObject.Find("BulletsContainer");
        bulletsContainerTr = bulletsContainer.transform;

        GameObject bossBarObj = GameObject.FindGameObjectWithTag("BossHealthBar");
        if (bossBarObj != null)
        {
            bossHealthBar = bossBarObj.GetComponent<BossHealthBar>();
        }
    }
    [Server]
    public void EnemyKilled()
    {
        enemiesRemaining--;
        //Debug.Log("There are: " + enemiesRemaining + " enemies left");
        if (enemiesRemaining <= 0)
        {
            //Debug.Log("nowa fala");
            LoadNextWave();
        }
    }
    [Server]
    private void LoadNextWave()
    {
        if (bossHealthBar != null) {
            bossHealthBar.Hide();
        }

        var waveSpawner = gameObject.GetComponent<WaveSpawner>();
        ClearRemainingBullets();
        waveSpawner.SpawnWave();
    }
    [Server]
    public void SetRemainingEnemies(int enemies)
    {
        enemiesRemaining = enemies;
        //Debug.Log($"Set enemies amount: {enemiesRemaining}");
    }
    private void ClearRemainingBullets()
    {
        foreach (Transform child in bulletsContainerTr)
        {
            Destroy(child.gameObject);
        }
    }
    [Server]
    public void AddToEnemyCounter(int number)
    {
        enemiesRemaining += number;
    }
}