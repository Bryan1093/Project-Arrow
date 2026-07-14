using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipCinematic : MonoBehaviour
{
    [Header("Escena de Destino")]
    // Escribe aquí el nombre exacto de tu escena de juego (por ejemplo: "SampleScene")
    public string nombreEscenaJuego = "SampleScene";

    void Update()
    {
        // Detecta si el jugador presiona la tecla Escape (puedes cambiarlo a KeyCode.Return para el Enter)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SaltarAlJuego();
        }
    }

    // Esta función la puedes usar también si decides hacer un botón clickeable con el ratón
    public void SaltarAlJuego()
    {
        // ¡Opcional! Apagar la música de la cinemática si estás usando tu AudioManager
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopSound("StoryTheme"); // Cambia el nombre por el de tu pista actual
        }

        // Carga el nivel directamente
        SceneManager.LoadScene(nombreEscenaJuego);
    }
}