/**---------------------------------------------------------------------------------
 * @file AddressableManager.cs
 * @brief 패치 다운로드, 카탈로그 업데이트 등 인프라 작업을 담당
 *///-------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using BaseSingleton;
using Cysharp.Threading.Tasks;

/// <summary>
/// @class AddressableManager
/// @brief 
/// </summary>
public class AddressableManager : BaseSingleton<AddressableManager>, IBaseSingleton
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	private string _preloadLabel = "Preload";

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	public async UniTask Initialize_Async()
	{
		await Addressables.InitializeAsync().ToUniTask();

		DebugExtensions.Log( "[AddressableManager] 시스템 초기화 완료" );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 카탈로그 업데이트 체크 및 패치 다운로드 통합 로직
	/// </summary>
	public async UniTask<bool> UpdateAndDownload_Async( System.Action<float> onProgress )
	{
		// 1. 카탈로그 업데이트 확인
		var checkHandle = Addressables.CheckForCatalogUpdates( false );
		var catalogs	= await checkHandle.ToUniTask();

		if( 0 < catalogs.SafeCount() )
		{
			await Addressables.UpdateCatalogs( catalogs ).ToUniTask();

			DebugExtensions.Log( "[AddressableManager] 카탈로그 업데이트 완료" );
		}

		Addressables.Release( checkHandle );

		// 2. 다운로드 크기 확인
		var sizeHandle = Addressables.GetDownloadSizeAsync( _preloadLabel );

		long downloadSize = await sizeHandle.ToUniTask();

		Addressables.Release( sizeHandle );

		if( 0 < downloadSize )
		{
			DebugExtensions.Log( $"[AddressableManager] 다운로드 필요: {downloadSize} bytes" );

			onProgress.SafeExcute( 0 );

			var downloadHandle = Addressables.DownloadDependenciesAsync( _preloadLabel );

			while( !downloadHandle.IsDone )
			{
				// 진행률 전달 로직 (예: UI 이벤트 발생)
				DebugExtensions.Log( $"다운로드 중: {downloadHandle.PercentComplete * 100}%" );

				onProgress.SafeExcute( downloadHandle.PercentComplete );

				await UniTask.Yield();
			}

			onProgress.SafeExcute( 1 );

			bool success = downloadHandle.Status == AsyncOperationStatus.Succeeded;
			
			Addressables.Release( downloadHandle );
			
			return success;
		}

		DebugExtensions.Log( "[AddressableManager] 추가 다운로드 없음" );

		return true;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void Initialize() 
	{
		Initialize_Async().Forget();
	}
	
	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public void Release() 
	{ 
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
}//AddressableManager
