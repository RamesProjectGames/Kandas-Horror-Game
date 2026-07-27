using UnityEngine;

public class DissapearingEnemy : MonoBehaviour
{
    private PlayerSightInteraction playerSightInteraction;
    public GameObject ObjectToDisable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerSightInteraction = FindFirstObjectByType<PlayerSightInteraction>();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerSightInteraction == null)
        {
            return;
        }
        if (!playerSightInteraction.GetVisibleEnemies().Contains(ObjectToDisable.transform) )
        {
            ObjectToDisable.transform.LeanMoveY(-11.77f, 0.5f);
        }
        else
        {
            ObjectToDisable.transform.LeanMoveY(11.77f, 0.5f);
        }
    }
}
