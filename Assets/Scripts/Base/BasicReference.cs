using UnityEngine;
using UnityEngine.AddressableAssets;

public class BasicReference : MonoBehaviour
{
	public AssetReference baseCube;
	void Start()
	{
		baseCube.InstantiateAsync();
	}


	public void SpawnThing()
	{
		baseCube.InstantiateAsync();
	}
}
