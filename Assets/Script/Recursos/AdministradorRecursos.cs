using UnityEngine;
using System.Collections.Generic; // Para el Diccionario

public class AdministradorRecursos : MonoBehaviour
{
    // --- Singleton Pattern ---
    private static AdministradorRecursos _instancia;
    public static AdministradorRecursos Instancia
    {
        get
        {
            if (_instancia == null)
            {
                _instancia = FindObjectOfType<AdministradorRecursos>();
                if (_instancia == null)
                {
                    GameObject singletonObject = new GameObject(typeof(AdministradorRecursos).Name);
                    _instancia = singletonObject.AddComponent<AdministradorRecursos>();
                    Debug.Log("AdministradorRecursos creado automáticamente.");
                }
            }
            return _instancia;
        }
    }

    private Dictionary<int, Dictionary<TipoRecurso, int>> recursosPorEquipo = new Dictionary<int, Dictionary<TipoRecurso, int>>();

    public delegate void ActualizacionDeRecursosHandler(int equipoID, TipoRecurso tipo, int nuevaCantidad);
    public event ActualizacionDeRecursosHandler OnRecursosActualizados;

    protected virtual void Awake()
    {
        if (_instancia != null && _instancia != this)
        {
            Destroy(gameObject); // Asegurar que solo haya una instancia
            return;
        }
        _instancia = this;
        DontDestroyOnLoad(gameObject); 
    }

    // Inicializa los recursos para un equipo específico si aún no existen
    private void AsegurarDiccionarioDeEquipo(int equipoID)
    {
        if (!recursosPorEquipo.ContainsKey(equipoID))
        {
            recursosPorEquipo[equipoID] = new Dictionary<TipoRecurso, int>();
            // Inicializar todos los tipos de recursos a 0 para este nuevo equipo
            foreach (TipoRecurso tipo in System.Enum.GetValues(typeof(TipoRecurso)))
            {
                recursosPorEquipo[equipoID][tipo] = 0;
            }
            Debug.Log($"Recursos inicializados para equipo {equipoID}.");
        }
    }

    public void EstablecerRecursosIniciales(int equipoID, TipoRecurso tipo, int cantidad)
    {
        AsegurarDiccionarioDeEquipo(equipoID);
        recursosPorEquipo[equipoID][tipo] = cantidad;
        OnRecursosActualizados?.Invoke(equipoID, tipo, cantidad);
        // Debug.Log($"Establecidos recursos iniciales para Equipo {equipoID}: {tipo} = {cantidad}");
    }


    public void AnadirRecursos(int equipoID, TipoRecurso tipo, int cantidad)
    {
        if (cantidad <= 0) return;

        AsegurarDiccionarioDeEquipo(equipoID);

        recursosPorEquipo[equipoID][tipo] += cantidad;
        Debug.Log($"Equipo {equipoID} obtuvo {cantidad} de {tipo}. Total ahora: {recursosPorEquipo[equipoID][tipo]}");
        OnRecursosActualizados?.Invoke(equipoID, tipo, recursosPorEquipo[equipoID][tipo]);
    }

    public bool GastarRecursos(int equipoID, TipoRecurso tipo, int cantidad)
    {
        if (cantidad <= 0) return true; // No se gasta nada, se considera exitoso

        AsegurarDiccionarioDeEquipo(equipoID);

        if (recursosPorEquipo[equipoID].ContainsKey(tipo) && recursosPorEquipo[equipoID][tipo] >= cantidad)
        {
            recursosPorEquipo[equipoID][tipo] -= cantidad;
            Debug.Log($"Equipo {equipoID} gastó {cantidad} de {tipo}. Restante: {recursosPorEquipo[equipoID][tipo]}");
            OnRecursosActualizados?.Invoke(equipoID, tipo, recursosPorEquipo[equipoID][tipo]);
            return true;
        }
        else
        {
            Debug.LogWarning($"Equipo {equipoID} no tiene suficientes recursos de {tipo} para gastar {cantidad}. Tiene: {(recursosPorEquipo[equipoID].ContainsKey(tipo) ? recursosPorEquipo[equipoID][tipo] : 0)}");
            return false;
        }
    }

    public int ObtenerCantidadRecurso(int equipoID, TipoRecurso tipo)
    {
        AsegurarDiccionarioDeEquipo(equipoID);

        if (recursosPorEquipo[equipoID].ContainsKey(tipo))
        {
            return recursosPorEquipo[equipoID][tipo];
        }
        return 0;
    }
}