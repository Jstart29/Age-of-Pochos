using UnityEngine;

public class ControladorCamaraRTS : MonoBehaviour
{
    [Header("Velocidades de Movimiento")]
    public float velocidadMovimiento = 20f;    // Velocidad para WASD
    public float velocidadRotacion = 80f;     // Velocidad para Q y E (grados por segundo)
    public float velocidadElevacion = 15f;    // Velocidad para Espacio y Shift+Espacio

    [Header("Configuraci�n Inicial de C�mara")]
    public float anguloInclinacionInicial = 45f; // �ngulo en X para la vista isom�trica

    [Header("Sensibilidad del Zoom (Scroll)")]
    public float velocidadZoom = 10f;
    public float alturaMinimaZoom = 5f;  // Altura Y m�nima a la que puede llegar la c�mara
    public float alturaMaximaZoom = 50f; // Altura Y m�xima

    private float rotacionYActual; 

    void Start()
    {
        rotacionYActual = transform.eulerAngles.y;

        // Aplicar la inclinaci�n inicial en X, manteniendo la rotaci�n Y y Z=0
        AplicarRotacionInclinacion();
    }

    void Update()
    {
        // --- Movimiento con WASD ---
        Vector3 direccionInput = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            direccionInput += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direccionInput += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direccionInput += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direccionInput += Vector3.right;
        }

        if (direccionInput != Vector3.zero)
        {
            // Para que el movimiento sea relativo a la rotaci�n horizontal actual de la c�mara (ignorando la inclinaci�n en X)
            Quaternion rotacionHorizontalCamara = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            Vector3 direccionMovimiento = rotacionHorizontalCamara * direccionInput.normalized;
            transform.Translate(direccionMovimiento * velocidadMovimiento * Time.deltaTime, Space.World);
        }

        // --- Rotaci�n con Q y E ---
        float inputRotacion = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            inputRotacion = -1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            inputRotacion = 1f;
        }

        if (inputRotacion != 0f)
        {
            rotacionYActual += inputRotacion * velocidadRotacion * Time.deltaTime;
            AplicarRotacionInclinacion(); // Reaplicar para mantener el �ngulo en X
        }

        // --- Elevaci�n con Espacio y Shift+Espacio ---
        float inputElevacion = 0f;
        if (Input.GetKey(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                inputElevacion = -1f; // Bajar con Shift + Espacio
            }
            else
            {
                inputElevacion = 1f; // Subir con Espacio
            }
        }

        if (inputElevacion != 0f)
        {
            Vector3 nuevaPosicion = transform.position + Vector3.up * inputElevacion * velocidadElevacion * Time.deltaTime;
            // Limitar la altura del zoom/elevaci�n
            nuevaPosicion.y = Mathf.Clamp(nuevaPosicion.y, alturaMinimaZoom, alturaMaximaZoom);
            transform.position = nuevaPosicion;
        }

        // --- Zoom con Rueda del Rat�n (Comportamiento de elevaci�n) ---
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {
            Vector3 nuevaPosicionZoom = transform.position - Vector3.up * scrollInput * velocidadZoom * 10f * Time.deltaTime; // Multiplicador para hacerlo m�s sensible
            // Limitar la altura del zoom
            nuevaPosicionZoom.y = Mathf.Clamp(nuevaPosicionZoom.y, alturaMinimaZoom, alturaMaximaZoom);
            transform.position = nuevaPosicionZoom;
        }
    }

    void AplicarRotacionInclinacion()
    {
        // Aplicar la rotaci�n Y acumulada y la inclinaci�n X fija. Z se mantiene en 0.
        transform.rotation = Quaternion.Euler(anguloInclinacionInicial, rotacionYActual, 0);
    }
}
