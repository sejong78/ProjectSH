/**---------------------------------------------------------------------------------
 * @file WeightedRandomPicker.cs
 * @date 2025/6/8
 * @author sejong
 * @brief 가중치가 적용된 랜덤 선택기
 *///-------------------------------------------------------------------------------
using System.Collections.Generic;

/// <summary>
/// @class WeightedRandomPicker
/// @date 2025/6/8
/// @author sejong
/// @brief 가중치가 적용된 랜덤 선택기
/// </summary>
public class WeightedRandomPicker<T>
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	public class Item
	{
		public T Value;
		public int Weight;
	}

	protected List<Item> _items = new List<Item>();
	protected int _totalWeight = 0;
	protected bool _dirty = false;

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	public void Add( T value, int weight )
	{
		_items.Add( new Item { Value = value, Weight = weight } );

		_dirty = true;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	public void Clear()
	{
		_items.Clear();

		_totalWeight = 0;

		_dirty = false;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	protected void RecalculateTotalWeight()
	{
		_totalWeight = 0;

		foreach( var item in _items )
			_totalWeight += item.Weight;

		_dirty = false;
	}


	//@@-------------------------------------------------------------------------------------------------------------------------

	public T Pick()
	{
		if( _items.SafeCount() < 1 )
			return default( T );

		if( true == _dirty )
			RecalculateTotalWeight();

		int rand = UnityEngine.Random.Range( 0, _totalWeight );
		int cumulative = 0;

		_items.Shuffle();

		foreach( var item in _items )
		{
			cumulative += item.Weight;

			if( rand < cumulative )
				return item.Value;
		}

		return default( T );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	public override string ToString()
	{
#if UNITY_EDITOR
		return string.Join( ", ", _items.ConvertAll( item => $"{item.Weight} : {item.Value?.ToString() ?? "null"}" ) );
#else//UNITY_EDITOR
		return "";
#endif//UNITY_EDITOR
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//WeightedRandomPicker
