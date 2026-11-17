using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hadoken : MonoBehaviour
{
    private enum Jugador
    {
        Uno,
        Dos
    }

    [SerializeField] private Jugador jugador;
    [SerializeField] private GameObject rival;
    [SerializeField] private float radioBola = 0.5f;
    [SerializeField] private float velocidad = 8f;
    [SerializeField] private float duracionRebote = 5f;
    [SerializeField] private float cooldown = 1f;
    [SerializeField, Min(0f)] private float danioHadoken = 20f;
    [SerializeField] private Color colorBola = Color.cyan;
    [SerializeField] private Sprite spriteBola;
    [SerializeField] private Sprite spriteOndaEspada;
    [SerializeField] private Vector2 tamanoOndaEspada = new Vector2(0.9f, 1.25f);
    [SerializeField] private float velocidadOndaEspada = 10f;
    [SerializeField] private float retrasoOndaEspada = 0.1f;
    [SerializeField, Min(0f)] private float danioOndaEspada = 18f;

    private Material materialBola;
    private float proximoDisparo;
    private readonly RaycastHit2D[] impactosRival2D = new RaycastHit2D[32];
    private readonly RaycastHit2D[] impactosOnda2D = new RaycastHit2D[32];
    private readonly RaycastHit[] impactosOnda3D = new RaycastHit[32];
    private readonly Collider2D[] solapamientosOnda2D = new Collider2D[32];
    private readonly Collider[] solapamientosOnda3D = new Collider[32];
    private readonly HashSet<Collider2D> colisionadoresRival2D = new HashSet<Collider2D>();
    private readonly HashSet<Collider> colisionadoresRival3D = new HashSet<Collider>();
    private Renderer[] renderersRival = new Renderer[0];
    private Hadoken controladorRival;
    private Transform raizRival;

    private void Start()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            Debug.LogError("No se encontró un shader compatible para la bola.", this);
            enabled = false;
            return;
        }

        materialBola = new Material(shader);
        materialBola.color = colorBola;
        materialBola.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        if (rival != null)
        {
            PrepararRival();
            EstablecerDireccionMirada(rival.transform.position.x < transform.position.x ? -1f : 1f);
        }
    }

    private void Update()
    {
        if (!Controlador_Tiempo.JuegoEnCurso)
            return;

        if (Input.GetKeyDown(jugador == Jugador.Uno ? KeyCode.E : KeyCode.O)
            && Time.time >= proximoDisparo)
        {
            proximoDisparo = Time.time + Mathf.Max(0f, cooldown);
            if (jugador == Jugador.Dos)
            {
                LanzarOndaEspada();
            }
            else
            {
                LanzarBola();
            }
        }
    }

    private void LanzarOndaEspada()
    {
        if (spriteOndaEspada == null)
        {
            Debug.LogError("Asigna el sprite de la onda de espada del jugador 2.", this);
            return;
        }

        PrepararRival();
        float direccionX = ObtenerDireccionMirada();
        Movimiento_Jugador_2 movimiento = GetComponent<Movimiento_Jugador_2>();
        if (movimiento != null)
        {
            movimiento.MostrarAtaque();
        }

        StartCoroutine(GenerarOndaEspada(direccionX));
    }

    private IEnumerator GenerarOndaEspada(float direccionX)
    {
        if (retrasoOndaEspada > 0f)
        {
            yield return new WaitForSeconds(retrasoOndaEspada);
        }

        Vector2 tamano = new Vector2(
            Mathf.Max(0.05f, Mathf.Abs(tamanoOndaEspada.x)),
            Mathf.Max(0.05f, Mathf.Abs(tamanoOndaEspada.y)));
        GameObject onda = new GameObject("Onda de espada morada");
        SpriteRenderer rendererOnda = onda.AddComponent<SpriteRenderer>();
        rendererOnda.sprite = spriteOndaEspada;
        rendererOnda.sortingOrder = 100;
        rendererOnda.flipX = direccionX < 0f;

        Vector2 tamanoSprite = spriteOndaEspada.bounds.size;
        onda.transform.localScale = new Vector3(
            tamano.x / Mathf.Max(0.01f, tamanoSprite.x),
            tamano.y / Mathf.Max(0.01f, tamanoSprite.y),
            1f);

        Collider2D colisionadorJugador = GetComponent<Collider2D>();
        float desplazamientoInicial = colisionadorJugador != null
            ? colisionadorJugador.bounds.extents.x + tamano.x * 0.5f
            : tamano.x * 0.5f;
        onda.transform.position = transform.position + new Vector3(
            direccionX * desplazamientoInicial,
            0.05f,
            -0.05f);

        float rapidez = Mathf.Abs(velocidadOndaEspada);
        if (rapidez <= 0f)
        {
            Debug.LogError("La velocidad de la onda de espada debe ser mayor que cero.", this);
            Destroy(onda);
            yield break;
        }

        ContactFilter2D filtro2D = new ContactFilter2D();
        filtro2D.useTriggers = true;
        filtro2D.SetLayerMask(Physics2D.AllLayers);
        Vector2 direccion = Vector2.right * direccionX;
        float radio3D = tamano.magnitude * 0.5f;

        while (onda != null)
        {
            Vector2 posicion = onda.transform.position;
            b escudoSolapado;
            Vida_Jugador vidaSolapada;
            if (SolapadaConOtroObjeto(
                    posicion,
                    tamano,
                    radio3D,
                    filtro2D,
                    out escudoSolapado,
                    out vidaSolapada))
            {
                if (escudoSolapado != null)
                {
                    escudoSolapado.RecibirImpacto();
                }
                else if (vidaSolapada != null)
                {
                    vidaSolapada.RecibirDano(danioOndaEspada, transform);
                }

                Destroy(onda);
                yield break;
            }

            float distancia = rapidez * Time.deltaTime;
            int cantidadImpactos2D = Physics2D.BoxCast(
                posicion,
                tamano,
                0f,
                direccion,
                filtro2D,
                impactosOnda2D,
                distancia);

            bool impactoEncontrado = false;
            float distanciaImpacto = distancia;
            b escudoImpactado = null;
            Vida_Jugador vidaImpactada = null;
            for (int i = 0; i < cantidadImpactos2D; i++)
            {
                RaycastHit2D impacto = impactosOnda2D[i];
                if (impacto.collider != null
                    && !EsParteDeEsteJugador(impacto.transform)
                    && impacto.distance <= distanciaImpacto)
                {
                    impactoEncontrado = true;
                    distanciaImpacto = impacto.distance;
                    escudoImpactado = ObtenerEscudo(impacto.collider);
                    vidaImpactada = ObtenerVidaRival(impacto.collider);
                }
            }

            int cantidadImpactos3D = Physics.SphereCastNonAlloc(
                new Vector3(posicion.x, posicion.y, onda.transform.position.z),
                radio3D,
                new Vector3(direccion.x, direccion.y, 0f),
                impactosOnda3D,
                distancia,
                ~0,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < cantidadImpactos3D; i++)
            {
                RaycastHit impacto = impactosOnda3D[i];
                if (impacto.collider != null
                    && !EsParteDeEsteJugador(impacto.transform)
                    && impacto.distance <= distanciaImpacto)
                {
                    impactoEncontrado = true;
                    distanciaImpacto = impacto.distance;
                    escudoImpactado = null;
                    vidaImpactada = ObtenerVidaRival(impacto.collider);
                }
            }

            if (impactoEncontrado)
            {
                if (escudoImpactado != null)
                {
                    escudoImpactado.RecibirImpacto();
                }
                else if (vidaImpactada != null)
                {
                    vidaImpactada.RecibirDano(danioOndaEspada, transform);
                }

                Destroy(onda);
                yield break;
            }

            onda.transform.position += new Vector3(direccion.x, direccion.y, 0f) * distancia;
            yield return null;
        }
    }

    private bool EsParteDeEsteJugador(Transform objeto)
    {
        return objeto != null
            && (objeto == transform || objeto.IsChildOf(transform));
    }

    private bool SolapadaConOtroObjeto(
        Vector2 posicion,
        Vector2 tamano,
        float radio,
        ContactFilter2D filtro2D,
        out b escudoImpactado,
        out Vida_Jugador vidaImpactada)
    {
        escudoImpactado = null;
        vidaImpactada = null;
        bool otroObjetoSolapado = false;

        int cantidad2D = Physics2D.OverlapBox(
            posicion,
            tamano,
            0f,
            filtro2D,
            solapamientosOnda2D);
        for (int i = 0; i < cantidad2D; i++)
        {
            Collider2D colisionador = solapamientosOnda2D[i];
            if (colisionador != null && !EsParteDeEsteJugador(colisionador.transform))
            {
                b escudo = ObtenerEscudo(colisionador);
                if (escudo != null)
                {
                    escudoImpactado = escudo;
                    return true;
                }

                Vida_Jugador vida = ObtenerVidaRival(colisionador);
                if (vida != null)
                    vidaImpactada = vida;
                else
                    otroObjetoSolapado = true;
            }
        }

        Vector3 posicion3D = new Vector3(posicion.x, posicion.y, transform.position.z);
        int cantidad3D = Physics.OverlapSphereNonAlloc(
            posicion3D,
            radio,
            solapamientosOnda3D,
            ~0,
            QueryTriggerInteraction.Collide);
        for (int i = 0; i < cantidad3D; i++)
        {
            Collider colisionador = solapamientosOnda3D[i];
            if (colisionador != null && !EsParteDeEsteJugador(colisionador.transform))
            {
                Vida_Jugador vida = ObtenerVidaRival(colisionador);
                if (vida != null)
                    vidaImpactada = vida;
                else
                    otroObjetoSolapado = true;
            }
        }

        return vidaImpactada != null || otroObjetoSolapado;
    }

    private void LanzarBola()
    {
        PrepararRival();
        Movimiento_Jugador_1 movimientoJugador1 = GetComponent<Movimiento_Jugador_1>();
        if (movimientoJugador1 != null)
        {
            movimientoJugador1.MostrarHadoken();
        }
        else
        {
            Movimiento_Jugador_2 movimientoJugador2 = GetComponent<Movimiento_Jugador_2>();
            if (movimientoJugador2 != null)
                movimientoJugador2.MostrarAtaque();
        }

        if (rival == null)
        {
            Debug.LogWarning("Asigna el otro personaje en el campo Rival para detectar cuándo el hadoken lo toca.", this);
        }

        float direccionX = ObtenerDireccionMirada();
        GameObject bola;
        if (spriteBola != null)
        {
            bola = new GameObject("Bola Hadoken");
            SpriteRenderer spriteRenderer = bola.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = spriteBola;
            spriteRenderer.flipX = direccionX < 0f;
            spriteRenderer.sortingOrder = 100;

            Vector2 tamanoSprite = spriteBola.bounds.size;
            float dimensionMayor = Mathf.Max(tamanoSprite.x, tamanoSprite.y);
            float dimensionDeseada = Mathf.Max(0.02f, radioBola * 2f);
            float escala = dimensionDeseada / Mathf.Max(0.01f, dimensionMayor);
            bola.transform.localScale = Vector3.one * escala;
        }
        else
        {
            bola = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bola.transform.localScale = Vector3.one * Mathf.Max(0.02f, radioBola * 2f);
            Renderer rendererBola = bola.GetComponent<Renderer>();
            rendererBola.sharedMaterial = materialBola;
            rendererBola.sortingOrder = 100;
        }

        bola.name = "Bola Hadoken";
        bola.transform.position = transform.position + Vector3.right * (direccionX * Mathf.Max(0.01f, radioBola));
        StartCoroutine(MoverBola(bola, direccionX));
    }

    public void EstablecerJugador(int numeroJugador)
    {
        jugador = numeroJugador == 1 ? Jugador.Uno : Jugador.Dos;
    }

    private IEnumerator MoverBola(GameObject bola, float direccionX)
    {
        float tiempoTranscurrido = 0f;
        float duracion = Mathf.Max(0f, duracionRebote);
        float rapidez = Mathf.Abs(velocidad);

        if (duracion <= 0f || rapidez <= 0f)
        {
            Destroy(bola);
            yield break;
        }

        Vector2 direccion = Vector2.right * direccionX;
        while (bola != null && tiempoTranscurrido < duracion)
        {
            float deltaTiempo = Mathf.Min(Time.deltaTime, duracion - tiempoTranscurrido);
            float distanciaRestante = rapidez * deltaTiempo;
            tiempoTranscurrido += deltaTiempo;

            for (int rebotesEnFrame = 0; bola != null && distanciaRestante > 0.001f && rebotesEnFrame < 4; rebotesEnFrame++)
            {
                Vector3 posicion = bola.transform.position;
                Vector3 destino = posicion + new Vector3(direccion.x, direccion.y, 0f) * distanciaRestante;
                if (TocaAlRivalEnTrayectoria(posicion, destino))
                {
                    AplicarDanioRival(danioHadoken);
                    Destroy(bola);
                    yield break;
                }

                bool impactoEncontrado = false;
                float distanciaImpacto = distanciaRestante;
                Vector2 normalImpacto = Vector2.zero;
                bool impactoEsRival = false;
                b escudoImpactado = null;

                ContactFilter2D filtro2D = new ContactFilter2D();
                filtro2D.useTriggers = true;
                filtro2D.SetLayerMask(~0);
                int cantidadImpactos2D = Physics2D.CircleCast(
                    posicion,
                    Mathf.Max(0.01f, radioBola),
                    direccion,
                    filtro2D,
                    impactosRival2D,
                    distanciaRestante);

                for (int i = 0; i < cantidadImpactos2D; i++)
                {
                    if (EsOtroObjeto(impactosRival2D[i].transform, bola)
                        && impactosRival2D[i].distance < distanciaImpacto)
                    {
                        impactoEncontrado = true;
                        distanciaImpacto = impactosRival2D[i].distance;
                        normalImpacto = impactosRival2D[i].normal;
                        impactoEsRival = EsColliderRival(impactosRival2D[i].collider);
                        escudoImpactado = ObtenerEscudo(impactosRival2D[i].collider);
                    }
                }

                RaycastHit[] impactos3D = Physics.SphereCastAll(
                    posicion,
                    Mathf.Max(0.01f, radioBola),
                    new Vector3(direccion.x, direccion.y, 0f),
                    distanciaRestante,
                    ~0,
                    QueryTriggerInteraction.Collide);

                for (int i = 0; i < impactos3D.Length; i++)
                {
                    if (EsOtroObjeto(impactos3D[i].transform, bola)
                        && impactos3D[i].distance < distanciaImpacto)
                    {
                        impactoEncontrado = true;
                        distanciaImpacto = impactos3D[i].distance;
                        normalImpacto = new Vector2(impactos3D[i].normal.x, impactos3D[i].normal.y);
                        impactoEsRival = EsColliderRival(impactos3D[i].collider);
                        escudoImpactado = null;
                    }
                }

                if (!impactoEncontrado)
                {
                    bola.transform.position = posicion
                        + new Vector3(direccion.x, direccion.y, 0f) * distanciaRestante;
                    break;
                }

                if (impactoEsRival && escudoImpactado == null)
                {
                    AplicarDanioRival(danioHadoken);
                    Destroy(bola);
                    yield break;
                }

                if (escudoImpactado != null)
                {
                    escudoImpactado.RecibirImpacto();
                }

                Vector3 avance = new Vector3(direccion.x, direccion.y, 0f) * distanciaImpacto;
                bola.transform.position = posicion + avance;
                distanciaRestante -= distanciaImpacto;

                if (normalImpacto.sqrMagnitude < 0.001f)
                {
                    break;
                }

                normalImpacto.Normalize();
                direccion = Vector2.Reflect(direccion, normalImpacto).normalized;
                bola.transform.position += new Vector3(normalImpacto.x, normalImpacto.y, 0f) * 0.01f;
                distanciaRestante = Mathf.Max(0f, distanciaRestante - 0.01f);
            }

            yield return null;
        }

        if (bola != null)
        {
            Destroy(bola);
        }
    }

    private bool EsOtroObjeto(Transform objetoImpactado, GameObject bola)
    {
        if (objetoImpactado == null)
        {
            return false;
        }

        return objetoImpactado != bola.transform
            && !objetoImpactado.IsChildOf(bola.transform)
            && objetoImpactado != transform
            && !objetoImpactado.IsChildOf(transform);
    }

    private bool TocaAlRivalEnTrayectoria(Vector3 inicio, Vector3 destino)
    {
        if (rival == null)
        {
            return false;
        }

        float radio = Mathf.Max(0.01f, radioBola);
        float radioCuadrado = radio * radio;
        float distancia = Vector3.Distance(inicio, destino);
        float pasoMaximo = Mathf.Max(0.05f, radio * 0.5f);
        int cantidadPasos = Mathf.Max(1, Mathf.CeilToInt(distancia / pasoMaximo));

        for (int paso = 0; paso <= cantidadPasos; paso++)
        {
            Vector3 posicion = Vector3.Lerp(inicio, destino, paso / (float)cantidadPasos);
            bool rivalCubiertoPorEscudo = false;
            bool rivalEnContacto = false;

            foreach (Collider2D colisionador in colisionadoresRival2D)
            {
                if (colisionador == null || !colisionador.enabled || !colisionador.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector2 puntoMasCercano = colisionador.ClosestPoint(posicion);
                if ((puntoMasCercano - (Vector2)posicion).sqrMagnitude > radioCuadrado)
                {
                    continue;
                }

                if (EsColliderEscudo(colisionador))
                {
                    rivalCubiertoPorEscudo = true;
                }
                else
                {
                    rivalEnContacto = true;
                }
            }

            foreach (Collider colisionador in colisionadoresRival3D)
            {
                if (colisionador == null || !colisionador.enabled || !colisionador.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Vector3 puntoMasCercano = colisionador.ClosestPoint(posicion);
                if ((puntoMasCercano - posicion).sqrMagnitude <= radioCuadrado)
                {
                    rivalEnContacto = true;
                }
            }

            for (int i = 0; i < renderersRival.Length; i++)
            {
                Renderer rendererRival = renderersRival[i];
                if (rendererRival == null
                    || !rendererRival.enabled
                    || !rendererRival.gameObject.activeInHierarchy
                    || rendererRival is LineRenderer)
                {
                    continue;
                }

                Vector3 puntoMasCercano = rendererRival.bounds.ClosestPoint(posicion);
                if ((puntoMasCercano - posicion).sqrMagnitude <= radioCuadrado)
                {
                    rivalEnContacto = true;
                    break;
                }
            }

            if (rivalEnContacto && !rivalCubiertoPorEscudo)
            {
                return true;
            }
        }

        return false;
    }

    private void PrepararRival()
    {
        if (rival == null)
            return;

        controladorRival = rival.GetComponentInParent<Hadoken>();
        raizRival = controladorRival != null ? controladorRival.transform : rival.transform;
        CachearColisionadoresRival();
    }

    private Vida_Jugador ObtenerVidaRival(Collider2D colisionador)
    {
        Vida_Jugador vida = colisionador != null
            ? colisionador.GetComponentInParent<Vida_Jugador>()
            : null;
        return vida != null && EsDelRival(vida.transform) ? vida : null;
    }

    private Vida_Jugador ObtenerVidaRival(Collider colisionador)
    {
        Vida_Jugador vida = colisionador != null
            ? colisionador.GetComponentInParent<Vida_Jugador>()
            : null;
        return vida != null && EsDelRival(vida.transform) ? vida : null;
    }

    private void AplicarDanioRival(float cantidad)
    {
        if (raizRival == null)
            return;

        Vida_Jugador vida = raizRival.GetComponentInParent<Vida_Jugador>();
        if (vida == null)
            vida = raizRival.GetComponentInChildren<Vida_Jugador>();

        if (vida == null)
        {
            Debug.LogError("El rival no tiene un componente Vida_Jugador.", this);
            return;
        }

        vida.RecibirDano(cantidad, transform);
    }

    private void CachearColisionadoresRival()
    {
        colisionadoresRival2D.Clear();
        colisionadoresRival3D.Clear();
        renderersRival = raizRival.GetComponentsInChildren<Renderer>(true);

        Transform actual = raizRival;
        while (actual != null)
        {
            foreach (Collider2D colisionador in actual.GetComponents<Collider2D>())
            {
                colisionadoresRival2D.Add(colisionador);
            }

            foreach (Collider colisionador in actual.GetComponents<Collider>())
            {
                colisionadoresRival3D.Add(colisionador);
            }

            actual = actual.parent;
        }

        foreach (Collider2D colisionador in raizRival.GetComponentsInChildren<Collider2D>(true))
        {
            colisionadoresRival2D.Add(colisionador);
        }

        foreach (Collider colisionador in raizRival.GetComponentsInChildren<Collider>(true))
        {
            colisionadoresRival3D.Add(colisionador);
        }
    }

    private bool EsColliderRival(Collider2D colisionador)
    {
        return colisionador != null
            && (colisionadoresRival2D.Contains(colisionador)
                || EsDelRival(colisionador.transform));
    }

    private bool EsColliderRival(Collider colisionador)
    {
        return colisionador != null
            && (colisionadoresRival3D.Contains(colisionador)
                || EsDelRival(colisionador.transform));
    }

    private bool EsDelRival(Transform objeto)
    {
        return raizRival != null
            && objeto != null
            && (objeto == raizRival || objeto.IsChildOf(raizRival));
    }

    private bool EsColliderEscudo(Collider2D colisionador)
    {
        return ObtenerEscudo(colisionador) != null;
    }

    private b ObtenerEscudo(Collider2D colisionador)
    {
        b escudo = colisionador != null ? colisionador.GetComponentInParent<b>() : null;
        return escudo != null && escudo.EsColliderEscudo(colisionador) ? escudo : null;
    }

    private float ObtenerDireccionMirada()
    {
        float direccion = Mathf.Sign(transform.localScale.x);
        if (jugador == Jugador.Uno)
        {
            Movimiento_Jugador_1 movimiento = GetComponent<Movimiento_Jugador_1>();
            if (movimiento != null)
            {
                direccion = movimiento.DireccionMirada;
            }
        }
        else
        {
            Movimiento_Jugador_2 movimiento = GetComponent<Movimiento_Jugador_2>();
            if (movimiento != null)
            {
                direccion = movimiento.DireccionMirada;
            }
        }

        return direccion == 0f ? 1f : direccion;
    }

    private void EstablecerDireccionMirada(float direccion)
    {
        if (jugador == Jugador.Uno)
        {
            Movimiento_Jugador_1 movimiento = GetComponent<Movimiento_Jugador_1>();
            if (movimiento != null)
            {
                movimiento.EstablecerDireccionMirada(direccion);
                return;
            }
        }
        else
        {
            Movimiento_Jugador_2 movimiento = GetComponent<Movimiento_Jugador_2>();
            if (movimiento != null)
            {
                movimiento.EstablecerDireccionMirada(direccion);
                return;
            }
        }

        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) * Mathf.Sign(direccion);
        transform.localScale = escala;
    }

    private void OnDestroy()
    {
        if (materialBola != null)
        {
            Destroy(materialBola);
        }
    }
}
