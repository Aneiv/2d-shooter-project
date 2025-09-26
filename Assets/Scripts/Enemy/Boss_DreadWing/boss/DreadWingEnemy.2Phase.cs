using DG.Tweening;
using Mirror;
using System.Collections;
using UnityEngine;

// PARTIAL
// --- SECOND PHASE ---
// OF DreadWingEnemy

// Defines:
// - Cannon activation on second phase
// - Activating third phase
public partial class DreadWingEnemy
{
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
        // client
        RpcThirdPhaseAnim();
        // server
        StartCoroutine(DelayedOnThirdPhaseActionsCoroutine(7f));
    }

    [ClientRpc]
    private void RpcThirdPhaseAnim()
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
                        Vector2 pos = BezierCurve.Cubic(PosIn0, PosIn1, PosIn12, PosIn2, t);
                        rootEnemy.transform.position = pos; //boss position change

                        //future position calculation (for place and rotation prediction)
                        //Vector2 futurePos = QuadraticBezier(start, control, end, t + 0.01f);
                        Vector2 futurePos = BezierCurve.Cubic(PosIn0, PosIn1, PosIn12, PosIn2, Mathf.Min(t + 0.01f, 1f));

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
                    });
                    //.OnComplete(() => //after fly-in animation end
                    //{
                    //    OnThirdPhaseActions();
                    //})
                });
    }

    [Server]
    IEnumerator DelayedOnThirdPhaseActionsCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnThirdPhaseActions();
    }

    [Server]
    private void OnThirdPhaseActions()
    {
        isVulnerable = true;

        if (DreadWingShoot != null && DreadWingSpawner != null)
        {
            DreadWingShoot.StartShootingFromBelowDeck();
            DreadWingSpawner.StartSpawningEnemies();
        }

    }

}

