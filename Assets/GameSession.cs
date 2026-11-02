using UnityEngine;

public static class GameSession
{
    // Здесь мы храним позицию игрока в 2D перед входом в домик
    public static Vector3 savedPlayerPosition = Vector3.zero;
    public static bool hasSavedPosition = false;
}
