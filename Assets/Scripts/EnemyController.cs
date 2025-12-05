using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public float detectionRadius = 5.0f;
    public float speed = 2.0f;
    public float danio = 10f;
    public float vida = 30f;
    public float attackCooldown = 1f;
    public float attackRange = 1f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool canAttack = true;
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void Update()
    {
        if (isDead || isAttacking) return; // No moverse durante el ataque

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Detección y movimiento
        if (distanceToPlayer < detectionRadius && distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            movement = new Vector2(direction.x, 0);

            // Animación de correr
            animator.SetBool("Run", true);
            animator.SetBool("Idle", false);

            // Flip sprite según dirección
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
        if (isDead || isAttacking) return; // No moverse durante el ataque

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

        // Aplicar daño al jugador
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
        // Esperar un tiempo suficiente para que termine la animación de ataque
        yield return new WaitForSeconds(0.6f); // Ajusta este valor según la duración de tu animación
        isAttacking = false;
    }

    public void RecibirDanio(float cantidadDanio)
    {
        if (isDead) return;

        vida -= cantidadDanio;

        // Animación de hurt
        animator.SetTrigger("Hurt");

        if (vida <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        isDead = true;
        movement = Vector2.zero;
        rb.velocity = Vector2.zero;

        animator.SetTrigger("Dead");

        // Deshabilitar colisiones y script después de morir
        GetComponent<Collider2D>().enabled = false;
        rb.gravityScale = 0;

        // Destruir después de la animación
        Destroy(gameObject, 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && canAttack && !isDead && !isAttacking)
        {
            // Forzar ataque inmediato cuando colisiona con el jugador
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
}