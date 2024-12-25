using UnityEngine;

public class BoomSkill : TowerSkillCommon
{
    public override void CastSkill()
    {
        GameObject target = tower.CurrentTarget;
        if (target != null)
        {
            // 加载预制体并实例化
            GameObject bombPrefab = Resources.Load<GameObject>("Bullet/Boom");
            GameObject bomb = Instantiate(bombPrefab);
            bomb.transform.position = target.transform.position;

            // 将炸弹设置为目标的子物体
            bomb.transform.SetParent(target.transform);
            bomb.transform.localPosition = Vector3.zero;  // 相对于父物体的位置设为原点

            // 添加爆炸组件（替换原来的BombFollower）
            BombEffect bombEffect = bomb.AddComponent<BombEffect>();
            bombEffect.damage = tower.TowerData.skillDamage;
            bombEffect.explosionRange = tower.TowerData.skillRange;
            bombEffect.explosionDelay = tower.TowerData.skillCastTime;
            bombEffect.tower = tower;

            // 重置MP和技能状态
            tower.CurrentMp = 0;
            tower.IsSkilling = false;
        }
    }
}
