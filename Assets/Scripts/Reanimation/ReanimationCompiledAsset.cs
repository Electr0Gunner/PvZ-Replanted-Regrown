
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "Reanimation", menuName = "ScriptableObjects/Compiled Reanimation", order = 1)]
[System.Serializable]
public class ReanimationCompiledAsset : ScriptableObject
{
    public ReanimatorDefinition definition;
}

[CustomEditor(typeof(ReanimationCompiledAsset))]
public class ReanimationCompiledAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var asset = (ReanimationCompiledAsset)target;
        EditorGUILayout.LabelField("Reanimation Definition:");
        if (target == null)
        {
            EditorGUILayout.TextArea("Definition doesn't exist");
            return;
        }
        EditorGUILayout.LabelField("FPS:", asset.definition.mFPS.ToString());

        EditorGUILayout.LabelField("Track Count:", asset.definition.mTrackCount.ToString());
        EditorGUILayout.LabelField("Tracks:", asset.definition.mTracks != null ? asset.definition.mTracks.Count.ToString() : "0");

        EditorGUILayout.Space();
        if (asset.definition.mTracks != null)
        {
            foreach (var track in asset.definition.mTracks)
            {
                if (track == null)
                    continue;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Track", track.mName ?? "(unnamed)");
                EditorGUILayout.LabelField("Transforms", track.mTransforms?.Count.ToString() ?? "null");
                EditorGUILayout.EndVertical();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("mTracks is null (not serialized or not loaded).", MessageType.Error);
        }

        EditorGUI.BeginChangeCheck();
        ReanimAtlas newAtlas = (ReanimAtlas)EditorGUILayout.ObjectField("Atlas", asset.definition.mReanimAtlas, typeof(ReanimAtlas), false);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(asset, "Changed Reanim Atlas");
            asset.definition.mReanimAtlas = newAtlas;
            EditorUtility.SetDirty(asset);
        }
    }
}