// MenuEntrenamiento.cs
using UnityEngine;
// Podrías necesitar using UnityEngine.UI; si quieres interactuar con elementos de UI directamente desde aquí,
// pero para los OnClick de los botones, no es estrictamente necesario.

public class MenuEntrenamiento : MonoBehaviour
{
    [Header("Prefabs de Unidades")]
    public GameObject prefabTrabajador;
    public GameObject prefabCaballero;
    public GameObject prefabMago;
    public GameObject prefabTanque;

    [Header("Costos en Oro")]
    public int costoOroTrabajador = 25;
    public int costoOroCaballero = 50;
    public int costoOroMago = 75;
    public int costoOroTanque = 100;
    // Podrías expandir esto a otros tipos de recursos si es necesario

    [Header("Configuración de Aparición")]
    public Transform puntoDeAparicion; // Asigna un Empty Object en tu escena aquí

    private int idEquipoJugador;

    void Start()
    {
        idEquipoJugador = Unidad.equipoDelJugador; // Asumimos que este menú es para el jugador principal

        if (puntoDeAparicion == null)
        {
            Debug.LogError("MenuEntrenamiento: ¡El Punto de Aparición no está asignado en el Inspector!", this);
            // Considera desactivar el script o los botones si no hay punto de aparición
            enabled = false;
        }
        if (AdministradorRecursos.Instancia == null)
        {
            Debug.LogError("MenuEntrenamiento: ¡AdministradorRecursos.Instancia no encontrado! Asegúrate de que exista en la escena.", this);
            enabled = false;
        }
    }

    // Método privado genérico para intentar crear una unidad
    private void IntentarCrearUnidad(GameObject prefabUnidad, int costoOro, string nombreUnidadLog)
    {
        if (prefabUnidad == null)
        {
            Debug.LogError($"MenuEntrenamiento: Prefab para '{nombreUnidadLog}' no asignado.", this);
            return;
        }
        if (puntoDeAparicion == null) // Doble chequeo
        {
            Debug.LogError("MenuEntrenamiento: ¡El Punto de Aparición no está configurado!", this);
            return;
        }

        // Verificar y gastar recursos
        if (AdministradorRecursos.Instancia.GastarRecursos(idEquipoJugador, TipoRecurso.Oro, costoOro))
        {
            // Si los recursos se gastaron con éxito, instanciar la unidad
            GameObject nuevaUnidadGO = Instantiate(prefabUnidad, puntoDeAparicion.position, puntoDeAparicion.rotation);
            Unidad scriptUnidad = nuevaUnidadGO.GetComponent<Unidad>();
            if (scriptUnidad != null)
            {
                scriptUnidad.equipoID = idEquipoJugador; // Asegurar que la unidad pertenezca al jugador
            }
            Debug.Log($"Unidad '{nombreUnidadLog}' creada para el equipo {idEquipoJugador}. Costo: {costoOro} Oro.");

            // Opcional: Darle una orden de moverse a un punto de reunión cercano
            // if(scriptUnidad != null) scriptUnidad.MoverA(puntoDeAparicion.position + puntoDeAparicion.forward * 5f); // Mover 5 unidades hacia adelante del punto de aparición
        }
        else
        {
            Debug.Log($"No hay suficiente Oro para crear '{nombreUnidadLog}'. Se requieren: {costoOro}.");
            // Aquí podrías mostrar un mensaje al jugador en la UI o reproducir un sonido de error
        }
    }

    // --- Métodos Públicos para los Botones de la UI ---

    public void CrearTrabajador()
    {
        IntentarCrearUnidad(prefabTrabajador, costoOroTrabajador, "Trabajador");
    }

    public void CrearCaballero()
    {
        IntentarCrearUnidad(prefabCaballero, costoOroCaballero, "Caballero");
    }

    public void CrearMago()
    {
        IntentarCrearUnidad(prefabMago, costoOroMago, "Mago");
    }

    public void CrearTanque()
    {
        IntentarCrearUnidad(prefabTanque, costoOroTanque, "Tanque");
    }
}