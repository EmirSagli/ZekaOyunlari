using UnityEngine;

[CreateAssetMenu(fileName = "NewPuzzleLevel", menuName = "BrainQuest/Puzzle Level Data")]
public class PuzzleLevelData : ScriptableObject
{
    [Header("Bölüm Bilgileri")]
    public int levelNumber;
    [Range(2, 4)] public int gridSize = 3; // 2 -> 2x2, 3 -> 3x3, 4 -> 4x4

    [Header("Zorluk & Karýþtýrma")]
    [Tooltip("Tahtanýn çözülebilir þekilde kaç hamle karýþtýrýlacaðý")]
    public int shuffleSteps = 10;

    [Header("Yýldýz Hedefleri (Hamle Sayýsý)")]
    public int targetMoves3Stars = 10;
    public int targetMoves2Stars = 18;

    [Header("Görseller (Opsiyonel)")]
    [Tooltip("Eðer parçalar resimli olacaksa parça görselleri (GridSize x GridSize - 1 adet)")]
    public Sprite[] puzzleSprites;
}