// MenuEntrenamiento.cs
using UnityEngine;

public class MenuEntrenamiento : MonoBehaviour
{
    [Header("Prefabs de Unidades del Jugador")]
    public GameObject prefabTrabajador;
    public GameObject prefabCaballero;
    public GameObject prefabMago;
    public GameObject prefabTanque;

    [Header("Costos en Oro")]
    public int costoOroTrabajador = 25;
    public int costoOroCaballero = 50;
    public int costoOroMago = 75;
    public int costoOroTanque = 100;

    [Header("Puntos de Aparici�n Espec�ficos (Jugador)")]
    public Transform puntoAparicionTrabajador;
    public Transform puntoAparicionCaballero;
    public Transform puntoAparicionMago;
    public Transform puntoAparicionTanque;
    // El 'puntoDeAparicion' general anterior ya no es necesario si cada unidad tiene el suyo.

    private int idEquipoJugador;

    void Start()
    {
        idEquipoJugador = Unidad.equipoDelJugador;

        // Validaciones iniciales para los puntos de aparici�n (opcional, pero recomendado)
        if (prefabTrabajador != null && puntoAparicionTrabajador == null)
            Debug.LogError($"MenuEntrenamiento: Punto de Aparici�n para Trabajador no asignado!", this);
        if (prefabCaballero != null && puntoAparicionCaballero == null)
            Debug.LogError($"MenuEntrenamiento: Punto de Aparici�n para Caballero no asignado!", this);
        if (prefabMago != null && puntoAparicionMago == null)
            Debug.LogError($"MenuEntrenamiento: Punto de Aparici�n para Mago no asignado!", this);
        if (prefabTanque != null && puntoAparicionTanque == null)
            Debug.LogError($"MenuEntrenamiento: Punto de Aparici�n para Tanque no asignado!", this);

        if (AdministradorRecursos.Instancia == null)
        {
            Debug.LogError("MenuEntrenamiento: �AdministradorRecursos.Instancia no encontrado! Aseg�rate de que exista en la escena.", this);
            // Considera desactivar los botones o el script si esto es cr�tico
            // enabled = false; 
        }
    }

    // M�todo privado gen�rico para intentar crear una unidad, ahora usa un punto de aparici�n espec�fico
    private void IntentarCrearUnidadEspecifica(GameObject prefabUnidad, int costoOro, string nombreUnidadLog, Transform puntoDeAparicionEspecifico)
    {
        if (prefabUnidad == null)
        {
            Debug.LogError($"MenuEntrenamiento: Prefab para '{nombreUnidadLog}' no asignado.", this);
            return;
        }
        if (puntoDeAparicionEspecifico == null)
        {
            Debug.LogError($"MenuEntrenamiento: �El Punto de Aparici�n para '{nombreUnidadLog}' no est� configurado en el Inspector!", this);
            return;
        }
        if (AdministradorRecursos.Instancia == null) // Chequeo por si acaso
        {
            Debug.LogError("MenuEntrenamiento: AdministradorRecursos no disponible.", this);
            return;
        }


        if (AdministradorRecursos.Instancia.GastarRecursos(idEquipoJugador, TipoRecurso.Oro, costoOro))
        {
            GameObject nuevaUnidadGO = Instantiate(prefabUnidad, puntoDeAparicionEspecifico.position, puntoDeAparicionEspecifico.rotation);
            Unidad scriptUnidad = nuevaUnidadGO.GetComponent<Unidad>();
            if (scriptUnidad != null)
            {
                scriptUnidad.equipoID = idEquipoJugador;
            }
            // Debug.Log($"Unidad '{nombreUnidadLog}' creada para el equipo {idEquipoJugador} en {puntoDeAparicionEspecifico.name}. Costo: {costoOro} Oro.");

            // Opcional: Darle una orden de moverse a un punto de reuni�n cercano al punto de aparici�n
            // if(scriptUnidad != null && puntoDeAparicionEspecifico != null) 
            // {
            //     scriptUnidad.MoverA(puntoDeAparicionEspecifico.position + puntoDeAparicionEspecifico.forward * 5f);
            // }
        }
        else
        {
            // Debug.Log($"No hay suficiente Oro para crear '{nombreUnidadLog}'. Se requieren: {costoOro}.");
        }
    }

    // --- M�todos P�blicos para los Botones de la UI ---

    public void CrearTrabajador()
    {
        IntentarCrearUnidadEspecifica(prefabTrabajador, costoOroTrabajador, "Trabajador", puntoAparicionTrabajador);
    }

    public void CrearCaballero()
    {
        IntentarCrearUnidadEspecifica(prefabCaballero, costoOroCaballero, "Caballero", puntoAparicionCaballero);
    }

    public void CrearMago()
    {
        IntentarCrearUnidadEspecifica(prefabMago, costoOroMago, "Mago", puntoAparicionMago);
    }

    public void CrearTanque()
    {
        IntentarCrearUnidadEspecifica(prefabTanque, costoOroTanque, "Tanque", puntoAparicionTanque);
    }
}