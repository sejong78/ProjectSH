using UnityEngine;

public static class VectorExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Vector2Int를 주어진 각도(degree)만큼 회전시킵니다.
	/// </summary>
	/// <param name="v">회전할 벡터</param>
	/// <param name="degree">회전 각도(도)</param>
	/// <returns>회전된 Vector2Int</returns>
	public static Vector2Int Rotate( this Vector2Int v, float degree )
	{
		float rad = degree * Mathf.Deg2Rad;
		float cos = Mathf.Cos(rad);
		float sin = Mathf.Sin(rad);

		int x = Mathf.RoundToInt(v.x * cos - v.y * sin);
		int y = Mathf.RoundToInt(v.x * sin + v.y * cos);

		return new Vector2Int( x, y );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//VectorExtensions
