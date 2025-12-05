using UnityEngine;

public class ProyectilJefe : MonoBehaviour
{
    [Header("Configuracion del Proyectil")]
    public float danio = 20f;
    public float velocidad = 8f;
    public float tiempoVida = 3f;

    [Header("Efectos")]
    public GameObject efectoImpacto;

    private Rigidbody2D rb2D;
    private Vector2 direccion;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        Destroy(gameObject, tiempoVida); // Auto-destrucción después de tiempo
    }

    public void SetDirection(Vector2 dir)
    {
        direccion = dir.normalized;
        rb2D.velocity = direccion * velocidad;

        // Rotar el proyectil hacia la dirección (opcional)
        if (direccion != Vector2.zero)
        {
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // No colisionar con el jefe que lo disparó
        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
            return;

        if (collision.CompareTag("Player"))
        {
            PlayerMove jugador = collision.GetComponent<PlayerMove>();
            if (jugador != null)
            {
                jugador.RecibirDanio(danio);
            }
            DestruirProyectil();
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            DestruirProyectil();
        }
    }

    void DestruirProyectil()
    {
        // Crear efecto de impacto si está asignado
        if (efectoImpacto != null)
        {
            Instantiate(efectoImpacto, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}