/**---------------------------------------------------------------------------------
 * @file ColorExtensions.cs
 * @date 2022/8/11
 * @author sejong
 * @brief Color <--> string
 *///-------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Text.RegularExpressions;

/// <summary>
/// @class ColorExtensions
/// @date 2022/8/11
/// @author sejong
/// @brief Color <--> string
/// </summary>
public static class ColorExtensions 
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 0 ~ 255 사이의 값으로 구성된 컬러값을 파싱합니다.
	/// 예) "255,255,255,255 ( r,g,b,a )"
	/// </summary>
	/// <param name="color"></param>
	/// <param name="value"></param>
	public static Color ParseFromIntValues( string value )
	{
		Color color = Color.white;

        string[] strings = value.Split(',');

        if( null == strings || strings.Length < 1 )
            return color;

        int length = Mathf.Min( strings.Length, 4 );
		int c = 0;

		for( int i = 0; i < length; ++i )
		{
			if( false == int.TryParse( strings[ i ], out c ) )
			{
				continue;
			}

			color[ i ] = Mathf.Clamp01( c / 255f );
		}

		return color;
    }

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Color.ToString 으로 변환된 컬러값을 파싱합니다.
	/// 예) "RGBA(1.0, 1.0, 0.35, 1.0)"
	/// </summary>
	/// <param name="color"></param>
	/// <param name="value"></param>
	public static void Parse( this Color color, string value )
	{
		//Takes strings formatted with numbers and no spaces before or after the commas:
		// "RGBA(1.0, 1.0, 0.35, 1.0)"
		if( false == Regex.IsMatch( value, "^RGBA.*.$" ) )
		{
			return;
		}

		string[] strings = value.Substring( 5, value.Length - 6 ).Split(',');

		if( null == strings )
		{
			return;
		}

		int length = Mathf.Min( strings.Length, 4 );

		for( int i = 0; i < length; ++i )
		{
			color[ i ] = Single.Parse( strings[ i ] );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
}