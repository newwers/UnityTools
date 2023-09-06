using UnityEngine;
using UnityEngine.UI;
using Z.UI;
namespace Z.UI
{
	public partial class TestUIBuild
	{
		private UnityEngine.RectTransform m_Image_RectTransform;
		private UnityEngine.UI.RawImage m_Image_RawImage;
		private Gif.Encode.GifTest m_Image_GifTest;
		//------------------------------------------------------
		public void AwakeUI(UIReferenceComponent ui)
		{
			if (ui == null) return;
			m_Image_RectTransform = ui.GetUI<UnityEngine.RectTransform>("Image_RectTransform");
			m_Image_RawImage = ui.GetUI<UnityEngine.UI.RawImage>("Image_RawImage");
			m_Image_GifTest = ui.GetUI<Gif.Encode.GifTest>("Image_GifTest");
		}
		//------------------------------------------------------
		public void SetImage_RawImageRawImage(bool active)
		{
			m_Image_RawImage.gameObject.SetActive(active);
		}
	}
}
