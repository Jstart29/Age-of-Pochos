using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InfoUnidadAGenerarIA
{
    public string nombreDescriptivo;
    public GameObject prefabUnidadIA;
    public int costoOro;
    public Transform puntoDeAparicionEspecifico;
}  

public class GeneradorUnidadesIA : MonoBehaviour
{
    [Header("Configuración General IA")]
    public int equipoIA = 2;
    public Transform puntoDeAparicionGeneralIA; 
    public bool generacionActiva = true;

    [Header("Unidades Generables por la IA")]
    public List<InfoUnidadAGenerarIA> listaUnidadesGenerables = new List<InfoUnidadAGenerarIA>();

    [Header("Economía y Tiempos de IA")]
    public float intervaloIntentoGeneracion = 15f;
    public int oroInicialIA = 500;
    public int oroPorIntervaloIA = 50;
    public float intervaloIngresoOroIA = 20f;

    private float proximoTiempoGeneracion;
    private float proximoTiempoIngresoOro;

    void Start()
    {
        if (puntoDeAparicionGeneralIA == null)
        {
            Debug.LogWarning($"GeneradorUnidadesIA ({gameObject.name}): El 'Punto De Aparicion General IA' no está asignado. Algunas unidades podrían no generarse si no tienen un punto específico.", this);
        }

        if (listaUnidadesGenerables.Count == 0)
        {
            Debug.LogWarning($"GeneradorUnidadesIA ({gameObject.name}): La lista de unidades generables está vacía.", this);
        }
        else
        {
            // Verificar si los puntos de aparición específicos están asignados para las unidades en la lista
            foreach (var unidadInfo in listaUnidadesGenerables)
            {
                if (unidadInfo.prefabUnidadIA != null && unidadInfo.puntoDeAparicionEspecifico == null)
                {
                    Debug.LogWarning($"GeneradorUnidadesIA ({gameObject.name}): La unidad '{unidadInfo.nombreDescriptivo}' no tiene un 'Punto De Aparicion Especifico' asignado. Usará el general si está disponible.", this);
                }
            }
        }
        
        if (AdministradorRecursos.Instancia == null)
        {
             Debug.LogError($"GeneradorUnidadesIA ({gameObject.name}): ¡AdministradorRecursos.Instancia no encontrado!", this);
             generacionActiva = false;
             return;
        }

        AdministradorRecursos.Instancia.EstablecerRecursosIniciales(equipoIA, TipoRecurso.Oro, oroInicialIA);
        // Debug.Log($"IA Equipo {equipoIA} inicia con {oroInicialIA} de Oro.");

        proximoTiempoGeneracion = Time.time + Random.Range(intervaloIntentoGeneracion * 0.5f, intervaloIntentoGeneracion * 1.5f); // Añadir algo de aleatoriedad al primer spawn
        proximoTiempoIngresoOro = Time.time + intervaloIngresoOroIA;
    }

    void Update()
    {
        if (!generacionActiva) return;

        if (Time.time >= proximoTiempoIngresoOro)
        {
            AdministradorRecursos.Instancia.AnadirRecursos(equipoIA, TipoRecurso.Oro, oroPorIntervaloIA);
            proximoTiempoIngresoOro = Time.time + intervaloIngresoOroIA;
        }

        if (Time.time >= proximoTiempoGeneracion)
        {
            IntentarGenerarUnidadDesdeLista(); // Renombrado para claridad
            proximoTiempoGeneracion = Time.time + intervaloIntentoGeneracion;
        }
    }

    void IntentarGenerarUnidadDesdeLista()
    {
        if (listaUnidadesGenerables.Count == 0) return;

        int indiceAleatorio = Random.Range(0, listaUnidadesGenerables.Count);
        InfoUnidadAGenerarIA unidadInfo = listaUnidadesGenerables[indiceAleatorio];

        if (unidadInfo.prefabUnidadIA == null)
        {
            // Debug.LogWarning($"GeneradorUnidadesIA: Prefab nulo en la lista de unidades generables en el índice {indiceAleatorio}.", this);
            return;
        }

        // Determinar el punto de aparición
        Transform puntoDeAparicionDesignado = unidadInfo.puntoDeAparicionEspecifico;
        if (puntoDeAparicionDesignado == null) // Si no hay uno específico para este tipo de unidad
        {
            puntoDeAparicionDesignado = this.puntoDeAparicionGeneralIA; // Usar el general como fallback
        }

        if (puntoDeAparicionDesignado == null) // Si ni el específico ni el general están asignados
        {
            Debug.LogError($"GeneradorUnidadesIA: No se pudo determinar un punto de aparición para '{unidadInfo.nombreDescriptivo}'. Ni específico ni general están asignados.", this);
            return;
        }
        
        // Debug.Log($"IA Equipo {equipoIA} intentando generar '{unidadInfo.nombreDescriptivo}' (Costo: {unidadInfo.costoOro} Oro). Oro actual: {AdministradorRecursos.Instancia.ObtenerCantidadRecurso(equipoIA, TipoRecurso.Oro)}");

        if (AdministradorRecursos.Instancia.GastarRecursos(equipoIA, TipoRecurso.Oro, unidadInfo.costoOro))
        {
            GameObject nuevaUnidadGO = Instantiate(unidadInfo.prefabUnidadIA, puntoDeAparicionDesignado.position, puntoDeAparicionDesignado.rotation);
            Unidad scriptUnidad = nuevaUnidadGO.GetComponent<Unidad>();
            if (scriptUnidad != null)
            {
                scriptUnidad.equipoID = equipoIA;
            }
            // Debug.Log($"IA (Equipo {equipoIA}) generó '{unidadInfo.nombreDescriptivo}' en {puntoDeAparicionDesignado.name}. Costo: {unidadInfo.costoOro} Oro.");
        }
    }
}