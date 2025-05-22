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

    [Header("Puntos de Aparición Específicos (Jugador)")]
    public Transform puntoAparicionTrabajador;
    public Transform puntoAparicionCaballero;
    public Transform puntoAparicionMago;
    public Transform puntoAparicionTanque;
    // El 'puntoDeAparicion' general anterior ya no es necesario si cada unidad tiene el suyo.

    private int idEquipoJugador;

    void Start()
    {
        idEquipoJugador = Unidad.equipoDelJugador;

        // Validaciones iniciales para los puntos de aparición (opcional, pero recomendado)
        if (prefabTrabajador != null && puntoAparicionTrabajador == null)
            Debug.LogError($"MenuEntrenamiento: Punto de Aparición para Trabajador no asignado!", this);
        if (prefabCaballero != null && puntoAparicionCaballero == null)
            Debug.LogError($"MenuEntrenamiento: Punto de Aparición para Caballero no asignado!", this);
        if (prefabMago != null && puntoAparicionMago == null)
            Debug.LogError($"MenuEntrenamiento: Punto de Aparición para Mago no asignado!", this);
        if (prefabTanque != null && puntoAparicionTanque == null)
            Debug.LogError($"MenuEntrenamiento: Punto de Aparición para Tanque no asignado!", this);

        if (AdministradorRecursos.Instancia == null)
        {
            Debug.LogError("MenuEntrenamiento: ¡AdministradorRecursos.Instancia no encontrado! Asegúrate de que exista en la escena.", this);
            // Considera desactivar los botones o el script si esto es crítico
            // enabled = false; 
        }
    }

    // Método privado genérico para intentar crear una unidad, ahora usa un punto de aparición específico
    private void IntentarCrearUnidadEspecifica(GameObject prefabUnidad, int costoOro, string nombreUnidadLog, Transform puntoDeAparicionEspecifico)
    {
        if (prefabUnidad == null)
        {
            Debug.LogError($"MenuEntrenamiento: Prefab para '{nombreUnidadLog}' no asignado.", this);
            return;
        }
        if (puntoDeAparicionEspecifico == null)
        {
            Debug.LogError($"MenuEntrenamiento: ¡El Punto de Aparición para '{nombreUnidadLog}' no está configurado en el Inspector!", this);
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

            // Opcional: Darle una orden de moverse a un punto de reunión cercano al punto de aparición
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

    // --- Métodos Públicos para los Botones de la UI ---

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