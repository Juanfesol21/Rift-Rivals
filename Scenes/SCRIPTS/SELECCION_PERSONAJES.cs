using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SELECCION_PERSONAJES : MonoBehaviour
{
    public const string ClavePersonajeJugador1 = "PersonajeJugador1";
    public const string ClavePersonajeJugador2 = "PersonajeJugador2";

    private static int personajeJugador1 = -1;
    private static int personajeJugador2 = -1;

    private void Awake()
    {
        Button boton = GetComponent<Button>();
        if (boton == null)
            return;

        if (gameObject.name == "BOTON_JUGADOR_1")
            boton.onClick.AddListener(() => SeleccionarPersonaje(0));
        else if (gameObject.name == "BOTON_JUGADOR_2")
            boton.onClick.AddListener(() => SeleccionarPersonaje(1));
        else if (gameObject.name == "Boton continuar")
        {
            personajeJugador1 = -1;
            personajeJugador2 = -1;
            boton.onClick.AddListener(ConfirmarSeleccion);
        }
    }

    public void SeleccionarPersonaje(int personaje)
    {
        if (personaje < 0 || personaje > 1)
        {
            Debug.LogError($"El personaje seleccionado no es valido: {personaje}.", this);
            return;
        }

        personajeJugador1 = personaje;
        personajeJugador2 = personaje == 0 ? 1 : 0;
        PlayerPrefs.SetInt(ClavePersonajeJugador1, personajeJugador1);
        PlayerPrefs.SetInt(ClavePersonajeJugador2, personajeJugador2);
    }

    public void ConfirmarSeleccion()
    {
        if (personajeJugador1 < 0 || personajeJugador2 < 0)
        {
            Debug.LogWarning("Selecciona un personaje antes de comenzar.", this);
            return;
        }

        const string escenaPartida = "SampleScene";
        if (!Application.CanStreamedLevelBeLoaded(escenaPartida))
        {
            Debug.LogError($"No se puede cargar la escena '{escenaPartida}'.", this);
            return;
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene(escenaPartida);
    }
}
