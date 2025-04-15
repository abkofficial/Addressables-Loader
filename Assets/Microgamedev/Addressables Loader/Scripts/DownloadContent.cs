using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace com.microgamedev.AddressablesLoader
{
    public class DownloadContent : MonoBehaviour
    {
        private ContentDownloadBehavior contentDownloadBehavior;

        public void AddDownloadContent(ContentDownloadBehavior contentDownloadBehavior)
        {
            this.contentDownloadBehavior = contentDownloadBehavior;
        }

        IEnumerator Start()
        {
            Debug.Log("Downloading..");

            if (contentDownloadBehavior.IsContentLoaded())
            {
                contentDownloadBehavior.TriggerContentLoaded();
                Debug.Log("Downloaded.");
                Destroy(this);
                yield break;
            }

            AsyncOperationHandle<Object> asyncOperationHandle = Addressables.LoadAssetAsync<Object>(contentDownloadBehavior.assetReference);

            while (asyncOperationHandle.Status == AsyncOperationStatus.None)
            {
                contentDownloadBehavior.progress = (int)Mathf.Round((float)asyncOperationHandle.PercentComplete * 100f);
                contentDownloadBehavior.UpdateProgress();
                yield return null;
            }

            if (asyncOperationHandle.Status == AsyncOperationStatus.Failed)
            {
                var exception = asyncOperationHandle.OperationException;
                contentDownloadBehavior.progress = 0;
                contentDownloadBehavior.UpdateProgress();
                Debug.LogWarning($"Failed To Load {contentDownloadBehavior.assetReference} due to {exception.Message} exception:{JsonUtility.ToJson(exception)} retrying...");
                StartCoroutine(nameof(Start));
                yield break;
            }

            contentDownloadBehavior.progress = 100;
            contentDownloadBehavior.UpdateProgress();

            contentDownloadBehavior.Content = asyncOperationHandle.Result;

            Debug.Log("Downloaded..");
            Destroy(this);
        }
    }
}
