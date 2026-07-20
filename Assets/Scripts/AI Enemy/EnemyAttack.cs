using Dialogue;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackRange;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public GameObject player;

    public bool canAttackPlayer;

    public Animator animator;
    public float distanceToPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        // animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // if (Application.isPlaying && (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo))
        //     return;
            
        Vector3 enemyPos = transform.position;
        Vector3 playerPos = player.transform.position;

        Vector3 playerTarget = (playerPos - enemyPos).normalized;
        playerTarget.y = 0;
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if(SettingManager.Instance.gameOver)
        {
            animator.SetFloat("UpperBody", 0f);
            canAttackPlayer = false;
            return;
        }

        if (distanceToPlayer < attackRange)
        {
            if (!Physics.Raycast(transform.position, playerTarget, distanceToPlayer, obstacleMask))
            {
                animator.SetFloat("UpperBody", 0.11f);
                canAttackPlayer = true;
            }
            else
            {
                animator.SetFloat("UpperBody", 0f);
                canAttackPlayer = false;
            }
        }
        else
        {
            animator.SetFloat("UpperBody", 0f);
            canAttackPlayer = false;
        }
    }
    public void PlayerAttack()
    {
        if (canAttackPlayer)
        {
            var settingUI = FindAnyObjectByType<SettingsUI>();
            if (settingUI != null)
            {
                settingUI.ShowGameover(true);
            }
        }
    }
}
