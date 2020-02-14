using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BuildScriptInherited.asset", menuName = "Addressables/Custom Build/Packed Variations")]
public class BuildScriptInherited : BuildScriptPackedMode
{
    public override string Name
    {
        get { return "Test Packed Variations"; }
    }

    protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput context)
    {
        var result = base.BuildDataImplementation<TResult>(context);

        AddressableAssetSettings settings = context.AddressableSettings;
        DoCleanup(settings);
        return result;
    }
    /// <summary>
    /// 在build bundle 的时候实时处理
    /// </summary>
    /// <param name="assetGroup">就是对应Group的Schema</param>
    /// <param name="aaContext"></param>
    /// <returns></returns>
    protected override string ProcessGroup(AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
    {
        if (assetGroup.HasSchema<TextureVariationSchema>())
        {
            var errorString = ProcessTextureScaler(assetGroup.GetSchema<TextureVariationSchema>(), assetGroup, aaContext);
            if (!string.IsNullOrEmpty(errorString))
                return errorString;
        }

        return base.ProcessGroup(assetGroup, aaContext);
    }

    List<AddressableAssetGroup> m_SourceGroupList = new List<AddressableAssetGroup>();
    Dictionary<string, AddressableAssetGroup> m_GeneratedGroups = new Dictionary<string, AddressableAssetGroup>();




    string ProcessTextureScaler(TextureVariationSchema schema, AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
    {
        m_SourceGroupList.Add(assetGroup);

        var entries = new List<AddressableAssetEntry>(assetGroup.entries);//获取对应Group下面的所有条目
        foreach (var entry in entries)
        {
            var entryPath = entry.AssetPath;
            if (AssetDatabase.GetMainAssetTypeAtPath(entryPath) == typeof(Texture2D))
            {
                var fileName = Path.GetFileNameWithoutExtension(entryPath);
                if (string.IsNullOrEmpty(fileName))
                    return "Failed to get file name for: " + entryPath;
                if (!Directory.Exists("Assets/GeneratedTextures"))
                    Directory.CreateDirectory("Assets/GeneratedTextures");
                if (!Directory.Exists("Assets/GeneratedTextures/Texture"))
                    Directory.CreateDirectory("Assets/GeneratedTextures/Texture");

                var sourceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(entryPath);
                var aiSource = AssetImporter.GetAtPath(entryPath) as TextureImporter;
                int maxDim = Math.Max(sourceTex.width, sourceTex.height);

                foreach (var pair in schema.Variations)//根据对应的策略开始处理 （现在是根据这一张图片分别设置不同的标签）
                {
                    var newGroup = FindOrCopyGroup(assetGroup.Name + "_" + pair.label, assetGroup, aaContext.settings, schema);
                    var newFile = entryPath.Replace(fileName, fileName + "_variationCopy_" + pair.label);
                    newFile = newFile.Replace("Assets/", "Assets/GeneratedTextures/");//生成新的对应条目原始文件fullName

                    AssetDatabase.CopyAsset(entryPath, newFile);//创建对应新的文件

                    var aiDest = AssetImporter.GetAtPath(newFile) as TextureImporter;//转换成texture
                    if (aiDest == null)
                    {
                        var message = "failed to get TextureImporter on new texture asset: " + newFile;
                        return message;
                    }

                    float scaleFactor = pair.textureScale;

                    float desiredLimiter = maxDim * scaleFactor;
                    aiDest.maxTextureSize = NearestMaxTextureSize(desiredLimiter);//生成里18*32*64*128*256等最近的尺寸

                    aiDest.isReadable = true;//可以通过脚本获取纹理信息

                    aiDest.SaveAndReimport();//保存并导入（如果资产导入器是脏的，则保存资产导入器设置。）
                    var newEntry = aaContext.settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(newFile), newGroup);
                    newEntry.address = entry.address;//对应条目的名称或者地址
                    newEntry.SetLabel(pair.label, true);//设置为true将添加标签，设置为false将删除标签。
                }
                entry.SetLabel(schema.BaselineLabel, true);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="groupName">assetGroup.Name + "_" + pair.label</param>
    /// <param name="baseGroup"></param>
    /// <param name="settings"></param>
    /// <param name="schema"></param>
    /// <returns></returns>
    AddressableAssetGroup FindOrCopyGroup(string groupName, AddressableAssetGroup baseGroup, AddressableAssetSettings settings, TextureVariationSchema schema)
    {
        AddressableAssetGroup result;
        if (!m_GeneratedGroups.TryGetValue(groupName, out result))//没有对应的Group就创建
        {
            List<AddressableAssetGroupSchema> schemas = new List<AddressableAssetGroupSchema>(baseGroup.Schemas);//这个group的所有Schemas
            schemas.Remove(schema);//移除自定义这个schema
            result = settings.CreateGroup(groupName, false, false, false, schemas);//创建一个新的Group并设置除了自定义schema以为的所有schema
            m_GeneratedGroups.Add(groupName, result);//添加这个临时生成的Group
        }
        return result;
    }
    static int NearestMaxTextureSize(float desiredLimiter)
    {
        float lastDiff = Math.Abs(desiredLimiter);
        int lastPow = 32;
        for (int i = 0; i < 9; i++)
        {

            int powOfTwo = lastPow << 1;
            float newDiff = Math.Abs(desiredLimiter - powOfTwo);
            if (newDiff > lastDiff)
                return lastPow;

            lastPow = powOfTwo;
            lastDiff = newDiff;

        }
        return 8192;
    }

    void DoCleanup(AddressableAssetSettings settings)
    {
        List<string> directories = new List<string>();
        foreach (var group in m_GeneratedGroups.Values)//遍历每个Group
        {
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>(group.entries);
            foreach (var entry in entries)//遍历每个Group下的所有条目
            {
                var path = entry.AssetPath;
                AssetDatabase.DeleteAsset(path);//删除这些临时条目下的文件
            }

            settings.RemoveGroup(group);//移除对应的临时Group
        }
        m_GeneratedGroups.Clear();

        foreach (var group in m_SourceGroupList)
        {
            var schema = group.GetSchema<TextureVariationSchema>();
            if (schema == null)
                continue;

            foreach (var entry in group.entries)
            {
                entry.labels.Remove(schema.BaselineLabel);
            }
        }

        m_SourceGroupList.Clear();
    }
}
