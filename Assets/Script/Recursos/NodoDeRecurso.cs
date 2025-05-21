using UnityEngine;

public class NodoDeRecurso : MonoBehaviour
{
    [Header("Configuración del Nodo")]
    public TipoRecurso tipoDeRecurso;
    public int cantidadInicial = 1000;
    [Tooltip("Cuántos recursos se extraen por cada ciclo de recolección de una unidad.")]
    public int cantidadPorExtraccion = 10;
    public bool esAgotable = true;

    [Header("Estado Actual")]
    [SerializeField]
    private int cantidadActual;
    public int CantidadActual => cantidadActual;

    [Header("Visuales (Opcional)")]
    public GameObject modeloVisualNormal;
    public GameObject modeloVisualAgotado;
    public float retrasoAntesDeDestruir = 0.5f; // Pequeño retraso para mostrar el estado agotado o efecto

    void Awake()
    {
        cantidadActual = cantidadInicial;
        ActualizarVisual();
        if (!Unidad.todosLosNodosDeRecurso.Contains(this))
        {
            Unidad.todosLosNodosDeRecurso.Add(this);
        }
    }

    void OnDestroy()
    {
        if (Unidad.todosLosNodosDeRecurso.Contains(this))
        {
            Unidad.todosLosNodosDeRecurso.Remove(this);
        }
    }

    public int TomarRecursos(int cantidadSolicitadaPorUnidad)
    {
        if (cantidadActual <= 0 && esAgotable)
        {
            // Si ya estaba agotado y se intenta tomar de nuevo (podría pasar si varias unidades llegan al mismo tiempo al último recurso)
            return 0;
        }

        int cantidadAExtraer;

        if (!esAgotable)
        {
            // Si el nodo no es agotable, siempre da la cantidad solicitada hasta su límite por extracción
            cantidadAExtraer = Mathf.Min(cantidadSolicitadaPorUnidad, cantidadPorExtraccion);
        }
        else // El nodo es agotable
        {
            cantidadAExtraer = Mathf.Min(cantidadSolicitadaPorUnidad, cantidadActual);
            cantidadAExtraer = Mathf.Min(cantidadAExtraer, cantidadPorExtraccion);

            cantidadActual -= cantidadAExtraer;
        }


        if (esAgotable && cantidadActual <= 0)
        {
            cantidadActual = 0; // Asegurar que no sea negativo
            Debug.Log($"Nodo de {tipoDeRecurso} en {gameObject.name} se ha agotado. Última extracción: {cantidadAExtraer}.");
            ActualizarVisual(); // Mostrar el modelo de "agotado" si existe

            Debug.Log($"Nodo {gameObject.name} programado para destrucción en {retrasoAntesDeDestruir}s.");
            Destroy(gameObject, retrasoAntesDeDestruir);
        }
        else if (esAgotable) // Actualizar visual si no se agotó pero cambió la cantidad (podrías tener etapas visuales)
        {
            // ActualizarVisual(); // Descomenta si tienes modelos intermedios
        }

        return cantidadAExtraer;
    }

    void ActualizarVisual()
    {
        if (modeloVisualNormal != null)
        {
            modeloVisualNormal.SetActive(cantidadActual > 0 || !esAgotable);
        }
        if (modeloVisualAgotado != null && esAgotable) // Solo mostrar agotado si es agotable
        {
            modeloVisualAgotado.SetActive(cantidadActual <= 0);
        }
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"{tipoDeRecurso}\n{(esAgotable ? cantidadActual.ToString() : "Infinito")}/{cantidadInicial}");
#endif
    }
}