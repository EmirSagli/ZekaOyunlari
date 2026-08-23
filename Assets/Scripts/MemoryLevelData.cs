using UnityEngine;

[CreateAssetMenu(fileName = "NewMemoryLevel", menuName = "BrainQuest/Memory Level Data")]
public class MemoryLevelData : ScriptableObject
{
    [Header("Bölüm Bilgisi")]
    public int levelNumber = 1;
    public int rows = 2;       // Satýr sayýsý
    public int columns = 2;    // Sütun sayýsý (rows * columns çift olmalýdýr)

    [Header("Yýldýz Hedefleri (Hamle = 2 Kart Açýmý)")]
    public int targetMoves3Stars = 3;
    public int targetMoves2Stars = 5;
}