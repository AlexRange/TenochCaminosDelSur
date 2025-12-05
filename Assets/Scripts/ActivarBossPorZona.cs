using UnityEngine;

public class ActivadorBossPorZona : MonoBehaviour
{
    [Header("Referencias")]
    public SistemaDialogos sistemaDialogos;
    public GameObject bossObject; // Referencia al GameObject completo del boss
    public FinalBoss scriptBoss;
    public Transform jugador;

    [Header("Configuración")]
    public float distanciaActivacion = 10f;
    private bool activado = false;
    private bool batallaIniciada = false;

    void Start()
    {
        // Desactivar COMPLETAMENTE el boss al inicio
        DesactivarBossCompletamente();
        Debug.Log("Boss desactivado al inicio");
    }

    void Update()
    {
        if (!activado && !batallaIniciada && jugador != null)
        {
            float distancia = Vector3.Distance(transform.position, jugador.position);

            if (distancia <= distanciaActivacion)
            {
                Debug.Log("Jugador entró en zona de activación. Distancia: " + distancia);
                ActivarDialogoBoss();
            }
        }
    }

    void DesactivarBossCompletamente()
    {
        // Desactivar el script del boss
        if (scriptBoss != null)
        {
            scriptBoss.enabled = false;
        }

        // Desactivar el GameObject del boss (opcional - prueba con esto)
        // if (bossObject != null)
        // {
        //     bossObject.SetActive(false);
        // }

        // También desactivar el Rigidbody2D para que no se mueva
        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }

    void ActivarBossCompletamente()
    {
        // Reactivar el script del boss
        if (scriptBoss != null)
        {
            scriptBoss.enabled = true;
        }

        // Reactivar el GameObject del boss si lo desactivaste
        // if (bossObject != null)
        // {
        //     bossObject.SetActive(true);
        // }

        // Reactivar el Rigidbody2D
        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    void ActivarDialogoBoss()
    {
        activado = true;

        // Desactivar movimiento del jugador
        PlayerMove playerMove = jugador.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = false;
            Debug.Log("Movimiento del jugador desactivado");
        }

        // Asegurar que el boss esté desactivado
        DesactivarBossCompletamente();

        if (sistemaDialogos != null)
        {
            Debug.Log("Iniciando diálogo del boss...");
            sistemaDialogos.IniciarDialogoBoss();
        }
        else
        {
            Debug.LogError("SistemaDialogos no asignado");
        }
    }

    public void IniciarBatalla()
    {
        if (batallaIniciada) return;

        batallaIniciada = true;
        Debug.Log("INICIANDO BATALLA CONTRA EL BOSS");

        // Reactivar movimiento del jugador
        PlayerMove playerMove = jugador.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = true;
            Debug.Log("Movimiento del jugador reactivado");
        }

        // Activar el boss
        ActivarBossCompletamente();
        Debug.Log("Boss activado para la batalla");

        // Opcional: Destruir este activador ya que no se necesita más
        // Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacion);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = activado ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacion);
    }
}