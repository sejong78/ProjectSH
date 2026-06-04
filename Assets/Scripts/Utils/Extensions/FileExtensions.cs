/**---------------------------------------------------------------------------------
 * @file FileExtensions.cs
 * @date 2022/8/11
 * @author sejong
 * @brief File 및 Folder 관리
 *///-------------------------------------------------------------------------------
using System.IO;

/// <summary>
/// @class FileExtensions
/// @date 2022/8/11
/// @author sejong
/// @brief File 및 Folder 관리
/// </summary>
public static class FileExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 폴더를 복사합니다.
	/// 동일한 파일이나 폴더가 있을 경우 덮어씁니다.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	public static void CopyDirectory( string from, string to )
	{
		CopyDirectory( from, to, true );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 폴더를 복사합니다.
	/// 동일한 파일이나 폴더가 있을 경우 덮어쓰기 여부를 선택할 수 있습니다.
	/// </summary>
	/// <param name="from">복사할 원본 폴더</param>
	/// <param name="to">복사 대상 폴더</param>
	/// <param name="overwrite">true 일 경우 덮어쓰기 처리</param>
	public static void CopyDirectory( string from, string to, bool overwrite )
	{
		if( false == Directory.Exists( from ) || FileAttributes.Hidden == ( FileAttributes.Hidden & File.GetAttributes( from ) ) )
			return;

		// 복사할 디렉토리가 없으면 만든다.
		if( false == Directory.Exists( to ) )
		{
			Directory.CreateDirectory( to );
		}

		string[] files = Directory.GetFiles( from );
		string[] folders = Directory.GetDirectories( from );

		string name = "";
		string dest = "";

		// 감춰진 파일, 폴더는 무시
		foreach( string file in files )
		{
			if( FileAttributes.Hidden == ( FileAttributes.Hidden & File.GetAttributes( file ) ) )
			{
				continue;
			}

			name = Path.GetFileName( file );
			dest = Path.Combine( to, name );
			File.Copy( file, dest, overwrite );
		}

		foreach( string folder in folders )
		{
			if( FileAttributes.Hidden == ( FileAttributes.Hidden & File.GetAttributes( folder ) ) )
			{
				continue;
			}

			name = Path.GetFileName(folder);
			dest = Path.Combine(to, name);
			CopyDirectory( folder, dest, overwrite );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// 파일을 이동합니다.
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	public static void FileMove( string from, string to )
	{
		File.Copy( from, to, true );
		File.Delete( from );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//FileExtensions
