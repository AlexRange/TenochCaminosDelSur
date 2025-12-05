using UnityEngine;
using TMPro;
using System.Collections;

public class CartelInteractivo : MonoBehaviour
{
    [Header("Configuración del Cartel")]
    public KeyCode teclaInteractuar = KeyCode.E;
    public float rangoDeteccion = 2f;

    [Header("Contenido del Cartel")]
    [TextArea(3, 10)]
    public string textoCartel = "¡Bienvenido! Pulsa E para leer las instrucciones.";

    [Header("Referencias UI")]
    public GameObject panelCartel;
    public TextMeshProUGUI textoUI;
    public GameObject indicadorPulsaE;

    [Header("Configuración")]
    public float tiempoMostrarCartel = 0f; // 0 = no se cierra automáticamente
    public bool puedeReutilizar = true;

    private bool jugadorEnRango = false;
    private bool cartelActivo = false;
    private Transform jugador;

    void Start()
    {
        Debug.Log("Cartel inicializado - Buscando referencias...");

        // Ocultar UI al inicio
        OcultarUI();

        // Buscar jugador automáticamente
        BuscarJugador();

        // Verificar referencias
        VerificarReferencias();
    }

    void BuscarJugador()
    {
        if (jugador == null)
        {
            GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
            if (jugadorObj != null)
            {
                jugador = jugadorObj.transform;
                Debug.Log("Jugador encontrado: " + jugador.name);
            }
            else
            {
                Debug.LogError("No se encontró ningún GameObject con tag 'Player'");
            }
        }
    }

    void VerificarReferencias()
    {
        if (panelCartel == null)
            Debug.LogError("Panel Cartel no asignado en el inspector");

        if (textoUI == null)
            Debug.LogError("Texto UI no asignado en el inspector");

        if (indicadorPulsaE == null)
            Debug.LogError("Indicador Pulsa E no asignado en el inspector");
    }

    void Update()
    {
        if (jugador == null)
        {
            BuscarJugador();
            return;
        }

        // Verificar distancia al jugador
        float distancia = Vector2.Distance(transform.position, jugador.position);
        bool estabaEnRango = jugadorEnRango;
        jugadorEnRango = distancia <= rangoDeteccion;

        // Mostrar/ocultar indicador "Pulsa E"
        if (jugadorEnRango && !cartelActivo)
        {
            if (!estabaEnRango)
            {
                MostrarIndicador();
                Debug.Log("Jugador entró en rango del cartel");
            }
        }
        else
        {
            if (estabaEnRango && !cartelActivo)
            {
                OcultarIndicador();
                Debug.Log("Jugador salió del rango del cartel");
            }
        }

        // Interacción con el cartel
        if (jugadorEnRango && Input.GetKeyDown(teclaInteractuar))
        {
            if (!cartelActivo)
            {
                MostrarCartel();
            }
            else
            {
                CerrarCartel();
            }
        }
    }

    void MostrarIndicador()
    {
        if (indicadorPulsaE != null)
        {
            indicadorPulsaE.SetActive(true);
        }
    }

    void OcultarIndicador()
    {
        if (indicadorPulsaE != null)
        {
            indicadorPulsaE.SetActive(false);
        }
    }

    void MostrarCartel()
    {
        cartelActivo = true;
        Debug.Log("Mostrando cartel: " + textoCartel);

        // Ocultar indicador "Pulsa E"
        OcultarIndicador();

        // Mostrar panel del cartel
        if (panelCartel != null)
        {
            panelCartel.SetActive(true);
        }

        // Mostrar texto
        if (textoUI != null)
        {
            textoUI.text = textoCartel;
        }

        // Cierre automático después de un tiempo (si está configurado)
        if (tiempoMostrarCartel > 0)
        {
            StartCoroutine(CierreAutomatico());
        }
    }

    void CerrarCartel()
    {
        cartelActivo = false;
        Debug.Log("Cerrando cartel");
        OcultarUI();

        // Si no es reutilizable, desactivar el cartel
        if (!puedeReutilizar)
        {
            this.enabled = false;
        }
    }

    IEnumerator CierreAutomatico()
    {
        yield return new WaitForSeconds(tiempoMostrarCartel);

        if (cartelActivo)
        {
            CerrarCartel();
        }
    }

    void OcultarUI()
    {
        if (panelCartel != null)
            panelCartel.SetActive(false);

        if (indicadorPulsaE != null)
            indicadorPulsaE.SetActive(false);
    }

    // Visualizar rango de detección en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}