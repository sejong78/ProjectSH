/**---------------------------------------------------------------------------------
 * @file PopupManager.cs
 * @brief 
 *///-------------------------------------------------------------------------------
using UnityEngine;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

/// <summary>
/// @class PopupManager
/// @brief 
/// </summary>
public static class PopupManager
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	private static GameObject _rootGameObject;

	public static GameObject RootGameObject
	{
		get
		{
			if( _rootGameObject == null )
			{
				_rootGameObject = GameObject.FindWithTag( "RootPopup" );
			}

			return _rootGameObject;
		}
		set
		{
			_rootGameObject = value;
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	public static async Task<U> CreatePopup<T,U>( string popupName, GameObject root = null ) where U : UIPopupBase<T>
	{
		string address = $"Prefabs/Popup/{popupName}";

		GameObject prefabObj = await Resources.LoadAsync<GameObject>( address ) as GameObject;

		if( null == prefabObj )
		{
			return default(U);
		}

		// 인스턴스 생성 및 배치
		U instance = GameObject.Instantiate( prefabObj ).GetComponent<U>();

		if( null == instance )
			return default( U );

		var rt = ( null == root ) ? RootGameObject : root;

		instance.gameObject.SafeAttach( rt );

		return instance;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
}//PopupManager
