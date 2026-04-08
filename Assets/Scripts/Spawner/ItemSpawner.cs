using UnityEngine;

public class ItemSpawner : MultiInstanceManager
{
    public ItemInteraction itemPrefab; // Prefab of the item to spawn
    PlayerGrabInteraction playerGrabInteraction;

    private void Start()
    {
        playerGrabInteraction = FindFirstObjectByType<PlayerGrabInteraction>(FindObjectsInactive.Include);
        if (playerGrabInteraction == null)
        {
            Debug.LogError("PlayerGrabInteraction component not found in the scene.");
        }
    }
    public void SpawnItem()
    {
        if (itemPrefab == null || playerGrabInteraction == null)
        {
            Debug.LogWarning("ItemPrefab or PlayerGrabInteraction is not assigned in ItemSpawner.");
            return;
        }
        ItemInteraction spawnedItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        spawnedItem.Pickup(playerGrabInteraction.holdPoint); // Automatically pick up the item for testing purposes
    }
}
