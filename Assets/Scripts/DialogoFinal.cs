using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogoFinalJuego : MonoBehaviour
{
    [System.Serializable]
    public class DialogoLinea
    {
        [TextArea(2, 5)]
        public string texto;
        public float velocidadTexto = 0.05f;
        public float tiempoEspera = 2f; // Tiempo que permanece la línea en pantalla
        public float tiempoTransicion = 1f; // Tiempo de fade entre líneas
    }

    [Header("Configuración del Diálogo")]
    public List<DialogoLinea> lineasDialogo;
    public string nombreMenuPrincipal = "MenuPrincipal";
    public float tiempoFinalAntesDeMenu = 3f; // Tiempo después del último diálogo

    [Header("Referencias UI - TextMeshPro")]
    public GameObject panelDialogo;
    public TextMeshProUGUI textoDialogo;
    public Image fondoDialogo;

    [Header("Configuración Visual")]
    public Color colorTexto = Color.white;
    public Color colorFondo = new Color(0, 0, 0, 0.8f);

    [Header("Configuración de TextMeshPro")]
    public TMP_FontAsset fuenteDialogo;
    public int tamañoTextoDialogo = 36;
    public FontStyles estiloTextoDialogo = FontStyles.Normal;

    [Header("Configuración de Animación")]
    public bool usarFadeEntreLineas = true;
    public float velocidadFade = 2f;

    private int indiceLineaActual = 0;
    private bool escribiendo = false;
    private bool dialogoEnProgreso = true;
    private Coroutine corrutinaEscritura;

    void Start()
    {
        // Aplicar estilos de TextMeshPro
        AplicarEstilosTextMeshPro();

        // Configurar colores
        if (textoDialogo != null)
            textoDialogo.color = colorTexto;

        if (fondoDialogo != null)
            fondoDialogo.color = colorFondo;

        // Asegurar que el panel esté activo
        if (panelDialogo != null)
            panelDialogo.SetActive(true);

        // Iniciar diálogo automáticamente
        StartCoroutine(ProcesarDialogoCompleto());
    }

    void AplicarEstilosTextMeshPro()
    {
        // Configurar TextMeshPro para el diálogo principal
        if (textoDialogo != null)
        {
            if (fuenteDialogo != null)
                textoDialogo.font = fuenteDialogo;

            textoDialogo.fontSize = tamañoTextoDialogo;
            textoDialogo.fontStyle = estiloTextoDialogo;
            textoDialogo.alignment = TextAlignmentOptions.Center;
            textoDialogo.enableWordWrapping = true;
            textoDialogo.alpha = 0f; // Iniciar invisible para fade in
        }
    }

    IEnumerator ProcesarDialogoCompleto()
    {
        // Esperar un momento antes de empezar
        yield return new WaitForSeconds(1f);

        // Procesar cada línea de diálogo
        for (int i = 0; i < lineasDialogo.Count; i++)
        {
            indiceLineaActual = i;
            DialogoLinea linea = lineasDialogo[i];

            // Mostrar la línea actual
            yield return StartCoroutine(MostrarLineaConEfecto(linea));

            // Si no es la última línea, esperar y transicionar
            if (i < lineasDialogo.Count - 1)
            {
                // Esperar el tiempo configurado para esta línea
                yield return new WaitForSeconds(linea.tiempoEspera);

                // Transición a la siguiente línea si está habilitado
                if (usarFadeEntreLineas)
                {
                    yield return StartCoroutine(FadeOutTexto(linea.tiempoTransicion));
                }
                else
                {
                    // Limpiar texto rápidamente
                    textoDialogo.text = "";
                }
            }
        }

        // Última línea ya se mostró, ahora esperar
        yield return new WaitForSeconds(tiempoFinalAntesDeMenu);

        // Transición final al menú principal
        yield return StartCoroutine(TransicionAlMenu());
    }

    IEnumerator MostrarLineaConEfecto(DialogoLinea linea)
    {
        // Limpiar texto
        if (textoDialogo != null)
            textoDialogo.text = "";

        // Fade in del texto si está habilitado
        if (usarFadeEntreLineas)
        {
            yield return StartCoroutine(FadeInTexto(linea.tiempoTransicion));
        }
        else
        {
            textoDialogo.alpha = 1f;
        }

        // Escribir texto caracter por caracter
        escribiendo = true;
        corrutinaEscritura = StartCoroutine(EscribirTexto(linea.texto, linea.velocidadTexto));

        // Esperar a que termine la escritura
        while (escribiendo)
            yield return null;
    }

    IEnumerator EscribirTexto(string texto, float velocidad)
    {
        if (textoDialogo == null) yield break;

        // Escribir caracter por caracter
        for (int i = 0; i < texto.Length; i++)
        {
            textoDialogo.text += texto[i];

            // Efecto de sonido opcional (descomentar si quieres)
            // if (sonidoEscribir != null) sonidoEscribir.Play();

            yield return new WaitForSeconds(velocidad);
        }

        escribiendo = false;
    }

    IEnumerator FadeInTexto(float duracion)
    {
        float tiempoTranscurrido = 0f;
        Color color = textoDialogo.color;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime * velocidadFade;
            float alpha = Mathf.Lerp(0f, 1f, tiempoTranscurrido / duracion);

            color.a = alpha;
            textoDialogo.color = color;

            yield return null;
        }

        color.a = 1f;
        textoDialogo.color = color;
    }

    IEnumerator FadeOutTexto(float duracion)
    {
        float tiempoTranscurrido = 0f;
        Color color = textoDialogo.color;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime * velocidadFade;
            float alpha = Mathf.Lerp(1f, 0f, tiempoTranscurrido / duracion);

            color.a = alpha;
            textoDialogo.color = color;

            yield return null;
        }

        color.a = 0f;
        textoDialogo.color = color;
        textoDialogo.text = "";
    }

    IEnumerator TransicionAlMenu()
    {
        // Fade out final más largo
        float duracionFade = 3f;
        float tiempoTranscurrido = 0f;

        Color colorFondoInicial = fondoDialogo.color;
        Color colorFondoFinal = Color.black;

        Color colorTextoInicial = textoDialogo.color;
        Color colorTextoFinal = new Color(1f, 1f, 1f, 0f);

        // Efecto de fade out para fondo y texto
        while (tiempoTranscurrido < duracionFade)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = tiempoTranscurrido / duracionFade;

            // Fade del fondo
            if (fondoDialogo != null)
            {
                fondoDialogo.color = Color.Lerp(colorFondoInicial, colorFondoFinal, t);
            }

            // Fade del texto
            if (textoDialogo != null)
            {
                textoDialogo.color = Color.Lerp(colorTextoInicial, colorTextoFinal, t);
            }

            yield return null;
        }

        // Asegurar que esté completamente negro
        if (fondoDialogo != null)
            fondoDialogo.color = Color.black;

        if (textoDialogo != null)
            textoDialogo.color = new Color(1f, 1f, 1f, 0f);

        // Pausa final antes de cambiar de escena
        yield return new WaitForSeconds(2f);

        // Cargar menú principal
        SceneManager.LoadScene(nombreMenuPrincipal);
    }

    // Método para skip (opcional, para pruebas)
    void Update()
    {
        // Solo para pruebas en el editor - permite saltar con Espacio
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaltarDialogo();
        }
#endif
    }

    void SaltarDialogo()
    {
        if (corrutinaEscritura != null)
        {
            StopCoroutine(corrutinaEscritura);
        }

        StopAllCoroutines();

        // Completar texto actual
        if (textoDialogo != null && indiceLineaActual < lineasDialogo.Count)
        {
            textoDialogo.text = lineasDialogo[indiceLineaActual].texto;
            textoDialogo.alpha = 1f;
        }

        // Ir directamente al menú
        StartCoroutine(TransicionAlMenu());
    }

    // Método para cargar diálogo desde otro script
    public void CargarDialogo(List<DialogoLinea> nuevoDialogo)
    {
        lineasDialogo = nuevoDialogo;
        StartCoroutine(ProcesarDialogoCompleto());
    }

    // Métodos adicionales útiles
    public void CambiarFuenteDialogo(TMP_FontAsset nuevaFuente)
    {
        if (textoDialogo != null && nuevaFuente != null)
        {
            textoDialogo.font = nuevaFuente;
        }
    }

    public void CambiarVelocidadTexto(float nuevaVelocidad)
    {
        // Afecta a todas las líneas
        foreach (var linea in lineasDialogo)
        {
            linea.velocidadTexto = nuevaVelocidad;
        }
    }
}