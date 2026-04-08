using System.Collections.Generic;
using UnityEngine;

public class ObjectAreaSpawner : MultiInstanceManager
{
    [Header("Pool Settings")]
    public GameObject prefab;
    public int poolSize = 20;

    [Header("Spawn Area")]
    public Transform centerPoint;               // The GameObject used as the center
    public bool useCenterPointScale = true;     // Use centerPoint.localScale as area size
    public Vector3 manualAreaSize = new Vector3(10f, 0f, 10f); // fallback if disabled
    public bool useLocalOffset = false;         // If true, area follows centerPoint rotation

    [Header("Spawn Settings")]
    public bool spawnOnStart = true;
    public int spawnCountOnStart = 10;
    public bool randomYRotation = true;

    private List<GameObject> pool = new List<GameObject>();

    private void Start()
    {
        CreatePool();

        if (spawnOnStart)
        {
            for (int i = 0; i < spawnCountOnStart; i++)
            {
                SpawnFromPool();
            }
        }
    }

    private void CreatePool()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not assigned!");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject SpawnFromPool()
    {
        GameObject pooledObj = GetPooledObject();

        if (pooledObj == null)
        {
            Debug.LogWarning("No available pooled object!");
            return null;
        }

        Vector3 spawnPos = GetRandomPositionInArea();
        Quaternion spawnRot = randomYRotation
            ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            : Quaternion.identity;

        pooledObj.transform.position = spawnPos;
        pooledObj.transform.rotation = spawnRot;
        pooledObj.SetActive(true);

        return pooledObj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    private GameObject GetPooledObject()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
                return pool[i];
        }

        return null;
    }

    private Vector3 GetRandomPositionInArea()
    {
        if (centerPoint == null)
        {
            Debug.LogWarning("Center Point not assigned, using spawner position.");
            centerPoint = transform;
        }

        Vector3 areaSize = GetAreaSize();

        Vector3 randomOffset = new Vector3(
            Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
            Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f),
            Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f)
        );

        if (useLocalOffset)
        {
            return centerPoint.position + centerPoint.TransformDirection(randomOffset);
        }

        return centerPoint.position + randomOffset;
    }

    private Vector3 GetAreaSize()
    {
        if (useCenterPointScale && centerPoint != null)
        {
            return centerPoint.lossyScale; // world scale (better than localScale if parent scaled)
        }

        return manualAreaSize;
    }

    private void OnDrawGizmosSelected()
    {
        if (centerPoint == null) return;

        Vector3 areaSize = useCenterPointScale ? centerPoint.lossyScale : manualAreaSize;

        Gizmos.color = Color.green;
        Matrix4x4 oldMatrix = Gizmos.matrix;

        if (useLocalOffset)
        {
            Gizmos.matrix = Matrix4x4.TRS(centerPoint.position, centerPoint.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, areaSize);
        }
        else
        {
            Gizmos.DrawWireCube(centerPoint.position, areaSize);
        }

        Gizmos.matrix = oldMatrix;
    }
}
