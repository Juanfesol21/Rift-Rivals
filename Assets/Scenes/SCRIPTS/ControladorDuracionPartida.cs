using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class ControladorDuracionPartida : MonoBehaviour
{
    private const float DuracionMinima = 60f;
    private const float DuracionMaxima = 900f;

    private Text etiquetaDuracion;

    private void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("ControladorDuracionPartida necesita estar en un Canvas.", this);
            enabled = false;
            return;
        }

        canvas.transform.localScale = Vector3.one;
        CrearControl(canvas.transform);
        CrearBotonVolver(canvas.transform);
        CrearBotonJugar(canvas.transform);
    }

    private void CrearControl(Transform padre)
    {
        Transform etiquetaExistente = padre.Find("Etiqueta duracion partida");
        Transform sliderExistente = padre.Find("Barra duracion partida");
        if (etiquetaExistente != null && sliderExistente != null)
        {
            etiquetaDuracion = etiquetaExistente.GetComponent<Text>();
            Slider sliderExistenteComponente = sliderExistente.GetComponent<Slider>();
            if (etiquetaDuracion == null || sliderExistenteComponente == null)
            {
                Debug.LogError("El control de duracion existente esta incompleto.", this);
                return;
            }

            sliderExistenteComponente.onValueChanged.RemoveListener(GuardarDuracion);
            sliderExistenteComponente.onValueChanged.AddListener(GuardarDuracion);
            float duracionActual = PlayerPrefs.GetFloat(
                Controlador_Tiempo.ClaveDuracionPartida,
                Controlador_Tiempo.DuracionPredeterminadaSegundos);
            sliderExistenteComponente.SetValueWithoutNotify(
                Mathf.Clamp(duracionActual, DuracionMinima, DuracionMaxima));
            GuardarDuracion(sliderExistenteComponente.value);
            return;
        }

        GameObject objetoEtiqueta = new GameObject(
            "Etiqueta duracion partida",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text));
        objetoEtiqueta.transform.SetParent(padre, false);

        RectTransform rectEtiqueta = objetoEtiqueta.GetComponent<RectTransform>();
        rectEtiqueta.anchorMin = new Vector2(0.5f, 0.5f);
        rectEtiqueta.anchorMax = new Vector2(0.5f, 0.5f);
        rectEtiqueta.pivot = new Vector2(0.5f, 0.5f);
        rectEtiqueta.anchoredPosition = new Vector2(0f, -135f);
        rectEtiqueta.sizeDelta = new Vector2(440f, 32f);

        etiquetaDuracion = objetoEtiqueta.GetComponent<Text>();
        etiquetaDuracion.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        etiquetaDuracion.fontSize = 22;
        etiquetaDuracion.fontStyle = FontStyle.Bold;
        etiquetaDuracion.alignment = TextAnchor.MiddleCenter;
        etiquetaDuracion.color = Color.white;
        etiquetaDuracion.raycastTarget = false;

        GameObject objetoSlider = new GameObject(
            "Barra duracion partida",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Slider));
        objetoSlider.transform.SetParent(padre, false);

        RectTransform rectSlider = objetoSlider.GetComponent<RectTransform>();
        rectSlider.anchorMin = new Vector2(0.5f, 0.5f);
        rectSlider.anchorMax = new Vector2(0.5f, 0.5f);
        rectSlider.pivot = new Vector2(0.5f, 0.5f);
        rectSlider.anchoredPosition = new Vector2(0f, -180f);
        rectSlider.sizeDelta = new Vector2(360f, 30f);

        Image fondo = objetoSlider.GetComponent<Image>();
        fondo.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
        fondo.raycastTarget = true;

        RectTransform areaRelleno = CrearParteSlider(
            "Area de relleno",
            rectSlider,
            Color.clear,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(8f, 6f),
            new Vector2(-8f, -6f));
        RectTransform relleno = CrearParteSlider(
            "Relleno",
            areaRelleno,
            new Color(0.12f, 0.55f, 0.95f, 1f),
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero);

        RectTransform areaControlador = CrearParteSlider(
            "Area controlador",
            rectSlider,
            Color.clear,
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(8f, 6f),
            new Vector2(-8f, -6f));
        RectTransform control = CrearParteSlider(
            "Controlador",
            areaControlador,
            Color.white,
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(-10f, -10f),
            new Vector2(10f, 10f));

        Slider slider = objetoSlider.GetComponent<Slider>();
        slider.minValue = DuracionMinima;
        slider.maxValue = DuracionMaxima;
        slider.wholeNumbers = true;
        slider.direction = Slider.Direction.LeftToRight;
        slider.fillRect = relleno;
        slider.handleRect = control;
        slider.targetGraphic = control.GetComponent<Image>();
        slider.transition = Selectable.Transition.None;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };
        slider.onValueChanged.AddListener(GuardarDuracion);

        float duracionGuardada = PlayerPrefs.GetFloat(
            Controlador_Tiempo.ClaveDuracionPartida,
            Controlador_Tiempo.DuracionPredeterminadaSegundos);
        slider.SetValueWithoutNotify(Mathf.Clamp(duracionGuardada, DuracionMinima, DuracionMaxima));
        GuardarDuracion(slider.value);
        ActualizarEtiqueta(slider.value);
    }

    private void CrearBotonJugar(Transform padre)
    {
        Transform botonExistente = padre.Find("Boton jugar");
        if (botonExistente != null)
        {
            Button botonExistenteComponente = botonExistente.GetComponent<Button>();
            if (botonExistenteComponente == null)
            {
                Debug.LogError("El boton JUGAR existente no tiene un componente Button.", this);
                return;
            }

            botonExistenteComponente.onClick.RemoveListener(IniciarPartida);
            botonExistenteComponente.onClick.AddListener(IniciarPartida);
            return;
        }

        GameObject objetoBoton = new GameObject(
            "Boton jugar",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        objetoBoton.transform.SetParent(padre, false);

        RectTransform rectBoton = objetoBoton.GetComponent<RectTransform>();
        rectBoton.anchorMin = new Vector2(0.5f, 0.5f);
        rectBoton.anchorMax = new Vector2(0.5f, 0.5f);
        rectBoton.pivot = new Vector2(0.5f, 0.5f);
        rectBoton.anchoredPosition = new Vector2(120f, -205f);
        rectBoton.sizeDelta = new Vector2(200f, 48f);

        Image fondo = objetoBoton.GetComponent<Image>();
        fondo.color = new Color(0.12f, 0.55f, 0.95f, 1f);

        Button boton = objetoBoton.GetComponent<Button>();
        boton.targetGraphic = fondo;
        boton.onClick.AddListener(IniciarPartida);

        GameObject objetoTexto = new GameObject(
            "Texto",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text));
        objetoTexto.transform.SetParent(objetoBoton.transform, false);

        RectTransform rectTexto = objetoTexto.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;

        Text texto = objetoTexto.GetComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        texto.fontSize = 24;
        texto.fontStyle = FontStyle.Bold;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = Color.white;
        texto.raycastTarget = false;
        texto.text = "JUGAR";
    }

    private void CrearBotonVolver(Transform padre)
    {
        Transform botonExistente = padre.Find("Boton volver");
        if (botonExistente != null)
        {
            Button botonExistenteComponente = botonExistente.GetComponent<Button>();
            if (botonExistenteComponente == null)
            {
                Debug.LogError("El boton VOLVER existente no tiene un componente Button.", this);
                return;
            }

            botonExistenteComponente.onClick.RemoveListener(VolverAlMenu);
            botonExistenteComponente.onClick.AddListener(VolverAlMenu);
            return;
        }

        GameObject objetoBoton = new GameObject(
            "Boton volver",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        objetoBoton.transform.SetParent(padre, false);

        RectTransform rectBoton = objetoBoton.GetComponent<RectTransform>();
        rectBoton.anchorMin = new Vector2(0.5f, 0.5f);
        rectBoton.anchorMax = new Vector2(0.5f, 0.5f);
        rectBoton.pivot = new Vector2(0.5f, 0.5f);
        rectBoton.anchoredPosition = new Vector2(-120f, -205f);
        rectBoton.sizeDelta = new Vector2(200f, 48f);

        Image fondo = objetoBoton.GetComponent<Image>();
        fondo.color = new Color(0.25f, 0.25f, 0.3f, 1f);

        Button boton = objetoBoton.GetComponent<Button>();
        boton.targetGraphic = fondo;
        boton.onClick.AddListener(VolverAlMenu);

        GameObject objetoTexto = new GameObject(
            "Texto",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Text));
        objetoTexto.transform.SetParent(objetoBoton.transform, false);

        RectTransform rectTexto = objetoTexto.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;

        Text texto = objetoTexto.GetComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        texto.fontSize = 24;
        texto.fontStyle = FontStyle.Bold;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = Color.white;
        texto.raycastTarget = false;
        texto.text = "VOLVER";
    }

    private RectTransform CrearParteSlider(
        string nombre,
        RectTransform padre,
        Color color,
        Vector2 anclaMinima,
        Vector2 anclaMaxima,
        Vector2 margenMinimo,
        Vector2 margenMaximo)
    {
        GameObject objeto = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        objeto.transform.SetParent(padre, false);

        RectTransform rect = objeto.GetComponent<RectTransform>();
        rect.anchorMin = anclaMinima;
        rect.anchorMax = anclaMaxima;
        rect.offsetMin = margenMinimo;
        rect.offsetMax = margenMaximo;

        Image imagen = objeto.GetComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = false;
        return rect;
    }

    private void GuardarDuracion(float segundos)
    {
        PlayerPrefs.SetFloat(Controlador_Tiempo.ClaveDuracionPartida, segundos);
        PlayerPrefs.Save();
        ActualizarEtiqueta(segundos);
    }

    private void IniciarPartida()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("ESCENA_ESCENARIOS");
    }

    private void VolverAlMenu()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("ESCENA_MENU_PRINCIPAL");
    }

    private void ActualizarEtiqueta(float segundos)
    {
        int totalSegundos = Mathf.RoundToInt(segundos);
        etiquetaDuracion.text = string.Format(
            "Duracion de partida: {0:00}:{1:00}",
            totalSegundos / 60,
            totalSegundos % 60);
    }
}
