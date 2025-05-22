using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Necesario para cargar escenas

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuUI; // Asigna el panel del men� en Unity

    private bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false); // Asegura que el men� empieza desactivado
        Cursor.lockState = CursorLockMode.Locked; // Bloquea el cursor al inicio
        Cursor.visible = false; // Hace que el cursor no sea visible al inicio
    }

    void Update()
    {
        // Revisa si se presiona Escape, Start o el bot�n select en el control
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame ||
            Gamepad.all.Count > 0 && (
                Gamepad.current.startButton.wasPressedThisFrame ||
                Gamepad.current.selectButton.wasPressedThisFrame)) // Bot�n "+" en Nintendo
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
            pauseMenuUI.SetActive(true); // Muestra el men�
            Cursor.lockState = CursorLockMode.None; // Libera el cursor
            Cursor.visible = true; // Hace que el cursor sea visible
        }
        else
        {
            Time.timeScale = 1f; // Reanuda el juego
            pauseMenuUI.SetActive(false); // Oculta el men�
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
        SceneManager.LoadScene("Menu Principal"); // Cambia a la escena del men� principal
    }

    // Funci�n para reanudar el juego, misma l�gica que TogglePause
    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause(); // Llama al mismo TogglePause para reanudar
        }
    }
}
