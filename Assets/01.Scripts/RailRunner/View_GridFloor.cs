/**---------------------------------------------------------------------------------
 * @file View_GridFloor.cs
 * @brief 월드공간 격자 바닥 메시를 절차적으로 생성. 카메라 이동 시 격자선이 흘러 속도 착시를 만든다.
 *///-------------------------------------------------------------------------------
using UnityEngine;

/// <summary>
/// 유한 박스 영역에 월드공간 격자 라인 메시를 깐다(정적).
/// 카메라가 그 위를 빠르게 이동하면 격자선이 밀려나며 속도감을 인지시킨다(원작 체커보드 인사이트).
/// </summary>
[RequireComponent( typeof( MeshFilter ) )]
[RequireComponent( typeof( MeshRenderer ) )]
public class View_GridFloor : MonoBehaviour
{
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    
    [Header( "격자 영역 (정사각 박스, m)" )]
    [SerializeField] private float _worldSize = 80f;
    [Header( "격자 칸 크기 (m)" )]
    [SerializeField] private float _cellSize  = 2f;
    [Header( "바닥 높이 (y)" )]
    [SerializeField] private float _height = 0f;
    [Header( "격자선 색상" )]
    [SerializeField] private Color _lineColor = new Color( 0.2f, 0.9f, 1f, 1f );

    [Header( "지형 높이 반영 (빌더에서 베이크)" )]
    [Tooltip( "true면 _heightSamples를 정점 높이로 사용. false면 평면(_height)." )]
    [SerializeField] private bool _useHeightMap = false;
    [Tooltip( "격자 교차점 로컬 높이 샘플(lines×lines, 인덱스 iz*lines+ix). 빌더가 채운다." )]
    [SerializeField] private float[] _heightSamples = null;

    [Header( "에디터 기즈모 (플레이 없이 씬뷰 표시)" )]
    [SerializeField] private bool _drawGizmo = true;

    private MeshFilter   _meshFilter   = null;
    private MeshRenderer _meshRenderer = null;
    private Material     _runtimeMat   = null;

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------
	
    #region MonoBehaviour

	private void Awake()
    {
        _meshFilter   = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        BuildGrid();
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
    /// 격자 라인 메시와 언릿 머티리얼을 생성해 렌더러에 적용한다.
    /// </summary>
    private void BuildGrid()
    {
        if( _cellSize < 0.1f )
            _cellSize = 0.1f;
        if( _worldSize < _cellSize )
            _worldSize = _cellSize;

        float half  = _worldSize * 0.5f;
        int   lines = Mathf.FloorToInt( _worldSize / _cellSize ) + 1;

        // 높이맵 사용 가능 여부: 토글 ON + 샘플 길이가 격자 교차점 수와 일치해야 한다.
        bool useHeight = _useHeightMap
                      && null != _heightSamples
                      && _heightSamples.Length == ( lines * lines );

        if( true == _useHeightMap && false == useHeight )
        {
            int expected = lines * lines;
            int actual   = ( null == _heightSamples ) ? 0 : _heightSamples.Length;
            DebugExtensions.LogError( $"[View_GridFloor] 높이맵 길이 불일치(기대 {expected}, 실제 {actual}) → 평면 폴백. 격자 크기 변경 후 재베이크 필요.", Color.red );
        }

        Vector3[] verts;
        int[]     indices;

        if( true == useHeight )
            BuildTessellated( half, lines, out verts, out indices );
        else
            BuildFlat( half, lines, out verts, out indices );

        int vertCount = verts.Length;

        Mesh mesh = new Mesh();
        mesh.name = "GridFloorMesh";
        mesh.indexFormat = ( 65000 < vertCount )
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.vertices = verts;
        mesh.SetIndices( indices, MeshTopology.Lines, 0 );
        mesh.RecalculateBounds();

        _meshFilter.sharedMesh = mesh;

        Shader shader = Shader.Find( "Universal Render Pipeline/Unlit" );
        if( null == shader )
            shader = Shader.Find( "Unlit/Color" );

        _runtimeMat = new Material( shader );
        if( _runtimeMat.HasProperty( "_BaseColor" ) )
            _runtimeMat.SetColor( "_BaseColor", _lineColor );
        if( _runtimeMat.HasProperty( "_Color" ) )
            _runtimeMat.SetColor( "_Color", _lineColor );
        _meshRenderer.sharedMaterial = _runtimeMat;

        DebugExtensions.Log( $"[View_GridFloor] 격자 생성: {_worldSize}m / {_cellSize}m → 라인 {lines * 2}개 (높이맵 {( useHeight ? "ON" : "OFF" )})", Color.cyan );
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 평면 격자(전폭 라인)를 생성한다. 라인당 정점 2개(저비용).
    /// </summary>
    private void BuildFlat( float half, int lines, out Vector3[] verts, out int[] indices )
    {
        int vertCount = lines * 4;
        verts   = new Vector3[ vertCount ];
        indices = new int[ vertCount ];

        int v = 0;
        for( int i = 0; i < lines; ++i )
        {
            float p = -half + ( i * _cellSize );

            // X축 평행선 (z 고정)
            verts[ v ]     = new Vector3( -half, _height, p );
            verts[ v + 1 ] = new Vector3(  half, _height, p );
            // Z축 평행선 (x 고정)
            verts[ v + 2 ] = new Vector3( p, _height, -half );
            verts[ v + 3 ] = new Vector3( p, _height,  half );

            indices[ v ]     = v;
            indices[ v + 1 ] = v + 1;
            indices[ v + 2 ] = v + 2;
            indices[ v + 3 ] = v + 3;
            v += 4;
        }
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 높이맵을 따라 칸 단위로 분할한 격자를 생성한다. 각 교차점 정점 Y는 _heightSamples를 사용한다.
    /// </summary>
    private void BuildTessellated( float half, int lines, out Vector3[] verts, out int[] indices )
    {
        // 세그먼트 수: X평행 + Z평행 각각 lines * (lines-1). 정점/인덱스는 그 2배.
        int segCount  = 2 * lines * ( lines - 1 );
        int vertCount = segCount * 2;
        verts   = new Vector3[ vertCount ];
        indices = new int[ vertCount ];

        int v = 0;
        for( int iz = 0; iz < lines; ++iz )
        {
            float z = -half + ( iz * _cellSize );

            // X축 평행선 (z 고정): ix → ix+1 칸 분할
            for( int ix = 0; ix < lines - 1; ++ix )
            {
                float x0 = -half + ( ix * _cellSize );
                float x1 = -half + ( ( ix + 1 ) * _cellSize );
                verts[ v ]     = new Vector3( x0, SampleY( ix,     iz, lines ), z );
                verts[ v + 1 ] = new Vector3( x1, SampleY( ix + 1, iz, lines ), z );
                indices[ v ]     = v;
                indices[ v + 1 ] = v + 1;
                v += 2;
            }
        }

        for( int ix = 0; ix < lines; ++ix )
        {
            float x = -half + ( ix * _cellSize );

            // Z축 평행선 (x 고정): iz → iz+1 칸 분할
            for( int iz = 0; iz < lines - 1; ++iz )
            {
                float z0 = -half + ( iz * _cellSize );
                float z1 = -half + ( ( iz + 1 ) * _cellSize );
                verts[ v ]     = new Vector3( x, SampleY( ix, iz,     lines ), z0 );
                verts[ v + 1 ] = new Vector3( x, SampleY( ix, iz + 1, lines ), z1 );
                indices[ v ]     = v;
                indices[ v + 1 ] = v + 1;
                v += 2;
            }
        }
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// 교차점 (ix, iz)의 로컬 높이 샘플을 반환한다.
    /// </summary>
    private float SampleY( int ix, int iz, int lines )
    {
        int idx = ( iz * lines ) + ix;
        if( 0 <= idx && idx < _heightSamples.Length )
            return _heightSamples[ idx ];
        return _height;
    }

    //@@-------------------------------------------------------------------------------------------------------------------------
    #region Gizmo

    /// <summary>
    /// 플레이하지 않아도 씬뷰에 격자를 미리 그린다(런타임 메시와 동일한 형상·높이맵).
    /// 실제 렌더 메시가 아니라 에디터 가이드.
    /// </summary>
    private void OnDrawGizmos()
    {
        if( false == _drawGizmo )
            return;

        float cell  = ( _cellSize < 0.1f ) ? 0.1f : _cellSize;
        float ws    = ( _worldSize < cell ) ? cell : _worldSize;
        float half  = ws * 0.5f;
        int   lines = Mathf.FloorToInt( ws / cell ) + 1;

        bool useHeight = _useHeightMap
                      && null != _heightSamples
                      && _heightSamples.Length == ( lines * lines );

        Matrix4x4 prevMatrix = Gizmos.matrix;
        Color     prevColor  = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color  = _lineColor;

        if( false == useHeight )
        {
            for( int i = 0; i < lines; ++i )
            {
                float p = -half + ( i * cell );
                Gizmos.DrawLine( new Vector3( -half, _height, p ), new Vector3( half, _height, p ) );
                Gizmos.DrawLine( new Vector3( p, _height, -half ), new Vector3( p, _height, half ) );
            }
        }
        else
        {
            // 높이맵: 칸 단위 분할(BuildTessellated와 동일 형상)
            for( int iz = 0; iz < lines; ++iz )
            {
                float z = -half + ( iz * cell );
                for( int ix = 0; ix < lines - 1; ++ix )
                {
                    float x0 = -half + ( ix * cell );
                    float x1 = -half + ( ( ix + 1 ) * cell );
                    Gizmos.DrawLine( new Vector3( x0, SampleY( ix,     iz, lines ), z ),
                                     new Vector3( x1, SampleY( ix + 1, iz, lines ), z ) );
                }
            }
            for( int ix = 0; ix < lines; ++ix )
            {
                float x = -half + ( ix * cell );
                for( int iz = 0; iz < lines - 1; ++iz )
                {
                    float z0 = -half + ( iz * cell );
                    float z1 = -half + ( ( iz + 1 ) * cell );
                    Gizmos.DrawLine( new Vector3( x, SampleY( ix, iz,     lines ), z0 ),
                                     new Vector3( x, SampleY( ix, iz + 1, lines ), z1 ) );
                }
            }
        }

        Gizmos.matrix = prevMatrix;
        Gizmos.color  = prevColor;
    }

    #endregion
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------

}//View_GridFloor
