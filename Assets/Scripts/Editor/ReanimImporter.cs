using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;


[ScriptedImporter(1, "reanim")]
public class ReanimImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var fileText = File.ReadAllText(ctx.assetPath);
        
        var reanimAsset = ScriptableObject.CreateInstance<ReanimAsset>();
        reanimAsset.data = fileText;

        var compiled = CreateCompiledAsset(reanimAsset);

        //ctx.AddObjectToAsset("compiled", compiled);
        //ctx.SetMainObject(compiled);
        //Save the compiled data only after the import is done
        EditorApplication.delayCall += () =>
        {
            string dir = Path.GetDirectoryName(assetPath);
            string baseName = Path.GetFileNameWithoutExtension(assetPath);
            string compiledPath = Path.Combine(dir, baseName + "_Compiled.asset");

            AssetDatabase.CreateAsset(compiled, compiledPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created compiled asset at {compiledPath}");
        };

    }
    private ReanimationCompiledAsset CreateCompiledAsset(ReanimAsset source)
    {
        ReanimatorDefinition def = ReanimReader.Read(source.data);

        var compiled = ScriptableObject.CreateInstance<ReanimationCompiledAsset>();
        compiled.definition = def;

    //    EditorUtility.DisplayDialog("Success", $"Created compiled asset", "OK");
        return compiled;
    }
}
