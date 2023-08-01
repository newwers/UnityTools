using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z.Data
{
    public partial class DataManager: ADataManager
    {
        //µ¥Àý

        private static DataManager ms_pInstance = null;

        public static DataManager Instance
        {
            get
            {
                if (ms_pInstance == null)
                {
                    ms_pInstance = new DataManager();
                }
                return ms_pInstance;
            }
        }

        protected override void OnAwake()
        {
            
        }

        protected override void OnDestroy()
        {
            ms_pInstance = null;
        }

    }
}
