using UnityEngine;

public class CamaraController : MonoBehaviour
{
    public Transform objetivo;
    public float velocidadCamara = 0.1f;
    public Vector3 desplazamiento = new Vector3(0, 0, -10);

    private void LateUpdate()
    {
        if (objetivo != null)
        {
            Vector3 posicionDeseada = objetivo.position + desplazamiento;
            Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, velocidadCamara);
            transform.position = posicionSuavizada;
        }
    }
}