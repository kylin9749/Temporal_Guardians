using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TowerSkillCommon : MonoBehaviour
{
    protected towerCommon tower;  // 引用防御塔组件
    
    protected virtual void Awake()
    {
        tower = GetComponent<towerCommon>();
    }

    public abstract void CastSkill();
}
