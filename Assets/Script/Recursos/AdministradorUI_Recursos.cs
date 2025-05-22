using UnityEngine;
using TMPro;

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
            Debug.LogError("AdministradorRecursos.Instancia no encontrada. Asegúrate de que haya un AdministradorRecursos en la escena.");
            enabled = false; // Desactivar este script si no hay gestor de recursos
            return;
        }

        idEquipoJugador = Unidad.equipoDelJugador; // Obtener el ID del equipo del jugador

        // Suscribirse al evento de actualización de recursos
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
    void ActualizarTextoRecursoEspecifico(int equipoID, TipoRecurso tipo, int nuevaCantidad)
    {
        // Solo actualizar la UI si los recursos son del equipo del jugador actual
        if (equipoID != idEquipoJugador)
        {
            return;
        }

        // Debug.Log($"UI Recibió actualización: Equipo {equipoID}, Recurso {tipo}, Cantidad {nuevaCantidad}");

        switch (tipo)
        {
            case TipoRecurso.Oro:
                if (textoOro != null) textoOro.text = $"Oro: {nuevaCantidad}";
                break;
            default:
                break;
        }
    }

    public void ActualizarTodosLosTextos()
    {
        // Debug.Log($"UI Actualizando todos los textos para el equipo {idEquipoJugador}");
        if (AdministradorRecursos.Instancia == null) return;

        if (textoOro != null)
            textoOro.text = $"Oro: {AdministradorRecursos.Instancia.ObtenerCantidadRecurso(idEquipoJugador, TipoRecurso.Oro)}";

    }
}