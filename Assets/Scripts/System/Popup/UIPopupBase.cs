/**---------------------------------------------------------------------------------
 * @file UIPopupBase.cs
 * @brief 
 *///-------------------------------------------------------------------------------
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// @class UIPopupBase
/// @brief 
/// </summary>
public class UIPopupBase<T> : MonoBehaviour
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 팝업이 닫힐 때까지 대기하는 UniTaskCompletionSource
	/// </summary>
	private UniTaskCompletionSource<T> _tcs = null;

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 팝업을 표시하고 결과를 기다리는 메서드입니다. 기존에 진행 중인 작업이 있다면 취소 처리 후 새로운 작업을 시작합니다.
	/// </summary>
	public async UniTask<T> ShowPopup( CancellationToken ct, Intent it )
	{
		// 1. 기존에 진행 중인 작업이 있다면 취소 처리
		// 팝업이 이미 떠 있는데 다시 ShowAsync가 호출된 경우를 방지합니다.
		if( null != _tcs && UniTaskStatus.Pending == _tcs.Task.Status )
		{
			_tcs.TrySetCanceled( ct );
		}

		// 2. 새로운 TCS 생성
		_tcs = new UniTaskCompletionSource<T>();

		// 3. 내부 세팅
		Initialize( it );

		// 4. 외부에서 씬 전환 등으로 인해 오브젝트가 파괴될 때 대응
		// 팝업 오브젝트가 파괴되면 TCS도 함께 취소되도록 등록
		// CancellationToken 등록 (유효한 경우에만)
		if( true == ct.CanBeCanceled )
		{
			using( ct.Register( () => _tcs.TrySetCanceled() ) )
			{
				return await _tcs.Task;
			}
		}

		return await _tcs.Task;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 팝업을 닫고 결과를 반환하는 메서드입니다. 팝업이 떠 있지 않은 상태에서 호출되는 경우를 방지하기 위해 TCS가 존재하고 대기 중인 상태인지 확인합니다.
	/// </summary>
	protected async UniTaskVoid ClosePopup( T result )
	{
		// 팝업이 떠 있지 않은 상태에서 호출되는 경우를 방지
		if( null == _tcs || UniTaskStatus.Pending != _tcs.Task.Status || null == gameObject ) 
			return;

		_tcs.TrySetResult( result );

		ReleaseValues();
		// 작업 완료 후 참조 해제
		_tcs = null;

		await UniTask.Yield(); // 현재 프레임이 끝날 때까지 대기하여 UI 업데이트가 완료되도록 함

		if( null != gameObject )
			Destroy( gameObject );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	#region MonoBehaviour
	protected virtual void Awake()
	{
		AddListener();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	protected virtual void OnDestroy()
	{
		RemoveListener();
	}
	#endregion//MonoBehaviour

	//@@-------------------------------------------------------------------------------------------------------------------------

	#region virtual
	/// <summary>
	/// 팝업이 열릴 때 필요한 초기화 작업을 수행하는 메서드입니다.
	/// </summary>
	/// <param name="it"></param>
	protected virtual void Initialize( Intent it )
	{
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 팝업이 닫힐 때 필요한 정리 작업을 수행하는 메서드입니다.
	/// </summary>
	protected virtual void ReleaseValues()
	{

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 뭔가 이벤트 리스너를 등록해야 하는 경우 이 메서드에서 등록하도록 합니다. 
	/// </summary>
	protected virtual void AddListener()
	{
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 뭔가 이벤트 리스너를 등록해야 하는 경우 이 메서드에서 해제하도록 합니다.
	/// </summary>
	protected virtual void RemoveListener()
	{
	}

	#endregion//virtual

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 팝업을 생성하고 표시하는 정적 메서드입니다. 
	/// 팝업 이름이 제공되지 않은 경우, 제네릭 타입 UPopup의 이름을 사용하여 팝업을 생성합니다. 
	/// 생성된 팝업 인스턴스의 RectTransform을 초기화하여 화면에 맞게 조정한 후, 
	/// ShowPopup 메서드를 호출하여 팝업을 표시하고 결과를 반환합니다.
	/// </summary>
	public static async UniTask<TResult> Show_Async<TResult,UPopup>( string popupName = "", CancellationToken? ct = null, Intent it = null ) where UPopup : UIPopupBase<TResult>
	{
		if( true == popupName.IsNullorEmpty() )
			popupName = typeof( UPopup ).Name.ToString();

		UIPopupBase<TResult> inst = null;

		inst = await PopupManager.CreatePopup<TResult, UPopup>( popupName );

		if( null == inst )
		{
#if DEBUG
			DebugExtensions.LogError( $"{popupName} 팝업의 생성에 실패했습니다.", Color.white );
#endif//DEBUG
			return default( TResult );
		}

		var rt = inst.gameObject.GetComponent<RectTransform>();
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;

		if( null == ct )
			ct = new CancellationToken();

		return await inst.ShowPopup( ct.Value, it );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
}//UIPopupBase

