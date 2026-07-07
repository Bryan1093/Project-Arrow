using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para el botón de ir al menú

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverUIBG;

    [Header("Victory UI")]
    [SerializeField] private GameObject victoryUIBG;

    [Header("Pause UI")]
    [SerializeField] private GameObject pauseUIBG;

    public int key;

    // Variables para controlar el estado de la pausa
    private bool isPaused = false;
    private bool isTransitioning = false; // Evita que se rompa si pulsas 'P' muy rápido

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        key = 0;

        // Escondemos los menús moviéndolos hacia abajo (-1200) al iniciar
        if (gameOverUIBG != null) gameOverUIBG.transform.localPosition = new Vector3(0f, -1200f, 0f);
        if (pauseUIBG != null) pauseUIBG.transform.localPosition = new Vector3(0f, -1200f, 0f);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopSound("MenuTheme"); // Apaga la del menú
            AudioManager.instance.PlaySound("GameTheme"); // Enciende la del juego
        }
    }

    private void Update()
    {
        // Detectar tecla 'P' para pausar/reanudar
        if (Input.GetKeyDown(KeyCode.P) && !isTransitioning)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        // Atajo de desarrollador: F4 para forzar la victoria instantánea
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("¡Truco activado: Victoria instantánea!");
            
            // Desactiva la pausa si el juego estaba pausado antes de ganar
            if (isPaused)
            {
                ResumeGame(); 
            }

            // Llamamos a la función que muestra la pantalla de victoria
            TriggerVictoryUI();
        }
    }

    public void TriggerGameOverUI()
    {
        // Apagamos la música de fondo en el instante que morimos y reproducimos Game Over
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopSound("GameTheme");
            AudioManager.instance.PlaySound("Game Over");
        }

        gameOverUIBG.LeanMoveLocalY(0f, .8f).setEaseOutBounce();
    }

    public void TriggerVictoryUI()
    {
        // Apagamos la música de fondo y reproducimos el sonido de victoria
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopSound("GameTheme");
            AudioManager.instance.PlaySound("victory");
        }

        victoryUIBG.LeanMoveLocalY(0f, .8f).setEaseInOutBack();
    }

    // --- LÓGICA DE PAUSA ---

    public void PauseGame()
    {
        isPaused = true;
        isTransitioning = true;

        if (pauseUIBG != null)
        {
            // Sube el BG al centro de la pantalla, ignorando que el tiempo se va a detener
            pauseUIBG.LeanMoveLocalY(0f, 0.6f)
                .setIgnoreTimeScale(true)
                .setEaseOutBounce()
                .setOnComplete(() => isTransitioning = false);
        }

        Time.timeScale = 0f; // Congela el juego
    }

    public void ResumeGame()
    {
        isTransitioning = true;

        if (pauseUIBG != null)
        {
            // Baja el BG de vuelta a -1200
            pauseUIBG.LeanMoveLocalY(-1200f, 0.5f)
                .setIgnoreTimeScale(true)
                .setEaseInBack()
                .setOnComplete(() => {
                    // Solo cuando termina la animación, reactivamos el juego
                    isPaused = false;
                    isTransitioning = false;
                    Time.timeScale = 1f;
                });
        }
        else
        {
            isPaused = false;
            isTransitioning = false;
            Time.timeScale = 1f;
        }
    }

    // Función para el botón "Menu"
    public void GoToMenu()
    {
        Time.timeScale = 1f; // Descongelar antes de cambiar de escena

        // ¡NUEVO: Apagamos la música de gameplay antes de salir!
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopSound("GameTheme");
            AudioManager.instance.StopSound("CreditsTheme");
            AudioManager.instance.StopSound("Game Over");

            AudioManager.instance.PlaySound("MenuTheme");
        }

        SceneManager.LoadScene(1);
    }

    // Función para el botón "Ver Créditos" o "Continuar" en la UI de Victoria
    public void GoToCredits()
    {
        Time.timeScale = 1f;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopSound("GameTheme");
            AudioManager.instance.PlaySound("CreditsTheme");
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("CreditsScene");
    }
}