// GeneradorUnidadesIA.cs
using UnityEngine;
using System.Collections.Generic; // Para usar Listas

// Peque�a clase para organizar la informaci�n de las unidades que la IA puede generar
[System.Serializable] // Para que aparezca en el Inspector
public class InfoUnidadAGenerarIA
{
    public string nombreDescriptivo; // Ej. "Guerrero IA", "Mago IA"
    public GameObject prefabUnidadIA;
    public int costoOro;
    // Podr�as a�adir m�s costos aqu� si usas otros recursos
}

public class GeneradorUnidadesIA : MonoBehaviour
{
    [Header("Configuraci�n General IA")]
    public int equipoIA = 2; // El ID del equipo para esta IA
    public Transform puntoDeAparicionIA; // D�nde aparecer�n las unidades de la IA
    public bool generacionActiva = true; // Para activar/desactivar la generaci�n

    [Header("Unidades Generables por la IA")]
    public List<InfoUnidadAGenerarIA> listaUnidadesGenerables = new List<InfoUnidadAGenerarIA>();

    [Header("Econom�a y Tiempos de IA")]
    public float intervaloIntentoGeneracion = 15f; // Cada cu�ntos segundos intenta generar una unidad
    public int oroInicialIA = 500;
    public int oroPorIntervaloIA = 50; // Cu�nto oro "gana" la IA cada cierto tiempo
    public float intervaloIngresoOroIA = 20f; // Cada cu�ntos segundos recibe el oroPorIntervaloIA

    private float proximoTiempoGeneracion;
    private float proximoTiempoIngresoOro;

    void Start()
    {
        if (puntoDeAparicionIA == null)
        {
            Debug.LogError($"GeneradorUnidadesIA ({gameObject.name}): �Punto de Aparici�n no asignado! La generaci�n se desactivar�.", this);
            generacionActiva = false;
            return;
        }

        if (listaUnidadesGenerables.Count == 0)
        {
            Debug.LogWarning($"GeneradorUnidadesIA ({gameObject.name}): La lista de unidades generables est� vac�a.", this);
            // generacionActiva = false; // Podr�as desactivarlo si no hay nada que generar
        }

        if (AdministradorRecursos.Instancia == null)
        {
            Debug.LogError($"GeneradorUnidadesIA ({gameObject.name}): �AdministradorRecursos.Instancia no encontrado!", this);
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

        // Otorgar recursos a la IA peri�dicamente
        if (Time.time >= proximoTiempoIngresoOro)
        {
            AdministradorRecursos.Instancia.AnadirRecursos(equipoIA, TipoRecurso.Oro, oroPorIntervaloIA);
            // Debug.Log($"IA Equipo {equipoIA} recibi� {oroPorIntervaloIA} de Oro. Total: {AdministradorRecursos.Instancia.ObtenerCantidadRecurso(equipoIA, TipoRecurso.Oro)}");
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
            Debug.LogWarning($"GeneradorUnidadesIA: Prefab nulo en la lista de unidades generables en el �ndice {indiceAleatorio}.", this);
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
                scriptUnidad.equipoID = equipoIA; // �Importante asignar el equipo correcto a la unidad de la IA!
            }
            Debug.Log($"IA (Equipo {equipoIA}) gener� '{unidadInfo.nombreDescriptivo}'. Costo: {unidadInfo.costoOro} Oro.");

            // Opcional: Darle una orden inicial a la unidad IA
            // if(scriptUnidad != null)
            // {
            //    // Ejemplo: Moverla a un punto de reuni�n o buscar enemigos
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