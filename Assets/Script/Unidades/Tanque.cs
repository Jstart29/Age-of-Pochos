using UnityEngine;
using UnityEngine.AI;

public class Tanque : Unidad
{
    [Header("Estadísticas de Tanque")]
    public float danoAtaqueTanque = 8f;
    public float rangoAtaqueTanque = 2.0f;
    public float cooldownAtaqueTanque = 2.2f;
    public float armaduraTanque = 8f;

    private float proximoTiempoAtaque = 0f;
    private Unidad objetivoAtaqueActual;

    [Header("Comportamiento de Auto-Agresión del Tanque")]
    public bool puedeAgredirAutomaticamente = true;
    public float maxDistanciaAgresionTanque = 12f;
    private float proximoTiempoBusquedaEnemigo = 0f;
    private float intervaloBusquedaEnemigo = 1.0f;

    // Inicializa las estadísticas y componentes específicos del Tanque al ser creado.
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

    // Gestiona la lógica de ataque, movimiento hacia el objetivo y búsqueda automática de enemigos en cada frame.
    public override void Update()
    {
        base.Update();

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

    // Orienta al Tanque para que mire hacia un punto específico en el espacio.
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

    // Establece una unidad enemiga como el objetivo de ataque actual del Tanque.
    public override void Atacar(Unidad objetivo)
    {
        if (objetivo != null && objetivo.vidaActual > 0 && objetivo.equipoID != this.equipoID)
        {
            objetivoAtaqueActual = objetivo;
        }
        else
        {
            if (objetivoAtaqueActual == objetivo)
            {
                objetivoAtaqueActual = null;
            }
        }
    }

    // Ejecuta la acción de ataque contra la unidad objetivo especificada.
    private void RealizarAtaqueSimple(Unidad objetivo)
    {
        if (objetivo == null || objetivo.vidaActual <= 0) return;

        objetivo.RecibirDano(danoAtaqueTanque);
        proximoTiempoAtaque = Time.time + cooldownAtaqueTanque;
    }

    // Procesa el daño recibido por el Tanque, aplicando la reducción por armadura.
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

    // Maneja la lógica de destrucción del Tanque cuando su vida llega a cero.
    protected override void Morir()
    {
        objetivoAtaqueActual = null;
        base.Morir();
    }

    // Busca y asigna automáticamente la unidad enemiga más cercana dentro del rango de agresión.
    protected virtual void BuscarSiguienteEnemigoMasCercano()
    {
        if (navMeshAgent == null) return;

        Unidad enemigoMasCercanoEncontrado = null;
        float menorDistanciaSqr = maxDistanciaAgresionTanque * maxDistanciaAgresionTanque;

        foreach (Unidad unidadPotencial in Unidad.todasLasUnidadesActivas)
        {
            if (unidadPotencial != null && unidadPotencial.vidaActual > 0 && unidadPotencial.equipoID != this.equipoID)
            {
                float distanciaSqrActual = (transform.position - unidadPotencial.transform.position).sqrMagnitude;
                if (distanciaSqrActual < menorDistanciaSqr)
                {
                    menorDistanciaSqr = distanciaSqrActual;
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