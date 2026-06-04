/**---------------------------------------------------------------------------------
 * @file EventExtensions.cs
 * @date 2019/9/23
 * @author sejong
 * @brief 게임 내 이벤트의 널 처리등을 할 확장함수들의 관리 클레스
 *///-------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// @class EventExtensions
/// @date 2019/9/23
/// @author sejong
/// @brief 1. Action, Function, delegate 등의 확장함수 관리를 합니다.\n
/// 2. Coroutine 등의 확장 함수 관리를 합니다.
/// 3. weakrefrence 관련
/// </summary>
public static class EventExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Action함수에 대한 null 처리
	/// </summary>
	/// <param name="action">매개변수가 없는 Action</param>
	public static void SafeExcute( this Action action )
	{
		if( action == null )
		{
			return;
		}

		action();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Func함수에 대한 null 처리
	/// </summary>
	/// <param name="func">매개변수가 없는 Function</param>
	public static R SafeExcute<R>( this Func<R> func )
	{
		if( func == null )
		{
			return default(R);
		}

		return func();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Action함수에 대한 null 처리
	/// </summary>
	/// <typeparam name="T">매개변수의 Type</typeparam>
	/// <param name="action">매개변수가 하나 있는 Action</param>
	/// <param name="param1">파라미터</param>
	public static void SafeExcute<T>( this Action<T> action, T param1 )
	{
		if( action == null )
		{
			return;
		}

		action( param1 );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Action함수에 대한 null 처리
	/// </summary>
	/// <typeparam name="T1">매개변수의 param1 의 Type</typeparam>
	/// <typeparam name="T2">매개변수의 param2 의 Type</typeparam>
	/// <param name="action">매개변수가 두개 있는 Action</param>
	/// <param name="param1">첫번쨰 파라미터</param>
	/// <param name="param2">두번째 파라미터</param>
	public static void SafeExcute<T1,T2>( this Action<T1,T2> action, T1 param1, T2 param2 )
	{
		if( action == null )
		{
			return;
		}

		action( param1, param2 );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Action함수에 대한 null 처리
	/// </summary>
	/// <typeparam name="T1">매개변수의 param1 의 Type</typeparam>
	/// <typeparam name="T2">매개변수의 param2 의 Type</typeparam>
	/// <typeparam name="T3">매개변수의 param3 의 Type</typeparam>
	/// <param name="action">매개변수가 두개 있는 Action</param>
	/// <param name="param1">첫번쨰 파라미터</param>
	/// <param name="param2">두번째 파라미터</param>
	/// <param name="param3">세번째 파라미터</param>
	public static void SafeExcute<T1, T2, T3>( this Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3 )
	{
		if( action == null )
		{
			return;
		}

		action( param1, param2, param3 );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// Action함수에 대한 null 처리
	/// </summary>
	/// <typeparam name="T1">매개변수의 param1 의 Type</typeparam>
	/// <typeparam name="T2">매개변수의 param2 의 Type</typeparam>
	/// <typeparam name="T3">매개변수의 param3 의 Type</typeparam>
	/// <typeparam name="T4">매개변수의 param4 의 Type</typeparam>
	/// <param name="action">매개변수가 두개 있는 Action</param>
	/// <param name="param1">첫번쨰 파라미터</param>
	/// <param name="param2">두번째 파라미터</param>
	/// <param name="param3">세번째 파라미터</param>
	/// <param name="param4">네번째 파라미터</param>
	public static void SafeExcute<T1, T2, T3, T4>( this Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4 )
	{
		if( action == null )
		{
			return;
		}

		action( param1, param2, param3, param4 );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// Action함수에 대한 null 처리
	/// </summary>
	/// <typeparam name="T1">매개변수의 param1 의 Type</typeparam>
	/// <typeparam name="T2">매개변수의 param2 의 Type</typeparam>
	/// <typeparam name="T3">매개변수의 param3 의 Type</typeparam>
	/// <typeparam name="T4">매개변수의 param4 의 Type</typeparam>
	/// <typeparam name="T5">매개변수의 param5 의 Type</typeparam>
	/// <param name="action">매개변수가 두개 있는 Action</param>
	/// <param name="param1">첫번쨰 파라미터</param>
	/// <param name="param2">두번째 파라미터</param>
	/// <param name="param3">세번째 파라미터</param>
	/// <param name="param4">네번째 파라미터</param>
	/// <param name="param5">다섯번째 파라미터</param>
	public static void SafeExcute<T1, T2, T3, T4, T5>( this Action<T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5 )
	{
		if( action == null )
		{
			return;
		}

		action( param1, param2, param3, param4, param5 );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Action함수에 대한 null 처리
	/// </summary>
	/// <typeparam name="T1">매개변수의 param1 의 Type</typeparam>
	/// <typeparam name="T2">매개변수의 param2 의 Type</typeparam>
	/// <typeparam name="T3">매개변수의 param3 의 Type</typeparam>
	/// <typeparam name="T4">매개변수의 param4 의 Type</typeparam>
	/// <typeparam name="T5">매개변수의 param5 의 Type</typeparam>
	/// <typeparam name="T6">매개변수의 param6 의 Type</typeparam>
	/// <param name="action">매개변수가 두개 있는 Action</param>
	/// <param name="param1">첫번쨰 파라미터</param>
	/// <param name="param2">두번째 파라미터</param>
	/// <param name="param3">세번째 파라미터</param>
	/// <param name="param4">네번째 파라미터</param>
	/// <param name="param5">다섯번째 파라미터</param>
	/// <param name="param6">여섯번째 파라미터</param>
	public static void SafeExcute<T1, T2, T3, T4, T5, T6>( this Action<T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6 )
	{
		if( action == null )
		{
			return;
		}

		action( param1, param2, param3, param4, param5, param6 );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// StartCoroutine 에 대한 null 처리
	/// </summary>
	/// <param name="targetMonoBehaviour"></param>
	/// <param name="routineMethodName"></param>
	/// <returns></returns>
	public static Coroutine SafeStartCoroutine( this MonoBehaviour targetMonoBehaviour, string routineMethodName )
	{
		if( targetMonoBehaviour == null || string.IsNullOrEmpty( routineMethodName ) == true )
		{
			return null;
		}

		if( targetMonoBehaviour.gameObject.activeSelf == false )
        {
			return null;
        }

		return targetMonoBehaviour.StartCoroutine( routineMethodName );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// StartCoroutine 에 대한 null 처리
	/// </summary>
	/// <param name="targetMonoBehaviour"></param>
	/// <param name="routine"></param>
	/// <returns></returns>
	public static Coroutine SafeStartCoroutine( this MonoBehaviour targetMonoBehaviour, IEnumerator routine )
    {
		if( targetMonoBehaviour == null || routine == null )
        {
			return null;
        }

		if( targetMonoBehaviour.gameObject.activeSelf == false )
		{
			return null;
		}

		return targetMonoBehaviour.StartCoroutine( routine );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Coroutine 에 대한 null 처리
	/// </summary>
	/// <param name="targetCoroutine">정지시킬 Coroutine</param>
	/// <param name="targetMonoBehaviour">Coroutine이 돌고 있는 MonoBehaviour</param>
	public static void SafeStopCorutine( this Coroutine targetCoroutine, MonoBehaviour targetMonoBehaviour )
	{
		if( targetCoroutine == null || targetMonoBehaviour == null )
        {
            return;
        }

        targetMonoBehaviour.StopCoroutine( targetCoroutine );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// StopAllCoroutines 에 대한 null 처리
	/// </summary>
	/// <param name="targetMonoBehaviour"></param>
	public static void SafeStopAllCoroutines( this MonoBehaviour targetMonoBehaviour )
    {
		if( targetMonoBehaviour == null )
		{
			return;
		}

		targetMonoBehaviour.StopAllCoroutines();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 클래스 한정으로 WeakReference 값을 가져온다.
	/// </summary>
	/// <typeparam name="T">가져올 클래스</typeparam>
	/// <param name="wReference">타겟 WeakReference</param>
	/// <returns></returns>
	public static T SafeGetReference<T>( this WeakReference wReference ) where T : class
	{
		if( wReference == null )
		{
			return null;
		}

		return wReference.Target as T;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//class EventExtensions
