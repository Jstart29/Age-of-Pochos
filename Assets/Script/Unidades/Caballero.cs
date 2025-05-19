// Caballero.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Caballero : Unidad
{
    public float armadura = 5f;
    public float danoAtaqueCaballero = 15f;
    public float rangoAtaqueCaballero = 2.5f;
    public float cooldownAtaque = 1.5f;
    private float proximoTiempoAtaque = 0f;

    public float velocidadCarga = 8f;
    public float duracionBoostVelocidadCarga = 1f;
    public float danoCarga = 25f;
    public float cooldownCarga = 10f;
    private float proximoTiempoCarga = 0f;
    private bool estaCargando = false;

    private Unidad objetivoAtaqueActual;

    [Header("Comportamiento de Búsqueda de Objetivo")]
    public bool puedeAgredirAutomaticamente = true;
    public float maxDistanciaEnganche = 50f;
    private float proximoTiempoBusquedaEnemigo = 0f;
    private float intervaloBusquedaEnemigo = 1.0f;

    [Header("Comportamiento de Ataque")]
    public float distanciaRetroceso = 0.5f;
    public float duracionRetroceso = 0.2f;
    private bool estaRetrocediendo = false;

    // Inicializa las estadísticas y componentes específicos del Caballero al ser creado.
    protected override void Awake()
    {
        base.Awake();
        vidaMaxima = 150f;
        vidaActual = vidaMaxima;
        velocidadMovimiento = 3.0f;
        if (navMeshAgent != null) navMeshAgent.speed = velocidadMovimiento;
    }

    // Gestiona la lógica de carga, retroceso, ataque, movimiento y búsqueda automática de enemigos en cada frame.
    public override void Update()
    {
        base.Update();

        if (estaCargando)
        {
            if (navMeshAgent != null && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.1f)
            {
                FinalizarCarga(true);
            }
            return;
        }

        if (estaRetrocediendo)
        {
            return;
        }

        if (objetivoAtaqueActual != null)
        {
            if (objetivoAtaqueActual.vidaActual <= 0)
            {
                objetivoAtaqueActual = null;
                if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();

                if (puedeAgredirAutomaticamente)
                {
                    BuscarSiguienteEnemigoMasCercano();
                }
            }
            else
            {
                float distanciaAlObjetivo = Vector3.Distance(transform.position, objetivoAtaqueActual.transform.position);
                if (distanciaAlObjetivo > rangoAtaqueCaballero)
                {
                    MoverA(objetivoAtaqueActual.transform.position);
                }
                else
                {
                    if (navMeshAgent != null && navMeshAgent.hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.1f)
                    {
                        navMeshAgent.ResetPath();
                    }
                    else if (navMeshAgent != null && navMeshAgent.velocity.sqrMagnitude > 0.1f && distanciaAlObjetivo <= rangoAtaqueCaballero)
                    {
                        navMeshAgent.velocity = Vector3.zero;
                        if (navMeshAgent.hasPath) navMeshAgent.ResetPath();
                    }
                    MirarHacia(objetivoAtaqueActual.transform.position);
                    if (Time.time >= proximoTiempoAtaque)
                    {
                        RealizarAtaqueMelee(objetivoAtaqueActual);
                    }
                }
            }
        }
        else if (puedeAgredirAutomaticamente && navMeshAgent != null && (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f))
        {
            if (Time.time >= proximoTiempoBusquedaEnemigo)
            {
                BuscarSiguienteEnemigoMasCercano();
                proximoTiempoBusquedaEnemigo = Time.time + intervaloBusquedaEnemigo;
            }
        }
    }

    // Orienta al Caballero para que mire hacia un punto específico en el espacio.
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

    // Establece una unidad enemiga como el objetivo de ataque actual del Caballero.
    public override void Atacar(Unidad objetivo)
    {
        if (estaRetrocediendo || estaCargando) return;

        if (objetivo != null && objetivo.vidaActual > 0 && objetivo.equipoID != this.equipoID)
        {
            objetivoAtaqueActual = objetivo;
            if (navMeshAgent != null) navMeshAgent.speed = velocidadMovimiento;
        }
        else
        {
            if (objetivoAtaqueActual == objetivo)
            {
                objetivoAtaqueActual = null;
            }
        }
    }

    // Ejecuta un ataque cuerpo a cuerpo contra la unidad objetivo y puede iniciar un retroceso.
    private void RealizarAtaqueMelee(Unidad objetivo)
    {
        if (objetivo == null || objetivo.vidaActual <= 0 || estaRetrocediendo || estaCargando) return;

        objetivo.RecibirDano(danoAtaqueCaballero);

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            if (objetivo.vidaActual > 0 && distanciaRetroceso > 0)
            {
                StartCoroutine(RetrocederTrasAtaqueCoroutine(objetivo.transform.position));
            }
            else
            {
                proximoTiempoAtaque = Time.time + cooldownAtaque;
            }
        }
        else
        {
            proximoTiempoAtaque = Time.time + cooldownAtaque;
        }
    }

    // Corrutina que hace retroceder al Caballero una corta distancia después de realizar un ataque.
    private IEnumerator RetrocederTrasAtaqueCoroutine(Vector3 posicionDelObjetivoAlAtacar)
    {
        if (estaRetrocediendo) yield break;

        estaRetrocediendo = true;
        if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();

        Vector3 direccionDesdeObjetivo = (transform.position - posicionDelObjetivoAlAtacar).normalized;
        if (direccionDesdeObjetivo.sqrMagnitude < 0.01f) direccionDesdeObjetivo = -transform.forward;

        float tiempoPasado = 0f;
        float velocidadDeRetroceso = distanciaRetroceso / duracionRetroceso;

        while (tiempoPasado < duracionRetroceso)
        {
            if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
            {
                estaRetrocediendo = false;
                proximoTiempoAtaque = Time.time + cooldownAtaque;
                yield break;
            }
            navMeshAgent.Move(direccionDesdeObjetivo * velocidadDeRetroceso * Time.deltaTime);
            tiempoPasado += Time.deltaTime;
            yield return null;
        }

        estaRetrocediendo = false;
        proximoTiempoAtaque = Time.time + cooldownAtaque;
    }

    // Procesa el daño recibido por el Caballero, aplicando la reducción por armadura.
    public override void RecibirDano(float cantidad)
    {
        float danoReducido = Mathf.Max(0, cantidad - armadura);
        vidaActual -= danoReducido;
        if (vidaActual <= 0) { vidaActual = 0; Morir(); }
    }

    // Maneja la lógica de destrucción del Caballero cuando su vida llega a cero.
    protected override void Morir()
    {
        StopAllCoroutines();
        objetivoAtaqueActual = null;
        estaCargando = false;
        estaRetrocediendo = false;
        base.Morir();
    }

    // Inicia la habilidad de carga del Caballero hacia un punto destino.
    public void IniciarCargaHacia(Vector3 puntoDestino)
    {
        if (Time.time < proximoTiempoCarga) return;
        if (estaCargando || estaRetrocediendo) return;

        estaCargando = true;
        objetivoAtaqueActual = null;
        if (navMeshAgent != null) navMeshAgent.speed = velocidadCarga;
        MoverA(puntoDestino);
        Invoke(nameof(FinalizarCargaPorTiempoSiNoLlego), duracionBoostVelocidadCarga);
    }

    // Finaliza la carga si no se ha alcanzado el objetivo después de un tiempo determinado, restaurando la velocidad normal.
    private void FinalizarCargaPorTiempoSiNoLlego()
    {
        if (estaCargando && navMeshAgent != null) navMeshAgent.speed = velocidadMovimiento;
    }

    // Finaliza la habilidad de carga, aplica daño en área si corresponde y restaura la velocidad normal.
    private void FinalizarCarga(bool aplicarDanoArea)
    {
        if (!estaCargando) return;
        estaCargando = false;
        if (navMeshAgent != null) navMeshAgent.speed = velocidadMovimiento;
        if (aplicarDanoArea)
        {
            Collider[] unidadesGolpeadas = Physics.OverlapSphere(transform.position, 2.0f, Unidad.capaUnidades);
            foreach (Collider col in unidadesGolpeadas)
            {
                Unidad u = col.GetComponent<Unidad>();
                if (u != null && u != this && u.equipoID != this.equipoID && u.vidaActual > 0)
                {
                    u.RecibirDano(danoCarga);
                }
            }
        }
        proximoTiempoCarga = Time.time + cooldownCarga;
    }

    // Intenta usar una habilidad especial, primariamente la carga si es la habilidad número 0.
    public override void UsarHabilidadEspecial(int numeroHabilidad, Unidad objetivoUnidad = null, Vector3 posicionObjetivo = default)
    {
        if (estaRetrocediendo) return;
        if (numeroHabilidad == 0)
        {
            Vector3 puntoDeCarga = default;
            bool objetivoValidoParaCarga = false;
            if (objetivoUnidad != null && objetivoUnidad.equipoID != this.equipoID && objetivoUnidad.vidaActual > 0)
            {
                puntoDeCarga = objetivoUnidad.transform.position;
                objetivoValidoParaCarga = true;
            }
            else if (posicionObjetivo != default)
            {
                puntoDeCarga = posicionObjetivo;
                objetivoValidoParaCarga = true;
            }
            if (objetivoValidoParaCarga) IniciarCargaHacia(puntoDeCarga);
        }
        else base.UsarHabilidadEspecial(numeroHabilidad, objetivoUnidad, posicionObjetivo);
    }

    // Busca y asigna automáticamente la unidad enemiga más cercana dentro del rango de enganche.
    protected virtual void BuscarSiguienteEnemigoMasCercano()
    {
        if (navMeshAgent == null || estaRetrocediendo || estaCargando) return;

        Unidad enemigoMasCercanoEncontrado = null;
        float menorDistanciaSqr = float.MaxValue;

        foreach (Unidad unidadPotencial in Unidad.todasLasUnidadesActivas)
        {
            if (unidadPotencial != null && unidadPotencial.vidaActual > 0 && unidadPotencial.equipoID != this.equipoID)
            {
                float distanciaSqr = (transform.position - unidadPotencial.transform.position).sqrMagnitude;
                if (distanciaSqr < menorDistanciaSqr)
                {
                    menorDistanciaSqr = distanciaSqr;
                    enemigoMasCercanoEncontrado = unidadPotencial;
                }
            }
        }

        if (enemigoMasCercanoEncontrado != null)
        {
            if (menorDistanciaSqr <= (maxDistanciaEnganche * maxDistanciaEnganche))
            {
                Atacar(enemigoMasCercanoEncontrado);
            }
        }
    }
}