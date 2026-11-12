using UnityEngine;

public static class GameSession
{
    // Здесь мы храним позицию игрока в 2D перед входом в домик
    public static Vector3 savedPlayerPosition = Vector3.zero;
    public static bool hasSavedPosition = false;
    // ДЛЯ СЮЖЕТА
    public static int collectedArtifacts = 0; // Сколько собрали
    public static int maxArtifacts = 9;       // Сколько всего нужно собрать (ограничение)
    // Для записок и концовки
    public static int collectedNotes = 0;
    public static int maxNotes = 3; // Укажите, сколько всего записок в игре
}
