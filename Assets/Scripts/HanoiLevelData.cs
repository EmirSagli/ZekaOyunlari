using UnityEngine;

[CreateAssetMenu(fileName = "NewHanoiLevel", menuName = "BrainQuest/Hanoi Level Data")]
public class HanoiLevelData : ScriptableObject
{
    [Header("Bölüm Ayarlarý")]
    public int levelNumber = 1;
    [Range(3, 6)] public int diskCount = 3; // Bölümde kaç disk olacak?

    [Header("Baþlangýç & Hedef Sütunu")]
    [Tooltip("0: Sol, 1: Orta, 2: Sað")]
    public int startPegIndex = 0;
    public int targetPegIndex = 2;

    [Header("Yýldýz Hedefleri")]
    public int targetMoves3Stars = 7;
    public int targetMoves2Stars = 11;
}