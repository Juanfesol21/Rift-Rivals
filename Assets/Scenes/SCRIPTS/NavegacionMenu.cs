using UnityEngine;
using UnityEngine.SceneManagement;

public class NavegacionMenu : MonoBehaviour
{
    public void AbrirConfiguracion()
    {
        SceneManager.LoadScene("ESCENA_CONFIGURACION");
    }

    public void IniciarJuego()
    {
        SceneManager.LoadScene("ESCENA_ESCENARIOS");
    }
}
