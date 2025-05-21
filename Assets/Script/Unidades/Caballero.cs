using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Caballero : Unidad
{
    [Header("Estadísticas de Caballero")]
    public float armadura = 5f;
    public float danoAtaqueCaballero = 15f;
    public float rangoAtaqueCaballero = 2.5f;
    public float cooldownAtaque = 1.5f;
    private float proximoTiempoAtaque = 0f;

    [Header("Habilidad de Carga")]
    public float velocidadCarga = 8f;
    public float duracionBoostVelocidadCarga = 1f;
    public float danoCarga = 25f;
    public float cooldownCarga = 10f;
    private float proximoTiempoCarga = 0f;
    private bool estaCargando = false;

    [Header("Comportamiento de Ataque")]
    public float distanciaRetroceso = 0.5f;
    public float duracionRetroceso = 0.2f;
    private bool estaRetrocediendo = false;

    private Unidad objetivoAtaqueActual;

    [Header("Comportamiento de Auto-Agresión")]
    public bool puedeAgredirAutomaticamente = true;
    public float maxDistanciaAgresion = 20f;
    private float proximoTiempoBusquedaEnemigo = 0f;
    private float intervaloBusquedaEnemigo = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        vidaMaxima = 150f;
        vidaActual = vidaMaxima;
        velocidadMovimiento = 3.0f;
        if (navMeshAgent != null) navMeshAgent.speed = velocidadMovimiento;
    }

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

    public override void Atacar(Unidad objetivo)
    {
        if (estaRetrocediendo || estaCargando) return;

        if (objetivo != null && objetivo.vidaActual > 0 && objetivo.equipoID != this.equipoID)
        {
            objetivoAtaqueActual = objetivo;
            estaCargando = false;
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

    private void RealizarAtaqueMelee(Unidad objetivo)
    {
        if (objetivo == null || objetivo.vidaActual <= 0 || estaRetrocediendo || estaCargando) return;

        objetivo.RecibirDano(danoAtaqueCaballero);

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh && distanciaRetroceso > 0 && duracionRetroceso > 0)
        {
            if (objetivo.vidaActual > 0)
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

    private IEnumerator RetrocederTrasAtaqueCoroutine(Vector3 posicionDelObjetivoAlAtacar)
    {
        if (estaRetrocediendo) yield break;

        estaRetrocediendo = true;
        if (navMeshAgent != null && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
        }

        Vector3 direccionDesdeObjetivo = (transform.position - posicionDelObjetivoAlAtacar).normalized;
        if (direccionDesdeObjetivo.sqrMagnitude < 0.01f)
        {
            direccionDesdeObjetivo = -transform.forward;
        }

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

    public override void RecibirDano(float cantidad)
    {
        float danoReducido = Mathf.Max(0, cantidad - armadura);
        vidaActual -= danoReducido;
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            Morir();
        }
    }

    protected override void Morir()
    {
        StopAllCoroutines();
        objetivoAtaqueActual = null;
        estaCargando = false;
        estaRetrocediendo = false;
        base.Morir();
    }

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

    private void FinalizarCargaPorTiempoSiNoLlego()
    {
        if (estaCargando && navMeshAgent != null)
        {
            if (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance + 0.1f)
            {
                navMeshAgent.speed = velocidadMovimiento;
            }
        }
    }

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

    public override void UsarHabilidadEspecial(int numeroHabilidad, Unidad objetivoUnidad = null, Vector3 posicionObjetivoSuelo = default)
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
            else if (posicionObjetivoSuelo != default)
            {
                puntoDeCarga = posicionObjetivoSuelo;
                objetivoValidoParaCarga = true;
            }

            if (objetivoValidoParaCarga)
            {
                IniciarCargaHacia(puntoDeCarga);
            }
        }
        else
        {
            base.UsarHabilidadEspecial(numeroHabilidad, objetivoUnidad, posicionObjetivoSuelo);
        }
    }

    protected virtual void BuscarSiguienteEnemigoMasCercano()
    {
        if (navMeshAgent == null || estaRetrocediendo || estaCargando) return;

        Unidad enemigoMasCercanoEncontrado = null;
        float menorDistanciaSqr = maxDistanciaAgresion * maxDistanciaAgresion;

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
            Atacar(enemigoMasCercanoEncontrado);
        }
    }
}