/**---------------------------------------------------------------------------------
 * @file StringExtensions.cs
 * @date 2020/7/21
 * @author sejong
 * @brief 게임 내 스트링 관련 확장함수들의 관리
 *///-------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// @class StringExtensions
/// @date 2020/7/21
/// @author sejong
/// @brief 게임 내 스트링 관련 확장함수들의 관리 클레스
/// </summary>
public static class StringExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 문자열에 값이 비어 있는가?
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public static bool IsNullorEmpty( this string target )
	{
		return string.IsNullOrEmpty( target );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 문자열에 매개변수를 적용하여 문자열을 완성한다.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="parms"></param>
	/// <returns></returns>
	public static string ApplyParameter( this string target, params object[] parms )
	{
		if( parms.Length < 1 )
		{
			return target;
		}

		return string.Format( target, parms );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 문자열에서 숫자를 제거합니다.
	/// </summary>
	/// <param name="input"></param>
	/// <returns></returns>
	public static string RemoveDigits( this string input )
	{
		if( true == string.IsNullOrEmpty( input ) )
			return input;

		return new string( input.Where( c => !char.IsDigit( c ) ).ToArray() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// long 값을 천 단위(K), 백만 단위(M), 십억 단위(B)로 변환하는 확장 함수
	/// </summary>
	public static string ToAbbreviatedString( this long value )
	{
		if( value >= 1_000_000_000 )
			return ( value / 1_000_000_000d ).ToString( "0.#" ) + " B";
		if( value >= 1_000_000 )
			return ( value / 1_000_000d ).ToString( "0.#" ) + " M";
		if( value >= 1_000 )
			return ( value / 1_000d ).ToString( "0.#" ) + " K";
		return value.ToString();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// int 값을 천 단위(K), 백만 단위(M), 십억 단위(B)로 변환하는 확장 함수
	/// </summary>
	public static string ToAbbreviatedString( this int value )
	{
		return ToAbbreviatedString( (long)value );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//class StringExtensions
