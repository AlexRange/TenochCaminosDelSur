using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Configuración Básica")]
    public float runSpeed = 2;
    public float jumpSpeed = 3;
    public float doubleJumpSpeed = 2.5f;
    public float vida = 100f;
    public float vidaMaxima = 100f;
    public float cooldownAtaque = 0.5f;
    public Vector3 spawnPoint = Vector3.zero;

    [Header("Configuración Móvil")]
    public float mobileSpeedMultiplier = 1.3f;
    public float mobileJumpMultiplier = 1.1f;

    [Header("Referencias")]
    public CombateCC combateCC;
    public SistemaDialogos sistemaDialogos; // Referencia al sistema de diálogos

    // Componentes
    private Rigidbody2D rb2D;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D playerCollider;

    // Estados
    private bool canDoubleJump;
    private bool recibeDanio = false;
    private bool puedeAtacar = true;
    private bool estaAtacando = false;
    private bool estaMuerto = false;

    // Input
    private float currentHorizontalInput = 0f;
    private bool jumpRequested = false;
    private bool attackRequested = false;
    private bool interactRequested = false;
    private bool advanceDialogRequested = false; // Nuevo: para avanzar diálogo

    // Plataforma
    private bool isMobile = false;

    void Awake()
    {
        // Detectar plataforma
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        isMobile = true;
#else
        isMobile = false;
#endif
    }

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();

        if (combateCC == null)
        {
            combateCC = GetComponent<CombateCC>();
        }

        // Buscar sistema de diálogos si no está asignado
        if (sistemaDialogos == null)
        {
            sistemaDialogos = FindObjectOfType<SistemaDialogos>();
        }

        vida = vidaMaxima;

        if (spawnPoint == Vector3.zero)
            spawnPoint = transform.position;

        // Conectar con MobileInputManager SIEMPRE, en cada Start
        StartCoroutine(ConnectToMobileControls());
    }

    IEnumerator ConnectToMobileControls()
    {
        // Esperar un momento para asegurar que MobileInputManager esté listo
        yield return new WaitForEndOfFrame();

        if (isMobile && MobileInputManager.Instance != null)
        {
            // Conectar este jugador con el manager
            MobileInputManager.Instance.playerController = this;

            // Pasar referencia del sistema de diálogos
            if (sistemaDialogos != null)
            {
                MobileInputManager.Instance.SetDialogSystem(sistemaDialogos);
            }

            Debug.Log("Jugador conectado a controles móviles");
        }
    }

    void Update()
    {
        if (estaMuerto || recibeDanio || vida <= 0 || estaAtacando) return;

        // Procesar inputs según plataforma
        if (isMobile)
        {
            ProcessMobileUpdate();
        }
        else
        {
            ProcessDesktopUpdate();
        }

        UpdateJumpAnimations();
    }

    void FixedUpdate()
    {
        if (estaMuerto || recibeDanio || vida <= 0 || estaAtacando) return;

        ApplyMovement();
    }

    void ProcessMobileUpdate()
    {
        // Los inputs vienen de MobileInputManager
        // Solo procesamos acciones inmediatas aquí
    }

    void ProcessDesktopUpdate()
    {
        // Input de teclado para salto
        if (Input.GetKeyDown("space"))
        {
            jumpRequested = true;
        }

        // Input de teclado para ataque
        if ((Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && puedeAtacar)
        {
            attackRequested = true;
        }

        // Input de teclado para interacción
        if (Input.GetKeyDown(KeyCode.E))
        {
            interactRequested = true;
        }

        // Input de teclado para avanzar diálogo (Space o Enter)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            advanceDialogRequested = true;
        }

        // Input de movimiento horizontal (teclado)
        currentHorizontalInput = 0f;
        if (Input.GetKey("d") || Input.GetKey("right"))
        {
            currentHorizontalInput = 1f;
        }
        else if (Input.GetKey("a") || Input.GetKey("left"))
        {
            currentHorizontalInput = -1f;
        }
    }

    void ApplyMovement()
    {
        if (Mathf.Abs(currentHorizontalInput) > 0.1f)
        {
            float speed = runSpeed * (isMobile ? mobileSpeedMultiplier : 1f);
            rb2D.velocity = new Vector2(currentHorizontalInput * speed, rb2D.velocity.y);
            spriteRenderer.flipX = currentHorizontalInput < 0;
            animator.SetBool("Run", true);
        }
        else
        {
            rb2D.velocity = new Vector2(0, rb2D.velocity.y);
            animator.SetBool("Run", false);
        }

        // Procesar saltos
        if (jumpRequested)
        {
            PerformJump();
            jumpRequested = false;
        }

        // Procesar ataques
        if (attackRequested && puedeAtacar)
        {
            PerformAttack();
            attackRequested = false;
        }

        // Procesar interacciones
        if (interactRequested)
        {
            PerformInteraction();
            interactRequested = false;
        }

        // Procesar avance de diálogo
        if (advanceDialogRequested)
        {
            AdvanceDialog();
            advanceDialogRequested = false;
        }
    }

    void PerformJump()
    {
        if (CheckGround.isGrounded)
        {
            canDoubleJump = true;
            float jumpForce = jumpSpeed * (isMobile ? mobileJumpMultiplier : 1f);
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpForce);
            animator.SetBool("Jump", true);
        }
        else
        {
            if (canDoubleJump)
            {
                animator.SetBool("DoubleJump", true);
                float doubleJumpForce = doubleJumpSpeed * (isMobile ? mobileJumpMultiplier : 1f);
                rb2D.velocity = new Vector2(rb2D.velocity.x, doubleJumpForce);
                canDoubleJump = false;
            }
        }
    }

    void PerformAttack()
    {
        puedeAtacar = false;
        estaAtacando = true;

        animator.SetTrigger("Attack");

        StartCoroutine(CooldownAtaque());
        StartCoroutine(FinAtaque());
    }

    void PerformInteraction()
    {
        Debug.Log("Interactuando desde PlayerController");
        // Aquí va tu lógica de interacción
        Collider2D[] interactables = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D collider in interactables)
        {
            if (collider.CompareTag("Interactable"))
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }

    // NUEVO MÉTODO: Avanzar diálogo
    void AdvanceDialog()
    {
        // Si hay un sistema de diálogos y está activo, avanzar el diálogo
        if (sistemaDialogos != null)
        {
            // Verificar si el sistema de diálogos está en modo diálogo activo
            if (sistemaDialogos.EstaEnDialogo())
            {
                // Simular la tecla de espacio para avanzar diálogo
                // En realidad, el sistema de diálogos maneja esto internamente
                // pero podemos llamar a un método público si existe
                if (sistemaDialogos.GetType().GetMethod("AdvanceDialogMobile") != null)
                {
                    sistemaDialogos.SendMessage("AdvanceDialogMobile");
                }
            }
        }
    }

    void UpdateJumpAnimations()
    {
        if (CheckGround.isGrounded == false)
        {
            animator.SetBool("Jump", true);
            animator.SetBool("Run", false);
        }
        if (CheckGround.isGrounded == true)
        {
            animator.SetBool("Jump", false);
            animator.SetBool("DoubleJump", false);
            animator.SetBool("Falling", false);
        }

        if (rb2D.velocity.y < 0)
        {
            animator.SetBool("Falling", true);
        }
        else if (rb2D.velocity.y > 0)
        {
            animator.SetBool("Falling", false);
        }
    }

    IEnumerator CooldownAtaque()
    {
        yield return new WaitForSeconds(cooldownAtaque);
        puedeAtacar = true;
    }

    IEnumerator FinAtaque()
    {
        yield return new WaitForSeconds(0.4f);
        estaAtacando = false;
    }

    // MÉTODOS PÚBLICOS PARA MOBILE INPUT MANAGER
    public void SetHorizontalInput(float input)
    {
        currentHorizontalInput = Mathf.Clamp(input, -1f, 1f);
    }

    public void Jump()
    {
        jumpRequested = true;
    }

    public void Attack()
    {
        if (puedeAtacar)
            attackRequested = true;
    }

    public void Interact()
    {
        interactRequested = true;
    }

    public void AdvanceDialogRequest() // Renombrado de PauseGame()
    {
        advanceDialogRequested = true;
    }

    // Métodos existentes (sin cambios)
    public void RecibirDanio(float cantidadDanio)
    {
        if (recibeDanio || vida <= 0 || estaMuerto) return;

        vida -= cantidadDanio;
        if (vida < 0) vida = 0;

        Debug.Log("Vida restante: " + vida);

        if (animator != null)
        {
            animator.SetTrigger("Damage");
        }

        recibeDanio = true;
        Vector2 direccionRebote = new Vector2(spriteRenderer.flipX ? 1 : -1, 1).normalized;
        rb2D.velocity = Vector2.zero;
        rb2D.AddForce(direccionRebote * 5f, ForceMode2D.Impulse);

        StartCoroutine(DesactivarEstadoDanio());

        if (vida <= 0)
        {
            Morir();
        }
    }

    private IEnumerator DesactivarEstadoDanio()
    {
        yield return new WaitForSeconds(0.5f);
        recibeDanio = false;
    }

    private void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        Debug.Log("¡El jugador ha muerto!");

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        rb2D.velocity = Vector2.zero;
        rb2D.isKinematic = true;

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1f);
        ReactivarJugador();
        Debug.Log("¡Jugador respawneado!");
    }

    private void ReactivarJugador()
    {
        estaMuerto = false;
        vida = vidaMaxima;
        rb2D.isKinematic = false;
        rb2D.velocity = Vector2.zero;
        transform.position = spawnPoint;

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.ResetTrigger("Damage");
            animator.ResetTrigger("Attack");
            animator.Play("Idle", -1, 0f);
            animator.SetBool("Run", false);
            animator.SetBool("Jump", false);
            animator.SetBool("DoubleJump", false);
            animator.SetBool("Falling", false);
        }

        recibeDanio = false;
        estaAtacando = false;
        puedeAtacar = true;
        canDoubleJump = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        enabled = true;

        // Reconectar controles después del respawn
        StartCoroutine(ConnectToMobileControls());
    }

    public void SetSpawnPoint(Vector3 newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }

    // Nuevo método para verificar si hay diálogo activo
    public bool IsInDialogue()
    {
        return sistemaDialogos != null && sistemaDialogos.EstaEnDialogo();
    }

    // Método para obtener el estado de ataque
    public bool IsAttacking()
    {
        return estaAtacando;
    }

    // Método para obtener el estado de daño
    public bool IsTakingDamage()
    {
        return recibeDanio;
    }

    // Método para obtener el estado de muerte
    public bool IsDead()
    {
        return estaMuerto;
    }
}