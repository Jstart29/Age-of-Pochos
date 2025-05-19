// ProyectilParabolicoAoE.cs
using UnityEngine;
using System.Collections.Generic;

public class ProyectilParabolicoAoE : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    public float velocidadHorizontal = 15f;
    public float alturaArco = 7f;
    public float radioImpactoAoE = 3.5f;
    public float danoAoE = 30f;
    public LayerMask capaUnidadesAfectables;
    public GameObject efectoImpactoPrefab;

    private Vector3 puntoInicio;
    private Vector3 puntoObjetivoTierra;
    private int equipoEmisor;
    private float tiempoDeVueloEstimado;
    private float tiempoTranscurrido;
    private bool impactoRealizado = false;
    private List<Unidad> unidadesYaAfectadas = new List<Unidad>();

    // Inicializa y configura el proyectil para su lanzamiento hacia un objetivo.
    public void Lanzar(Vector3 inicio, Vector3 objetivoTierra, int equipoDelLanzador, LayerMask capaUnidadesDetect)
    {
        this.puntoInicio = inicio;
        this.puntoObjetivoTierra = objetivoTierra;
        this.equipoEmisor = equipoDelLanzador;
        this.capaUnidadesAfectables = capaUnidadesDetect;

        transform.position = inicio;

        Vector3 diferenciaHorizontal = objetivoTierra - inicio;
        diferenciaHorizontal.y = 0;

        if (velocidadHorizontal <= 0.01f) velocidadHorizontal = 0.01f;
        tiempoDeVueloEstimado = diferenciaHorizontal.magnitude / velocidadHorizontal;

        if (tiempoDeVueloEstimado <= 0.05f)
        {
            tiempoDeVueloEstimado = 0.05f;
        }

        tiempoTranscurrido = 0f;
        impactoRealizado = false;
        unidadesYaAfectadas.Clear();
    }

    // Actualiza la posición del proyectil en cada frame para simular una trayectoria parabólica y llama al impacto al llegar al objetivo.
    void Update()
    {
        if (impactoRealizado) return;

        tiempoTranscurrido += Time.deltaTime;
        float t = tiempoTranscurrido / tiempoDeVueloEstimado;

        if (t >= 1.0f)
        {
            transform.position = puntoObjetivoTierra;
            RealizarImpacto();
        }
        else
        {
            Vector3 posicionHorizontal = Vector3.Lerp(puntoInicio, puntoObjetivoTierra, t);
            float yOffsetParabolico = alturaArco * 4 * (t - (t * t));
            float yBase = Mathf.Lerp(puntoInicio.y, puntoObjetivoTierra.y, t);
            transform.position = new Vector3(posicionHorizontal.x, yBase + yOffsetParabolico, posicionHorizontal.z);
        }
    }

    // Ejecuta la lógica de impacto: aplica daño en área a las unidades enemigas y genera efectos visuales.
    void RealizarImpacto()
    {
        if (impactoRealizado) return;
        impactoRealizado = true;

        if (efectoImpactoPrefab != null)
        {
            Instantiate(efectoImpactoPrefab, transform.position, Quaternion.identity);
        }

        Collider[] unidadesEnArea = Physics.OverlapSphere(transform.position, radioImpactoAoE, capaUnidadesAfectables);

        foreach (Collider col in unidadesEnArea)
        {
            Unidad unidadAfectada = col.GetComponent<Unidad>();
            if (unidadAfectada != null)
            {
                if (unidadAfectada.equipoID != equipoEmisor && unidadAfectada.vidaActual > 0)
                {
                    if (!unidadesYaAfectadas.Contains(unidadAfectada))
                    {
                        // El Debug.Log original fue eliminado según la solicitud.
                        // Si necesitas depurar qué unidades se están dañando, puedes añadir un Log aquí.
                        // Ejemplo: Debug.Log($"DAÑANDO a {unidadAfectada.name} (Equipo {unidadAfectada.equipoID})!");
                        unidadAfectada.RecibirDano(danoAoE);
                        unidadesYaAfectadas.Add(unidadAfectada);
                    }
                }
            }
        }
        Destroy(gameObject, 0.2f);
    }

    // Dibuja un Gizmo en el editor para visualizar el radio de impacto del proyectil cuando está seleccionado.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, radioImpactoAoE);
    }
}