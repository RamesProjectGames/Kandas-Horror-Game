using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshManager : MonoBehaviour
{
    public static NavMeshManager Instance { get; private set; }
    [SerializeField] private NavMeshSurface surface;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    public void UpdateNavMesh()
    {
        surface.BuildNavMesh();
    }
}
