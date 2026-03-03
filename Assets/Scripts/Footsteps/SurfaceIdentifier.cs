using UnityEngine;

public class SurfaceIdentifier : MonoBehaviour
{
    public SurfaceType surfaceType;
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
