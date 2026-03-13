using UnityEngine;

public class EnemyHitTrigger : MonoBehaviour
{
    public LayerMask registeredHitLayer;
    public LayerMask groundLayer;
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == registeredHitLayer)
        {
            // TO DO : one hit kill player
        }
        else if(collision.gameObject.layer == groundLayer)
        {
            // TO DO : play hit ground sound
        }
    }
}
