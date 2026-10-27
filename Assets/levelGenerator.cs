using UnityEngine;

public class GameGenerator : MonoBehaviour
{
    [Header("Настройки Фона")]
    public GameObject[] backgroundPrefabs; // Массив префабов фона
    public Transform backgroundSpawnPoint; // Точка, где появится фон (обычно 0,0,0)

    [Header("Настройки Объектов")]
    public GameObject[] objectPrefabs;     // Массив префабов игровых объектов
    public int minObjects = 5;             // Минимальное количество объектов
    public int maxObjects = 15;            // Максимальное количество объектов

    [Header("Зона генерации объектов")]
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;

    void Start()
    {
        GenerateBackground();
        GenerateObjects();
    }

    void GenerateBackground()
    {
        if (backgroundPrefabs.Length == 0) return;

        // Выбираем случайный индекс фона из массива
        int randomIndex = Random.Range(0, backgroundPrefabs.Length);

        // Создаем случайный фон в заданной точке без вращения
        Instantiate(backgroundPrefabs[randomIndex], backgroundSpawnPoint.position, Quaternion.identity);
    }

    void GenerateObjects()
    {
        if (objectPrefabs.Length == 0) return;

        // Определяем случайное количество объектов для этой игровой сессии
        // (Исправлено: убрана строка с несуществующими minXCount() и maxXCount())
        int finalCount = Random.Range(minObjects, maxObjects + 1);

        for (int i = 0; i < finalCount; i++)
        {
            // Выбираем случайный объект из списка
            int randomIndex = Random.Range(0, objectPrefabs.Length);

            // Генерируем случайную позицию в заданных границах
            Vector2 randomPosition = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            // Создаем объект
            Instantiate(objectPrefabs[randomIndex], randomPosition, Quaternion.identity);
        }
    }
}