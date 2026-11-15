using UnityEngine;

public class b : MonoBehaviour
{
    private enum Jugador
    {
        Uno,
        Dos
    }

    [SerializeField] private Jugador jugador;
    [SerializeField] private float radioInicial = 3f;
    [SerializeField] private float duracion = 5f;
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private float grosor = 0.08f;
    [SerializeField] private int segmentos = 64;
    [SerializeField] private Color color = Color.cyan;

    private LineRenderer linea;
    private CircleCollider2D colliderEscudo;
    private Material materialCreado;
    private float tiempoTranscurrido;
    private float proximoUso;
    private float radioActual;
    [SerializeField] private float fuerzaRetrocesoEscudo = 8f;
    [SerializeField] private float desplazamientoRetrocesoEscudo = 0.35f;
    private bool inicializado;
    private bool teclaPresionada;
    private float siguienteEmpujeContacto;

    public bool EstaActivo => inicializado && linea != null && linea.enabled &&
        colliderEscudo != null && colliderEscudo.enabled;

    public void EstablecerJugador(int numeroJugador)
    {
        jugador = numeroJugador == 1 ? Jugador.Uno : Jugador.Dos;
    }

    private void Start()
    {
        linea = GetComponent<LineRenderer>();
        if (linea == null)
        {
            linea = gameObject.AddComponent<LineRenderer>();
        }

        linea.useWorldSpace = false;
        linea.loop = true;
        linea.positionCount = Mathf.Max(3, segmentos);
        linea.startWidth = grosor;
        linea.endWidth = grosor;
        linea.startColor = color;
        linea.endColor = color;
        linea.sortingOrder = 100;

        GameObject objetoEscudo = new GameObject("Collider del escudo");
        objetoEscudo.layer = gameObject.layer;
        objetoEscudo.transform.SetParent(transform, false);
        colliderEscudo = objetoEscudo.AddComponent<CircleCollider2D>();
        colliderEscudo.isTrigger = true;
        colliderEscudo.enabled = false;

        if (linea.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                Debug.LogError("No se encontró un shader compatible para dibujar el círculo de energía.", this);
                linea.enabled = false;
                return;
            }

            materialCreado = new Material(shader);
            linea.sharedMaterial = materialCreado;
        }

        ActualizarRadio(Mathf.Max(0f, radioInicial));
        linea.enabled = false;
        inicializado = true;
    }

    private void Update()
    {
        if (linea == null || !inicializado)
        {
            return;
        }

        if (!Controlador_Tiempo.JuegoEnCurso)
        {
            linea.enabled = false;
            colliderEscudo.enabled = false;
            return;
        }

        bool mantenerActivo = Input.GetKey(jugador == Jugador.Uno ? KeyCode.S : KeyCode.K);

        if (!mantenerActivo)
        {
            linea.enabled = false;
            colliderEscudo.enabled = false;
            tiempoTranscurrido = 0f;
            teclaPresionada = false;
            return;
        }

        if (!teclaPresionada)
        {
            teclaPresionada = true;
            if (Time.time < proximoUso || duracion <= 0f)
            {
                colliderEscudo.enabled = false;
                return;
            }

            tiempoTranscurrido = 0f;
            proximoUso = Time.time + Mathf.Max(0f, cooldown);
            radioActual = Mathf.Max(0f, radioInicial);
            linea.enabled = true;
            colliderEscudo.enabled = true;
        }

        if (!linea.enabled)
        {
            return;
        }

        if (duracion <= 0f)
        {
            linea.enabled = false;
            colliderEscudo.enabled = false;
            return;
        }

        tiempoTranscurrido += Time.deltaTime;
        float progreso = Mathf.Clamp01(tiempoTranscurrido / duracion);
        radioActual = Mathf.Max(0f, radioActual - Mathf.Max(0f, radioInicial) * Time.deltaTime / duracion);
        ActualizarRadio(radioActual);

        if (progreso >= 1f)
        {
            linea.enabled = false;
            colliderEscudo.enabled = false;
        }
    }

    private void ActualizarRadio(float radio)
    {
        int cantidadSegmentos = Mathf.Max(3, segmentos);
        linea.positionCount = cantidadSegmentos;
        if (colliderEscudo != null)
        {
            colliderEscudo.radius = radio;
        }

        for (int i = 0; i < cantidadSegmentos; i++)
        {
            float angulo = i * (Mathf.PI * 2f / cantidadSegmentos);
            linea.SetPosition(i, new Vector3(Mathf.Cos(angulo) * radio, Mathf.Sin(angulo) * radio, 0f));
        }
    }

    public bool EsColliderEscudo(Collider2D colisionador)
    {
        return colliderEscudo != null && colisionador == colliderEscudo && colliderEscudo.enabled;
    }

    public void RecibirImpacto()
    {
        if (!inicializado || !linea.enabled || colliderEscudo == null || !colliderEscudo.enabled)
        {
            return;
        }

        radioActual = Mathf.Max(0f, radioActual - Mathf.Max(0f, radioInicial) * 0.2f);
        ActualizarRadio(radioActual);
        if (radioActual <= 0f)
        {
            linea.enabled = false;
            colliderEscudo.enabled = false;
        }
    }

    public void BloquearAtaque(Transform atacante)
    {
        if (!EstaActivo || atacante == null)
            return;

        RecibirImpacto();
        EmpujarRival(atacante);
    }

    private void OnTriggerEnter2D(Collider2D otro)
    {
        if (!EstaActivo || Time.time < siguienteEmpujeContacto)
            return;

        siguienteEmpujeContacto = Time.time + 0.2f;
        EmpujarRival(otro.transform);
    }

    private void EmpujarRival(Transform rival)
    {
        if (rival == null)
            return;

        Vida_Jugador vidaRival = rival.GetComponentInParent<Vida_Jugador>();
        Vida_Jugador vidaPropia = GetComponent<Vida_Jugador>();
        if (vidaRival == null || vidaRival == vidaPropia)
            return;

        Rigidbody2D rbRival = vidaRival.GetComponent<Rigidbody2D>();
        if (rbRival == null)
            return;

        float direccion = Mathf.Sign(rbRival.position.x - transform.position.x);
        if (direccion == 0f)
            direccion = Mathf.Sign(rival.position.x - transform.position.x);
        if (direccion == 0f)
            direccion = 1f;

        rbRival.position += Vector2.right * direccion * Mathf.Max(0f, desplazamientoRetrocesoEscudo);
        Vector2 velocidadRetroceso = new Vector2(
            direccion * Mathf.Max(Mathf.Abs(rbRival.velocity.x), fuerzaRetrocesoEscudo),
            rbRival.velocity.y);
        Movimiento_Jugador_1 movimientoJugador1 = vidaRival.GetComponent<Movimiento_Jugador_1>();
        Movimiento_Jugador_2 movimientoJugador2 = vidaRival.GetComponent<Movimiento_Jugador_2>();
        if (movimientoJugador1 != null)
            movimientoJugador1.AplicarRetroceso(velocidadRetroceso);
        else if (movimientoJugador2 != null)
            movimientoJugador2.AplicarRetroceso(velocidadRetroceso);
        else
            rbRival.velocity = velocidadRetroceso;
    }

    private void OnDestroy()
    {
        if (materialCreado != null)
        {
            Destroy(materialCreado);
        }
    }
}
