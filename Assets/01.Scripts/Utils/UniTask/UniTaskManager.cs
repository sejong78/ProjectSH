/**---------------------------------------------------------------------------------
 * @file UniTaskManager.cs
 * @date 2022/7/16
 * @author sejong
 * @brief UniTask 의 래핑 클래스
 *///-------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseSingleton;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// @class UniTaskManager
/// @date 2022/7/16
/// @author sejong
/// @brief UniTask 의 래핑 클래스
/// </summary>
public class UniTaskManager : BaseSingleton<UniTaskManager>, IBaseSingleton
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// CancellationToken 관리용 딕셔너리
	/// </summary>
	private Dictionary<string,CancellationTokenSource> _cancelTokenDic = new Dictionary<string, CancellationTokenSource>();

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 뭐라도 하나 끝날때 까지 기다린다.\n
	/// WhenAny 가 종료 되더라도, tasks 비동기 함수들은 계속 실행 된다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="cancleKey"></param>
	/// <param name="tasks"></param>
	/// <returns>(int:가장 먼저 끝난 비동기 함수 인덱스, T:그 결과)</returns>
	public async UniTask<(int index, T result)> WhenAny<T>( string cancleKey, params UniTask<T>[] tasks )
	{
		if( null == tasks || tasks.Length < 1 )
			return ( 0, default(T) );

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			return await UniTask.WhenAny( tasks ).AttachExternalCancellation( cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] WhenAny = {ex.Message}" );
#endif//DEBUG
			return (0, default( T ));
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 뭐라도 하나 끝날때 까지 기다린다.\n
	/// WhenAny 가 종료 되더라도, tasks 비동기 함수들은 계속 실행 된다.
	/// </summary>
	/// <param name="cancleKey"></param>
	/// <param name="tasks"></param>
	/// <returns>가장 먼저 끝난 비동기 함수 인덱스</returns>
	public async UniTask<int> WhenAny( string cancleKey, params UniTask[] tasks )
	{
		if( null == tasks || tasks.Length < 1 )
			return 0;

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			return await UniTask.WhenAny( tasks ).AttachExternalCancellation( cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] WhenAny = {ex.Message}" );
#endif//DEBUG
			return 0;
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// tasks 등록된 모든 비동기 함수가 끝날때 까지 기다린다.
	/// </summary>
	/// <param name="cancleKey"></param>
	/// <param name="tasks"></param>
	/// <returns></returns>
	public async UniTask WhenAll( string cancleKey, params UniTask[] tasks )
	{
		if( null == tasks || tasks.Length < 1 )
			return;

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );
	
			await UniTask.WhenAll( tasks ).AttachExternalCancellation( cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] WhenAll = {ex.Message}" );
#endif//DEBUG
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// tasks 등록된 모든 비동기 함수가 끝날때 까지 기다린다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="cancleKey"></param>
	/// <param name="tasks"></param>
	/// <returns></returns>
	public async UniTask<T[]> WhenAll<T>( string cancleKey, params UniTask<T>[] tasks )
	{
		if( null == tasks || tasks.Length < 1 )
			return default( T[] );

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			return await UniTask.WhenAll( tasks ).AttachExternalCancellation( cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] WhenAll = {ex.Message}" );
#endif//DEBUG
			return default( T[] );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 일반 동기 Action 을 비동기로 처리한다.
	/// </summary>
	/// <param name="syncAction"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask Run( Action syncAction, string cancleKey = "" )
	{
		if( null == syncAction )
			return;

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			await UniTask.RunOnThreadPool( syncAction, cancellationToken: cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] Run (Action)= {ex.Message}" );
#endif//DEBUG
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 일반 동기 Function 을 비동기로 처리한다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="syncFunction"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask<T> Run<T>( Func<T> syncFunction, string cancleKey = "" )
	{
		if( null == syncFunction )
			return default(T);

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );
	
			return await UniTask.RunOnThreadPool( syncFunction, cancellationToken: cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] Run (Function)= {ex.Message}" );
#endif//DEBUG

			return default(T);
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 코루틴 함수를 비동기로 처리한다.
	/// </summary>
	/// <param name="corutineFunction"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask Run( IEnumerator corutineFunction, string cancleKey = "" )
	{
		if( null == corutineFunction )
			return;

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			await corutineFunction.WithCancellation( cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] Run (Coroutine)= {ex.Message}" );
#endif//DEBUG

			return;
		}

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 특정 타이밍 까지 대기
	/// </summary>
	/// <param name="loopTimming"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask Yield( PlayerLoopTiming loopTimming = PlayerLoopTiming.Update, string cancleKey = "" )
	{
		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );
	
			await UniTask.Yield( loopTimming, cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] Yield = {ex.Message}" );
#endif//DEBUG
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate) 와 완전히 동일하다.
	/// </summary>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask WaitForEndOfFrame( string cancleKey = "" )
	{
		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );
#if UNITY_2023_1_OR_NEWER
			if( default == cancelToken )
				await UniTask.WaitForEndOfFrame();
			else
				await UniTask.WaitForEndOfFrame( cancelToken );
#else//UNITY_2023_1_OR_NEWER
			await UniTask.Yield( PlayerLoopTiming.LastPostLateUpdate, cancelToken );
#endif//UNITY_2023_1_OR_NEWER
		}
		catch (System.Exception ex)
		{
#if DEBUG
			Debug.Log( $"[Exception] WaitForEndOfFrame = {ex.Message}" );
#endif//DEBUG
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// UniTask.Yield(PlayerLoopTiming.FixedUpdate) 와 완전히 동일하다.
	/// </summary>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask WaitForFixedUpdate( string cancleKey = "" )
	{
		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			if( default == cancelToken )
				await UniTask.WaitForFixedUpdate();
			else
				await UniTask.WaitForFixedUpdate( cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] WaitForFixedUpdate = {ex.Message}" );
#endif//DEBUG
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 모니터링 하는 값이 변경 될 때 까지 대기
	/// T 클레스의 U값이 변경될 때 까지 대기
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="U"></typeparam>
	/// <param name="target"></param>
	/// <param name="monitorFunc"></param>
	/// <param name="equalityComparer"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask<U> WaitUntilValueChanged<T,U>( T target, Func<T, U> monitorFunc, IEqualityComparer<U> equalityComparer = null, string cancleKey = "" ) where T : class
	{
		if( null == target || null == monitorFunc )
			return default(U);

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );
	
			return await UniTask.WaitUntilValueChanged( target, monitorFunc, equalityComparer: equalityComparer, cancellationToken: cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] WaitUntilValueChanged = {ex.Message}" );
#endif//DEBUG

			return default( U );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 캔슬 될때 까지 대기
	/// </summary>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask WaitUntilCanceled( string cancleKey )
	{
		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );
	
			// 등록이 안된 토큰은 사용할 수 없다.
			if( default == cancelToken )
				return;
	
			await UniTask.WaitUntilCanceled( cancellationToken: cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] WaitUntilCanceled = {ex.Message}" );
#endif//DEBUG

		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 해당 조건을 만족할때까지 대기\n
	/// conditionFunc 이 true 를 리턴할때 까지 대기
	/// </summary>
	/// <param name="conditionFunc"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask WaitUntil( Func<bool> conditionFunc, string cancleKey = "" )
	{
		if( null == conditionFunc )
			return;

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );
	
			await UniTask.WaitUntil( conditionFunc, cancellationToken: cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] WaitUntil = {ex.Message}" );
#endif//DEBUG

		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 해당 조건을 만족하는 동안 대기\n
	/// conditionFunc 이 false 를 리턴할때 까지 대기
	/// </summary>
	/// <param name="conditionFunc"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask WaitWhile( Func<bool> conditionFunc, string cancleKey = "" )
	{
		if( null == conditionFunc )
			return;

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );
	
			await UniTask.WaitWhile( conditionFunc, cancellationToken: cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] WaitWhile = {ex.Message}" );
#endif//DEBUG

		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 딜레이 프레임
	/// </summary>
	/// <param name="frames"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask DelayFrames( int frames, string cancleKey = "" )
	{
		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			// 딜레이
			await UniTask.DelayFrame( frames, cancellationToken: cancelToken );

		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] DelayFrame = {ex.Message}" );
#endif//DEBUG

		}

	}


	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 딜레이 밀리초
	/// </summary>
	/// <param name="milliseconds"></param>
	/// <param name="ignoreTimeScale"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask Delay( int milliseconds, bool ignoreTimeScale = false, string cancleKey = "" )
	{
		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			// 딜레이
			await UniTask.Delay( milliseconds, ignoreTimeScale, cancellationToken: cancelToken );

		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] Delay = {ex.Message}" );
#endif//DEBUG

		}

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Unity 통신 Get
	/// 서버의 데이터나 상태를 변경시키지 않아야 하기 때문에 주로 조회를 할 때에 사용해야 힌다. 
	/// 브라우저에서 웹페이지를 열어보거나 게시글을 읽는 등 조회시 사용
	/// </summary>
	/// <param name="url"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask<DownloadHandler> WebRequest_Get( string url, string cancleKey = "", Action<UnityWebRequest> setHeader = null, Action<float> onProgress = null )
	{
		// Url 이 비어 있다면 무시한다.
		if( true == string.IsNullOrEmpty( url ) )
			return null;
		
		try
		{
			// UnityWebRequest 수령
			UnityWebRequest wr = UnityWebRequest.Get( url );

			// Header 정보가 있다면 세팅
			setHeader.SafeExcute( wr );

			// 공통처리
			bool isSuccess = await ProcessWebRequestResult( wr, cancleKey:cancleKey, onProgress: onProgress );

			if( false == isSuccess )
				return null;

			// 결과
			return wr.downloadHandler;
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] UnityWebRequest_Get = {ex.Message}" );
#endif//DEBUG
			return null;
		}

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Unity 통신 Post
	/// 서버의 상태나 데이터를 변경시킬 때 사용됨.
	/// 게시글을 써서 서버에 게시글을 저장하거나, 게시글을 삭제하여 해당 데이터가 없어지는 등 서버의 무언가가 변경될때 사용
	/// </summary>
	/// <param name="url"></param>
	/// <param name="setBody"></param>
	/// <param name="cancleKey"></param>
	/// <param name="onProgress"></param>
	/// <returns></returns>
	public async UniTask<DownloadHandler> WebRequest_Post( string url, Func<WWWForm> setBody, string cancleKey = "", Action<float> onProgress = null )
	{
		// Url 이 비어 있다면 무시한다.
		if( true == string.IsNullOrEmpty( url ) )
			return null;

		try
		{
			// Body Data 를 체운다.
			WWWForm body = setBody.SafeExcute();

			// UnityWebRequest 수령
			UnityWebRequest wr = UnityWebRequest.Post( url, body );

			// 공통처리
			bool isSuccess = await ProcessWebRequestResult( wr, cancleKey:cancleKey, onProgress: onProgress );

			if( false == isSuccess )
				return null;

			// 결과
			return wr.downloadHandler;
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] UnityWebRequest_Post = {ex.Message}" );
#endif//DEBUG
			return null;
		}

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// WebRequest 의 결과 처리
	/// </summary>
	/// <param name="wr"></param>
	/// <returns></returns>
	private async UniTask<bool> ProcessWebRequestResult( UnityWebRequest wr, string cancleKey = "", Action<float> onProgress = null )
	{
		if( null == wr )
			return false;

		try
		{
			// 캔슬 토큰
			var cancelToken = GetCancellationToken( cancleKey );
	
			// 진행도
			IProgress<float> prog = onProgress != null ? Progress.Create<float>( onProgress ) : null;
	
			// SendWebRequest 실행
			UnityWebRequestAsyncOperation wrao = wr.SendWebRequest();
	
			if( default == cancelToken && null == prog )
				await wrao;
			else
				await wrao.ToUniTask( progress: prog, cancellationToken: cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.LogError( $"[Error] ProcessWebRequestResult Fail!! => {ex.Message}" );
#endif//DEBUG
			return false;
		}

		switch( wr.result )
		{
			case UnityWebRequest.Result.InProgress:
			{
#if DEBUG
				Debug.LogError( $"[Error] UnityWebRequest InProgress!!" );
#endif//DEBUG
				return false;
			}

			case UnityWebRequest.Result.ConnectionError:
			case UnityWebRequest.Result.DataProcessingError:
			case UnityWebRequest.Result.ProtocolError:
			{
#if DEBUG
				Debug.LogError( $"[Error] UnityWebRequest result = {wr.result}\n - {wr.error}" );
#endif//DEBUG
				return false;
			}

			case UnityWebRequest.Result.Success:
				break;
		}

		return true;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 씬을 비동기로 로딩한다.
	/// </summary>
	/// <param name="sceneName"></param>
	/// <param name="mode"></param>
	/// <param name="cancleKey"></param>
	/// <param name="onProgress"></param>
	/// <returns></returns>
	public async UniTask LoadScene( string sceneName, LoadSceneMode mode = LoadSceneMode.Single, string cancleKey = "", Action<float> onProgress = null )
	{
		// sceneName 이 비어 있다면 무시한다.
		if( true == string.IsNullOrEmpty( sceneName ) )
			return;

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );
	
			// 진행도
			IProgress<float> prog = onProgress != null ? Progress.Create<float>( onProgress ) : null;

			// 씬 로드 진행
			AsyncOperation ao = SceneManager.LoadSceneAsync( sceneName, mode );

			if( default == cancelToken && null == prog )
				await ao;
			else
				await ao.ToUniTask( progress: prog, cancellationToken: cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] SceneManager.LoadSceneAsync = {ex.Message}" );
#endif//DEBUG

		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 씬을 비동기로 언로딩 한다.
	/// </summary>
	/// <param name="sceneName"></param>
	/// <param name="option"></param>
	/// <param name="cancleKey"></param>
	/// <param name="onProgress"></param>
	/// <returns></returns>
	public async UniTask UnLoadScene( string sceneName, UnloadSceneOptions option = UnloadSceneOptions.None, string cancleKey = "", Action<float> onProgress = null )
	{
		// sceneName 이 비어 있다면 무시한다.
		if( true == string.IsNullOrEmpty( sceneName ) )
			return;

		try
		{
			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			// 진행도
			IProgress<float> prog = onProgress != null ? Progress.Create<float>( onProgress ) : null;

			// 씬 언로드 진행
			AsyncOperation ao = SceneManager.UnloadSceneAsync( sceneName, option );

			if( default == cancelToken && null == prog )
				await ao;
			else
				await ao.ToUniTask( progress: prog, cancellationToken: cancelToken );
		}
		catch( System.Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] SceneManager.UnloadSceneAsync = {ex.Message}" );
#endif//DEBUG

		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 리소스 로딩을 비동기로 처리한다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="path"></param>
	/// <param name="cancleKey"></param>
	/// <returns></returns>
	public async UniTask<T> LoadResource<T>( string path, string cancleKey = "", Action<float> onProgress = null ) where T : UnityEngine.Object
	{
		// path 가 비어 있다면 무시한다.
		if( true == string.IsNullOrEmpty( path ) )
			return null;

		try
		{
			var res = default( UnityEngine.Object );

			// 캔슬 토큰 값이 없으면 default 
			var cancelToken = GetCancellationToken( cancleKey );

			// 진행도
			IProgress<float> prog = onProgress != null ? Progress.Create<float>( onProgress ) : null;

			// 리소스 로딩 
			ResourceRequest rr = Resources.LoadAsync<T>( path );

			if( default == cancelToken && null == prog )
				res = await rr;
			else
				res = await rr.ToUniTask( progress: prog, cancellationToken: cancelToken );

			return res as T;
		}
		catch( Exception ex )
		{
#if DEBUG
			Debug.Log( $"[Exception] Resources.LoadAsync = {ex.Message}" );
#endif//DEBUG
			return null;
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 여러개의 캔슬 토큰을 묶어서 하나로 만들어 준다.
	/// </summary>
	/// <param name="ctns"></param>
	/// <returns></returns>
	public CancellationTokenSource LinkCancellationToken( params CancellationToken[] ctns )
	{
		var linkedCtn = CancellationTokenSource.CreateLinkedTokenSource( ctns );

		return linkedCtn; 
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 캔슬 토큰을 추가한다.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="cts"></param>
	public void AddCancellationToken( string key, CancellationTokenSource cts )
	{
		// 키가 null 이거나, 토큰 소스가 없으면 pass
		if( true == string.IsNullOrEmpty( key ) || null == cts )
			return;

		// 이미 있었으면, 기존의 것과 병합한다.
		if( true == _cancelTokenDic.ContainsKey( key ) )
		{
			// 사용한 적이 있다면 그대로 제거
			if( true == _cancelTokenDic[ key ].IsCancellationRequested )
			{
				_cancelTokenDic[ key ].Dispose();
				_cancelTokenDic.Remove( key );
			}
			// 없다면 기존의 것과 병합
			else
			{
				cts = LinkCancellationToken( _cancelTokenDic[ key ].Token, cts.Token );
			}
		}

		// 딕셔너리에 추가
		_cancelTokenDic[ key ] = cts;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 캔슬 토큰을 얻어 온다.\n
	/// 캔슬 토큰은 비동기 작업 함수 내에서 리스너 역할을 한다.
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public CancellationToken GetCancellationToken( string key )
	{
		if( true == string.IsNullOrEmpty( key ) )
			return default;

		// 이미 있는 경우에
		if( true == _cancelTokenDic.ContainsKey( key ) )
		{
			// 사용한 적이 없다면 그대로 리턴
			if( false == _cancelTokenDic[key].IsCancellationRequested )
				return _cancelTokenDic[ key ].Token;

			// 사용한 적이 있으면 제거
			_cancelTokenDic[ key ].Dispose();
			_cancelTokenDic.Remove( key );
		}

		// 할당 한다.
		_cancelTokenDic[ key ] = new CancellationTokenSource();

		return _cancelTokenDic[ key ].Token;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 해당 비동기 작업을 캔슬 한다.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="isForce">예외를 즉시 전파해야 하는 경우 true이고, 그렇지 않으면 false입니다</param>
	public void CancelTask( string key, bool isForce = false )
	{
		if( false == _cancelTokenDic.ContainsKey( key ) )
		{
#if DEBUG
			Debug.Log( $"[UniTaskManager] {key} 는 등록되지 않은 캔슬 토큰 입니다." );
#endif//DEBUG
			return;
		}

		/// 중지 시킨 후에는 지워준다.
		_cancelTokenDic[ key ].Cancel( isForce );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	#region IBaseSingleton

	/// <summary>
	/// 초기화
	/// </summary>
	public void Initialize()
	{
		Release();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 릴리즈 함수 
	/// </summary>
	public void Release()
	{
		// 살아 있는 토큰은 모두 정리하고 클리어
		foreach( var ctn in _cancelTokenDic.Values )
		{
			ctn.Cancel();
			ctn.Dispose();
		}

		_cancelTokenDic.Clear();

	}

	#endregion//IBaseSingleton

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//UniTaskManager
