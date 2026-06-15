using UnityEngine;

public class EnemyGrab : MonoBehaviour
{
    public Transform followPoint;
    public Transform LookPoint;

    private PlayerController player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
    }

    public void GrabPlayer()
    {
        if(player == null) return;
        player.isBeingGrab = true;
        player.ChangeCameraFollow(followPoint);
        player.ChangeCameraLookAt(LookPoint);
    }
}
