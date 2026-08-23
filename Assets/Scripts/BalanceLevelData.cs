using UnityEngine;

[System.Serializable]
public struct WeightData
{
    public int weightValue;   // Aðýrlýðýn deðeri (Örn: 1, 2, 3, 4)
    public Sprite itemSprite; // Aðýrlýk görseli (varsa)
}

[CreateAssetMenu(fileName = "BalanceLevel_1", menuName = "BrainGame/Balance Level Data")]
public class BalanceLevelData : ScriptableObject
{
    [Header("Seviye Bilgisi")]
    public int levelNumber = 1;

    [Header("Yýldýz Hedefleri (Hamle Sayýsý)")]
    public int optimalMoves3Stars = 4; // Parça sayýsý kadar ideal hamle
    public int targetMoves2Stars = 6;

    [Header("Oyuncuya Verilecek Tüm Aðýrlýklar")]
    public WeightData[] inventoryWeights;
}