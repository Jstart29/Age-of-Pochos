using System.Collections.Generic;
using UnityEngine;

public class GeneradorPrefabs : MonoBehaviour
{
    [Header("Configuración de Generación de Objetos")]
    // Ahora es una lista para que puedas asignar múltiples prefabs en el Inspector
    public List<GameObject> listaDePrefabsAGenerar = new List<GameObject>();
    public int cantidadMinimaAGenerar = 25;
    public int cantidadMaximaAGenerar = 35;

    [Header("Zona de Generación")]
    public BoxCollider zonaDelimitadora;

    void Start()
    {
        if (listaDePrefabsAGenerar == null || listaDePrefabsAGenerar.Count == 0)
        {
            Debug.LogError("¡La lista 'Lista De Prefabs A Generar' está vacía o no asignada en el Inspector! No se generarán objetos.", this);
            return;
        }
        // Verificar que ningún elemento de la lista sea null
        for (int i = 0; i < listaDePrefabsAGenerar.Count; i++)
        {
            if (listaDePrefabsAGenerar[i] == null)
            {
                Debug.LogError($"El elemento {i} en 'Lista De Prefabs A Generar' es nulo. Por favor, asigna un prefab válido.", this);
                return; // Detener si hay un prefab nulo en la lista
            }
        }


        if (zonaDelimitadora == null)
        {
            zonaDelimitadora = GetComponent<BoxCollider>();
            if (zonaDelimitadora == null)
            {
                Debug.LogError("¡ZonaDelimitadora (BoxCollider) no está asignada y no se encontró una en este GameObject! No se generarán objetos.", this);
                return;
            }
        }
        GenerarObjetos();
    }

    public void GenerarObjetos() // Renombrado para ser más genérico
    {
        if (cantidadMinimaAGenerar < 0) cantidadMinimaAGenerar = 0;
        if (cantidadMaximaAGenerar < cantidadMinimaAGenerar) cantidadMaximaAGenerar = cantidadMinimaAGenerar;

        int cantidadRealAGenerar = Random.Range(cantidadMinimaAGenerar, cantidadMaximaAGenerar + 1);

        Bounds limites = zonaDelimitadora.bounds;

        Debug.Log($"Iniciando generación de {cantidadRealAGenerar} objetos aleatorios de la lista proporcionada.");

        for (int i = 0; i < cantidadRealAGenerar; i++)
        {
            // --- SELECCIÓN ALEATORIA DEL PREFAB ---
            if (listaDePrefabsAGenerar.Count == 0) // Doble chequeo por si acaso, aunque ya se hizo en Start
            {
                Debug.LogWarning("La lista de prefabs está vacía, no se puede generar más.");
                break;
            }
            int indiceAleatorio = Random.Range(0, listaDePrefabsAGenerar.Count);
            GameObject prefabSeleccionado = listaDePrefabsAGenerar[indiceAleatorio];

            if (prefabSeleccionado == null) // Por si un elemento de la lista es nulo (aunque ya validamos en Start)
            {
                Debug.LogWarning($"El prefab en el índice {indiceAleatorio} de la lista es nulo. Saltando esta generación.");
                continue;
            }
            // --- FIN DE SELECCIÓN ALEATORIA ---

            float randomXPos = Random.Range(limites.min.x, limites.max.x);
            float randomZPos = Random.Range(limites.min.z, limites.max.z);
            float spawnY = 2.0f; 
            Vector3 posicionAleatoria = new Vector3(randomXPos, spawnY, randomZPos);

            float randomRotacionY = Random.Range(0f, 360f);
            Quaternion rotacionAleatoria = Quaternion.Euler(0f, randomRotacionY, 0f);

            GameObject nuevoObjeto = Instantiate(prefabSeleccionado, posicionAleatoria, rotacionAleatoria);
        }
        Debug.Log($"Generación de {cantidadRealAGenerar} objetos completada.");
    }

    void OnDrawGizmosSelected()
    {
        if (zonaDelimitadora != null)
        {
            Vector3 centroVisualizacionY = new Vector3(zonaDelimitadora.bounds.center.x, 2.0f, zonaDelimitadora.bounds.center.z);
            Vector3 tamanoVisualizacionY = new Vector3(zonaDelimitadora.bounds.size.x, 0.1f, zonaDelimitadora.bounds.size.z);
            Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.4f); // Un verde diferente para el generador multi-objeto
            Gizmos.DrawWireCube(centroVisualizacionY, tamanoVisualizacionY);
            Gizmos.DrawCube(centroVisualizacionY, tamanoVisualizacionY);
        }
    }
}