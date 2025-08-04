using UnityEngine;

public class NextWaveTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private int enemiesRemaining=0;
    private GameObject bulletsContainer;
    private Transform bulletsContainerTr;
    private void Start()
    {
        bulletsContainer = GameObject.Find("BulletsContainer");
        bulletsContainerTr = bulletsContainer.transform;
    }
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
    private void LoadNextWave()
    {
        var waveSpawner = gameObject.GetComponent<WaveSpawner>();
        ClearRemainingBullets();
        waveSpawner.SpawnWave();
    }
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

    public void AddToEnemyCounter(int number)
    {
        enemiesRemaining += number;
    }
}