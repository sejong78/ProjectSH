/**---------------------------------------------------------------------------------
 * @file RandomExtensions.cs
 * @date 2022/8/11
 * @author sejong
 * @brief Random 확장함수
 *///-------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// @class RandomExtensions
/// @date 2022/8/11
/// @author sejong
/// @brief Random 확장함수
/// </summary>
public static class RandomExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
	
	private static readonly int		_percent		= 100;
	private static readonly float	_percentFloat	= 100f;

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 퍼센트값을 확률값으로 처리합니다.
	/// </summary>
	/// <param name="chance">0~100 사이의 값</param>
	/// <returns></returns>
	public static bool RandomPercent( int chance )
	{
		return Random.Range( 0, _percent ) < chance;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 퍼센트값을 확률값으로 처리합니다.(float)
	/// </summary>
	/// <param name="chance">0~100 사이의 값</param>
	/// <returns></returns>
	public static bool RandomPercent( float chance )
	{
		if( true == chance.Equals( _percentFloat ) )
		{
			return true;
		}

		return Random.Range( 0f, _percentFloat ) < chance;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 0 ~ 1 값을 확률값으로 처리합니다.
	/// </summary>
	/// <param name="chance">0 ~ 1 사이의 확률값</param>
	/// <returns></returns>
	public static bool Random01( float chance )
	{
		if( true == chance.Equals( 1f ) )
		{
			return true;
		}

		return Random.Range( 0, 1f ) < chance;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// Enum 값 안에서 Random 하게 하나의 값을 뽑는다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public static T RandomEnum<T>() where T : struct
	{
		var vals = System.Enum.GetValues( typeof( T ) );

		int len = vals.Length;

		string val = vals.GetValue((int)UnityEngine.Random.Range( 0, len )).ToString();

		System.Enum.TryParse<T>( val, out T rtn );

		return rtn;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	private static System.Random rand = null;

	public static void SetRandomSeed( int seed )
	{
		rand = new System.Random( seed );
	}

	public static IList<T> Shuffle<T>( this IList<T> target )
	{
		if( null == target || target.Count < 1 )
			return target;

		if( null == rand )
			SetRandomSeed( (int)GameTime.RealTime );

		int n = target.Count;

		while( n > 1 )
		{
			n--;
			int k = rand.Next(n + 1);
			T value = target[k];
			target[ k ] = target[ n ];
			target[ n ] = value;
		}

		return target;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//RandomExtensions
