using System;
using System.Collections;
using UnityEngine;

public static class MonoBehaviorExtensions
{
	public delegate void OnCoroutineException( Exception e );

	public static Coroutine StartCoroutine( this MonoBehaviour obj, IEnumerator coroutine, OnCoroutineException onCoroutineException )
	{
		return obj.StartCoroutine( InnerCoroutine( coroutine, onCoroutineException ) );
	}
	private static IEnumerator InnerCoroutine( IEnumerator coroutine, OnCoroutineException onCoroutineException )
	{
		while( true )
		{
			try
			{
				if( false == coroutine.MoveNext() )
				{
					break;
				}
			}
			catch( System.Exception e )
			{
				if( null != onCoroutineException )
				{
					onCoroutineException( e );
				}
			}

			yield return coroutine.Current;
		}
	}
	public static Coroutine WaitForRealSeconds( this MonoBehaviour obj, float seconds )
	{
		return obj.StartCoroutine( WaitForRealSecondsImpl( seconds ) );
	}
	private static IEnumerator WaitForRealSecondsImpl( float seconds )
	{
		float finishTime = Time.realtimeSinceStartup + seconds;

		while( Time.realtimeSinceStartup < finishTime )
		{
			yield return null;
		}
	}

}//MonoBehaviorExtensions
