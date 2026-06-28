
using System;
using System.Runtime.InteropServices;

public class BrowserUtil 
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void BrowserPopupWithString(string str);

    [DllImport("__Internal")]
    private static extern string GetUrl();
#endif

	public static void ShowBrowserPopup(string str)
    {
        // ����Ƽ������ �ݵ�� ������ ���´�. ũ�ҿ����� ��ȿ��.
#if UNITY_WEBGL && !UNITY_EDITOR
        BrowserPopupWithString(str);
#endif
        return;
    }

    public static string GetUrlFromWeb()
    {

        // ����Ƽ������ �ݵ�� ������ ���´�. ũ�ҿ����� ��ȿ��.
#if UNITY_WEBGL && !UNITY_EDITOR
        //BrowserPopupWithString("GetUrlFromWeb");
        string str = GetUrl();
        //BrowserPopupWithString("GetUrlFromWeb:" + str);
        return str;
#else
        return "";
#endif


    }
}