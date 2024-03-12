using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using com.microgamedev.AddressablesLoader;

public class TestingManager : MonoBehaviour
{
    [SerializeField] Button loadSingleSpriteBtn;
    [SerializeField] Button loadMultipleSpriteBtn;
    [SerializeField] Button loadSingleGameObjectBtn;
    [SerializeField] Button loadMultipleGameObjectBtn;
    [SerializeField] Button clearAllSpriteGameObjectBtn;

    [SerializeField] Transform gameObjLoadingPlace;
    [SerializeField] Transform imgLoadingPlace;
    [SerializeField] Image imgPrefab;

    [SerializeField] List<AssetReference> spriteRefList = new List<AssetReference>();
    [SerializeField] List<AssetReference> gameObjectRefList = new List<AssetReference>();

    private void Start()
    {
        loadSingleSpriteBtn.onClick.AddListener(HandleLoadSingleSpriteBtn);
        loadMultipleSpriteBtn.onClick.AddListener(HandleLoadMultipleSpriteBtn);
        loadSingleGameObjectBtn.onClick.AddListener(HandleLoadSingleGameObjectBtn);
        loadMultipleGameObjectBtn.onClick.AddListener(HandleLoadMultipleGameObjectBtn);
        clearAllSpriteGameObjectBtn.onClick.AddListener(HandleClearAllSpriteGameObjectBtn);
        Random.InitState((int)DateTime.Now.Ticks);
    }

    private void HandleLoadSingleSpriteBtn()
    {
        int index = Random.Range(0, spriteRefList.Count);
        AddressablesLoaderManager.instance.LoadImage(spriteRefList[index], (percentage) => { Debug.Log($"Loading {spriteRefList[index].SubObjectName}: {percentage}"); }, AddSpriteToGame);
    }

    private void HandleLoadMultipleSpriteBtn()
    {
        for (int i = 0; i < spriteRefList.Count; i++)
        {
            int index = i;
            AddressablesLoaderManager.instance.QueLoadImage(spriteRefList[index], (percentage) => { Debug.Log($"Loading {spriteRefList[index].SubObjectName}: {percentage}"); }, AddSpriteToGame);
        }
        AddressablesLoaderManager.instance.StartQueLoadSpriteDownload();
    }
    private void HandleLoadSingleGameObjectBtn()
    {
        int index = Random.Range(0, gameObjectRefList.Count);
        AddressablesLoaderManager.instance.LoadGameObject(gameObjectRefList[index], (percentage) => { Debug.Log($"Loading {gameObjectRefList[index].SubObjectName}: {percentage}"); }, AddGameObjectToGame);
    }

    private void HandleLoadMultipleGameObjectBtn()
    {
        for (int i = 0; i < gameObjectRefList.Count; i++)
        {
            int index = i;
            AddressablesLoaderManager.instance.QueLoadGameObject(gameObjectRefList[index], (percentage) => { Debug.Log($"Loading {gameObjectRefList[index].SubObjectName}: {percentage}"); }, AddGameObjectToGame);
        }
        AddressablesLoaderManager.instance.StartQueLoadGameObjectDownload();
    }

    private void HandleClearAllSpriteGameObjectBtn()
    {
        foreach (Transform item in imgLoadingPlace)
        {
            Destroy(item.gameObject);
        }
        foreach (Transform item in gameObjLoadingPlace)
        {
            Destroy(item.gameObject);
        }
        AddressablesLoaderManager.instance.ClearAll();
    }

    private void AddSpriteToGame(ContentDownloadData contentDownloadBehavior)
    {
        Image image = Instantiate(imgPrefab, imgLoadingPlace);
        Object spriteToLoad = contentDownloadBehavior.Content;
        var type = spriteToLoad.GetType();
        if (spriteToLoad is Sprite)
            image.sprite = (Sprite)spriteToLoad;
        else if (spriteToLoad is Texture2D)
        {
            Texture2D texture = (Texture2D)spriteToLoad;
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100);
        }
        else
        {
            Debug.LogError("Error Casting Image");
        }
        contentDownloadBehavior.OnContentLoaded -= AddSpriteToGame;
    }

    private void AddGameObjectToGame(ContentDownloadData contentDownloadBehavior)
    {
        Object gameObjectToLoad = contentDownloadBehavior.Content;
        Instantiate(gameObjectToLoad, gameObjLoadingPlace);
        contentDownloadBehavior.OnContentLoaded -= AddGameObjectToGame;
    }
}
