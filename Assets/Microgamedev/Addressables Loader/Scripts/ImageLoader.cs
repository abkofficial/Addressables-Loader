using com.microgamedev.AddressablesLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageLoader : MonoBehaviour
{
    [SerializeField] Image imageComponent;
    [SerializeField] TextMeshProUGUI loadingText;
    [SerializeField] string Prefix = "Loading: ";
    [SerializeField] string Suffix = "%";
    [SerializeField] Color loadingColor;
    [SerializeField] Color loadedColor = Color.white;
    [SerializeField] AssetReference imageToLoad;
    private void Awake()
    {
        AddressablesLoaderManager.instance.LoadImage(imageToLoad, OnImageLoading, OnImageLoaded);

        imageComponent.color = loadingColor;
    }

    private void OnImageLoading(int arg1, string arg2)
    {
        if (loadingText == null)
        {
            Debug.Log($"{Prefix}{arg1}{Suffix}");
            return;
        }

        loadingText.text = $"{Prefix}{arg1}{Suffix}";
    }

    private void OnImageLoaded(ContentDownloadData contentDownloadData)
    {
        throw new NotImplementedException();
    }

    private IEnumerator Co_EnableImage()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            Color newColor = Color.Lerp(imageComponent.color, loadedColor, Time.deltaTime);
            imageComponent.color = newColor;
        }
    }
}
