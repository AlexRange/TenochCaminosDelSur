using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Canvas))]
public class MobileInputManager : MonoBehaviour
{
    // Singleton para acceso fácil
    public static MobileInputManager Instance { get; private set; }

    [Header("Referencias UI")]
    public Button jumpButton;
    public Button attackButton;
    public Button interactButton;
    public Button dialogButton; // Renombrado de pauseButton a dialogButton

    [Header("Configuración Botones")]
    public float buttonPressScale = 0.85f;
    public float buttonPressDuration = 0.1f;

    [Header("Referencias")]
    public PlayerMove playerController;
    public SistemaDialogos sistemaDialogos; // Referencia al sistema de diálogos

    // Variables de estado
    private bool jumpPressed = false;
    private bool attackPressed = false;
    private bool interactPressed = false;
    private bool dialogPressed = false; // Renombrado de pausePressed

    // Para detectar si estamos en móvil
    private bool isMobilePlatform = false;

    // Para almacenar escalas originales
    private Vector3 jumpButtonOriginalScale;
    private Vector3 attackButtonOriginalScale;
    private Vector3 interactButtonOriginalScale;
    private Vector3 dialogButtonOriginalScale;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Detectar plataforma
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        isMobilePlatform = true;
        Debug.Log("Plataforma móvil detectada");
#else
        isMobilePlatform = false;
        Debug.Log("Plataforma no móvil, controles ocultos");
#endif
    }

    void Start()
    {
        // Guardar escalas originales
        if (jumpButton != null) jumpButtonOriginalScale = jumpButton.transform.localScale;
        if (attackButton != null) attackButtonOriginalScale = attackButton.transform.localScale;
        if (interactButton != null) interactButtonOriginalScale = interactButton.transform.localScale;
        if (dialogButton != null) dialogButtonOriginalScale = dialogButton.transform.localScale;

        InitializeControls();
        SetupOrientation();
    }

    void InitializeControls()
    {
        // Configurar visibilidad según plataforma
        if (!isMobilePlatform)
        {
            gameObject.SetActive(false);
            return;
        }

        // Configurar botones
        SetupButtons();

        Debug.Log("Controles móviles inicializados");
    }

    void SetupOrientation()
    {
        if (!isMobilePlatform) return;

        // Forzar landscape
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // Ajustar calidad para móviles
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    void SetupButtons()
    {
        // Botón de salto
        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(() =>
            {
                jumpPressed = true;
                StartCoroutine(ButtonPressEffect(jumpButton.transform, jumpButtonOriginalScale));
            });
        }

        // Botón de ataque
        if (attackButton != null)
        {
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(() =>
            {
                attackPressed = true;
                StartCoroutine(ButtonPressEffect(attackButton.transform, attackButtonOriginalScale));
            });
        }

        // Botón de interacción
        if (interactButton != null)
        {
            interactButton.onClick.RemoveAllListeners();
            interactButton.onClick.AddListener(() =>
            {
                interactPressed = true;
                StartCoroutine(ButtonPressEffect(interactButton.transform, interactButtonOriginalScale));
            });
        }

        // Botón de diálogo (antes pausa)
        if (dialogButton != null)
        {
            dialogButton.onClick.RemoveAllListeners();
            dialogButton.onClick.AddListener(() =>
            {
                dialogPressed = true;
                StartCoroutine(ButtonPressEffect(dialogButton.transform, dialogButtonOriginalScale));
            });
        }
    }

    IEnumerator ButtonPressEffect(Transform buttonTransform, Vector3 originalScale)
    {
        // Reducir tamaño
        buttonTransform.localScale = originalScale * buttonPressScale;

        // Esperar
        yield return new WaitForSeconds(buttonPressDuration);

        // Restaurar tamaño original
        buttonTransform.localScale = originalScale;
    }

    void Update()
    {
        if (!isMobilePlatform) return;

        // Procesar inputs
        ProcessInputs();

        // Resetear botones después de procesar
        ResetButtonStates();
    }

    void ProcessInputs()
    {
        // Saltar
        if (jumpPressed && playerController != null)
        {
            playerController.Jump();
        }

        // Atacar
        if (attackPressed && playerController != null)
        {
            playerController.Attack();
        }

        // Interactuar
        if (interactPressed && playerController != null)
        {
            playerController.Interact();
        }

        // Diálogo - VERSIÓN CORREGIDA (sin duplicación)
        if (dialogPressed)
        {
            // Primero intentar con el sistema de diálogos directamente
            if (sistemaDialogos != null)
            {
                sistemaDialogos.AdvanceDialogMobile();
            }
            // Si no hay sistema de diálogos o para manejo adicional, usar el player controller
            else if (playerController != null)
            {
                playerController.AdvanceDialogRequest();
            }
        }
    }

    void ResetButtonStates()
    {
        jumpPressed = false;
        attackPressed = false;
        interactPressed = false;
        dialogPressed = false;
    }

    // Métodos públicos
    public bool IsMobile()
    {
        return isMobilePlatform;
    }

    // Actualizar referencia a sistema de diálogos si es necesario
    public void SetDialogSystem(SistemaDialogos dialogSystem)
    {
        sistemaDialogos = dialogSystem;
    }
}