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
    [Header("Configuraci�n General IA")]
    public int equipoIA = 2;
    public Transform puntoDeAparicionGeneralIA; 
    public bool generacionActiva = true;

    [Header("Unidades Generables por la IA")]
    public List<InfoUnidadAGenerarIA> listaUnidadesGenerables = new List<InfoUnidadAGenerarIA>();

    [Header("Econom�a y Tiempos de IA")]
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
            Debug.LogWarning($"GeneradorUnidadesIA ({gameObject.name}): El 'Punto De Aparicion General IA' no est� asignado. Algunas unidades podr�an no generarse si no tienen un punto espec�fico.", this);
        }

        if (listaUnidadesGenerables.Count == 0)
        {
            Debug.LogWarning($"GeneradorUnidadesIA ({gameObject.name}): La lista de unidades generables est� vac�a.", this);
        }
        else
        {
            // Verificar si los puntos de aparici�n espec�ficos est�n asignados para las unidades en la lista
            foreach (var unidadInfo in listaUnidadesGenerables)
            {
                if (unidadInfo.prefabUnidadIA != null && unidadInfo.puntoDeAparicionEspecifico == null)
                {
                    Debug.LogWarning($"GeneradorUnidadesIA ({gameObject.name}): La unidad '{unidadInfo.nombreDescriptivo}' no tiene un 'Punto De Aparicion Especifico' asignado. Usar� el general si est� disponible.", this);
                }
            }
        }
        
        if (AdministradorRecursos.Instancia == null)
        {
             Debug.LogError($"GeneradorUnidadesIA ({gameObject.name}): �AdministradorRecursos.Instancia no encontrado!", this);
             generacionActiva = false;
             return;
        }

        AdministradorRecursos.Instancia.EstablecerRecursosIniciales(equipoIA, TipoRecurso.Oro, oroInicialIA);
        // Debug.Log($"IA Equipo {equipoIA} inicia con {oroInicialIA} de Oro.");

        proximoTiempoGeneracion = Time.time + Random.Range(intervaloIntentoGeneracion * 0.5f, intervaloIntentoGeneracion * 1.5f); // A�adir algo de aleatoriedad al primer spawn
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
            // Debug.LogWarning($"GeneradorUnidadesIA: Prefab nulo en la lista de unidades generables en el �ndice {indiceAleatorio}.", this);
            return;
        }

        // Determinar el punto de aparici�n
        Transform puntoDeAparicionDesignado = unidadInfo.puntoDeAparicionEspecifico;
        if (puntoDeAparicionDesignado == null) // Si no hay uno espec�fico para este tipo de unidad
        {
            puntoDeAparicionDesignado = this.puntoDeAparicionGeneralIA; // Usar el general como fallback
        }

        if (puntoDeAparicionDesignado == null) // Si ni el espec�fico ni el general est�n asignados
        {
            Debug.LogError($"GeneradorUnidadesIA: No se pudo determinar un punto de aparici�n para '{unidadInfo.nombreDescriptivo}'. Ni espec�fico ni general est�n asignados.", this);
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
            // Debug.Log($"IA (Equipo {equipoIA}) gener� '{unidadInfo.nombreDescriptivo}' en {puntoDeAparicionDesignado.name}. Costo: {unidadInfo.costoOro} Oro.");
        }
    }
}