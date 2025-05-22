using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Trabajador : Unidad
{
    [Header("Configuración del Trabajador")]
    public float rangoDeteccionNodos = 20f;
    public float rangoInteraccionNodo = 2.5f;
    public float tiempoPorCicloRecoleccion = 2f;
    public int cantidadPorCicloRecoleccion = 10;

    private NodoDeRecurso nodoObjetivoRecoleccion;
    private bool estaRecolectando = false;
    private float proximoTiempoCicloRecoleccion = 0f;

    private float proximoTiempoBusquedaNodo = 0f;
    private float intervaloBusquedaNodo = 2.5f;

    protected override void Awake()
    {
        base.Awake();
        vidaMaxima = 60f;
        vidaActual = vidaMaxima;
        velocidadMovimiento = 3.2f;
        if (navMeshAgent != null) navMeshAgent.speed = velocidadMovimiento;
    }

    public override void Update()
    {
        base.Update();

        if (estaRecolectando)
        {
            if (nodoObjetivoRecoleccion == null || (nodoObjetivoRecoleccion.CantidadActual <= 0 && nodoObjetivoRecoleccion.esAgotable))
            {
                nodoObjetivoRecoleccion = null;
                estaRecolectando = false;
                return;
            }
            MirarHacia(nodoObjetivoRecoleccion.transform.position);
            if (Time.time >= proximoTiempoCicloRecoleccion)
            {
                int cantidadObtenida = nodoObjetivoRecoleccion.TomarRecursos(cantidadPorCicloRecoleccion);
                if (cantidadObtenida > 0)
                {
                    AdministradorRecursos.Instancia.AnadirRecursos(this.equipoID, nodoObjetivoRecoleccion.tipoDeRecurso, cantidadObtenida);
                }
                else
                {
                    nodoObjetivoRecoleccion = null;
                    estaRecolectando = false;
                    return;
                }
                proximoTiempoCicloRecoleccion = Time.time + tiempoPorCicloRecoleccion;
            }
        }
        else if (nodoObjetivoRecoleccion != null)
        {
            float distanciaAlNodo = Vector3.Distance(transform.position, nodoObjetivoRecoleccion.transform.position);
            if (distanciaAlNodo > rangoInteraccionNodo)
            {
                MoverA(nodoObjetivoRecoleccion.transform.position);
            }
            else
            {
                if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();
                estaRecolectando = true;
                proximoTiempoCicloRecoleccion = Time.time + tiempoPorCicloRecoleccion;
            }
        }
        else if (navMeshAgent != null && (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f))
        {
            if (Time.time >= proximoTiempoBusquedaNodo)
            {
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

    public override void ComandoInteraccionNodoRecurso(NodoDeRecurso nodo)
    {
        if (nodo == null) return;
        nodoObjetivoRecoleccion = nodo;
        estaRecolectando = false;
        if (navMeshAgent != null) MoverA(nodo.transform.position);
    }

    protected virtual void BuscarSiguienteNodoDeRecurso()
    {
        if (Unidad.todosLosNodosDeRecurso.Count == 0) return;
        NodoDeRecurso nodoMasCercano = null;
        float menorDistanciaSqr = rangoDeteccionNodos * rangoDeteccionNodos;

        foreach (NodoDeRecurso nodo in Unidad.todosLosNodosDeRecurso)
        {
            if (nodo != null && nodo.CantidadActual > 0)
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
            ComandoInteraccionNodoRecurso(nodoMasCercano);
        }
    }

    public override void Atacar(Unidad objetivo)
    {
        // Los trabajadores no atacan
    }
}