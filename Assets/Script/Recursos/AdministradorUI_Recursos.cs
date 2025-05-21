using UnityEngine;
using TMPro; // O using UnityEngine.UI; si usas Legacy Text

public class AdministradorUI_Recursos : MonoBehaviour
{
    [Header("Referencias a los Textos de la UI")]
    // Cambia 'Text' por 'TextMeshProUGUI' si usas TextMeshPro
    public TextMeshProUGUI textoOro;

    private int idEquipoJugador;

    void Start()
    {
        // Asegurarse de que la instancia del AdministradorRecursos exista
        if (AdministradorRecursos.Instancia == null)
        {
            Debug.LogError("AdministradorRecursos.Instancia no encontrada. Aseg�rate de que haya un AdministradorRecursos en la escena.");
            enabled = false; // Desactivar este script si no hay gestor de recursos
            return;
        }

        idEquipoJugador = Unidad.equipoDelJugador; // Obtener el ID del equipo del jugador

        // Suscribirse al evento de actualizaci�n de recursos
        AdministradorRecursos.Instancia.OnRecursosActualizados += ActualizarTextoRecursoEspecifico;

        // Actualizar todos los textos de recursos al inicio con los valores actuales
        ActualizarTodosLosTextos();
    }

    void OnDestroy()
    {
        // Desuscribirse del evento para evitar errores si este objeto se destruye antes que AdministradorRecursos
        if (AdministradorRecursos.Instancia != null)
        {
            AdministradorRecursos.Instancia.OnRecursosActualizados -= ActualizarTextoRecursoEspecifico;
        }
    }

    // Este m�todo se llama cuando el evento OnRecursosActualizados se dispara
    void ActualizarTextoRecursoEspecifico(int equipoID, TipoRecurso tipo, int nuevaCantidad)
    {
        // Solo actualizar la UI si los recursos son del equipo del jugador actual
        if (equipoID != idEquipoJugador)
        {
            return;
        }

        // Debug.Log($"UI Recibi� actualizaci�n: Equipo {equipoID}, Recurso {tipo}, Cantidad {nuevaCantidad}");

        switch (tipo)
        {
            case TipoRecurso.Oro:
                if (textoOro != null) textoOro.text = $"Oro: {nuevaCantidad}";
                break;
            default:
                // Debug.LogWarning($"Tipo de recurso no manejado por la UI: {tipo}");
                break;
        }
    }

    // M�todo para forzar la actualizaci�n de todos los textos de recursos
    // �til para la inicializaci�n o si necesitas refrescar toda la UI de recursos
    public void ActualizarTodosLosTextos()
    {
        // Debug.Log($"UI Actualizando todos los textos para el equipo {idEquipoJugador}");
        if (AdministradorRecursos.Instancia == null) return;

        if (textoOro != null)
            textoOro.text = $"Oro: {AdministradorRecursos.Instancia.ObtenerCantidadRecurso(idEquipoJugador, TipoRecurso.Oro)}";

    }
}