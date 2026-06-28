/**---------------------------------------------------------------------------------
 * @file View_RailMarkerSpawner.cs
 * @brief 레일 주변에 속도 기준물(기둥)을 주기 배치. 빠르게 지나가며 속도를 체감시키는 기능 요소.
 *///-------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// View_RailPath를 따라 일정 거리 간격으로 단순 기둥을 좌/우 교대 배치한다.
/// 아트가 아니라 "지나가는 속도"를 인지시키는 기준물(역기획 인사이트).
/// </summary>
public class View_RailMarkerSpawner : MonoBehaviour
{
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    
    [Header( "레일 참조" )]
    [SerializeField] private View_RailPath _rail = null;

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
    [Header( "기준물 색상" )]
    [SerializeField] private Color _markerColor = new Color( 1f, 0.5f, 0.15f, 1f );

    private readonly List<GameObject> _spawned = new List<GameObject>();
    private Material _runtimeMat = null;

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
	#region MonoBehaviour

	private void Start()
    {
        Spawn();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    
    private void OnDestroy()
    {
        if( null != _runtimeMat )
            Destroy( _runtimeMat );
    }

    #endregion

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 레일을 따라 기준물을 생성한다.
    /// </summary>
    private void Spawn()
    {
        if( null == _rail || false == _rail.IsReady )
        {
            DebugExtensions.LogError( "[View_RailMarkerSpawner] 레일 미준비 → 배치 불가.", Color.red );
            return;
        }

        if( _interval < 0.5f )
            _interval = 0.5f;

        float total = _rail.TotalLength;
        PrepareMaterial();

        int count = 0;
        bool rightSide = true;
        for( float d = 0f; d < total; d += _interval )
        {
            Vector3 pos     = _rail.GetPositionByDistance( d );
            Vector3 tangent = _rail.GetTangentByDistance( d );
            Vector3 right   = Vector3.Cross( Vector3.up, tangent ).normalized;

            if( true == _alternateSides )
            {
                Vector3 side = rightSide ? right : -right;
                CreatePillar( pos + side * _lateralOffset );
                rightSide = !rightSide;
                ++count;
            }
            else
            {
                CreatePillar( pos + right * _lateralOffset );
                CreatePillar( pos - right * _lateralOffset );
                count += 2;
            }
        }

        DebugExtensions.Log( $"[View_RailMarkerSpawner] 기준물 {count}개 배치 (간격 {_interval}m, 길이 {total:F1}m)", Color.cyan );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 공용 기준물 머티리얼을 준비한다.
    /// </summary>
    private void PrepareMaterial()
    {
        Shader shader = Shader.Find( "Universal Render Pipeline/Lit" );
        if( null == shader )
            shader = Shader.Find( "Standard" );

        _runtimeMat = new Material( shader );
        if( _runtimeMat.HasProperty( "_BaseColor" ) )
            _runtimeMat.SetColor( "_BaseColor", _markerColor );
        if( _runtimeMat.HasProperty( "_Color" ) )
            _runtimeMat.SetColor( "_Color", _markerColor );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 지정 위치에 기둥 프리미티브를 생성한다.
    /// </summary>
    /// <param name="basePos">기둥 바닥 중심 위치.</param>
    private void CreatePillar( Vector3 basePos )
    {
        GameObject pillar = GameObject.CreatePrimitive( PrimitiveType.Cube );
        pillar.name = "SpeedMarker";
        pillar.transform.SetParent( transform, true );
        pillar.transform.position   = basePos + new Vector3( 0f, _pillarHeight * 0.5f, 0f );
        pillar.transform.localScale = new Vector3( _pillarWidth, _pillarHeight, _pillarWidth );

        MeshRenderer mr = pillar.GetComponent<MeshRenderer>();
        if( null != mr && null != _runtimeMat )
            mr.sharedMaterial = _runtimeMat;

        _spawned.Add( pillar );
    }

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//View_RailMarkerSpawner
