using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelExit : MonoBehaviour
{
    [Header("Ссылки на UI")]
    public GameObject victoryPanel;  
    public TMP_Text statsText; 
    public GameObject gameplayUIPanel; 

    [Header("Ссылки на аудио")]
    public AudioSource backgroundMusicSource; 
    public AudioClip victoryMusic;  

    [Header("Эффекты")]
    public ParticleSystem[] fireworksEffects; // Массив салютов
    public Transform doorCenter;         // Точка центра двери

    [Header("Настройки переходов")]
    public string nextLevelName;
    public string mainMenuName = "MainMenu";

    private bool isLevelCompleted = false;
    private AudioSource localAudioSource;

    void Start()
    {
        // Автоматически готовим аудио-источник для победного трека на самом доме
        localAudioSource = GetComponent<AudioSource>();
        if (localAudioSource == null)
        {
            localAudioSource = gameObject.AddComponent<AudioSource>();
        }
        localAudioSource.playOnAwake = false;
        localAudioSource.loop = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isLevelCompleted)
        {
            isLevelCompleted = true;
            StartCoroutine(CompleteLevelRoutine(collision.gameObject));
        }
    }

    private IEnumerator CompleteLevelRoutine(GameObject player)
    {
        //  скрываем игровой интерфейс
        if (gameplayUIPanel != null)
        {
            gameplayUIPanel.SetActive(false);
        }

        //  ОСТАНАВЛИВАЕМ фоновую музыку уровня
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.Stop();
        }

        //  ЗАПУСКАЕМ победную музыку на доме
        if (victoryMusic != null && localAudioSource != null)
        {
            localAudioSource.clip = victoryMusic;
            localAudioSource.Play();
        }

        //  Отключаем управление игрока
        PlayerController playerMovement = player.GetComponent<PlayerController>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();

        if (playerMovement != null) playerMovement.enabled = false;

        if (playerRb != null)
        {
            playerRb.bodyType = RigidbodyType2D.Kinematic;
            playerRb.linearVelocity = Vector2.zero;
        }

        // Анимация всасывания в дверь
        Vector3 targetPos = doorCenter != null ? doorCenter.position : transform.position;
        Vector3 startPos = player.transform.position;
        Vector3 startScale = player.transform.localScale;

        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            player.transform.position = Vector3.Lerp(startPos, targetPos, t);
            player.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        player.SetActive(false);

        //Запускаем все салюты
        if (fireworksEffects != null)
        {
            foreach (ParticleSystem fw in fireworksEffects)
            {
                if (fw != null) fw.Play();
            }
        }

        // Ждем 1.5 секунды, пока гремит салют, перед показом меню победы
        yield return new WaitForSeconds(1.5f);

        // Включаем экран победы и пишем статистику звезд
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            // Получаем данные о собранных звездах из скрипта CoinCollector на игроке
            CoinCollector collector = player.GetComponent<CoinCollector>();
            if (collector != null && statsText != null)
            {
                statsText.text = $"The level is passed!\nCollected stars: {collector.CollectedCoins} / {collector.TotalCoinsInLevel}";
            }
        }
    }

    // Метод для кнопки "Следующий уровень"
    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
    }

    // Метод для кнопки "Главное меню"
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuName);
    }
}