using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class GameActionAutoMigrate
{
    private const string PREFS_KEY = "GameActionMigrated_v3";
    private static bool _migrating;

    static GameActionAutoMigrate()
    {
        // Only auto-run once per editor environment. Re-running the migration
        // recreates the SerializeReference object graph and causes Unity to
        // assign new managed reference ids (rid), which creates noisy diffs.
        if (!_migrating && !EditorPrefs.GetBool(PREFS_KEY, false))
        {
            EditorApplication.update += TryMigrate;
        }
    }

    private static void TryMigrate()
    {
        // Wait until editor is fully ready
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying)
            return;

        EditorApplication.update -= TryMigrate;
        _migrating = true;

        Debug.Log("[AutoMigrate] Editor ready, migrating...");
        GameActionMigrationTool.MigrateAll();
        EditorPrefs.SetBool(PREFS_KEY, true);
        _migrating = false;
    }

    [MenuItem("Tools/GameAction/\u5f3a\u5236\u91cd\u65b0\u8fc1\u79fb\u6240\u6709\u8d44\u4ea7")]
    public static void ForceRemigrate()
    {
        EditorPrefs.DeleteKey(PREFS_KEY);
        Debug.Log("[ForceRemigrate] Flag cleared. Triggering migration...");
        _migrating = true;
        GameActionMigrationTool.MigrateAll();
        EditorPrefs.SetBool(PREFS_KEY, true);
        _migrating = false;
    }
}
