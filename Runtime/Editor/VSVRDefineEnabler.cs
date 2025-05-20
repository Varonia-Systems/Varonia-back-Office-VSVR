
#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class VSVRDefineEnabler
{
    static VSVRDefineEnabler()
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (!symbols.Contains("VBO_VSVR"))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                symbols + ";VBO_VSVR"
            );
        }
    }
}
#endif