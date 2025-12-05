using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpike : MonoBehaviour
{
    public float danio = 10f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Debug.Log("Player Damaged");

            PlayerMove player = collision.gameObject.GetComponent<PlayerMove>();
            if (player != null)
            {
                player.RecibirDanio(danio);
            }
        }
    }
}