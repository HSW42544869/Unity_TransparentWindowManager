using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MinimizeWindowOnStart : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_MINIMIZE = 6;

    void Start()
    {
        IntPtr windowHandle = GetActiveWindow();
        ShowWindow(windowHandle, SW_MINIMIZE);
    }
}
