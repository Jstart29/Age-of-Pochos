using UnityEngine;
using UnityEngine.SceneManagement;
public class ControladorNavegacionUI : MonoBehaviour
{
    public void CargarEscenaPorNombre(string nombreDeLaEscena)
    {
        if (string.IsNullOrEmpty(nombreDeLaEscena))
        {
            return;
        }

        Debug.Log($"ControladorNavegacionUI: Cargando escena '{nombreDeLaEscena}'...");

        if (FindObjectOfType<AdministradorDeEscenas>() != null)
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