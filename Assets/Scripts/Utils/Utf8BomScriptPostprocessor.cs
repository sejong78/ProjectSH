// Assets/Editor/Utf8BomScriptPostprocessor.cs
//
// 목적:
//  - Unity에서 .cs 파일이 "처음 생성/임포트" 되었을 때,
//    인코딩을 확인하고 UTF-8 BOM 형태로 강제 저장합니다.
//
// 사용법:
//  1) 이 파일을 Assets/Editor 폴더에 넣습니다.
//  2) 이후 새로 생성되는 .cs 파일은 임포트 시점에 자동으로 UTF-8 BOM으로 정규화됩니다.
//  3) 필요하면 메뉴( Tools > Encoding > Normalize All C# To UTF-8 BOM )로 전체 일괄 처리도 가능합니다.
//
// 주의:
//  - 파일을 다시 저장하면 Unity가 해당 스크립트를 재임포트합니다.
//    하지만 BOM이 이미 붙으면 더 이상 수정하지 않으므로 무한 루프가 발생하지 않게 방어되어 있습니다.
//  - "BOM 없는 UTF-8" 또는 "ANSI(예: CP949)"로 저장된 파일을 발견하면
//    텍스트를 읽어 UTF-8 BOM으로 다시 씁니다.

#nullable enable

using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public sealed class Utf8BomScriptPostprocessor : AssetPostprocessor
{
	// 재진입(같은 임포트 사이클에서 또 호출) 방지용 가드
	private static bool s_isProcessing;

	private const string kMenuNormalizeAll = "Tools/Encoding/Normalize All C# To UTF-8 BOM";

	/// <summary>
	/// Unity 에셋 임포트 파이프라인 훅.
	/// 새로 임포트(=새로 생성되었거나 외부에서 추가된) .cs 파일을 대상으로 인코딩을 정규화합니다.
	/// </summary>
	private static void OnPostprocessAllAssets(
		string[ ] importedAssets,
		string[ ] deletedAssets,
		string[ ] movedAssets,
		string[ ] movedFromAssetPaths )
	{
		if( s_isProcessing )
		{
			return;
		}

		try
		{
			s_isProcessing = true;

			for( int i = 0; i < importedAssets.Length; ++i )
			{
				string assetPath = importedAssets[i];

				// 스크립트만 대상으로 합니다.
				if( !assetPath.EndsWith( ".cs", StringComparison.OrdinalIgnoreCase ) )
				{
					continue;
				}

				// 패키지 경로는 직접 쓰기 권한이 없거나 의도치 않은 변경이 될 수 있으니 기본 제외합니다.
				if( assetPath.StartsWith( "Packages/", StringComparison.OrdinalIgnoreCase ) )
				{
					continue;
				}

				bool changed = Utf8BomEncodingUtility.ConvertToUtf8BomIfNeeded(assetPath);

				// 파일을 수정했다면 재임포트가 필요합니다.
				// (대부분은 저장 자체로 감지되지만, 안정성을 위해 강제 업데이트를 걸어줍니다.)
				if( changed )
				{
					AssetDatabase.ImportAsset( assetPath, ImportAssetOptions.ForceUpdate );
				}
			}
		}
		finally
		{
			s_isProcessing = false;
		}
	}

	[MenuItem( kMenuNormalizeAll )]
	private static void NormalizeAllCsFiles()
	{
		// 프로젝트 전체(Assets 이하)에서 .cs를 찾아 UTF-8 BOM으로 정규화합니다.
		// 팀 프로젝트에서는 "레거시 파일 정리" 같은 시점에 한 번 돌려두면 효과가 좋습니다.
		string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets" });

		int changedCount = 0;

		try
		{
			s_isProcessing = true;

			for( int i = 0; i < guids.Length; ++i )
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

				if( !assetPath.EndsWith( ".cs", StringComparison.OrdinalIgnoreCase ) )
				{
					continue;
				}

				if( Utf8BomEncodingUtility.ConvertToUtf8BomIfNeeded( assetPath ) )
				{
					++changedCount;
					AssetDatabase.ImportAsset( assetPath, ImportAssetOptions.ForceUpdate );
				}
			}
		}
		finally
		{
			s_isProcessing = false;
		}

#if UNITY_EDITOR
		Debug.Log( $"[UTF-8 BOM] Normalized C# scripts: {changedCount}" );
#endif//UNITY_EDITOR
	}
}

internal static class Utf8BomEncodingUtility
{
	// UTF-8 BOM: EF BB BF
	private static readonly byte[] s_utf8Bom = { 0xEF, 0xBB, 0xBF };

	/// <summary>
	/// 에셋 경로(예: Assets/Scripts/Foo.cs)를 받아,
	/// 파일이 UTF-8 BOM이 아니라면 UTF-8 BOM으로 다시 저장합니다.
	/// </summary>
	/// <returns>변경했으면 true, 이미 UTF-8 BOM이었거나 실패하면 false</returns>
	public static bool ConvertToUtf8BomIfNeeded( string assetPath )
	{
		string fullPath = ToFullPath(assetPath);

		if( !File.Exists( fullPath ) )
		{
			return false;
		}

		// .cs는 보통 작지만, 그래도 스트림으로 BOM만 먼저 확인하고 필요할 때만 전체를 읽습니다.
		if( HasUtf8Bom( fullPath ) )
		{
			return false; // 이미 원하는 형태
		}

		// 텍스트 디코딩을 "최대한 안전하게" 합니다.
		// 1) UTF-16/32 BOM이면 그 인코딩으로 읽기
		// 2) 아니면 "유효한 UTF-8인지" 검사해서 UTF-8로 읽기
		// 3) 둘 다 아니면 시스템 기본 인코딩(Windows 한글 환경이면 보통 CP949)으로 읽기
		byte[] bytes = File.ReadAllBytes(fullPath);

		Encoding sourceEncoding = DetectEncodingFromBom(bytes) ?? GuessEncodingWithoutBom(bytes);

		string text;
		try
		{
			text = sourceEncoding.GetString( bytes );
		}
		catch( Exception ex )
		{
			Debug.LogWarning( $"[UTF-8 BOM] Failed to decode '{assetPath}' with {sourceEncoding.WebName}. {ex.Message}" );
			return false;
		}

		// UTF-8 BOM으로 다시 저장합니다.
		// new UTF8Encoding(true)는 BOM을 포함합니다.
		try
		{
			var utf8BomEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
			File.WriteAllText( fullPath, text, utf8BomEncoding );
			return true;
		}
		catch( Exception ex )
		{
			Debug.LogWarning( $"[UTF-8 BOM] Failed to write '{assetPath}' as UTF-8 BOM. {ex.Message}" );
			return false;
		}
	}

	private static string ToFullPath( string assetPath )
	{
		// Application.dataPath = <ProjectRoot>/Assets
		// assetPath는 "Assets/..." 형태이므로, 프로젝트 루트 기준으로 절대경로를 만든다.
		string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
		return Path.GetFullPath( Path.Combine( projectRoot, assetPath ) );
	}

	private static bool HasUtf8Bom( string fullPath )
	{
		using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

		if( stream.Length < 3 )
		{
			return false;
		}

		Span<byte> head = stackalloc byte[3];
		int read = stream.Read(head);

		return read == 3 &&
			   head[ 0 ] == s_utf8Bom[ 0 ] &&
			   head[ 1 ] == s_utf8Bom[ 1 ] &&
			   head[ 2 ] == s_utf8Bom[ 2 ];
	}

	private static Encoding? DetectEncodingFromBom( byte[ ] bytes )
	{
		// BOM 기반 확정 판별 (UTF-8/16/32)
		if( 3 <= bytes.Length &&
			0xEF == bytes[ 0 ] &&
			0xBB == bytes[ 1 ] && 
			0xBF == bytes[ 2 ] )
		{
			return new UTF8Encoding( encoderShouldEmitUTF8Identifier: true );
		}

		if( 4 <= bytes.Length &&
			0xFF == bytes[ 0 ] && 
			0xFE == bytes[ 1 ] &&
			0x00 == bytes[ 2 ] && 
			0x00 == bytes[ 3 ] )
		{
			return new UTF32Encoding( bigEndian: false, byteOrderMark: true );
		}

		if( 4 <= bytes.Length &&
			0x00 == bytes[ 0 ] && 
			0x00 == bytes[ 1 ] && 
			0xFE == bytes[ 2 ] &&
			0xFF == bytes[ 3 ] )
		{
			return new UTF32Encoding( bigEndian: true, byteOrderMark: true );
		}

		if( 2 <= bytes.Length &&
			0xFF == bytes[ 0 ] &&
			0xFE == bytes[ 1 ] )
		{
			return Encoding.Unicode; // UTF-16 LE
		}

		if( 2 <= bytes.Length &&
			0xFE == bytes[ 0 ] && 
			0xFF == bytes[ 1 ] )
		{
			return Encoding.BigEndianUnicode; // UTF-16 BE
		}

		return null;
	}

	private static Encoding GuessEncodingWithoutBom( byte[ ] bytes )
	{
		// "BOM 없는 UTF-8"인지 먼저 확인합니다.
		// throwOnInvalidBytes=true 로 유효성 검사를 하고, 실패하면 시스템 기본 인코딩으로 폴백합니다.
		var utf8Strict = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

		try
		{
			_ = utf8Strict.GetString( bytes );
			return new UTF8Encoding( encoderShouldEmitUTF8Identifier: false ); // BOM 없는 UTF-8로 읽기
		}
		catch
		{
			// 여기까지 왔다는 것은 "유효한 UTF-8 바이트열이 아니다" 가능성이 큽니다.
			// 한국 Windows 환경에선 대개 CP949(Encoding.Default)로 저장된 파일이 이런 케이스입니다.
			return Encoding.Default;
		}
	}
}//Utf8BomScriptPostprocessor

