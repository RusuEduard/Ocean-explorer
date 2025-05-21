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
        InitPopulation(generationSize);
        InvokeRepeating("ExportData", 0, 5);
    }

    void InitPopulation(int generationSize)
    {
        this.boids = new TrainingBoid[generationSize];
        for (int i = 0; i < generationSize; i++)
        {
            Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * 5;
            TrainingBoid boid = Instantiate(prefab);
            boid.transform.position = pos;
            boid.transform.forward = UnityEngine.Random.insideUnitSphere;

            boid.MinSpeed = UnityEngine.Random.Range(1, 5);
            boid.MaxSpeed = UnityEngine.Random.Range(5, 10);
            boid.AvoidanceRadius = UnityEngine.Random.Range(5, 10);
            boid.MaxSteerForce = UnityEngine.Random.Range(2, 8);
            boid.AlignWeight = UnityEngine.Random.Range(0, 5);
            boid.CohesionWeight = UnityEngine.Random.Range(0, 5);
            boid.SeparateWeight = UnityEngine.Random.Range(0, 5);
            boid.TargetWeight = UnityEngine.Random.Range(0, 5);
            boid.AvoidCollisionWeight = UnityEngine.Random.Range(5, 10);
            boid.CollisionAvoidDst = UnityEngine.Random.Range(2, 6);

            boid.Initialize();
            this.boids[i] = boid;
        }
    }

    void Update()
    {
        updateBoids();
    }

    private TrainingBoid GetOffspring(TrainingBoid parent1, TrainingBoid parent2)
    {
        var child = Instantiate(prefab);
        Vector3 pos = transform.position + UnityEngine.Random.insideUnitSphere * 5;
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
        var totalFitness = boids.Select(boid => boid.getFitness()).Sum();

        var probability = UnityEngine.Random.Range(0, totalFitness);

        var sum = 0f;
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
                    float sqrDst = Mathf.Sqrt(offset.x * offset.x + offset.y * offset.y + offset.z * offset.z);

                    if (sqrDst <= boids[i].perceptionRadius)
                    {
                        boids[i].numPerceivedFlockmates += 1;
                        boids[i].avgFlockHeading += boidB.forward;
                        boids[i].centreOfFlockmates += boidB.position;

                        boids[i].AddFitness(20 / sqrDst);
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
