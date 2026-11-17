using UnityEngine;
using UnityEngine.SceneManagement;

public class SELECCION_ESCENARIO : MonoBehaviour
{
    public const string ClaveEscenarioSeleccionado = "EscenarioSeleccionado";

    public string[] escenarios = new string[4];
    private int escenarioSeleccionado = -1;

    public void SeleccionarEscenario(int numero)
    {
        if (escenarios == null || numero < 0 || numero >= escenarios.Length)
        {
            Debug.LogError($"El indice de escenario no es valido: {numero}.", this);
            return;
        }

        escenarioSeleccionado = numero;
        PlayerPrefs.SetInt(ClaveEscenarioSeleccionado, numero);
        Debug.Log("Escenario seleccionado: " + numero, this);
    }

    public void ConfirmarEscenario()
    {
        if (escenarioSeleccionado < 0 ||
            escenarios == null ||
            escenarioSeleccionado >= escenarios.Length)
        {
            Debug.LogWarning("Primero debes seleccionar un escenario.", this);
            return;
        }

        string escenaDestino = escenarios[escenarioSeleccionado];
        if (string.IsNullOrWhiteSpace(escenaDestino) ||
            !Application.CanStreamedLevelBeLoaded(escenaDestino))
        {
            Debug.LogError($"No se puede cargar la escena '{escenaDestino}'.", this);
            return;
        }

        PlayerPrefs.SetInt(ClaveEscenarioSeleccionado, escenarioSeleccionado);
        PlayerPrefs.Save();
        SceneManager.LoadScene(escenaDestino);
    }
}