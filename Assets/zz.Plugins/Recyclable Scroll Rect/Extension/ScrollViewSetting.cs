/**---------------------------------------------------------------------------------
 * @file ScrollViewSetting.cs
 * @date 2023/7/20
 * @author sejong
 * @brief 리사이클 스크롤 뷰 사용을 돕기 위한 클레스
 *///-------------------------------------------------------------------------------
using PolyAndCode.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// @class ScrollViewSetting
/// @date 2023/7/20
/// @author sejong
/// @brief 리사이클 스크롤 뷰 사용을 돕기 위한 클레스
/// </summary>
public class ScrollViewSetting<T> : IRecyclableScrollRectDataSource
{
	protected List<T> _dataList = new();

	protected Action<List<T>,ICell,Int32>  OnSetCell = null;

	/// <summary>
	/// 외부 인터페이스로 초기화시 사용한다.
	/// </summary>
	/// <param name="onSetData"></param>
	/// <param name="onSetCell"></param>
	public void Init( Action<List<T>> onSetData, Action<List<T>,ICell,Int32> onSetCell )
	{
		OnSetCell = onSetCell;

		RemoveAll();

		onSetData.SafeExcute( _dataList );
	}

	/// <summary>
	/// 데이터를 업데이트 한다.
	/// </summary>
	/// <param name="onSetData"></param>
	public void UpdateData( Action<List<T>> onSetData )
	{
		if( null == _dataList )
			return;

		onSetData.SafeExcute( _dataList );
	}

	/// <summary>
	/// 파기시 사용한다.
	/// </summary>
	public void Release()
	{
		RemoveAll();
		OnSetCell = null;
	}

	public void RemoveAll()
	{
		_dataList.SafeClear();
	}

	public void Remove( T data )
	{
		if( null == _dataList )
			return;

		_dataList.Remove( data );
	}

	#region IRecyclableScrollRectDataSource
	public Int32 GetItemCount()
	{
		return _dataList.SafeCount();
	}

	public void SetCell( ICell cell, Int32 index )
	{
		OnSetCell.SafeExcute( _dataList, cell, index );
	}

	#endregion//IRecyclableScrollRectDataSource
}//ScrollViewSetting
