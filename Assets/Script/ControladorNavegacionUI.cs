// ControladorNavegacionUI.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class ControladorNavegacionUI : MonoBehaviour
{
    public void CargarEscenaPorNombre(string nombreDeLaEscena)
    {
        if (string.IsNullOrEmpty(nombreDeLaEscena))
        {
            return;
        }

        Debug.Log($"ControladorNavegacionUI: Cargando escena '{nombreDeLaEscena}'...");

        if (FindObjectOfType<AdministradorDeEscenas>() != null) // Una forma de verificar si existe una instancia
        {
            AdministradorDeEscenas.PermitirNuevaTransicionDeEscena();
        }

        SceneManager.LoadScene(nombreDeLaEscena);
    }

    // Método para salir del juego (sin cambios)
    public void SalirDelJuego()
    {
        Debug.Log("ControladorNavegacionUI: Intentando salir del juego...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}