//MIT License
//Copyright (c) 2020 Mohammed Iqubal Hussain
//Website : Polyandcode.com 

using System;
using UnityEngine;
using UnityEngine.UI;

namespace PolyAndCode.UI
{
    /// <summary>
    /// Entry for the recycling system. Extends Unity's inbuilt ScrollRect.
    /// </summary>
    public class RecyclableScrollRect : ScrollRect
    {
        [HideInInspector]
        public IRecyclableScrollRectDataSource DataSource = null;

        public bool IsGrid;
        //Prototype cell can either be a prefab or present as a child to the content(will automatically be disabled in runtime)
        public RectTransform PrototypeCell;
        //If true the intiziation happens at Start. Controller must assign the datasource in Awake.
        //Set to false if self init is not required and use public init API.
        public bool SelfInitialize = true;

		[SerializeField]
		private float _cellPadding = 0f; // 셀 사이의 간격(픽셀 단위)

		public enum DirectionType
        {
            Vertical,
            Horizontal
        }

        public DirectionType Direction;

        //Segments : coloums for vertical and rows for horizontal.
        public int Segments
        {
            set
            {
                _segments = Math.Max(value, 2);
            }
            get
            {
                return _segments;
            }
        }
        [SerializeField]
        private int _segments;

        private RecyclingSystem _recyclingSystem;
        private Vector2 _prevAnchoredPos;

        public Action OnInitialized = null;

        protected override void Start()
        {
            //defafult(built-in) in scroll rect can have both directions enabled, Recyclable scroll rect can be scrolled in only one direction.
            //setting default as vertical, Initialize() will set this again. 
            vertical = true;
            horizontal = false;

            if (!Application.isPlaying) return;

            if (SelfInitialize) Initialize();
        }

        /// <summary>
        /// Initialization when selfInitalize is true. Assumes that data source is set in controller's Awake.
        /// </summary>
        private void Initialize()
        {
            if( null == PrototypeCell )
			{
				return;
			}

			//Contruct the recycling system.
			if( Direction == DirectionType.Vertical)
            {
				if( null == _recyclingSystem )
					_recyclingSystem = new  VerticalRecyclingSystem();

                ( ( VerticalRecyclingSystem )_recyclingSystem ).InitData( PrototypeCell, viewport, content, DataSource, IsGrid, Segments );
            }
            else if (Direction == DirectionType.Horizontal)
            {
				if( null == _recyclingSystem )
					_recyclingSystem = new HorizontalRecyclingSystem();

				( ( HorizontalRecyclingSystem )_recyclingSystem ).InitData( PrototypeCell, viewport, content, DataSource, IsGrid, Segments );
            }
            _recyclingSystem.Padding = _cellPadding;

			vertical = Direction == DirectionType.Vertical;
            horizontal = Direction == DirectionType.Horizontal;

            _prevAnchoredPos = content.anchoredPosition;
            onValueChanged.RemoveListener(OnValueChangedListener);
            //Adding listener after pool creation to avoid any unwanted recycling behaviour.(rare scenerio)
            StartCoroutine(_recyclingSystem.InitCoroutine(() => 
            { 
                onValueChanged.AddListener( OnValueChangedListener );

                OnInitialized?.SafeExcute();
			} ));
        }

        /// <summary>
        /// public API for Initializing when datasource is not set in controller's Awake. Make sure selfInitalize is set to false. 
        /// </summary>
        public void Initialize(IRecyclableScrollRectDataSource dataSource)
        {
            DataSource = dataSource;
            Initialize();
        }

        /// <summary>
        /// Added as a listener to the OnValueChanged event of Scroll rect.
        /// Recycling entry point for recyling systems.
        /// </summary>
        /// <param name="direction">scroll direction</param>
        public void OnValueChangedListener(Vector2 normalizedPos)
        {
            Vector2 dir = content.anchoredPosition - _prevAnchoredPos;
            m_ContentStartPosition += _recyclingSystem.OnValueChangedListener(dir);
            _prevAnchoredPos = content.anchoredPosition;
        }

        /// <summary>
        ///Reloads the data. Call this if a new datasource is assigned.
        /// </summary>
        public void ReloadData()
        {
            ReloadData(DataSource);
        }

        /// <summary>
        /// Overloaded ReloadData with dataSource param
        ///Reloads the data. Call this if a new datasource is assigned.
        /// </summary>
        public void ReloadData(IRecyclableScrollRectDataSource dataSource)
        {
            if (_recyclingSystem != null)
            {
                StopMovement();
                onValueChanged.RemoveListener(OnValueChangedListener);
                _recyclingSystem.DataSource = dataSource;
                StartCoroutine(_recyclingSystem.InitCoroutine(() =>
                                                               onValueChanged.AddListener(OnValueChangedListener)
                                                              ));
                _prevAnchoredPos = content.anchoredPosition;
            }
        }

        public void UpdateData()
        {
            if( null == _recyclingSystem )
                return;

            _recyclingSystem.UpdateDatas();
		}

        public void UpdateBoundsArea()
        {
            UpdateBounds();
			UpdateData();
		}

        public void GetCellOneByOne( Action<ICell,int> onGet )
        {
			if( null == _recyclingSystem )
				return;

            _recyclingSystem.GetCellOneByOne( onGet );
		}

		/// <summary>
		/// Content의 anchoredPosition이 영역을 벗어나면 자동으로 영역 안으로 이동시킴
		/// </summary>
		public void ClampContentToBounds()
        {
            if( null == content )
                return;

			Vector2 contentSize = content.rect.size;
			Vector2 viewportSize = viewport.rect.size;

			Vector2 min = Vector2.zero;
			Vector2 max = Vector2.zero;

			// 가로 스크롤
			if( true == horizontal )
			{
				float diff = contentSize.x - viewportSize.x;
				min.x = diff < 0 ? 0 : -diff;
				max.x = 0;
			}
			// 세로 스크롤
			if( true == vertical )
			{
				float diff = contentSize.y - viewportSize.y;
				min.y = 0;
				max.y = diff < 0 ? 0 : diff;
			}

			Vector2 pos = content.anchoredPosition;
			Vector2 clamped = pos;

			clamped.x = Mathf.Clamp( clamped.x, min.x, max.x );
			clamped.y = Mathf.Clamp( clamped.y, min.y, max.y );

			if( clamped != pos )
				content.anchoredPosition = clamped;
		}

		/*
        #region Testing
        private void OnDrawGizmos()
        {
            if (_recyclableScrollRect is VerticalRecyclingSystem)
            {
                ((VerticalRecyclingSystem)_recyclableScrollRect).OnDrawGizmos();
            }

            if (_recyclableScrollRect is HorizontalRecyclingSystem)
            {
                ((HorizontalRecyclingSystem)_recyclableScrollRect).OnDrawGizmos();
            }

        }
        #endregion
        */
	}
}