
using UnityEngine;
using UnityEngine.SceneManagement;

public class MENU_PRINCIPAL : MonoBehaviour
{
    public void IniciarJuego()
    {
        SceneManager.LoadScene("ESCENA_JUGADORES");
    }
        public void SalirJuego()
    {
        Application.Quit();
    }

}
