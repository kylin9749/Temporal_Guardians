using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemySkillCommon : MonoBehaviour
{
    protected EnemyCommon enemy;  // 引用防御塔组件
    
    protected virtual void Awake()
    {
        enemy = GetComponent<EnemyCommon>();
    }

    public abstract void CastSkill();
}
