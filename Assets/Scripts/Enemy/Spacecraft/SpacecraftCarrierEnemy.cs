using DG.Tweening;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpacecraftCarrierEnemy : Enemy
{
    public GameObject[] deathExplosionsObj;
    public float miniExplosionDelay = 0.2f;
    public ParticleSystem hugeExplosionPart;
    public ParticleSystem hugeFragPart;

    [Header("Cannons")]
    [SyncVar] public int cannonCounter = 6;
    [SerializeField] private GameObject[] cannonPoss;
    [SerializeField] private GameObject cannonPrefab;
    [SerializeField] private GameObject[] rocketLauncherPoss;
    [SerializeField] private GameObject rocketLauncherPrefab;
    private List<GameObject> cannonsObjs = new List<GameObject>();
    public GameObject[] cannonContainers;

    private SpacecraftCarrierSpawner SpacecraftCarrierSpawner;
    private SpacecraftCarrierShoot SpacecraftCarrierShoot;
    private Animator animator;

    protected override void Start()
    {
        base.Start();        
        isVulnerable = false;
        SpacecraftCarrierSpawner = GetComponent<SpacecraftCarrierSpawner>();
        SpacecraftCarrierShoot = GetComponent<SpacecraftCarrierShoot>();
        animator = GetComponent<Animator>();

        if (isServer)
        {
            SpawnCannons();
        }

        foreach (GameObject container in cannonContainers)
        {
            if (container.TryGetComponent<FollowSprite>(out var followSprite))
            {
                followSprite.StartFollow();
            }
        }
    }

    [Server]
    void SpawnCannons()
    {
        cannonsObjs.Clear();

        SpawnSpecificCannons(cannonPoss, cannonPrefab);
        SpawnSpecificCannons(rocketLauncherPoss, rocketLauncherPrefab);

        cannonCounter = cannonsObjs.Count;
    }

    [Server]
    void SpawnSpecificCannons(GameObject[] positions, GameObject cannonPrefab)
    {
        foreach (GameObject pos in positions)
        {
            GameObject cannon = Instantiate(cannonPrefab, pos.transform.position, Quaternion.Euler(0f, 0f, 180f));
            Mirror.NetworkServer.Spawn(cannon);
            cannonsObjs.Add(cannon);
            RpcParentCannon(cannon);
        }
    }

    [ClientRpc]
    void RpcParentCannon(GameObject cannon)
    {
        cannon.transform.SetParent(cannonContainers[0].transform, true);
        cannon.transform.localScale = Vector3.one;
        cannon.transform.localRotation = Quaternion.identity;
    }


    [Server]
    public void DestroyCannon()
    {
        cannonCounter--;
        if (cannonCounter <= 0) {
            isVulnerable = true;
        }
    }

    [Server]
    public override void OnArrival()
    {
        isVulnerable = false;
        foreach (GameObject obj in cannonsObjs)
        {
            var cannon = obj.GetComponentInChildren<EnemyCannonShoot>();
            if (cannon != null)
            {
                cannon.ReadyToShoot();
            }
        }

        if(SpacecraftCarrierSpawner != null)
        {
            SpacecraftCarrierSpawner.StartSpawningEnemies();
        }
        if(SpacecraftCarrierShoot != null)
        {
            SpacecraftCarrierShoot.OnArrival();
        }
    }

    [Server]
    public override void TakeDamage(int damage, GameObject attacker)
    {
        if (!isVulnerable) return;

        //Debug.Log("Enemy took: " + damage.ToString() + " dmg");
        currentHp = Mathf.Max(currentHp - damage, 0);
        if(currentHp <= 0)
        {
            Die(attacker);
        }

    }

    [Server]
    IEnumerator DieWithDelayCoroutine()
    {
        yield return new WaitForSeconds(2f);
        Mirror.NetworkServer.Destroy(rootEnemy);
    }

    [Server]
    public override void Die(GameObject attacker)
    {
        if (enemyKilled) return;

        Player player = attacker.GetComponent<Player>();

        // death animation
        RpcSetDeathAnimation();

        // mini explosions
        ExplosionsAndScore(player);

        var destroyTrigger = waveManager.GetComponent<NextWaveTrigger>();
        destroyTrigger.EnemyKilled();
        enemyKilled = true;

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
        Color color = mainSprite.color;
        color.a = 1f;
        mainSprite.color = color;
        mainSprite.material = mainMaterial;

        animator.SetTrigger("OnDeath");
    }

    [ClientRpc]
    void RpcExplosions()
    {
        StartCoroutine(RpcExplosionsCoroutine());
    }
    private IEnumerator RpcExplosionsCoroutine()
    {
        float scale = 1f;
        foreach (GameObject obj in deathExplosionsObj)
        {
            Vector2 pos = obj.transform.position;
            ExplosionParticles(pos, null, null, scale);
            yield return new WaitForSeconds(miniExplosionDelay);
            scale -= 0.1f;
        }
        yield return new WaitForSeconds(0.1f);
        ExplosionParticles(null, hugeExplosionPart, hugeFragPart, 0.8f, -16f, 90f);
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
            float totalDelay = (deathExplosionsObj.Length * miniExplosionDelay) + 0.1f;
            StartCoroutine(DelayedAddScore(player, totalDelay));
            TargetScoreAnim(player.connectionToClient, totalDelay);
        }
    }

    [Server]
    IEnumerator DelayedAddScore(Player player, float delay)
    {
        yield return new WaitForSeconds(delay);
        player.AddToScore(scoreReward);
    }

    protected override void OnCollisionWithPlayer(GameObject playerObj) {}
}

