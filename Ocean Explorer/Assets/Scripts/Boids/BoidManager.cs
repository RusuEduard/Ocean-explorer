using UnityEngine;

public class BoidManager : MonoBehaviour
{

    public BoidSettings settings;
    public Transform target;
    Boid[] boids;

    void Start()
    {
        boids = FindObjectsOfType<Boid>();
        this.settings.Initialize(Application.dataPath + "/data.csv");

        foreach (Boid b in boids)
        {
            b.Initialize(settings);
        }
    }

    void Update()
    {
        updateBoids();
    }

    private void updateBoids()
    {
        int numBoids = boids.Length;
        for (int i = 0; i < numBoids; i++)
        {
            for (int j = 0; j < numBoids; j++)
            {
                if (j != i)
                {
                    Boid boidB = boids[j];
                    Vector3 offset = boidB.position - boids[i].position;
                    float sqrDst = (offset.x * offset.x + offset.y * offset.y + offset.z * offset.z) / 2;

                    if (sqrDst <= settings.perceptionRadius)
                    {
                        boids[i].numPerceivedFlockmates += 1;
                        boids[i].avgFlockHeading += boidB.forward;
                        boids[i].centreOfFlockmates += boidB.position;

                        if (sqrDst < settings.avoidanceRadius)
                        {
                            boids[i].avgAvoidanceHeading -= offset / sqrDst;
                        }
                    }
                }
            }
            boids[i].UpdateBoid();
        }
    }
}
