using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class BoidTrainer : MonoBehaviour
{

    public int generationSize;
    TrainingBoid[] boids;
    public TrainingBoid prefab;

    public CsvWriter csvWriter;

    void Start()
    {
        this.csvWriter = new CsvWriter(Application.dataPath + "/data.csv");
        this.WriteCsvTitles();

        this.boids = new TrainingBoid[generationSize];
        for (int i = 0; i < generationSize; i++)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * 15;
            TrainingBoid boid = Instantiate(prefab);
            boid.transform.position = pos;
            boid.transform.forward = UnityEngine.Random.insideUnitSphere;

            // boid.MinSpeed = 1;
            // boid.MaxSpeed = 5;
            // boid.PerceptionRadius = 5;
            // boid.AvoidanceRadius = 5;
            // boid.MaxSteerForce = 2;
            // boid.AlignWeight = 1;
            // boid.CohesionWeight = 1;
            // boid.SeparateWeight = 1;
            // boid.TargetWeight = 1;
            // boid.AvoidCollisionWeight = 1;
            // boid.CollisionAvoidDst = 5;

            boid.MinSpeed = UnityEngine.Random.Range(1, 5);
            boid.MaxSpeed = UnityEngine.Random.Range(5, 10);
            boid.PerceptionRadius = UnityEngine.Random.Range(5, 10);
            boid.AvoidanceRadius = UnityEngine.Random.Range(5, 10);
            boid.MaxSteerForce = UnityEngine.Random.Range(2, 8);
            boid.AlignWeight = UnityEngine.Random.Range(0, 5);
            boid.CohesionWeight = UnityEngine.Random.Range(0, 5);
            boid.SeparateWeight = UnityEngine.Random.Range(0, 5);
            boid.TargetWeight = UnityEngine.Random.Range(0, 5);
            boid.AvoidCollisionWeight = UnityEngine.Random.Range(0, 5);
            boid.CollisionAvoidDst = UnityEngine.Random.Range(5, 15);

            boid.Initialize();
            this.boids[i] = boid;
        }
    }

    void Update()
    {
        updateBoids();
    }

    private TrainingBoid[] GetNewGeneration(TrainingBoid[] previousGeneration)
    {
        var newGeneration = new TrainingBoid[generationSize];
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

    private TrainingBoid GetOffspring(TrainingBoid parent1, TrainingBoid parent2)
    {
        var child = Instantiate(prefab);
        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * 15;
        child.transform.position = pos;
        child.transform.forward = UnityEngine.Random.insideUnitSphere;

        PropertyInfo[] properties = typeof(IBoid).GetProperties();
        foreach (var property in properties)
        {
            property.SetValue(child, UnityEngine.Random.Range(1, 100) < 50 ? property.GetValue(parent1) : property.GetValue(parent2));
            if (UnityEngine.Random.Range(1, 100) < 3)
            {
                property.SetValue(child, (float)property.GetValue(child) + UnityEngine.Random.Range(-3f, 3f));
            }
        }
        return child;
    }

    private TrainingBoid SelectParentBasedOnFitness(TrainingBoid[] boids)
    {
        Int64 totalFitness = boids.Select(boid => boid.getFitness()).Sum();
        Debug.Log(totalFitness);

        var probability = UnityEngine.Random.Range(0, totalFitness);

        long sum = 0;
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
            for (int j = 0; j < numBoids; j++)
            {
                if (j != i)
                {
                    TrainingBoid boidB = boids[j];
                    Vector3 offset = boidB.position - boids[i].position;
                    float sqrDst = (offset.x * offset.x + offset.y * offset.y + offset.z * offset.z) / 2;

                    if (sqrDst <= boids[i].PerceptionRadius)
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
                ExportData();
            }
        }
    }

    void WriteCsvTitles()
    {
        PropertyInfo[] properties = typeof(IBoid).GetProperties();
        this.csvWriter.WriteToFile(properties.Select(property => property.Name).ToList(), false);
    }

    void ExportData()
    {
        var exportData = new List<string>();
        PropertyInfo[] properties = typeof(IBoid).GetProperties();
        foreach (var property in properties)
        {
            float sum = 0;
            foreach (var boid in boids)
            {
                sum += (float)property.GetValue(boid);
            }
            exportData.Add((sum / boids.Length).ToString());
        }
        this.csvWriter.WriteToFile(exportData, true);
    }
}
