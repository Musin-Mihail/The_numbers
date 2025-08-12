using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TEST
{
    public class ScreenshotTaker : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.pKey.wasPressedThisFrame) return;
            var screenshotName = "Screenshot_" + DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";
            ScreenCapture.CaptureScreenshot(screenshotName);
            Debug.Log("Скриншот сохранен: " + screenshotName);
        }
    }
}