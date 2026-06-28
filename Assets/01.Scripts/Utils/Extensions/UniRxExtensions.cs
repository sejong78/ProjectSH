/**---------------------------------------------------------------------------------
 * @file UniRxExtensions.cs
 * @date 2023/9/11
 * @author sejong
 * @brief UniRx 의 확장 함수
 *///-------------------------------------------------------------------------------
using System;
using UnityEngine;
using UniRx;

/// <summary>
/// @class UniRxExtensions
/// @date 2023/9/11
/// @author sejong
/// @brief UniRx 의 확장 함수 클레스
/// </summary>
public static class UniRxExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// waitTime 이후 onNext 실행
	/// </summary>
	/// <param name="onNext"></param>
	/// <param name="waitTime"></param>
	/// <returns></returns>
	public static IObservable<long> Timer( this Action onNext, float waitTime )
	{
		if( null == onNext )
			return null;

		return Timer( waitTime, onNext );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// waitTime 이후 onNext 실행
	/// </summary>
	/// <param name="waitTime"></param>
	/// <param name="onNext"></param>
	/// <returns></returns>
	public static IObservable<long> Timer( float waitTime, Action onNext )
	{
		if( null == onNext )
			return null;

		return Observable
			.Timer( TimeSpan.FromSeconds( waitTime ) )
			.Do( _ => onNext.SafeExcute() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// waitTime 이후 onNext 실행
	/// </summary>
	/// <param name="waitTime"></param>
	/// <param name="onNext"></param>
	/// <returns></returns>
	public static IObservable<long> Timer( float waitTime, Action<long> onNext )
	{
		if( null == onNext )
			return null;

		return Observable
			.Timer( TimeSpan.FromSeconds( waitTime ) )
			.Do( onNext );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// intervalTime 마다 onNext 실행
	/// </summary>
	/// <param name="onNext"></param>
	/// <param name="intervalTime"></param>
	/// <returns></returns>
	public static IObservable<long> Interval( this Action onNext, float intervalTime )
	{
		if( null == onNext )
			return null;

		return Interval( intervalTime, onNext);
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// intervalTime 마다 onNext 실행
	/// </summary>
	/// <param name="intervalTime"></param>
	/// <param name="onNext"></param>
	/// <returns></returns>
	public static IObservable<long> Interval( float intervalTime, Action onNext )
	{
		if( null == onNext )
			return null;

		return Observable
			.Interval( TimeSpan.FromSeconds( intervalTime ) )
			.Do( _ => onNext.SafeExcute() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// intervalTime 마다 onNext 실행
	/// </summary>
	/// <param name="intervalTime"></param>
	/// <param name="onNext"></param>
	/// <returns></returns>
	public static IObservable<long> Interval( float intervalTime, Action<long> onNext )
	{
		if( null == onNext )
			return null;

		return Observable
			.Interval( TimeSpan.FromSeconds( intervalTime ) )
			.Do( onNext );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// MonoBehaviour 의 Update 마다 onNext 실행
	/// </summary>
	/// <param name="onNext"></param>
	/// <returns></returns>
	public static IObservable<long> EveryUpdate( this Action onNext )
	{
		if( null == onNext )
			return null;

		return Observable.EveryUpdate().Do( _ => onNext.SafeExcute() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// MonoBehaviour 의 FixedUpdate 마다 onNext 실행
	/// </summary>
	/// <param name="onNext"></param>
	/// <returns></returns>
	public static IObservable<long> EveryFixedUpdate( this Action onNext )
	{
		if( null == onNext )
			return null;

		return Observable.EveryFixedUpdate().Do( _ => onNext.SafeExcute() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// MonoBehaviour 의 LateUpdate 마다 onNext 실행
	/// </summary>
	/// <param name="onNext"></param>
	/// <returns></returns>
	public static IObservable<long> EveryLateUpdate( this Action onNext )
	{
		if( null == onNext )
			return null;

		return Observable.EveryLateUpdate().Do( _ => onNext.SafeExcute() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// delayTime 이후 target 을 매개변수로 하는 onNext 실행
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="target"></param>
	/// <param name="delayTime"></param>
	/// <param name="onNext"></param>
	/// <returns></returns>
	public static IObservable<T> Delay<T>( this T target, float delayTime, Action<T> onNext )
	{
		if( null == target || null == onNext )
			return null;

		return Observable
			.Return( target )
			.Delay( TimeSpan.FromSeconds( delayTime ) )
			.Do( onNext );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//UniRxExtensions
