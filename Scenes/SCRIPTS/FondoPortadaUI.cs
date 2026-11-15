using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class FondoPortadaUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform contenedor;
    private Image imagen;
    private Vector2 tamanoContenedor;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        contenedor = transform.parent as RectTransform;
        imagen = GetComponent<Image>();
    }

    private void Start()
    {
        Canvas.ForceUpdateCanvases();
        AjustarAlContenedor();
    }

    private void LateUpdate()
    {
        if (contenedor != null && contenedor.rect.size != tamanoContenedor)
        {
            AjustarAlContenedor();
        }
    }

    private void AjustarAlContenedor()
    {
        if (contenedor == null || imagen.sprite == null || contenedor.rect.width <= 0f || contenedor.rect.height <= 0f)
        {
            Debug.LogError("El fondo de configuración necesita un contenedor UI y un sprite asignado.", this);
            return;
        }

        tamanoContenedor = contenedor.rect.size;
        float proporcionImagen = imagen.sprite.rect.width / imagen.sprite.rect.height;
        float proporcionContenedor = tamanoContenedor.x / tamanoContenedor.y;
        float ancho = tamanoContenedor.x;
        float alto = tamanoContenedor.y;

        if (proporcionContenedor > proporcionImagen)
        {
            alto = ancho / proporcionImagen;
        }
        else
        {
            ancho = alto * proporcionImagen;
        }

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(ancho, alto);
    }
}
