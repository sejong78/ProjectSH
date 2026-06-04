/**---------------------------------------------------------------------------------
 * @file InGameEventBusManager.cs
 * @date 2021/5/26
 * @author sejong
 * @brief InGame 에서 캐릭터의 행동에 따라 발생하는 Event 의 Routing 을 위한 EventBus 
 *///-------------------------------------------------------------------------------
using BaseSingleton;


public enum eInGameEvent
{
	None = -1,

	HPChanged, // HP가 변경되었을 때

	Game_Over, // 게임 오버가 되었을 때

	EnergyChanged, // 에너지가 변경되었을 때

	Refresh_UserData, // UserData 가 변경되었을 때

	ReCalculate_AttackPower, // 전투력을 재계산 해야 할 때

	Max, 
}

/// <summary>
/// @class InGameEventBusManager
/// @date 2021/5/26
/// @author sejong
/// @brief InGame 에서 캐릭터의 행동에 따라 발생하는 Event 의 Routing 을 위한 EventBus 클레스
/// </summary>
public class InGameEventBusManager : BaseSingleton<InGameEventBusManager>, IBaseSingleton
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public delegate bool EventFunc( params object[] prm );

	protected EventFunc[] _eventArray = new EventFunc[ (int)eInGameEvent.Max ]; 

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 이벤트를 구독합니다.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="func"></param>
	public void AddEventSubscribe( eInGameEvent key, EventFunc func )
	{
		if( func == null )
		{
			return;
		}

		// 기존에 등록된 이벤트가 없으면 할당 후 리턴
		if( null == _eventArray[(int)key] )
		{
			_eventArray[ ( int )key ] = func;
			return;
		}

		// 기 등록된 이벤트가 있으므로, 추가
		_eventArray[ ( int )key ] -= func;
		_eventArray[ ( int )key ] += func;

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 특정 이벤트 구독을 해제 합니다.
	/// 이벤트가 null 이면, 해당 키값의 이벤트 모두 제거
	/// </summary>
	/// <param name="key"></param>
	/// <param name="func"></param>
	public void RemoveEventSubscribe( eInGameEvent key, EventFunc func = null )
	{

		// 이벤트 제거
		_eventArray[ ( int )key ] -= func;

	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 구독자에게 알림
	/// </summary>
	/// <param name="key"></param>
	/// <param name="prms"></param>
	public bool FireEvent( eInGameEvent key, params object[] prms )
	{
#if UNITY_EDITOR
		//DebugExtensions.Log( $"[InGameEventBus] FireEvent( {key} )", Color.yellow );
#endif//UNITY_EDITOR

		return _eventArray[ ( int )key ] == null ? false : _eventArray[(int)key].Invoke( prms );

	}


	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 할당된 이벤트를 모두 정리한다.
	/// </summary>
	public void RemoveAll()
	{
		for( int i = 0; i < _eventArray.Length; ++i )
			_eventArray[ i ] = null;
	}

    public void Initialize()
    {
        // throw new System.NotImplementedException();
    }

    public void Release()
    {
        _eventArray = null;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

}//InGameEventBusManager

