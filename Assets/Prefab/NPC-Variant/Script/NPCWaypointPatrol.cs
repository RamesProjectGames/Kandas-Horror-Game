using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWaypointPatrol : MonoBehaviour
{
    public enum PatrolMode { Loop, Once }

    [Header("Movement & Waypoints")]
    [Tooltip("Daftar titik patok jalan NPC (Transform empty object)")]
    public List<Transform> waypoints = new List<Transform>();
    public PatrolMode patrolMode = PatrolMode.Loop;
    public float stopDistanceThreshold = 0.5f;

    [Header("Idle Settings at Waypoints")]
    [Tooltip("Durasi diam (Idle) NPC saat sampai di setiap titik sebelum lanjut jalan")]
    public float idleDurationAtWaypoint = 2f;

    [Header("Trigger Settings")]
    [Tooltip("Jika centang aktif, NPC diam di tempat sampai dipicu oleh Player/Trigger Box")]
    public bool waitOnTrigger = false;

    [Header("Animation Reference")]
    [Tooltip("Nama parameter Bool di Animator Anda (misal: 'isWalking' / 'IsMoving')")]
    public string isWalkingParam = "isWalking";

    [Header("Footstep Audio Settings")]
    public AudioSource footstepAudioSource;
    public AudioClip[] footstepSounds;
    public float stepInterval = 0.5f; // Jeda antar langkah kaki dalam detik
    private float stepTimer;

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private bool hasBeenTriggered = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Jika tidak memakai trigger, langsung jalankan NPC
        if (!waitOnTrigger)
        {
            StartPatrol();
        }
        else
        {
            UpdateAnimation(false); // Idle di tempat awal
        }
    }

    void Update()
    {
        if (waitOnTrigger && !hasBeenTriggered) return;
        if (waypoints.Count == 0 || isWaiting) return;

        // Logika Suara Langkah Kaki berdasarkan kecepatan gerak NPC
        if (agent.velocity.magnitude > 0.1f && !agent.isStopped)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstepSound();
                stepTimer = stepInterval;
            }
        }

        if (!agent.pathPending && agent.remainingDistance <= stopDistanceThreshold)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepAudioSource != null && footstepSounds != null && footstepSounds.Length > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            footstepAudioSource.PlayOneShot(clip);
        }
    }

    public void StartPatrol()
    {
        hasBeenTriggered = true;
        if (waypoints.Count > 0)
        {
            SetDestinationToCurrentWaypoint();
        }
    }

    private void SetDestinationToCurrentWaypoint()
    {
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        agent.isStopped = false;
        UpdateAnimation(true);
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        agent.isStopped = true;
        UpdateAnimation(false); // Ubah ke animasi Idle

        yield return new WaitForSeconds(idleDurationAtWaypoint);

        // Hitung indeks waypoint berikutnya
        if (patrolMode == PatrolMode.Loop)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            isWaiting = false;
            SetDestinationToCurrentWaypoint();
        }
        else if (patrolMode == PatrolMode.Once)
        {
            if (currentWaypointIndex < waypoints.Count - 1)
            {
                currentWaypointIndex++;
                isWaiting = false;
                SetDestinationToCurrentWaypoint();
            }
            else
            {
                // NPC sampai di waypoint terakhir dan berhenti permanen
                UpdateAnimation(false);
            }
        }
    }

    private void UpdateAnimation(bool isWalking)
    {
        if (animator != null && !string.IsNullOrEmpty(isWalkingParam))
        {
            animator.SetBool(isWalkingParam, isWalking);
        }
    }
}
