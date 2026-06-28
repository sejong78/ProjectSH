/**---------------------------------------------------------------------------------
 * @file View_RailPath.cs
 * @brief 닫힌 Catmull-Rom 스플라인 레일. 거리 기반 위치/접선/곡률을 제공한다.
 *///-------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제어점으로 구성된 닫힌 곡선 레일.
/// 호 길이(arc-length) LUT를 만들어 거리(m) 기반 위치/접선/곡률 샘플링을 제공한다.
/// Step1 곡선 경로 "월드 접기"의 실체.
/// </summary>
public class View_RailPath : MonoBehaviour
{
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    
    [Header( "제어점 (자식 Transform, 최소 4개)" )]
    [SerializeField] private List<Transform> _controlPoints = new List<Transform>();

    [Header( "호 길이 LUT 해상도 (세그먼트당 샘플 수)" )]
    [SerializeField] private int _samplesPerSegment = 32;

    [Header( "기즈모 표시" )]
    [SerializeField] private bool _drawGizmo = true;

    private readonly List<Vector3> _points     = new List<Vector3>();  // 캐싱된 제어점 월드 위치
    private readonly List<float>   _arcLengths = new List<float>();    // 누적 호 길이 LUT
    private readonly List<float>   _arcParams  = new List<float>();    // LUT 샘플에 대응하는 전역 파라미터 u
    private float _totalLength = 0f;
    private bool  _isReady     = false;

    /// <summary> 레일 전체 길이(m). </summary>
    public float TotalLength { get { return _totalLength; } }
    /// <summary> LUT 빌드 완료 여부. </summary>
    public bool  IsReady     { get { return _isReady; } }
    /// <summary> 닫힌 곡선 세그먼트 수(= 제어점 수). </summary>
    public int   SegmentCount { get { return _points.Count; } }

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
	#region MonoBehaviour

	private void Awake()
    {
        Rebuild();
    }

    #endregion

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 제어점을 캐싱하고 호 길이 LUT를 재구성한다. 무결성 검증 포함.
    /// </summary>
    public void Rebuild()
    {
        _points.Clear();
        for( int i = 0; i < _controlPoints.Count; ++i )
        {
            if( null == _controlPoints[ i ] )
                continue;
            _points.Add( _controlPoints[ i ].position );
        }

        if( _points.Count < 4 )
        {
            _isReady = false;
            DebugExtensions.LogError( $"[View_RailPath] 제어점이 {_points.Count}개. 닫힌 Catmull-Rom은 최소 4개 필요.", Color.red );
            return;
        }

        if( _samplesPerSegment < 2 )
            _samplesPerSegment = 2;

        BuildArcLengthTable();
        _isReady = true;
        DebugExtensions.Log( $"[View_RailPath] LUT 빌드 완료. 제어점 {_points.Count}개, 전체 길이 {_totalLength:F1}m", Color.cyan );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 닫힌 곡선을 균일 샘플링하여 누적 호 길이 LUT를 만든다.
    /// </summary>
    private void BuildArcLengthTable()
    {
        _arcLengths.Clear();
        _arcParams.Clear();

        int segCount    = _points.Count;
        int totalSample = segCount * _samplesPerSegment;
        Vector3 prev    = EvaluateByParam( 0f );
        float accum     = 0f;

        _arcLengths.Add( 0f );
        _arcParams.Add( 0f );

        for( int i = 1; i <= totalSample; ++i )
        {
            float u    = ( (float)i / totalSample ) * segCount;
            Vector3 pt = EvaluateByParam( u );
            accum     += Vector3.Distance( prev, pt );
            _arcLengths.Add( accum );
            _arcParams.Add( u );
            prev = pt;
        }

        _totalLength = accum;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 전역 파라미터 u(0 ~ SegmentCount)에 해당하는 곡선 위치를 반환한다.
    /// </summary>
    /// <param name="u">전역 파라미터. 정수부 = 세그먼트, 소수부 = 보간 t.</param>
    public Vector3 EvaluateByParam( float u )
    {
        int segCount = _points.Count;
        if( segCount < 4 )
            return transform.position;

        u = Mathf.Repeat( u, segCount );
        int seg = Mathf.FloorToInt( u );
        float t = u - seg;

        Vector3 p0 = _points[ WrapIndex( seg - 1 ) ];
        Vector3 p1 = _points[ WrapIndex( seg ) ];
        Vector3 p2 = _points[ WrapIndex( seg + 1 ) ];
        Vector3 p3 = _points[ WrapIndex( seg + 2 ) ];

        return CatmullRom( p0, p1, p2, p3, t );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 거리(m)를 닫힌 곡선 길이로 래핑하여 곡선 위치를 반환한다.
    /// </summary>
    /// <param name="distance">레일 시작점 기준 누적 거리(m).</param>
    public Vector3 GetPositionByDistance( float distance )
    {
        if( false == _isReady )
            return transform.position;
        return EvaluateByParam( ParamFromDistance( distance ) );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 거리(m) 지점의 진행 방향(접선, 정규화)을 반환한다.
    /// </summary>
    public Vector3 GetTangentByDistance( float distance )
    {
        if( false == _isReady )
            return transform.forward;

        float ds   = 0.5f;
        Vector3 a  = GetPositionByDistance( distance );
        Vector3 b  = GetPositionByDistance( distance + ds );
        Vector3 dir = b - a;
        if( dir.sqrMagnitude < 1e-6f )
            return transform.forward;
        return dir.normalized;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 거리(m) 지점의 곡률(1/m 근사)을 반환한다. 접선 변화량 / 호 길이로 계산.
    /// </summary>
    public float GetCurvatureByDistance( float distance )
    {
        if( false == _isReady )
            return 0f;

        float ds    = 1.0f;
        Vector3 t0  = GetTangentByDistance( distance - ds );
        Vector3 t1  = GetTangentByDistance( distance + ds );
        float angle = Vector3.Angle( t0, t1 ) * Mathf.Deg2Rad;
        return angle / ( 2f * ds );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 거리(m) 지점에서 좌/우 부호가 있는 곡률을 반환한다. 양수 = 우회전, 음수 = 좌회전.
    /// 뱅크(roll) 연출 방향 결정용.
    /// </summary>
    public float GetSignedCurvatureByDistance( float distance )
    {
        if( false == _isReady )
            return 0f;

        float ds   = 1.0f;
        Vector3 t0 = GetTangentByDistance( distance - ds );
        Vector3 t1 = GetTangentByDistance( distance + ds );
        // 수평면 기준 좌우 회전 부호 (y축 외적)
        float cross = Vector3.Cross( t0, t1 ).y;
        float angle = Vector3.Angle( t0, t1 ) * Mathf.Deg2Rad;
        float mag   = angle / ( 2f * ds );
        return ( cross < 0f ) ? mag : -mag;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 누적 거리(m)를 전역 파라미터 u로 변환한다. LUT 이분 탐색.
    /// </summary>
    private float ParamFromDistance( float distance )
    {
        distance = Mathf.Repeat( distance, _totalLength );

        int lo = 0;
        int hi = _arcLengths.Count - 1;
        while( lo < hi )
        {
            int mid = ( lo + hi ) / 2;
            if( _arcLengths[ mid ] < distance )
                lo = mid + 1;
            else
                hi = mid;
        }

        int idx = Mathf.Max( 1, lo );
        float segLen = _arcLengths[ idx ] - _arcLengths[ idx - 1 ];
        float frac   = ( segLen < 1e-6f ) ? 0f : ( distance - _arcLengths[ idx - 1 ] ) / segLen;
        return Mathf.Lerp( _arcParams[ idx - 1 ], _arcParams[ idx ], frac );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 닫힌 곡선 인덱스를 래핑한다.
    /// </summary>
    private int WrapIndex( int index )
    {
        int n = _points.Count;
        return ( ( index % n ) + n ) % n;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Catmull-Rom 보간점을 계산한다.
    /// </summary>
    private static Vector3 CatmullRom( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t )
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ( ( 2f * p1 )
                      + ( -p0 + p2 ) * t
                      + ( 2f * p0 - 5f * p1 + 4f * p2 - p3 ) * t2
                      + ( -p0 + 3f * p1 - 3f * p2 + p3 ) * t3 );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    #region Gizmo

    private void OnDrawGizmos()
    {
        if( false == _drawGizmo )
            return;

        List<Vector3> pts = new List<Vector3>();
        for( int i = 0; i < _controlPoints.Count; ++i )
        {
            if( null != _controlPoints[ i ] )
                pts.Add( _controlPoints[ i ].position );
        }

        if( pts.Count < 4 )
            return;

        Gizmos.color = Color.green;
        int segCount = pts.Count;
        int steps    = segCount * 24;
        Vector3 prev = CatmullRomClosed( pts, 0f );
        for( int i = 1; i <= steps; ++i )
        {
            float u    = ( (float)i / steps ) * segCount;
            Vector3 cur = CatmullRomClosed( pts, u );
            Gizmos.DrawLine( prev, cur );
            prev = cur;
        }

        Gizmos.color = Color.yellow;
        for( int i = 0; i < pts.Count; ++i )
            Gizmos.DrawWireSphere( pts[ i ], 0.6f );
    }

    private static Vector3 CatmullRomClosed( List<Vector3> pts, float u )
    {
        int n   = pts.Count;
        u       = Mathf.Repeat( u, n );
        int seg = Mathf.FloorToInt( u );
        float t = u - seg;
        Vector3 p0 = pts[ ( ( seg - 1 ) % n + n ) % n ];
        Vector3 p1 = pts[ ( ( seg ) % n + n ) % n ];
        Vector3 p2 = pts[ ( ( seg + 1 ) % n + n ) % n ];
        Vector3 p3 = pts[ ( ( seg + 2 ) % n + n ) % n ];
        return CatmullRom( p0, p1, p2, p3, t );
    }

    #endregion
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    
}// View_RailPath
