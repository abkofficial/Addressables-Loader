using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace com.microgamedev.AddressablesLoader
{
    public class DownloadContents : MonoBehaviour
    {
        public event Action<float> OnProgressUpdated;
        public event Action<List<ContentDownloadData>> OnContentsLoaded;

        private Dictionary<AssetReference, ContentDownloadBehavior> contentDownloadBehaviorsDic = new();
        private int totalCount = 0;

        public void AddDownloadContent(ContentDownloadBehavior contentDownloadBehavior)
        {
            totalCount++;
            AssetReference AssetKey = contentDownloadBehavior.assetReference;
            if (contentDownloadBehaviorsDic.ContainsKey(AssetKey))
            {
                Debug.LogError($"{nameof(DownloadContents)}: Error downloading Same Asset");
                return;
            }
            else
            {
                contentDownloadBehaviorsDic.Add(AssetKey, contentDownloadBehavior);
            }
        }

        private bool SetAllContent()
        {
            bool allContentReady = true;
            List<AssetReference> removeList = new();
            foreach (KeyValuePair<AssetReference, ContentDownloadBehavior> item in contentDownloadBehaviorsDic)
            {
                if (!item.Value.IsContentLoaded())
                {
                    allContentReady = false;
                    continue;
                }
                item.Value.TriggerContentLoaded();
                removeList.Add(item.Key);
            }

            foreach (var item in removeList)
            {
                contentDownloadBehaviorsDic.Remove(item);
            }

            return allContentReady;
        }

        IEnumerator Start()
        {
            Debug.Log("Downloading..");

            if (SetAllContent())
            {
                Debug.Log("Downloaded.");
                Destroy(this);
                yield break;
            }

            Dictionary<AssetReference, AsyncOperationHandle<Object>> operationDictionary = new Dictionary<AssetReference, AsyncOperationHandle<Object>>();
            foreach (AssetReference assetReference in contentDownloadBehaviorsDic.Keys)
            {
                AsyncOperationHandle<Object> handle = Addressables.LoadAssetAsync<Object>(assetReference);
                operationDictionary.Add(assetReference, handle);
            }

            int readyCount = 0;
            List<ContentDownloadData> contentDownloadDataList = new List<ContentDownloadData>();
            while (readyCount < contentDownloadBehaviorsDic.Count)
            {
                List<AssetReference> removeList = new();
                List<AssetReference> reQueList = new();
                float unCompletePercentage = 0;
                foreach (KeyValuePair<AssetReference, AsyncOperationHandle<Object>> item in operationDictionary)
                {
                    float itemPercentComplete = Mathf.Round((float)item.Value.PercentComplete * 100f);
                    contentDownloadBehaviorsDic[item.Key].progress = itemPercentComplete;
                    contentDownloadBehaviorsDic[item.Key].UpdateProgress();
                    unCompletePercentage += 100f - itemPercentComplete;

                    switch (item.Value.Status)
                    {
                        case AsyncOperationStatus.None:
                            break;
                        case AsyncOperationStatus.Succeeded:
                            contentDownloadBehaviorsDic[item.Key].Content = item.Value.Result;
                            contentDownloadDataList.Add(contentDownloadBehaviorsDic[item.Key]);
                            removeList.Add(item.Key);
                            readyCount++;
                            break;
                        case AsyncOperationStatus.Failed:
                            var exception = item.Value.OperationException;
                            Debug.LogWarning($"Failed To Load {contentDownloadBehaviorsDic[item.Key].assetReference} due to {exception.Message} exception:{JsonUtility.ToJson(exception)} retrying...");
                            reQueList.Add(item.Key);
                            break;
                        default:
                            Debug.LogError($"DownloadContents {contentDownloadBehaviorsDic[item.Key].assetReference} Unexpeted Action");
                            break;
                    }
                }

                foreach (var itemToRemove in removeList)
                {
                    operationDictionary.Remove(itemToRemove);
                    Debug.Log($"DownloadContents {itemToRemove} Completed");
                }

                foreach (var itemToReQue in reQueList)
                {
                    operationDictionary.Remove(itemToReQue);
                    AsyncOperationHandle<Object> handle = Addressables.LoadAssetAsync<Object>(itemToReQue);
                    operationDictionary.Add(itemToReQue, handle);
                }

                float downloadPercentage = ((100f * totalCount) - unCompletePercentage) / totalCount;
                OnProgressUpdated?.Invoke(downloadPercentage);

                yield return null;
            }

            OnContentsLoaded?.Invoke(contentDownloadDataList);

            Debug.Log("Downloaded..");
            Destroy(this);
        }
    }
}
