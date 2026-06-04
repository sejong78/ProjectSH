using TMPro;

public static class TMP_TextExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 포맷에 맞춰 텍스트를 설정합니다.
	/// </summary>
	/// <param name="tmpText"></param>
	/// <param name="format"></param>
	/// <param name="args"></param>
	public static void SetTextWithFormat( this TMP_Text tmpText, string format, params object[ ] args )
	{
		if( null == tmpText )
			return;

		tmpText.text = string.Format( format, args );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 텍스트를 최대 길이로 자르고 "..."을 붙여서 설정합니다.
	/// </summary>
	/// <param name="tmpText"></param>
	/// <param name="text"></param>
	/// <param name="maxLength"></param>
	public static void SetTextWithClamp( this TMP_Text tmpText, string text, int maxLength )
	{
		if( null == tmpText )
			return;

		if( text.Length > maxLength )
		{
			tmpText.text = text.Substring( 0, maxLength ) + "...";
		}
		else
		{
			tmpText.text = text;
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//TMP_TextExtensions
