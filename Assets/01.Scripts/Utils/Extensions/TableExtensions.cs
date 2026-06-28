using System;
using System.Xml;

public static class TableExtensions
{
	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

	public static void Parse( this UnityEngine.TextAsset textAsset, string xPath, Action<XmlNode> onParse )
	{
		if( null == textAsset || true == string.IsNullOrEmpty( textAsset.text ) )
		{
			DebugExtensions.LogError( "TextAsset is null or empty." );
			return;
		}

		textAsset.ParseTextAsset( xmlString => 
			xmlString.ParseXml( doc => 
				doc.ParseXmlDocument( xPath, onParse ) ) );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------

	public static void ParseTextAsset( this UnityEngine.TextAsset textAsset, Action<string> onParse )
	{
		if( null == textAsset || true == string.IsNullOrEmpty( textAsset.text ) )
		{
			DebugExtensions.LogError( "TextAsset is null or empty." );
			return;
		}

		onParse.SafeExcute( textAsset.text );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------


	public static void ParseXml( this string xmlString, Action<XmlDocument> onParse )
	{
		XmlDocument xmlDoc = new XmlDocument();

		// XML 문자열 로드
		xmlDoc.LoadXml( xmlString );

		onParse.SafeExcute( xmlDoc );
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public static void ParseXmlDocument( this XmlDocument doc, string xPath, Action<XmlNode> onParse )
	{
		XmlNodeList xmlNodeList = doc.SelectNodes( xPath );

		if( xmlNodeList == null || xmlNodeList.Count == 0 )
		{
			DebugExtensions.LogError( $"No nodes found at xPath: {xPath}" );
			return;
		}

		foreach( XmlNode node in xmlNodeList )
		{
			onParse.SafeExcute( node );
		}
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	
	public static string Parse( this XmlNode node, string attributes, string defaultString = "" )
	{
		if( null == node )
			return defaultString;

		return node.Attributes[ attributes ]?.Value ?? defaultString;
	}

	//@@-------------------------------------------------------------------------------------------------------------------------
	//@@-------------------------------------------------------------------------------------------------------------------------

}//TableExtensions
