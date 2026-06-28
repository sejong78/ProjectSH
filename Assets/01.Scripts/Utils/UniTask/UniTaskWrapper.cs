/**---------------------------------------------------------------------------------
 * @file UniTaskWrapper.cs
 * @date 2025/6/14
 * @author sejong
 * @brief async/await 구조의 UniTask 를 코루틴 형식처럼 회부 handler 를 사용해서 종료갛 수 있는 형태로 관리하기 위한 래퍼 클래스
 * 다만 이 경우 OperationCanceledException이 발생하게 되므로, try/catch 구문을 사용하여 예외를 처리해야 한다.
 *///-------------------------------------------------------------------------------
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

/// <summary>
/// @class UniTaskWrapper
/// @date 2025/6/14
/// @author sejong
/// @brief async/await 구조의 UniTask 를 코루틴 형식처럼 회부 handler 를 사용해서 종료갛 수 있는 형태로 관리하기 위한 래퍼 클래스
/// </summary>
public static class UniTaskWrapper
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	public static void SafeCancel( this UniTaskHandler hdr )
	{
		if( null != hdr && !hdr.IsCancellationRequested )
		{
			hdr.Cancel();
			hdr.Dispose();
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 비동기 작업을 시작하고, 취소 토큰 소스를 반환합니다.
	/// </summary>
	public static UniTaskHandler StartTask( Func<CancellationToken, UniTaskVoid> asyncAction, UniTaskHandler hdr = null )
	{
		if( null == hdr )
			hdr = new UniTaskHandler();

		asyncAction( hdr.Token ).Forget(); // fire & forget

		return hdr;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 1개의 매개변수를 가진 비동기 작업을 시작하고, 취소 토큰 소스를 반환합니다.
	/// </summary>
	public static UniTaskHandler StartTask<T1>( Func<CancellationToken, T1, UniTaskVoid> asyncAction, T1 val, UniTaskHandler hdr = null )
	{
		if( null == hdr )
			hdr = new UniTaskHandler();

		asyncAction( hdr.Token, val ).Forget(); // fire & forget

		return hdr;
	}

	/// <summary>
	/// 1개의 매개변수를 가진 비동기 작업을 시작하고, 취소 토큰 소스를 반환합니다.
	/// </summary>
	public static UniTaskHandler StartTask<T1, T2>( Func<CancellationToken, T1, T2, UniTaskVoid> asyncAction, T1 val1, T2 val2, UniTaskHandler hdr = null )
	{
		if( null == hdr )
			hdr = new UniTaskHandler();

		asyncAction( hdr.Token, val1, val2 ).Forget(); // fire & forget

		return hdr;
	}

	/// <summary>
	/// 1개의 매개변수를 가진 비동기 작업을 시작하고, 취소 토큰 소스를 반환합니다.
	/// </summary>
	public static UniTaskHandler StartTask<T1, T2, T3>( Func<CancellationToken, T1, T2, T3, UniTaskVoid> asyncAction, T1 val1, T2 val2, T3 val3, UniTaskHandler hdr = null )
	{
		if( null == hdr )
			hdr = new UniTaskHandler();

		asyncAction( hdr.Token, val1, val2, val3 ).Forget(); // fire & forget

		return hdr;
	}

	/// <summary>
	/// 1개의 매개변수를 가진 비동기 작업을 시작하고, 취소 토큰 소스를 반환합니다.
	/// </summary>
	public static UniTaskHandler StartTask<T1, T2, T3, T4>( Func<CancellationToken, T1, T2, T3, T4, UniTaskVoid> asyncAction, T1 val1, T2 val2, T3 val3, T4 val4, UniTaskHandler hdr = null )
	{
		if( null == hdr )
			hdr = new UniTaskHandler();

		asyncAction( hdr.Token, val1, val2, val3, val4 ).Forget(); // fire & forget

		return hdr;
	}

	/// <summary>
	/// 1개의 매개변수를 가진 비동기 작업을 시작하고, 취소 토큰 소스를 반환합니다.
	/// </summary>
	public static UniTaskHandler StartTask<T1, T2, T3, T4, T5>( Func<CancellationToken, T1, T2, T3, T4, T5, UniTaskVoid> asyncAction, T1 val1, T2 val2, T3 val3, T4 val4, T5 val5, UniTaskHandler hdr = null )
	{
		if( null == hdr )
			hdr = new UniTaskHandler();

		asyncAction( hdr.Token, val1, val2, val3, val4, val5 ).Forget(); // fire & forget

		return hdr;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//UniTaskWrapper

/// <summary>
/// @class UniTaskHandler
/// @date 2025/6/14
/// @author sejong
/// @brief CancellationTokenSource 이름이 너무 길고 복잡하여 UniTaskHandler로 간단하게 줄인 래퍼 클래스
/// 주로 종료를 담당한다.
/// </summary>
public class UniTaskHandler : CancellationTokenSource
{ 
}
