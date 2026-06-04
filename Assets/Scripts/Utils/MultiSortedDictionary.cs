using System.Collections.Generic;
using System.Linq;

/// <summary>
/// C#용 MultiMap
/// </summary>
/// <typeparam name="Key"></typeparam>
/// <typeparam name="Value"></typeparam>
public class MultiSortedDictionary<Key, Value>
{
    private SortedDictionary<Key, List<Value>> _dic = null;

    public MultiSortedDictionary()
    {
        _dic = new SortedDictionary<Key, List<Value>>();
    }

    public MultiSortedDictionary(IComparer<Key> comparer)
    {
        _dic = new SortedDictionary<Key, List<Value>>(comparer);
    }

    public void Add(Key key, Value value)
    {
        List<Value> list = null;

        if (_dic.TryGetValue(key, out list))
        {
            list.Add(value);
        }
        else
        {
            list = new List<Value>();
            list.Add(value);
            _dic.Add(key, list);
        }
    }

    /// <summary>
    /// Key에 속한 모든 Value를 제거
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(Key key)
    {
        return _dic.Remove(key);
    }

    public void Clear()
    {
        _dic.Clear();
    }

    /// <summary>
    /// 특정 Value를 제거
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Remove(Key key, Value value)
    {
        List<Value> list = this[key];
        if (list != null)
        {
			if( list.Contains( value ) == false )
				return false;

			list.Remove(value);

			if( list.Count < 1 )
			{
				Remove( key );
			}

			return true;
        }

        return false;
    }

    public bool ContainsKey(Key key)
    {
        return _dic.ContainsKey(key);
    }

    /// <summary>
    /// 특정 Key에 특정 Value가 속해있는지 체크
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool ContainsValue(Key key, Value value)
    {
        List<Value> list = this[key];
        if (list != null && list.Count > 0)
        {
            return list.FirstOrDefault(s => s.Equals(value)) != null ? true : false;
        }

        return false;
    }


	/// <summary>
	/// 해당 키값에 준하는 Value List 값을 가져옵니다.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="outList"></param>
	/// <returns></returns>
	public bool TryGetValue( Key key, out List<Value> outList )
	{
		outList = null;

		return _dic.TryGetValue( key, out outList );
	}


	public List<Value> this[Key key]
    {
        get
        {
            List<Value> list = null;
            if (_dic.TryGetValue(key, out list) == false)
            {
                list = new List<Value>();
                _dic.Add(key, list);
            }

            return list;
        }
    }

    public IEnumerable<Key> keys
    {
        get
        {
            return _dic.Keys;
        }
    }

    public int Count
    {
        get
        {
            if (_dic != null)
            {
                return _dic.Count;
            }

            return 0;
        }
    }
}
