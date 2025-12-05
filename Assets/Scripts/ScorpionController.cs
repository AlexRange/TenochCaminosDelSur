using System.Collections;
using UnityEngine;

public class ScorpionController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Configuración de Movimiento")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.0f;
    public float patrolDistance = 4.0f;
    public float detectionRadius = 6.0f;
    public float attackRange = 2.0f;

    [Header("Configuración de Combate")]
    public float danio = 15f;
    public float vida = 40f;
    public float attackCooldown = 1.5f;

    [Header("Configuración de Visión")]
    public LayerMask obstacleLayers;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Estados del escorpión
    private enum ScorpionState { Patrolling, Chasing, Attacking, Dead }
    private ScorpionState currentState = ScorpionState.Patrolling;

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

        // Inicializar puntos de patrulla
        patrolStartPoint = transform.position;
        patrolTargetPoint = patrolStartPoint + Vector2.right * patrolDistance;

        // Configurar layers por defecto si no están asignados
        if (obstacleLayers == 0) obstacleLayers = LayerMask.GetMask("Obstacle", "Wall");
    }

    void Update()
    {
        if (currentState == ScorpionState.Dead) return;

        CheckPlayerDetection();
        StateMachine();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (currentState == ScorpionState.Dead || currentState == ScorpionState.Attacking) return;

        MoveScorpion();
    }

    void CheckPlayerDetection()
    {
        if (currentState == ScorpionState.Dead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Verificar si el jugador está en el radio de detección
        if (distanceToPlayer <= detectionRadius)
        {
            // Verificar línea de visión
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRadius, obstacleLayers);

            if (hit.collider == null)
            {
                // Jugador visible
                playerInSight = true;

                if (distanceToPlayer <= attackRange)
                {
                    currentState = ScorpionState.Attacking;
                }
                else
                {
                    currentState = ScorpionState.Chasing;
                }
            }
            else
            {
                // Jugador no visible
                playerInSight = false;
                if (currentState == ScorpionState.Chasing || currentState == ScorpionState.Attacking)
                {
                    currentState = ScorpionState.Patrolling;
                }
            }
        }
        else
        {
            // Jugador fuera del radio de detección
            playerInSight = false;
            if (currentState == ScorpionState.Chasing || currentState == ScorpionState.Attacking)
            {
                currentState = ScorpionState.Patrolling;
            }
        }
    }

    void StateMachine()
    {
        switch (currentState)
        {
            case ScorpionState.Patrolling:
                PatrolBehavior();
                break;

            case ScorpionState.Chasing:
                ChaseBehavior();
                break;

            case ScorpionState.Attacking:
                AttackBehavior();
                break;

            case ScorpionState.Dead:
                // Comportamiento de muerte ya manejado
                break;
        }
    }

    void PatrolBehavior()
    {
        // Calcular dirección hacia el punto objetivo de patrulla
        Vector2 direction = (patrolTargetPoint - (Vector2)transform.position).normalized;
        movement = new Vector2(direction.x, 0);

        // Flip sprite según dirección
        spriteRenderer.flipX = direction.x < 0;

        // Cambiar de dirección cuando llega al punto objetivo
        float distanceToTarget = Vector2.Distance(transform.position, patrolTargetPoint);
        if (distanceToTarget < 0.3f)
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

        // Flip sprite según dirección del jugador
        spriteRenderer.flipX = (player.position.x < transform.position.x);
    }

    void AttackBehavior()
    {
        movement = Vector2.zero;

        // Mirar hacia el jugador mientras ataca
        spriteRenderer.flipX = (player.position.x < transform.position.x);

        if (canAttack)
        {
            Attack();
        }

        // Verificar si el jugador se alejó del rango de ataque
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange && playerInSight)
        {
            currentState = ScorpionState.Chasing;
        }
        else if (!playerInSight)
        {
            currentState = ScorpionState.Patrolling;
        }
    }

    void MoveScorpion()
    {
        if (movement != Vector2.zero)
        {
            float currentSpeed = currentState == ScorpionState.Chasing ? chaseSpeed : patrolSpeed;
            Vector2 newPosition = rb.position + movement * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    void UpdateAnimations()
    {
        // SOLUCIÓN: Usar solo Run e Idle como parámetros booleanos
        bool isMoving = movement != Vector2.zero;

        animator.SetBool("Run", isMoving);
        animator.SetBool("Idle", !isMoving);

        // Los triggers Attack y Dead se manejan en sus respectivos métodos
    }

    void Attack()
    {
        if (!canAttack || currentState == ScorpionState.Dead) return;

        canAttack = false;
        animator.SetTrigger("Attack");

        // SOLUCIÓN: Usar OverlapCircleAll para detectar al jugador de manera más confiable
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, attackRange * 1.2f);
        foreach (Collider2D collider in hitPlayers)
        {
            if (collider.CompareTag("Player"))
            {
                PlayerMove playerMove = collider.GetComponent<PlayerMove>();
                if (playerMove != null)
                {
                    playerMove.RecibirDanio(danio);
                    Debug.Log("¡Escorpión atacó al jugador!");
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

    public void RecibirDanio(float cantidadDanio)
    {
        if (currentState == ScorpionState.Dead) return;

        vida -= cantidadDanio;

        // SOLUCIÓN: No tenemos animación "Hurt", usar un efecto visual alternativo o simplemente no usar animación
        StartCoroutine(FlashRed());

        Debug.Log("Escorpión recibió daño. Vida: " + vida);

        // Cambiar a estado de persecución si recibe daño mientras patrulla
        if (currentState == ScorpionState.Patrolling)
        {
            currentState = ScorpionState.Chasing;
        }

        if (vida <= 0)
        {
            Morir();
        }
    }

    // Efecto visual para cuando recibe daño (ya que no tienes animación "Hurt")
    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

    private void Morir()
    {
        currentState = ScorpionState.Dead;
        movement = Vector2.zero;
        rb.velocity = Vector2.zero;

        animator.SetTrigger("Dead");
        Debug.Log("¡Escorpión murió!");

        // Deshabilitar componentes
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        // Destruir el objeto después de la animación
        Destroy(gameObject, 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Cambiar dirección de patrulla si choca con obstáculos
        if (currentState == ScorpionState.Patrolling &&
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
        // Radio de detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Radio de ataque
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Puntos de patrulla (solo en Play mode)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(patrolStartPoint, 0.3f);
            Gizmos.DrawWireSphere(patrolTargetPoint, 0.3f);
            Gizmos.DrawLine(patrolStartPoint, patrolTargetPoint);
        }
    }
}