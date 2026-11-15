
using UnityEngine;
using UnityEngine.SceneManagement;

public class MENU_PRINCIPAL_2 : MonoBehaviour
{
    public void IniciarJuego()
    {
        SceneManager.LoadScene("ESCENA_CONFIGURACIÓN");
    }
        public void SalirJuego()
    {
        Application.Quit();
    }

}
