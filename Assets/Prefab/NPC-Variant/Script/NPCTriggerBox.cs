using System.Collections.Generic;
using UnityEngine;

public class NPCTriggerBox : MonoBehaviour
{
    [Header("Target NPCs")]
    [Tooltip("Tarik semua NPC yang ingin dijalankan secara bersamaan ke dalam list ini")]
    public List<NPCWaypointPatrol> targetNPCs = new List<NPCWaypointPatrol>();

    [Header("Settings")]
    public bool triggerOnlyOnce = true;
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered && triggerOnlyOnce) return;

        if (other.CompareTag("Player"))
        {
            // Jalankan semua NPC yang ada di dalam List
            foreach (NPCWaypointPatrol npc in targetNPCs)
            {
                if (npc != null)
                {
                    npc.StartPatrol();
                }
            }

            isTriggered = true;
        }
    }
}
