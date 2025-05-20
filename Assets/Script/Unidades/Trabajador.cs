// Trabajador.cs
using UnityEngine;
using UnityEngine.AI; // Por si accedes a m�s detalles del NavMeshAgent
using System.Collections.Generic; // Por si se necesitan listas

public class Trabajador : Unidad
{
    [Header("Configuraci�n del Trabajador")]
    public float rangoDeteccionNodos = 20f; // Qu� tan lejos busca nodos autom�ticamente
    public float rangoInteraccionNodo = 2.5f; // Distancia para empezar a recolectar
    public float tiempoPorCicloRecoleccion = 2f; // Segundos por cada "tick" de recolecci�n
    public int cantidadPorCicloRecoleccion = 10; // Cu�nto intenta tomar del nodo por ciclo

    private NodoDeRecurso nodoObjetivoRecoleccion;
    private bool estaRecolectando = false; // Estado: �Est� en el nodo extrayendo?
    private float proximoTiempoCicloRecoleccion = 0f;

    // Para la b�squeda autom�tica de nodos cuando est� idle
    private float proximoTiempoBusquedaNodo = 0f;
    private float intervaloBusquedaNodo = 2.5f; // Buscar nodos cada 2.5 segundos si est� idle

    protected override void Awake()
    {
        base.Awake();
        // Stats espec�ficos del trabajador (ej. menos vida, sin ataque por defecto)
        vidaMaxima = 60f;
        vidaActual = vidaMaxima;
        velocidadMovimiento = 3.2f; // Quiz�s un poco m�s r�pidos que otras unidades base
        if (navMeshAgent != null) navMeshAgent.speed = velocidadMovimiento;
    }

    public override void Update()
    {
        base.Update(); // Llama al Update de Unidad (para indicador de selecci�n, etc.)

        if (estaRecolectando) // Fase 2: Est� en el nodo, recolectando
        {
            if (nodoObjetivoRecoleccion == null || nodoObjetivoRecoleccion.CantidadActual <= 0 && nodoObjetivoRecoleccion.esAgotable)
            {
                // Debug.Log($"{gameObject.name} (Trabajador): Nodo objetivo ({nodoObjetivoRecoleccion?.name}) agotado o inv�lido. Buscando nuevo nodo.");
                nodoObjetivoRecoleccion = null;
                estaRecolectando = false;
                // BuscarSiguienteNodoDeRecurso(); // Buscar inmediatamente otro nodo
                return; // Sale del Update para que en el siguiente frame re-eval�e (o busque)
            }

            // Mirar hacia el nodo mientras recolecta (opcional, est�tico)
            MirarHacia(nodoObjetivoRecoleccion.transform.position);

            if (Time.time >= proximoTiempoCicloRecoleccion)
            {
                int cantidadObtenida = nodoObjetivoRecoleccion.TomarRecursos(cantidadPorCicloRecoleccion);
                if (cantidadObtenida > 0)
                {
                    AdministradorRecursos.Instancia.AnadirRecursos(this.equipoID, nodoObjetivoRecoleccion.tipoDeRecurso, cantidadObtenida);
                    // Debug.Log($"{gameObject.name} (Trabajador) recolect� {cantidadObtenida} de {nodoObjetivoRecoleccion.tipoDeRecurso} del nodo {nodoObjetivoRecoleccion.name}.");
                    // Aqu� podr�as disparar una animaci�n de "golpe" o recolecci�n
                }
                else // Nodo podr�a haberse agotado justo en este ciclo por otro trabajador
                {
                    // Debug.Log($"{gameObject.name} (Trabajador): Nodo {nodoObjetivoRecoleccion.name} no dio recursos este ciclo (podr�a estar agotado).");
                    nodoObjetivoRecoleccion = null; // Considerar el nodo agotado para este trabajador
                    estaRecolectando = false;
                    // BuscarSiguienteNodoDeRecurso();
                    return;
                }
                proximoTiempoCicloRecoleccion = Time.time + tiempoPorCicloRecoleccion;
            }
        }
        else if (nodoObjetivoRecoleccion != null) // Fase 1: Tiene un nodo asignado, movi�ndose hacia �l
        {
            float distanciaAlNodo = Vector3.Distance(transform.position, nodoObjetivoRecoleccion.transform.position);
            if (distanciaAlNodo > rangoInteraccionNodo)
            {
                MoverA(nodoObjetivoRecoleccion.transform.position);
            }
            else // Lleg� al nodo
            {
                // Debug.Log($"{gameObject.name} (Trabajador) lleg� al nodo {nodoObjetivoRecoleccion.name}. Comenzando recolecci�n.");
                if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath(); // Detener movimiento
                estaRecolectando = true;
                proximoTiempoCicloRecoleccion = Time.time + tiempoPorCicloRecoleccion; // Iniciar primer ciclo de recolecci�n
                // Aqu� podr�as disparar una animaci�n de "empezar a recolectar"
            }
        }
        // Fase 0: Idle, no tiene nodo asignado, buscar uno autom�ticamente
        else if (navMeshAgent != null && (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f)) // Si est� idle
        {
            if (Time.time >= proximoTiempoBusquedaNodo)
            {
                // Debug.Log($"{gameObject.name} (Trabajador) est� idle, buscando nodo de recurso cercano...");
                BuscarSiguienteNodoDeRecurso();
                proximoTiempoBusquedaNodo = Time.time + intervaloBusquedaNodo;
            }
        }
    }

    protected void MirarHacia(Vector3 punto)
    {
        if (navMeshAgent == null) return;
        if (!navMeshAgent.updateRotation || navMeshAgent.velocity.sqrMagnitude < 0.01f)
        {
            Vector3 direccion = (punto - transform.position).normalized;
            if (direccion != Vector3.zero)
            {
                Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionDeseada, navMeshAgent.angularSpeed * Time.deltaTime);
            }
        }
    }

    // Sobrescribir el m�todo de la clase base para manejar el comando espec�fico
    public override void ComandoInteraccionNodoRecurso(NodoDeRecurso nodo)
    {
        if (nodo == null) return;

        // Debug.Log($"{gameObject.name} (Trabajador) recibi� comando de ir a recolectar al nodo {nodo.name} ({nodo.tipoDeRecurso}).");
        // Cancelar otras acciones (como atacar si la Unidad base tuviera esa l�gica por defecto)
        // Si tienes una variable como 'objetivoAtaqueActual' en Unidad, ponla a null aqu�.
        // Ejemplo: this.objetivoAtaqueActual = null; (Si 'objetivoAtaqueActual' fuera de Unidad y protected)

        nodoObjetivoRecoleccion = nodo;
        estaRecolectando = false; // Reiniciar estado, se mover� primero al nodo
        if (navMeshAgent != null) MoverA(nodo.transform.position); // MoverA ya est� en la clase Unidad
    }

    protected virtual void BuscarSiguienteNodoDeRecurso()
    {
        if (Unidad.todosLosNodosDeRecurso.Count == 0)
        {
            // Debug.Log($"{gameObject.name} (Trabajador): No hay nodos de recurso registrados en la escena.");
            return;
        }

        NodoDeRecurso nodoMasCercano = null;
        float menorDistanciaSqr = rangoDeteccionNodos * rangoDeteccionNodos; // Usar el cuadrado de la distancia

        foreach (NodoDeRecurso nodo in Unidad.todosLosNodosDeRecurso)
        {
            if (nodo != null && nodo.CantidadActual > 0) // Que el nodo exista y tenga recursos
            {
                float distanciaSqrAlNodo = (transform.position - nodo.transform.position).sqrMagnitude;
                if (distanciaSqrAlNodo < menorDistanciaSqr)
                {
                    menorDistanciaSqr = distanciaSqrAlNodo;
                    nodoMasCercano = nodo;
                }
            }
        }

        if (nodoMasCercano != null)
        {
            // Debug.Log($"{gameObject.name} (Trabajador) [AUTO-BUSQUEDA] encontr� nodo {nodoMasCercano.name} ({nodoMasCercano.tipoDeRecurso}). Dirigi�ndose.");
            ComandoInteraccionNodoRecurso(nodoMasCercano); // Enviar al nodo encontrado
        }
        // else { Debug.Log($"{gameObject.name} (Trabajador) [AUTO-BUSQUEDA] no encontr� nodos de recurso con recursos en rango de {rangoDeteccionNodos}."); }
    }

    // Los trabajadores usualmente no atacan, pero si lo hicieran, heredar�an Atacar() de Unidad.
    // Podr�as sobrescribirlo para que hagan poco o nada de da�o si son solo recolectores.
    public override void Atacar(Unidad objetivo)
    {
        // base.Atacar(objetivo); // Llama a la implementaci�n base (que loguea un warning)
        Debug.Log($"{gameObject.name} (Trabajador) no puede atacar.");
        // Opcional: Si un trabajador es atacado, podr�a huir o cambiar de tarea.
    }
}