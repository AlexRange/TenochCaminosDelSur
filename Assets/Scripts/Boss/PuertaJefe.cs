using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaJefe : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreSiguienteEscena = "PasilloJefe";
    public KeyCode teclaInteractuar = KeyCode.UpArrow;

    [Header("Referencias")]
    public GameObject indicadorInteractuar;
    public Transform puntoTeleport;

    private bool jugadorEnRango = false;
    private Transform jugador;

    void Update()
    {
        if (jugadorEnRango && Input.GetKeyDown(teclaInteractuar))
        {
            EntrarPasilloJefe();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = true;
            jugador = other.transform;

            if (indicadorInteractuar != null)
                indicadorInteractuar.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = false;
            jugador = null;

            if (indicadorInteractuar != null)
                indicadorInteractuar.SetActive(false);
        }
    }

    void EntrarPasilloJefe()
    {
        Debug.Log("Entrando al pasillo del jefe...");

        // Cambiar a la escena del pasillo
        if (!string.IsNullOrEmpty(nombreSiguienteEscena))
        {
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
    }
}