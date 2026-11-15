using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultadoJugador2UI : MonoBehaviour
{
    private void Start()
    {
        Canvas canvas = new GameObject(
            "Resultado UI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        CrearTexto(canvas.transform, "Jugador 2 gana", new Vector2(0f, 100f), 48);
        CrearBotonMenu(canvas.transform);

        if (EventSystem.current == null)
        {
            GameObject eventSystem = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            eventSystem.transform.SetAsLastSibling();
        }
    }

    private static void CrearTexto(Transform padre, string contenido, Vector2 posicion, int tamano)
    {
        GameObject objeto = new GameObject(
            "Resultado",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = new Vector2(700f, 100f);

        Text texto = objeto.GetComponent<Text>();
        texto.text = contenido;
        texto.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        texto.fontSize = tamano;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = Color.white;
    }

    private static void CrearBotonMenu(Transform padre)
    {
        GameObject objeto = new GameObject(
            "Volver al menu principal",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 80f);
        rect.sizeDelta = new Vector2(320f, 64f);

        Image imagen = objeto.GetComponent<Image>();
        imagen.color = new Color(0.12f, 0.25f, 0.45f, 1f);

        Button boton = objeto.GetComponent<Button>();
        boton.targetGraphic = imagen;
        boton.onClick.AddListener(() =>
        {
            const string escenaMenu = "ESCENA_MENU_PRINCIPAL";
            if (!Application.CanStreamedLevelBeLoaded(escenaMenu))
            {
                Debug.LogError($"No se puede cargar la escena '{escenaMenu}'.");
                return;
            }

            SceneManager.LoadScene(escenaMenu);
        });

        CrearTexto(objeto.transform, "VOLVER AL MENU PRINCIPAL", Vector2.zero, 22);
    }
}
