using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickSearch
{

    public interface ISearchableElement
    {
        string PrimaryContents { get; }
        string SecondaryContents { get; }
        float Priority { get; }

        Texture2D Icon { get; }
        string Title { get; }
        string Description { get; }

        void Execute();
        void Select();
    }

    public interface ISearchIndexer
    {
        void CollectIndexes();
        void OnQuery(string query);
        List<ISearchableElement> GetElements();
    }

    public sealed class AssetSearchableElement : ISearchableElement
    {

        private readonly float priority_ = 1f;
        private readonly string assetPath_ = null;
        private readonly string assetName_ = null;

        public AssetSearchableElement (string assetPath)
        {
            assetPath_ = assetPath;
            assetName_ = Path.GetFileNameWithoutExtension(assetPath);
        }

        string ISearchableElement.Description
        {
            get
            {
                return "Asset";
            }
        }

        Texture2D ISearchableElement.Icon
        {
            get
            {
                return null;
            }
        }

        string ISearchableElement.PrimaryContents
        {
            get
            {
                return assetName_;
            }
        }

        string ISearchableElement.Title
        {
            get
            {
                return assetName_;
            }
        }

        string ISearchableElement.SecondaryContents
        {
            get
            {
                return assetPath_;
            }
        }

        float ISearchableElement.Priority
        {
            get
            {
                return priority_;
            }
        }

        void ISearchableElement.Execute()
        {

        }

        void ISearchableElement.Select()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath_);
            EditorUtility.FocusProjectWindow();
        }
    }

    public sealed class AssetIndexer : ISearchIndexer
    {
        private List<ISearchableElement> elements_ = null;

        void ISearchIndexer.CollectIndexes()
        {
            var assetPaths = AssetDatabase.GetAllAssetPaths();
            elements_ = new List<ISearchableElement>(assetPaths.Length);

            for (var i = 0; i < assetPaths.Length; ++i)
            {
                var assetPath = assetPaths[i];

                // Ignore non-project assets
                if (assetPath.StartsWith("Assets/") == false)
                    continue;

                var assetElement = new AssetSearchableElement(assetPath);
                elements_.Add(assetElement);
            }
        }

        void ISearchIndexer.OnQuery(string query)
        {
            // do nothing
        }

        List<ISearchableElement> ISearchIndexer.GetElements()
        {
            return elements_;
        }
    }

}