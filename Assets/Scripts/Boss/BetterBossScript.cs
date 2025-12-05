using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BetterBossScript : MonoBehaviour
{
    [Header("Configuración del Jefe")]
    public float vidaMaxima = 500f;
    public float vidaActual;
    public float danioAtaque = 30f;
    public float rangoAtaque = 3f;
    public float velocidad = 2f;
    public float cooldownAtaque = 2f;
    public float fuerzaSalto = 8f;
    public float tiempoEntreSaltos = 3f;

    [Header("Referencias")]
    public Transform jugador;
    public GameObject proyectilPrefab;
    public Transform puntoDisparo;
    public ControladorSalaJefe controladorSala;
    public string nombreSiguienteNivel = "Nivel3";

    [Header("Componentes")]
    private Animator animator;
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    private Collider2D bossCollider;

    [Header("Estados")]
    private bool estaMuerto = false;
    private bool puedeAtacar = true;
    private bool puedeSaltar = true;
    private bool fase2Activada = false;
    private bool fase3Activada = false;

    [Header("Patrones de Ataque")]
    public float[] probabilidadesAtaque = { 0.4f, 0.3f, 0.2f, 0.1f }; // Cuerpo, Distancia, Salto, Especial

    void Start()
    {
        // Obtener componentes
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();

        // Inicializar vida
        vidaActual = vidaMaxima;

        // Buscar al jugador automáticamente si no está asignado
        if (jugador == null)
        {
            GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
            if (jugadorObj != null)
                jugador = jugadorObj.transform;
        }

        // Iniciar rutina de saltos
        StartCoroutine(RutinaSaltos());
    }

    void Update()
    {
        if (estaMuerto) return;

        // Verificar cambios de fase
        VerificarFases();

        // Perseguir al jugador
        if (jugador != null)
        {
            PerseguirJugador();
        }
    }

    void VerificarFases()
    {
        float porcentajeVida = vidaActual / vidaMaxima;

        if (!fase2Activada && porcentajeVida <= 0.6f)
        {
            ActivarFase2();
        }

        if (!fase3Activada && porcentajeVida <= 0.3f)
        {
            ActivarFase3();
        }
    }

    void PerseguirJugador()
    {
        if (estaMuerto) return;

        // Calcular dirección hacia el jugador
        Vector2 direccion = (jugador.position - transform.position).normalized;

        // Movimiento solo si está en el suelo
        if (Mathf.Abs(rb2D.velocity.y) < 0.1f)
        {
            rb2D.velocity = new Vector2(direccion.x * velocidad, rb2D.velocity.y);
        }

        // Flip del sprite según la dirección
        if (direccion.x > 0)
            spriteRenderer.flipX = false;
        else if (direccion.x < 0)
            spriteRenderer.flipX = true;

        // Actualizar animación de movimiento
        if (animator != null)
        {
            animator.SetBool("Run", Mathf.Abs(rb2D.velocity.x) > 0.1f);
        }

        // Verificar si está en rango para atacar
        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);
        if (distanciaAlJugador <= rangoAtaque && puedeAtacar)
        {
            ElegirAtaque();
        }
    }

    void ElegirAtaque()
    {
        if (!puedeAtacar || estaMuerto) return;

        puedeAtacar = false;

        // Elegir ataque basado en probabilidades y fase
        float random = Random.value;
        float acumulado = 0f;

        for (int i = 0; i < probabilidadesAtaque.Length; i++)
        {
            acumulado += probabilidadesAtaque[i];
            if (random <= acumulado)
            {
                EjecutarAtaque(i);
                break;
            }
        }

        // Cooldown del ataque
        StartCoroutine(CooldownAtaque());
    }

    void EjecutarAtaque(int tipoAtaque)
    {
        switch (tipoAtaque)
        {
            case 0: // Ataque cuerpo a cuerpo
                StartCoroutine(AtaqueCuerpoACuerpo());
                break;
            case 1: // Ataque a distancia
                StartCoroutine(AtaqueDistancia());
                break;
            case 2: // Ataque con salto
                StartCoroutine(AtaqueSalto());
                break;
            case 3: // Ataque especial
                StartCoroutine(AtaqueEspecial());
                break;
        }
    }

    IEnumerator AtaqueCuerpoACuerpo()
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

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
        if (animator != null)
            animator.SetTrigger("Shot");

        yield return new WaitForSeconds(0.3f);

        if (proyectilPrefab != null && puntoDisparo != null)
        {
            GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);
            Vector2 direccion = (jugador.position - puntoDisparo.position).normalized;

            ProyectilJefe proyectilScript = proyectil.GetComponent<ProyectilJefe>();
            if (proyectilScript != null)
            {
                proyectilScript.SetDirection(direccion);
            }
        }
    }

    IEnumerator AtaqueSalto()
    {
        if (animator != null)
            animator.SetTrigger("Jump");

        // Saltar hacia el jugador
        Vector2 direccionSalto = ((Vector2)jugador.position - (Vector2)transform.position).normalized;
        rb2D.velocity = new Vector2(direccionSalto.x * 3f, fuerzaSalto);

        yield return new WaitForSeconds(0.8f);

        // Ataque al caer
        Collider2D[] objetivos = Physics2D.OverlapCircleAll(transform.position, rangoAtaque * 1.5f);
        foreach (Collider2D colision in objetivos)
        {
            if (colision.CompareTag("Player"))
            {
                PlayerMove jugador = colision.GetComponent<PlayerMove>();
                if (jugador != null)
                {
                    jugador.RecibirDanio(danioAtaque * 1.2f); // Más daño en ataque de salto
                }
            }
        }
    }

    IEnumerator AtaqueEspecial()
    {
        if (animator != null)
            animator.SetTrigger("Recharge");

        // Disparar múltiples proyectiles en abanico
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.2f);

            if (proyectilPrefab != null && puntoDisparo != null)
            {
                GameObject proyectil = Instantiate(proyectilPrefab, puntoDisparo.position, Quaternion.identity);

                float angulo = -45f + (i * 22.5f);
                Vector2 direccion = Quaternion.Euler(0, 0, angulo) *
                                   (spriteRenderer.flipX ? Vector2.left : Vector2.right);

                ProyectilJefe proyectilScript = proyectil.GetComponent<ProyectilJefe>();
                if (proyectilScript != null)
                {
                    proyectilScript.SetDirection(direccion);
                }
            }
        }
    }

    IEnumerator RutinaSaltos()
    {
        while (!estaMuerto)
        {
            yield return new WaitForSeconds(tiempoEntreSaltos);

            if (!estaMuerto && puedeSaltar && Random.value > 0.7f) // 30% de probabilidad
            {
                SaltarAleatorio();
            }
        }
    }

    void SaltarAleatorio()
    {
        if (puedeSaltar && Mathf.Abs(rb2D.velocity.y) < 0.1f)
        {
            Vector2 direccionSalto = new Vector2(Random.Range(-1f, 1f), 1).normalized;
            rb2D.velocity = direccionSalto * fuerzaSalto * 0.7f;

            if (animator != null)
                animator.SetTrigger("Jump");
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
        velocidad *= 1.3f;
        cooldownAtaque *= 0.8f;

        // Cambiar probabilidades de ataque
        probabilidadesAtaque = new float[] { 0.3f, 0.3f, 0.3f, 0.1f };

        if (animator != null)
            animator.SetTrigger("Idle_2");

        Debug.Log("¡FASE 2 ACTIVADA! El jefe es más rápido y agresivo.");
    }

    void ActivarFase3()
    {
        fase3Activada = true;
        velocidad *= 1.2f;
        cooldownAtaque *= 0.7f;
        tiempoEntreSaltos *= 0.6f;

        // Cambiar probabilidades de ataque
        probabilidadesAtaque = new float[] { 0.2f, 0.2f, 0.3f, 0.3f };

        if (animator != null)
            animator.SetTrigger("Idle_2");

        Debug.Log("¡FASE 3 ACTIVADA! El jefe está desesperado.");
    }

    public void RecibirDanio(float cantidadDanio)
    {
        if (estaMuerto) return;

        vidaActual -= cantidadDanio;

        if (animator != null)
            animator.SetTrigger("Hurt");

        StartCoroutine(EfectoDanio());

        Debug.Log("Jefe recibió " + cantidadDanio + " de daño. Vida: " + vidaActual);

        if (vidaActual <= 0)
        {
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

        if (animator != null)
            animator.SetTrigger("Dead");

        rb2D.velocity = Vector2.zero;
        if (bossCollider != null)
            bossCollider.enabled = false;

        // Notificar al controlador de la sala
        if (controladorSala != null)
        {
            controladorSala.OnBossMuere();
        }

        StartCoroutine(Victoria());
    }

    IEnumerator Victoria()
    {
        yield return new WaitForSeconds(3f);

        if (!string.IsNullOrEmpty(nombreSiguienteNivel))
        {
            SceneManager.LoadScene(nombreSiguienteNivel);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}