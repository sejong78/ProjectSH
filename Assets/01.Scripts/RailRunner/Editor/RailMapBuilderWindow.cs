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

    private View_RailMap     _target        = null;
    private bool             _autoBake      = true;
    private LightmapBakeType _bakeLightMode = LightmapBakeType.Mixed;
    private string           _status        = "대기 중.";
    private Vector2          _scroll        = Vector2.zero;

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

            // Export
            EditorGUILayout.LabelField( "2. 맵 서브신 Export", EditorStyles.miniBoldLabel );
            _autoBake = EditorGUILayout.ToggleLeft( "Export 시 라이팅 자동 베이크", _autoBake );
            if( true == _autoBake )
                _bakeLightMode = (LightmapBakeType)EditorGUILayout.EnumPopup( "라이트 모드(자동 전환)", _bakeLightMode );
            EditorGUILayout.HelpBox( $"출력: {MAPS_FOLDER}/<MapId>.unity\n마커 베이크 → 클론 → 서브신 저장 → Build Settings 등록" + ( _autoBake ? $" → 라이트 {_bakeLightMode} 전환 → 베이크" : "" ), MessageType.None );
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
        SetStatus( $"대상 지정: {found.name} (MapId: {found.MapId})" );
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

        EnsureMapsFolder();

        string mapId      = string.IsNullOrEmpty( _target.MapId ) ? _target.name : _target.MapId;
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
