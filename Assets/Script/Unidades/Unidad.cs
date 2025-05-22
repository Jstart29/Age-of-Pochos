using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Unidad : MonoBehaviour
{
    [Header("Estadísticas Base de Unidad")]
    public float velocidadMovimiento = 3.5f;
    public float velocidadAngular = 120f;
    public float vidaMaxima = 100f;
    [HideInInspector] public float vidaActual;
    public int equipoID = 1;

    [Header("Selección Visual")]
    public GameObject indicadorSeleccion;
    [HideInInspector] public bool estaSeleccionada = false;

    protected NavMeshAgent navMeshAgent;

    public static List<Unidad> unidadesSeleccionadas = new List<Unidad>();
    public static List<Unidad> todasLasUnidadesActivas = new List<Unidad>();
    public static List<NodoDeRecurso> todosLosNodosDeRecurso = new List<NodoDeRecurso>();

    public static Camera camaraPrincipal;
    public static LayerMask capaSuelo;
    public static LayerMask capaUnidades;
    public static LayerMask capaNodosRecursos;
    public static int equipoDelJugador = 1;

    public static bool estaArrastrandoCaja = false;
    public static Vector2 posicionInicioArrastreCaja;
    protected static float umbralMinimoArrastreSqr = 5f * 5f;

    protected virtual void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = velocidadMovimiento;
            navMeshAgent.angularSpeed = velocidadAngular;
        }
        else if (!(this is Estructura)) // Las estructuras no necesitan NavMeshAgent obligatoriamente
        {
            Debug.LogError($"Componente NavMeshAgent no encontrado en {gameObject.name}! La unidad no podrá moverse.", this);
        }

        vidaActual = vidaMaxima;
        if (indicadorSeleccion != null)
        {
            indicadorSeleccion.SetActive(false);
        }

        if (!todasLasUnidadesActivas.Contains(this))
        {
            todasLasUnidadesActivas.Add(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (todasLasUnidadesActivas.Contains(this))
        {
            todasLasUnidadesActivas.Remove(this);
        }
        if (unidadesSeleccionadas.Contains(this))
        {
            unidadesSeleccionadas.Remove(this);
        }
    }

    public virtual void Update()
    {
        if (indicadorSeleccion != null)
        {
            indicadorSeleccion.SetActive(estaSeleccionada);
        }
    }

    protected virtual void CancelarAccionActual()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.hasPath)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = true; // Detenerlo explícitamente
        }
    }

    public virtual void MoverA(Vector3 destino)
    {
        if (this is Estructura) return;

        CancelarAccionActual();

        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(destino);
        }
    }

    public virtual void RecibirDano(float cantidad)
    {
        if (vidaActual <= 0 && vidaMaxima > 0) return;

        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            vidaActual = 0;
            Morir();
        }
    }

    protected virtual void Morir()
    {
        Destroy(gameObject);
    }

    public virtual void Atacar(Unidad objetivo)
    {
        if (this is Estructura) return;
        CancelarAccionActual();
    }

    public virtual void ComandoInteraccionNodoRecurso(NodoDeRecurso nodo)
    {
        if (this is Estructura) return;
        CancelarAccionActual();
    }

    public virtual void UsarHabilidadEspecial(int numeroHabilidad, Unidad objetivoUnidad = null, Vector3 posicionObjetivoSuelo = default)
    {
        if (this is Estructura) return;
        CancelarAccionActual();
    }

    public void SeleccionarEstaUnidad() { estaSeleccionada = true; }
    public void DeseleccionarEstaUnidad() { estaSeleccionada = false; }

    public static void ProcesarInputGlobal()
    {
        if (camaraPrincipal == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            estaArrastrandoCaja = true;
            posicionInicioArrastreCaja = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (estaArrastrandoCaja)
            {
                Vector2 posicionFinArrastre = Input.mousePosition;
                float distanciaSqr = (posicionInicioArrastreCaja - posicionFinArrastre).sqrMagnitude;
                bool fueClicSimple = distanciaSqr < umbralMinimoArrastreSqr;
                estaArrastrandoCaja = false;
                bool shiftPresionado = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                Ray rayo = camaraPrincipal.ScreenPointToRay(posicionFinArrastre);
                RaycastHit hitInfo;

                if (fueClicSimple)
                {
                    if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaUnidades))
                    {
                        Unidad unidadClickeada = hitInfo.collider.GetComponent<Unidad>();
                        if (unidadClickeada != null && unidadClickeada.equipoID == equipoDelJugador)
                        {
                            if (!shiftPresionado) { DeseleccionarTodasLasUnidadesStatic(); SeleccionarUnidadStatic(unidadClickeada); }
                            else { if (!unidadesSeleccionadas.Contains(unidadClickeada)) { SeleccionarUnidadStatic(unidadClickeada); } else { DeseleccionarUnidadStatic(unidadClickeada); } }
                        }
                    }
                    else if (!shiftPresionado && Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaSuelo))
                    {
                        DeseleccionarTodasLasUnidadesStatic();
                    }
                }
                else
                {
                    Rect rectSeleccion = ObtenerRectanguloDePantalla(posicionInicioArrastreCaja, posicionFinArrastre);
                    if (!shiftPresionado) { DeseleccionarTodasLasUnidadesStatic(); }
                    foreach (Unidad unidadEnJuego in todasLasUnidadesActivas)
                    {
                        if (unidadEnJuego != null && unidadEnJuego.equipoID == equipoDelJugador)
                        {
                            Vector3 posUnidadEnPantalla = camaraPrincipal.WorldToScreenPoint(unidadEnJuego.transform.position);
                            if (posUnidadEnPantalla.z > 0 && rectSeleccion.Contains(new Vector2(posUnidadEnPantalla.x, Screen.height - posUnidadEnPantalla.y), true))
                            { SeleccionarUnidadStatic(unidadEnJuego); }
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && unidadesSeleccionadas.Count > 0)
        {
            Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaNodosRecursos))
            {
                NodoDeRecurso nodoDetectado = hitInfo.collider.GetComponent<NodoDeRecurso>();
                if (nodoDetectado != null && nodoDetectado.CantidadActual > 0)
                {
                    foreach (Unidad uSeleccionada in unidadesSeleccionadas)
                    {
                        uSeleccionada.ComandoInteraccionNodoRecurso(nodoDetectado);
                    }
                    return;
                }
            }

            if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaUnidades))
            {
                Unidad unidadObjetivo = hitInfo.collider.GetComponent<Unidad>();
                if (unidadObjetivo != null && unidadObjetivo.equipoID != equipoDelJugador)
                {
                    foreach (Unidad uSeleccionada in unidadesSeleccionadas)
                    {
                        if (unidadObjetivo.equipoID != uSeleccionada.equipoID) uSeleccionada.Atacar(unidadObjetivo);
                    }
                    return;
                }
            }

            if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaSuelo))
            {
                MoverUnidadesSeleccionadas(hitInfo.point);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && Input.GetMouseButtonDown(0) && unidadesSeleccionadas.Count > 0)
        {
            Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            Unidad unidadHabilidadObjetivo = null;
            Vector3 posicionHabilidadObjetivo = default;
            bool objetivoHabilidadEncontrado = false;

            if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaUnidades))
            {
                unidadHabilidadObjetivo = hitInfo.collider.GetComponent<Unidad>();
                if (unidadHabilidadObjetivo != null)
                {
                    posicionHabilidadObjetivo = unidadHabilidadObjetivo.transform.position;
                    objetivoHabilidadEncontrado = true;
                }
            }
            if (!objetivoHabilidadEncontrado && Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaSuelo))
            {
                posicionHabilidadObjetivo = hitInfo.point;
                objetivoHabilidadEncontrado = true;
            }

            if (objetivoHabilidadEncontrado)
            {
                foreach (Unidad uSeleccionada in unidadesSeleccionadas)
                {
                    uSeleccionada.UsarHabilidadEspecial(0, unidadHabilidadObjetivo, posicionHabilidadObjetivo);
                }
            }
        }
    }

    protected static void SeleccionarUnidadStatic(Unidad unidad)
    {
        if (unidad == null || unidad.equipoID != equipoDelJugador || unidadesSeleccionadas.Contains(unidad)) return;
        unidadesSeleccionadas.Add(unidad);
        unidad.SeleccionarEstaUnidad();
    }

    protected static void DeseleccionarUnidadStatic(Unidad unidad)
    {
        if (unidad != null && unidadesSeleccionadas.Contains(unidad))
        {
            unidad.DeseleccionarEstaUnidad();
            unidadesSeleccionadas.Remove(unidad);
        }
    }

    protected static void DeseleccionarTodasLasUnidadesStatic()
    {
        if (unidadesSeleccionadas.Count == 0) return;
        foreach (Unidad u in new List<Unidad>(unidadesSeleccionadas)) { if (u != null) u.DeseleccionarEstaUnidad(); }
        unidadesSeleccionadas.Clear();
    }

    protected static void MoverUnidadesSeleccionadas(Vector3 centroDestino)
    {
        List<Unidad> unidadesMovibles = new List<Unidad>();
        foreach (Unidad u in unidadesSeleccionadas)
        {
            if (u != null && !(u is Estructura))
            {
                unidadesMovibles.Add(u);
            }
        }

        int count = unidadesMovibles.Count;
        if (count == 0) return;

        float espaciado = 2.0f;
        Vector3 direccionFormacion = camaraPrincipal != null ? camaraPrincipal.transform.right : Vector3.right;

        if (count == 1)
        {
            unidadesMovibles[0].MoverA(centroDestino);
            return;
        }

        Vector3 inicioFormacion = centroDestino - direccionFormacion * (espaciado * (count - 1) / 2f);
        for (int i = 0; i < count; i++)
        {
            if (unidadesMovibles[i] != null) // Ya filtrado, pero por seguridad
            {
                Vector3 posicionUnidadEnFormacion = inicioFormacion + direccionFormacion * (i * espaciado);
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(posicionUnidadEnFormacion, out navHit, espaciado, NavMesh.AllAreas))
                { unidadesMovibles[i].MoverA(navHit.position); }
                else { unidadesMovibles[i].MoverA(centroDestino); }
            }
        }
    }

    public static Rect ObtenerRectanguloDePantalla(Vector2 p1Mouse, Vector2 p2Mouse)
    {
        Vector2 p1GUI = new Vector2(p1Mouse.x, Screen.height - p1Mouse.y);
        Vector2 p2GUI = new Vector2(p2Mouse.x, Screen.height - p2Mouse.y);
        float xMin = Mathf.Min(p1GUI.x, p2GUI.x); float xMax = Mathf.Max(p1GUI.x, p2GUI.x);
        float yMin = Mathf.Min(p1GUI.y, p2GUI.y); float yMax = Mathf.Max(p1GUI.y, p2GUI.y);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}