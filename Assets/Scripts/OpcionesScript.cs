using UnityEngine;
using UnityEngine.SceneManagement;
public class VolverMenu : MonoBehaviour
{
    public void Regresar()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
