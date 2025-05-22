using UnityEngine;
using UnityEngine.AI;

public class Tanque : Unidad
{
    [Header("Estadísticas de Tanque")]
    public float danoAtaqueTanque = 8f;
    public float rangoAtaqueTanque = 2.0f;
    public float cooldownAtaqueTanque = 2.2f;
    public float armaduraTanque = 10f;

    private float proximoTiempoAtaque = 0f;
    private Unidad objetivoAtaqueActual;

    [Header("Comportamiento de Auto-Agresión del Tanque")]
    public bool puedeAgredirAutomaticamente = true;
    public float maxDistanciaAgresionTanque = 15f;
    private float proximoTiempoBusquedaEnemigo = 0f;
    private float intervaloBusquedaEnemigo = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        vidaMaxima = 400f;
        vidaActual = vidaMaxima;
        velocidadMovimiento = 1.8f;
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = velocidadMovimiento;
        }
    }

    protected override void CancelarAccionActual()
    {
        base.CancelarAccionActual(); // Llama a la implementación de Unidad (resetea path)
        objetivoAtaqueActual = null;
        // Si el Tanque tuviera otros estados específicos (ej. un modo defensivo), resetéalos aquí.
    }

    public override void Update()
    {
        base.Update();

        if (objetivoAtaqueActual != null)
        {
            if (objetivoAtaqueActual.vidaActual <= 0)
            {
                objetivoAtaqueActual = null;
                if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();
                if (puedeAgredirAutomaticamente) BuscarSiguienteEnemigoMasCercano();
            }
            else
            {
                float distanciaAlObjetivo = Vector3.Distance(transform.position, objetivoAtaqueActual.transform.position);
                if (distanciaAlObjetivo > rangoAtaqueTanque)
                {
                    MoverA(objetivoAtaqueActual.transform.position);
                }
                else
                {
                    if (navMeshAgent != null && navMeshAgent.hasPath) navMeshAgent.ResetPath();
                    MirarHacia(objetivoAtaqueActual.transform.position);
                    if (Time.time >= proximoTiempoAtaque)
                    {
                        RealizarAtaqueSimple(objetivoAtaqueActual);
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
        // CancelarAccionActual() ya es llamado por Unidad.Atacar()
        if (objetivo != null && objetivo.vidaActual > 0 && objetivo.equipoID != this.equipoID)
        {
            objetivoAtaqueActual = objetivo;
        }
        else
        {
            if (objetivoAtaqueActual == objetivo) objetivoAtaqueActual = null;
        }
    }

    private void RealizarAtaqueSimple(Unidad objetivo)
    {
        if (objetivo == null || objetivo.vidaActual <= 0) return;

        objetivo.RecibirDano(danoAtaqueTanque);
        proximoTiempoAtaque = Time.time + cooldownAtaqueTanque;
    }

    public override void RecibirDano(float cantidad)
    {
        float danoReducido = Mathf.Max(0, cantidad - armaduraTanque);
        vidaActual -= danoReducido;
        if (vidaActual <= 0)
        {
            vidaActual = 0;
            Morir();
        }
    }

    protected override void Morir()
    {
        CancelarAccionActual();
        base.Morir();
    }

    protected virtual void BuscarSiguienteEnemigoMasCercano()
    {
        if (navMeshAgent == null) return;

        Unidad enemigoMasCercanoEncontrado = null;
        float menorDistanciaSqr = maxDistanciaAgresionTanque * maxDistanciaAgresionTanque;

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