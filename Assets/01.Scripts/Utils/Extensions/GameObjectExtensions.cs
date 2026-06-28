/**---------------------------------------------------------------------------------
 * @file GameObjectExtensions.cs
 * @date 2023/7/12
 * @author sejong
 * @brief Unity GameObject 관련 Extension 
 *///-------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// @class GameObjectExtensions
/// @date 2023/7/12
/// @author sejong
/// @brief Unity GameObject 관련 Extension 
/// </summary>
public static class GameObjectExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	public static void SafeDestroy( GameObject instance )
	{
		if( instance == null )
			return;

#if UNITY_EDITOR
		if( true == Application.isPlaying )
			GameObject.Destroy( instance );
		else
			GameObject.DestroyImmediate( instance );
#else
        GameObject.Destroy(instance);
#endif
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 지정한 태그의 게임 오브젝트를 모두 제거합니다.
	/// </summary>
	/// <param name="tagName"></param>
	public static void DestoryTagedGameObjectAll( string tagName )
	{
		GameObject[] objs = GameObject.FindGameObjectsWithTag(tagName);

		foreach( GameObject obj in objs )
		{
			SafeDestroy( obj );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 게임오브젝트의 Active 상태를 입력받은 active 로 바꾸어 준다.
	/// </summary>
	/// <param name="targetGO">타겟 게임오브젝트</param>
	/// <param name="active">바꾸려는 active 상태</param>
	/// <returns>타겟 게임오브젝트의 active 상태</returns>
	public static bool SafeSetActive( this GameObject targetGO, bool active )
	{
		if( null == targetGO )
		{
			return false;
		}

		if( targetGO.activeSelf != active )
			targetGO.SetActive( active );

		return active;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 트렌스폼의 Active 상태를 입력받은 active 로 바꾸어 준다.
	/// </summary>
	/// <param name="targetTf">타겟 트렌스폼</param>
	/// <param name="active">바꾸려는 active 상태</param>
	/// <returns>타겟 트렌스폼의 active 상태</returns>
	public static bool SafeSetActive( this Transform targetTf, bool active )
	{
		if( null == targetTf )
		{
			return false;
		}

		if( targetTf.gameObject.activeSelf != active )
			targetTf.gameObject.SetActive( active );

		return active;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// MonoBehaviour의 Active 상태를 입력받은 active 로 바꾸어 준다.
	/// </summary>
	/// <param name="targetMono">타겟 MonoBehaviour</param>
	/// <param name="active">바꾸려는 active 상태</param>
	/// <returns>타겟 MonoBehaviour의 active 상태</returns>
	public static bool SafeSetActive( this MonoBehaviour targetMono, bool active )
	{
		if( null == targetMono || null == targetMono.gameObject )
		{
			return false;
		}

		if( targetMono.gameObject.activeSelf != active )
			targetMono.gameObject.SetActive( active );

		return active;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 게임오브젝트들의 Active 상태를 입력받은 active 로 바꾸어 준다.
	/// </summary>
	/// <param name="targetGOs">타겟 게임오브젝트</param>
	/// <param name="active">바꾸려는 active 상태</param>
	/// <returns>타겟 게임오브젝트들의 active 상태</returns>
	public static bool SafeSetActives( this List<GameObject> targetGOs, bool active )
	{
		if( null == targetGOs )
		{
			return false;
		}

		foreach( GameObject go in targetGOs )
		{
			go.SafeSetActive( active );
		}

		return active;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 게임오브젝트들의 Active 상태를 입력받은 active 로 바꾸어 준다.
	/// </summary>
	/// <param name="targetGOs">타겟 게임오브젝트</param>
	/// <param name="active">바꾸려는 active 상태</param>
	/// <returns>타겟 게임오브젝트들의 active 상태</returns>
	public static bool SafeSetActives( this GameObject[ ] targetGOs, bool active )
	{
		if( null == targetGOs )
		{
			return false;
		}

		foreach( GameObject go in targetGOs )
		{
			go.SafeSetActive( active );
		}

		return active;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// MonoBehaviour들의 Active 상태를 입력받은 active 로 바꾸어 준다.
	/// </summary>
	/// <param name="targetMonos">타겟 MonoBehaviour</param>
	/// <param name="active">바꾸려는 active 상태</param>
	/// <returns>타겟 MonoBehaviour들의 active 상태</returns>
	public static bool SafeSetActives<K, V>( this Dictionary<K, V> targetMonos, bool active ) where V : MonoBehaviour
	{
		if( targetMonos == null )
		{
			return false;
		}

		foreach( var it in targetMonos.Values )
		{
			it.SafeSetActive( active );
		}

		return active;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// MonoBehaviour들의 Active 상태를 입력받은 active 로 바꾸어 준다.
	/// </summary>
	/// <param name="targetMonos">타겟 MonoBehaviour</param>
	/// <param name="active">바꾸려는 active 상태</param>
	/// <returns>타겟 MonoBehaviour들의 active 상태</returns>
	public static bool SafeSetActives( this List<MonoBehaviour> targetMonos, bool active )
	{
		if( targetMonos == null )
		{
			return false;
		}

		foreach( var go in targetMonos )
		{
			go.SafeSetActive( active );
		}

		return active;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// MonoBehaviour들의 Active 상태를 입력받은 active 로 바꾸어 준다.
	/// </summary>
	/// <param name="targetMonos">타겟 MonoBehaviour</param>
	/// <param name="active">바꾸려는 active 상태</param>
	/// <returns>타겟 MonoBehaviour들의 active 상태</returns>
	public static bool SafeSetActives( this MonoBehaviour[ ] targetMonos, bool active )
	{
		if( targetMonos == null )
		{
			return false;
		}

		foreach( var go in targetMonos )
		{
			go.SafeSetActive( active );
		}


		return active;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 컴포넌트의 Enabled 상태를 입력 받은 enabled 로 바꾸어준다.
	/// </summary>
	/// <param name="targetBehaviour">타겟 컴포넌트</param>
	/// <param name="enabled">Enabled 값</param>
	/// <returns>Behaviour 컴포넌트의 enabled 상태</returns>
	public static bool SafeSetEnabled( this Behaviour targetBehaviour, bool enabled )
	{
		if( targetBehaviour == null )
		{
			return false;
		}

		if( targetBehaviour.enabled != enabled )
			targetBehaviour.enabled = enabled;

		return enabled;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// target 을 parent 아래로 붙입니다.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public static Transform SafeAttach( this Transform target, Transform parent )
	{
		if( null == target || null == parent || target.parent == parent )
		{
			return target;
		}

		target.SetParent( parent, false );

		target.localPosition	= Vector3.zero;
		target.localRotation	= Quaternion.identity;
		target.localScale		= Vector3.one;

		return target;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// target 을 parent 아래로 붙입니다.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public static GameObject SafeAttach( this GameObject target, GameObject parent )
	{
		if( null == target || null == parent )
		{
			return target;
		}

		target.transform.SafeAttach( parent.transform );

		return target;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// target 을 떼어 냅니다.
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public static Transform SafeDetach( this Transform target )
	{
		if( null == target || null == target.parent )
		{
			return target;
		}

		target.parent = null;

		return target;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// target 을 떼어 냅니다.
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public static GameObject SafeDetach( this GameObject target )
	{
		if( null == target || null == target.transform.parent )
		{
			return target;
		}

		target.transform.SafeDetach();

		return target;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// position, rotate, scale 을 초기화 합니다.
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public static Transform Reset( this Transform target )
	{
		if( null == target )
		{
			return target;
		}

		target.localPosition	= Vector3.zero;
		target.localRotation	= Quaternion.identity;
		target.localScale		= Vector3.one;

		return target;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// position, rotate, scale 을 초기화 합니다.
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public static GameObject Reset( this GameObject target )
	{
		if( null == target )
		{
			return target;
		}

		target.transform.Reset();

		return target;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//GameObjectExtensions
