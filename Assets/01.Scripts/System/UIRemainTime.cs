/**---------------------------------------------------------------------------------
 * @file UIRemainTime.cs
 * @date 2026/2/1
 * @author sejong
 * @brief 남은 시간을 표시하는 UI
 *///-------------------------------------------------------------------------------
using System;
using TMPro;
using UnityEngine;

/// <summary>
/// @class UIRemainTime
/// @date 2026/2/1
/// @author sejong
/// @brief 남은 시간을 표시하는 UI
/// </summary>
public class UIRemainTime : MonoBehaviour
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	[SerializeField]
	private TMP_Text _remainTime = null;

	[SerializeField]
	private string _format = @"hh\:mm\:ss";

	private DateTime	_finishTime = DateTime.MinValue;
	private Action		_onFinished = null;

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	private void Awake()
	{
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	public void Set( DateTime finishTime, Action onFinished = null )
	{
		_finishTime = finishTime;
		_onFinished = onFinished;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	private void Update()
	{
		if( false == gameObject.activeInHierarchy || false == gameObject.activeSelf )
			return;

		if( DateTime.MinValue == _finishTime )
			return;

		// 남은 시간
		TimeSpan remaining = _finishTime - GameTime.UTCNow;

		if( remaining <= TimeSpan.Zero )
		{
			// 콜백이 있으면 한 번만 호출
			_onFinished.SafeExcute();
			_onFinished = null;

			return;
		}

		_remainTime.SetTextWithFormat( remaining.ToString( _format ) );

	}


	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//UIRemainTime
