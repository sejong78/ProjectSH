/**---------------------------------------------------------------------------------
 * @file CollectionExtensions.cs
 * @date 2019/10/24
 * @author sejong
 * @brief 게임 내 Collection 관련 처리를 도울 확장함수 관리 클래스
 *///-------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// @class CollectionExtensions
/// @date 2019/10/24
/// @author sejong
/// @brief 1. List, Dictionary 등의 확장함수를 관리합니다.
/// </summary>
public static class CollectionExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 리스트의 카운트를 널 체크 후 가져온다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetList"></param>
    /// <returns></returns>
    public static int SafeCount<T>( this List<T> targetList )
    {
        if( targetList == null )
        {
            return 0;
        }

        return targetList.Count;
    }

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 어레이의 카운트를 널 체크 후 가져온다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="targetList"></param>
	/// <returns></returns>
	public static int SafeCount<T>( this T[] targetList )
	{
		if( targetList == null )
		{
			return 0;
		}

		return targetList.Length;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 딕셔너리의 카운트를 널 체크 후 가져온다.
	/// </summary>
	/// <typeparam name="K"></typeparam>
	/// <typeparam name="V"></typeparam>
	/// <param name="targetDictionary"></param>
	/// <returns></returns>
	public static int SafeCount<K, V>( this Dictionary<K, V> targetDictionary )
    {
        if( targetDictionary == null )
        {
            return 0;
        }

        return targetDictionary.Count;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 해시 셋의 카운트를 널 체크 후 가져온다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetSet"></param>
    /// <returns></returns>
    public static int SafeCount<T>( this HashSet<T> targetSet )
    {
        if( targetSet == null )
        {
            return 0;
        }

        return targetSet.Count;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 스택의 카운트를 널 체크 후 가져온다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetStack"></param>
    /// <returns></returns>
    public static int SafeCount<T>( this Stack<T> targetStack )
    {
        if( targetStack == null )
        {
            return 0;
        }

        return targetStack.Count;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 스택의 최상단 데이터를 널 체크 후 가져온다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetStack"></param>
    /// <returns></returns>
    public static T SafePeek<T>( this Stack<T> targetStack )
    {
        if( targetStack == null )
        {
            return default;
        }

        return targetStack.Peek();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 스택 클리어 시 널 체크 후 수행한다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetStack"></param>
    public static void SafeClear<T>( this Stack<T> targetStack )
    {
        if( targetStack == null || targetStack.Count == 0 )
        {
            return;
        }

        targetStack.Clear();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 큐의 카운트를 널 체크 후 가져온다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetQueue"></param>
    /// <returns></returns>
    public static int SafeCount<T>( this Queue<T> targetQueue )
    {
        if( targetQueue == null )
        {
            return 0;
        }

        return targetQueue.Count;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 큐의 최상단 데이터를 널 체크 후 가져온다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetQueue"></param>
    /// <returns></returns>
    public static T SafePeek<T>( this Queue<T> targetQueue )
    {
        if( targetQueue == null )
        {
            return default;
        }

        return targetQueue.Peek();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 큐 클리어 시 널 체크 후 수행한다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetQueue"></param>
    public static void SafeClear<T>( this Queue<T> targetQueue )
    {
        if( targetQueue == null || targetQueue.Count == 0 )
        {
            return;
        }

        targetQueue.Clear();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 리스트 Add시의 널처리를 한다.
    /// </summary>
    /// <typeparam name="T">리스트 데이터의 타입</typeparam>
    /// <param name="targetList">타겟 리스트</param>
    /// <param name="data">추가할 데이터</param>
    /// <param name="condition">조건 처리 Func, 조건이 충족되어야 리스트에 추가된다..</param>
    public static void SafeAdd<T>( this List<T> targetList, T data, Func<T,bool> condition = null )
	{
		if( targetList == null || data == null )
		{
			return;
		}

		// 조건이 있는경우 조건을 충족하지 않으면, 추가하지 않는다.
		if( condition != null )
		{
			if( false == condition( data ) )
			{
				return;
			}
		}

		targetList.Add( data );
	}

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 딕셔너리 Add시의 널처리를 한다.
    /// </summary>
    /// <typeparam name="K">딕셔너리의 키 타입</typeparam>
    /// <typeparam name="V">딕셔너리의 벨류 타입</typeparam>
    /// <param name="targetDictionary">타겟 딕셔너리</param>
    /// <param name="data">추가할 데이터</param>
    public static void SafeAdd<K,V>( this Dictionary<K,V> targetDictionary, K key, V data )
	{
		if( targetDictionary == null || data == null )
		{
			return;
		}

		// 이미 값이 있으면 pass
		if( targetDictionary.ContainsKey( key ) == true )
		{
			return;
		}

		targetDictionary.Add( key, data );
	}

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 해시 셋 Add 시의 널처리를 한다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetSet"></param>
    /// <param name="data"></param>
    public static void SafeAdd<T>( this HashSet<T> targetSet, T data )
    {
        if( targetSet == null || data == null )
        {
            return;
        }

        targetSet.Add( data );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 리스트 클리어시의 널처리를 한다.
    /// </summary>
    /// <typeparam name="T">리스트 데이터의 타입</typeparam>
    /// <param name="targetList">타겟 리스트</param>
    public static void SafeClear<T>( this List<T> targetList )
	{
		if( targetList == null || targetList.Count == 0 )
		{
			return;
		}

		targetList.Clear();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 딕셔너리 클리어시의 널처리
	/// </summary>
	/// <typeparam name="K">딕셔너리의 키 타입</typeparam>
	/// <typeparam name="V">딕셔너리의 벨류 타입</typeparam>
	/// <param name="targetDictionary"></param>
	public static void SafeClear<K,V>( this Dictionary<K,V> targetDictionary )
	{
		if( targetDictionary == null || targetDictionary.Count == 0 )
		{
			return;
		}

		targetDictionary.Clear();
	}

    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 해시 셋 클리어 시의 널처리를 한다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetSet"></param>
    public static void SafeClear<T>( this HashSet<T> targetSet )
    {
        if( targetSet == null || targetSet.Count == 0 )
        {
            return;
        }

        targetSet.Clear();
    }

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 리스트 데이터를 하나씩 가져온다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="targetList"></param>
	/// <param name="output"></param>
	public static void GetOneByOne<T>( this List<T> targetList, Action<T> output )
	{
		if( targetList == null || output == null )
		{
			return;
		}

		for( int i = 0; i < targetList.Count; ++i )
		{
			output( targetList[ i ] );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// Array 데이터를 하나씩 가져온다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="targetArray"></param>
	/// <param name="output"></param>
	public static void GetOneByOne<T>( this T[] targetArray, Action<T> output )
	{
		if( targetArray == null || output == null )
		{
			return;
		}

		for( int i = 0; i < targetArray.Length; ++i )
		{
			output( targetArray[ i ] );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 딕셔너리 Key 데이터를 하나씩 가져온다.
	/// </summary>
	/// <typeparam name="K"></typeparam>
	/// <typeparam name="V"></typeparam>
	/// <param name="targetDictionary"></param>
	/// <param name="output"></param>
	public static void GetKeyOneByOne<K, V>( this Dictionary<K,V> targetDictionary, Action<K> output )
	{
		if( targetDictionary == null || output == null )
		{
			return;
		}

		foreach( K key in targetDictionary.Keys )
		{
			output( key );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 딕셔너리 Value 데이터를 하나씩 가져온다.
	/// </summary>
	/// <typeparam name="K"></typeparam>
	/// <typeparam name="V"></typeparam>
	/// <param name="targetDictionary"></param>
	/// <param name="output"></param>
	public static void GetValueOneByOne<K, V>( this Dictionary<K, V> targetDictionary, Action<V> output )
	{
		if( targetDictionary == null || output == null )
		{
			return;
		}

		foreach( V val in targetDictionary.Values )
		{
			output( val );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 리스트의 인자를 적절히 섞어준다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="targetList"></param>
	/// <param name="count">셔플횟수</param>
	public static void Shuffle<T>( this IList<T> targetList, int count = -1 )
	{
		if( targetList == null || targetList.Count < 1 )
		{
			return;
		}

		int rand1	= 0;
		int rand2	= 0;
		T	tmp		= default(T);

		if( count < 1 )
			count = targetList.Count;

		for( int i = 0; i < count; ++i )
		{
			rand1 = UnityEngine.Random.Range( 0, count );
			rand2 = UnityEngine.Random.Range( 0, count );

			tmp = targetList[ rand1 ];
			targetList[ rand1 ] = targetList[ rand2 ];
			targetList[ rand2 ] = tmp;
		}

		tmp = default( T );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 리스트 인자중 하나를 랜덤으로 가져온다.
	/// </summary>
	/// <typeparam name="K"></typeparam>
	/// <typeparam name="V"></typeparam>
	/// <param name="targetDictionary"></param>
	/// <param name="output"></param>
	public static void GetRandomOne<T>( this List<T> targetList, Action<T> output )
	{
		int count = targetList.SafeCount();

		if( count < 1 || null == output )
		{
			return;
		}

		output.SafeExcute( targetList.GetRandomOne() );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 리스트 인자중 하나를 랜덤으로 가져온다.
	/// </summary>
	/// <param name="targetDictionary"></param>
	public static T GetRandomOne<T>( this List<T> targetList )
	{
		int count = targetList.SafeCount();

		if( count < 1 )
		{
			return default(T);
		}

		int idx = UnityEngine.Random.Range( 0, count );

		return targetList[ idx ];
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// IEnumerable 인자중 하나를 랜덤으로 가져온다.
	/// </summary>
	/// <typeparam name="K"></typeparam>
	/// <typeparam name="V"></typeparam>
	/// <param name="targetDictionary"></param>
	/// <param name="output"></param>
	public static void GetRandomOne<T>( this IEnumerable<T> target, Action<T> output )
	{
		int count = target.Count();

		if( count < 1 || null == output )
		{
			return;
		}

		int idx = UnityEngine.Random.Range( 0, count );

		var enumerator = target.GetEnumerator();

		for( int i = 0; i < count; ++i )
		{
			enumerator.MoveNext();

			if( i < idx )
			{
				continue;
			}

			output.SafeExcute( enumerator.Current );
			break;
		}

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// IEnumerable 인자중 하나를 랜덤으로 가져온다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="target"></param>
	/// <returns></returns>
	public static T GetRandomOne<T>( this IEnumerable<T> target )
	{
		int count = target.Count();

		if( count < 1 )
		{
			return default(T);
		}

		int idx = UnityEngine.Random.Range( 0, count );

		var enumerator = target.GetEnumerator();

		for( int i = 0; i < count; ++i )
		{
			enumerator.MoveNext();

			if( i < idx )
			{
				continue;
			}

			return enumerator.Current;
		}

		return default( T );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// IEnumerable 인자중 하나를 키로 딕셔너리를 만들어 반환한다.
	/// </summary>
	/// <typeparam name="K"></typeparam>
	/// <typeparam name="T"></typeparam>
	/// <param name="source"></param>
	/// <param name="keySelector"></param>
	/// <returns></returns>
	public static Dictionary<T, K> ConvertDictionary<K, T>( this IEnumerable<K> source, Func<K, T> keySelector )
	{
		if( null == source )
			return null;

		Dictionary<T, K> dic = new Dictionary<T, K>();

		foreach( K val in source )
		{
			T key = keySelector( val );

			if( true == dic.ContainsKey( key ) )
				continue;

			dic.Add( key, val );
		}

		return dic;
	}


	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//class CollectionExtensions
