/**---------------------------------------------------------------------------------
 * @file RailMapBuilderWindow.cs
 * @brief 맵 저작 씬에서 RailRunner 맵을 구성하고, 마커 베이크 + 맵 서브신 Export
 *        + 라이팅 베이크 + Build Settings 등록을 수행하는 에디터 툴.
 *///-------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Tools/RailRunner/Map Builder. View_RailMap 루트를 대상으로 맵을 빌드한다.
/// 1) 마커 베이크 → 2) 맵 서브신 Export(00.Scenes/Maps) → 3) 라이팅 베이크 → 4) Build Settings 등록.
/// </summary>
public class RailMapBuilderWindow : EditorWindow
{
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

    private const string MAPS_FOLDER   = "Assets/00.Scenes/Maps";
    private const string MATERIAL_PATH = "Assets/00.Scenes/Maps/SpeedMarker.mat";

    private const float RAY_UP   = 1000f;   // 높이 샘플 Raycast 시작점(격자 평면 위)
    private const float RAY_DOWN = 2000f;   // 추가 하강 거리

    private View_RailMap     _target         = null;
    private string           _sceneName      = "";
    private bool             _autoBake       = true;
    private LightmapBakeType _bakeLightMode  = LightmapBakeType.Mixed;
    private float            _gridMargin     = 6f;
    private bool             _bakeGridHeight = true;
    private string           _status         = "대기 중.";
    private Vector2          _scroll         = Vector2.zero;

    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 메뉴에서 빌더 창을 연다.
    /// </summary>
    [MenuItem( "Tools/RailRunner/Map Builder" )]
    private static void Open()
    {
        RailMapBuilderWindow win = GetWindow<RailMapBuilderWindow>( "Rail Map Builder" );
        win.minSize = new Vector2( 360f, 420f );
        win.Show();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 창 GUI를 그린다.
    /// </summary>
    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView( _scroll );

        EditorGUILayout.LabelField( "RailRunner 맵 빌더", EditorStyles.boldLabel );
        EditorGUILayout.Space();

        // 타겟
        EditorGUILayout.LabelField( "대상 맵 (View_RailMap)", EditorStyles.miniBoldLabel );
        _target = (View_RailMap)EditorGUILayout.ObjectField( _target, typeof( View_RailMap ), true );
        if( GUILayout.Button( "현재 씬에서 자동 탐색" ) )
            FindTargetInScene();

        EditorGUILayout.Space();

        using( new EditorGUI.DisabledScope( null == _target ) )
        {
            // 마커
            EditorGUILayout.LabelField( "1. 마커", EditorStyles.miniBoldLabel );
            EditorGUILayout.BeginHorizontal();
            if( GUILayout.Button( "마커 베이크" ) )
                BakeMarkers();
            if( GUILayout.Button( "마커 클리어" ) )
                ClearMarkers();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // RailPath / 격자 정리
            EditorGUILayout.LabelField( "RailPath · 격자 정리", EditorStyles.miniBoldLabel );
            if( GUILayout.Button( "제어점 자동 정렬·등록 (CP_n)" ) )
                RebuildControlPoints();
            EditorGUILayout.BeginHorizontal();
            _gridMargin = EditorGUILayout.FloatField( "격자 여유(m)", _gridMargin );
            if( GUILayout.Button( "격자 크기 맵에 맞춤" ) )
                FitGridToBounds();
            EditorGUILayout.EndHorizontal();
            if( GUILayout.Button( "지형 높이 격자 베이크 (Terrain→Raycast)" ) )
                BakeGridHeight();

            EditorGUILayout.Space();

            // Export
            EditorGUILayout.LabelField( "2. 맵 서브신 Export", EditorStyles.miniBoldLabel );
            _sceneName = EditorGUILayout.TextField( "저장 씬 이름", _sceneName );
            _bakeGridHeight = EditorGUILayout.ToggleLeft( "Export 시 지형 높이 격자 베이크", _bakeGridHeight );
            _autoBake = EditorGUILayout.ToggleLeft( "Export 시 라이팅 자동 베이크", _autoBake );
            if( true == _autoBake )
                _bakeLightMode = (LightmapBakeType)EditorGUILayout.EnumPopup( "라이트 모드(자동 전환)", _bakeLightMode );
            EditorGUILayout.HelpBox( $"출력: {MAPS_FOLDER}/{ResolveSceneName()}.unity\n마커 베이크" + ( _bakeGridHeight ? " → 지형 높이 격자" : "" ) + " → 클론 → 서브신 저장 → Build Settings 등록" + ( _autoBake ? $" → 라이트 {_bakeLightMode} 전환 → 베이크" : "" ), MessageType.None );
            if( GUILayout.Button( "맵 서브신 Export", GUILayout.Height( 30f ) ) )
                ExportMapScene();

            EditorGUILayout.Space();

            // 라이팅 수동
            EditorGUILayout.LabelField( "3. 라이팅 (수동)", EditorStyles.miniBoldLabel );
            if( GUILayout.Button( "현재 열린 씬 라이팅 베이크" ) )
                BakeLightingCurrent();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField( "상태", EditorStyles.miniBoldLabel );
        EditorGUILayout.HelpBox( _status, MessageType.Info );

        EditorGUILayout.EndScrollView();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 현재 씬에서 View_RailMap을 탐색해 타겟으로 지정한다.
    /// </summary>
    private void FindTargetInScene()
    {
        View_RailMap found = Object.FindObjectOfType<View_RailMap>();
        if( null == found )
        {
            SetStatus( "현재 씬에 View_RailMap 없음.", true );
            return;
        }

        _target = found;
        if( true == string.IsNullOrEmpty( _sceneName ) )
            _sceneName = found.MapId;
        SetStatus( $"대상 지정: {found.name} (MapId: {found.MapId})" );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 저장할 씬 이름을 결정한다. 입력값 우선, 비면 MapId, 그것도 비면 대상 이름.
    /// </summary>
    private string ResolveSceneName()
    {
        if( false == string.IsNullOrEmpty( _sceneName ) )
            return _sceneName.Trim();
        if( null != _target && false == string.IsNullOrEmpty( _target.MapId ) )
            return _target.MapId;
        return ( null != _target ) ? _target.name : "Map";
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 마커 머티리얼을 보장하고, 레일을 따라 마커를 베이크한다. 베이크 후 GI static 플래그를 설정한다.
    /// </summary>
    private void BakeMarkers()
    {
        if( null == _target )
            return;

        View_RailMarkerBaker spawner = _target.GetComponentInChildren<View_RailMarkerBaker>( true );
        if( null == spawner )
        {
            SetStatus( "View_RailMarkerBaker 없음 → 베이크 불가.", true );
            return;
        }

        EnsureMarkerMaterial( spawner );

        Undo.RegisterFullObjectHierarchyUndo( _target.gameObject, "Bake Markers" );
        spawner.BakeMarkers();
        ApplyMarkerStaticFlags( spawner.transform );

        EditorSceneManager.MarkSceneDirty( _target.gameObject.scene );
        SetStatus( $"마커 베이크 완료: {spawner.BakedCount}개" );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 베이크된 마커를 제거한다.
    /// </summary>
    private void ClearMarkers()
    {
        if( null == _target )
            return;

        View_RailMarkerBaker spawner = _target.GetComponentInChildren<View_RailMarkerBaker>( true );
        if( null == spawner )
        {
            SetStatus( "View_RailMarkerBaker 없음.", true );
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo( _target.gameObject, "Clear Markers" );
        spawner.ClearMarkers();
        EditorSceneManager.MarkSceneDirty( _target.gameObject.scene );
        SetStatus( "마커 클리어 완료." );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// RailPath 자식 GameObject들을 계층 순서대로 CP_n으로 리네임하고 _controlPoints에 등록·갱신한다.
    /// </summary>
    private void RebuildControlPoints()
    {
        if( null == _target )
            return;

        View_RailPath rail = _target.RailPath;
        if( null == rail )
        {
            SetStatus( "RailPath 없음 → 정렬 불가.", true );
            return;
        }

        Transform root = rail.transform;
        int       n    = root.childCount;
        if( n < 4 )
        {
            SetStatus( $"제어점 {n}개 — 닫힌 곡선은 최소 4개 필요.", true );
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo( rail.gameObject, "Rebuild Control Points" );

        SerializedObject   so   = new SerializedObject( rail );
        SerializedProperty list = so.FindProperty( "_controlPoints" );
        list.ClearArray();
        list.arraySize = n;
        for( int i = 0; i < n; ++i )
        {
            Transform child = root.GetChild( i );
            child.name = "CP_" + i;
            list.GetArrayElementAtIndex( i ).objectReferenceValue = child;
        }
        so.ApplyModifiedProperties();

        rail.Rebuild();
        EditorSceneManager.MarkSceneDirty( rail.gameObject.scene );
        SetStatus( $"제어점 {n}개 정렬·등록 완료 (길이 {rail.TotalLength:F1}m)" );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 격자(View_GridFloor) 크기를 RailPath 제어점 + 맵 렌더러를 모두 포함하도록 자동 조정한다.
    /// 격자는 자신의 트랜스폼 위치 기준 정사각이므로, 중심에서의 최대 반경 기반으로 _worldSize를 산출한다.
    /// </summary>
    private void FitGridToBounds()
    {
        if( null == _target )
            return;

        View_GridFloor grid = _target.GetComponentInChildren<View_GridFloor>( true );
        if( null == grid )
        {
            SetStatus( "View_GridFloor 없음 → 맞춤 불가.", true );
            return;
        }

        Bounds b   = new Bounds();
        bool   has = false;

        // RailPath 제어점 포함
        View_RailPath rail = _target.RailPath;
        if( null != rail )
        {
            Transform rt = rail.transform;
            for( int i = 0; i < rt.childCount; ++i )
            {
                Vector3 p = rt.GetChild( i ).position;
                if( false == has ) { b = new Bounds( p, Vector3.zero ); has = true; }
                else b.Encapsulate( p );
            }
        }

        // 맵 렌더러 포함(격자 자신 제외, 빈 바운드 제외)
        Renderer[] rends = _target.GetComponentsInChildren<Renderer>( true );
        for( int i = 0; i < rends.Length; ++i )
        {
            if( rends[ i ].gameObject == grid.gameObject )
                continue;

            Bounds rb = rends[ i ].bounds;
            if( rb.size.sqrMagnitude < 1e-6f )
                continue;

            if( false == has ) { b = rb; has = true; }
            else b.Encapsulate( rb );
        }

        if( false == has )
        {
            SetStatus( "맞출 대상 없음(제어점/렌더러).", true );
            return;
        }

        Vector3 c    = grid.transform.position;
        float   dx   = Mathf.Max( Mathf.Abs( b.max.x - c.x ), Mathf.Abs( c.x - b.min.x ) );
        float   dz   = Mathf.Max( Mathf.Abs( b.max.z - c.z ), Mathf.Abs( c.z - b.min.z ) );
        float   half = Mathf.Max( dx, dz ) + Mathf.Max( 0f, _gridMargin );

        SerializedObject   so       = new SerializedObject( grid );
        SerializedProperty cellProp = so.FindProperty( "_cellSize" );
        float cell = ( null != cellProp ) ? cellProp.floatValue : 2f;
        if( cell < 0.1f )
            cell = 0.1f;

        float size = Mathf.Ceil( ( half * 2f ) / cell ) * cell;

        SerializedProperty sizeProp = so.FindProperty( "_worldSize" );
        if( null != sizeProp )
            sizeProp.floatValue = size;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty( grid.gameObject.scene );
        SetStatus( $"격자 크기 {size:F1}m로 맞춤 (여유 {_gridMargin}m, 중심 {c.x:F0},{c.z:F0})" );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 격자 교차점마다 지형 높이를 샘플해 View_GridFloor의 높이맵에 베이크한다.
    /// 포인트별 우선순위: Terrain.SampleHeight(범위 내) → 아래로 Physics.Raycast → 평면 폴백.
    /// </summary>
    private void BakeGridHeight()
    {
        if( null == _target )
            return;

        View_GridFloor grid = _target.GetComponentInChildren<View_GridFloor>( true );
        if( null == grid )
        {
            SetStatus( "View_GridFloor 없음 → 높이 베이크 불가.", true );
            return;
        }

        SerializedObject so = new SerializedObject( grid );
        float worldSize = so.FindProperty( "_worldSize" ).floatValue;
        float cellSize  = so.FindProperty( "_cellSize" ).floatValue;
        float baseH     = so.FindProperty( "_height" ).floatValue;
        if( cellSize < 0.1f )
            cellSize = 0.1f;
        if( worldSize < cellSize )
            worldSize = cellSize;

        float half  = worldSize * 0.5f;
        int   lines = Mathf.FloorToInt( worldSize / cellSize ) + 1;
        int   count = lines * lines;

        Transform gt      = grid.transform;
        Terrain   terrain = _target.GetComponentInChildren<Terrain>( true );
        if( null == terrain )
            terrain = Terrain.activeTerrain;

        // 직전 이동된 지오메트리도 raycast가 잡도록 콜라이더 위치 동기화
        Physics.SyncTransforms();

        float[] samples = new float[ count ];
        int     hit     = 0;
        for( int iz = 0; iz < lines; ++iz )
        {
            for( int ix = 0; ix < lines; ++ix )
            {
                // 격자 로컬 교차점 → 월드
                Vector3 local = new Vector3( -half + ( ix * cellSize ), baseH, -half + ( iz * cellSize ) );
                Vector3 world = gt.TransformPoint( local );

                float worldY;
                if( true == TrySampleHeight( world, terrain, out worldY ) )
                    ++hit;
                else
                    worldY = world.y;   // 폴백: 평면 높이 유지

                // 메시 정점은 로컬 좌표 → 로컬 Y로 환산해 저장
                samples[ ( iz * lines ) + ix ] = gt.InverseTransformPoint( new Vector3( world.x, worldY, world.z ) ).y;
            }
        }

        Undo.RecordObject( grid, "Bake Grid Height" );
        so.FindProperty( "_useHeightMap" ).boolValue = true;
        SerializedProperty arr = so.FindProperty( "_heightSamples" );
        arr.ClearArray();
        arr.arraySize = count;
        for( int i = 0; i < count; ++i )
            arr.GetArrayElementAtIndex( i ).floatValue = samples[ i ];
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty( grid.gameObject.scene );
        string src = ( null != terrain ) ? $"Terrain '{terrain.name}'+Raycast" : "Raycast";
        SetStatus( $"높이 베이크: {lines}x{lines}={count}점, 적중 {hit}/{count} (소스 {src})" + ( ( hit < count ) ? " — 미적중은 평면 폴백" : "" ) );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 월드 좌표 (x, z)에서 지형 표면 높이를 샘플한다. Terrain 범위 내면 SampleHeight, 아니면 아래로 Raycast.
    /// </summary>
    /// <param name="world">샘플할 월드 위치(y는 격자 평면 기준).</param>
    /// <param name="terrain">우선 사용할 Terrain(없으면 null).</param>
    /// <param name="worldY">샘플된 월드 높이.</param>
    /// <returns>샘플 성공 여부(둘 다 실패 시 false).</returns>
    private bool TrySampleHeight( Vector3 world, Terrain terrain, out float worldY )
    {
        worldY = 0f;

        // 1순위: Terrain (XZ 범위 안일 때만)
        if( null != terrain && null != terrain.terrainData )
        {
            Vector3 tp = terrain.transform.position;
            Vector3 ts = terrain.terrainData.size;
            float   lx = world.x - tp.x;
            float   lz = world.z - tp.z;
            if( 0f <= lx && lx <= ts.x && 0f <= lz && lz <= ts.z )
            {
                worldY = terrain.SampleHeight( world ) + tp.y;
                return true;
            }
        }

        // 2순위: 아래로 Physics.Raycast(콜라이더 필요)
        Vector3      origin = new Vector3( world.x, world.y + RAY_UP, world.z );
        RaycastHit   info;
        if( true == Physics.Raycast( origin, Vector3.down, out info, RAY_UP + RAY_DOWN ) )
        {
            worldY = info.point.y;
            return true;
        }

        return false;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 마커 공유 머티리얼 에셋을 보장하고 스포너에 할당한다.
    /// </summary>
    private void EnsureMarkerMaterial( View_RailMarkerBaker spawner )
    {
        SerializedObject   so   = new SerializedObject( spawner );
        SerializedProperty prop = so.FindProperty( "_markerMaterial" );
        if( null != prop && null != prop.objectReferenceValue )
            return;

        EnsureMapsFolder();

        Material mat = AssetDatabase.LoadAssetAtPath<Material>( MATERIAL_PATH );
        if( null == mat )
        {
            Shader shader = Shader.Find( "Universal Render Pipeline/Lit" );
            if( null == shader )
                shader = Shader.Find( "Standard" );

            mat = new Material( shader );
            mat.name = "SpeedMarker";
            if( mat.HasProperty( "_BaseColor" ) )
                mat.SetColor( "_BaseColor", new Color( 1f, 0.5f, 0.15f, 1f ) );
            if( mat.HasProperty( "_Color" ) )
                mat.SetColor( "_Color", new Color( 1f, 0.5f, 0.15f, 1f ) );

            AssetDatabase.CreateAsset( mat, MATERIAL_PATH );
            AssetDatabase.SaveAssets();
        }

        if( null != prop )
        {
            prop.objectReferenceValue = mat;
            so.ApplyModifiedProperties();
        }
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 마커 자식들에 ContributeGI static 플래그를 설정한다(라이트맵 대상).
    /// </summary>
    private void ApplyMarkerStaticFlags( Transform root )
    {
        for( int i = 0; i < root.childCount; ++i )
        {
            Transform child = root.GetChild( i );
            if( View_RailMarkerBaker.MARKER_NAME == child.name )
                GameObjectUtility.SetStaticEditorFlags( child.gameObject, StaticEditorFlags.ContributeGI );
        }
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 맵 루트를 클론해 빈 서브신으로 옮기고 00.Scenes/Maps에 저장한다.
    /// Build Settings에 등록하고, 옵션에 따라 라이팅을 베이크한다.
    /// </summary>
    private void ExportMapScene()
    {
        if( null == _target )
            return;

        // 1. 마커 선 베이크(최신 보장)
        BakeMarkers();

        // 1-2. 지형 높이 격자 베이크(옵션)
        if( true == _bakeGridHeight )
            BakeGridHeight();

        EnsureMapsFolder();

        string mapId      = ResolveSceneName();
        string scenePath  = $"{MAPS_FOLDER}/{mapId}.unity";
        string authoring  = _target.gameObject.scene.path;

        // 2. 저작 씬 먼저 저장(라이팅 단계 씬 전환 대비)
        EditorSceneManager.SaveOpenScenes();

        // 3. 빈 서브신 생성 후 맵 루트 클론 이동
        Scene mapScene      = EditorSceneManager.NewScene( NewSceneSetup.EmptyScene, NewSceneMode.Additive );
        GameObject clone    = Object.Instantiate( _target.gameObject );
        clone.name          = mapId;
        SceneManager.MoveGameObjectToScene( clone, mapScene );

        bool saved = EditorSceneManager.SaveScene( mapScene, scenePath );
        if( false == saved )
        {
            EditorSceneManager.CloseScene( mapScene, true );
            SetStatus( $"서브신 저장 실패: {scenePath}", true );
            return;
        }

        EditorSceneManager.CloseScene( mapScene, true );
        AssetDatabase.Refresh();

        // 4. Build Settings 등록
        RegisterBuildScene( scenePath );

        // 5. 라이팅 자동 베이크(씬 단독 오픈 → 베이크 → 저장 → 저작 씬 복귀)
        if( true == _autoBake )
        {
            EditorSceneManager.OpenScene( scenePath, OpenSceneMode.Single );
            ConvertSceneLightsToBake( _bakeLightMode );
            bool baked = Lightmapping.Bake();
            EditorSceneManager.SaveOpenScenes();

            if( false == string.IsNullOrEmpty( authoring ) )
                EditorSceneManager.OpenScene( authoring, OpenSceneMode.Single );

            SetStatus( $"Export 완료 + 라이트 {_bakeLightMode} 전환 + 베이크({( baked ? "성공" : "실패/스킵" )}): {scenePath}" );
        }
        else
        {
            SetStatus( $"Export 완료(라이팅 미베이크): {scenePath}" );
        }
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 현재 씬의 모든 라이트를 지정 베이크 모드(Mixed/Baked)로 전환한다. Realtime은 라이트맵 미생성이므로 베이크 전 호출.
    /// </summary>
    /// <param name="mode">전환할 라이트맵 베이크 모드.</param>
    private void ConvertSceneLightsToBake( LightmapBakeType mode )
    {
        if( LightmapBakeType.Realtime == mode )
            return;

        Light[] lights = Object.FindObjectsOfType<Light>();
        for( int i = 0; i < lights.Length; ++i )
        {
            Undo.RecordObject( lights[ i ], "Convert Light Bake Mode" );
            lights[ i ].lightmapBakeType = mode;
        }

        DebugExtensions.Log( $"[RailMapBuilder] 라이트 {lights.Length}개 → {mode} 전환", Color.cyan );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 현재 열린 씬들의 라이팅을 베이크한다(수동).
    /// </summary>
    private void BakeLightingCurrent()
    {
        bool baked = Lightmapping.Bake();
        EditorSceneManager.SaveOpenScenes();
        SetStatus( baked ? "라이팅 베이크 완료(현재 씬)." : "라이팅 베이크 실패/스킵." );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 씬 경로를 Build Settings에 중복 없이 등록한다.
    /// </summary>
    private void RegisterBuildScene( string scenePath )
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>( EditorBuildSettings.scenes );
        for( int i = 0; i < scenes.Count; ++i )
        {
            if( scenePath == scenes[ i ].path )
                return;
        }

        scenes.Add( new EditorBuildSettingsScene( scenePath, true ) );
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 00.Scenes/Maps 폴더를 보장한다.
    /// </summary>
    private void EnsureMapsFolder()
    {
        if( true == AssetDatabase.IsValidFolder( MAPS_FOLDER ) )
            return;

        if( false == AssetDatabase.IsValidFolder( "Assets/00.Scenes" ) )
            AssetDatabase.CreateFolder( "Assets", "00.Scenes" );

        AssetDatabase.CreateFolder( "Assets/00.Scenes", "Maps" );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 상태 메시지를 갱신하고 로그를 남긴다.
    /// </summary>
    private void SetStatus( string message, bool isError = false )
    {
        _status = message;
        if( true == isError )
            DebugExtensions.LogError( $"[RailMapBuilder] {message}", Color.red );
        else
            DebugExtensions.Log( $"[RailMapBuilder] {message}", Color.cyan );
        Repaint();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

}//RailMapBuilderWindow
