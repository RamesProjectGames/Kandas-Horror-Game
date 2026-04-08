using System.Collections.Generic;
using UnityEngine;

public class MultiInstanceManager : MonoBehaviour
{
    public string instanceID;

    private static Dictionary<string, MultiInstanceManager> instances = new Dictionary<string, MultiInstanceManager>();

    public static MultiInstanceManager GetInstance(string id)
    {
        if (instances.TryGetValue(id, out var instance))
            return instance;

        return null;
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(instanceID))
        {
            Debug.LogError($"{name} has no instanceID assigned!");
            return;
        }

        if (instances.ContainsKey(instanceID) && instances[instanceID] != this)
        {
            Debug.LogWarning($"Duplicate instanceID '{instanceID}' found on {name}, destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        instances[instanceID] = this;
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(instanceID) && instances.ContainsKey(instanceID) && instances[instanceID] == this)
        {
            instances.Remove(instanceID);
        }
    }
}
