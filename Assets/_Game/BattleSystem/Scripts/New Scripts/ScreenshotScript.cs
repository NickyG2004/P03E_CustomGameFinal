using UnityEngine;
using UnityEngine.Rendering;

public class ScreenshotScript : MonoBehaviour
{
    void OnApplicationFocus(bool hasFocus)
    {
        if (Input.GetKeyDown("k"))
        {
            // Create a folder in your Assets folder called "screenshots" if it doesn't exist
            if (!System.IO.Directory.Exists("Assets/screenshots"))
            {
                System.IO.Directory.CreateDirectory("Assets/screenshots");
            }

            // Capture the screenshot and save it
            ScreenCapture.CaptureScreenshot("Assets/screenshots/game_screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");
        }
    }
}
