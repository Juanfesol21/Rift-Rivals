
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Vida_Jugador : MonoBehaviour
{
    [Header("Configuracion de vida")]
    public float vidaMaxima = 100f;
    public float vidaActual;

    [Header("Identificacion del jugador")]
    public int numeroJugador = 1;

    [Header("Barra de vida")]
    public Slider barraVida;

    private bool yaMurio = false;
    private int numeroUltimoAtacante;
    private const float TiempoRespawn = 1f;
    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private Vector3 escalaInicial;
    private Rigidbody2D rb;
    private Renderer[] renderers;
    private bool[] estadoRenderers;
    private Collider2D[] colisionadores;
    private bool[] estadoColisionadores;
    private MonoBehaviour[] comportamientos;
    private bool[] estadoComportamientos;

    private void Awake()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        escalaInicial = transform.localScale;
        rb = GetComponent<Rigidbody2D>();

        renderers = GetComponentsInChildren<Renderer>(true);
        estadoRenderers = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            estadoRenderers[i] = renderers[i] != null && renderers[i].enabled;

        colisionadores = GetComponentsInChildren<Collider2D>(true);
        estadoColisionadores = new bool[colisionadores.Length];
        for (int i = 0; i < colisionadores.Length; i++)
            estadoColisionadores[i] = colisionadores[i] != null && colisionadores[i].enabled;

        comportamientos = GetComponentsInChildren<MonoBehaviour>(true);
        estadoComportamientos = new bool[comportamientos.Length];
        for (int i = 0; i < comportamientos.Length; i++)
            estadoComportamientos[i] = comportamientos[i] != null && comportamientos[i].enabled;
    }

    void Start()
    {
        vidaActual = vidaMaxima;

        ActualizarBarra();
    }

    // Recibir daño
    public void RecibirDano(float cantidad)
    {
        AplicarDano(cantidad, 0);
    }

    public void RecibirDano(float cantidad, Transform atacante)
    {
        Vida_Jugador vidaAtacante = atacante != null
            ? atacante.GetComponentInParent<Vida_Jugador>()
            : null;
        int numeroJugadorAsesino = vidaAtacante != null && vidaAtacante != this
            ? vidaAtacante.numeroJugador
            : 0;

        AplicarDano(cantidad, numeroJugadorAsesino);
    }

    private void AplicarDano(float cantidad, int numeroJugadorAsesino)
    {
        if (yaMurio)
            return;

        numeroUltimoAtacante = cantidad > 0f ? numeroJugadorAsesino : 0;
        vidaActual -= cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima);
        ActualizarBarra();

        if (vidaActual <= 0)
            Morir(numeroUltimoAtacante);
    }

    // Actualizar barra de vida
    void ActualizarBarra()
    {
        if (barraVida != null)
        {
            barraVida.maxValue = vidaMaxima;
            barraVida.value = vidaActual;
        }
    }

    public void AsignarBarraVida(Slider barra)
    {
        barraVida = barra;
        ActualizarBarra();
    }

    // Registrar muerte
    void Morir(int numeroJugadorAsesino)
    {
        if (yaMurio)
            return;

        yaMurio = true;

        Debug.Log(gameObject.name + " ha muerto");

        if (Controlador_Tiempo.Instance != null)
        {
            Controlador_Tiempo.Instance
                .RegistrarMuerte(numeroJugadorAsesino);
        }
        numeroUltimoAtacante = 0;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = false;
        }

        for (int i = 0; i < colisionadores.Length; i++)
        {
            if (colisionadores[i] != null)
                colisionadores[i].enabled = false;
        }

        for (int i = 0; i < comportamientos.Length; i++)
        {
            if (comportamientos[i] != null && comportamientos[i] != this)
                comportamientos[i].enabled = false;
        }

        StartCoroutine(Revivir());
    }

    private IEnumerator Revivir()
    {
        yield return new WaitForSecondsRealtime(TiempoRespawn);

        transform.SetPositionAndRotation(posicionInicial, rotacionInicial);
        transform.localScale = escalaInicial;

        if (rb != null)
        {
            rb.simulated = true;
            rb.position = posicionInicial;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        for (int i = 0; i < colisionadores.Length; i++)
        {
            if (colisionadores[i] != null)
                colisionadores[i].enabled = estadoColisionadores[i];
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = estadoRenderers[i];
        }

        vidaActual = vidaMaxima;
        yaMurio = false;
        numeroUltimoAtacante = 0;
        ActualizarBarra();

        for (int i = 0; i < comportamientos.Length; i++)
        {
            if (comportamientos[i] != null && comportamientos[i] != this)
                comportamientos[i].enabled = estadoComportamientos[i];
        }

        Debug.Log(gameObject.name + " ha revivido");
    }
}
