#pragma warning disable CS0414

/**---------------------------------------------------------------------------------
 * @file UniTaskHow2Use.cs
 * @date 2022/7/13
 * @author sejong
 * @brief UniTask 사용법
 *///-------------------------------------------------------------------------------
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// @class UniTaskHow2Use
/// @date 2022/7/14
/// @author sejong
/// @brief UniTask 사용법
/// </summary>
public class UniTaskHow2Use : MonoBehaviour
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	[SerializeField]
	private Transform _trObj = null;

	[Range(0, 1)]
	[SerializeField]
	private float _value = 0;

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------


	private CancellationToken? _cancellationToken = null;

	private CancellationToken CTS
	{
		get 
		{ 
			if( _cancellationToken == null )
			{
				_cancellationToken = new CancellationToken();
				_cancellationToken = this.GetCancellationTokenOnDestroy();
			}

			return _cancellationToken.Value;
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	private void OnProgress_WebRequest( float prog )
	{
		Debug.Log( $"[WebRequest] Prog : {prog:p0}" );
	}

	private void OnProgress_LoadScene( float prog )
	{
		Debug.Log( $"[LoadScene] Prog : {prog:p0}" );
	}

	private void OnProgress_UnLoadScene( float prog )
	{
		Debug.Log( $"[UnLoadScene] Prog : {prog:p0}" );
	}

	private void OnProgress_LoadResource( float prog )
	{
		Debug.Log( $"[LoadResource] Prog : {prog:p0}" );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// MonoBehaviour 의 Start 를 사용할 수 있다.
	/// </summary>
	/// <returns></returns>
	protected async UniTaskVoid Start()
	{
		var testTkn = UniTaskManager.INSTANCE.GetCancellationToken( "Test" );

		// 일반 함수로 비동기로 처리가 가능하다. 단 이경우 .Forget(); 을 추가해야 워닝이 없다.
		UniTaskManager.INSTANCE.Run( NormalSyncFunction, "Test" ).Forget();

		Debug.Log( $"<color=#ff0000>WhenAll Start!!!</color>" );
		await UniTaskManager.INSTANCE.WhenAll( "",
			UniTaskManager.INSTANCE.Run( NormalSyncFunction, "Test" ), // 일반 함수로 비동기로 처리가 가능하다.
			UniTaskManager.INSTANCE.Delay( 20000 ) );
		Debug.Log( $"<color=#ff0000>WhenAll Done!!!</color>" );

		Debug.Log( $"<color=#ff0000>WhenAny Start!!!</color>" );
		var rslt = await UniTaskManager.INSTANCE.WhenAny( "",
					WaitSeconds( 3f ),
					WaitSeconds( 2f ) );
		Debug.Log( $"<color=#ff0000>WhenAny Done!!! 먼저 끝난 {rslt.index} 번 비동기함수 의 결과는 {rslt.result} </color>" );

		AsynFunction( testTkn ).Forget();
		await UniTaskManager.INSTANCE.Delay( 1000 );
		UniTaskManager.INSTANCE.CancelTask( "Test" );

		var txt = await UniTaskManager.INSTANCE.WebRequest_Get( "https://www.google.co.jp/", onProgress: OnProgress_WebRequest );
		Debug.Log( txt );

		await UniTaskManager.INSTANCE.LoadScene( "SampleAddScene", UnityEngine.SceneManagement.LoadSceneMode.Additive, onProgress: OnProgress_LoadScene );

		await UniTaskManager.INSTANCE.Delay( 1000 );

		await UniTaskManager.INSTANCE.UnLoadScene( "SampleAddScene", UnityEngine.SceneManagement.UnloadSceneOptions.UnloadAllEmbeddedSceneObjects, onProgress: OnProgress_UnLoadScene );

		var go = await UniTaskManager.INSTANCE.LoadResource<GameObject>( "Cube", onProgress: OnProgress_LoadResource );

		/*
			<color=#0000ff>Check Start!!</color>
			<color=#ff0000>[WrappingAsync] Start!!</color>
			[NormalFunction] 0 to 9 : 0
			<color=#0000ff>Check End!!</color>
			[NormalFunction] 0 to 9 : 1
			[NormalFunction] 0 to 9 : 2
			[NormalFunction] 0 to 9 : 3
			[NormalFunction] 0 to 9 : 4
			[NormalFunction] 0 to 9 : 5
			[NormalFunction] 0 to 9 : 6
			[NormalFunction] 0 to 9 : 7
			[NormalFunction] 0 to 9 : 8
			[NormalFunction] 0 to 9 : 9
			<color=#ff0000>[WrappingAsync] End!!</color>

			- await 처리를 하지 않은 WrappingAsync 는 비동기로 처리 되지만, 함수 내부는 순서가 보장 된다.
		*/
		Debug.Log( $"<color=#0000ff>Check Start!!</color>" );
		WrappingAsync().Forget();
		Debug.Log( $"<color=#0000ff>Check End!!</color>" );

		var clone = GameObject.Instantiate( go ) as GameObject;
		clone.transform.SetParent( _trObj );
		clone.transform.localPosition	= Vector3.zero;
		clone.transform.localRotation	= Quaternion.identity;
		clone.transform.localScale		= Vector3.one;

		await UniTask.Yield();

		await MoveObject( Vector3.zero, Vector3.one * 1000, 10000 );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 오브젝트 이동
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="time"></param>
	/// <returns></returns>
	private async UniTask<Vector3> MoveObject( Vector3 from, Vector3 to, float time )
	{
		if( from == to || time <= 0 )
			return to;

		float t = 0;
		
		while( t < time )
		{
			_trObj.localPosition = Vector3.Lerp( from, to, t / time );

			await UniTask.Yield( CTS );

			t += Time.deltaTime;
		}

		_trObj.localPosition = to;

		return to;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 비동기 테스트용
	/// </summary>
	/// <returns></returns>
	private async UniTaskVoid WrappingAsync()
	{
		Debug.Log( $"<color=#ff0000>[WrappingAsync] Start!!</color>" );

		await NormalAsyncFunction();

		Debug.Log( $"<color=#ff0000>[WrappingAsync] End!!</color>" );

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 일반 비동기 처리가 가능한 함수
	/// </summary>
	private void NormalSyncFunction()
	{
		Debug.Log( $"<color=#ff0000>일반 함수의 비동기 시작!!</color>" );
		AsynFunction( default).Forget();
		Debug.Log( $"<color=#ff0000>일반 함수의 비동기 종료!!</color>" );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 일반 비동기 처리가 가능한 함수
	/// </summary>
	private async UniTask NormalAsyncFunction()
	{
		for( int i = 0; i < 10; ++i )
		{
			Debug.Log( $"[NormalFunction] 0 to 9 : {i}" );

			await UniTask.Delay( 1000 );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// UniTaskVoid 리턴형 함수는 외부에서 await 을 통한 대기가 불가능하다.
	/// </summary>
	/// <param name="ctn"></param>
	/// <returns></returns>
	private async UniTaskVoid AsynFunction( CancellationToken ctn )
	{
		for( int i = 0; i < 10; ++i )
		{
			Debug.Log( $"0 to 9 : {i}" );

			await UniTask.Delay( 1000, cancellationToken: ctn );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 리턴이 있는 경우 처리
	/// </summary>
	/// <param name="waitSeconds"></param>
	/// <returns></returns>
	private async UniTask<float> WaitSeconds( float waitSeconds )
	{
		Debug.Log( $"Start Waiting....." );

		await UniTask.Delay( TimeSpan.FromSeconds( waitSeconds ), cancellationToken: CTS );

		Debug.Log( $"Wait {waitSeconds} Seconds Done!!" );

		return waitSeconds;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	private void Awake()
	{
		DontDestroyOnLoad( gameObject );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	private void OnDestroy()
	{
		UniTaskManager.Reset();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//UniTaskHow2Use

#pragma warning restore CS0414
