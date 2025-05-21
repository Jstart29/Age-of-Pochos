// GeneradorUnidadesIA.cs
using UnityEngine;
using System.Collections.Generic; // Para usar Listas

// Pequeña clase para organizar la información de las unidades que la IA puede generar
[System.Serializable] // Para que aparezca en el Inspector
public class InfoUnidadAGenerarIA
{
    public string nombreDescriptivo; // Ej. "Guerrero IA", "Mago IA"
    public GameObject prefabUnidadIA;
    public int costoOro;
    // Podrías añadir más costos aquí si usas otros recursos
}

public class GeneradorUnidadesIA : MonoBehaviour
{
    [Header("Configuración General IA")]
    public int equipoIA = 2; // El ID del equipo para esta IA
    public Transform puntoDeAparicionIA; // Dónde aparecerán las unidades de la IA
    public bool generacionActiva = true; // Para activar/desactivar la generación

    [Header("Unidades Generables por la IA")]
    public List<InfoUnidadAGenerarIA> listaUnidadesGenerables = new List<InfoUnidadAGenerarIA>();

    [Header("Economía y Tiempos de IA")]
    public float intervaloIntentoGeneracion = 15f; // Cada cuántos segundos intenta generar una unidad
    public int oroInicialIA = 500;
    public int oroPorIntervaloIA = 50; // Cuánto oro "gana" la IA cada cierto tiempo
    public float intervaloIngresoOroIA = 20f; // Cada cuántos segundos recibe el oroPorIntervaloIA

    private float proximoTiempoGeneracion;
    private float proximoTiempoIngresoOro;

    void Start()
    {
        if (puntoDeAparicionIA == null)
        {
            Debug.LogError($"GeneradorUnidadesIA ({gameObject.name}): ¡Punto de Aparición no asignado! La generación se desactivará.", this);
            generacionActiva = false;
            return;
        }

        if (listaUnidadesGenerables.Count == 0)
        {
            Debug.LogWarning($"GeneradorUnidadesIA ({gameObject.name}): La lista de unidades generables está vacía.", this);
            // generacionActiva = false; // Podrías desactivarlo si no hay nada que generar
        }

        if (AdministradorRecursos.Instancia == null)
        {
            Debug.LogError($"GeneradorUnidadesIA ({gameObject.name}): ¡AdministradorRecursos.Instancia no encontrado!", this);
            generacionActiva = false;
            return;
        }

        // Dar recursos iniciales a la IA
        AdministradorRecursos.Instancia.EstablecerRecursosIniciales(equipoIA, TipoRecurso.Oro, oroInicialIA);
        Debug.Log($"IA Equipo {equipoIA} inicia con {oroInicialIA} de Oro.");

        proximoTiempoGeneracion = Time.time + intervaloIntentoGeneracion;
        proximoTiempoIngresoOro = Time.time + intervaloIngresoOroIA;
    }

    void Update()
    {
        if (!generacionActiva) return;

        // Otorgar recursos a la IA periódicamente
        if (Time.time >= proximoTiempoIngresoOro)
        {
            AdministradorRecursos.Instancia.AnadirRecursos(equipoIA, TipoRecurso.Oro, oroPorIntervaloIA);
            // Debug.Log($"IA Equipo {equipoIA} recibió {oroPorIntervaloIA} de Oro. Total: {AdministradorRecursos.Instancia.ObtenerCantidadRecurso(equipoIA, TipoRecurso.Oro)}");
            proximoTiempoIngresoOro = Time.time + intervaloIngresoOroIA;
        }

        // Intentar generar una unidad si es el momento
        if (Time.time >= proximoTiempoGeneracion)
        {
            IntentarGenerarUnidadAleatoria();
            proximoTiempoGeneracion = Time.time + intervaloIntentoGeneracion;
        }
    }

    void IntentarGenerarUnidadAleatoria()
    {
        if (listaUnidadesGenerables.Count == 0) return;

        // Elegir una unidad aleatoria de la lista para intentar generar
        int indiceAleatorio = Random.Range(0, listaUnidadesGenerables.Count);
        InfoUnidadAGenerarIA unidadInfo = listaUnidadesGenerables[indiceAleatorio];

        if (unidadInfo.prefabUnidadIA == null)
        {
            Debug.LogWarning($"GeneradorUnidadesIA: Prefab nulo en la lista de unidades generables en el índice {indiceAleatorio}.", this);
            return;
        }

        // Debug.Log($"IA Equipo {equipoIA} intentando generar '{unidadInfo.nombreDescriptivo}' (Costo: {unidadInfo.costoOro} Oro). Oro actual: {AdministradorRecursos.Instancia.ObtenerCantidadRecurso(equipoIA, TipoRecurso.Oro)}");

        // Verificar y gastar recursos
        if (AdministradorRecursos.Instancia.GastarRecursos(equipoIA, TipoRecurso.Oro, unidadInfo.costoOro))
        {
            GameObject nuevaUnidadGO = Instantiate(unidadInfo.prefabUnidadIA, puntoDeAparicionIA.position, puntoDeAparicionIA.rotation);
            Unidad scriptUnidad = nuevaUnidadGO.GetComponent<Unidad>();
            if (scriptUnidad != null)
            {
                scriptUnidad.equipoID = equipoIA; // ¡Importante asignar el equipo correcto a la unidad de la IA!
            }
            Debug.Log($"IA (Equipo {equipoIA}) generó '{unidadInfo.nombreDescriptivo}'. Costo: {unidadInfo.costoOro} Oro.");

            // Opcional: Darle una orden inicial a la unidad IA
            // if(scriptUnidad != null)
            // {
            //    // Ejemplo: Moverla a un punto de reunión o buscar enemigos
            //    Vector3 puntoReunion = puntoDeAparicionIA.position + Random.insideUnitSphere * 5f;
            //    puntoReunion.y = puntoDeAparicionIA.position.y; // Mantener en el suelo
            //    scriptUnidad.MoverA(puntoReunion);
            // }
        }
        // else
        // {
        //     Debug.Log($"IA (Equipo {equipoIA}) no tiene suficiente Oro para generar '{unidadInfo.nombreDescriptivo}'. Necesita: {unidadInfo.costoOro}.");
        // }
    }
}