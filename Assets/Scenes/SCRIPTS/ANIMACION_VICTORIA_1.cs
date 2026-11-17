using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANIMACION_VICTORIA_1 : MonoBehaviour
{
    public SpriteRenderer personaje;

    public Sprite imagenVictoria;
    public Sprite imagenCopa;

    public float tiempoCambio = 1f;

    void Start()
    {
        StartCoroutine(CambiarImagenes());
    }

    IEnumerator CambiarImagenes()
    {
        while (true)
        {
            personaje.sprite = imagenVictoria;

            yield return new WaitForSeconds(tiempoCambio);

            personaje.sprite = imagenCopa;

            yield return new WaitForSeconds(tiempoCambio);
        }
    }
}