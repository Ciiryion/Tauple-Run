using UnityEditor;

public class UpdateEnterPlayModeSettings : Editor
{
    [MenuItem("Tools/Play Mode/Enable Enter Play Mode Settings (FAST)")]
    public static void EnableEnterPlayModeSettingsMenu()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
    }

    [MenuItem("Tools/Play Mode/Disable Enter Play Mode Settings (SLOW)")]
    public static void DisableEnterPlayModeSettingsMenu()
    {
        EditorSettings.enterPlayModeOptionsEnabled = false;
    }
}