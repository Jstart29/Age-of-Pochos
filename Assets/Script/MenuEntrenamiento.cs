// MenuEntrenamiento.cs
using UnityEngine;
// Podr�as necesitar using UnityEngine.UI; si quieres interactuar con elementos de UI directamente desde aqu�,
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
    // Podr�as expandir esto a otros tipos de recursos si es necesario

    [Header("Configuraci�n de Aparici�n")]
    public Transform puntoDeAparicion; // Asigna un Empty Object en tu escena aqu�

    private int idEquipoJugador;

    void Start()
    {
        idEquipoJugador = Unidad.equipoDelJugador; // Asumimos que este men� es para el jugador principal

        if (puntoDeAparicion == null)
        {
            Debug.LogError("MenuEntrenamiento: �El Punto de Aparici�n no est� asignado en el Inspector!", this);
            // Considera desactivar el script o los botones si no hay punto de aparici�n
            enabled = false;
        }
        if (AdministradorRecursos.Instancia == null)
        {
            Debug.LogError("MenuEntrenamiento: �AdministradorRecursos.Instancia no encontrado! Aseg�rate de que exista en la escena.", this);
            enabled = false;
        }
    }

    // M�todo privado gen�rico para intentar crear una unidad
    private void IntentarCrearUnidad(GameObject prefabUnidad, int costoOro, string nombreUnidadLog)
    {
        if (prefabUnidad == null)
        {
            Debug.LogError($"MenuEntrenamiento: Prefab para '{nombreUnidadLog}' no asignado.", this);
            return;
        }
        if (puntoDeAparicion == null) // Doble chequeo
        {
            Debug.LogError("MenuEntrenamiento: �El Punto de Aparici�n no est� configurado!", this);
            return;
        }

        // Verificar y gastar recursos
        if (AdministradorRecursos.Instancia.GastarRecursos(idEquipoJugador, TipoRecurso.Oro, costoOro))
        {
            // Si los recursos se gastaron con �xito, instanciar la unidad
            GameObject nuevaUnidadGO = Instantiate(prefabUnidad, puntoDeAparicion.position, puntoDeAparicion.rotation);
            Unidad scriptUnidad = nuevaUnidadGO.GetComponent<Unidad>();
            if (scriptUnidad != null)
            {
                scriptUnidad.equipoID = idEquipoJugador; // Asegurar que la unidad pertenezca al jugador
            }
            Debug.Log($"Unidad '{nombreUnidadLog}' creada para el equipo {idEquipoJugador}. Costo: {costoOro} Oro.");

            // Opcional: Darle una orden de moverse a un punto de reuni�n cercano
            // if(scriptUnidad != null) scriptUnidad.MoverA(puntoDeAparicion.position + puntoDeAparicion.forward * 5f); // Mover 5 unidades hacia adelante del punto de aparici�n
        }
        else
        {
            Debug.Log($"No hay suficiente Oro para crear '{nombreUnidadLog}'. Se requieren: {costoOro}.");
            // Aqu� podr�as mostrar un mensaje al jugador en la UI o reproducir un sonido de error
        }
    }

    // --- M�todos P�blicos para los Botones de la UI ---

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