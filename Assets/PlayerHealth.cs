using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    [Header("UI")]
    public GameObject gameOverPanel; // Сюда перетащим панель

    void Start()
    {
        currentHealth = maxHealth;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Здоровье: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Игрок умер!");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Показываем экран смерти

            // Останавливаем время (по желанию)
            // Time.timeScale = 0f; 
        }
    }

    // Этот метод мы повесим на кнопку "Попробовать снова"
    public void RestartGame()
    {
        // Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}