public interface IBoid
{
    public float MinSpeed { get; set; }
    public float MaxSpeed { get; set; }
    public float PerceptionRadius { get; set; }
    public float AvoidanceRadius { get; set; }
    public float MaxSteerForce { get; set; }
    public float AlignWeight { get; set; }
    public float CohesionWeight { get; set; }
    public float SeparateWeight { get; set; }
    public float TargetWeight { get; set; }
    public float BoundsRadius { get; set; }
    public float AvoidCollisionWeight { get; set; }
    public float CollisionAvoidDst { get; set; }
}