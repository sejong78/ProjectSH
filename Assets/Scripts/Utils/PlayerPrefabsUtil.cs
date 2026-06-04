using UnityEngine;

public static class PlayerPrefabsUtil
{
	private static string _token = "";

	public static void Init( string token )
	{
		_token = token;
	}

	private static string ConvertKey( string key )
	{
		return $"{_token}/{key}";
	}

	public static bool HasKey( string key )
	{
		return PlayerPrefs.HasKey( ConvertKey( key ) );
	}

	public static void SaveVector3( string key, Vector3 val )
	{
		string strVal = $"{val.x:0.##},{val.y:0.##},{val.z:0.##}";

		SaveString( key, strVal );
	}

	public static Vector3 LoadVector3( string key )
	{
		string val = LoadString( key );

		if( true == string.IsNullOrEmpty( val ) )
			return Vector3.zero;

		var vals = val.Split(',');

		if( vals.SafeCount() < 3 )
			return Vector3.zero;

		return new Vector3( float.Parse( vals[ 0 ] ), float.Parse( vals[ 1 ] ), float.Parse( vals[ 2 ] ) );
	}


	public static void SaveString( string key, string val )
	{
		PlayerPrefs.SetString( ConvertKey( key ), val );
		PlayerPrefs.Save();
	}

	public static string LoadString( string key, string defaultVal = "" )
	{
		key = ConvertKey( key );

		if( false == PlayerPrefs.HasKey( key ) )
			return defaultVal;

		return PlayerPrefs.GetString( key );
	}

	public static void SaveInt( string key, int val )
	{
		PlayerPrefs.SetInt( ConvertKey( key ), val );
		PlayerPrefs.Save();
	}

	public static int LoadInt( string key, int defaultVal = 0 )
	{
		key = ConvertKey( key );

		if( false == PlayerPrefs.HasKey( key ) )
			return defaultVal;

		return PlayerPrefs.GetInt( key );
	}

	public static void SaveFloat( string key, float val )
	{
		PlayerPrefs.SetFloat( ConvertKey( key ), val );
		PlayerPrefs.Save();
	}

	public static float LoadFloat( string key, float defaultVal = 0f )
	{
		key = ConvertKey( key );

		if( false == PlayerPrefs.HasKey( key ) )
			return defaultVal;

		return PlayerPrefs.GetFloat( key );
	}

	public static void SaveBool( string key, bool val )
	{
		PlayerPrefs.SetInt( ConvertKey( key ), val ? 1 : 0 );
		PlayerPrefs.Save();
	}

	public static bool LoadBool( string key, bool defaultVal = false )
	{
		key = ConvertKey( key );

		if( false == PlayerPrefs.HasKey( key ) )
			return defaultVal;

		return 0 < PlayerPrefs.GetInt( key );
	}

}
