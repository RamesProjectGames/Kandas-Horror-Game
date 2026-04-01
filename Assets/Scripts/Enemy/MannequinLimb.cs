using UnityEngine;

/// <summary>
/// Detects strikes on specific limbs of the mannequin
/// Communicates strike info to the parent mannequin
/// </summary>
public class MannequinLimb : MonoBehaviour
{
    [SerializeField] private string limbName = "Limb";
    private MannequinFullGame mannequin;

    void Start()
    {
        // Find parent mannequin
        mannequin = GetComponentInParent<MannequinFullGame>();
        if (mannequin == null)
        {
            Debug.LogError($"MannequinLimb on {gameObject.name}: No MannequinEnemyType2 found in parent!");
        }
    }

    /// <summary>
    /// Call this when the player strikes this limb
    /// Can be called from raycast detection or collision
    /// </summary>
    public void OnStrike()
    {
        if (mannequin != null)
        {
            mannequin.OnStruck(limbName);
        }
    }

    /// <summary>
    /// Alternative: Use collision detection
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // Check if collision is from player weapon/strike
        if (collision.gameObject.CompareTag("PlayerWeapon") || collision.gameObject.name.Contains("weapon"))
        {
            OnStrike();
        }
    }

    /// <summary>
    /// Get the limb name
    /// </summary>
    public string GetLimbName()
    {
        return limbName;
    }
}
