using UnityEngine;

public class SurfaceIdentifier : MonoBehaviour
{
    public GroundSurface surfaceType;
}

public enum SurfaceType
{
    Default,
    Grass,
    Wood,
    Metal,
    Water,
    Road,
    Dirt
}
