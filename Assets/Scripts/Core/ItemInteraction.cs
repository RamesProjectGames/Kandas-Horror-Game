using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ItemInteraction : MonoBehaviour
{
    [Header("Throwing")]
    public float throwForce = 10f;

    [Header("Pickup UI")]
    public GameObject pickupUI;
    public InputActionAsset inputActions;
    public string ItemInteractionText;

    [Header("Landing / Alert")]
    [Tooltip("Clip that plays when the object lands after being thrown.")]
    public AudioClip landingSound;
    [Tooltip("Enemies within this radius will be alerted when the item lands.")]
    public float landingAlertRadius = 10f;

    public UnityEvent onInteract;

    private Rigidbody rb;
    private bool hasBeenThrown;

    public Transform player;
    public bool IsHeld { get; private set; }
    public TextMeshPro pickupText;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pickupText = pickupUI.GetComponent<TextMeshPro>();
        pickupUI.SetActive(false);
        hasBeenThrown = false;
    }

    void Update()
    {

        if (pickupText != null && inputActions != null)
        {
            var interactAction = inputActions.FindAction("Interact");
            if (interactAction != null)
            {
                string bindingDisplay = interactAction.GetBindingDisplayString(0);
                pickupText.text = $"Press {bindingDisplay} {ItemInteractionText}";
            }
        }
    }

    public void ShowUI()
    {
        if (!IsHeld)
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
            hasBeenThrown = false;
            HandleLanding();
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

    private void AlertNearbyEnemies()
    {
        if (landingAlertRadius <= 0f) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, landingAlertRadius, LayerMask.GetMask("Enemy"));
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
