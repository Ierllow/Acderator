using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Screenshot
{
    private static readonly string rootPath = Application.dataPath + "/..";
    private static readonly string path = rootPath + "/" + "ScreenShot" + "/";
    private static readonly string fileName = string.Format("{0}{1}.png", path, DateTime.Now.ToString("yy-MM-dd_HH-mm-ss"));

    [MenuItem("Tools/Screenshot")]
    public static void CaptureScreenshot() => ExecCaptureScreenshot();

    private static void ExecCaptureScreenshot()
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        ScreenCapture.CaptureScreenshot(fileName);

        Debug.Log("Screenshot is completed. ");
        Debug.Log(string.Format("path: {0}", path));
        Debug.Log(string.Format("fileName: {0}", fileName));
    }
}