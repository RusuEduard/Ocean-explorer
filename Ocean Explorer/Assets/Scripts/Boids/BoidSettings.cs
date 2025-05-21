using System.Globalization;
using System.IO;
using System;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class BoidSettings : ScriptableObject
{
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    public float perceptionRadius = 5f;
    public float avoidanceRadius = 1f;
    public float maxSteerForce = 5f;

    public float alignWeight = 1f;
    public float cohesionWeight = 1f;
    public float seperateWeight = 1f;

    public float targetWeight = 1f;

    [Header("Collisions")]
    public LayerMask obstacleMask = 0;
    public float boundsRadius = .5f;
    public float avoidCollisionWeight = 10f;
    public float collisionAvoidDst = 50f;

    public void Initialize(string dataPath)
    {
        if (File.Exists(dataPath))
        {
            var fileData = System.IO.File.ReadAllLines(dataPath);
            if (fileData.Length > 10)
            {
                this.SetValues(fileData.Last());
            }
        }
    }

    private void SetValues(String data)
    {
        var parsedData = data.Trim().Split(',');

        this.minSpeed = float.Parse(parsedData[(int)DataIndexingEnum.MIN_SPEED], CultureInfo.InvariantCulture.NumberFormat);
        this.maxSpeed = float.Parse(parsedData[(int)DataIndexingEnum.MAX_SPEED], CultureInfo.InvariantCulture.NumberFormat);
        this.avoidanceRadius = float.Parse(parsedData[(int)DataIndexingEnum.AVOIDANCE_RADIUS], CultureInfo.InvariantCulture.NumberFormat);
        this.maxSteerForce = float.Parse(parsedData[(int)DataIndexingEnum.MAX_STEER_FORCE], CultureInfo.InvariantCulture.NumberFormat);
        this.alignWeight = float.Parse(parsedData[(int)DataIndexingEnum.ALIGN_WEIGHT], CultureInfo.InvariantCulture.NumberFormat);
        this.cohesionWeight = float.Parse(parsedData[(int)DataIndexingEnum.COHESION_WEIGHT], CultureInfo.InvariantCulture.NumberFormat);
        this.seperateWeight = float.Parse(parsedData[(int)DataIndexingEnum.SEPARATE_WEIGHT], CultureInfo.InvariantCulture.NumberFormat);
        this.avoidCollisionWeight = float.Parse(parsedData[(int)DataIndexingEnum.AVOID_COLLISION_WEIGHT], CultureInfo.InvariantCulture.NumberFormat);
        this.collisionAvoidDst = float.Parse(parsedData[(int)DataIndexingEnum.COLLISION_AVOID_DIST], CultureInfo.InvariantCulture.NumberFormat);
    }
}
