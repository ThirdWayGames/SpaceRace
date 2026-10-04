namespace Assets.Scripts.Enums
{
    /// <summary>
    /// The HeadingToTarget type, either I am not heading to a target, I am heading to a target beacuse its not in LineOfSight or because its not in ShotLineOfSight.
    /// </summary>
    public enum HeadingToTarget
    {
        None,
        NotInLOS,
        NotInShotLOS,
        Disengage
    }
}