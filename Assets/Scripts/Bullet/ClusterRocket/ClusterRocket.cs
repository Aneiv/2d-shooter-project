
public class ClusterRocket : SturdyBullet
{
    public override void Die()
    {
        var  clusterRocketMovement = GetComponent<ClusterRocketMovement>();
        clusterRocketMovement.DivisionIntoRockets();
    }
}

