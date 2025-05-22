using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Necesario para cargar escenas

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuUI; // Asigna el panel del menú en Unity

    private bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false); // Asegura que el menú empieza desactivado
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor al inicio
        Cursor.visible = false; // Hace que el cursor no sea visible al inicio
    }

    void Update()
    {
        // Revisa si se presiona Escape, Start o el botón select en el control
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame ||
            Gamepad.all.Count > 0 && (
                Gamepad.current.startButton.wasPressedThisFrame ||
                Gamepad.current.selectButton.wasPressedThisFrame)) // Botón "+" en Nintendo
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Pausa el juego
            pauseMenuUI.SetActive(true); // Muestra el menú
            Cursor.lockState = CursorLockMode.None; // Libera el cursor
            Cursor.visible = true; // Hace que el cursor sea visible
        }
        else
        {
            Time.timeScale = 1f; // Reanuda el juego
            pauseMenuUI.SetActive(false); // Oculta el menú
            Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor
            Cursor.visible = false; // Hace que el cursor no sea visible
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // Asegura que el tiempo vuelve a la normalidad
        Application.Quit(); // Cierra el juego
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Asegura que el tiempo vuelve a la normalidad
        SceneManager.LoadScene("Menu Principal"); // Cambia a la escena del menú principal
    }

    // Función para reanudar el juego, misma lógica que TogglePause
    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause(); // Llama al mismo TogglePause para reanudar
        }
    }
}
