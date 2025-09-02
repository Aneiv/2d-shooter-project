using DG.Tweening.Core.Easing;
using DG.Tweening;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Reflection;
using Unity.VisualScripting;

public class DreadWingEnemy : Enemy
{
    //public GameObject[] deathExplosionsObj;
    //public float miniExplosionDelay = 0.2f;
    //public ParticleSystem hugeExplosionPart;
    //public ParticleSystem hugeFragPart;

    [Header("Cannons")]
    private int wingCannonCounter;
    private int allCannonsCounter;
    public GameObject[] cannonsObjsLeftWing;
    public GameObject[] cannonsObjsRightWing;
    public GameObject[] cannonsRocketLaunchers;
    public GameObject[] cannonsSniperCannon;
    public GameObject[] cannonContainers;
    public List<AttackPattern> attackPatternsTransforms = new List<AttackPattern>();
    private Animator animator;
    private List<Action> attackPatterns;
    public float nextAttackMaxTimeDelay; //max delay before next attack (range[3,max])
    private float delay;
    public int singleCannonScoreValue = 50;
    private bool customDelay = true;

    private bool secondPhaseActivated = false;
    private bool thirdPhaseActivated = false;
    //private bool arrived = false;
    private DreadWingShoot DreadWingShoot;

    [Header("Explosions")]
    public GameObject[] deathExplosionsObj;
    public float miniExplosionDelay = 0.3f;
    public ParticleSystem hugeExplosionTextPart;
    public ParticleSystem hugeFragPart;

    protected override void Start()
    {
        base.Start();
        isVulnerable = false;
        wingCannonCounter = cannonsObjsLeftWing.Length + cannonsObjsRightWing.Length;
        allCannonsCounter = wingCannonCounter + cannonsRocketLaunchers.Length + cannonsSniperCannon.Length;
        //add HP for wing and other cannon types
        maxHp += allCannonsCounter * singleCannonScoreValue;
        healthBar.SetMaxHealth(maxHp);
        animator = GetComponent<Animator>();

        attackPatterns = new List<Action>
        {
            AttackCenter,
            DoubleWaves
            //more to be made
        };
        DreadWingShoot = GetComponent<DreadWingShoot>();
    }
    public void SpawnAttack()
    {
        StartCoroutine(SpawnAttackWithDelay());
    }
    //random attack style
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

    public void DestroyCannon()
    {
        allCannonsCounter--;
        if (allCannonsCounter <= cannonsRocketLaunchers.Length + cannonsSniperCannon.Length && secondPhaseActivated == false)
        {
            //isVulnerable = true;
            ActiveSecondPhase();
            secondPhaseActivated = true;
        }
        if (allCannonsCounter <= 0 && thirdPhaseActivated == false)
        {
            ActiveThirdPhase();
            thirdPhaseActivated = true;
        }
    }
    private void ActiveSecondPhase()
    {
        foreach (GameObject obj in cannonsRocketLaunchers)
        {
            if (obj.TryGetComponent<EnemyCannonShoot>(out var cannon))
            {
                cannon.ReadyToShoot();
            }
            if (obj.TryGetComponent<DreadWingEnemyCannon>(out var enemyCannon))
            {
                enemyCannon.OnArrival();
            }
        }
        foreach (GameObject obj in cannonsSniperCannon)
        {
            if (obj.TryGetComponent<EnemyCannonShoot>(out var cannon))
            {
                cannon.ReadyToShoot();
            }
            if (obj.TryGetComponent<DreadWingEnemyCannon>(out var enemyCannon))
            {
                enemyCannon.OnArrival();
            }
        }
    }
    private void ActiveThirdPhase()
    {
        Vector2 PosOut0 = rootEnemy.transform.position;
        Vector2 PosOut1 = new Vector2(2.4f, 0.4f);
        Vector2 PosOut2 = new Vector2(6f, -3f);
        Vector2 PosIn0 = new Vector2(-4.8f, -3.2f);
        Vector2 PosIn1 = new Vector2(-0.5f, -1f);
        Vector2 PosIn2 = new Vector2(0f, 2.5f);
        //movement animation start
        //fly-out animation
        DOVirtual.Float(0f, 1f, 3.5f, (t) =>
        {
            //position on Bezier curve
            Vector2 pos = BezierCurve.Quadratic(PosOut0, PosOut1, PosOut2, t);
            rootEnemy.transform.position = pos; //boss position change

            //future position calculation (for place and rotation prediction)
            //Vector2 futurePos = QuadraticBezier(start, control, end, t + 0.01f);
            Vector2 futurePos = BezierCurve.Quadratic(PosOut0, PosOut1, PosOut2, Mathf.Min(t + 0.01f, 1f));

            Vector2 dir = (futurePos - pos).normalized;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; //angle calculation
            float rotationSpeed = 1f;
            Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
            //rotation adjusted to curve with sprite offset
            rootEnemy.transform.rotation = Quaternion.Slerp(rootEnemy.transform.rotation, targetRot, rotationSpeed * Time.deltaTime); // -90f offset
            if (t >= 0.998f)
            {
                rootEnemy.transform.position = PosOut2;
                //transform.rotation = Quaternion.Euler(0, 0, targetAngleDeg);
            }
        })
        .SetEase(Ease.InOutSine)//make smooth begin and end of animation
                .OnComplete(() => //after animation end
                {
                    //fly-in animation
                    rootEnemy.transform.rotation = Quaternion.Euler(0f, 0f, -45f);
                    DOVirtual.Float(0f, 1f, 3.5f, (t) =>
                    {
                        //position on Bezier curve
                        Vector2 PosIn12 = new Vector2(PosIn2.x, PosIn2.y - 0.2f);
                        Vector2 pos = BezierCurve.Cubic(PosIn0, PosIn1, PosIn12 ,PosIn2, t);
                        rootEnemy.transform.position = pos; //boss position change

                        //future position calculation (for place and rotation prediction)
                        //Vector2 futurePos = QuadraticBezier(start, control, end, t + 0.01f);
                        Vector2 futurePos = BezierCurve.Cubic(PosIn0, PosIn1,PosIn12 ,PosIn2, Mathf.Min(t + 0.01f, 1f));

                        Vector2 dir = (futurePos - pos).normalized;
                        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; //angle calculation
                        float rotationSpeed = 10f;
                        Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
                        //rotation adjusted to curve with sprite offset
                        rootEnemy.transform.rotation = Quaternion.Slerp(rootEnemy.transform.rotation, targetRot, rotationSpeed * Time.deltaTime); // -90f offset
                        if (t >= 0.998f)
                        {
                            rootEnemy.transform.position = PosIn2;
                            rootEnemy.transform.rotation = Quaternion.identity;
                        }
                    })
                                    .OnComplete(() => //after fly-in animation end
                                    {
                                        OnThirdPhaseActions();
                                    });
                });
}


    public override void OnArrival()
    {
        isVulnerable = false;
        foreach (GameObject obj in cannonsObjsLeftWing)
        {
            if (obj.TryGetComponent<EnemyCannonShoot>(out var cannon))
            {
                cannon.ReadyToShoot();
            }
            if (obj.TryGetComponent<DreadWingEnemyCannon>(out var enemyCannon))
            {
                enemyCannon.OnArrival();
            }
        }
        foreach (GameObject obj in cannonsObjsRightWing)
        {
            if (obj.TryGetComponent<EnemyCannonShoot>(out var cannon))
            {
                cannon.ReadyToShoot();
            }
            if (obj.TryGetComponent<DreadWingEnemyCannon>(out var enemyCannon))
            {
                enemyCannon.OnArrival();
            }
        }
        foreach (GameObject container in cannonContainers)
        {
            if (container.TryGetComponent<FollowSprite>(out var followSprite))
            {
                followSprite.StartFollow();
            }
        }
        //arrived = true;
        SpawnAttack();
        //AttackCenter();
        /*        if (SpacecraftCarrierSpawner != null)
                {
                    SpacecraftCarrierSpawner.StartSpawningEnemies();
                }
                if (SpacecraftCarrierShoot != null)
                {
                    SpacecraftCarrierShoot.OnArrival();
                }*/
    }

    public override void TakeDamage(int damage, GameObject attacker)
    {
        if (isVulnerable)
        {
            //Debug.Log("Enemy took: " + damage.ToString() + " dmg");
            if (currentHp - damage > 0)
            {
                currentHp -= damage;
                healthBar.SetHealth(currentHp);

                HitFlashAnim(mainSprite);
            }
            else
            {
                Die(attacker);
            }
        }
    }

    public override void Die(GameObject attacker)
    {
        //Debug.Log("KILLED ENEMY");
        if (!enemyKilled)
        {
            Player player = attacker.GetComponent<Player>();

            // color
            Color color = mainSprite.color;
            color.a = 1f;
            mainSprite.color = color;
            mainSprite.material = mainMaterial;

            // mini explosions
            animator.SetTrigger("OnDeath");
            StartCoroutine(ExplosionsAndDeathCoroutine(player));
            enemyKilled = true;

            DOTween.Kill(mainSprite);
            foreach (var sprite in addSprites)
            {
                DOTween.Kill(sprite);
            }
            DOTween.Kill(gameObject);
            Destroy(rootEnemy, 4f);
        }
    }

    IEnumerator ExplosionsAndDeathCoroutine(Player player)
    {
        // quick explosions
        foreach (GameObject obj in deathExplosionsObj)
        {
            Vector2 pos = obj.transform.position;
            ExplosionParticles(pos, null, null, 1f);
            yield return new WaitForSeconds(miniExplosionDelay / 2f);
        }

        // explosions when boss is falling
        float scale = 1f;
        foreach (GameObject obj in deathExplosionsObj)
        {
            Vector2 pos = obj.transform.position;
            ExplosionParticles(pos, null, null, scale);
            yield return new WaitForSeconds(miniExplosionDelay);
            scale -= 0.1f;
        }
        yield return new WaitForSeconds(0.5f);
        ExplosionParticles(null, hugeExplosionTextPart, hugeFragPart, 0.8f, 75f,90f);

        // score reward
        if (player != null)
        {
            player.AddToScore(scoreReward);
        }
        GameObject srObj = Instantiate(scoreRewardPrefab, transform.position, Quaternion.identity);
        ScoreRewardAnim srAnim = srObj.GetComponent<ScoreRewardAnim>();
        if (srAnim != null)
        {
            srAnim.SetScore(scoreReward);
        }

        var destroyTrigger = waveManager.GetComponent<NextWaveTrigger>();
        destroyTrigger.EnemyKilled();
    }

    private void AttackCenter()
    {
        var method = MethodBase.GetCurrentMethod();
        int index = attackPatterns.FindIndex(a => a.Method == method);
        //Debug.Log($"index: {index}");
        int randPlace = UnityEngine.Random.Range(0, cannonsObjsLeftWing.Length);

        for (int i = 0; i < cannonsObjsLeftWing.Length; i++)
        {
            if (cannonsObjsLeftWing[i] != null)
            {
                cannonsObjsLeftWing[i].GetComponent<DreadWingCannon>().RotateCannonToDestination(attackPatternsTransforms[index].transforms[randPlace], 0.3f, 1, 1, true);
            }
            if (cannonsObjsRightWing[i] != null)
            {
                cannonsObjsRightWing[i].GetComponent<DreadWingCannon>().RotateCannonToDestination(attackPatternsTransforms[index].transforms[randPlace], 0.3f, 1, 1, true);

            }
        }
    }
    private void DoubleWaves()
    {
        SetCustomAttackDelay(5f);
        var method = MethodBase.GetCurrentMethod();
        //int index = attackPatterns.FindIndex(a => a.Method == method);
        int index = 1;
        float initialDelayL = 0.1f, initialDelayR = 0.1f;
        for (int i = 0; i < cannonsObjsRightWing.Length; i++)
        {
            if (cannonsObjsRightWing[i] != null)
            {
                cannonsObjsRightWing[i].GetComponent<DreadWingCannon>().RotateCannonToDestination(attackPatternsTransforms[index].transforms[i], initialDelayR, 0.55f, 5, true);
                initialDelayR += 0.3f;
            }
        }
        //Debug.Log($"transforms: {attackPatternsTransforms[index].transforms.Count}");
        for (int j = 6; j < attackPatternsTransforms[index].transforms.Count; j++)
        {
            int a = (j - 6) % cannonsObjsRightWing.Length;
            if (cannonsObjsLeftWing[a] != null)
            {
                //Debug.Log($"cannon left {a}, transform {attackPatternsTransforms[index].transforms[j].name}");
                cannonsObjsLeftWing[a].GetComponent<DreadWingCannon>().RotateCannonToDestination(attackPatternsTransforms[index].transforms[j], initialDelayL, 0.55f, 5, true);
                initialDelayL += 0.3f;
            }
        }
    }

    private void SetCustomAttackDelay(float newDelay)
    {
        customDelay = true;
        delay = newDelay;
    }

    private void OnThirdPhaseActions()
    {
        isVulnerable = true;

        if (DreadWingShoot != null) {
            DreadWingShoot.StartShootingFromBelowDeck();
        }
    }

    protected override void OnCollisionWithPlayer(GameObject playerObj) { }
}
[System.Serializable]
public class AttackPattern
{
    public List<Transform> transforms;
}