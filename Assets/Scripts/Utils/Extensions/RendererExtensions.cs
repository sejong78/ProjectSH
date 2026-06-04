/**---------------------------------------------------------------------------------
 * @file RendererExtensions.cs
 * @date 2022/8/11
 * @author sejong
 * @brief Renderer 확장함수
 *///-------------------------------------------------------------------------------
using UnityEngine;

/// <summary>
/// @class RendererExtensions
/// @date 2022/8/11
/// @author sejong
/// @brief Renderer 확장함수
/// </summary>
public static class RendererExtensions
{
    //@@-------------------------------------------------------------------------------------------------------------------------
    //@@-------------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// 특정 카메라에서 보이는지 검사합니다.
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
		return GeometryUtility.TestPlanesAABB( planes, renderer.bounds );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}// RendererExtensions