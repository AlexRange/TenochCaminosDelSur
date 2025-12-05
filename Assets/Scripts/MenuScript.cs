using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void Jugar()
    {
        Debug.Log("Cargando el juego...");
        SceneManager.LoadScene("GameScene");
    }
    public void Opciones()
    {
        Debug.Log("Cargando menú opciones…");
        SceneManager.LoadScene("OpcionesMenu");
    }
    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}