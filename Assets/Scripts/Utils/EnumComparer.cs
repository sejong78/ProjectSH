/**---------------------------------------------------------------------------------
* @file EnumComparer.cs
* @date 2021/5/26
* @author sejong
* @brief Enum 값의 int converting 을 사용한 IEqualityComparer 구현
*///-------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
 
/// <summary>
/// @class EnumComparer
/// @date 2021/5/26
/// @author sejong
/// @brief Enum 값의 int converting 을 사용한 IEqualityComparer 구현
/// </summary>
public class EnumEqualityComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : struct
{
	protected Func<TEnum,int> _convert = null;

	/// <summary>
	/// 생성자
	/// </summary>
	/// <param name="cvtFunc"></param>
	public EnumEqualityComparer( Func<TEnum, int> cvtFunc )
	{
		_convert = cvtFunc;
	}

	#region IEqualityComparer
	/// <summary>
	/// 
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public bool Equals( TEnum x, TEnum y )
	{
		return _convert( x ) == _convert( y );
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	public int GetHashCode( TEnum obj )
	{
		return ( _convert( obj ) ).GetHashCode();
	}

	#endregion//IEqualityComparer

	#region Default Function

	/// <summary>
	/// 
	/// </summary>
	public static EnumEqualityComparer<TEnum> Default = new EnumEqualityComparer<TEnum>( ConvertDefaultFunc );

	/// <summary>
	/// /
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	static public int ConvertDefaultFunc( TEnum e ) 
	{
		var i = Convert.ChangeType( e, typeof( int ) );

		return ( int )i;
	}

	#endregion//Default Function

}//class EnumEqualityComparer

//// <summary>
/// @class EnumComparer
/// @date 2021/6/7
/// @author sejong
/// @brief Enum 값의 int converting 을 사용한 IComparer 구현
/// </summary>
public class EnumComparer<TEnum> : IComparer<TEnum> where TEnum : struct
{
	protected Func<TEnum,int> _convert = null;

	/// <summary>
	/// 생성자
	/// </summary>
	/// <param name="cvtFunc"></param>
	public EnumComparer( Func<TEnum, int> cvtFunc )
	{
		_convert = cvtFunc;
	}

	#region IComparer

	public Int32 Compare( TEnum x, TEnum y )
	{
		return _convert( x ).CompareTo( _convert( y ) );
	}

	#endregion//IComparer

	#region Default Function
	/// <summary>
	/// 
	/// </summary>
	public static EnumComparer<TEnum> Default = new EnumComparer<TEnum>( ConvertDefaultFunc );

	/// <summary>
	/// /
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	static public int ConvertDefaultFunc( TEnum e )
	{
		var i = Convert.ChangeType( e, typeof( int ) );

		return ( int )i;
	}
	#endregion// Default Function

}