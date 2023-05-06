using UnityEngine;

[CreateAssetMenu]
public class BoidSettings : ScriptableObject
{
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1f;
    public float maxSteerForce = 20f;

    public float alignWeight = 1f;
    public float cohesionWeight = 1f;
    public float seperateWeight = 1f;

    public float targetWeight = 1f;

    [Header("Collisions")]
    public LayerMask obstacleMask = 0;
    public float boundsRadius = .27f;
    public float avoidCollisionWeight = 10f;
    public float collisionAvoidDst = 50f;
}
