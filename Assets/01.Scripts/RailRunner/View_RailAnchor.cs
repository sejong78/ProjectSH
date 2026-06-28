/**---------------------------------------------------------------------------------
 * @file View_RailAnchor.cs
 * @brief 레일에 종속되어 전진하는 가상 앵커. 곡률 연동 가/감속과 무한 루프 주행을 담당.
 *///-------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "정해진 라인"의 실체. View_RailPath를 따라 거리(m)를 누적 전진한다.
/// 속도는 곡률 연동(직선 가속 / 급곡선 감속) + 수동 구간 지정으로 산출하며 부드럽게 보간한다.
/// 앵커보다 L(m) 앞의 레일 위 점을 look-ahead 타겟으로 제공한다(같은 레일, second anchor).
/// </summary>
public class View_RailAnchor : MonoBehaviour
{
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    
    /// <summary> 앵커 주행 상태. </summary>
    public enum eAnchorState
    {
        Idle,       // 레일 미준비
        Running,    // 정상 주행
    }

    [System.Serializable]
    public struct SpeedZone
    {
        [Tooltip( "구간 시작 거리(m)" )]   public float StartDistance;
        [Tooltip( "구간 끝 거리(m)" )]     public float EndDistance;
        [Tooltip( "속도 배율 (>1 부스트, <1 슬로)" )] public float SpeedMultiplier;
    }

    [Header( "레일 참조" )]
    [SerializeField] private View_RailPath _rail = null;

    [Header( "속도 프로파일 (m/s)" )]
    [SerializeField] private float _maxSpeed = 40f;
    [SerializeField] private float _minSpeed = 16f;
    [Tooltip( "이 곡률(1/m) 이상이면 최저속까지 감속" )]
    [SerializeField] private float _curvatureForMinSpeed = 0.12f;
    [Tooltip( "목표 속도 추종 가속 응답성(클수록 민첩)" )]
    [SerializeField] private float _accelResponse = 1.5f;
    [Tooltip( "감속 시 곡률 선행 예측 거리(m)" )]
    [SerializeField] private float _curvatureLookAhead = 6f;

    [Header( "수동 속도 구간 (선택, 연출 강제)" )]
    [SerializeField] private List<SpeedZone> _manualZones = new List<SpeedZone>();

    [Header( "전방 주시 (Look-Ahead)" )]
    [Tooltip( "앵커 앞 주시 거리 L(m)" )]
    [SerializeField] private float _lookAheadDistance = 12f;
    [Tooltip( "속도 연동 추가 주시(빠를수록 더 멀리, m per (m/s))" )]
    [SerializeField] private float _lookAheadSpeedFactor = 0.25f;

    [Header( "디버그" )]
    [SerializeField] private bool _verboseLog = false;

    private eAnchorState _state         = eAnchorState.Idle;
    private float _distance             = 0f;    // 누적 주행 거리(m)
    private float _currentSpeed         = 0f;    // 현재 속도(m/s)
    private float _currentCurvature     = 0f;    // 현재 곡률(1/m)
    private float _currentSignedCurv    = 0f;    // 부호 곡률(뱅크용)
    private int   _lapCount             = 0;     // 루프 횟수

    /// <summary> 앵커 현재 월드 위치. </summary>
    public Vector3 CurrentPosition { get; private set; }
    /// <summary> 앵커 진행 방향(정규화). </summary>
    public Vector3 CurrentForward  { get; private set; }
    /// <summary> L만큼 앞의 레일 위 look 타겟 위치. </summary>
    public Vector3 LookAheadPosition { get; private set; }
    /// <summary> 현재 속도(m/s). </summary>
    public float CurrentSpeed { get { return _currentSpeed; } }
    /// <summary> 속도 정규화(0~1). FOV/셰이크 입력용. </summary>
    public float SpeedNormalized
    {
        get
        {
            float range = Mathf.Max( 0.001f, _maxSpeed - _minSpeed );
            return Mathf.Clamp01( ( _currentSpeed - _minSpeed ) / range );
        }
    }
    /// <summary> 현재 곡률(1/m). </summary>
    public float CurrentCurvature { get { return _currentCurvature; } }
    /// <summary> 부호 곡률(우회전 양수). 뱅크 roll 방향. </summary>
    public float CurrentSignedCurvature { get { return _currentSignedCurv; } }
    /// <summary> 현재 누적 거리(m). </summary>
    public float CurrentDistance { get { return _distance; } }
    /// <summary> 완료한 루프 횟수. </summary>
    public int LapCount { get { return _lapCount; } }

    /// <summary> 루프(끝→처음) 발생 시 통보. </summary>
    public event Action OnLapCompleted = null;

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
	#region MonoBehaviour

	private void Awake()
    {
        ValidateAndClamp();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    
    private void Start()
    {
        // 레일이 미리 지정된 경우(저작/단일 씬) 즉시 주행 시작.
        // 미지정이면 외부 로더의 BindRail() 호출을 대기(Idle).
        if( null == _rail )
        {
            _state = eAnchorState.Idle;
            DebugExtensions.Log( "[View_RailAnchor] 레일 미지정 → BindRail() 대기.", Color.gray );
            return;
        }

        TryBeginRun();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    
    private void Update()
    {
        if( eAnchorState.Running != _state )
            return;

        Advance( Time.deltaTime );
    }

    #endregion

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 외부(맵 로더)에서 런타임에 레일을 주입하고 주행을 시작한다.
    /// 부트스트랩 씬 + additive 맵 로드 흐름의 진입점.
    /// </summary>
    /// <param name="rail">로드된 맵의 레일.</param>
    public void BindRail( View_RailPath rail )
    {
        if( null == rail )
        {
            _state = eAnchorState.Idle;
            DebugExtensions.LogError( "[View_RailAnchor] BindRail 레일이 null → Idle.", Color.red );
            return;
        }

        _rail = rail;
        TryBeginRun();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 레일 준비를 확인하고 주행 상태로 진입한다. 거리/속도/곡률/루프 카운트를 초기화한다.
    /// </summary>
    private void TryBeginRun()
    {
        if( null == _rail )
            return;

        if( false == _rail.IsReady )
            _rail.Rebuild();

        if( false == _rail.IsReady )
        {
            _state = eAnchorState.Idle;
            DebugExtensions.LogError( "[View_RailAnchor] 레일 준비 실패 → Idle. 주행 불가.", Color.red );
            return;
        }

        _distance          = 0f;
        _currentSpeed      = _minSpeed;
        _currentCurvature  = 0f;
        _currentSignedCurv = 0f;
        _lapCount          = 0;
        _state             = eAnchorState.Running;
        SampleRail();
        DebugExtensions.Log( $"[View_RailAnchor] 주행 시작. 레일 길이 {_rail.TotalLength:F1}m", Color.cyan );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 직렬화 파라미터 무결성 검증 및 클램프.
    /// </summary>
    private void ValidateAndClamp()
    {
        if( _minSpeed < 0.1f )
            _minSpeed = 0.1f;
        if( _maxSpeed < _minSpeed )
            _maxSpeed = _minSpeed;
        if( _curvatureForMinSpeed < 0.001f )
            _curvatureForMinSpeed = 0.001f;
        if( _accelResponse < 0.01f )
            _accelResponse = 0.01f;
        if( _lookAheadDistance < 0f )
            _lookAheadDistance = 0f;
        if( _curvatureLookAhead < 0f )
            _curvatureLookAhead = 0f;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// dt만큼 곡률 연동 속도로 전진하고 루프를 처리한다.
    /// </summary>
    private void Advance( float dt )
    {
        float total = _rail.TotalLength;

        if( total < 1e-3f )
            return;

        // 선행 곡률 기반 목표 속도 산출
        float aheadDist        = _distance + _curvatureLookAhead;
        float aheadCurv        = _rail.GetCurvatureByDistance( aheadDist );
        float curvT            = Mathf.Clamp01( aheadCurv / _curvatureForMinSpeed );
        float targetSpeed      = Mathf.Lerp( _maxSpeed, _minSpeed, curvT );
        targetSpeed           *= GetManualMultiplier( _distance );

        // 부드러운 속도 보간(급변 금지)
        _currentSpeed = Mathf.Lerp( _currentSpeed, targetSpeed, 1f - Mathf.Exp( -_accelResponse * dt ) );

        float prevDistance = _distance;
        _distance += _currentSpeed * dt;

        // 루프 처리
        if( total <= _distance )
        {
            _distance = Mathf.Repeat( _distance, total );
            ++_lapCount;
            OnLapCompleted.SafeExcute();
            if( true == _verboseLog )
                DebugExtensions.Log( $"[View_RailAnchor] 루프 완료 #{_lapCount} (prev {prevDistance:F1} → wrap {_distance:F1})", Color.magenta );
        }

        SampleRail();
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 현재 거리에서 레일을 샘플링해 위치/방향/곡률/look-ahead를 갱신한다.
    /// </summary>
    private void SampleRail()
    {
        CurrentPosition   = _rail.GetPositionByDistance( _distance );
        CurrentForward    = _rail.GetTangentByDistance( _distance );
        _currentCurvature = _rail.GetCurvatureByDistance( _distance );
        _currentSignedCurv = _rail.GetSignedCurvatureByDistance( _distance );

        float lookDist    = _lookAheadDistance + ( _currentSpeed * _lookAheadSpeedFactor );
        LookAheadPosition = _rail.GetPositionByDistance( _distance + lookDist );

        // 앵커 트랜스폼도 동기화(시각 확인 + 추후 자식 배치용)
        transform.position = CurrentPosition;
        if( 1e-6f < CurrentForward.sqrMagnitude )
            transform.rotation = Quaternion.LookRotation( CurrentForward, Vector3.up );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 거리(m)가 수동 속도 구간에 속하면 배율을, 아니면 1을 반환한다.
    /// </summary>
    private float GetManualMultiplier( float distance )
    {
        for( int i = 0; i < _manualZones.Count; ++i )
        {
            SpeedZone z = _manualZones[ i ];
            if( z.StartDistance <= distance && distance <= z.EndDistance )
            {
                if( 0.01f < z.SpeedMultiplier )
                    return z.SpeedMultiplier;
            }
        }
        return 1f;
    }
    
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    
}//View_RailAnchor
