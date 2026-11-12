using UnityEngine;
public class Note : MonoBehaviour
{
    public void Read()
    {
        GameSession.collectedNotes++;
        Debug.Log("Записок собрано: " + GameSession.collectedNotes);
        Destroy(gameObject);
    }
}