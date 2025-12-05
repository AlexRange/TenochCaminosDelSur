using UnityEngine;

public class MoneyCollected : MonoBehaviour
{
    [Header("Money Value")]
    public int moneyAmount = 1;
    public int scoreValue = 10;

    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;

        if (collision.CompareTag("Player"))
        {
            CollectMoney();
        }
    }

    void CollectMoney()
    {
        collected = true;

        // Ocultar sprite principal
        GetComponent<SpriteRenderer>().enabled = false;

        // Activar animación de recolección (hijo)
        if (transform.childCount > 0)
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
        }

        // Agregar al score y dinero
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddMoney(moneyAmount, transform.position);
        }
        else
        {
            Debug.LogWarning("ScoreManager no encontrado. Asegúrate de tener uno en la escena.");
        }

        // Destruir después de 0.5 segundos
        Destroy(gameObject, 0.5f);

        Debug.Log($"Dinero recolectado: ${moneyAmount}");
    }
}