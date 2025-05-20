// NodoDeRecurso.cs (Modificado)
using UnityEngine;

public class NodoDeRecurso : MonoBehaviour
{
    [Header("Configuración del Nodo")]
    public TipoRecurso tipoDeRecurso;
    public int cantidadInicial = 1000;
    [Tooltip("Cuántos recursos se extraen por cada ciclo de recolección de una unidad.")]
    public int cantidadPorExtraccion = 10; // Lo máximo que este nodo da por ciclo
    public bool esAgotable = true;

    [Header("Estado Actual")]
    [SerializeField]
    private int cantidadActual;
    public int CantidadActual => cantidadActual;

    public GameObject modeloVisualNormal;
    public GameObject modeloVisualAgotado;

    void Awake()
    {
        cantidadActual = cantidadInicial;
        ActualizarVisual();
        // Registrar este nodo en la lista global de Unidad
        if (!Unidad.todosLosNodosDeRecurso.Contains(this))
        {
            Unidad.todosLosNodosDeRecurso.Add(this);
        }
    }

    void OnDestroy()
    {
        // Quitar este nodo de la lista global al ser destruido
        if (Unidad.todosLosNodosDeRecurso.Contains(this))
        {
            Unidad.todosLosNodosDeRecurso.Remove(this);
        }
    }

    public int TomarRecursos(int cantidadSolicitadaPorUnidad)
    {
        if (!esAgotable && cantidadActual <= 0) // Nodo infinito teórico
        {
            return Mathf.Min(cantidadSolicitadaPorUnidad, cantidadPorExtraccion);
        }

        if (cantidadActual <= 0 && esAgotable)
        {
            return 0;
        }

        int cantidadAExtraer = Mathf.Min(cantidadSolicitadaPorUnidad, cantidadActual);
        cantidadAExtraer = Mathf.Min(cantidadAExtraer, cantidadPorExtraccion); // El nodo no da más de su 'cantidadPorExtraccion' por ciclo

        if (esAgotable)
        {
            cantidadActual -= cantidadAExtraer;
        }

        if (cantidadActual <= 0 && esAgotable)
        {
            cantidadActual = 0;
            Debug.Log($"Nodo de {tipoDeRecurso} en {gameObject.name} se ha agotado.");
            ActualizarVisual();
            // Considera desactivar el collider para que no sea más un objetivo
            // GetComponent<Collider>().enabled = false; 
        }

        return cantidadAExtraer;
    }

    void ActualizarVisual()
    {
        if (modeloVisualNormal != null) modeloVisualNormal.SetActive(cantidadActual > 0 || !esAgotable);
        if (modeloVisualAgotado != null) modeloVisualAgotado.SetActive(cantidadActual <= 0 && esAgotable);
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"{tipoDeRecurso}\n{(esAgotable ? cantidadActual.ToString() : "Infinito")}/{cantidadInicial}");
#endif
    }
}