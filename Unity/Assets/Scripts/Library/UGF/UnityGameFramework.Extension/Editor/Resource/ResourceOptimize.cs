using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GameFramework;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace UnityGameFramework.Extension.Editor
{
    public sealed partial class ResourceOptimize
    {
        private readonly long MAX_COMBINE_SHARE_AB_ITEM_SIZE = 500 * 1024 * 8; // 500K 文件体积小于这个数量才能合并
        private readonly long MAX_COMBINE_SHARE_AB_SIZE = 1024 * 1024 * 8; // 1M 最终合并的目标大小
        private readonly long MIN_NO_NAME_COMBINE_SIZE = 32 * 1024 * 8; // 32K 最终合并的目标大小
        // public const long MAX_COMBINE_SHARE_NO_NAME = 60 * 1024 * 8; // 60K 没有包名的最大体积 
        // public const int MAX_COMBINE_SHARE_NO_NAME_REFERENCE_COUNT = 7; // 没有包名的最多的引用计数
        //  public const int MIN_COMBINE_AB_SIZE_2 = 100 * 1024 * 8;//  100K 没有包名的最大体积 
        private readonly int MAX_COMBINE_SHARE_MIN_REFERENCE_COUNT = 3; //最大的引用计数
        
        //有需要改名，修改这里
        public static string GetNewCombineName(List<string> currentCombineBundle)
        {
            var newCombine = string.Join("@@", currentCombineBundle);
            return $"Auto/Combine/{Utility.Verifier.GetCrc32(Encoding.UTF8.GetBytes(newCombine))}";
        }
        
        private ResourceCollection m_ResourceCollection;

        private readonly Dictionary<string, DependencyData> m_DependencyDatas;
        private readonly Dictionary<string, List<Asset>> m_ScatteredAssets;
        private readonly HashSet<Stamp> m_AnalyzedStamps;
        private readonly Dictionary<string, List<string>> m_CombineBundles;
        private readonly MethodInfo m_GetStorageMemorySizeLongMethod;
        private readonly object[] m_ParamCache;

        public ResourceOptimize()
        {
            m_ResourceCollection = new ResourceCollection();
            m_DependencyDatas = new Dictionary<string, DependencyData>();
            m_ScatteredAssets = new Dictionary<string, List<Asset>>();
            m_AnalyzedStamps = new HashSet<Stamp>();
            m_CombineBundles = new Dictionary<string, List<string>>();
            m_GetStorageMemorySizeLongMethod = typeof(EditorWindow).Assembly.GetType("UnityEditor.TextureUtil").GetMethod("GetStorageMemorySizeLong", BindingFlags.Static | BindingFlags.Public);
            m_ParamCache = new object[1];
        }

        public void Optimize(ResourceCollection resourceCollection = null)
        {
            Analyze(resourceCollection);
            CalCombine();
            Save();
        }

        private void Save()
        {
            foreach (var kv in m_CombineBundles)
            {
                m_ResourceCollection.AddResource(kv.Key, null, null, LoadType.LoadFromFile, true);
                foreach (var name in kv.Value)
                {
                    m_ResourceCollection.AssignAsset(AssetDatabase.AssetPathToGUID(name), kv.Key, null);
                }
            }
            m_ResourceCollection.Save();
        }

        private void CalCombine()
        {
            m_CombineBundles.Clear();
            Dictionary<string, ABInfo> allCombines = new Dictionary<string, ABInfo>();
            int allShareCount = 0; //所有的share ab 的数量
            int allShareCanCombine = 0; // 所有size 合格的ab
            int allShareRemoveByNoName = 0; // 因为 size 太小，ref count 太少 放弃合并，放弃包名
            int allShareRemoveByReferenceCountTooFew = 0; // 因为 ref count 太少 放弃合并
            int allFinalCombine = 0; // 最终合并的数量

            foreach (var kv in m_ScatteredAssets)
            {
                var assetPath = kv.Key;
                allShareCount++;
                if (!File.Exists(assetPath))
                {
                    Debug.LogError(Utility.Text.Format("File do not exist :{0}", assetPath));
                    continue;
                }
                long byteSize = GetAssetSize(assetPath);
                if (byteSize < MAX_COMBINE_SHARE_AB_ITEM_SIZE)
                {
                    allCombines.Add(assetPath, new ABInfo()
                    {
                        name = assetPath,
                        size = byteSize,
                        referenceCount = kv.Value.Count
                    });
                    allShareCanCombine++;
                }
            }
            
            foreach (var abInfo in allCombines.Values.ToArray())
            {
                var bundleName = abInfo.name;
                if (abInfo.size * abInfo.referenceCount < MIN_NO_NAME_COMBINE_SIZE)
                {
                    allShareRemoveByNoName++;
                    allCombines.Remove(bundleName);
                }
                else 
                {
                    if (abInfo.referenceCount < MAX_COMBINE_SHARE_MIN_REFERENCE_COUNT)
                    {
                        allShareRemoveByReferenceCountTooFew++;
                        allCombines.Remove(bundleName);
                    }
                }
            }

            var left =  allCombines.Values.ToList();
            left.Sort((a,b) => a.size.CompareTo(b.size));
            allFinalCombine = left.Count;
            List<string> currentCombineBundle = null;
            long currentCombineBundleSize = 0;
            for (int i = 0; i < left.Count; i++) 
            { 
                var abName= left[i].name; 
                var size= left[i].size;
                currentCombineBundle ??= new List<string>();
                currentCombineBundle.Add(abName);
                currentCombineBundleSize += size;
                if (currentCombineBundleSize > MAX_COMBINE_SHARE_AB_SIZE)
                {
                    m_CombineBundles[GetNewCombineName(currentCombineBundle)] = currentCombineBundle;
                    currentCombineBundle = null;
                    currentCombineBundleSize = 0;
                }
            }
            if (currentCombineBundle != null && currentCombineBundle.Count > 0)
            {
                m_CombineBundles[GetNewCombineName(currentCombineBundle)] = currentCombineBundle;
            }
            Debug.Log($"总共有share ab的数量{allShareCount}，大小合格的数量{allShareCanCombine}，" +
                      $"因为ab太小和引用计数太少而被取消包名的数量{allShareRemoveByNoName}，" +
                      $"因为引用过少被移除合并的数量{allShareRemoveByReferenceCountTooFew}，" +
                      $"最终{allFinalCombine}个share ab，" +
                      $"合并成{m_CombineBundles.Count}个share_combine，" +
                      $"因为这次合并操作，总共减少了{(m_CombineBundles.Count > 0 ? allShareRemoveByNoName + allFinalCombine - m_CombineBundles.Count : 0)}个share bundle");
        }

        private void Analyze(ResourceCollection resourceCollection)
        {
            if (resourceCollection == null)
            {
                m_ResourceCollection = new ResourceCollection();
                m_ResourceCollection.Load();
            }
            else
            {
                m_ResourceCollection = resourceCollection;
            }
            m_DependencyDatas.Clear();
            m_ScatteredAssets.Clear();
            m_AnalyzedStamps.Clear();

            HashSet<string> scriptAssetNames = GetFilteredAssetNames("t:Script");
            Asset[] assets = m_ResourceCollection.GetAssets();
            int count = assets.Length;
            for (int i = 0; i < count; i++)
            {
                string assetName = assets[i].Name;
                if (string.IsNullOrEmpty(assetName))
                {
                    Debug.LogWarning(Utility.Text.Format("Can not find asset by guid '{0}'.", assets[i].Guid));
                    continue;
                }

                DependencyData dependencyData = new DependencyData();
                AnalyzeAsset(assetName, assets[i], dependencyData, scriptAssetNames);
                dependencyData.RefreshData();
                m_DependencyDatas.Add(assetName, dependencyData);
            }

            foreach (List<Asset> scatteredAsset in m_ScatteredAssets.Values)
            {
                scatteredAsset.Sort((a, b) => a.Name.CompareTo(b.Name));
            }
        }

        private void AnalyzeAsset(string assetName, Asset hostAsset, DependencyData dependencyData,
            HashSet<string> scriptAssetNames)
        {
            string[] dependencyAssetNames = AssetDatabase.GetDependencies(assetName, false);
            foreach (string dependencyAssetName in dependencyAssetNames)
            {
                if (scriptAssetNames.Contains(dependencyAssetName))
                {
                    continue;
                }

                if (dependencyAssetName == assetName)
                {
                    continue;
                }

                if (dependencyAssetName.EndsWith(".unity", StringComparison.Ordinal))
                {
                    // 忽略对场景的依赖
                    continue;
                }

                Stamp stamp = new Stamp(hostAsset.Name, dependencyAssetName);
                if (m_AnalyzedStamps.Contains(stamp))
                {
                    continue;
                }

                m_AnalyzedStamps.Add(stamp);

                string guid = AssetDatabase.AssetPathToGUID(dependencyAssetName);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning(Utility.Text.Format("Can not find guid by asset '{0}'.", dependencyAssetName));
                    continue;
                }

                Asset asset = m_ResourceCollection.GetAsset(guid);
                if (asset != null)
                {
                    dependencyData.AddDependencyAsset(asset);
                }
                else
                {
                    dependencyData.AddScatteredDependencyAsset(dependencyAssetName);

                    List<Asset> scatteredAssets = null;
                    if (!m_ScatteredAssets.TryGetValue(dependencyAssetName, out scatteredAssets))
                    {
                        scatteredAssets = new List<Asset>();
                        m_ScatteredAssets.Add(dependencyAssetName, scatteredAssets);
                    }

                    scatteredAssets.Add(hostAsset);

                    AnalyzeAsset(dependencyAssetName, hostAsset, dependencyData, scriptAssetNames);
                }
            }
        }

        private HashSet<string> GetFilteredAssetNames(string filter)
        {
            string[] filterAssetGuids = AssetDatabase.FindAssets(filter);
            HashSet<string> filterAssetNames = new HashSet<string>();
            foreach (string filterAssetGuid in filterAssetGuids)
            {
                filterAssetNames.Add(AssetDatabase.GUIDToAssetPath(filterAssetGuid));
            }

            return filterAssetNames;
        }

        private long GetAssetSize(string assetPath)
        {
            Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            if (type == typeof(Texture2D))
            {
                //记录精准的贴图大小
                Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                m_ParamCache[0] = texture2D;
                long size = (long)m_GetStorageMemorySizeLongMethod.Invoke(null, m_ParamCache);
                Resources.UnloadAsset(texture2D);
                return size;
            }
            else
            {
                //直接使用文件长度
                return new FileInfo(assetPath).Length;
            }
        }
    }
}