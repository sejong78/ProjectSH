/**---------------------------------------------------------------------------------
 * @file TransformExtensions.cs
 * @brief Transform 기반 확장 함수 모음입니다.
 *///-------------------------------------------------------------------------------
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// @class TransformExtensions
/// @brief Transform 기반 확장 함수 모음입니다. 
/// </summary>
public static class TransformExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// [World Space] 유니티 월드 좌표(position)를 직접 제어하는 비동기 연출입니다.
	/// </summary>
	public static async UniTask TransformWorld_Async( this Transform tf, Vector3 targetWorldPos, Quaternion targetRot, Vector3 targetScale, float duration, CancellationToken token )
	{
		if( null == tf ) 
			return;

		// 월드 좌표 기준 시작점 저장
		Vector3 startWorldPos	= tf.position;
		Quaternion startRot		= tf.localRotation;
		Vector3 startScale		= tf.localScale;

		float elapsed = 0f;

		while( elapsed < duration )
		{
			if( true == token.IsCancellationRequested )
				return;

			elapsed += GameTime.RealTimeDelta;
			
			float t = Mathf.Clamp01( elapsed / duration );

			float easeT = t * ( 2 - t ); // OutQuad

			// 월드 좌표(position)에 직접 대입
			tf.position			= Vector3.Lerp( startWorldPos, targetWorldPos, easeT );
			tf.localRotation	= Quaternion.Slerp( startRot, targetRot, easeT );
			tf.localScale		= Vector3.Lerp( startScale, targetScale, easeT );

			await UniTask.Yield( PlayerLoopTiming.Update, token );
		}

		// 최종 값 보정
		tf.position			= targetWorldPos;
		tf.localRotation	= targetRot;
		tf.localScale	= targetScale;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Transform 기반 범용 비동기 연출. 
	/// UI(RectTransform)라면 anchoredPosition을, 일반 객체라면 localPosition을 사용합니다.
	/// </summary>
	public static async UniTask Transform_Async( this Transform tf, Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, float duration, CancellationToken token )
	{
		if( null == tf ) 
			return;

		Vector3     startPos    = tf.localPosition;
		Quaternion  startRot    = tf.localRotation;
		Vector3     startScale  = tf.localScale;

		RectTransform rect = tf as RectTransform;
		// UI 요소인지 확인
		if( null != rect )
			startPos = rect.anchoredPosition;

		float elapsed = 0f;

		try
		{
	
			while( elapsed < duration )
			{
				if( true == token.IsCancellationRequested ) 
					return;
	
				elapsed += GameTime.RealTimeDelta;
	
				float t = Mathf.Clamp01(elapsed / duration);
				float easeT = t * ( 2 - t ); // OutQuad
	
				Vector3 currentPos = Vector3.Lerp( startPos, targetPos, easeT );
	
				// 좌표 적용 분기
				if( null != rect ) 
					rect.anchoredPosition = ( Vector2 )currentPos;
				else 
					tf.localPosition = currentPos;
	
				tf.localRotation	= Quaternion.Slerp( startRot, targetRot, easeT );
				tf.localScale		= Vector3.Lerp( startScale, targetScale, easeT );
	
				await UniTask.Yield( PlayerLoopTiming.Update, token );
	
			}
		}
		catch( System.Exception ex )
		{
			DebugExtensions.Log( $"[Error][Transform_Async] {ex.Message}" );
		}
		finally
		{
			// 최종 값 보정
			if( null != rect )
				rect.anchoredPosition = ( Vector2 )targetPos;
			else
				tf.localPosition = targetPos;

			tf.localRotation = targetRot;
			tf.localScale = targetScale;
		}

	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
}//TransformExtensions
