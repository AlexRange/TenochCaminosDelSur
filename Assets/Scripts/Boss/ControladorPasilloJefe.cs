using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorPasilloJefe : MonoBehaviour
{
    [Header("Configuración")]
    public float duracionPasillo = 3f;
    public string nombreSalaJefe = "SalaJefe";

    [Header("Referencias")]
    public Animator transicionAnimator;
    public string triggerEntrada = "Entrar";
    public string triggerSalida = "Salir";

    void Start()
    {
        IniciarSecuenciaPasillo();
    }

    void IniciarSecuenciaPasillo()
    {
        // Animación de entrada al pasillo
        if (transicionAnimator != null)
            transicionAnimator.SetTrigger(triggerEntrada);

        // Esperar y luego ir a la sala del jefe
        Invoke("IrASalaJefe", duracionPasillo);
    }

    void IrASalaJefe()
    {
        // Animación de salida del pasillo
        if (transicionAnimator != null)
            transicionAnimator.SetTrigger(triggerSalida);

        // Cambiar a la sala del jefe después de la animación
        Invoke("CargarSalaJefe", 1f);
    }

    void CargarSalaJefe()
    {
        SceneManager.LoadScene(nombreSalaJefe);
    }
}