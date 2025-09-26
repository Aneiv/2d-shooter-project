
using Mirror;
using System.Collections;
using System.Reflection;
using UnityEngine;

// PARTIAL
// --- FIRST PHASE ---
// OF DreadWingEnemy

// Defines:
// - Attacks on first phase
// - Activating attacks
public partial class DreadWingEnemy
{
    [Server]
    public void SpawnAttack()
    {
        StartCoroutine(SpawnAttackWithDelay());
    }

    //random attack style
    [Server]
    IEnumerator SpawnAttackWithDelay()
    {
        if (!customDelay)
        {
            delay = UnityEngine.Random.Range(3, nextAttackMaxTimeDelay);
        }
        yield return new WaitForSeconds(delay);//delay before new wave        
        customDelay = false; //customDelay flag reset
        int index = UnityEngine.Random.Range(0, attackPatterns.Count);
        attackPatterns[index]?.Invoke();
        //add if 
        SpawnAttack(); //loop 
    }

    [Server]
    IEnumerator DelayedAddScoreAndNextWave(Player player, float delay)
    {
        yield return new WaitForSeconds(delay);
        player.AddToScore(scoreReward);

        if (waveManager.TryGetComponent<NextWaveTrigger>(out var destroyTrigger))
        {
            destroyTrigger.EnemyKilled();
        }
    }

    [Server]
    private void AttackCenter()
    {
        var method = MethodBase.GetCurrentMethod();
        int index = attackPatterns.FindIndex(a => a.Method == method);
        //Debug.Log($"index: {index}");
        int randPlace = UnityEngine.Random.Range(0, cannonsObjsLeftWing.Count);

        for (int i = 0; i < cannonsObjsLeftWing.Count; i++)
        {
            if (cannonsObjsLeftWing[i] != null)
            {
                var cannon = cannonsObjsLeftWing[i].GetComponentInChildren<DreadWingCannon>();
                if (cannon != null)
                {
                    cannon.RotateCannonToDestination(
                        attackPatternsTransforms[index].transforms[randPlace],
                        0.3f, 1, 1, true
                    );
                }
            }

            if (cannonsObjsRightWing[i] != null)
            {
                var cannon = cannonsObjsRightWing[i].GetComponentInChildren<DreadWingCannon>();
                if (cannon != null)
                {
                    cannon.RotateCannonToDestination(
                        attackPatternsTransforms[index].transforms[randPlace],
                        0.3f, 1, 1, true
                    );
                }
            }

        }
    }

    [Server]
    private void DoubleWaves()
    {
        SetCustomAttackDelay(5f);
        var method = MethodBase.GetCurrentMethod();
        //int index = attackPatterns.FindIndex(a => a.Method == method);
        int index = 1;
        float initialDelayL = 0.1f, initialDelayR = 0.1f;
        for (int i = 0; i < cannonsObjsRightWing.Count; i++)
        {
            if (cannonsObjsRightWing[i] != null)
            {
                var cannon = cannonsObjsRightWing[i].GetComponentInChildren<DreadWingCannon>();
                if (cannon != null)
                {
                    cannon.RotateCannonToDestination(
                        attackPatternsTransforms[index].transforms[i],
                        initialDelayR,
                        0.55f,
                        5,
                        true
                    );
                    initialDelayR += 0.3f;
                }
            }
        }

        // Debug.Log($"transforms: {attackPatternsTransforms[index].transforms.Count}");
        for (int j = 6; j < attackPatternsTransforms[index].transforms.Count; j++)
        {
            int a = (j - 6) % cannonsObjsRightWing.Count;
            if (cannonsObjsLeftWing[a] != null)
            {
                var cannon = cannonsObjsLeftWing[a].GetComponentInChildren<DreadWingCannon>();
                if (cannon != null)
                {
                    // Debug.Log($"cannon left {a}, transform {attackPatternsTransforms[index].transforms[j].name}");
                    cannon.RotateCannonToDestination(
                        attackPatternsTransforms[index].transforms[j],
                        initialDelayL,
                        0.55f,
                        5,
                        true
                    );
                    initialDelayL += 0.3f;
                }
            }
        }

    }

    [Server]
    private void SetCustomAttackDelay(float newDelay)
    {
        customDelay = true;
        delay = newDelay;
    }
}

