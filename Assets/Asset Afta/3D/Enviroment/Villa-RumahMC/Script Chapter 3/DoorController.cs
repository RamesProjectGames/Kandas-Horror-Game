using UnityEngine;
using System.Collections;
public class DoorController : MonoBehaviour
{
    public enum DoorType { Swing, Slide }

    [Header("Door Settings")]
    public DoorType doorType = DoorType.Swing;
    public float openSpeed = 3f;

    [Header("Swing Settings (Pintu Rotasi)")]
    [Tooltip("Derajat rotasi saat terbuka (misal: 90 atau -90)")]
    public float openAngle = 90f;
    [Tooltip("Sumbu rotasi pintu (biasanya Y)")]
    public Vector3 rotationAxis = Vector3.up;

    [Header("Slide Settings (Pintu Geser)")]
    [Tooltip("Jarak dan arah geser pintu saat terbuka (Relative Local Space)")]
    public Vector3 slideOffset = new Vector3(1.5f, 0f, 0f);

    [Header("State")]
    public bool isOpen = false;

    // Posisi & Rotasi Awal
    private Quaternion defaultRotation;
    private Quaternion openRotation;
    private Vector3 defaultPosition;
    private Vector3 openPosition;

    private Coroutine doorCoroutine;

    void Start()
    {
        // Simpan posisi awal (Local)
        defaultPosition = transform.localPosition;
        defaultRotation = transform.localRotation;

        // Hitung target posisi/rotasi saat terbuka
        openPosition = defaultPosition + slideOffset;
        openRotation = defaultRotation * Quaternion.AngleAxis(openAngle, rotationAxis);
    }

    /// <summary>
    /// Panggil fungsi ini untuk membuka/menutup pintu (bisa via Player Interact atau UI Event)
    /// </summary>
    public void ToggleDoor()
    {
        isOpen = !isOpen;

        if (doorCoroutine != null)
        {
            StopCoroutine(doorCoroutine);
        }

        if (doorType == DoorType.Swing)
        {
            Quaternion targetRot = isOpen ? openRotation : defaultRotation;
            doorCoroutine = StartCoroutine(AnimateRotation(targetRot));
        }
        else if (doorType == DoorType.Slide)
        {
            Vector3 targetPos = isOpen ? openPosition : defaultPosition;
            doorCoroutine = StartCoroutine(AnimatePosition(targetPos));
        }
    }

    private IEnumerator AnimateRotation(Quaternion targetRotation)
    {
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.localRotation = targetRotation;
    }

    private IEnumerator AnimatePosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.001f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * openSpeed);
            yield return null;
        }
        transform.localPosition = targetPosition;
    }
}
