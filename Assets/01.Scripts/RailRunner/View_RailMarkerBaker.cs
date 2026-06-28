/**---------------------------------------------------------------------------------
 * @file View_RailMarkerBaker.cs
 * @brief 레일 주변에 속도 기준물(기둥)을 배치한다. 런타임 스폰이 아니라
 *        맵 저작 시점에 실제 구조물로 베이크되어 맵 서브신에 저장된다.
 *///-------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// View_RailPath를 따라 일정 거리 간격으로 단순 기둥을 좌/우 교대 배치한다.
/// 아트가 아니라 "지나가는 속도"를 인지시키는 기준물(역기획 인사이트).
/// 마커는 BakeMarkers()로 에디터에서 실제 자식 GameObject로 생성되어 씬에 저장된다.
/// </summary>
public class View_RailMarkerBaker : MonoBehaviour
{
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>베이크된 마커 GameObject의 공통 이름. 클리어 시 식별용.</summary>
    public const string MARKER_NAME = "SpeedMarker";

    [Header( "레일 참조" )]
    [SerializeField] private View_RailPath _rail = null;

    [Header( "베이크 부모(미지정 시 자기 자신)" )]
    [SerializeField] private Transform _markerRoot = null;

    [Header( "배치 설정" )]
    [Tooltip( "기준물 간격(m)" )]
    [SerializeField] private float _interval = 8f;
    [Tooltip( "레일 좌우 측면 거리(m)" )]
    [SerializeField] private float _lateralOffset = 5f;
    [Tooltip( "기둥 높이(m)" )]
    [SerializeField] private float _pillarHeight = 4f;
    [Tooltip( "기둥 굵기(m)" )]
    [SerializeField] private float _pillarWidth = 0.6f;
    [Tooltip( "양쪽 교대 배치(꺼지면 양측 동시)" )]
    [SerializeField] private bool _alternateSides = true;

    [Header( "기준물 머티리얼(공유 에셋)" )]
    [SerializeField] private Material _markerMaterial = null;

    /// <summary>마지막 베이크로 생성된 마커 수.</summary>
    public int BakedCount { get; private set; }

    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// 마커 베이크 부모 Transform을 반환한다. 미지정 시 자기 자신.
    /// </summary>
    private Transform GetMarkerRoot()
    {
        return null != _markerRoot ? _markerRoot : transform;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 레일을 따라 기준물을 실제 자식 구조물로 베이크한다. 기존 마커는 먼저 제거한다.
    /// 에디터(맵 저작) 컨텍스트에서 호출.
    /// </summary>
    public void BakeMarkers()
    {
        if( null == _rail )
        {
            DebugExtensions.LogError( "[View_RailMarkerBaker] 레일 미지정 → 베이크 불가.", Color.red );
            return;
        }

        if( false == _rail.IsReady )
            _rail.Rebuild();

        if( false == _rail.IsReady )
        {
            DebugExtensions.LogError( "[View_RailMarkerBaker] 레일 준비 실패 → 베이크 불가.", Color.red );
            return;
        }

        ClearMarkers();

        if( _interval < 0.5f )
            _interval = 0.5f;

        Transform root  = GetMarkerRoot();
        float     total = _rail.TotalLength;

        int  count     = 0;
        bool rightSide = true;
        for( float d = 0f; d < total; d += _interval )
        {
            Vector3 pos     = _rail.GetPositionByDistance( d );
            Vector3 tangent = _rail.GetTangentByDistance( d );
            Vector3 right   = Vector3.Cross( Vector3.up, tangent ).normalized;

            if( true == _alternateSides )
            {
                Vector3 side = rightSide ? right : -right;
                CreatePillar( root, pos + side * _lateralOffset );
                rightSide = !rightSide;
                ++count;
            }
            else
            {
                CreatePillar( root, pos + right * _lateralOffset );
                CreatePillar( root, pos - right * _lateralOffset );
                count += 2;
            }
        }

        BakedCount = count;
        DebugExtensions.Log( $"[View_RailMarkerBaker] 마커 {count}개 베이크 (간격 {_interval}m, 길이 {total:F1}m)", Color.cyan );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 베이크된 마커 자식을 모두 제거한다.
    /// </summary>
    public void ClearMarkers()
    {
        Transform          root    = GetMarkerRoot();
        List<GameObject>   targets = new List<GameObject>();

        for( int i = 0; i < root.childCount; ++i )
        {
            Transform child = root.GetChild( i );
            if( MARKER_NAME == child.name )
                targets.Add( child.gameObject );
        }

        for( int i = 0; i < targets.Count; ++i )
            DestroyImmediate( targets[ i ] );

        BakedCount = 0;

        if( 0 < targets.Count )
            DebugExtensions.Log( $"[View_RailMarkerBaker] 마커 {targets.Count}개 클리어", Color.gray );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 지정 위치에 기둥 프리미티브를 자식으로 생성한다.
    /// </summary>
    /// <param name="root">부모 Transform.</param>
    /// <param name="basePos">기둥 바닥 중심 위치.</param>
    private void CreatePillar( Transform root, Vector3 basePos )
    {
        GameObject pillar = GameObject.CreatePrimitive( PrimitiveType.Cube );
        pillar.name = MARKER_NAME;
        pillar.transform.SetParent( root, true );
        pillar.transform.position   = basePos + new Vector3( 0f, _pillarHeight * 0.5f, 0f );
        pillar.transform.localScale = new Vector3( _pillarWidth, _pillarHeight, _pillarWidth );

        // 충돌체 불필요(시각 기준물) → 제거.
        Collider col = pillar.GetComponent<Collider>();
        if( null != col )
            DestroyImmediate( col );

        if( null != _markerMaterial )
        {
            MeshRenderer mr = pillar.GetComponent<MeshRenderer>();
            if( null != mr )
                mr.sharedMaterial = _markerMaterial;
        }
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

}//View_RailMarkerBaker
