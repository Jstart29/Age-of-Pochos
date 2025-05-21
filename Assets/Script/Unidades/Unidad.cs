// Unidad.cs (Clase Base)
using UnityEngine;
using UnityEngine.AI; // Necesario para NavMeshAgent
using System.Collections.Generic; // Necesario para List

public class Unidad : MonoBehaviour
{
    // --- VARIABLES DE INSTANCIA (Protegidas para herencia, configurables en el Inspector o por clases hijas) ---
    [Header("Estad�sticas Base de Unidad")]
    public float velocidadMovimiento = 3.5f;
    public float velocidadAngular = 120f; // Usada por NavMeshAgent y potencialmente por MirarHacia
    public float vidaMaxima = 100f;
    [HideInInspector] public float vidaActual; // Oculto en Inspector porque se inicializa en Awake
    public int equipoID = 1; // 1 para equipo del jugador, 2+ para enemigos u otros equipos

    [Header("Selecci�n Visual")]
    public GameObject indicadorSeleccion; // GameObject hijo que se activa/desactiva al seleccionar
    [HideInInspector] public bool estaSeleccionada = false;

    protected NavMeshAgent navMeshAgent;

    // --- VARIABLES Y M�TODOS EST�TICOS (Para gesti�n global) ---
    public static List<Unidad> unidadesSeleccionadas = new List<Unidad>();
    public static List<Unidad> todasLasUnidadesActivas = new List<Unidad>();
    public static List<NodoDeRecurso> todosLosNodosDeRecurso = new List<NodoDeRecurso>(); // Para que los trabajadores los encuentren

    // Referencias globales asignadas por ControladorInputGlobal.cs
    public static Camera camaraPrincipal;
    public static LayerMask capaSuelo;
    public static LayerMask capaUnidades;
    public static LayerMask capaNodosRecursos; // Para interactuar con nodos de recursos
    public static int equipoDelJugador = 1; // ID del equipo que el jugador controla

    // Estado para la selecci�n por caja (drag selection)
    public static bool estaArrastrandoCaja = false;
    public static Vector2 posicionInicioArrastreCaja;
    protected static float umbralMinimoArrastreSqr = 5f * 5f; // Umbral (al cuadrado) para diferenciar clic de arrastre

    // --- M�TODOS DE CICLO DE VIDA DE UNITY (Virtuales para ser extendidos) ---
    protected virtual void Awake()
    {
        // Debug.Log($"--- UNIDAD AWAKE: {gameObject.name} --- INICIO AWAKE");
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = velocidadMovimiento;
            navMeshAgent.angularSpeed = velocidadAngular;
        }
        else
        {
            Debug.LogError($"Componente NavMeshAgent no encontrado en {gameObject.name}! La unidad no podr� moverse.", this);
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
        // Debug.Log($"--- UNIDAD AWAKE: {gameObject.name} --- FIN AWAKE. Vida: {vidaActual}");
    }

    protected virtual void OnDestroy()
    {
        // Debug.Log($"--- UNIDAD ONDESTROY: {gameObject.name} ---");
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
        // Debug.Log($"--- UNIDAD UPDATE: {gameObject.name} ---"); // Puede ser muy verboso
        if (indicadorSeleccion != null)
        {
            indicadorSeleccion.SetActive(estaSeleccionada);
        }
    }

    // --- M�TODOS DE ACCI�N DE UNIDAD (Virtuales) ---
    public virtual void MoverA(Vector3 destino)
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(destino);
        }
    }

    public virtual void RecibirDano(float cantidad)
    {
        Debug.Log($"[UNIDAD BASE] {gameObject.name} (Equipo {equipoID}) - RecibirDano LLAMADO. Vida ANTES: {vidaActual}, Da�o bruto: {cantidad}");
        if (vidaActual <= 0)
        {
            Debug.Log($"[UNIDAD BASE] {gameObject.name} ya est� muerta/destruida. No se aplica m�s da�o.");
            return;
        }

        vidaActual -= cantidad; // �AQU� ES DONDE LA VIDA DEBER�A BAJAR!
        Debug.Log($"[UNIDAD BASE] {gameObject.name} - Vida DESPU�S de restar da�o: {vidaActual}. (Cantidad restada: {cantidad})");

        if (vidaActual <= 0)
        {
            vidaActual = 0;
            Debug.Log($"[UNIDAD BASE] {gameObject.name} - Vida lleg� a 0 o menos. Llamando a Morir().");
            Morir();
        }
    }

    protected virtual void Morir()
    {
        // Debug.Log($"{gameObject.name} (Equipo {equipoID}) ha muerto. Destruyendo objeto.");
        Destroy(gameObject);
    }

    public virtual void Atacar(Unidad objetivo)
    {
        Debug.LogWarning($"{gameObject.name} (Clase Unidad Base) no tiene un m�todo 'Atacar' espec�fico implementado. Objetivo: {objetivo?.name}");
    }

    public virtual void ComandoInteraccionNodoRecurso(NodoDeRecurso nodo)
    {
        Debug.LogWarning($"{gameObject.name} (Clase Unidad Base) no sabe c�mo interactuar con nodos de recurso. Nodo: {nodo?.name}");
        // Comportamiento base podr�a ser moverse al nodo:
        // if (nodo != null) MoverA(nodo.transform.position);
    }

    public virtual void UsarHabilidadEspecial(int numeroHabilidad, Unidad objetivoUnidad = null, Vector3 posicionObjetivoSuelo = default)
    {
        Debug.LogWarning($"{gameObject.name} (Clase Unidad Base) no tiene una habilidad especial N�{numeroHabilidad} implementada.");
    }

    // --- M�TODOS PARA GESTIONAR EL ESTADO DE SELECCI�N (individual) ---
    public void SeleccionarEstaUnidad() { estaSeleccionada = true; }
    public void DeseleccionarEstaUnidad() { estaSeleccionada = false; }


    // --- M�TODOS EST�TICOS (L�gica de Control Global para Selecci�n e Input) ---
    public static void ProcesarInputGlobal()
    {
        if (camaraPrincipal == null) return;

        // Inicio de Selecci�n por Clic o Arrastre
        if (Input.GetMouseButtonDown(0))
        {
            estaArrastrandoCaja = true;
            posicionInicioArrastreCaja = Input.mousePosition;
        }

        // Fin de Selecci�n por Clic o Arrastre
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
                    // Raycast para unidades (prioridad si se clickea una unidad)
                    if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaUnidades))
                    {
                        Unidad unidadClickeada = hitInfo.collider.GetComponent<Unidad>();
                        if (unidadClickeada != null && unidadClickeada.equipoID == equipoDelJugador)
                        {
                            if (!shiftPresionado) { DeseleccionarTodasLasUnidadesStatic(); SeleccionarUnidadStatic(unidadClickeada); }
                            else { if (!unidadesSeleccionadas.Contains(unidadClickeada)) { SeleccionarUnidadStatic(unidadClickeada); } else { DeseleccionarUnidadStatic(unidadClickeada); } }
                        }
                        // Si se clickea una unidad enemiga o no seleccionable, no hacer nada aqu� (el clic derecho se encarga de atacar)
                    }
                    // Si no se golpe� una unidad, y no se mantiene Shift, y se golpea el suelo -> deseleccionar
                    else if (!shiftPresionado && Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaSuelo))
                    {
                        DeseleccionarTodasLasUnidadesStatic();
                    }
                }
                else // Selecci�n por Caja (Arrastre)
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

        // �rdenes de Clic Derecho
        if (Input.GetMouseButtonDown(1) && unidadesSeleccionadas.Count > 0)
        {
            Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            // Prioridad 1: �Clic en un Nodo de Recurso?
            if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaNodosRecursos))
            {
                NodoDeRecurso nodoDetectado = hitInfo.collider.GetComponent<NodoDeRecurso>();
                if (nodoDetectado != null && nodoDetectado.CantidadActual > 0) // Asegurarse que el nodo tenga recursos
                {
                    // Debug.Log($"Input: Comando INTERACTUAR con NodoDeRecurso {nodoDetectado.name}");
                    foreach (Unidad uSeleccionada in unidadesSeleccionadas)
                    {
                        uSeleccionada.ComandoInteraccionNodoRecurso(nodoDetectado);
                    }
                    return;
                }
            }

            // Prioridad 2: �Clic en una unidad enemiga?
            if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaUnidades))
            {
                Unidad unidadObjetivo = hitInfo.collider.GetComponent<Unidad>();
                if (unidadObjetivo != null && unidadObjetivo.equipoID != equipoDelJugador)
                {
                    // Debug.Log($"Input: Comando ATACAR a {unidadObjetivo.name}");
                    foreach (Unidad uSeleccionada in unidadesSeleccionadas)
                    {
                        if (unidadObjetivo.equipoID != uSeleccionada.equipoID) uSeleccionada.Atacar(unidadObjetivo);
                    }
                    return;
                }
                // Si es unidad aliada, se podr�a implementar un "seguir" o "proteger" aqu�,
                // o simplemente dejar que pase a la l�gica de mover al suelo.
            }

            // Prioridad 3: �Clic en el suelo?
            if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaSuelo))
            {
                // Debug.Log($"Input: Comando MOVER a {hitInfo.point}");
                MoverUnidadesSeleccionadas(hitInfo.point);
                return;
            }
        }

        // Habilidad Especial (ej. Tecla 'Q' + Clic Izquierdo)
        if (Input.GetKeyDown(KeyCode.Q) && Input.GetMouseButtonDown(0) && unidadesSeleccionadas.Count > 0)
        {
            Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            Unidad unidadHabilidadObjetivo = null;
            Vector3 posicionHabilidadObjetivo = default;
            bool objetivoHabilidadEncontrado = false;

            // Priorizar objetivo de unidad para la habilidad
            if (Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaUnidades))
            {
                unidadHabilidadObjetivo = hitInfo.collider.GetComponent<Unidad>();
                if (unidadHabilidadObjetivo != null)
                {
                    posicionHabilidadObjetivo = unidadHabilidadObjetivo.transform.position; // Usar posici�n de la unidad como fallback
                    objetivoHabilidadEncontrado = true;
                }
            }
            // Si no se golpe� una unidad, verificar si se golpe� el suelo
            if (!objetivoHabilidadEncontrado && Physics.Raycast(rayo, out hitInfo, Mathf.Infinity, capaSuelo))
            {
                posicionHabilidadObjetivo = hitInfo.point;
                objetivoHabilidadEncontrado = true;
            }

            if (objetivoHabilidadEncontrado)
            {
                // Debug.Log($"Input: Comando HABILIDAD (0) sobre {(unidadHabilidadObjetivo != null ? unidadHabilidadObjetivo.name : posicionHabilidadObjetivo.ToString())}");
                foreach (Unidad uSeleccionada in unidadesSeleccionadas)
                {
                    uSeleccionada.UsarHabilidadEspecial(0, unidadHabilidadObjetivo, posicionHabilidadObjetivo);
                }
            }
        }
    }

    // --- M�todos Est�ticos Protegidos para Gesti�n de Selecci�n ---
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

    // --- M�todo Est�tico Protegido para Mover Grupo ---
    protected static void MoverUnidadesSeleccionadas(Vector3 centroDestino)
    {
        int count = unidadesSeleccionadas.Count;
        if (count == 0) return;

        float espaciado = 2.0f;
        Vector3 direccionFormacion = camaraPrincipal != null ? camaraPrincipal.transform.right : Vector3.right;

        if (count == 1 && unidadesSeleccionadas[0] != null) { unidadesSeleccionadas[0].MoverA(centroDestino); return; }

        Vector3 inicioFormacion = centroDestino - direccionFormacion * (espaciado * (count - 1) / 2f);
        for (int i = 0; i < count; i++)
        {
            if (unidadesSeleccionadas[i] != null)
            {
                Vector3 posicionUnidadEnFormacion = inicioFormacion + direccionFormacion * (i * espaciado);
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(posicionUnidadEnFormacion, out navHit, espaciado, NavMesh.AllAreas))
                { unidadesSeleccionadas[i].MoverA(navHit.position); }
                else { unidadesSeleccionadas[i].MoverA(centroDestino); }
            }
        }
    }

    // --- Helper Est�tico para Rect�ngulo de Selecci�n ---
    public static Rect ObtenerRectanguloDePantalla(Vector2 p1Mouse, Vector2 p2Mouse)
    {
        Vector2 p1GUI = new Vector2(p1Mouse.x, Screen.height - p1Mouse.y);
        Vector2 p2GUI = new Vector2(p2Mouse.x, Screen.height - p2Mouse.y);
        float xMin = Mathf.Min(p1GUI.x, p2GUI.x); float xMax = Mathf.Max(p1GUI.x, p2GUI.x);
        float yMin = Mathf.Min(p1GUI.y, p2GUI.y); float yMax = Mathf.Max(p1GUI.y, p2GUI.y);
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }
}