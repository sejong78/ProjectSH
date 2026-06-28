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

        int vertCount = lines * 4;
        Vector3[] verts = new Vector3[ vertCount ];
        int[] indices   = new int[ vertCount ];

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

        DebugExtensions.Log( $"[View_GridFloor] 격자 생성: {_worldSize}m / {_cellSize}m → 라인 {lines * 2}개", Color.cyan );
    }
   
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    
}//View_GridFloor
