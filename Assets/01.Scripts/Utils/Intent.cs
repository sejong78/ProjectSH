/**---------------------------------------------------------------------------------
 * @file Intent.cs
 * @date 2024/3/2
 * @author sejong
 * @brief 정보 전달용 클래스 
 *///-------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

/// <summary>
/// @class Intent
/// @date 2024/3/2
/// @author sejong
/// @brief 정보 전달용 클래스 
/// </summary>
public sealed class Intent : DisposableBase
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	private Dictionary<string, object> _intentData = new Dictionary<string, object>();

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	public Intent()
	{
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	protected override void Release()
	{
		_intentData.Clear();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	public object this[ string key ]
	{
		get
		{
			object data;
			_intentData.TryGetValue( key, out data );

			return data;
		}

		set
		{
			_intentData[ key ] = value;
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public object this[ Enum key ]
	{
		get
		{
			object data;
			_intentData.TryGetValue( key.ToString(), out data );

			return data;
		}

		set
		{
			_intentData[ key.ToString() ] = value;
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public bool Contains( string key )
	{
		if( null == _intentData )
		{
			return false;
		}

		return _intentData.ContainsKey( key );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public bool Contains( Enum key )
	{
		return Contains( key.ToString() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void RemoveIntentData( Enum key )
	{
		RemoveIntentData( key.ToString() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void RemoveIntentData( string key )
	{
		if( _intentData.ContainsKey( key ) == false )
			return;

		_intentData.Remove( key );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void AddIntentData<T>( string key, T value )
	{
		_intentData.Add( key, value );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void AddIntentData<T>( Enum key, T value )
	{
		_intentData.Add( key.ToString(), value );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void AddIntentData( string key, object value )
	{
		_intentData.Add( key, value );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void AddIntentData( Enum key, object value )
	{
		_intentData.Add( key.ToString(), value );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void ChangeValue<T>( string key, T value )
	{
		if( false == _intentData.ContainsKey( key ) )
		{
			return;
		}

		if( typeof( T ) != _intentData[ key ].GetType() )
		{
			return;
		}

		_intentData[ key ] = value;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void ChangeValue<T>( Enum key, T value )
	{
		ChangeValue<T>( key.ToString(), value );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public T GetValue<T>( string key )
	{
		object o = null;
		_intentData.TryGetValue( key, out o );

		if( o != null && o.GetType() == typeof( T ) )
		{
			return ( T )o;
		}

		return default( T );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public T GetValue<T>( Enum key )
	{
		return GetValue<T>( key.ToString() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//Intent
