/**---------------------------------------------------------------------------------
 * @file View_FollowCamera.cs
 * @brief 앵커를 스프링/댐핑으로 추종하는 백뷰 카메라. FOV/뱅크/셰이크/영향 오프셋 포함.
 *///-------------------------------------------------------------------------------
using UnityEngine;

/// <summary>
/// 레일에 강결합되지 않고 View_RailAnchor를 스프링으로 부드럽게 추종한다.
/// look-ahead 지점을 주시하며, 속도 연동 FOV/셰이크, 곡선 뱅크(roll), 외부 영향 오프셋(stub)을 적용한다.
/// 영향 입력은 Step1에서 0이며 AddInfluence 훅만 마련한다(후속 스텝에서 캐릭터 상태 연결).
/// </summary>
[RequireComponent( typeof( Camera ) )]
public class View_FollowCamera : MonoBehaviour
{
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    
    [Header( "추종 대상" )]
    [SerializeField] private View_RailAnchor _anchor = null;

    [Header( "오프셋 (앵커 로컬 기준)" )]
    [Tooltip( "백뷰 후방 거리/높이. (x=좌우, y=높이, z=전후 / -z=뒤)" )]
    [SerializeField] private Vector3 _localOffset = new Vector3( 0f, 2.2f, -6f );

    [Header( "스프링 추종" )]
    [Tooltip( "위치 추종 응답성(클수록 단단)" )]
    [SerializeField] private float _positionResponse = 6f;
    [Tooltip( "회전 추종 응답성" )]
    [SerializeField] private float _rotationResponse = 8f;

    [Header( "동적 FOV" )]
    [SerializeField] private float _baseFov  = 60f;
    [SerializeField] private float _boostFov = 78f;
    [SerializeField] private float _fovResponse = 4f;

    [Header( "곡선 뱅크 (roll)" )]
    [Tooltip( "최대 roll 각도(도)" )]
    [SerializeField] private float _maxBankAngle = 12f;
    [Tooltip( "부호 곡률 → roll 환산 배율" )]
    [SerializeField] private float _bankPerCurvature = 80f;
    [SerializeField] private float _bankResponse = 5f;

    [Header( "속도 셰이크 (속도↑일수록 거침)" )]
    [Tooltip( "정지~최고속 진폭 범위(m)" )]
    [SerializeField] private float _shakeAmplitudeMax = 0.18f;
    [Tooltip( "정지~최고속 빈도 범위(Hz)" )]
    [SerializeField] private float _shakeFrequencyMin = 2f;
    [SerializeField] private float _shakeFrequencyMax = 14f;
    [Tooltip( "셰이크 최소 시작 속도 비율(이하 잔잔)" )]
    [SerializeField] private float _shakeFloor = 0.05f;

    [Header( "영향 오프셋 (Influence / Step1 stub)" )]
    [Tooltip( "영향 오프셋 복귀 응답성" )]
    [SerializeField] private float _influenceReturnResponse = 4f;

    private Camera  _camera          = null;
    private Vector3 _currentPosition = Vector3.zero;   // 스프링 적용된 카메라 기준 위치
    private float   _currentRoll     = 0f;             // 현재 뱅크 roll
    private float   _currentFov      = 60f;
    private Vector3 _influenceOffset = Vector3.zero;   // 외부 영향 누적 오프셋(stub)
    private float   _shakeSeedX      = 0.137f;
    private float   _shakeSeedY      = 0.911f;
    private bool    _isReady         = false;

    /// <summary> 현재 카메라-앵커 거리(m). 강결합 아님 검증용. </summary>
    public float DistanceToAnchor
    {
        get
        {
            if( null == _anchor )
                return 0f;
            return Vector3.Distance( transform.position, _anchor.CurrentPosition );
        }
    }

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
	
    #region MonoBehaviour

	private void Awake()
    {
        _camera     = GetComponent<Camera>();
        _currentFov = _baseFov;
        if( null != _camera )
            _camera.fieldOfView = _baseFov;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        if( null == _anchor )
        {
            DebugExtensions.LogError( "[View_FollowCamera] 앵커 미할당 → 추종 불가.", Color.red );
            return;
        }

        // 첫 프레임 스냅(스프링 튐 방지)
        _currentPosition = _anchor.CurrentPosition + _anchor.transform.TransformVector( _localOffset );
        transform.position = _currentPosition;
        transform.rotation = Quaternion.LookRotation( _anchor.LookAheadPosition - _currentPosition, Vector3.up );
        _isReady = true;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    
    private void LateUpdate()
    {
        if( false == _isReady )
            return;

        float dt = Time.deltaTime;
        FollowAnchor( dt );
        ApplyDynamicFov( dt );
    }

    #endregion

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 앵커 기준 목표 위치/회전을 스프링으로 추종하고 뱅크/셰이크/영향을 합성한다.
    /// </summary>
    private void FollowAnchor( float dt )
    {
        // 목표 위치 = 앵커 로컬 오프셋 + 영향 오프셋
        Vector3 anchorRot = _anchor.transform.TransformVector( _localOffset );
        Vector3 targetPos = _anchor.CurrentPosition + anchorRot + _influenceOffset;

        float posLerp = 1f - Mathf.Exp( -_positionResponse * dt );
        _currentPosition = Vector3.Lerp( _currentPosition, targetPos, posLerp );

        // 영향 오프셋 스프링 복귀(stub: 입력 0이므로 항상 0으로 수렴)
        float infLerp = 1f - Mathf.Exp( -_influenceReturnResponse * dt );
        _influenceOffset = Vector3.Lerp( _influenceOffset, Vector3.zero, infLerp );

        // 속도 셰이크 합성
        Vector3 shake = ComputeShake();

        // 목표 회전: look-ahead 주시
        Vector3 lookDir = _anchor.LookAheadPosition - _currentPosition;
        Quaternion targetRot = ( lookDir.sqrMagnitude > 1e-6f )
            ? Quaternion.LookRotation( lookDir, Vector3.up )
            : transform.rotation;

        float rotLerp = 1f - Mathf.Exp( -_rotationResponse * dt );
        Quaternion baseRot = Quaternion.Slerp( transform.rotation, targetRot, rotLerp );

        // 곡선 뱅크(roll) 추종
        float targetRoll = Mathf.Clamp( _anchor.CurrentSignedCurvature * _bankPerCurvature, -_maxBankAngle, _maxBankAngle );
        float rollLerp   = 1f - Mathf.Exp( -_bankResponse * dt );
        _currentRoll     = Mathf.Lerp( _currentRoll, targetRoll, rollLerp );

        transform.position = _currentPosition + ( baseRot * shake );
        transform.rotation = baseRot * Quaternion.Euler( 0f, 0f, _currentRoll );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 속도 정규화에 비례한 Perlin 셰이크 오프셋(로컬)을 계산한다. 속도↑ → 진폭·빈도 동시 증가.
    /// </summary>
    private Vector3 ComputeShake()
    {
        float speedN = _anchor.SpeedNormalized;
        if( speedN <= _shakeFloor )
            return Vector3.zero;

        float amp  = _shakeAmplitudeMax * speedN;
        float freq = Mathf.Lerp( _shakeFrequencyMin, _shakeFrequencyMax, speedN );
        float t    = Time.time * freq;

        float x = ( Mathf.PerlinNoise( _shakeSeedX, t ) - 0.5f ) * 2f;
        float y = ( Mathf.PerlinNoise( _shakeSeedY, t ) - 0.5f ) * 2f;
        return new Vector3( x * amp, y * amp, 0f );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 속도 연동 FOV를 부드럽게 적용한다(가속 시 넓어짐).
    /// </summary>
    private void ApplyDynamicFov( float dt )
    {
        if( null == _camera )
            return;

        float targetFov = Mathf.Lerp( _baseFov, _boostFov, _anchor.SpeedNormalized );
        float fovLerp   = 1f - Mathf.Exp( -_fovResponse * dt );
        _currentFov     = Mathf.Lerp( _currentFov, targetFov, fovLerp );
        _camera.fieldOfView = _currentFov;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 외부 요소가 카메라를 앵커에서 잠시 이탈시키는 영향 오프셋을 누적한다(스프링이 복귀).
    /// Step1은 호출자 없음(stub). 후속 스텝에서 캐릭터 좌우 이동/충돌/바닥 달리기가 호출.
    /// </summary>
    /// <param name="worldOffset">월드 공간 영향 오프셋(m).</param>
    public void AddInfluence( Vector3 worldOffset )
    {
        _influenceOffset += worldOffset;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 이벤트(공격/피격 등) 셰이크 진입점(stub). Step1은 훅만.
    /// </summary>
    /// <param name="impulse">충격 진폭.</param>
    public void TriggerEventShake( float impulse )
    {
        // Step1: 훅만. 후속 스텝에서 일시적 셰이크 가중치로 연결.
    }

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//View_FollowCamera
