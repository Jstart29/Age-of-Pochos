using UnityEngine;

public class ControladorInputGlobal : MonoBehaviour
{
    public Camera camaraPrincipalRef;
    public LayerMask capaSueloRef;
    public LayerMask capaUnidadesRef;
    public int equipoDelJugadorActual = 1;
    public Texture2D texturaCajaSeleccion;

    // Inicializa las referencias globales para la clase Unidad y configura la textura de selección.
    void Start()
    {
        if (camaraPrincipalRef != null)
        {
            Unidad.camaraPrincipal = camaraPrincipalRef;
        }
        else
        {
            Debug.LogWarning("ControladorInputGlobal: Cámara Principal no asignada. Intentando usar Camera.main.");
            Unidad.camaraPrincipal = Camera.main;
            if (Unidad.camaraPrincipal == null)
            {
                Debug.LogError("ControladorInputGlobal: No se pudo encontrar Camera.main. Asigna una cámara.");
            }
        }
        Unidad.capaSuelo = capaSueloRef;
        Unidad.capaUnidades = capaUnidadesRef;
        Unidad.equipoDelJugador = equipoDelJugadorActual;

        if (texturaCajaSeleccion == null)
        {
            texturaCajaSeleccion = new Texture2D(1, 1);
            texturaCajaSeleccion.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            texturaCajaSeleccion.Apply();
        }
    }

    // Llama al método de procesamiento de input global de la clase Unidad en cada frame.
    void Update()
    {
        Unidad.ProcesarInputGlobal();
    }

    // Dibuja la caja de selección en la interfaz gráfica si el jugador está arrastrando el mouse.
    void OnGUI()
    {
        if (Unidad.estaArrastrandoCaja)
        {
            Rect rect = Unidad.ObtenerRectanguloDePantalla(Unidad.posicionInicioArrastreCaja, Input.mousePosition);
            if (texturaCajaSeleccion != null)
            {
                GUI.DrawTexture(rect, texturaCajaSeleccion, ScaleMode.StretchToFill);
            }
            else
            {
                GUI.Box(rect, "");
            }
            GUIStyle style = new GUIStyle();
            style.normal.background = Texture2D.whiteTexture;
            Color oldColor = GUI.color;
            GUI.color = new Color(0.8f, 0.8f, 0.95f, 0.7f);
            GUI.Box(new Rect(rect.xMin, rect.yMin, rect.width, 1), GUIContent.none, style);
            GUI.Box(new Rect(rect.xMin, rect.yMax - 1, rect.width, 1), GUIContent.none, style);
            GUI.Box(new Rect(rect.xMin, rect.yMin, 1, rect.height), GUIContent.none, style);
            GUI.Box(new Rect(rect.xMax - 1, rect.yMin, 1, rect.height), GUIContent.none, style);
            GUI.color = oldColor;
        }
    }
}