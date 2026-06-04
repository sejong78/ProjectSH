//MIT License
//Copyright (c) 2020 Mohammed Iqubal Hussain
//Website : Polyandcode.com 


using System.Collections;
using UnityEngine;
namespace PolyAndCode.UI
{
    /// <summary>
    /// Absract Class for creating a Recycling system.
    /// </summary>
    public abstract class RecyclingSystem
    {
        public IRecyclableScrollRectDataSource DataSource;

        protected RectTransform Viewport, Content;
        protected RectTransform PrototypeCell;
        protected bool IsGrid;

        protected float MinPoolCoverage = 1.5f; // The recyclable pool must cover (viewPort * _poolCoverage) area.
        protected int MinPoolSize = 10; // Cell pool must have a min size
        protected float RecyclingThreshold = .2f; //Threshold for recycling above and below viewport
		
        protected float CellPadding = 0f; // 셀 사이의 간격(픽셀 단위)

		public float Padding
		{
			get => CellPadding;
			set => CellPadding = value;
		}
		
        public abstract IEnumerator InitCoroutine(System.Action onInitialized = null);

        public abstract Vector2 OnValueChangedListener(Vector2 direction);

        /// <summary>
        /// 리스트에는 변화가 없이 데이터만 업데이트 한다.
        /// </summary>
        public abstract void UpdateDatas();

        public abstract void GetCellOneByOne( System.Action<ICell,int> onGet );
	}
}