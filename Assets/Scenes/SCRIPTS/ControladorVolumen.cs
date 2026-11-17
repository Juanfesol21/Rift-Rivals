using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ControladorVolumen : MonoBehaviour
{
    private const string ClaveVolumen = "Volumen";
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        float volumen = Mathf.Clamp01(PlayerPrefs.GetFloat(ClaveVolumen, 1f));

        AudioListener.volume = volumen;
        slider.SetValueWithoutNotify(volumen);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    public void AplicarVolumen(float volumen)
    {
        AudioListener.volume = Mathf.Clamp01(volumen);
        PlayerPrefs.SetFloat(ClaveVolumen, AudioListener.volume);
        PlayerPrefs.Save();
    }
}
