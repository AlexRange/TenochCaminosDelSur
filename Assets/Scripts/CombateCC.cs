using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombateCC : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    [SerializeField] private Transform controladorGolpe;
    [SerializeField] private float radioGolpe = 0.8f;
    [SerializeField] private float danioGolpe = 20f;
    [SerializeField] private float knockbackForce = 3f;

    private PlayerMove playerMove;
    private SpriteRenderer playerSprite;
    private bool golpeActivo = false;

    void Start()
    {
        playerMove = GetComponent<PlayerMove>();
        playerSprite = GetComponent<SpriteRenderer>();

        // Crear el controlador de golpe si no está asignado
        if (controladorGolpe == null)
        {
            GameObject controladorObj = new GameObject("ControladorGolpe");
            controladorGolpe = controladorObj.transform;
            controladorGolpe.SetParent(transform);
            controladorGolpe.localPosition = new Vector3(0.5f, 0f, 0f);
        }
    }

    // Método para activar el golpe (llamado desde Animation Events)
    public void ActivarGolpe()
    {
        golpeActivo = true;
        RealizarGolpe();
    }

    // Método para desactivar el golpe (llamado desde Animation Events)
    public void DesactivarGolpe()
    {
        golpeActivo = false;
    }

    private void RealizarGolpe()
    {
        if (!golpeActivo) return;

        // Actualizar posición del controlador según dirección del jugador
        if (playerSprite != null)
        {
            Vector3 offset = playerSprite.flipX ?
                new Vector3(-0.5f, 0f, 0f) :
                new Vector3(0.5f, 0f, 0f);

            controladorGolpe.localPosition = offset;
        }

        // Detectar enemigos en el área
        Collider2D[] objetos = Physics2D.OverlapCircleAll(controladorGolpe.position, radioGolpe);
        bool golpeConectado = false;

        foreach (Collider2D colisionador in objetos)
        {
            if (colisionador.CompareTag("Enemy"))
            {
                AplicarDanio(colisionador);
                golpeConectado = true;
            }
        }

        if (golpeConectado)
        {
            Debug.Log("¡Golpe conectado!");
        }
    }

    private void AplicarDanio(Collider2D enemyCollider)
    {
        Debug.Log("Aplicando daño a: " + enemyCollider.name);

        bool danioAplicado = false;

        // Spider
        SpiderController spider = enemyCollider.GetComponent<SpiderController>();
        if (spider != null)
        {
            spider.RecibirDanio(danioGolpe);
            danioAplicado = true;
        }

        // Scorpion
        if (!danioAplicado)
        {
            ScorpionController scorpion = enemyCollider.GetComponent<ScorpionController>();
            if (scorpion != null)
            {
                scorpion.RecibirDanio(danioGolpe);
                danioAplicado = true;
            }
        }

        // Enemy genérico
        if (!danioAplicado)
        {
            EnemyController enemigo = enemyCollider.GetComponent<EnemyController>();
            if (enemigo != null)
            {
                enemigo.RecibirDanio(danioGolpe);
                danioAplicado = true;
            }
        }

        // Boss
        if (!danioAplicado)
        {
            FinalBoss boss = enemyCollider.GetComponent<FinalBoss>();
            if (boss != null)
            {
                boss.RecibirDanio(danioGolpe);
                danioAplicado = true;
            }
        }

        if (danioAplicado)
        {
            AplicarKnockback(enemyCollider);
        }
    }

    private void AplicarKnockback(Collider2D enemyCollider)
    {
        Rigidbody2D enemyRb = enemyCollider.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            Vector2 direccion = (enemyCollider.transform.position - transform.position).normalized;
            enemyRb.AddForce(direccion * knockbackForce, ForceMode2D.Impulse);
        }
    }

    // Para debug visual en el Editor
    private void OnDrawGizmos()
    {
        if (controladorGolpe != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(controladorGolpe.position, radioGolpe);
        }
    }
}