using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

public class BOTON_INICIO : MonoBehaviour
{
    private void Awake()
    {
        if (EventSystem.current == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public void IrAlInicio()
    {
        const string escenaMenu = "ESCENA_MENU_PRINCIPAL";
        if (!Application.CanStreamedLevelBeLoaded(escenaMenu))
        {
            Debug.LogError($"No se puede cargar la escena '{escenaMenu}'.", this);
            return;
        }

        SceneManager.LoadScene(escenaMenu);
    }
}