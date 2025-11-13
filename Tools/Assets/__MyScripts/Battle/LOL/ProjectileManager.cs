using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Z.DefenseTower
{
    public class ProjectileManager : BaseMonoSingleClass<ProjectileManager>
    {
        public Projectile projectilePrefab;


        private List<Projectile> m_Projectiles = new List<Projectile>();
        


        void Start()
        {
            //管理所有投掷物的创建 ，update 和 回收
        }

        
        void Update()
        {
            for (int i = 0; i < m_Projectiles.Count; i++)
            {
                m_Projectiles[i].OnUpdate();
            }
        }

        public Projectile GetProjectile()
        {
            foreach (Projectile p in m_Projectiles)
            {
                if (!p.IsUse)
                {
                    return p;
                }
            }

            var projectile = GameObject.Instantiate<Projectile>(projectilePrefab);//todo:这边要接入资源管理系统加载资源
            m_Projectiles.Add(projectile);

            return projectile;
        }

        public void RecycleProjectile(Projectile projectile)
        {
            projectile.OnRecycle();
            if (!m_Projectiles.Contains(projectile))
            {
                m_Projectiles.Add(projectile);
            }
        }
    }
}
