using TMPro;
using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    public float throwForce = 10f;
    public GameObject pickupUI;

    private Rigidbody rb;
    public Transform player;
    public bool IsHeld { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pickupUI.SetActive(false);
    }

    void Update()
    {
        pickupUI.transform.LookAt(player.position);
        pickupUI.transform.Rotate(0, 180, 0);
    }

    public void ShowUI()
    {
        if (!IsHeld)
            pickupUI.SetActive(true);
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
    }
}
