using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class BarraVidaScript : MonoBehaviour
{
    public Image rellenoBarraVida;
    public PlayerMove playerController;

    void Start()
    {
        if (playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerController = player.GetComponent<PlayerMove>();
            }
        }

        if (rellenoBarraVida == null)
        {
            Debug.LogError("rellenoBarraVida no está asignado en el Inspector");
        }

        if (playerController == null)
        {
            Debug.LogError("PlayerController no encontrado. Asegúrate de que el jugador tenga el tag 'Player'");
        }
    }

    void Update()
    {
        if (rellenoBarraVida != null && playerController != null)
        {
            rellenoBarraVida.fillAmount = playerController.vida / playerController.vidaMaxima;
        }
    }
}