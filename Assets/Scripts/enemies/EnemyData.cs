using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Tower Defense/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float maxHealth = 1000f;           // 最大生命值
    public float distanceToEnd;              // 离终点的距离
    public int coinsDrop = 10;               // 死亡掉落金币数
    public float originalMoveSpeed = 1f;
    public EnemyType enemyType;
    public bool preferTurn = false;       // 是否偏好转弯
    public Sprite enemySprite;
}