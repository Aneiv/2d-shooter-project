using UnityEngine;

public class SniperEnemyShoot : MonoBehaviour, IEnemy
{
    private bool waiting = true;
    public float maxRandomShootingDelay = 2f;
    private float finalShootingDelay;

    private float timer = 0f;

    public GameObject cannon;

    private void Start()
    {
        finalShootingDelay = Random.value * maxRandomShootingDelay;
        timer = finalShootingDelay;
    }

    void FixedUpdate()
    {
        if (!waiting)
        {
            if (timer <= 0f)
            {
                var sniperCannon = cannon.GetComponent<SniperCannon>();
                var followSprite = cannon.GetComponent<FollowSprite>();
                if (sniperCannon != null && followSprite != null)
                {
                    followSprite.StartFollow();
                    sniperCannon.ReadyToShoot();
                }
            }

            timer -= Time.deltaTime;
        }
    }

    public void OnArrival()
    {
        waiting = false;
    }
}
