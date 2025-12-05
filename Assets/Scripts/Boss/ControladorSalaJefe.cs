using UnityEngine;

public class ControladorSalaJefe : MonoBehaviour
{
    [Header("Referencias")]
    public SistemaDialogos sistemaDialogos;
    public BetterBossScript scriptBoss;
    public GameObject puertaEntrada;
    public GameObject puertaSalida;
    public Transform puntoAparicionJugador;
    public Transform puntoAparicionBoss;

    [Header("Configuración")]
    public bool batallaIniciada = false;
    public bool bossDerrotado = false;

    void Start()
    {
        InicializarSala();
    }

    void InicializarSala()
    {
        // Posicionar al jugador
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null && puntoAparicionJugador != null)
        {
            jugador.transform.position = puntoAparicionJugador.position;

            // Desactivar movimiento temporalmente
            PlayerMove playerMove = jugador.GetComponent<PlayerMove>();
            if (playerMove != null)
                playerMove.enabled = false;
        }

        // Posicionar al boss
        if (scriptBoss != null && puntoAparicionBoss != null)
        {
            scriptBoss.transform.position = puntoAparicionBoss.position;
            scriptBoss.enabled = false; // Desactivar hasta que empiece la batalla
        }

        // Cerrar puerta de entrada
        if (puertaEntrada != null)
            puertaEntrada.SetActive(true);

        // Cerrar puerta de salida
        if (puertaSalida != null)
            puertaSalida.SetActive(true);

        // Iniciar diálogos
        if (sistemaDialogos != null)
        {
            sistemaDialogos.IniciarDialogoBoss();
        }
        else
        {
            // Si no hay diálogos, iniciar batalla directamente
            IniciarBatalla();
        }
    }

    public void IniciarBatalla()
    {
        if (batallaIniciada) return;

        batallaIniciada = true;
        Debug.Log("¡COMIENZA LA BATALLA CONTRA EL JEFE!");

        // Activar movimiento del jugador
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            PlayerMove playerMove = jugador.GetComponent<PlayerMove>();
            if (playerMove != null)
                playerMove.enabled = true;
        }

        // Activar el boss
        if (scriptBoss != null)
        {
            scriptBoss.enabled = true;
        }
    }

    public void BossDerrotado()
    {
        bossDerrotado = true;
        Debug.Log("¡JEFE DERROTADO!");

        // Abrir puerta de salida
        if (puertaSalida != null)
            puertaSalida.SetActive(false);

        // Opcional: mostrar mensaje de victoria
        Invoke("MostrarMensajeVictoria", 1f);
    }

    void MostrarMensajeVictoria()
    {
        Debug.Log("Puedes salir por la puerta");
        // Aquí puedes mostrar UI de victoria
    }

    // Llamar este método desde el FinalBoss cuando muera
    public void OnBossMuere()
    {
        BossDerrotado();
    }
}