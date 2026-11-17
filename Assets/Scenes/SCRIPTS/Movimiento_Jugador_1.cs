using UnityEngine;
using System.Collections.Generic;

public class Movimiento_Jugador_1 : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 80f;
    public float jumpForce = 100f;

    [Header("Controles")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;

    [Header("Deteccion del suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Animacion")]
    public SpriteRenderer spriteRenderer;
    public Sprite spriteReposo;
    public Sprite spriteCaminar1;
    public Sprite spriteCaminar2;
    public Sprite spriteAgachado;
    public Sprite spriteHadoken;
    public Sprite spriteAtaqueCuerpoACuerpo;
    public Sprite spriteGolpe;
    public float intervaloAnimacion = 0.14f;
    public float danioAtaqueCuerpoACuerpo = 12f;
    public float cooldownAtaqueCuerpoACuerpo = 0.5f;
    public Vector2 alcanceAtaqueCuerpoACuerpo = new Vector2(1.4f, 1f);
    public float duracionSpriteAtaqueCuerpoACuerpo = 0.25f;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool jumpRequested;
    private float direccionMirada = 1f;
    private float retrocesoHasta;
    private float proximaAnimacion;
    private float proximoAtaque;
    private bool alternarSpriteCaminar;
    private Sprite spriteTemporal;
    private float spriteTemporalHasta;

    public float DireccionMirada => direccionMirada;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        direccionMirada = Mathf.Sign(transform.localScale.x);
        if (direccionMirada == 0f)
            direccionMirada = 1f;

        ActualizarSprite();
    }

    private void Update()
    {
        horizontalInput = 0f;

        if (Input.GetKey(leftKey))
        {
            horizontalInput = -1f;
            EstablecerDireccionMirada(-1f);
        }

        if (Input.GetKey(rightKey))
        {
            horizontalInput = 1f;
            EstablecerDireccionMirada(1f);
        }

        if (Input.GetKeyDown(jumpKey) && IsGrounded())
            jumpRequested = true;

        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= proximoAtaque &&
            Controlador_Tiempo.JuegoEnCurso)
        {
            proximoAtaque = Time.time + Mathf.Max(0f, cooldownAtaqueCuerpoACuerpo);
            MostrarSpriteTemporal(
                spriteAtaqueCuerpoACuerpo,
                duracionSpriteAtaqueCuerpoACuerpo);
            AplicarDanioCuerpoACuerpo();
        }

        ActualizarSprite();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (Time.time >= retrocesoHasta)
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        if (jumpRequested)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpRequested = false;
        }
    }

    public void EstablecerDireccionMirada(float direccion)
    {
        if (direccion == 0f)
            return;

        direccionMirada = Mathf.Sign(direccion);
        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) * direccionMirada;
        transform.localScale = escala;
    }

    public void AplicarRetroceso(Vector2 velocidad)
    {
        retrocesoHasta = Time.time + 0.2f;
        if (rb != null)
            rb.velocity = velocidad;
    }

    private void AplicarDanioCuerpoACuerpo()
    {
        Vida_Jugador vidaPropia = GetComponent<Vida_Jugador>();
        Collider2D colisionadorPropio = GetComponent<Collider2D>();
        float centroY = colisionadorPropio != null
            ? colisionadorPropio.bounds.center.y - transform.position.y
            : 0f;
        Vector2 tamano = new Vector2(
            Mathf.Max(0.05f, Mathf.Abs(alcanceAtaqueCuerpoACuerpo.x)),
            Mathf.Max(0.05f, Mathf.Abs(alcanceAtaqueCuerpoACuerpo.y)));
        Vector2 centro = (Vector2)transform.position + new Vector2(
            DireccionMirada * tamano.x * 0.5f,
            centroY);
        Collider2D[] impactos = Physics2D.OverlapBoxAll(centro, tamano, 0f);
        HashSet<Vida_Jugador> objetivos = new HashSet<Vida_Jugador>();

        foreach (Collider2D impacto in impactos)
        {
            Vida_Jugador vidaObjetivo = impacto.GetComponentInParent<Vida_Jugador>();
            if (vidaObjetivo != null && vidaObjetivo != vidaPropia)
                objetivos.Add(vidaObjetivo);
        }

        foreach (Vida_Jugador vidaObjetivo in objetivos)
        {
            b escudo = vidaObjetivo.GetComponent<b>();
            if (escudo != null && escudo.EstaActivo)
                escudo.BloquearAtaque(transform);
            else
                vidaObjetivo.RecibirDano(danioAtaqueCuerpoACuerpo, transform);
        }
    }

    public void MostrarHadoken()
    {
        if (spriteHadoken == null)
            return;

        MostrarSpriteTemporal(spriteHadoken, 0.28f);
    }

    private void MostrarSpriteTemporal(Sprite sprite, float duracion)
    {
        if (sprite == null)
            return;

        spriteTemporal = sprite;
        spriteTemporalHasta = Time.time + Mathf.Max(0.01f, duracion);
        if (spriteRenderer != null)
            spriteRenderer.sprite = spriteTemporal;
    }

    private void ActualizarSprite()
    {
        if (spriteRenderer == null)
            return;

        if (spriteTemporal != null && Time.time < spriteTemporalHasta)
        {
            spriteRenderer.sprite = spriteTemporal;
            return;
        }

        spriteTemporal = null;
        if (horizontalInput != 0f && IsGrounded())
        {
            if (Time.time >= proximaAnimacion)
            {
                alternarSpriteCaminar = !alternarSpriteCaminar;
                proximaAnimacion = Time.time + Mathf.Max(0.01f, intervaloAnimacion);
            }

            Sprite spriteCaminar = alternarSpriteCaminar ? spriteCaminar1 : spriteCaminar2;
            spriteRenderer.sprite = spriteCaminar != null ? spriteCaminar : spriteReposo;
            return;
        }

        if (Input.GetKey(KeyCode.DownArrow) && IsGrounded() && spriteAgachado != null)
        {
            spriteRenderer.sprite = spriteAgachado;
            return;
        }

        spriteRenderer.sprite = spriteReposo;
    }

    private bool IsGrounded()
    {
        return groundCheck != null &&
               Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
