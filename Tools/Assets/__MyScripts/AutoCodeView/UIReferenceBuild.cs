using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Z.UI
{
    public partial class UIReferenceBuild : MonoBehaviour
    {
        public UIReferenceComponent ui;
        void Start()
        {
            AwakeUI(ui);
            SetImage_RawImageRawImage(false);
        }

    }
}
