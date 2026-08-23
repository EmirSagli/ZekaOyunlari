using UnityEngine;

[System.Serializable]
public struct ColorPairData
{
    public int colorId; // 0: Kýrmýzý, 1: Mavi, 2: Yeþil vb.
    public Vector2Int startPos; // Satýr, Sütun
    public Vector2Int endPos;   // Satýr, Sütun
}

[CreateAssetMenu(fileName = "NewCircuitLevel", menuName = "BrainQuest/Circuit Flow Level Data")]
public class CircuitFlowLevelData : ScriptableObject
{
    [Header("Bölüm Ayarlarý")]
    public int levelNumber = 1;
    public int gridSize = 5; // 5x5, 6x6, 7x7

    [Header("Renk Çiftleri")]
    public ColorPairData[] pairs;

    [Header("Yýldýz Hedefleri (Baðlantý Sayýsý)")]
    public int targetMoves3Stars = 4;
    public int targetMoves2Stars = 6;
}