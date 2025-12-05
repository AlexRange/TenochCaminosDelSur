using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    public BossController boss;
    public GameObject victoryUI;
    public TextMeshProUGUI victoryText; // Opcional: texto de victoria

    [Header("Configuración")]
    public string menuPrincipalScene = "MainMenu"; // Nombre de tu escena del menú principal
    public float tiempoAntesDeMenu = 5f; // Tiempo antes de cargar el menú

    private bool nivelCompletado = false;

    void Start()
    {
        // Buscar al boss automáticamente
        BuscarBoss();

        // Configurar UI
        if (victoryUI != null)
            victoryUI.SetActive(false);
    }

    void BuscarBoss()
    {
        if (boss == null)
        {
            boss = FindObjectOfType<BossController>();
            Debug.Log("GameManager: Buscando boss automáticamente...");
        }

        if (boss != null)
        {
            // Suscribirse al evento
            boss.OnBossMuerto += FinalizarNivel;
            Debug.Log("GameManager: Suscrito al evento de muerte del boss: " + boss.gameObject.name);
        }
        else
        {
            Debug.LogError("GameManager: No se encontró ningún BossController en la escena");
            // Intentar buscar de nuevo después de un tiempo
            Invoke("BuscarBoss", 1f);
        }
    }

    void Update()
    {
        // DEBUG: Tecla para testear el fin del nivel
        if (Input.GetKeyDown(KeyCode.T) && !nivelCompletado)
        {
            Debug.Log("TEST: Forzando fin del nivel");
            FinalizarNivel();
        }
    }

    void FinalizarNivel()
    {
        if (nivelCompletado) return;

        nivelCompletado = true;
        Debug.Log("GameManager: ¡Boss derrotado! Finalizando nivel...");

        StartCoroutine(ProcesoFinNivel());
    }

    private IEnumerator ProcesoFinNivel()
    {
        Debug.Log("1. Esperando animación de muerte...");
        yield return new WaitForSeconds(2f);

        Debug.Log("2. Mostrando UI de victoria...");

        // Mostrar UI de victoria
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);

            // Configurar texto si existe
            if (victoryText != null)
            {
                victoryText.text = "¡VICTORIA!\nDerrotaste al Boss Final\n\nRegresando al menú principal...";
            }
        }

        Debug.Log("3. Deshabilitando controles del jugador...");

        // Deshabilitar controles del jugador
        PlayerMove jugador = FindObjectOfType<PlayerMove>();
        if (jugador != null)
        {
            jugador.enabled = false;
            // También detener cualquier movimiento
            Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }

        Debug.Log("4. Esperando " + tiempoAntesDeMenu + " segundos antes del menú...");
        yield return new WaitForSeconds(tiempoAntesDeMenu);

        Debug.Log("5. Cargando menú principal: " + menuPrincipalScene);

        // Cargar menú principal
        CargarMenuPrincipal();
    }

    public void CargarMenuPrincipal()
    {
        // Reanudar el tiempo si estaba pausado
        Time.timeScale = 1f;

        // Cargar la escena del menú principal
        SceneManager.LoadScene(menuPrincipalScene);
    }

    // Método para testear desde el Inspector
    [ContextMenu("Test Fin del Nivel")]
    public void TestFinDelNivel()
    {
        if (!nivelCompletado)
        {
            FinalizarNivel();
        }
    }

    void OnDestroy()
    {
        // Limpiar suscripción
        if (boss != null)
        {
            boss.OnBossMuerto -= FinalizarNivel;
        }
    }
}