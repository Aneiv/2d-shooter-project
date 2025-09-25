using DG.Tweening;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DreadWingEnemy : Enemy
{
    //private bool arrived = false;

    [Header("Cannons")]
    [SyncVar] private int wingCannonCounter;
    [SyncVar] private int allCannonsCounter;

    // cannon possitions
    public GameObject[] cannonsPossLeftWing;
    public GameObject[] cannonsPossRightWing;
    public GameObject[] cannonsPossRocketLaunchers;
    public GameObject[] cannonsPossSniperCannon;

    // cannon instances
    private List<GameObject> cannonsObjsLeftWing = new List<GameObject>();
    private List<GameObject> cannonsObjsRightWing = new List<GameObject>();
    private List<GameObject> cannonsObjsRocketLaunchers = new List<GameObject>();
    private List<GameObject> cannonsObjsSniperCannon = new List<GameObject>();

    public GameObject[] cannonContainers;

    [Header("Cannons prefabs")]
    [SerializeField] private GameObject wingCannon;
    [SerializeField] private GameObject rocketLauncher;
    [SerializeField] private GameObject sniperCannon;

    [Header("Attack Patterns")]
    public List<AttackPattern> attackPatternsTransforms = new List<AttackPattern>();
    private List<Action> attackPatterns;
    [SyncVar] public float nextAttackMaxTimeDelay; //max delay before next attack (range[3,max])
    [SyncVar] private float delay;
    [SyncVar] private bool customDelay = true;
    [SyncVar] private bool secondPhaseActivated = false;
    [SyncVar] private bool thirdPhaseActivated = false;

    [Header("Explosions")]
    public GameObject[] deathExplosionsObj;

    public ParticleSystem hugeExplosionTextPart;
    public ParticleSystem hugeFragPart;
    public float miniExplosionDelay = 0.3f;

    public ParticleSystem firePart;
    public float fireSmokePartScale;

    [Header("Additional")]
    private Animator animator;
    private DreadWingShoot DreadWingShoot;
    private DreadWingSpawner DreadWingSpawner;
    private DragWithInputSystem inputSystem;

    public Transform clampPosition;

    // sync rotation
    [SyncVar(hook = nameof(OnRotationChanged))] private Quaternion syncRotation;

    [Server]
    protected override void Start()
    {
        base.Start();
        //find input system to change clamp
        inputSystem = FindAnyObjectByType<DragWithInputSystem>();

        SpawnCannons();
        CountHPAndCannons();

        animator = GetComponent<Animator>();

        attackPatterns = new List<Action>
        {
            AttackCenter,
            DoubleWaves
            //more to be made
        };
        DreadWingShoot = GetComponent<DreadWingShoot>();
        DreadWingSpawner = GetComponent<DreadWingSpawner>();

        RpcSetUpBossHPBar();
    }

    void OnRotationChanged(Quaternion oldRotation, Quaternion newRotation)
    {
        transform.rotation = newRotation;
    }

    [ClientRpc]
    void RpcSetUpBossHPBar()
    {
        // boss hp bar UI
        GameObject bossBarObj = GameObject.FindGameObjectWithTag("BossHealthBar");
        if (bossBarObj != null)
        {
            healthBar = bossBarObj.GetComponent<BossHealthBar>();
            BossHealthBar bossHealthBar = healthBar as BossHealthBar;
            if (bossHealthBar != null)
            {
                bossHealthBar.SetMaxHealth(maxHp);
                bossHealthBar.SetBossName("Dread Wing");
            }
        }
    }

    [Server]
    void SpawnCannons()
    {
        SpawnSpecificCannons(cannonsPossLeftWing, wingCannon, "wingLeft");
        SpawnSpecificCannons(cannonsPossRightWing, wingCannon, "wingRight");
        SpawnSpecificCannons(cannonsPossRocketLaunchers, rocketLauncher, "rocketLauncher");
        SpawnSpecificCannons(cannonsPossSniperCannon, sniperCannon, "sniper");
    }

    [Server]
    void SpawnSpecificCannons(GameObject[] positions, GameObject prefab, string type = null)
    {
        int i = 0;
        foreach (GameObject pos in positions)
        {
            if (pos == null) continue;

            GameObject cannon = Instantiate(prefab, pos.transform.position, Quaternion.Euler(0f, 0f, 180f));

            if (type == "rocketLauncher")
            {
                var rocket = cannon.GetComponentInChildren<RocketLauncher>();
                if (rocket != null && i == 0)
                {
                    rocket.rotationAngleOfReadyToShot = 240f;
                }
            }

            Mirror.NetworkServer.Spawn(cannon);
            RpcParentCannon(cannon, type);

            switch (type)
            {
                case "wingLeft":
                    cannonsObjsLeftWing.Add(cannon);
                    break;
                case "wingRight":
                    cannonsObjsRightWing.Add(cannon);
                    break;
                case "rocketLauncher":
                    cannonsObjsRocketLaunchers.Add(cannon);
                    break;
                case "sniper":
                    cannonsObjsSniperCannon.Add(cannon);
                    break;
                default:
                    Debug.LogWarning($"Unknown cannon type: {type}");
                    break;
            }
            i++;
        }
    }

    [ClientRpc]
    void RpcParentCannon(GameObject cannon, string type)
    {
        if (cannon == null) return;

        Transform parent = null;

        if (cannonContainers == null) return;
        if (cannonContainers.Length < 3) return;

        switch (type)
        {
            case "wingLeft":
                parent = cannonContainers[0].transform.parent;
                break;

            case "wingRight":
                parent = cannonContainers[0].transform.parent;
                break;

            case "rocketLauncher":
                parent = cannonContainers[1].transform.parent;
                break;

            case "sniper":
                parent = cannonContainers[2].transform.parent;
                break;
        }

        if (parent != null)
        {
            cannon.transform.SetParent(parent, true);
            //cannon.transform.localScale = Vector3.one;
            //cannon.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning($"Parent for cannon type '{type}' not found! Cannon not parented.");
        }
    }

    [Server]
    private void CountHPAndCannons()
    {
        wingCannonCounter = cannonsObjsLeftWing.Count + cannonsObjsRightWing.Count;
        allCannonsCounter = wingCannonCounter + cannonsObjsRocketLaunchers.Count + cannonsObjsSniperCannon.Count;

        //add HP for wing and other cannon types
        List<GameObject>[] allCannonsArrays = new List<GameObject>[]
        {
            cannonsObjsLeftWing,
            cannonsObjsRightWing,
            cannonsObjsRocketLaunchers,
            cannonsObjsSniperCannon
        };

        foreach (var cannonArray in allCannonsArrays)
        {
            if (cannonArray == null) continue;

            foreach (var cannonObj in cannonArray)
            {
                if (cannonObj == null) continue;

                Enemy cannon = cannonObj.GetComponentInChildren<Enemy>();
                if (cannon != null)
                {
                    maxHp += cannon.maxHp;
                }
                else
                {
                    Debug.LogWarning($"Obj {cannonObj.name} doesnt have class Enemy!");
                }
            }
        }
        currentHp = maxHp;
    }

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
    public void DestroyCannon(int cannonHp)
    {
        allCannonsCounter--;

        if (allCannonsCounter <= cannonsObjsRocketLaunchers.Count + cannonsObjsSniperCannon.Count && secondPhaseActivated == false)
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

        currentHp -= cannonHp;
        healthBar.SetHealth(currentHp);
    }

    [Server]
    private void ActiveSecondPhase()
    {
        foreach (GameObject obj in cannonsObjsRocketLaunchers)
        {
            if (obj == null) continue;

            var cannonShoot = obj.GetComponentInChildren<EnemyCannonShoot>();
            if (cannonShoot != null)
                cannonShoot.ReadyToShoot();

            var enemyCannon = obj.GetComponentInChildren<DreadWingEnemyCannon>();
            if (enemyCannon != null)
                enemyCannon.OnArrival();
        }

        foreach (GameObject obj in cannonsObjsSniperCannon)
        {
            if (obj == null) continue;

            var cannonShoot = obj.GetComponentInChildren<EnemyCannonShoot>();
            if (cannonShoot != null)
                cannonShoot.ReadyToShoot();

            var enemyCannon = obj.GetComponentInChildren<DreadWingEnemyCannon>();
            if (enemyCannon != null)
                enemyCannon.OnArrival();
        }
    }


    [Server]
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


    [Server]
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
        //clamp screen
        inputSystem.maxY = clampPosition.position.y;
        SpawnAttack();
    }

    [Server]
    public override void TakeDamage(int damage, GameObject attacker)
    {
        if (!isVulnerable) return;

        //Debug.Log("Enemy took: " + damage.ToString() + " dmg");
        currentHp = Mathf.Max(currentHp - damage, 0);
        if (currentHp <= 0)
        {
            Die(attacker);
        }

    }

    [Server]
    IEnumerator DieWithDelayCoroutine()
    {
        yield return new WaitForSeconds(4f);
        Mirror.NetworkServer.Destroy(rootEnemy);
    }

    [Server]
    public override void Die(GameObject attacker)
    {
        //reset screen clamp
        inputSystem.ResetScreenClamp();

        if (enemyKilled) return;
        enemyKilled = true;

        Player player = attacker.GetComponent<Player>();

        // death animation
        RpcSetDeathAnimation();

        // mini explosions
        ExplosionsAndScore(player);

        DOTween.Kill(mainSprite);
        foreach (var sprite in addSprites)
        {
            DOTween.Kill(sprite);
        }
        DOTween.Kill(gameObject);
        StartCoroutine(DieWithDelayCoroutine());

    }

    [ClientRpc]
    void RpcSetDeathAnimation()
    {
        if (mainSprite != null)
        {
            Color color = mainSprite.color;
            color.a = 1f;
            mainSprite.color = color;
            mainSprite.material = mainMaterial;
        }
        if (animator != null) {
            animator.SetTrigger("OnDeath");
        }
    }

    [ClientRpc]
    void RpcExplosions()
    {
        StartCoroutine(RpcExplosionsCoroutine());
    }
    private IEnumerator RpcExplosionsCoroutine()
    {
        // quick explosions
        foreach (GameObject obj in deathExplosionsObj)
        {
            Vector2 pos = obj.transform.position;
            ExplosionParticles(pos, null, null, 1f);
            // fire
            var fireInstance = Instantiate(firePart, pos, Quaternion.identity);
            fireInstance.transform.SetParent(transform, true);
            var mainFire = fireInstance.main;
            mainFire.startSizeMultiplier = fireSmokePartScale;
            fireInstance.Play();

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
        ExplosionParticles(null, hugeExplosionTextPart, hugeFragPart, 0.8f, 75f, 90f);
    }

    [TargetRpc]
    void TargetScoreAnim(NetworkConnection target, float delay)
    {
        StartCoroutine(DelayedScoreAnim(delay));
    }
    IEnumerator DelayedScoreAnim(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject srObj = Instantiate(scoreRewardPrefab, transform.position, Quaternion.identity);
        ScoreRewardAnim srAnim = srObj.GetComponent<ScoreRewardAnim>();
        if (srAnim != null)
        {
            srAnim.SetScore(scoreReward);
        }
    }

    [Server]
    void ExplosionsAndScore(Player player)
    {
        RpcExplosions();

        // score reward with delay
        if (player != null)
        {
            float totalDelay =
                (deathExplosionsObj.Length * (miniExplosionDelay / 2f)) +
                (deathExplosionsObj.Length * miniExplosionDelay) +
                0.5f +
                0.1f;
            StartCoroutine(DelayedAddScoreAndNextWave(player, totalDelay));
            TargetScoreAnim(player.connectionToClient, totalDelay);
        }
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

    [Server]
    private void OnThirdPhaseActions()
    {
        isVulnerable = true;

        if (DreadWingShoot != null && DreadWingSpawner != null) {
            DreadWingShoot.StartShootingFromBelowDeck();
            DreadWingSpawner.StartSpawningEnemies();
        }

    }

    protected override void OnCollisionWithPlayer(GameObject playerObj) { }
}
[System.Serializable]
public class AttackPattern
{
    public List<Transform> transforms;
}