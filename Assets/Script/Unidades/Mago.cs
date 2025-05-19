// Mago.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Mago : Unidad
{
    [Header("Estadísticas de Mago")]
    public float manaMaximo = 100f;
    public float manaActual;
    public float regeneracionManaPorSegundo = 2f;

    [Header("Ataque Principal (Área Parabólico)")]
    public GameObject proyectilAoEPrefab;
    public Transform puntoDeDisparo;
    public float rangoAtaqueAoE = 20f;
    public float cooldownAtaqueAoE = 4f;
    public float costoManaAtaqueAoE = 25f;
    private float proximoTiempoAtaqueAoE = 0f;
    private Unidad objetivoAtaqueActual;
    private Vector3? puntoObjetivoAtaqueSuelo;

    [Header("Comportamiento de Auto-Agresión")]
    public bool puedeAgredirAutomaticamente = true;
    public float maxDistanciaAgresion = 25f;
    private float proximoTiempoBusquedaEnemigo = 0f;
    private float intervaloBusquedaEnemigo = 1.0f;

    [Header("Comportamiento de Ataque")]
    public float distanciaRetroceso = 0.5f; // Actualmente no se usa en la lógica activa del Mago
    public float duracionRetroceso = 0.2f; // Actualmente no se usa en la lógica activa del Mago
    private bool estaRetrocediendo = false; // Actualmente no se usa en la lógica activa del Mago

    // Inicializa las estadísticas, componentes y referencias del Mago al ser creado.
    protected override void Awake()
    {
        base.Awake();

        vidaMaxima = 80f;
        manaActual = manaMaximo;
        vidaActual = vidaMaxima;
        velocidadMovimiento = 2.8f;
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = velocidadMovimiento;
        }

        if (puntoDeDisparo == null)
        {
            puntoDeDisparo = transform.Find("PuntoDisparo");
            if (puntoDeDisparo == null)
            {
                puntoDeDisparo = transform;
                Debug.LogWarning($"Mago {gameObject.name}: 'PuntoDeDisparo' no encontrado como hijo o no asignado. Usando la posición del Mago.", this);
            }
        }
        if (proyectilAoEPrefab == null)
        {
            Debug.LogError($"Mago {gameObject.name}: ¡'Proyectil AoE Prefab' no está asignado en el Inspector!", this);
        }
    }

    // Gestiona la regeneración de maná, lógica de ataque, movimiento y búsqueda de enemigos en cada frame.
    public override void Update()
    {
        base.Update();

        if (estaRetrocediendo) // Aunque la variable existe, la lógica de retroceso no está implementada.
        {
            return;
        }

        if (manaActual < manaMaximo)
        {
            manaActual += regeneracionManaPorSegundo * Time.deltaTime;
            manaActual = Mathf.Min(manaActual, manaMaximo);
        }

        bool puedeLanzarHechizo = Time.time >= proximoTiempoAtaqueAoE && manaActual >= costoManaAtaqueAoE;

        if (objetivoAtaqueActual != null)
        {
            if (objetivoAtaqueActual.vidaActual <= 0)
            {
                objetivoAtaqueActual = null;
                if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();
            }
            else
            {
                float distanciaAlObjetivo = Vector3.Distance(puntoDeDisparo.position, objetivoAtaqueActual.transform.position);
                if (distanciaAlObjetivo > rangoAtaqueAoE)
                {
                    MoverA(objetivoAtaqueActual.transform.position);
                }
                else
                {
                    if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();
                    MirarHacia(objetivoAtaqueActual.transform.position);
                    if (puedeLanzarHechizo)
                    {
                        LanzarAtaqueArea(objetivoAtaqueActual.transform.position);
                    }
                }
            }
        }
        else if (puntoObjetivoAtaqueSuelo.HasValue)
        {
            float distanciaAlPunto = Vector3.Distance(puntoDeDisparo.position, puntoObjetivoAtaqueSuelo.Value);
            if (distanciaAlPunto > rangoAtaqueAoE)
            {
                MoverA(puntoObjetivoAtaqueSuelo.Value);
            }
            else
            {
                if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();
                MirarHacia(puntoObjetivoAtaqueSuelo.Value);
                if (puedeLanzarHechizo)
                {
                    LanzarAtaqueArea(puntoObjetivoAtaqueSuelo.Value);
                    puntoObjetivoAtaqueSuelo = null;
                }
            }
        }
        else if (puedeAgredirAutomaticamente && navMeshAgent != null && (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f))
        {
            if (Time.time >= proximoTiempoBusquedaEnemigo)
            {
                BuscarSiguienteEnemigoMasCercanoParaAoE();
                proximoTiempoBusquedaEnemigo = Time.time + intervaloBusquedaEnemigo;
            }
        }
    }

    // Orienta al Mago para que mire hacia un punto específico en el espacio.
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

    // Establece una unidad enemiga como el objetivo de ataque actual del Mago.
    public override void Atacar(Unidad objetivo)
    {
        if (estaRetrocediendo) return;

        if (objetivo != null && objetivo.equipoID != this.equipoID && objetivo.vidaActual > 0)
        {
            objetivoAtaqueActual = objetivo;
            puntoObjetivoAtaqueSuelo = null;
            if (navMeshAgent != null) navMeshAgent.speed = velocidadMovimiento;
        }
        else
        {
            if (objetivoAtaqueActual == objetivo) objetivoAtaqueActual = null;
        }
    }

    // Placeholder para una lógica de ataque cuerpo a cuerpo (actualmente no utilizado por el Mago).
    private void RealizarAtaqueMelee(Unidad objetivo)
    {
        // (Sin implementación activa)
    }

    // Placeholder para una corrutina de retroceso tras un ataque (actualmente no utilizado por el Mago).
    private IEnumerator RetrocederTrasAtaqueCoroutine(Vector3 posicionDelObjetivoAlAtacar)
    {
        // (Sin implementación activa más allá de permitir que la corrutina finalice)
        yield return null;
    }

    // Procesa el daño recibido por el Mago, reduciendo su vida actual.
    public override void RecibirDano(float cantidad)
    {
        vidaActual -= cantidad;
        if (vidaActual <= 0) { vidaActual = 0; Morir(); }
    }

    // Maneja la lógica de destrucción del Mago cuando su vida llega a cero.
    protected override void Morir()
    {
        StopAllCoroutines();
        objetivoAtaqueActual = null;
        puntoObjetivoAtaqueSuelo = null;
        estaRetrocediendo = false; // Se resetea aunque la lógica principal de retroceso no esté activa.
        base.Morir();
    }

    // Intenta usar una habilidad especial, primariamente el ataque de área si es la habilidad número 0.
    public override void UsarHabilidadEspecial(int numeroHabilidad, Unidad objetivoUnidad = null, Vector3 posicionObjetivoSuelo = default)
    {
        if (numeroHabilidad == 0)
        {
            if (Time.time >= proximoTiempoAtaqueAoE && manaActual >= costoManaAtaqueAoE)
            {
                Vector3? targetPointForAbility = null;
                if (objetivoUnidad != null && objetivoUnidad.equipoID != this.equipoID && objetivoUnidad.vidaActual > 0)
                {
                    targetPointForAbility = objetivoUnidad.transform.position;
                }
                else if (posicionObjetivoSuelo != default)
                {
                    targetPointForAbility = posicionObjetivoSuelo;
                }

                if (targetPointForAbility.HasValue)
                {
                    objetivoAtaqueActual = null;
                    puntoObjetivoAtaqueSuelo = targetPointForAbility.Value;

                    if (Vector3.Distance(puntoDeDisparo.position, puntoObjetivoAtaqueSuelo.Value) <= rangoAtaqueAoE)
                    {
                        LanzarAtaqueArea(puntoObjetivoAtaqueSuelo.Value);
                        puntoObjetivoAtaqueSuelo = null;
                    }
                }
            }
        }
        else base.UsarHabilidadEspecial(numeroHabilidad, objetivoUnidad, posicionObjetivoSuelo);
    }

    // Lanza el proyectil de ataque de área hacia el punto objetivo especificado.
    private void LanzarAtaqueArea(Vector3 puntoObjetivoTierra)
    {
        if (proyectilAoEPrefab == null || puntoDeDisparo == null) return;

        GameObject instanciaProyectil = Instantiate(proyectilAoEPrefab, puntoDeDisparo.position, puntoDeDisparo.rotation);
        ProyectilParabolicoAoE scriptProyectil = instanciaProyectil.GetComponent<ProyectilParabolicoAoE>();

        if (scriptProyectil != null)
        {
            scriptProyectil.Lanzar(puntoDeDisparo.position, puntoObjetivoTierra, this.equipoID, Unidad.capaUnidades);
            manaActual -= costoManaAtaqueAoE;
            proximoTiempoAtaqueAoE = Time.time + cooldownAtaqueAoE;
        }
        else
        {
            Debug.LogError($"El prefab '{proyectilAoEPrefab.name}' no tiene 'ProyectilParabolicoAoE.cs'!", instanciaProyectil);
            Destroy(instanciaProyectil);
        }
    }

    // Busca y asigna automáticamente la unidad enemiga más cercana, priorizando unidades no-Mago.
    protected virtual void BuscarSiguienteEnemigoMasCercanoParaAoE()
    {
        if (navMeshAgent == null) return;

        Unidad enemigoNoMagoMasCercano = null;
        float menorDistanciaSqrNoMago = maxDistanciaAgresion * maxDistanciaAgresion;

        Unidad enemigoMagoMasCercano = null;
        float menorDistanciaSqrMago = maxDistanciaAgresion * maxDistanciaAgresion;

        foreach (Unidad unidadPotencial in Unidad.todasLasUnidadesActivas)
        {
            if (unidadPotencial != null && unidadPotencial.vidaActual > 0 && unidadPotencial.equipoID != this.equipoID)
            {
                float distanciaSqr = (transform.position - unidadPotencial.transform.position).sqrMagnitude;

                if (distanciaSqr < (maxDistanciaAgresion * maxDistanciaAgresion))
                {
                    if (unidadPotencial is Mago)
                    {
                        if (distanciaSqr < menorDistanciaSqrMago)
                        {
                            menorDistanciaSqrMago = distanciaSqr;
                            enemigoMagoMasCercano = unidadPotencial;
                        }
                    }
                    else
                    {
                        if (distanciaSqr < menorDistanciaSqrNoMago)
                        {
                            menorDistanciaSqrNoMago = distanciaSqr;
                            enemigoNoMagoMasCercano = unidadPotencial;
                        }
                    }
                }
            }
        }

        if (enemigoNoMagoMasCercano != null)
        {
            Atacar(enemigoNoMagoMasCercano);
        }
        else if (enemigoMagoMasCercano != null)
        {
            Atacar(enemigoMagoMasCercano);
        }
    }
}