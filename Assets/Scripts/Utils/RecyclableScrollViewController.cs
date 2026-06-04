/**---------------------------------------------------------------------------------
 * @file RecyclableScrollViewController.cs
 * @date 2025/5/10
 * @author sejong
 * @brief 스와이프 제스처를 인식하여 스크롤뷰를 부드럽게 이동시키는 컨트롤러
 *///-------------------------------------------------------------------------------
using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

/// <summary>
/// @class RecyclableScrollViewController
/// @date 2025/5/10
/// @author sejong
/// @brief 스와이프 제스처를 인식하여 스크롤뷰를 부드럽게 이동시키는 컨트롤러
/// </summary>
public class RecyclableScrollViewController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	[SerializeField]
	protected RecyclableScrollRect _recyclableScrollRect = null;

	[Header("스와이프를 인식하는 최소 거리")]
	[SerializeField]
	protected float _swipeThreshold = 50f; // 스와이프를 인식하는 최소 거리

	[Header("아이템을 재정렬할 시간")]
	[SerializeField]
	protected float _elapsTime = 0.2f; // 드레그 후 아이템을 재정렬할 시간

	protected Vector2 _dragStartPosition	= Vector2.zero;
	protected Vector2 _dragEndPosition		= Vector2.zero;

	protected float _curMoveValocity	= 0f;
	protected bool	_isDragging			= false;

	protected RectTransform _trCash	= null;

	protected Coroutine _coScrollViewMove = null;

	protected IRecyclableScrollRectDataSource _dataSource;
	protected int _totalItemCount = 0;
	protected int _startIndex = 0;

	public static Action OnMoveStart	= null;
	public static Action OnMoveEnd		= null;

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------


	protected void Awake()
	{
		if( null == _recyclableScrollRect )
			_recyclableScrollRect = gameObject.GetComponent<RecyclableScrollRect>();

		if( null != _recyclableScrollRect )
		{
			_recyclableScrollRect.onValueChanged.AddListener( OnEvent_ValueChange );

			_recyclableScrollRect.OnInitialized -= OnEvent_ScrollViewInitialized;
			_recyclableScrollRect.OnInitialized += OnEvent_ScrollViewInitialized;

			_trCash = _recyclableScrollRect.content;
		}

		_isDragging = false;
		_startIndex = 0;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	protected void OnDestroy()
	{
		if( null != _recyclableScrollRect )
		{
			_recyclableScrollRect.OnInitialized -= OnEvent_ScrollViewInitialized;

			_recyclableScrollRect.onValueChanged.RemoveListener( OnEvent_ValueChange );
		}

		_isDragging = false;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 드래그 시작 시 호출
	/// </summary>
	public void OnBeginDrag( PointerEventData eventData )
	{
		SetdragStartPosition( eventData.position );

		OnMoveStart.SafeExcute();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 드레그 이동시 호출
	/// </summary>
	/// <param name="eventData"></param>
	public void OnDrag( PointerEventData eventData )
	{
		SetdragStartPosition( eventData.position );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 드래그 종료 시 호출
	/// </summary>
	public void OnEndDrag( PointerEventData eventData )
	{
		_dragEndPosition = eventData.position;
		HandleSwipeGesture();
		_isDragging = false;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 드래그 시작 위치를 세팅한다.
	/// </summary>
	/// <param name="pos"></param>
	protected void SetdragStartPosition( Vector2 pos )
	{
		_isDragging = true;

		_dragStartPosition = pos;

		_coScrollViewMove.SafeStopCorutine( this );
		_coScrollViewMove = null;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 스와이프 제스처를 처리
	/// </summary>
	protected void HandleSwipeGesture()
	{
		Vector2 dragDelta = _dragEndPosition - _dragStartPosition;

		float dragdeltaX = Mathf.Abs( dragDelta.x );
		float dragdeltaY = Mathf.Abs( dragDelta.y );

		// 수직 스와이프 감지
		if( dragdeltaX < dragdeltaY &&
			_swipeThreshold < dragdeltaY )
		{
			SetSwipeState( dragDelta.y );
		}
		else
		// 그냥 손을 떼었다.
		{
			SetSwipeState( 0f );
		}

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 스와이프 상태 세팅
	/// </summary>
	/// <param name="posY"></param>
	protected void SetSwipeState( float posY )
	{
		int curIdx = GetCurIndex();

		int destIdx = curIdx;

		if( 0f != posY )
			destIdx = posY < 0 ? curIdx + 1 : curIdx - 1;

		int itemCount = _recyclableScrollRect.DataSource.GetItemCount();
		float cellSize = 1.0f / ( itemCount - 1 );

		float destPosY = destIdx * cellSize;
		
		SetScrollViewMove( destPosY );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 위치를 기반으로 현재 인덱스를 가져온다.
	/// </summary>
	/// <returns></returns>
	protected int GetCurIndex()
	{
		int itemCount = _recyclableScrollRect.DataSource.GetItemCount();

		float cellSize = 1.0f / ( itemCount - 1 );
		float hCellSize = cellSize * 0.5f;

		float curPosY = ( _curMoveValocity - hCellSize ) / cellSize;

		int curIndex = curPosY < 0 ? 0 : ( int )( curPosY + 1 );

		return curIndex;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 대상 까지 이동 세팅
	/// </summary>
	/// <param name="destVelocity"></param>
	protected void SetScrollViewMove( float destVelocity )
	{
		if( destVelocity < 0f )
			destVelocity = 0f;
		else if( 1f < destVelocity )
			destVelocity = 1f;

		_coScrollViewMove.SafeStopCorutine( this );
		_coScrollViewMove = null;

		_coScrollViewMove = StartCoroutine( FocusingMove( _curMoveValocity, destVelocity ) );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 스크롤뷰의 값이 변경될 때 호출되는 이벤트 핸들러
	/// </summary>
	/// <param name="v2Pos"></param>
	protected void OnEvent_ValueChange( Vector2 v2Pos )
	{
		float posY = v2Pos.y;

		_curMoveValocity = posY;

		if( posY <= 0 || 1 <= posY )
			return;

		_curMoveValocity = posY;

		if( true == _isDragging )
			return;

		_recyclableScrollRect.StopMovement();

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 스크롤뷰가 초기화 완료 되었을때 호출됨
	/// </summary>
	protected void OnEvent_ScrollViewInitialized()
	{

		_dataSource		= _recyclableScrollRect.DataSource;
		_totalItemCount = _dataSource.GetItemCount();

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 스크롤뷰를 부드럽게 이동시키는 코루틴
	/// </summary>
	/// <param name="curPosY"></param>
	/// <param name="destPosY"></param>
	/// <returns></returns>
	protected IEnumerator FocusingMove( float curPosY, float destPosY )
	{
		float StartTime = Time.realtimeSinceStartup;
		float elpasTime = 0f;
		float algToRad = Mathf.PI * 0.5f / _elapsTime; 
		float val = 0f;

		while( elpasTime < _elapsTime )
		{
			val = Mathf.Lerp( curPosY, destPosY, Mathf.Sin( elpasTime * algToRad ) );

			_recyclableScrollRect.verticalNormalizedPosition = val;

			yield return null;

			elpasTime = Time.realtimeSinceStartup - StartTime;
		}

		_recyclableScrollRect.verticalNormalizedPosition = destPosY;

		OnMoveEnd.SafeExcute();

		_coScrollViewMove = null;

		yield return null;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//RecyclableScrollViewController
