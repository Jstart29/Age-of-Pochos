// Estructura.cs (Modificado con Debug.LogError)
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
// using UnityEngine.AI; // Solo si es necesario

public class Estructura : Unidad
{
    [Header("UI de Vida de la Estructura")]
    public Slider sliderBarraDeVida;
    public Canvas canvasBarraDeVida;

    [Header("Condiciones de Fin de Juego")]
    [Tooltip("Marcar si la destrucción de esta estructura es crítica para ganar o perder.")]
    public bool esEstructuraClave = false;

    protected override void Awake()
    {
        // Debug.LogError($"--- ESTRUCTURA AWAKE: {gameObject.name} ---"); // Para verificar que Awake se llama
        vidaActual = vidaMaxima;
        if (indicadorSeleccion != null) indicadorSeleccion.SetActive(false);
        if (!Unidad.todasLasUnidadesActivas.Contains(this)) Unidad.todasLasUnidadesActivas.Add(this);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        ConfigurarSliderDeVida();
        ActualizarBarraDeVidaUI();

        if (canvasBarraDeVida != null && vidaActual >= vidaMaxima && vidaMaxima > 0)
        {
            // canvasBarraDeVida.gameObject.SetActive(false);
        }
    }

    void ConfigurarSliderDeVida()
    {
        if (sliderBarraDeVida != null)
        {
            sliderBarraDeVida.minValue = 0;
            sliderBarraDeVida.maxValue = vidaMaxima > 0 ? vidaMaxima : 1f;
            sliderBarraDeVida.value = vidaActual;
        }
    }

    public override void MoverA(Vector3 destino) { /* No se mueven */ }
    public override void Atacar(Unidad objetivo) { /* No atacan por defecto */ }
    public override void UsarHabilidadEspecial(int num, Unidad obj, Vector3 pos) { /* Sin habilidades */ }

    public override void RecibirDano(float cantidad)
    {
        // Debug.LogError($"[ESTRUCTURA RecibirDano] {gameObject.name} ANTES base.RecibirDano. Vida: {vidaActual}, Cantidad: {cantidad}");
        if (vidaActual <= 0 && vidaMaxima > 0) return;
        base.RecibirDano(cantidad);
        // Debug.LogError($"[ESTRUCTURA RecibirDano] {gameObject.name} DESPUÉS base.RecibirDano. Vida: {vidaActual}");
        ActualizarBarraDeVidaUI();
        if (canvasBarraDeVida != null && vidaActual < vidaMaxima && vidaActual > 0)
        {
            canvasBarraDeVida.gameObject.SetActive(true);
        }
    }

    protected void ActualizarBarraDeVidaUI()
    {
        if (sliderBarraDeVida != null)
        {
            if (sliderBarraDeVida.maxValue != vidaMaxima) sliderBarraDeVida.maxValue = vidaMaxima > 0 ? vidaMaxima : 1f;
            if (sliderBarraDeVida.minValue != 0) sliderBarraDeVida.minValue = 0;
            sliderBarraDeVida.value = vidaActual;
        }
    }

    protected override void Morir()
    {
        if (canvasBarraDeVida != null)
        {
            canvasBarraDeVida.gameObject.SetActive(false);
        }

        if (esEstructuraClave && !AdministradorDeEscenas.transicionEnProgreso)
        {
            Debug.Log($"[ESTRUCTURA MORIR] {gameObject.name} es estructura clave y no hay transición. Verificando equipo...");
            if (this.equipoID == Unidad.equipoDelJugador)
            {
                AdministradorDeEscenas.CargarEscenaDerrota();
            }
            else if (this.equipoID == 2) // Asumiendo que el enemigo principal es equipo 2
            {
                AdministradorDeEscenas.CargarEscenaVictoria();
            }
            else
            {
                Debug.LogWarning($"[ESTRUCTURA MORIR] {gameObject.name} es clave, pero su equipoID ({this.equipoID}) no coincide con jugador ({Unidad.equipoDelJugador}) ni enemigo principal (2).");
            }
        }
        else if (!esEstructuraClave)
        {
            Debug.Log($"[ESTRUCTURA MORIR] {gameObject.name} fue destruida, pero NO es estructura clave.");
        }
        else if (AdministradorDeEscenas.transicionEnProgreso)
        {
            Debug.LogWarning($"[ESTRUCTURA MORIR] {gameObject.name} es clave y fue destruida, pero ya hay una transición de escena en progreso. No se cargará otra escena.");
        }

        base.Morir(); // Esto llamará a Destroy(gameObject) en la clase Unidad
    }
}