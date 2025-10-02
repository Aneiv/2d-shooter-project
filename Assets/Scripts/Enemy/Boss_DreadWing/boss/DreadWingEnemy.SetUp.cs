
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;


// PARTIAL
// --- SET UP ---
// OF DreadWingEnemy

// Defines:
// - On Arrival behavior
// - HP management
// - Spawning Cannons
// - Cannon destroy behavior

public partial class DreadWingEnemy
{
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
    }
    protected override void OnCollisionWithPlayer(GameObject playerObj) { }
}
