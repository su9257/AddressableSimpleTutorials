using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace AddressablesManagement
{
    public class AddressableManager : MonoBehaviour
    {

        #region Singleton
        public const string Name = "AddressableManager";
        private static volatile AddressableManager instance = null;
        public static AddressableManager Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        //[ShowInInspector]
        //[DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
        public static Dictionary<string, List<AsyncOperationHandle<Sprite>>> spriteDic = new Dictionary<string, List<AsyncOperationHandle<Sprite>>>();
        public static Dictionary<string, List<AsyncOperationHandle<GameObject>>> prefabDic = new Dictionary<string, List<AsyncOperationHandle<GameObject>>>();
        public static Dictionary<string, AsyncOperationHandle<SceneInstance>> sceneDic = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();
        private void Awake()
        {
            instance = this;

            Addressables.InitializeAsync().Completed += (handle) =>
            {
                Debug.Log(handle.Result.LocatorId);
            };

        }
        void Start()
        {
        }

        #region 加载场景

        public static void LoadAddScene(string sceneName, Action callback)
        {
            Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (sceneDic.ContainsKey(sceneName))
                    {
                        Debug.LogWarning($"已经含有对应的场景句柄{sceneName}");
                        Addressables.Release(sceneDic[sceneName]);
                    }
                    sceneDic[sceneName] = handle;
                    callback.Invoke();
                }
            };
        }

        private static void UnLoadScene(string sceneName, Action callback)
        {
            if (!sceneDic.ContainsKey(sceneName))
            {
                Debug.LogWarning($"没有找到对应卸载场景的句柄:{sceneName}");
                return;
            }
            SceneInstance m_LoadedScene = sceneDic[sceneName].Result;
            Addressables.UnloadSceneAsync(m_LoadedScene).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Addressables.Release(sceneDic[sceneName]);
                    sceneDic.Remove(sceneName);
                }
            };
        }

        #endregion

        #region 加载Sprite
        public static async Task LoadSprite(string address, Action<Sprite> callback)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(address);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"加载：{address}失败请检查");
                return;
            }

            if (!spriteDic.ContainsKey(address))
            {
                List<AsyncOperationHandle<Sprite>> tempSpriteHandleList = new List<AsyncOperationHandle<Sprite>>();
                spriteDic[address] = tempSpriteHandleList;
            }
            spriteDic[address].Add(handle);
            Sprite sprite = handle.Result;
            callback?.Invoke(sprite);

        }
        public static void UnLoadSprite(string address)
        {
            if (!spriteDic.ContainsKey(address))
            {
                Debug.LogWarning("没有找到需要卸载的图片");
                return;
            }

            List<AsyncOperationHandle<Sprite>> tempSpriteHandleList = spriteDic[address];

            for (int i = 0; i < tempSpriteHandleList.Count; i++)
            {
                if (tempSpriteHandleList[i].IsValid())
                {
                    Addressables.Release(tempSpriteHandleList[i]);
                }
                else
                {
                    Debug.LogWarning("发现无效SpriteHandle");
                }
            }
            spriteDic.Remove(address);
            Debug.Log("卸载");
        }
        #endregion

        #region 加载预制体

        public static async Task LoadInstantiateAsyncGameObject(string address, Action<GameObject> callback)
        {
            var handle = Addressables.InstantiateAsync(address);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                callback?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogWarning("物体加载失败");
            }
        }

        public static void UnLoadGameObejct(GameObject @object)
        {
            bool tempBool = Addressables.ReleaseInstance(@object);
            Debug.Log($"卸载结果:{tempBool}");
        }

        /// <summary>
        /// 加载的为Prefab，需要自行实例化
        /// </summary>
        /// <param name="address"></param>
        /// <param name="callback"></param>
        public static async Task LoadPrefab(string address, Action<GameObject> callback)
        {

            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"加载：{address}失败请检查");
                return;
            }

            if (!prefabDic.ContainsKey(address))
            {
                List<AsyncOperationHandle<GameObject>> tempGameObjectHandleList = new List<AsyncOperationHandle<GameObject>>();
                prefabDic[address] = tempGameObjectHandleList;
            }
            prefabDic[address].Add(handle);
            GameObject tempPrefab = handle.Result;
            callback?.Invoke(tempPrefab);
        }

        public static void UnLoadPrefab(string address)
        {
            if (!prefabDic.ContainsKey(address))
            {
                Debug.LogWarning("没有找到需要卸载的图片");
                return;
            }

            List<AsyncOperationHandle<GameObject>> tempGameObjectHandleList = prefabDic[address];

            for (int i = 0; i < tempGameObjectHandleList.Count; i++)
            {
                if (tempGameObjectHandleList[i].IsValid())
                {
                    Addressables.Release(tempGameObjectHandleList[i]);
                }
                else
                {
                    Debug.LogWarning("发现无效SpriteHandle");
                }
            }
            prefabDic.Remove(address);
            Debug.Log("卸载");
        }
        #endregion


        #region Find Lable Address

        public static async Task<List<string>> DestinationLableAddress(IList<object> keys, Addressables.MergeMode mergeMode = Addressables.MergeMode.Intersection)
        {
            var handle = Addressables.LoadResourceLocationsAsync(keys, mergeMode);
            await handle.Task;

            IList<IResourceLocation> resourceLocations = handle.Result;

            List<string> addressList = new List<string>();

            for (int i = 0; i < resourceLocations.Count; i++)
            {
                addressList.Add(resourceLocations[i].PrimaryKey);
            }
            return addressList;
        }

        #endregion
    }
}