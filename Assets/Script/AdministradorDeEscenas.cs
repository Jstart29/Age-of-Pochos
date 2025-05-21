using UnityEngine;
using UnityEngine.SceneManagement;

public class AdministradorDeEscenas : MonoBehaviour
{
    public static bool transicionEnProgreso = false;

    public const string NOMBRE_ESCENA_VICTORIA = "Ganaste";
    public const string NOMBRE_ESCENA_DERROTA = "Perdiste";

    public static void CargarEscenaVictoria()
    {
        if (transicionEnProgreso) return;
        transicionEnProgreso = true;
        SceneManager.LoadScene(NOMBRE_ESCENA_VICTORIA);
    }

    public static void CargarEscenaDerrota()
    {
        if (transicionEnProgreso) return;
        transicionEnProgreso = true;
        SceneManager.LoadScene(NOMBRE_ESCENA_DERROTA);
    }

    public static void PermitirNuevaTransicionDeEscena()
    {
        transicionEnProgreso = false;
    }
}