using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FinalBoss : MonoBehaviour
{
    [Header("Configuración del Jefe")]
    public float vidaMaxima = 500f;
    public float vidaActual;
    public float danioAtaque = 30f;
    public float rangoAtaque = 3f;
    public float velocidad = 2f;
    public float cooldownAtaque = 2f;

    [Header("Referencias")]
    public Transform jugador;
    public GameObject proyectilPrefab;
    public Transform puntoDisparo;
    public string nombreSiguienteNivel = "GameScene";
    public SistemaDialogos sistemaDialogos;

    [Header("Componentes")]
    private Animator animator;
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;

    [Header("Estados")]
    private bool estaMuerto = false;
    private bool puedeAtacar = true;
    private bool fase2Activada = false;

    void Start()
    {
        // Obtener componentes
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Inicializar vida
        vidaActual = vidaMaxima;

        // Buscar al jugador automáticamente si no está asignado
        if (jugador == null)
        {
            GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
            if (jugadorObj != null)
                jugador = jugadorObj.transform;
        }
    }

    void Update()
    {
        // No hacer nada si hay diálogos activos
        if (sistemaDialogos != null && sistemaDialogos.EstaEnDialogo())
            return;

        if (estaMuerto) return;

        // Verificar si debe activarse la fase 2
        if (!fase2Activada && vidaActual <= vidaMaxima * 0.5f)
        {
            ActivarFase2();
        }

        // Perseguir al jugador
        if (jugador != null)
        {
            PerseguirJugador();
        }
    }

    void PerseguirJugador()
    {
        if (estaMuerto) return;

        // Calcular dirección hacia el jugador
        Vector2 direccion = (jugador.position - transform.position).normalized;

        // Movimiento
        rb2D.velocity = new Vector2(direccion.x * velocidad, rb2D.velocity.y);

        // Flip del sprite según la dirección
        if (direccion.x > 0)
            spriteRenderer.flipX = false;
        else if (direccion.x < 0)
            spriteRenderer.flipX = true;

        // Actualizar animación de movimiento - CAMBIADO A "Run"
        if (animator != null)
        {
            animator.SetBool("Run", Mathf.Abs(rb2D.velocity.x) > 0.1f);
        }

        // Verificar si está en rango para atacar
        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
        if (distanciaAlJugador <= rangoAtaque && puedeAtacar)
        {
            Atacar();
        }
    }

    void Atacar()
    {
        if (!puedeAtacar || estaMuerto) return;

        puedeAtacar = false;

        // Elegir tipo de ataque aleatorio o por fase
        int tipoAtaque = fase2Activada ? Random.Range(0, 3) : 0;

        switch (tipoAtaque)
        {
            case 0: // Ataque cuerpo a cuerpo
                StartCoroutine(AtaqueCuerpoACuerpo());
                break;
            case 1: // Ataque a distancia
                StartCoroutine(AtaqueDistancia());
                break;
            case 2: // Ataque especial - USARÁ "Recharge"
                StartCoroutine(AtaqueEspecial());
                break;
        }

        // Cooldown del ataque
        StartCoroutine(CooldownAtaque());
    }

    IEnumerator AtaqueCuerpoACuerpo()
    {
        // CAMBIADO: Usa "Attack" que ya tienes
        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

        // Detectar jugador en el rango de ataque
        Collider2D[] objetivos = Physics2D.OverlapCircleAll(transform.position, rangoAtaque);
        foreach (Collider2D colision in objetivos)
        {
            if (colision.CompareTag("Player"))
            {
                PlayerMove jugador = colision.GetComponent<PlayerMove>();
                if (jugador != null)
                {
                    jugador.RecibirDanio(danioAtaque);
                }
            }
        }
    }

    IEnumerator AtaqueDistancia()
    {
        // CAMBIADO: Usa "Shot" que ya tienes
        if (animator != null)
            animator.SetTrigger("Shot");

        yield return new WaitForSeconds(0.3f);

        // Disparar proyectil
        if (proyectilPrefab != null && puntoDisparo != null)
        {
            GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);
            Vector2 direccion = (jugador.position - puntoDisparo.position).normalized;

            // Configurar dirección del proyectil
            ProyectilJefe proyectilScript = proyectil.GetComponent<ProyectilJefe>();
            if (proyectilScript != null)
            {
                proyectilScript.SetDirection(direccion);
            }
            else
            {
                // Fallback si no tiene el script
                proyectil.GetComponent<Rigidbody2D>().velocity = direccion * 8f;
            }
        }
    }

    IEnumerator AtaqueEspecial()
    {
        // CAMBIADO: Usa "Recharge" para el ataque especial
        if (animator != null)
            animator.SetTrigger("Recharge");

        // Disparar múltiples proyectiles
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.5f);

            if (proyectilPrefab != null && puntoDisparo != null)
            {
                GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);

                // Dispersar los proyectiles
                float angulo = -30f + (i * 30f);
                Vector2 direccion = Quaternion.Euler(0, 0, angulo) *
                                   (spriteRenderer.flipX ? Vector2.left : Vector2.right);

                proyectil.GetComponent<Rigidbody2D>().velocity = direccion * 6f;
                Destroy(proyectil, 4f);
            }
        }
    }

    IEnumerator CooldownAtaque()
    {
        yield return new WaitForSeconds(cooldownAtaque);
        puedeAtacar = true;
    }

    void ActivarFase2()
    {
        fase2Activada = true;

        // Aumentar stats en fase 2
        velocidad *= 1.5f;
        cooldownAtaque *= 0.7f;

        // CAMBIADO: Usa "Idle_2" para la transformación de fase 2
        if (animator != null)
            animator.SetTrigger("Idle_2");

        Debug.Log("¡FASE 2 ACTIVADA! El jefe se ha vuelto más rápido y agresivo.");
    }

    public void RecibirDanio(float cantidadDanio)
    {
        if (estaMuerto)
        {
            Debug.Log("Boss ya está muerto, no recibe más daño");
            return;
        }

        Debug.Log("RECIBIENDO DAÑO - Cantidad: " + cantidadDanio + " - Vida antes: " + vidaActual);

        vidaActual -= cantidadDanio;

        // CAMBIADO: Usa "Hurt" que ya tienes
        if (animator != null)
        {
            animator.SetTrigger("Hurt");
            Debug.Log("Animación Hurt activada");
        }
        else
        {
            Debug.LogError("Animator no encontrado en el Boss");
        }

        // Efecto visual de daño (parpadeo)
        StartCoroutine(EfectoDanio());

        Debug.Log("Jefe recibió " + cantidadDanio + " de daño. Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            Debug.Log("VIDA LLEGÓ A CERO - LLAMANDO A MORIR()");
            Morir();
        }
    }

    IEnumerator EfectoDanio()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    void Morir()
    {
        estaMuerto = true;
        Debug.Log("¡JEFE DERROTADO!");

        // CAMBIADO: Usa "Dead" que ya tienes
        if (animator != null)
            animator.SetTrigger("Dead");

        // Deshabilitar componentes
        rb2D.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        // Iniciar secuencia de victoria
        StartCoroutine(Victoria());
    }

    IEnumerator Victoria()
    {
        // Esperar a que termine la animación de muerte
        yield return new WaitForSeconds(2f);

        // Mostrar mensaje de victoria
        Debug.Log("¡NIVEL COMPLETADO!");

        // Pasar al siguiente nivel
        CambiarNivel();
    }

    void CambiarNivel()
    {
        // Método 1: Cargar escena por nombre
        if (!string.IsNullOrEmpty(nombreSiguienteNivel))
        {
            SceneManager.LoadScene(nombreSiguienteNivel);
        }
        // Método 2: Cargar siguiente escena en build settings
        else
        {
            int siguienteEscena = SceneManager.GetActiveScene().buildIndex + 1;
            if (siguienteEscena < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(siguienteEscena);
            }
            else
            {
                Debug.Log("¡JUEGO COMPLETADO!");
                // O volver al menú principal
                SceneManager.LoadScene("MenuPrincipal");
            }
        }
    }

    // Visualizar rangos en el Editor
    private void OnDrawGizmosSelected()
    {
        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);

        // Rango de detección (opcional)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 10f);
    }
}