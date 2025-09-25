
public class SpacecraftCarrierCannon : EnemyCannon
{
    private SpacecraftCarrierEnemy spacecraftCarrierEnemy;

    protected override void Start()
    {
        base.Start();
        spacecraftCarrierEnemy = FindFirstObjectByType<SpacecraftCarrierEnemy>();
    }

    protected override void NotifyParentAboutDeath()
    {
        if (spacecraftCarrierEnemy != null)
        {
            spacecraftCarrierEnemy.DestroyCannon();
        }
    }
}
