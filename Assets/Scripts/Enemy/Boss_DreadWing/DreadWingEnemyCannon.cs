using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class DreadWingEnemyCannon : EnemyCannon
{
    private DreadWingEnemy dreadWingEnemy;

    protected override void Start()
    {
        base.Start();
        dreadWingEnemy = FindFirstObjectByType<DreadWingEnemy>();
    }

    protected override void NotifyParentAboutDeath()
    {
        if (dreadWingEnemy != null)
        {
            dreadWingEnemy.DestroyCannon(maxHp);
        }
    }
}
