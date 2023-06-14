using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingBoid : MonoBehaviour, IBoid
{
    // BoidSettings settings;

    //To be deleted after training

    /// <summary>
    public float MinSpeed { get; set; }
    public float MaxSpeed { get; set; }
    public float AvoidanceRadius { get; set; }
    public float MaxSteerForce { get; set; }
    public float AlignWeight { get; set; }
    public float CohesionWeight { get; set; }
    public float SeparateWeight { get; set; }
    public float TargetWeight { get; set; }
    public float AvoidCollisionWeight { get; set; }
    public float CollisionAvoidDst { get; set; }

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float collisionAvoidDst = 10f;
    /// </summary>

    private float boundsRadius = 0.5f;
    public float perceptionRadius = 5f;

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

    private float fitness = 0f;

    protected float timer = 0;
    private int delayAmount = 10;

    private int collisionPenalty = 2;

    public void Initialize()
    {
        this.cachedTransform = this.transform;
        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (this.MinSpeed + this.MaxSpeed) / 2;
        velocity = forward * startSpeed;
    }

    public void SetColour(Color col)
    {
        if (material != null)
        {
            this.material.color = col;
        }
    }

    public float getFitness()
    {
        return this.fitness;
    }

    public void AddFitness(float amount)
    {
        this.fitness += amount;
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
            acceleration = SteerTowards(offsetToTarget) * this.TargetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;
            avgFlockHeading /= numPerceivedFlockmates;
            Vector3 offsetToFlockmatesCenter = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards(avgFlockHeading) * this.AlignWeight;
            var cohesionForce = SteerTowards(offsetToFlockmatesCenter) * this.CohesionWeight;
            var separationForce = SteerTowards(avgAvoidanceHeading) * this.SeparateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += separationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * this.AvoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, this.MinSpeed, this.MaxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;

        this.timer += Time.deltaTime;
        if (this.timer >= delayAmount)
        {
            timer = 0f;
            this.fitness++;
        }

        return this.isAlive;
    }

    bool IsHeadingForCollision()
    {
        RaycastHit hit;
        if (Physics.SphereCast(cachedTransform.position, this.boundsRadius, forward, out hit, this.collisionAvoidDst, LayerMask.GetMask("Default")))
        {
            currentHitDistance = hit.distance;
            return true;
        }
        currentHitDistance = this.collisionAvoidDst;
        return false;
    }

    Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(cachedTransform.position, dir);
            if (!Physics.SphereCast(ray, this.boundsRadius, this.collisionAvoidDst, LayerMask.GetMask("Default")))
            {
                return dir;
            }
        }

        return forward;
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * this.MaxSpeed - velocity;
        return Vector3.ClampMagnitude(v, this.MaxSteerForce);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(cachedTransform.position, dir);
        }

        Gizmos.color = Color.red;
        Debug.DrawLine(cachedTransform.position, cachedTransform.position + forward * currentHitDistance);
        Gizmos.DrawWireSphere(cachedTransform.position + forward * currentHitDistance, this.boundsRadius);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.GetType() != this.GetType())
        {
            this.forward = new Vector3(0, 0, 0);
            this.velocity = new Vector3(0, 0, 0);
            this.isAlive = false;
            this.fitness = 0f;
        }
        else
        {
            this.collisionPenalty *= collisionPenalty;
            this.fitness /= collisionPenalty;
        }
    }
}
