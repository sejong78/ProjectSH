using UnityEngine;
using UnityEngine.UI;

public static class ImageExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 이미지를 세팅한다.
	/// </summary>
	/// <param name="img"></param>
	/// <param name="path"></param>
	/// <returns></returns>
	public static bool SetImage( this Image img, string path )
	{
		if( null == img || true == path.IsNullorEmpty() )
			return false;

		// 리소스를 로딩에서 img 에 세팅한다.
		Sprite sp = Resources.Load<Sprite>( path );

		if( null == sp )
		{
#if UNITY_EDITOR
			Debug.LogError( string.Format( "[ImageExtensions] SetImage - 리소스 로딩 실패: {0}", path ) );
#endif//UNITY_EDITOR
			return false;
		}

		img.sprite = sp;

		return true;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//ImageExtensions
