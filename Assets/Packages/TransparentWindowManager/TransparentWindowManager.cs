using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

public class TransparentWindowManager : SingletonMonoBehaviour<TransparentWindowManager>
{
    public Image interactiveArea; // 引用 UI Image 组件

    #region DLL Import

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    #endregion DLL Import

    #region Structs and Constants

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    private const uint WS_EX_TOPMOST = 0x00000008;

    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    private const uint SWP_SHOWWINDOW = 0x0040;

    #endregion

    private IntPtr windowHandle;

    protected virtual void Start()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        MakeWindowTransparentAndFullscreen();
        #endif
    }

    void Update()
    {
        #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        UpdateInteractiveArea();
        #endif
    }

    private void MakeWindowTransparentAndFullscreen()
    {
        windowHandle = GetActiveWindow();

        // 获取屏幕尺寸
        int screenWidth = GetSystemMetrics(SM_CXSCREEN);
        int screenHeight = GetSystemMetrics(SM_CYSCREEN);

        // 设置窗口样式
        SetWindowLong(windowHandle, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // 设置扩展窗口样式：layered, transparent 和 topmost
        SetWindowLong(windowHandle, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST);

        // 扩展窗口框架
        MARGINS margins = new MARGINS() { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(windowHandle, ref margins);

        // 设置窗口位置和大小为全屏
        SetWindowPos(windowHandle, new IntPtr(-1), 0, 0, screenWidth, screenHeight, SWP_SHOWWINDOW);
    }

    private void UpdateInteractiveArea()
    {
        if (interactiveArea == null) return;

        POINT cursorPos;
        GetCursorPos(out cursorPos);
        ScreenToClient(windowHandle, ref cursorPos);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            interactiveArea.rectTransform, 
            new Vector2(cursorPos.X, cursorPos.Y), 
            null, out localPoint);

        bool isInsideInteractiveArea = interactiveArea.rectTransform.rect.Contains(localPoint);

        uint exStyle = (uint)SetWindowLong(windowHandle, GWL_EXSTYLE, 0);
        if (isInsideInteractiveArea)
        {
            // 在交互区域内，移除 WS_EX_TRANSPARENT 标志
            exStyle &= ~WS_EX_TRANSPARENT;
        }
        else
        {
            // 在交互区域外，添加 WS_EX_TRANSPARENT 标志
            exStyle |= WS_EX_TRANSPARENT;
        }
        SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TOPMOST);
    }
}