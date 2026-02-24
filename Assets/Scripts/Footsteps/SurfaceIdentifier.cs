using UnityEngine;

public class SurfaceIdentifier : MonoBehaviour
{
    public SurfaceType surfaceType;
}

public enum SurfaceType
{
    Grass,
    Wood,
    Metal,
    Water,
    Road,
    Dirt
}
