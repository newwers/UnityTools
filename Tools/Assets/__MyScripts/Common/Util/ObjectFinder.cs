using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zdq
{
    /// <summary>
    /// 场景对象查找器
    /// </summary>
    public class ObjectFinder : BaseMonoSingleClass<ObjectFinder>
    {
        [Header("玩家跟随虚拟相机")]
        public CinemachineVirtualCamera PlayerFollowCamera;
    }
}