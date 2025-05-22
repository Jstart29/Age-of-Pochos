
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escenas

public class ControladorMenuPrincipal : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [Tooltip("El nombre exacto de la escena principal de tu juego.")]
    public string nombreEscenaDelJuego = "JuegoPrincipal"; // ¡Asegúrate de que este nombre coincida con tu escena!

    void Start()
    {
        // Asegurarse de que el cursor sea visible y esté desbloqueado en el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AdministradorDeEscenas.PermitirNuevaTransicionDeEscena();

    }

    // Método para el botón "Iniciar Partida"
    public void IniciarPartida()
    {
        if (string.IsNullOrEmpty(nombreEscenaDelJuego))
        {
            Debug.LogError("ControladorMenuPrincipal: ¡El nombre de la escena del juego no está configurado en el Inspector!");
            return;
        }
        Debug.Log($"Iniciando partida, cargando escena: {nombreEscenaDelJuego}...");
        SceneManager.LoadScene(nombreEscenaDelJuego);
    }

    // Método para el botón "Opciones"
    public void AbrirPanelOpciones()
    {
        Debug.Log("Botón de Opciones presionado. Lógica de opciones no implementada aún.");

    }

    // Método para el botón "Salir del Juego"
    public void SalirDelJuego()
    {
        Debug.Log("Intentando salir del juego...");
        Application.Quit();


#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}