using UnityEngine;

// Interfaz para objetos interactuables
public interface IInteractable
{
    void Interact();
}

// Ejemplo de implementación para un cartel
public class SignInteractable : MonoBehaviour, IInteractable
{
    [TextArea(3, 10)]
    public string message = "¡Hola! Esto es un cartel interactivo.";

    public void Interact()
    {
        Debug.Log("Mensaje del cartel: " + message);
        // Aquí puedes mostrar el mensaje en la UI
        // Ejemplo: UIManager.Instance.ShowMessage(message);
    }
}

// Ejemplo para objetos que dan items
public class ItemInteractable : MonoBehaviour, IInteractable
{
    public string itemName = "Poción de salud";
    public int amount = 1;

    public void Interact()
    {
        Debug.Log("Recogiste: " + amount + " " + itemName);
        // Aquí puedes agregar el item al inventario
        Destroy(gameObject); // Destruir después de interactuar
    }
}