using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "FightingGame/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHealth = 100;

    [Header("Phase Patterns")]
    // Kita ganti KeyCode menjadi int agar yang muncul di Inspector adalah angka
    public List<int> keysPerPhase;

    [Header("Defense Phase")]
    public float sliderSpeed = 2f;
}