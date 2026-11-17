
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Controlador_Tiempo : MonoBehaviour
{
    public const string ClaveDuracionPartida = "DuracionPartida";
    public const float DuracionPredeterminadaSegundos = 300f;

    private static Sprite spriteVida;

    [Header("Temporizador")]
    public TMP_Text textoTemporizador;
    public float tiempoInicial = 300f;

    private float tiempoRestante;
    private bool partidaActiva = true;

    [Header("Contador de muertes")]
    public TMP_Text textoMuertesJugador1;
    public TMP_Text textoMuertesJugador2;

    [Header("Escenarios")]
    public Sprite[] fondosEscenario;

    private int killsJugador1 = 0;
    private int killsJugador2 = 0;

    public static Controlador_Tiempo Instance { get; private set; }
    public static bool JuegoEnCurso => Instance != null && Instance.partidaActiva;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        tiempoInicial = PlayerPrefs.GetFloat(ClaveDuracionPartida, tiempoInicial);
        tiempoRestante = tiempoInicial;

        ActualizarTemporizador();
        ActualizarMuertes();
        AplicarEscenarioSeleccionado();
        ConfigurarBarrasVida();
    }

    void Update()
    {
        if (!partidaActiva)
            return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            partidaActiva = false;

            Debug.Log("¡Tiempo terminado!");
            ActualizarTemporizador();
            CargarPantallaFinal();
            return;
        }

        ActualizarTemporizador();
    }

    private void CargarPantallaFinal()
    {
        Vida_Jugador jugador1 = null;
        Vida_Jugador jugador2 = null;
        foreach (Vida_Jugador jugador in FindObjectsOfType<Vida_Jugador>())
        {
            if (jugador.numeroJugador == 1)
                jugador1 = jugador;
            else if (jugador.numeroJugador == 2)
                jugador2 = jugador;
        }

        if (jugador1 == null || jugador2 == null)
        {
            Debug.LogError("No se pudieron encontrar las vidas de ambos jugadores para decidir el resultado.", this);
            return;
        }

        int ganador;
        if (killsJugador1 != killsJugador2)
        {
            ganador = killsJugador1 > killsJugador2 ? 1 : 2;
        }
        else if (!Mathf.Approximately(jugador1.vidaActual, jugador2.vidaActual))
        {
            ganador = jugador1.vidaActual > jugador2.vidaActual ? 1 : 2;
        }
        else
        {
            ganador = 1;
            Debug.LogWarning("La partida terminó empatada; se mostrará la pantalla final del jugador 1.", this);
        }

        string escenaFinal = ganador == 1
            ? "ESCENA_VICTORIA_JUGADOR_1"
            : "ESCENA_VICTORIA_JUGADOR_2";
        if (!Application.CanStreamedLevelBeLoaded(escenaFinal))
        {
            Debug.LogError($"No se puede cargar la escena final '{escenaFinal}'.", this);
            return;
        }

        SceneManager.LoadScene(escenaFinal);
    }

    public void RegistrarMuerte(int numeroJugadorAsesino)
    {
        if (!partidaActiva || (numeroJugadorAsesino != 1 && numeroJugadorAsesino != 2))
            return;

        if (numeroJugadorAsesino == 1)
        {
            killsJugador1++;
        }
        else
        {
            killsJugador2++;
        }

        ActualizarMuertes();
    }

    void ActualizarTemporizador()
    {
        if (textoTemporizador != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);

            textoTemporizador.text =
                string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    void ActualizarMuertes()
    {
        if (textoMuertesJugador1 != null)
        {
            textoMuertesJugador1.text =
                "Jugador 1: " + killsJugador1;
        }

        if (textoMuertesJugador2 != null)
        {
            textoMuertesJugador2.text =
                "Jugador 2: " + killsJugador2;
        }
    }

    private void AplicarEscenarioSeleccionado()
    {
        int escenario = PlayerPrefs.GetInt(
            SELECCION_ESCENARIO.ClaveEscenarioSeleccionado,
            -1);
        if (escenario < 0)
            return;

        if (fondosEscenario == null || escenario >= fondosEscenario.Length ||
            fondosEscenario[escenario] == null)
        {
            Debug.LogError($"No hay un fondo asignado para el escenario {escenario}.", this);
            return;
        }

        GameObject objetoFondo = GameObject.Find("FONDO");
        SpriteRenderer rendererFondo = objetoFondo != null
            ? objetoFondo.GetComponent<SpriteRenderer>()
            : null;
        if (rendererFondo == null)
        {
            Debug.LogError("No se encontró el SpriteRenderer del objeto FONDO.", this);
            return;
        }

        rendererFondo.sprite = fondosEscenario[escenario];
    }

    private void ConfigurarBarrasVida()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No se encontró un Canvas para mostrar las barras de vida.", this);
            return;
        }

        Vida_Jugador[] jugadores = FindObjectsOfType<Vida_Jugador>();
        foreach (Vida_Jugador jugador in jugadores)
        {
            string nombreBarra = "BarraVida_Jugador" + jugador.numeroJugador;
            Transform existente = canvas.transform.Find(nombreBarra);
            Slider barra = existente != null
                ? existente.GetComponent<Slider>()
                : CrearBarraVida(canvas.transform, nombreBarra, jugador.numeroJugador);

            if (barra == null)
            {
                Debug.LogError($"No se pudo crear la barra de vida de {jugador.name}.", jugador);
                continue;
            }

            barra.maxValue = jugador.vidaMaxima;
            jugador.AsignarBarraVida(barra);
        }
    }

    private Slider CrearBarraVida(Transform padre, string nombre, int numeroJugador)
    {
        Sprite spriteUI = ObtenerSpriteVida();

        GameObject objetoBarra = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Slider));
        objetoBarra.transform.SetParent(padre, false);

        RectTransform rectBarra = objetoBarra.GetComponent<RectTransform>();
        rectBarra.anchorMin = new Vector2(numeroJugador == 1 ? 0.03f : 0.97f, 1f);
        rectBarra.anchorMax = rectBarra.anchorMin;
        rectBarra.pivot = new Vector2(numeroJugador == 1 ? 0f : 1f, 1f);
        rectBarra.anchoredPosition = new Vector2(0f, -28f);
        rectBarra.sizeDelta = new Vector2(220f, 16f);

        Image fondo = objetoBarra.GetComponent<Image>();
        fondo.sprite = spriteUI;
        fondo.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);
        fondo.raycastTarget = false;

        GameObject objetoRelleno = new GameObject(
            "Relleno",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        objetoRelleno.transform.SetParent(objetoBarra.transform, false);

        RectTransform rectRelleno = objetoRelleno.GetComponent<RectTransform>();
        rectRelleno.anchorMin = Vector2.zero;
        rectRelleno.anchorMax = Vector2.one;
        rectRelleno.offsetMin = new Vector2(2f, 2f);
        rectRelleno.offsetMax = new Vector2(-2f, -2f);

        Image imagenRelleno = objetoRelleno.GetComponent<Image>();
        imagenRelleno.sprite = spriteUI;
        imagenRelleno.type = Image.Type.Filled;
        imagenRelleno.fillMethod = Image.FillMethod.Horizontal;
        imagenRelleno.fillOrigin = (int)Image.OriginHorizontal.Left;
        imagenRelleno.color = numeroJugador == 1
            ? new Color(0.2f, 0.85f, 0.25f, 1f)
            : new Color(0.9f, 0.2f, 0.2f, 1f);
        imagenRelleno.raycastTarget = false;

        Slider barra = objetoBarra.GetComponent<Slider>();
        barra.minValue = 0f;
        barra.maxValue = 100f;
        barra.direction = Slider.Direction.LeftToRight;
        barra.fillRect = rectRelleno;
        barra.targetGraphic = fondo;
        barra.interactable = false;
        barra.transition = Selectable.Transition.None;
        barra.navigation = new Navigation { mode = Navigation.Mode.None };
        return barra;
    }

    private static Sprite ObtenerSpriteVida()
    {
        if (spriteVida == null)
        {
            Texture2D textura = Texture2D.whiteTexture;
            spriteVida = Sprite.Create(
                textura,
                new Rect(0f, 0f, textura.width, textura.height),
                new Vector2(0.5f, 0.5f));
        }

        return spriteVida;
    }
}
