// Filename: Screenshotter.cs
using UnityEngine;
using System.IO; // Required for directory and file operations
using System.Globalization; // Required for consistent date/time formatting

/// <summary>
/// Takes a screenshot of the game view when the 'K' key is pressed.
/// Saves screenshots to Assets/Screenshots, creating the folder if it doesn't exist.
/// Primarily intended for use in the Unity Editor.
/// </summary>
public class Screenshotter : MonoBehaviour
{
    [Header("Screenshot Settings")]
    [Tooltip("The key to press to take a screenshot.")]
    [SerializeField] private KeyCode screenshotKey = KeyCode.K;

    [Tooltip("The name of the folder within Assets to save screenshots to.")]
    [SerializeField] private string screenshotFolderName = "Screenshots";

    [Tooltip("Prefix for the screenshot filename.")]
    [SerializeField] private string filenamePrefix = "Screenshot_";

    // Update is called once per frame
    void Update()
    {
        // Check if the specified screenshot key is pressed down
        if (Input.GetKeyDown(screenshotKey))
        {
            CaptureAndSaveScreenshot();
        }
    }

    /// <summary>
    /// Captures the current screen and saves it to the designated folder.
    /// </summary>
    private void CaptureAndSaveScreenshot()
    {
        // Construct the full path to the Screenshots folder within the Assets folder
        // Application.dataPath points to the Assets folder in the Unity Editor
        string folderPath = Path.Combine(Application.dataPath, screenshotFolderName);

        // Check if the Screenshots folder exists
        if (!Directory.Exists(folderPath))
        {
            // If it doesn't exist, create it
            Directory.CreateDirectory(folderPath);
            Debug.Log($"[Screenshotter] Created folder at: {folderPath}");

#if UNITY_EDITOR
            // Optionally, refresh the AssetDatabase if running in the editor
            // to ensure the new folder is immediately visible in the Project window.
            // Unity often picks this up automatically, but this can be explicit.
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        // Generate a unique filename using a timestamp (including milliseconds for higher uniqueness)
        // Using InvariantCulture for consistent formatting across different system locales
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture);
        string fileName = $"{filenamePrefix}{timestamp}.png";

        // Combine the folder path and filename to get the full path for the screenshot
        string filePath = Path.Combine(folderPath, fileName);

        // Capture the screenshot
        ScreenCapture.CaptureScreenshot(filePath);

        // Log a message to the console confirming the save
        Debug.Log($"[Screenshotter] Screenshot saved to: <color=lightblue>{filePath}</color>\n" +
                  $"Ensure your Game view is active or you are capturing the intended view (e.g., Device Simulator).");

#if UNITY_EDITOR
        // After saving, you might want to refresh the AssetDatabase again so the
        // screenshot appears immediately in the Project window without needing to refocus Unity.
        // This is helpful if the screenshot doesn't show up right away.
        // Note: ScreenCapture.CaptureScreenshot usually triggers an import, but this can ensure it.
        // However, calling it too frequently or unnecessarily can add slight overhead.
        // UnityEditor.AssetDatabase.ImportAsset(filePath); // More targeted refresh
        // UnityEditor.AssetDatabase.Refresh(); // Broader refresh
#endif
    }
}