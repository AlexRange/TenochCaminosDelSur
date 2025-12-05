using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Estadísticas")]
    public Transform player;
    public float detectionRadius = 5.0f;
    public float speed = 2.0f;
    public float danio = 10f;
    public float vida = 300f;
    public float attackCooldown = 1f;
    public float attackRange = 1f;

    [Header("Componentes")]
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Estados")]
    private bool canAttack = true;
    private bool isDead = false;
    private bool isAttacking = false;

    // Evento para cuando el boss muera
    public System.Action OnBossMuerto;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        Debug.Log("BossController Inicializado - Vida: " + vida);
    }

    void Update()
    {
        if (isDead || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius && distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            movement = new Vector2(direction.x, 0);

            animator.SetBool("Run", true);
            animator.SetBool("Idle", false);

            spriteRenderer.flipX = direction.x < 0;
        }
        else if (distanceToPlayer <= attackRange && canAttack)
        {
            movement = Vector2.zero;
            animator.SetBool("Run", false);
            animator.SetBool("Idle", true);
            Attack();
        }
        else
        {
            movement = Vector2.zero;
            animator.SetBool("Run", false);
            animator.SetBool("Idle", true);
        }
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking) return;

        if (movement != Vector2.zero)
        {
            rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
        }
    }

    void Attack()
    {
        if (!canAttack || isDead || isAttacking) return;

        canAttack = false;
        isAttacking = true;

        animator.SetTrigger("Attack");

        PlayerMove playerMove = player.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.RecibirDanio(danio);
        }

        StartCoroutine(ResetAttack());
        StartCoroutine(ResetAttackingState());
    }

    private System.Collections.IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private System.Collections.IEnumerator ResetAttackingState()
    {
        yield return new WaitForSeconds(0.6f);
        isAttacking = false;
    }

    public void RecibirDanio(float cantidadDanio)
    {
        if (isDead) return;

        vida -= cantidadDanio;
        Debug.Log("Boss recibió daño. Vida restante: " + vida);

        animator.SetTrigger("Hurt");

        if (vida <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        if (isDead) return;

        isDead = true;
        movement = Vector2.zero;
        rb.velocity = Vector2.zero;

        Debug.Log("BOSS: Iniciando animación de muerte");

        animator.SetTrigger("Dead");

        GetComponent<Collider2D>().enabled = false;
        rb.gravityScale = 0;

        // Notificar que el boss murió
        Debug.Log("BOSS: Invocando evento OnBossMuerto");
        OnBossMuerto?.Invoke();

        // Destruir después de la animación
        Destroy(gameObject, 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && canAttack && !isDead && !isAttacking)
        {
            Attack();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // Método de test desde Inspector
    [ContextMenu("Test Matar Boss")]
    public void TestMatarBoss()
    {
        if (!isDead)
        {
            Debug.Log("TEST: Matando boss desde Inspector");
            Morir();
        }
    }
}