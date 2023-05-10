using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class BoidManager : MonoBehaviour
{

    public BoidSettings settings;
    public Transform target;
    public int generationSize;
    public int numberOfGenerations;
    Boid[] boids;
    private int currentGeneration;
    private int numberOfBoidsAlive;
    public Boid prefab;

    void Start()
    {
        this.currentGeneration = 1;
        this.numberOfBoidsAlive = this.generationSize;
        this.boids = new Boid[generationSize];
        for (int i = 0; i < generationSize; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * 15;
            Boid boid = Instantiate(prefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;

            boid.MinSpeed = Random.Range(1, 5);
            boid.MaxSpeed = Random.Range(5, 10);
            boid.PerceptionRadius = Random.Range(5, 10);
            boid.AvoidanceRadius = Random.Range(5, 10);
            boid.MaxSteerForce = Random.Range(2, 8);
            boid.AlignWeight = Random.Range(0, 5);
            boid.CohesionWeight = Random.Range(0, 5);
            boid.SeparateWeight = Random.Range(0, 5);
            boid.TargetWeight = Random.Range(0, 5);
            boid.BoundsRadius = 0.5f;
            boid.AvoidCollisionWeight = Random.Range(0, 5);
            boid.CollisionAvoidDst = Random.Range(5, 15);

            boid.Initialize();
            this.boids[i] = boid;
        }

        InvokeRepeating("LogBestFitness", 30f, 30f);
    }

    void Update()
    {
        if (this.currentGeneration > this.numberOfGenerations)
            return;

        // if (this.numberOfBoidsAlive < this.generationSize / 2)
        // {
        //     var newGeneration = GetNewGeneration(this.boids);
        //     foreach (Boid boid in this.boids)
        //     {
        //         Destroy(boid.gameObject);
        //     }

        //     this.boids = newGeneration;
        //     this.currentGeneration++;
        //     this.numberOfBoidsAlive = this.generationSize;
        // }

        updateBoids();
    }

    private Boid[] GetNewGeneration(Boid[] previousGeneration)
    {
        var newGeneration = new Boid[generationSize];
        Debug.Log(this.currentGeneration + " " + previousGeneration.Max(boid => boid.getFitness()));
        for (int i = 0; i < generationSize; i++)
        {
            var firstParent = this.SelectParentBasedOnFitness(previousGeneration);
            var secondParent = this.SelectParentBasedOnFitness(previousGeneration);
            var child = this.GetOffspring(firstParent, secondParent);
            child.Initialize();
            newGeneration[i] = child;
        }

        return newGeneration;
    }

    private Boid GetOffspring(Boid parent1, Boid parent2)
    {
        var child = Instantiate(prefab);
        Vector3 pos = transform.position + Random.insideUnitSphere * 15;
        child.transform.position = pos;
        child.transform.forward = Random.insideUnitSphere;

        PropertyInfo[] properties = typeof(IBoid).GetProperties();
        foreach (var property in properties)
        {
            property.SetValue(child, Random.Range(1, 100) < 50 ? property.GetValue(parent1) : property.GetValue(parent2));
            if (Random.Range(1, 100) < 3)
            {
                property.SetValue(child, (float)property.GetValue(child) + Random.Range(-3f, 3f));
            }
        }
        return child;
    }

    private Boid SelectParentBasedOnFitness(Boid[] boids)
    {
        var totalFitness = boids.Select(boid => boid.getFitness()).Sum();

        int probability = Random.Range(0, totalFitness);

        var sum = 0;
        for (int i = 0; i < generationSize; i++)
        {
            sum += boids[i].getFitness();
            if (probability <= sum)
            {
                return boids[i];
            }
        }

        return null;
    }

    private void updateBoids()
    {
        int numBoids = boids.Length;
        for (int i = 0; i < numBoids; i++)
        {
            if (!boids[i].isDead)
            {
                for (int j = 0; j < numBoids; j++)
                {
                    if (j != i && !boids[j].isDead)
                    {
                        Boid boidB = boids[j];
                        Vector3 offset = boidB.position - boids[i].position;
                        float sqrDst = (offset.x * offset.x + offset.y * offset.y + offset.z * offset.z) / 2;

                        if (sqrDst < boids[i].PerceptionRadius)
                        {
                            boids[i].numPerceivedFlockmates += 1;
                            boids[i].avgFlockHeading += boidB.forward;
                            boids[i].centreOfFlockmates += boidB.position;

                            if (sqrDst < boids[i].AvoidanceRadius)
                            {
                                boids[i].avgAvoidanceHeading -= offset / sqrDst;
                            }
                        }
                    }
                }
                if (!boids[i].UpdateBoid())
                {
                    var firstParent = this.SelectParentBasedOnFitness(boids);
                    var secondParent = this.SelectParentBasedOnFitness(boids);
                    var child = this.GetOffspring(firstParent, secondParent);
                    child.Initialize();
                    Destroy(boids[i].gameObject);
                    boids[i] = child;
                }
            }
        }
    }

    void LogBestFitness()
    {
        Debug.Log(this.boids.Sum(boid => boid.getFitness()) / this.boids.Length);
    }
}
