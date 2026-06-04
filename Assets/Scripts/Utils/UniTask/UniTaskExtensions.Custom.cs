#pragma warning disable CS0168
using UnityEngine;

namespace Cysharp.Threading.Tasks
{

	public static partial class UniTaskExtensions
	{
		//@@-------------------------------------------------------------------------------------------------------------------------
		//@@-------------------------------------------------------------------------------------------------------------------------

		public static async UniTask RotateAsync( this Transform tr, Quaternion from, Quaternion to, float time )
		{
			tr.rotation = from;

			float startTime = Time.realtimeSinceStartup;
			float elapsTime = 0f;

			while( elapsTime < time )
			{
				tr.rotation = Quaternion.Lerp( from, to, elapsTime / time );

				await UniTask.Yield();

				elapsTime = Time.realtimeSinceStartup - startTime;
			}

			tr.rotation = to;
		}

		//@@-------------------------------------------------------------------------------------------------------------------------

		public static async UniTask LocalRotateAsync( this Transform tr, Quaternion from, Quaternion to, float time )
		{
			tr.localRotation = from;

			float startTime = Time.realtimeSinceStartup;
			float elapsTime = 0f;

			while( elapsTime < time )
			{
				tr.localRotation = Quaternion.Lerp( from, to, elapsTime / time );

				await UniTask.Yield();

				elapsTime = Time.realtimeSinceStartup - startTime;
			}

			tr.localRotation = to;
		}

		//@@-------------------------------------------------------------------------------------------------------------------------

		public static async UniTask MoveAsync( this Transform tr, Vector3 from, Vector3 to, float time )
		{
			tr.position = from;

			float startTime = Time.realtimeSinceStartup;
			float elapsTime = 0f;

			while( elapsTime < time )
			{
				tr.position = Vector3.Lerp( from, to, elapsTime / time );

				await UniTask.Yield();

				elapsTime = Time.realtimeSinceStartup - startTime;
			}

			tr.position = to;
		}

		//@@-------------------------------------------------------------------------------------------------------------------------

		public static async UniTask LocalMoveAsync( this Transform tr, Vector3 from, Vector3 to, float time )
		{
			tr.localPosition = from;

			float startTime = Time.realtimeSinceStartup;
			float elapsTime = 0f;

			while( elapsTime < time )
			{
				tr.localPosition = Vector3.Lerp( from, to, elapsTime / time );

				await UniTask.Yield();

				elapsTime = Time.realtimeSinceStartup - startTime;
			}

			tr.localPosition = to;
		}

		//@@-------------------------------------------------------------------------------------------------------------------------

		public static async UniTaskVoid PlayAsync( this Animator ani, string clipName, System.Action callBack, float crossfadeDuration = 0f, float delayTime = 0f )
		{
			if( null == ani || true == clipName.IsNullorEmpty() )
				return;

			if( 0 < delayTime )
				await UniTask.Delay( ( int )( delayTime * 1000f ) );

			ani.CrossFade( clipName, crossfadeDuration, -1, 0f );

			float lastNt = -1f;

			await UniTaskManager.INSTANCE.WaitUntil( () =>
			{
				float nt = ani.GetCurrentAnimatorStateInfo( 0 ).normalizedTime;

				// 현재값이 전 값보다 작으면 종료되었다 판단한다.
				if( nt <= lastNt )
					return true;

				lastNt = nt;

				return 0.99f <= nt;
			} );

			callBack.SafeExcute();
		}

		//@@-------------------------------------------------------------------------------------------------------------------------


		public static async UniTaskVoid PlayAnimationAsync( System.Threading.CancellationToken tkn, Animator ani, string clipName, System.Action callBack, float crossfadeDuration = 0f, float delayTime = 0f )
		{
			if( null == ani || true == clipName.IsNullorEmpty() )
				return;

			try
			{
				if( 0 < delayTime )
					await UniTask.Delay( ( int )( delayTime * 1000f ), cancellationToken: tkn );
	
				ani.CrossFade( clipName, crossfadeDuration, -1, 0f );
	
				float lastNt = -1f;
	
				await UniTask.WaitUntil( () =>
				{
					float nt = ani.GetCurrentAnimatorStateInfo( 0 ).normalizedTime;
	
					// 현재값이 전 값보다 작으면 종료되었다 판단한다.
					if( nt <= lastNt )
						return true;
	
					lastNt = nt;
	
					return 0.99f <= nt;
				}, cancellationToken: tkn );
	
				callBack.SafeExcute();
			}
			catch( System.Exception ex )
			{
#if DEBUG
//				DebugExtensions.LogError( $"[PlayAnimationAsync] {ex.Message}" );
#endif//DEBUG
			}
		}

		//@@-------------------------------------------------------------------------------------------------------------------------

		public static async UniTask PlayAsync( this Animator ani, string clipName, float crossfadeDuration = 0f )
		{
			if( null == ani || true == clipName.IsNullorEmpty() )
				return;

			ani.CrossFade( clipName, crossfadeDuration, -1, 0f );

			float lastNt = -1f;

			await UniTaskManager.INSTANCE.WaitUntil( () =>
			{
				float nt = ani.GetCurrentAnimatorStateInfo( 0 ).normalizedTime;

				// 현재값이 전 값보다 작으면 종료되었다 판단한다.
				if( nt <= lastNt )
					return true;

				lastNt = nt;

				return 0.99f <= nt;
			} );

		}


		//@@-------------------------------------------------------------------------------------------------------------------------
		//@@-------------------------------------------------------------------------------------------------------------------------

	}//UniTaskExtension

}// Cysharp.Threading.Tasks

#pragma warning restore CS0168