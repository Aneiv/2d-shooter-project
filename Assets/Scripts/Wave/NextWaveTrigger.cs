using UnityEngine;

public class NextWaveTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private int enemiesRemaining=0;
    public void EnemyKilled()
    {
        enemiesRemaining--;
        //Debug.Log(enemiesRemaining);
        if (enemiesRemaining <= 0)
        {
            //Debug.Log("nowa fala");
            LoadNextWave();
        }
    }
    private void LoadNextWave()
    {
        var WaveSpawner = GetComponent<WaveSpawner>();
        WaveSpawner.SpawnWave();
    }
    public void SetRemainingEnemies(int enemies)
    {
        enemiesRemaining = enemies;
        //Debug.Log($"Set enemies amount: {enemiesRemaining}");
    }


}
