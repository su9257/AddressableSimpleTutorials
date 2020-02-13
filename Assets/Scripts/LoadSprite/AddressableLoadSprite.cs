using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEngine.U2D;

public class AddressableLoadSprite : MonoBehaviour
{
    public List<Image> images;

    public AssetReferenceSprite referenceSprite;

    //public AssetReferenceAtlasedSprite assetReferenceAtlasedSprite;
    //public AssetReferenceSprite assetReferenceSprite;
    //public AssetReferenceSprite assetReferenceSprite_1;
    //public AssetReferenceAtlas assetReferenceAtlas;
    void Start()
    {
        //Addressables.LoadAssetAsync<Sprite>("button_everyplay");
        referenceSprite.LoadAssetAsync().Completed += SpriteHandle;
        ;
        //assetReferenceAtlasedSprite.LoadAssetAsync().Completed += AtlasedSpriteHandle;
        //assetReferenceSprite.LoadAssetAsync().Completed += SpriteHandle;
        //assetReferenceSprite_1.LoadAssetAsync().Completed += SpriteHandle1;
        //assetReferenceAtlas.LoadAssetAsync().Completed += AtlasHandle;
    }

    private void AtlasedSpriteHandle(AsyncOperationHandle<Sprite> obj)
    {
        if (obj.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("加载失败");
        }
        images[0].sprite = obj.Result;
    }
    private void SpriteHandle(AsyncOperationHandle<Sprite> obj)
    {
        images[1].sprite = obj.Result;
    }

    private void SpriteHandle1(AsyncOperationHandle<Sprite> obj)
    {
        images[2].sprite = obj.Result;
    }
    private void AtlasHandle(AsyncOperationHandle<SpriteAtlas> obj)
    {
        images[3].sprite = obj.Result.GetSprite("button_twitter");
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {

        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {

        }
    }
}
