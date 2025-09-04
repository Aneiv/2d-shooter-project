
using UnityEngine;

public class ClusterRocketMovement : LightRocketBulletMovement
{
    public GameObject rocketPrefab;
    public ParticleSystem divisionParticle;
    protected GameObject bulletsContainer;

    protected float divisionTimer;
    public float divisionTime = 2.5f;

    public float[] newRocketsSpawnAngles;

    protected override void Start()
    {
        base.Start();
        bulletsContainer = GameObject.Find("BulletsContainer");

        if (target != null) 
        {
            divisionTimer = divisionTime;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if(divisionTimer <= 0f)
        {
            DivisionIntoRockets();
        }

        divisionTimer -= Time.deltaTime;
    }

    public void DivisionIntoRockets()
    {
        DivisionParticles();

        foreach (float spawnAngle in newRocketsSpawnAngles)
        {
            float diretionZ = transform.eulerAngles.z + spawnAngle;
            SpawnRocket(diretionZ);
        }

        Destroy(gameObject);
    }

    void SpawnRocket(float directionZ)
    {
        Quaternion direction = Quaternion.Euler(0f, 0f, directionZ);

        GameObject Bullet = Instantiate(rocketPrefab, rb.transform.position, direction);
        Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        Bullet.GetComponent<LightRocketBulletCollision>().Init(this.gameObject);

        var rocket = Bullet.GetComponent<LightRocketBulletMovement>();
        rocket.target = target; //give player position to bullet when spawned
    }

    private void DivisionParticles()
    {
        Instantiate(divisionParticle, transform.position, Quaternion.identity).Play();
    }
}
