/**---------------------------------------------------------------------------------
 * @file ResourceManager.cs
 * @brief 실제 게임 로직에서 호출하며, 메모리에 올라온 에셋을 관리합니다.
 *///-------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using BaseSingleton;

/// <summary>
/// @class ResourceManager
/// @brief 
/// </summary>
public class ResourceManager : BaseSingleton<ResourceManager>, IBaseSingleton
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	private const int LRUCapacity = 20;

	// 모든 정보를 하나의 딕셔너리로 관리
	private Dictionary<string, AssetEntry>	_assetCache = new Dictionary<string, AssetEntry>();
	private LinkedList<string>				_lruList	= new LinkedList<string>();

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 어드레서블 시스템을 통해 에셋을 로드하는 메인 함수입니다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="address"></param>
	/// <param name="isPermanent"></param>
	/// <returns></returns>
	public async UniTask<T> LoadAsset<T>( string address, bool isPermanent = false ) where T : Object
	{
		// 1. 캐시 확인 및 상태 업데이트, 기존 로드내역이 있다면 리턴
		if( true == _assetCache.TryGetValue( address, out AssetEntry entry ) )
		{
			entry.RefCount++;
			entry.IsPermanent = isPermanent || entry.IsPermanent; // 상주 속성 갱신

			if( true == _lruList.Contains( address ) )
				_lruList.Remove( address );

			return entry.GetAsset<T>();
		}

		// 2. 로드 내역이 없으므로, 신규 로드
		var handle = Addressables.LoadAssetAsync<T>( address );
		try
		{
			await handle.ToUniTask();

			if( handle.Status == AsyncOperationStatus.Succeeded )
			{
				// 엔트리 생성 및 캐시 등록
				_assetCache[ address ] = new AssetEntry( address, handle, typeof( T ), isPermanent );

				DebugExtensions.Log( $"[ResourceManager] 로드 및 엔트리 등록: {address}" );
				return handle.Result;
			}
		}
		catch( System.Exception e )
		{
			DebugExtensions.LogError( $"[ResourceManager] 로드 실패: {address} - {e.Message}" );
		}

		if( true == handle.IsValid() ) 
			Addressables.Release( handle ); //

		return null;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 로드된 에셋을 언로드하는 메인 함수입니다. 참조 카운트를 감소시키고, 
	/// 필요 시 LRU 리스트에 추가하여 나중에 메모리에서 해제될 수 있도록 합니다.
	/// </summary>
	/// <param name="address"></param>
	public void UnloadAsset( string address )
	{
		if( !_assetCache.TryGetValue( address, out AssetEntry entry ) ) 
			return;

		entry.RefCount--;

		if( entry.RefCount <= 0 )
		{
			// 상주 에셋은 메모리 해제 후보(LRU)에 넣지 않음
			if( true == entry.IsPermanent ) 
				return;

			_lruList.AddLast( address );
			UpdateLRUCache();
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	private void UpdateLRUCache()
	{
		while( LRUCapacity < _lruList.Count )
		{
			string oldestAddress = _lruList.First.Value;

			_lruList.RemoveFirst();

			if( false == _assetCache.TryGetValue( oldestAddress, out AssetEntry entry ) )
				continue;

			if( true == entry.Handle.IsValid() )
				Addressables.Release( entry.Handle ); //

			_assetCache.Remove( oldestAddress );

			DebugExtensions.Log( $"[ResourceManager] LRU 해제: {oldestAddress}" );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void ResetValues()
	{
		List<string> keysToRemove = new List<string>();

		foreach( var pair in _assetCache )
		{
			// 상주 에셋이 아니고, 현재 아무도 참조하지 않는 경우(또는 씬 전환 시 강제 정리 대상)
			if( !pair.Value.IsPermanent )
			{
				keysToRemove.Add( pair.Key );
			}
		}

		foreach( var key in keysToRemove )
		{
			if( _assetCache.TryGetValue( key, out AssetEntry entry ) )
			{
				if( entry.Handle.IsValid() ) Addressables.Release( entry.Handle ); //
				_assetCache.Remove( key );
			}
		}

		_lruList.Clear();
		DebugExtensions.Log( $"[ResourceManager] 비상주 리소스 정리 완료 (상주: {_assetCache.Count}개)" );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void Initialize() => ResetValues();

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void Release()
	{
		foreach( var entry in _assetCache.Values )
		{
			if( false == entry.Handle.IsValid() )
				continue;

			Addressables.Release( entry.Handle ); //
		}

		_assetCache.Clear();
		_lruList.Clear();
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// @class AssetEntry
	/// @brief 먼저 에셋의 모든 정보를 담는 내부 클래스를 정의합니다.
	/// </summary>
	private class AssetEntry
	{
		//@@-------------------------------------------------------------------------------------------------------------------------
		//@@-------------------------------------------------------------------------------------------------------------------------
		
		public string		Address			= "";
		public int			RefCount		= 0;
		public bool			IsPermanent		= false;
		public System.Type	AssetType		= null;

		public AsyncOperationHandle Handle;

		//@@-------------------------------------------------------------------------------------------------------------------------
		//@@-------------------------------------------------------------------------------------------------------------------------

		public AssetEntry( string address, AsyncOperationHandle handle, System.Type type, bool isPermanent )
		{
			Address		= address;
			Handle		= handle;
			AssetType	= type;
			IsPermanent = isPermanent;
			RefCount	= 1; // 생성 시 기본 카운트 1
		}

		//@@-------------------------------------------------------------------------------------------------------------------------
		
		public T GetAsset<T>() where T : Object => Handle.Convert<T>().Result;

		//@@-------------------------------------------------------------------------------------------------------------------------
		//@@-------------------------------------------------------------------------------------------------------------------------

	}//AssetEntry

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//ResourceManager

