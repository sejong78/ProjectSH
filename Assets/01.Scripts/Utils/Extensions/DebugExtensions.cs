using System;
using UnityEngine;

public static class DebugExtensions
{
    public static void Log(object message, Color color = default(Color))
    {
		if( color == default( Color ) )
			color = Color.white;

		Debug.Log(string.Format("<color={0}>{1}</color>", HexString(color), message));
    }

    public static void LogError(object message, Color color = default(Color))
    {
		if( color == default( Color ) )
			color = Color.white;
		
		Debug.LogError(string.Format("<color={0}>{1}</color>", HexString(color), message));
    }


    private static string HexString(Color aColor)
    {
        return HexString((Color32)aColor, true);
    }

    private static string HexString(Color aColor, bool includeAlpha)
    {
        return HexString((Color32)aColor, includeAlpha);
    }

    private static string HexString(Color32 aColor, bool includeAlpha)
    {
        string rs = Convert.ToString(aColor.r, 16).ToUpper();
        string gs = Convert.ToString(aColor.g, 16).ToUpper();
        string bs = Convert.ToString(aColor.b, 16).ToUpper();
        string a_s = Convert.ToString(aColor.a, 16).ToUpper();

        while (rs.Length < 2) rs = "0" + rs;
        while (gs.Length < 2) gs = "0" + gs;
        while (bs.Length < 2) bs = "0" + bs;
        while (a_s.Length < 2) a_s = "0" + a_s;

        if (includeAlpha)
        {
            return string.Format("#{0}{1}{2}{3}", rs, gs, bs, a_s);
        }

        return string.Format("#{0}{1}{2}", rs, gs, bs);
    }

	/// <summary>
	/// 디버그 로그용 스트링 빌더
	/// </summary>
	static private System.Text.StringBuilder _logStringBuilder = new System.Text.StringBuilder();

	/// <summary>
	/// 로그 스트링 빌더 클리어
	/// </summary>
	static public void ClearLog()
	{
		_logStringBuilder.Clear();
	}

	/// <summary>
	/// Append Log
	/// </summary>
	/// <param name="logMessage"></param>
	static public void AppendLog( string logMessage )
	{
		_logStringBuilder.Append( logMessage );
	}

	/// <summary>
	/// Append Log
	/// </summary>
	/// <param name="logMessage"></param>
	/// <param name="color"></param>
	static public void AppendLog( string logMessage, Color color = default( Color ) )
	{
		if( color == default( Color ) )
			color = Color.white;

		_logStringBuilder.Append( string.Format( "<color={0}>{1}</color>", HexString( color ), logMessage ) );
	}

	/// <summary>
	/// Append Log
	/// </summary>
	static public void AppendLineLog()
	{
		_logStringBuilder.AppendLine();
	}

	/// <summary>
	/// Append Log
	/// </summary>
	/// <param name="logMessage"></param>
	/// <param name="color"></param>
	static public void AppendLineLog( string logMessage, Color color = default( Color ) )
	{
		if( color == default( Color ) )
			color = Color.white;

		_logStringBuilder.AppendLine( string.Format( "<color={0}>{1}</color>", HexString( color ), logMessage ) );
	}

	/// <summary>
	/// Append 된 텍스트를 가져온다.
	/// </summary>
	/// <returns></returns>
	static public string GetAppendedString()
	{
		return _logStringBuilder.ToString();
	}

	/// <summary>
	/// 구성한 문자열을 Debug.Log로 남긴다.
	/// </summary>
	static public void WriteDebugLog()
	{
		Debug.Log( _logStringBuilder.ToString() );
		_logStringBuilder.Clear();
	}

	/// <summary>
	/// 구성한 문자열을 Debug.LogWarning로 남긴다.
	/// </summary>
	static public void WriteWarningLog()
	{
		Debug.LogWarning( _logStringBuilder.ToString() );
		_logStringBuilder.Clear();
	}

	/// <summary>
	/// 구성한 문자열을 Debug.LogError로 남긴다.
	/// </summary>
	static public void WriteErrorLog()
	{
		Debug.LogError( _logStringBuilder.ToString() );
		_logStringBuilder.Clear();
	}

}
