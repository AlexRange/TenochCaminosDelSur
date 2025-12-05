using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Dialogo
{
    public string nombrePersonaje;
    public Sprite imagenPersonaje;
    [TextArea(3, 10)]
    public string texto;
}

public class SistemaDialogos : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panelDialogo;
    public Image imagenPersonaje;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoDialogo;
    public GameObject indicadorContinuar;

    [Header("Configuración")]
    public float velocidadTexto = 0.05f;
    public KeyCode teclaContinuar = KeyCode.Space;

    [Header("Diálogos")]
    public Dialogo[] secuenciaDialogosIntro; // Diálogos iniciales
    public Dialogo[] secuenciaDialogosBoss;  // Diálogos al ver al boss

    private int indiceDialogoActual = 0;
    private bool escribiendoTexto = false;
    private bool dialogoActivo = false;
    private Dialogo[] secuenciaActual;

    void Start()
    {
        // Iniciar la cinemática al empezar el nivel
        IniciarDialogoIntro();
    }

    void Update()
    {
        if (dialogoActivo && Input.GetKeyDown(teclaContinuar))
        {
            if (escribiendoTexto)
            {
                // Completar texto inmediatamente
                StopAllCoroutines();
                textoDialogo.text = secuenciaActual[indiceDialogoActual].texto;
                escribiendoTexto = false;
                indicadorContinuar.SetActive(true);
            }
            else
            {
                // Siguiente diálogo
                SiguienteDialogo();
            }
        }
    }

    public void IniciarDialogoIntro()
    {
        secuenciaActual = secuenciaDialogosIntro;
        IniciarDialogoSecuencia();
    }

    public void IniciarDialogoBoss()
    {
        secuenciaActual = secuenciaDialogosBoss;
        IniciarDialogoSecuencia();
    }

    void IniciarDialogoSecuencia()
    {
        if (secuenciaActual == null || secuenciaActual.Length == 0)
        {
            Debug.LogWarning("No hay diálogos en la secuencia actual");
            return;
        }

        dialogoActivo = true;
        panelDialogo.SetActive(true);
        indiceDialogoActual = 0;

        // Pausar el juego
        Time.timeScale = 0f;

        MostrarDialogo(secuenciaActual[indiceDialogoActual]);
    }

    void MostrarDialogo(Dialogo dialogo)
    {
        imagenPersonaje.sprite = dialogo.imagenPersonaje;
        textoNombre.text = dialogo.nombrePersonaje;
        textoDialogo.text = "";

        indicadorContinuar.SetActive(false);
        escribiendoTexto = true;

        StartCoroutine(EscribirTexto(dialogo.texto));
    }

    IEnumerator EscribirTexto(string texto)
    {
        // Usar WaitForSecondsRealtime porque el juego está pausado
        foreach (char letra in texto.ToCharArray())
        {
            textoDialogo.text += letra;
            yield return new WaitForSecondsRealtime(velocidadTexto);
        }

        escribiendoTexto = false;
        indicadorContinuar.SetActive(true);
    }

    void SiguienteDialogo()
    {
        indiceDialogoActual++;

        if (indiceDialogoActual < secuenciaActual.Length)
        {
            MostrarDialogo(secuenciaActual[indiceDialogoActual]);
        }
        else
        {
            // Fin de la cinemática
            TerminarDialogo();
        }
    }

    void TerminarDialogo()
    {
        dialogoActivo = false;
        panelDialogo.SetActive(false);

        // Reanudar el juego
        Time.timeScale = 1f;

        Debug.Log("Diálogo terminado - Reanudando gameplay");

        // Si era el diálogo del boss, iniciar la batalla
        if (secuenciaActual == secuenciaDialogosBoss)
        {
            Debug.Log("Buscando activador del boss...");

            // Buscar el activador y iniciar batalla
            ActivadorBossPorZona activador = FindObjectOfType<ActivadorBossPorZona>();
            if (activador != null)
            {
                activador.IniciarBatalla();
            }
            else
            {
                Debug.LogError("No se encontró ActivadorBossPorZona");

                // Fallback: activar el boss manualmente
                FinalBoss boss = FindObjectOfType<FinalBoss>();
                if (boss != null)
                {
                    boss.enabled = true;
                }
            }
        }
    }

    void IniciarBatallaBoss()
    {
        // Aquí puedes activar música de batalla, etc.
        Debug.Log("¡COMIENZA LA BATALLA CONTRA EL BOSS!");
    }

    // Método público para activar desde otros scripts
    public bool EstaEnDialogo()
    {
        return dialogoActivo;
    }

    // En SistemaDialogos Mobile:
    public void AdvanceDialogMobile()
    {
        if (dialogoActivo)
        {
            if (escribiendoTexto)
            {
                // Completar texto inmediatamente
                StopAllCoroutines();
                textoDialogo.text = secuenciaActual[indiceDialogoActual].texto;
                escribiendoTexto = false;
                indicadorContinuar.SetActive(true);
            }
            else
            {
                // Siguiente diálogo
                SiguienteDialogo();
            }
        }
    }
}