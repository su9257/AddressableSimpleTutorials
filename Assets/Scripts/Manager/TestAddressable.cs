using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceLocations;
using AddressablesManagement;
public class TestAddressable : MonoBehaviour
{
    // Start is called before the first frame update
    //AddressablesManagement

    //public AssetLabelReference assetLabelReference;
    public AssetReference assetReference;
    void Start()
    {
        //assetReference.LoadAssetAsync<Sprite>();
        //var handle =  Addressables.InitializeAsync();

        // handle.Completed += (resourceLocator) =>
        // {
        //     Debug.Log(resourceLocator.Result.LocatorId);
        // };

        //Addressables.LoadResourceLocationsAsync("Sprite1");
        //Debug.Log("准备执行pre");
        //Test1();
        //Debug.Log("执行后");
    }


    public async Task Test1()
    {
        var handle = Addressables.LoadResourceLocationsAsync("Sprite1");
        await handle.Task;

        for (int i = 0; i < handle.Result.Count; i++)
        {
            IResourceLocation resourceLocation = handle.Result[i];
            Debug.Log($"PrimaryKey:{resourceLocation.PrimaryKey}");
            Debug.Log($"InternalId:{resourceLocation.InternalId}");
            Debug.Log($"ProviderId:{resourceLocation.ProviderId}");
        }
    }

    // Update is called once per frame

    public GameObject temp;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddressableManager.LoadSprite("button1", SpriteCallBack);
            Debug.Log("加载图片");
            //Test1();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //AddressableManager.UnLoadGameObejct(temp);
            Task.Run(() => 
            {
                Task<List<string>> task = AddressableManager.DestinationLableAddress(new List<object> { "CoffeeShop" });

                Debug.Log(task.Result);

                for (int i = 0; i < task.Result.Count; i++)
                {
                    Debug.Log(task.Result[i]);
                }

                Debug.Log("按键2");
            });
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //ClawMachineVariant1
            Task tempTask = AddressableManager.LoadInstantiateAsyncGameObject("Huadian", (obj) =>
            {
                Debug.Log(obj);
                temp = obj;
            });

            Debug.Log("完事了");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Addressables.InstantiateAsync("GroupPrefab");
            Debug.Log("按键4");
        }
    }

    public void LoadOBJ()
    {
        AddressableManager.LoadInstantiateAsyncGameObject("Huadian", (obj) =>
        {
            Debug.Log(obj);
            temp = obj;
        });
    }
    public void UnLoadObj()
    {
        AddressableManager.UnLoadGameObejct(temp);
    }


    public void SpriteCallBack(Sprite sprites)
    {
        //Debug.Log($"对应的数量为：{sprites.Count}");
        GameObject.Find("Image").GetComponent<Image>().sprite = sprites;
        Debug.Log("副回调");
    }
}
