using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANIMACION_PERDEDOR_1 : MonoBehaviour
{
    public SpriteRenderer personaje;

    public Sprite imagenDerrota1;
    public Sprite imagenDerrota2;

    public float tiempoCambio = 1f;

    void Start()
    {
        StartCoroutine(CambiarImagenes());
    }

    IEnumerator CambiarImagenes()
    {
        while (true)
        {
            personaje.sprite = imagenDerrota1;

            yield return new WaitForSeconds(tiempoCambio);

            personaje.sprite = imagenDerrota2;

            yield return new WaitForSeconds(tiempoCambio);
        }
    }
}