using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    BoidSettings settings;
    [Header("Collisions")]

    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    public Vector3 velocity;

    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    Material material;
    Transform cachedTransform;
    private float currentHitDistance;
    Transform target;
    public bool isAlive = true;

    private int fitness = 0;

    public bool isDead = false;

    public void Initialize()
    {
        this.cachedTransform = this.transform;
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        this.settings.Initialize(Application.dataPath + "/data.csv");

        float startSpeed = (this.settings.minSpeed + this.settings.maxSpeed) / 2;
        velocity = forward * startSpeed;
    }

    public void SetColour(Color col)
    {
        if (material != null)
        {
            this.material.color = col;
        }
    }

    public bool UpdateBoid()
    {
        if (!this.isAlive)
        {
            return false;
        }

        Vector3 acceleration = Vector3.zero;

        if (target != null)
        {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards(offsetToTarget) * this.settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;
            Vector3 offsetToFlockmatesCenter = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards(avgFlockHeading) * this.settings.alignWeight;
            var cohesionForce = SteerTowards(offsetToFlockmatesCenter) * this.settings.cohesionWeight;
            var separationForce = SteerTowards(avgAvoidanceHeading) * this.settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += separationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * this.settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, this.settings.minSpeed, this.settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;

        this.fitness++;

        return this.isAlive;
    }

    bool IsHeadingForCollision()
    {
        RaycastHit hit;
        if (Physics.SphereCast(cachedTransform.position, this.settings.boundsRadius, forward, out hit, this.settings.collisionAvoidDst, LayerMask.GetMask("Default")))
        {
            currentHitDistance = hit.distance;
            return true;
        }
        currentHitDistance = this.settings.collisionAvoidDst;
        return false;
    }

    Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(cachedTransform.position, dir);
            if (!Physics.SphereCast(ray, this.settings.boundsRadius, this.settings.collisionAvoidDst, LayerMask.GetMask("Default")))
            {
                return dir;
            }
        }

        return forward;
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * this.settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, this.settings.maxSteerForce);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(cachedTransform.position, dir);
            // Gizmos.DrawRay(ray);
            // Debug.DrawLine(ray.origin, ray.origin + ray.direction * settings.collisionAvoidDst, Color.red);
        }

        Gizmos.color = Color.red;
        Debug.DrawLine(cachedTransform.position, cachedTransform.position + forward * currentHitDistance);
        Gizmos.DrawWireSphere(cachedTransform.position + forward * currentHitDistance, this.settings.boundsRadius);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.GetType() != this.GetType())
        {
            this.forward = new Vector3(0, 0, 0);
            this.velocity = new Vector3(0, 0, 0);
            this.isAlive = false;
        }
    }
}
