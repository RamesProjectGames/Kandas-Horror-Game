using Dialogue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ItemInteraction : MonoBehaviour
{
    [Header("Throwing")]
    [SerializeField] private float throwForce = 10f;

    [Header("Pickup UI")]
    [SerializeField] private GameObject pickupUI;
    [SerializeField] private string itemInteractionText = "Pick Up";

    [Header("Landing / Alert")]
    [SerializeField] private bool globalAlert = false;
    [Tooltip("Clip that plays when the object lands after being thrown.")]
    [SerializeField] private AudioClip landingSound;
    [Tooltip("Enemies within this radius will be alerted when the item lands.")]
    [SerializeField] private float landingAlertRadius = 10f;

    [Header("Dialogue")]
    [SerializeField] private List<ObjectiveDialoguePair> objectiveDialoguePair;

    [Header("Destructible")]
    [SerializeField] private bool isDestructible = false;
    [SerializeField] private GameObject destructiblePrefab;
    [SerializeField] private float breakImpactThreshold = 6f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float pieceFadeSpeed = 1f;
    [SerializeField] private float pieceDestroyDelay = 2f;
    [SerializeField] private float pieceSleepCheckDelay = 0.1f;
    [SerializeField] private float maxPhysicsLifetime = 3f;
    [SerializeField] private bool disableDebrisDebrisCollision = false;
    [SerializeField] private string debrisLayerName = "Debris";
    [SerializeField] private AudioClip breakSound;

    [Header("Interaction")]
    [SerializeField] private bool showTextOnPickup = true;
    [SerializeField] private InputActionReference interactAction;
    public UnityEvent onInteract;

    [Header("UI References")]
    [SerializeField] private TMP_Text pickupText;
    [SerializeField] private TMP_Text buttonInteractionText;

    [Header("Optional References")]
    [SerializeField] private Transform player;

    // Components
    private Collider col;
    private Rigidbody rb;
    private NavMeshObstacle obstacle;
    private Renderer[] originalRenderers;
    private AudioSource cachedAudioSource;

    // State
    private bool hasBeenThrown;
    private bool isBroken;
    private bool uiInitialized;
    private bool bindingTextInitialized;

    public bool IsHeld { get; private set; }

    // Reusable non-alloc buffer
    private static readonly Collider[] alertHitsBuffer = new Collider[32];

    #region Unity Lifecycle

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        obstacle = TryGetComponent(out NavMeshObstacle navObstacle) ? navObstacle : null;
        originalRenderers = GetComponentsInChildren<Renderer>(true);
        cachedAudioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (pickupUI != null)
            pickupUI.SetActive(false);

        hasBeenThrown = false;
        isBroken = false;
        IsHeld = false;
    }

    #endregion

    #region UI

    /// <summary>
    /// Updates interaction UI text only when needed, not every frame.
    /// </summary>
    private void InitializePickupUI()
    {
        if (uiInitialized) return;

        if (pickupText != null)
            pickupText.text = itemInteractionText;

        if (buttonInteractionText != null)
        {
            if (interactAction != null && interactAction.action != null)
            {
                buttonInteractionText.text = interactAction.action.GetBindingDisplayString(0);
            }
            else
            {
                buttonInteractionText.text = string.Empty;
            }
        }

        uiInitialized = true;
        bindingTextInitialized = true;
    }

    public void RefreshBindingText()
    {
        if (buttonInteractionText == null) return;

        if (interactAction != null && interactAction.action != null)
        {
            buttonInteractionText.text = interactAction.action.GetBindingDisplayString(0);
        }
        else
        {
            buttonInteractionText.text = string.Empty;
        }

        bindingTextInitialized = true;
    }

    public void ShowUI()
    {
        if (isBroken) return;
        if (IsHeld) return;
        if (!showTextOnPickup) return;
        if (pickupUI == null) return;

        if (!uiInitialized || !bindingTextInitialized)
            InitializePickupUI();

        pickupUI.SetActive(true);
    }

    public void HideUI()
    {
        if (pickupUI != null)
            pickupUI.SetActive(false);
    }

    #endregion

    #region Interaction

    public void InvokeInteract()
    {
        onInteract?.Invoke();
    }

    public void Pickup(Transform holdPoint)
    {
        if (isBroken) return;
        if (holdPoint == null) return;

        IsHeld = true;
        hasBeenThrown = false;

        if (col != null)
            col.enabled = false;

        if (obstacle != null)
            obstacle.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Optional: keep your original scale logic if needed
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        HideUI();
    }

    public void Drop()
    {
        if (isBroken) return;

        IsHeld = false;

        if (col != null)
            col.enabled = true;

        if (obstacle != null)
            obstacle.enabled = true;

        transform.SetParent(null);
        transform.localScale = Vector3.one;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public void Throw(Vector3 direction)
    {
        if (isBroken) return;
        if (rb == null) return;

        Drop();

        rb.AddForce(direction * throwForce, ForceMode.Impulse);
        hasBeenThrown = true;
    }

    #endregion

    #region Collision / Landing / Break

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

        HandleThrownCollision(collision);
        TryBreak(collision);
    }

    private void HandleThrownCollision(Collision collision)
    {
        if (!hasBeenThrown || IsHeld)
            return;

        // Ignore tiny "settle" bumps
        if (rb != null && rb.linearVelocity.magnitude <= 0.05f)
        {
            hasBeenThrown = false;
            return;
        }

        // Hit enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            hasBeenThrown = false;

            PlayLandingSound();

            EnemyMovement enemy = collision.gameObject.GetComponent<EnemyMovement>();
            if (enemy != null)
            {
                enemy.GetStunned();
            }

            return;
        }

        // Normal landing
        HandleLanding();
        hasBeenThrown = false;
    }

    private void HandleLanding()
    {
        PlayLandingSound();
        AlertNearbyEnemies();
    }

    private void PlayLandingSound()
    {
        if (landingSound == null) return;

        // Prefer local audio source if available to avoid PlayClipAtPoint temp object allocation
        if (cachedAudioSource != null)
        {
            cachedAudioSource.PlayOneShot(landingSound);
        }
        else
        {
            AudioSource.PlayClipAtPoint(landingSound, transform.position);
        }
    }

    private void TryBreak(Collision collision)
    {
        if (!isDestructible) return;
        if (destructiblePrefab == null) return;
        if (isBroken) return;

        float impact = collision.relativeVelocity.magnitude;
        if (impact < breakImpactThreshold) return;

        BreakObject();
    }

    private void BreakObject()
    {
        isBroken = true;
        HideUI();

        PlayBreakingSound();

        GameObject brokenInstance = Instantiate(destructiblePrefab, transform.position, transform.rotation);

        Rigidbody[] pieceRigidbodies = brokenInstance.GetComponentsInChildren<Rigidbody>(true);
        Renderer[] pieceRenderers = brokenInstance.GetComponentsInChildren<Renderer>(true);

        Vector3 inheritedVelocity = rb != null ? rb.linearVelocity : Vector3.zero;

        int debrisLayer = -1;
        if (disableDebrisDebrisCollision && !string.IsNullOrEmpty(debrisLayerName))
        {
            debrisLayer = LayerMask.NameToLayer(debrisLayerName);
        }

        for (int i = 0; i < pieceRigidbodies.Length; i++)
        {
            Rigidbody pieceRb = pieceRigidbodies[i];
            if (pieceRb == null) continue;

            // Inherit velocity
            pieceRb.linearVelocity = inheritedVelocity;

            // Cheaper debris physics defaults
            pieceRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            pieceRb.interpolation = RigidbodyInterpolation.None;
            pieceRb.solverIterations = 4;
            pieceRb.solverVelocityIterations = 1;

            // Optional debris layer
            if (debrisLayer >= 0)
            {
                pieceRb.gameObject.layer = debrisLayer;
            }

            // Explosion
            pieceRb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0f, ForceMode.Impulse);
        }

        AlertNearbyEnemies();

        // Hide / disable original object immediately
        DisableOriginalObjectVisualsAndPhysics();

        // Start optimized debris cleanup
        StartCoroutine(FadeOutBrokenObject(brokenInstance, pieceRigidbodies, pieceRenderers));
    }
    private void PlayBreakingSound()
    {
        if (breakSound == null) return;

        // Prefer local audio source if available to avoid PlayClipAtPoint temp object allocation
        if (cachedAudioSource != null)
        {
            cachedAudioSource.PlayOneShot(breakSound);
        }
        else
        {
            AudioSource.PlayClipAtPoint(breakSound, transform.position);
        }
    }


    private void DisableOriginalObjectVisualsAndPhysics()
    {
        if (col != null)
            col.enabled = false;

        if (obstacle != null)
            obstacle.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (originalRenderers != null)
        {
            for (int i = 0; i < originalRenderers.Length; i++)
            {
                if (originalRenderers[i] != null)
                    originalRenderers[i].enabled = false;
            }
        }
    }

    private IEnumerator FadeOutBrokenObject(GameObject brokenRoot, Rigidbody[] pieceRigidbodies, Renderer[] pieceRenderers)
    {
        WaitForSeconds sleepCheckWait = new WaitForSeconds(pieceSleepCheckDelay);

        float elapsed = 0f;

        // Wait until all pieces are sleeping OR timeout
        while (elapsed < maxPhysicsLifetime)
        {
            bool allSleeping = true;

            for (int i = 0; i < pieceRigidbodies.Length; i++)
            {
                Rigidbody pieceRb = pieceRigidbodies[i];
                if (pieceRb != null && !pieceRb.IsSleeping())
                {
                    allSleeping = false;
                    break;
                }
            }

            if (allSleeping)
                break;

            elapsed += pieceSleepCheckDelay;
            yield return sleepCheckWait;
        }

        // Extra delay before sinking
        if (pieceDestroyDelay > 0f)
            yield return new WaitForSeconds(pieceDestroyDelay);

        // Cache heights ONCE
        float[] rendererHeights = new float[pieceRenderers.Length];
        for (int i = 0; i < pieceRenderers.Length; i++)
        {
            if (pieceRenderers[i] != null)
                rendererHeights[i] = Mathf.Max(0.01f, pieceRenderers[i].bounds.size.y);
            else
                rendererHeights[i] = 0.01f;
        }

        // Disable physics instead of destroying each component one by one
        for (int i = 0; i < pieceRigidbodies.Length; i++)
        {
            Rigidbody pieceRb = pieceRigidbodies[i];
            if (pieceRb == null) continue;

            Collider pieceCol = pieceRb.GetComponent<Collider>();
            if (pieceCol != null)
                pieceCol.enabled = false;

            pieceRb.isKinematic = true;
            pieceRb.useGravity = false;
            pieceRb.linearVelocity = Vector3.zero;
            pieceRb.angularVelocity = Vector3.zero;
        }

        // Sink effect
        float t = 0f;
        while (t < 1f)
        {
            float step = Time.deltaTime * pieceFadeSpeed;

            for (int i = 0; i < pieceRenderers.Length; i++)
            {
                Renderer renderer = pieceRenderers[i];
                if (renderer == null) continue;

                renderer.transform.Translate(
                    Vector3.down * (step / rendererHeights[i]),
                    Space.World
                );
            }

            t += step;
            yield return null;
        }

        Destroy(brokenRoot);
        Destroy(gameObject);
    }

    #endregion

    #region Dialogue
    
    public void TriggerDialogue()
    {
        if (ObjectiveManager.Instance == null) return;
        if (objectiveDialoguePair == null || objectiveDialoguePair.Count == 0) return;

        NpcMovement npcMovement = GetComponent<NpcMovement>();
        if (npcMovement != null)
        {
            npcMovement.StartCoroutine(npcMovement.FacePlayer());
        }

        var bestMatch = objectiveDialoguePair
            .Select(pair => new
            {
                Pair = pair,
                MatchCount = pair.objective.Count(obj => ObjectiveManager.Instance.isCurrentAndNotCompleted(obj)),
                EarliestMatchIndex = pair.objective
                    .Where(obj => ObjectiveManager.Instance.currentObjectives.Contains(obj))
                    .Select(obj => ObjectiveManager.Instance.currentObjectives.IndexOf(obj))
                    .DefaultIfEmpty(int.MaxValue)
                    .Min()
            })
            .Where(x => x.MatchCount > 0)
            .OrderByDescending(x => x.MatchCount)
            .ThenBy(x => x.EarliestMatchIndex)
            .FirstOrDefault();

        if (bestMatch != null)
        {
            DialogueSystem.Instance.OpenDialogue(bestMatch.Pair.dialogueAsset);
        }
        else
        {
            ObjectiveDialoguePair fallback = objectiveDialoguePair.Find(x => x.objective.Length == 0);
            if (fallback != null)
            {
                DialogueSystem.Instance.OpenDialogue(fallback.dialogueAsset);
            }
        }
    }

    #endregion

    #region Enemy Alert

    private void AlertNearbyEnemies()
    {
        if (globalAlert)
        {
            EnemyMovement[] enemies = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None);
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                    enemies[i].OnEnterAudioRadius(gameObject);
            }

            return;
        }

        if (landingAlertRadius <= 0f)
            return;

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            landingAlertRadius,
            alertHitsBuffer
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = alertHitsBuffer[i];
            if (hit == null) continue;

            EnemyMovement enemy = hit.GetComponent<EnemyMovement>();
            if (enemy != null)
            {
                enemy.OnEnterAudioRadius(gameObject);
            }

            // Clear buffer slot for cleanliness (optional)
            alertHitsBuffer[i] = null;
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (landingAlertRadius > 0f)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, landingAlertRadius);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (landingAlertRadius > 0f)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, landingAlertRadius);
        }
    }

    #endregion
}
