using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; 

public class GameManagerConections : MonoBehaviour
{
    public static GameManagerConections Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Buscar jugador y conectar controles
        StartCoroutine(ConnectPlayerToControls());
    }

    IEnumerator ConnectPlayerToControls()
    {
        yield return new WaitForSeconds(0.1f);

        PlayerMove player = FindObjectOfType<PlayerMove>();
        if (player != null && MobileInputManager.Instance != null)
        {
            MobileInputManager.Instance.playerController = player;
            Debug.Log("Jugador conectado a controles móviles");
        }
    }
}