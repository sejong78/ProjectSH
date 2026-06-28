/**---------------------------------------------------------------------------------
 * @file View_MapLoader.cs
 * @brief 맵 서브신을 additive로 로드하고, 맵의 RailPath를 카메라 앵커에 바인딩한다.
 *        로딩 구현(SceneManager/Addressables)은 한 메서드로 격리해 교체 가능하다.
 *///-------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

/// <summary>
/// 부트스트랩/플레이 씬에 배치. Start에서(또는 외부 호출로) 맵 서브신을 additive 로드하고,
/// 로드된 View_RailMap에서 RailPath를 추출해 View_RailAnchor에 BindRail로 주입한다.
/// 현재 로딩은 SceneManager additive(Build Settings 등록 필요), 추후 Addressables로 교체.
/// </summary>
public class View_MapLoader : MonoBehaviour
{
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

    [Header( "바인딩 대상" )]
    [SerializeField] private View_RailAnchor _anchor = null;

    [Header( "맵 설정" )]
    [Tooltip( "로드할 맵 서브신 이름(= MapId, Build Settings 등록명)" )]
    [SerializeField] private string _mapId = "Map_RailRunner";
    [Tooltip( "Start 시 자동 로드" )]
    [SerializeField] private bool _loadOnStart = true;
    [Tooltip( "로드 후 맵 씬을 활성 씬으로 설정(베이크 라이팅/환경광 적용)" )]
    [SerializeField] private bool _setActiveScene = true;

    private Scene _loadedScene;
    private bool  _isLoaded = false;

    /// <summary> 맵 로드 완료 여부. </summary>
    public bool IsLoaded { get { return _isLoaded; } }

    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    #region MonoBehaviour

    private void Start()
    {
        if( true == _loadOnStart )
            LoadMap_Async().Forget();
    }

    #endregion

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 맵 서브신을 additive 로드하고 앵커에 레일을 바인딩한다.
    /// </summary>
    public async UniTask LoadMap_Async()
    {
        if( true == _isLoaded )
        {
            DebugExtensions.Log( $"[View_MapLoader] 이미 로드됨: {_mapId}", Color.gray );
            return;
        }

        if( true == string.IsNullOrEmpty( _mapId ) )
        {
            DebugExtensions.LogError( "[View_MapLoader] MapId 미지정 → 로드 불가.", Color.red );
            return;
        }

        bool ok = await LoadSceneAdditive_Async( _mapId );
        if( false == ok )
        {
            DebugExtensions.LogError( $"[View_MapLoader] 씬 로드 실패: {_mapId} (Build Settings 등록 확인)", Color.red );
            return;
        }

        Scene scene = SceneManager.GetSceneByName( _mapId );
        if( false == scene.IsValid() )
        {
            DebugExtensions.LogError( $"[View_MapLoader] 로드 씬 무효: {_mapId}", Color.red );
            return;
        }

        _loadedScene = scene;
        _isLoaded    = true;

        if( true == _setActiveScene )
            SceneManager.SetActiveScene( scene );

        View_RailMap map = FindMap( scene );
        if( null == map )
        {
            DebugExtensions.LogError( $"[View_MapLoader] '{_mapId}'에 View_RailMap 없음.", Color.red );
            return;
        }

        if( false == map.EnsureReady() )
        {
            DebugExtensions.LogError( $"[View_MapLoader] '{_mapId}' 레일 준비 실패.", Color.red );
            return;
        }

        BindAnchor( map );
        DebugExtensions.Log( $"[View_MapLoader] 맵 로드·바인딩 완료: {_mapId}", Color.cyan );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 로드한 맵 서브신을 언로드한다.
    /// </summary>
    public async UniTask UnloadMap_Async()
    {
        if( false == _isLoaded )
            return;

        AsyncOperation op = SceneManager.UnloadSceneAsync( _loadedScene );
        if( null != op )
            await op.ToUniTask();

        _isLoaded = false;
        DebugExtensions.Log( $"[View_MapLoader] 맵 언로드: {_mapId}", Color.gray );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// === 로딩 격리 지점 ===
    /// 현재: SceneManager additive 로드(Build Settings 등록 필요).
    /// 추후: Addressables.LoadSceneAsync( mapId, LoadSceneMode.Additive )로 본문만 교체.
    /// </summary>
    /// <param name="mapId">맵 씬 이름.</param>
    /// <returns>로드 성공 여부.</returns>
    private async UniTask<bool> LoadSceneAdditive_Async( string mapId )
    {
        AsyncOperation op = SceneManager.LoadSceneAsync( mapId, LoadSceneMode.Additive );
        if( null == op )
            return false;

        await op.ToUniTask();
        return true;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 씬 루트들에서 View_RailMap을 탐색한다.
    /// </summary>
    private View_RailMap FindMap( Scene scene )
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for( int i = 0; i < roots.Length; ++i )
        {
            View_RailMap map = roots[ i ].GetComponentInChildren<View_RailMap>( true );
            if( null != map )
                return map;
        }
        return null;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 앵커를 확보(미지정 시 탐색)하고 맵 레일을 바인딩한다.
    /// </summary>
    private void BindAnchor( View_RailMap map )
    {
        if( null == _anchor )
            _anchor = Object.FindObjectOfType<View_RailAnchor>();

        if( null == _anchor )
        {
            DebugExtensions.LogError( "[View_MapLoader] View_RailAnchor 없음 → 바인딩 불가.", Color.red );
            return;
        }

        _anchor.BindRail( map.RailPath );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

}//View_MapLoader
