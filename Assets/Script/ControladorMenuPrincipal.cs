
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escenas

public class ControladorMenuPrincipal : MonoBehaviour
{
    [Header("Configuraci�n de Escenas")]
    [Tooltip("El nombre exacto de la escena principal de tu juego.")]
    public string nombreEscenaDelJuego = "JuegoPrincipal"; // �Aseg�rate de que este nombre coincida con tu escena!

    void Start()
    {
        // Asegurarse de que el cursor sea visible y est� desbloqueado en el men�
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AdministradorDeEscenas.PermitirNuevaTransicionDeEscena();

    }

    // M�todo para el bot�n "Iniciar Partida"
    public void IniciarPartida()
    {
        if (string.IsNullOrEmpty(nombreEscenaDelJuego))
        {
            Debug.LogError("ControladorMenuPrincipal: �El nombre de la escena del juego no est� configurado en el Inspector!");
            return;
        }
        Debug.Log($"Iniciando partida, cargando escena: {nombreEscenaDelJuego}...");
        SceneManager.LoadScene(nombreEscenaDelJuego);
    }

    // M�todo para el bot�n "Opciones"
    public void AbrirPanelOpciones()
    {
        Debug.Log("Bot�n de Opciones presionado. L�gica de opciones no implementada a�n.");

    }

    // M�todo para el bot�n "Salir del Juego"
    public void SalirDelJuego()
    {
        Debug.Log("Intentando salir del juego...");
        Application.Quit();


#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}