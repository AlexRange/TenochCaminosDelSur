using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI moneyText;
    public GameObject scorePopupPrefab; // Prefab para mostrar "+10" etc.

    [Header("Configuration")]
    public int currentScore = 0;
    public int currentMoney = 0;

    // Singleton instance
    public static ScoreManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddMoney(int amount, Vector3 position)
    {
        currentMoney += amount;
        currentScore += amount * 10; // 10 puntos por cada unidad de dinero

        Debug.Log($"¡+${amount}! Dinero: {currentMoney} | Score: {currentScore}");

        UpdateUI();
        ShowScorePopup(amount, position);
    }

    public void AddScore(int amount, Vector3 position)
    {
        currentScore += amount;
        UpdateUI();
        ShowScorePopup(amount, position, false);
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {currentScore}";

        if (moneyText != null)
            moneyText.text = $"${currentMoney}";
    }

    void ShowScorePopup(int amount, Vector3 worldPosition, bool isMoney = true)
    {
        if (scorePopupPrefab != null)
        {
            // Convertir posición mundial a pantalla
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            GameObject popup = Instantiate(scorePopupPrefab, screenPosition, Quaternion.identity);
            popup.transform.SetParent(transform); // Parent to canvas

            TextMeshProUGUI popupText = popup.GetComponent<TextMeshProUGUI>();
            if (popupText != null)
            {
                string symbol = isMoney ? "$" : "";
                popupText.text = $"+{symbol}{amount}";
                popupText.color = isMoney ? Color.yellow : Color.white;
            }

            // Destruir después de 1 segundo
            Destroy(popup, 1f);
        }
    }

    // Métodos estáticos para fácil acceso
    public static void AddMoneyStatic(int amount, Vector3 position)
    {
        if (Instance != null)
            Instance.AddMoney(amount, position);
    }

    public static int GetMoney()
    {
        return Instance != null ? Instance.currentMoney : 0;
    }

    public static int GetScore()
    {
        return Instance != null ? Instance.currentScore : 0;
    }

    // Guardar y cargar progreso
    public void SaveProgress()
    {
        PlayerPrefs.SetInt("TotalScore", currentScore);
        PlayerPrefs.SetInt("TotalMoney", currentMoney);
        PlayerPrefs.Save();
    }

    public void LoadProgress()
    {
        currentScore = PlayerPrefs.GetInt("TotalScore", 0);
        currentMoney = PlayerPrefs.GetInt("TotalMoney", 0);
        UpdateUI();
    }

    public void ResetProgress()
    {
        currentScore = 0;
        currentMoney = 0;
        UpdateUI();
        PlayerPrefs.DeleteAll();
    }
}