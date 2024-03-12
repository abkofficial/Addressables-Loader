using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace com.microgamedev.AddressablesLoader
{
    public class AddressablesLoaderManager : MonoBehaviour
    {
        public static AddressablesLoaderManager instance;

        private Dictionary<AssetReference, ContentDownloadBehavior> SpritePreDownloadQue = new();
        private Dictionary<AssetReference, ContentDownloadBehavior> SpriteDownloadQue = new();
        private Dictionary<AssetReference, ContentDownloadBehavior> SpriteInMemory = new();

        private Dictionary<AssetReference, ContentDownloadBehavior> GameObjectPreDownloadQue = new();
        private Dictionary<AssetReference, ContentDownloadBehavior> GameObjectDownloadQue = new();
        private Dictionary<AssetReference, ContentDownloadBehavior> GameObjectInMemory = new();

        private void Start()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            instance = this;
        }

        #region SpriteLoadFunctions
        public void LoadImage(AssetReference assetReference, Action<float> OnProgressUpdated, Action<ContentDownloadData> OnContentLoaded)
        {
            if (TriggerLoaddedSpriteIfInList(assetReference, OnProgressUpdated, OnContentLoaded)) return;

            ContentDownloadBehavior contentDownloadBehavior = new ContentDownloadBehavior();
            contentDownloadBehavior.assetReference = assetReference;
            contentDownloadBehavior.OnProgressUpdated += OnProgressUpdated;
            contentDownloadBehavior.OnContentLoaded += OnContentLoaded;
            contentDownloadBehavior.OnContentLoaded += HandleQuedImageLoaded;

            DownloadContent spriteDownloader = gameObject.AddComponent<DownloadContent>();
            spriteDownloader.AddDownloadContent(contentDownloadBehavior);

            SpriteDownloadQue.Add(assetReference, contentDownloadBehavior);
        }

        public void QueLoadImage(AssetReference assetReference, Action<float> OnProgressUpdated, Action<ContentDownloadData> OnContentLoaded)
        {
            if (TriggerLoaddedSpriteIfInList(assetReference, OnProgressUpdated, OnContentLoaded)) return;

            ContentDownloadBehavior contentDownloadBehavior = new ContentDownloadBehavior();
            contentDownloadBehavior.assetReference = assetReference;
            contentDownloadBehavior.OnProgressUpdated += OnProgressUpdated;
            contentDownloadBehavior.OnContentLoaded += OnContentLoaded;
            contentDownloadBehavior.OnContentLoaded += HandleQuedImageLoaded;

            SpritePreDownloadQue.Add(assetReference, contentDownloadBehavior);
        }

        private void HandleQuedImageLoaded(ContentDownloadData behavior)
        {
            AssetReference key = behavior.assetReference;
            if (SpriteDownloadQue.ContainsKey(key) && !SpriteInMemory.ContainsKey(key))
            {
                ContentDownloadBehavior removed = SpriteDownloadQue[key];
                SpriteDownloadQue.Remove(key);
                SpriteInMemory.Add(key, removed);
                behavior.OnContentLoaded -= HandleQuedImageLoaded;
                behavior.OnContentNull += HandleQuedImageUnLoaded;
            }
        }

        private void HandleQuedImageUnLoaded(ContentDownloadData behavior)
        {
            AssetReference key = behavior.assetReference;
            if (SpriteInMemory.ContainsKey(key) && !SpriteDownloadQue.ContainsKey(key))
            {
                ContentDownloadBehavior removed = SpriteInMemory[key];
                SpriteInMemory.Remove(key);
                SpriteDownloadQue.Add(key, removed);
                behavior.OnContentLoaded += HandleQuedImageLoaded;
                behavior.OnContentNull -= HandleQuedImageUnLoaded;
            }
        }

        public void StartQueLoadSpriteDownload(Action<float> OnProgressUpdated = null, Action<List<ContentDownloadData>> OnContentsLoaded = null)
        {
            if (SpritePreDownloadQue.Count == 0) return;

            DownloadContents spriteDownloader = gameObject.AddComponent<DownloadContents>();
            if (OnProgressUpdated != null)
                spriteDownloader.OnProgressUpdated += OnProgressUpdated;
            if (OnContentsLoaded != null)
                spriteDownloader.OnContentsLoaded += OnContentsLoaded;
            foreach (KeyValuePair<AssetReference, ContentDownloadBehavior> SpritePreDownloadQueItem in SpritePreDownloadQue)
            {
                spriteDownloader.AddDownloadContent(SpritePreDownloadQueItem.Value);
                bool isContentLoaded = SpritePreDownloadQueItem.Value.IsContentLoaded();
                if (isContentLoaded)
                {
                    SpriteInMemory.Add(SpritePreDownloadQueItem.Key, SpritePreDownloadQueItem.Value);
                }
                else
                {
                    SpriteDownloadQue.Add(SpritePreDownloadQueItem.Key, SpritePreDownloadQueItem.Value);
                }
            }

            SpritePreDownloadQue.Clear();
        }

        private bool TriggerLoaddedSpriteIfInList(AssetReference assetReference, Action<float> OnProgressUpdated, Action<ContentDownloadData> OnContentLoaded)
        {

            if (SpritePreDownloadQue.ContainsKey(assetReference))
            {
                SpritePreDownloadQue[assetReference].OnProgressUpdated += OnProgressUpdated;
                SpritePreDownloadQue[assetReference].OnContentLoaded += OnContentLoaded;
                return true;
            }
            else if (SpriteDownloadQue.ContainsKey(assetReference))
            {
                SpriteDownloadQue[assetReference].OnProgressUpdated += OnProgressUpdated;
                SpriteDownloadQue[assetReference].OnContentLoaded += OnContentLoaded;
                return true;
            }
            else if (SpriteInMemory.ContainsKey(assetReference))
            {
                SpriteInMemory[assetReference].OnProgressUpdated += OnProgressUpdated;
                SpriteInMemory[assetReference].OnContentLoaded += OnContentLoaded;
                SpriteInMemory[assetReference].TriggerContentLoaded();
                return true;
            }
            return false;
        }
        #endregion SpriteLoadFunctions

        #region GameObjectLoadFunctions
        public void LoadGameObject(AssetReference assetReference, Action<float> OnProgressUpdated, Action<ContentDownloadData> OnContentLoaded)
        {
            if (TriggerLoaddedGameObjectIfInList(assetReference, OnProgressUpdated, OnContentLoaded)) return;

            ContentDownloadBehavior contentDownloadBehavior = new ContentDownloadBehavior();
            contentDownloadBehavior.assetReference = assetReference;
            contentDownloadBehavior.OnProgressUpdated += OnProgressUpdated;
            contentDownloadBehavior.OnContentLoaded += OnContentLoaded;
            contentDownloadBehavior.OnContentLoaded += HandleQuedGameObjectLoaded;

            DownloadContent GameObjectDownloader = gameObject.AddComponent<DownloadContent>();
            GameObjectDownloader.AddDownloadContent(contentDownloadBehavior);

            GameObjectDownloadQue.Add(assetReference, contentDownloadBehavior);
        }

        public void QueLoadGameObject(AssetReference assetReference, Action<float> OnProgressUpdated, Action<ContentDownloadData> OnContentLoaded)
        {
            if (TriggerLoaddedGameObjectIfInList(assetReference, OnProgressUpdated, OnContentLoaded)) return;

            ContentDownloadBehavior contentDownloadBehavior = new ContentDownloadBehavior();
            contentDownloadBehavior.assetReference = assetReference;
            contentDownloadBehavior.OnProgressUpdated += OnProgressUpdated;
            contentDownloadBehavior.OnContentLoaded += OnContentLoaded;
            contentDownloadBehavior.OnContentLoaded += HandleQuedGameObjectLoaded;

            GameObjectPreDownloadQue.Add(assetReference, contentDownloadBehavior);
        }

        private void HandleQuedGameObjectLoaded(ContentDownloadData behavior)
        {
            AssetReference key = behavior.assetReference;
            if (GameObjectDownloadQue.ContainsKey(key) && !GameObjectInMemory.ContainsKey(key))
            {
                ContentDownloadBehavior removed = GameObjectDownloadQue[key];
                GameObjectDownloadQue.Remove(key);
                GameObjectInMemory.Add(key, removed);
                behavior.OnContentLoaded -= HandleQuedGameObjectLoaded;
                behavior.OnContentNull += HandleQuedGameObjectUnLoaded;
            }
        }

        private void HandleQuedGameObjectUnLoaded(ContentDownloadData behavior)
        {
            AssetReference key = behavior.assetReference;
            if (GameObjectInMemory.ContainsKey(key) && !GameObjectDownloadQue.ContainsKey(key))
            {
                ContentDownloadBehavior removed = GameObjectInMemory[key];
                GameObjectInMemory.Remove(key);
                GameObjectDownloadQue.Add(key, removed);
                behavior.OnContentLoaded += HandleQuedGameObjectLoaded;
                behavior.OnContentNull -= HandleQuedGameObjectUnLoaded;
            }
        }

        public void StartQueLoadGameObjectDownload(Action<float> OnProgressUpdated = null, Action<List<ContentDownloadData>> OnContentsLoaded = null)
        {
            if (GameObjectPreDownloadQue.Count == 0) return;

            DownloadContents GameObjectDownloader = gameObject.AddComponent<DownloadContents>();
            if (OnProgressUpdated != null)
                GameObjectDownloader.OnProgressUpdated += OnProgressUpdated;
            if (OnContentsLoaded != null)
                GameObjectDownloader.OnContentsLoaded += OnContentsLoaded;
            foreach (KeyValuePair<AssetReference, ContentDownloadBehavior> GameObjectPreDownloadQueItem in GameObjectPreDownloadQue)
            {
                GameObjectDownloader.AddDownloadContent(GameObjectPreDownloadQueItem.Value);
                bool isContentLoaded = GameObjectPreDownloadQueItem.Value.IsContentLoaded();
                if (isContentLoaded)
                {
                    GameObjectInMemory.Add(GameObjectPreDownloadQueItem.Key, GameObjectPreDownloadQueItem.Value);
                }
                else
                {
                    GameObjectDownloadQue.Add(GameObjectPreDownloadQueItem.Key, GameObjectPreDownloadQueItem.Value);
                }
            }

            GameObjectPreDownloadQue.Clear();
        }

        private bool TriggerLoaddedGameObjectIfInList(AssetReference assetReference, Action<float> OnProgressUpdated, Action<ContentDownloadData> OnContentLoaded)
        {

            if (GameObjectPreDownloadQue.ContainsKey(assetReference))
            {
                GameObjectPreDownloadQue[assetReference].OnProgressUpdated += OnProgressUpdated;
                GameObjectPreDownloadQue[assetReference].OnContentLoaded += OnContentLoaded;
                return true;
            }
            if (GameObjectDownloadQue.ContainsKey(assetReference))
            {
                GameObjectDownloadQue[assetReference].OnProgressUpdated += OnProgressUpdated;
                GameObjectDownloadQue[assetReference].OnContentLoaded += OnContentLoaded;
                return true;
            }
            if (GameObjectInMemory.ContainsKey(assetReference))
            {
                GameObjectInMemory[assetReference].OnProgressUpdated += OnProgressUpdated;
                GameObjectInMemory[assetReference].OnContentLoaded += OnContentLoaded;
                GameObjectInMemory[assetReference].TriggerContentLoaded();
                return true;
            }
            return false;
        }
        #endregion GameObjectLoadFunctions

        private void OnDestroy()
        {
            ClearAll();
        }

        public void ClearAll()
        {
            GameObjectPreDownloadQue.Clear();
            GameObjectDownloadQue.Clear();
            GameObjectInMemory.Clear();
            SpritePreDownloadQue.Clear();
            SpriteDownloadQue.Clear();
            SpriteInMemory.Clear();
        }

        public void ClearFromMemory(AssetReference assetReference)
        {
            if (GameObjectInMemory.ContainsKey(assetReference))
            {
                GameObjectInMemory.Remove(assetReference);
                assetReference.ReleaseAsset();
            }
            else if (SpriteInMemory.ContainsKey(assetReference))
            {
                SpriteInMemory.Remove(assetReference);
                assetReference.ReleaseAsset();
            }
        }
    }

    public class ContentDownloadBehavior : ContentDownloadData
    {
        public new bool IsContentLoaded()
        {
            return base.IsContentLoaded();
        }
        public new void TriggerContentLoaded()
        {
            base.TriggerContentLoaded();
        }

        public new void UpdateProgress()
        {
            base.UpdateProgress();
        }
    }

    public class ContentDownloadData
    {
        public event Action<ContentDownloadData> OnContentLoaded;
        public event Action<ContentDownloadData> OnContentNull;
        public event Action<float> OnProgressUpdated;

        public float progress;

        public AssetReference assetReference;
        private Object _content;
        public Object Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;

                if (IsContentLoaded())
                {
                    OnContentLoaded?.Invoke(this);
                }
            }
        }

        protected bool IsContentLoaded()
        {
            if (_content == null && assetReference.Asset == null) return false;
            else if (_content == null && assetReference.Asset != null) _content = assetReference.Asset;

            return true;
        }

        protected void TriggerContentLoaded()
        {
            if (_content == null && assetReference.Asset == null)
            {
                OnContentNull?.Invoke(this);
                return;
            }
            else if (_content == null && assetReference.Asset != null) _content = assetReference.Asset;

            OnContentLoaded?.Invoke(this);
            return;
        }

        protected void UpdateProgress()
        {
            OnProgressUpdated?.Invoke(progress);

            if (progress == 100f)
            {
                TriggerContentLoaded();
            }
        }
    }
}

