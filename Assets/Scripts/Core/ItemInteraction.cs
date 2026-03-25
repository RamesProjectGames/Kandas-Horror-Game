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
    public float throwForce = 10f;

    [Header("Pickup UI")]
    public GameObject pickupUI;
    public string ItemInteractionText;

    [Header("Landing / Alert")]
    public bool globalAlert = false;
    [Tooltip("Clip that plays when the object lands after being thrown.")]
    public AudioClip landingSound;
    [Tooltip("Enemies within this radius will be alerted when the item lands.")]
    public float landingAlertRadius = 10f;

    [Header("Dialogue")]
    [SerializeField] private List<ObjectiveDialoguePair> objectiveDialoguePair;

    [Header("Destructible")]
    public bool isDestructible = false;
    public GameObject destructiblePrefab;
    public float explosionForce = 1000f;
    public float explosionRadius = 2f;
    public float piecefadeSpeed = 1f;
    public float pieceDestroyDelay = 2f;
    public float pieceSleepCheckDelay = .1f;

    public bool showTextOnPickup = true;
    public UnityEvent onInteract;

    private Collider col;
    private Rigidbody rb;
    private NavMeshObstacle obstacle;
    private bool hasBeenThrown;
    public InputActionReference interactAction;

    public Transform player;
    public bool IsHeld { get; private set; }
    public TMP_Text pickupText;
    public TMP_Text ButtonInteractionText;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        obstacle = TryGetComponent<NavMeshObstacle>(out obstacle) ? obstacle : null;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pickupUI.SetActive(false);
        hasBeenThrown = false;
    }
    //void Update()
    //{
    //    if (pickupText != null)
    //    {
    //        if (interactAction != null)
    //        {
    //            string bindingDisplay = interactAction.action.GetBindingDisplayString(0);
    //            ButtonInteractionText.text = $"{bindingDisplay}";
    //            pickupText.text = $"{ItemInteractionText}";
    //        }
    //        else
    //        {
    //            pickupText.gameObject.SetActive(false);
    //        }
    //    }
    //}

    void Update()
    {
        if (pickupText != null)
        {
            string bindingDisplay = interactAction.action.GetBindingDisplayString(0);
            ButtonInteractionText.text = $"{bindingDisplay}";
            pickupText.text = $"{ItemInteractionText}";
        }
    }

    public void ShowUI()
    {
        if (!IsHeld && showTextOnPickup)
        {
            pickupUI.SetActive(true);
        }
    }

    public void HideUI()
    {
        pickupUI.SetActive(false);
    }

    public void Pickup(Transform holdPoint)
    {
        IsHeld = true;

        col.enabled = false;
        if(obstacle != null) obstacle.enabled = false;

        rb.isKinematic = true;
        rb.useGravity = false;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localScale = new Vector3(.5f, .5f, .5f);
        transform.localRotation = Quaternion.identity;

        HideUI();
    }

    public void Drop()
    {
        IsHeld = false;
        col.enabled = true;
        if(obstacle != null) obstacle.enabled = true;
        
        transform.SetParent(null);
        
        transform.localScale = Vector3.one;
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    public void Throw(Vector3 direction)
    {
        Drop();
        rb.AddForce(direction * throwForce, ForceMode.Impulse);
        hasBeenThrown = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if the object was just thrown and it hits something, assume it has landed
        if (hasBeenThrown && !IsHeld)
        {
            if (rb.linearVelocity.magnitude == 0)
            {
                hasBeenThrown = false;
                return;
            }
            // if object hits enemy, stun it
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                hasBeenThrown = false;
                AudioSource.PlayClipAtPoint(landingSound, transform.position);
                collision.gameObject.GetComponent<EnemyMovement>().GetStunned();
            }
            else
            {
                HandleLanding();
            }
        }
    }

    public void TriggerDialogue()
    {
        //Check if Objective and Dialogue Pair Exists
        if (ObjectiveManager.Instance == null) return;
        if (objectiveDialoguePair == null || objectiveDialoguePair.Count == 0) return;

        //NPC faces player
        if (GetComponent<NpcMovement>() != null)
            GetComponent<NpcMovement>().StartCoroutine(GetComponent<NpcMovement>().FacePlayer());

        //Find the best matching dialogue pair
        var bestMatch = objectiveDialoguePair.Select(pair => new
        {
            Pair = pair,
            MatchCount = pair.objective.Count(obj => ObjectiveManager.Instance.isCurrentAndNotCompleted(obj)),
            //MatchCount and EarliestMatchIndex might be interchangeable if needed, currently brain fried to consider
            EarliestMatchIndex = pair.objective.Where(obj => ObjectiveManager.Instance.currentObjectives.Contains(obj))
                .Select(obj => ObjectiveManager.Instance.currentObjectives.IndexOf(obj))
                .DefaultIfEmpty(int.MaxValue).Min()
        })
        .Where(x => x.MatchCount > 0).OrderByDescending(x => x.MatchCount)
        .ThenBy(x => x.EarliestMatchIndex)
        .FirstOrDefault();

        if (bestMatch != null)
        {
            DialogueSystem.Instance.OpenDialogue(bestMatch.Pair.dialogueAsset);
        }
        else
        {
            DialogueSystem.Instance.OpenDialogue(objectiveDialoguePair.Find(x => x.objective.Length == 0).dialogueAsset);
        }
    }

    private void HandleLanding()
    {
        // play landing sound if available
        if (landingSound != null)
        {
            // Play one-shot at the impact position so that the sound can be picked up by
            // the enemy sound detection system (which uses AudioSources) or just heard by
            // the player.
            AudioSource.PlayClipAtPoint(landingSound, transform.position);
        }

        // manually alert any nearby enemies so they start investigating the source
        AlertNearbyEnemies();
    }
    public void SpawnBrokenObject()
    {
        if(!isDestructible || destructiblePrefab == null) return;
        GameObject brokenInstance = Instantiate(destructiblePrefab, transform.position, transform.rotation);
        Rigidbody[] rigidbodies = brokenInstance.GetComponentsInChildren<Rigidbody>();
        foreach (var body in rigidbodies)
        {
            if(rb != null)
            {
                body.linearVelocity = rb.linearVelocity;
            }
            body.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }
        StartCoroutine(FadeOutRigidbodies(rigidbodies));
    }
    IEnumerator FadeOutRigidbodies(Rigidbody[] Rigidbodies)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(pieceSleepCheckDelay);
        int activeRigidbodyCount = Rigidbodies.Length;

        while(activeRigidbodyCount > 0)
        {
            yield return waitForSeconds;
            foreach (var body in Rigidbodies)
            {
                if(body.IsSleeping())
                {
                    activeRigidbodyCount--;
                }
            }
        }

        yield return new WaitForSeconds(pieceDestroyDelay);

        float time = 0;
        Renderer[] renderers = Array.ConvertAll(Rigidbodies, GetRendererFromRigidbodies);

        foreach (var body in Rigidbodies)
        {
            Destroy(body.GetComponent<Collider>());
            Destroy(body);
        }
        while(time < 1)
        {
            float step = Time.deltaTime * piecefadeSpeed;
            foreach (var renderer in renderers)
            {
                renderer.transform.Translate(Vector3.down * (step / renderer.bounds.size.y), Space.World);
            }
            time += step;
            yield return null;
        }
        foreach (var renderer in renderers)
        {
            Destroy(renderer.gameObject);
        }
        Destroy(gameObject);
    }
    private Renderer GetRendererFromRigidbodies(Rigidbody rigidbody)
    {
        return rigidbody.GetComponent<Renderer>();
    }

    private void AlertNearbyEnemies()
    {
        if(globalAlert)
        {
            EnemyMovement[] ems = FindObjectsByType<EnemyMovement>(FindObjectsSortMode.None); 
            foreach (EnemyMovement em in ems)
            {
                em.OnEnterAudioRadius(this.gameObject);
            }
        }
        else
        {
            if (landingAlertRadius <= 0f) return;

            Collider[] hits = Physics.OverlapSphere(transform.position, landingAlertRadius);
            foreach (Collider hit in hits)
            {
                EnemyMovement em = hit.GetComponent<EnemyMovement>();
                if (em != null)
                {
                    // reuse the audio radius listener method to trigger pursuit
                    em.OnEnterAudioRadius(this.gameObject);
                }
            }
        }
    }

    #region Gizmos

    private void OnDrawGizmos()
    {
        // Draw alert radius when item lands and alerts enemies
        if (landingAlertRadius > 0f)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, landingAlertRadius);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw filled sphere when selected for better visibility
        if (landingAlertRadius > 0f)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, landingAlertRadius);
        }
    }

    #endregion
}
