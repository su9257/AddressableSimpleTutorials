using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
public class SelfDestruct : MonoBehaviour
{
    public float lifetime = 2f;
    void Start()
    {
        Invoke("Release", lifetime);
    }

    void Release()
    {
        //非经过Addressable创建的实例不能销毁，所以需要判断
        if (!Addressables.ReleaseInstance(gameObject))
        {
            Destroy(gameObject);
        }
    }
}
