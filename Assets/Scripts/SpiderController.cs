using System.Collections;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Configuración de Movimiento")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.0f;
    public float patrolDistance = 3.0f;
    public float detectionRadius = 5.0f;
    public float attackRange = 1.5f;

    [Header("Configuración de Combate")]
    public float danio = 10f;
    public float vida = 30f;
    public float attackCooldown = 1f;

    [Header("Configuración de Raycast")]
    public LayerMask obstacleLayers;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Estados
    private enum EnemyState { Patrolling, Chasing, Attacking, Dead }
    private EnemyState currentState = EnemyState.Patrolling;

    // Variables de patrulla
    private Vector2 patrolStartPoint;
    private Vector2 patrolTargetPoint;
    private bool movingRight = true;

    // Variables de ataque
    private bool canAttack = true;
    private bool playerInSight = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        patrolStartPoint = transform.position;
        patrolTargetPoint = patrolStartPoint + Vector2.right * patrolDistance;

        if (obstacleLayers == 0) obstacleLayers = LayerMask.GetMask("Obstacle", "Wall");
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        CheckPlayerDetection();
        StateMachine();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.Dead || currentState == EnemyState.Attacking) return;
        MoveEnemy();
    }

    void CheckPlayerDetection()
    {
        if (currentState == EnemyState.Dead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRadius)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRadius, obstacleLayers);

            if (hit.collider == null)
            {
                playerInSight = true;
                if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attacking;
                }
                else
                {
                    currentState = EnemyState.Chasing;
                }
            }
            else
            {
                playerInSight = false;
                if (currentState == EnemyState.Chasing || currentState == EnemyState.Attacking)
                {
                    currentState = EnemyState.Patrolling;
                }
            }
        }
        else
        {
            playerInSight = false;
            if (currentState == EnemyState.Chasing || currentState == EnemyState.Attacking)
            {
                currentState = EnemyState.Patrolling;
            }
        }
    }

    void StateMachine()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                PatrolBehavior();
                break;
            case EnemyState.Chasing:
                ChaseBehavior();
                break;
            case EnemyState.Attacking:
                AttackBehavior();
                break;
        }
    }

    void PatrolBehavior()
    {
        Vector2 direction = (patrolTargetPoint - (Vector2)transform.position).normalized;
        movement = new Vector2(direction.x, 0);
        spriteRenderer.flipX = direction.x < 0;

        float distanceToTarget = Vector2.Distance(transform.position, patrolTargetPoint);
        if (distanceToTarget < 0.2f)
        {
            movingRight = !movingRight;
            patrolTargetPoint = movingRight ?
                patrolStartPoint + Vector2.right * patrolDistance :
                patrolStartPoint - Vector2.right * patrolDistance;
        }
    }

    void ChaseBehavior()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        movement = new Vector2(direction.x, 0);
        spriteRenderer.flipX = direction.x < 0;
    }

    void AttackBehavior()
    {
        movement = Vector2.zero;
        if (canAttack)
        {
            Attack();
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange && playerInSight)
        {
            currentState = EnemyState.Chasing;
        }
        else if (!playerInSight)
        {
            currentState = EnemyState.Patrolling;
        }
    }

    void MoveEnemy()
    {
        if (movement != Vector2.zero)
        {
            float currentSpeed = currentState == EnemyState.Chasing ? chaseSpeed : patrolSpeed;
            rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
        }
    }

    void UpdateAnimations()
    {
        // SOLUCIÓN: Solo usar Run e Idle
        bool isMoving = movement != Vector2.zero;
        bool isChasing = currentState == EnemyState.Chasing;
        bool isPatrolling = currentState == EnemyState.Patrolling;

        // Usar "Run" para ambos movimientos (patrulla y persecución)
        animator.SetBool("Run", isMoving);
        animator.SetBool("Idle", !isMoving);
    }

    void Attack()
    {
        if (!canAttack || currentState == EnemyState.Dead) return;

        canAttack = false;
        animator.SetTrigger("Attack");

        // SOLUCIÓN: Usar OverlapCircleAll para detectar al jugador
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange * 1.2f);
        foreach (Collider2D collider in hitPlayers)
        {
            if (collider.CompareTag("Player"))
            {
                PlayerMove playerMove = collider.GetComponent<PlayerMove>();
                if (playerMove != null)
                {
                    playerMove.RecibirDanio(danio);
                    Debug.Log("¡Araña atacó al jugador!");
                }
            }
        }

        StartCoroutine(ResetAttack());
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // SOLUCIÓN: Hacer este método público para que el jugador pueda llamarlo
    public void RecibirDanio(float cantidadDanio)
    {
        if (currentState == EnemyState.Dead) return;

        vida -= cantidadDanio;
        animator.SetTrigger("Hurt");
        Debug.Log("Araña recibió daño. Vida: " + vida);

        if (currentState == EnemyState.Patrolling)
        {
            currentState = EnemyState.Chasing;
        }

        if (vida <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        currentState = EnemyState.Dead;
        movement = Vector2.zero;
        rb.velocity = Vector2.zero;

        animator.SetTrigger("Dead");
        Debug.Log("¡Araña murió!");

        GetComponent<Collider2D>().enabled = false;
        rb.gravityScale = 0;

        Destroy(gameObject, 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == EnemyState.Patrolling &&
            (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle")))
        {
            movingRight = !movingRight;
            patrolTargetPoint = movingRight ?
                patrolStartPoint + Vector2.right * patrolDistance :
                patrolStartPoint - Vector2.right * patrolDistance;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}