using UnityEngine;

public class PlayerParticleController : MonoBehaviour
{
    public ParticleSystem playerParticles;
    public float baseEmission = 1f;      //standard emission
    private float maxSpeed = 2f;          //max speed (scale of t)
    private Vector2 lastPosition;
    private Transform playerTransform;
    void Start()
    {
        playerTransform = GetComponent<Transform>();
        lastPosition = playerTransform.position;
    }
    void FixedUpdate()
    {
        var emission = playerParticles.emission;
        //velocity calculation
        float velocity = (transform.position.y - lastPosition.y) / Time.fixedDeltaTime;
        lastPosition = playerTransform.position;
        //Velocity calculation with clamp
        float t = Mathf.Clamp(velocity / maxSpeed,0,50);

        //Emission adjusted with velocity
        if (velocity >= 0)
        {

            emission.rateOverTime = baseEmission * t + 10;
        }
        else
        {
            emission.rateOverTime = 0;
        }
    }
}
