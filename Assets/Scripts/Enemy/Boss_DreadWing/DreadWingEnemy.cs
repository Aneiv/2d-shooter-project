using DG.Tweening;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// --- BASE CLASS ---
// DREAD WING ENEMY
public partial class DreadWingEnemy : Enemy
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

}
[System.Serializable]
public class AttackPattern
{
    public List<Transform> transforms;
}