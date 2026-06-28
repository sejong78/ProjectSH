using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public static class GameTime
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	private static long		_serverTimeSpan		= 0;
	private static float    _realTime			= 0f;
	private static double	_realTimeDouble		= 0;
	private static float    _realTimeLast		= 0f;
	private static double   _realTimeDoubleLast = 0;

	private static UniTaskHandler   _timeLoopHandler = null;

	public static DateTime UTCNow
	{
		get
		{
			return DateTime.UtcNow.AddTicks( _serverTimeSpan );
		}
	}

	public static bool IS_PAUSE { get; set; } = false;

	public static float RealTime
	{
		get { return _realTime; }
	}

	public static double RealTimeDouble
	{
		get { return _realTimeDouble; }
	}

	public static float RealTimeDelta
	{
		get 
		{
			if( true == IS_PAUSE )
				return 0f;

			return _realTime - _realTimeLast; 
		}
	}

	public static double RealTimeDoubleDelta
	{
		get 
		{
			if( true == IS_PAUSE )
				return 0;

			return _realTimeDouble - _realTimeDoubleLast; 
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	public static void Initialize()
	{
// 		// 서버 시간 동기화
// 		json.GetData<long>( "SERVER_TIME", val =>
// 		{
// 			_serverTimeSpan = val - DateTime.UtcNow.Ticks;
// 		} );

		_timeLoopHandler.SafeCancel();
		_timeLoopHandler = null;
		_timeLoopHandler = UniTaskWrapper.StartTask( GameTimeLoop );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	public static void Release()
	{
		_timeLoopHandler.SafeCancel();
		_timeLoopHandler = null;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 시간이 만료 됐는지 확인합니다.
	/// 0초 표기(0 ~ 0.999 초 까지) 문제로 1초 미만일경우 만료라 판정합니다.
	/// </summary>
	/// <param name="utcExpireTime"></param>
	/// <returns></returns>
	public static bool IsExpired( DateTime utcExpireTime )
	{
		return ( utcExpireTime - UTCNow ).TotalSeconds < 1;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// IsExpired 는 시작시간 검사를 안하고 있어 아래 메소드로 검사 진행합니다.
	/// <returns></returns>
	public static bool IsInTime( DateTime startTime, DateTime endTime )
	{
		DateTime dt = UTCNow;

		return !( dt < startTime || endTime < dt );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 경과 시간을 리턴합니다.
	/// </summary>
	/// <param name="utcTime"></param>
	/// <returns></returns>
	public static TimeSpan GetElapsTime( DateTime utcTime )
	{
		return ( UTCNow - utcTime );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 만료까지 남은 시간을 계산합니다. (UTC 기준)
	/// </summary>
	/// <param name="utcExpireTime"></param>
	/// <returns></returns>
	public static TimeSpan GetRemainTime( DateTime utcExpireTime )
	{
		return utcExpireTime - UTCNow;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// TimeScale 영향을 받지 않는 게임 시간 루프
	/// </summary>
	/// <param name="tk"></param>
	/// <returns></returns>
	private static async UniTaskVoid GameTimeLoop( CancellationToken tk )
	{
		while( !tk.IsCancellationRequested )
		{
			_realTimeLast		= _realTime;
			_realTimeDoubleLast = _realTimeDouble;

			_realTime		= Time.realtimeSinceStartup;
			_realTimeDouble = Time.realtimeSinceStartupAsDouble;

			// 다음 프레임까지 대기 (timeScale 영향 받지 않음)
			await UniTask.Yield( PlayerLoopTiming.Update, cancellationToken:tk );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//GameTime
