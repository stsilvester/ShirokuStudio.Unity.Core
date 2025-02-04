using ShirokuStudio.Core;
using ShirokuStudio.Core.Models;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ShirokuStudio.Editor
{
    public static class AssetDatabaseUtility
    {
        static AssetDatabaseUtility()
        {
            SerializableObjectReference.CreateFromObjectDelegate = obj => TryGetReference(obj, out var result) ? result : default;
        }

        public static string GetGUID(UnityEngine.Object obj)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
        }

        public static T FindAndLoad<T>(string filter)
            where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets(filter);
            if (guids.Length == 0)
                return default;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static T LoadAssetByGUID<T>(string guid)
            where T : UnityEngine.Object
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static void CreateFolderIfNotExists(string path)
        {
            var folderPath = Path.GetDirectoryName(path).Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                var pathParts = folderPath.Split('/');
                var currentPath = "";
                foreach (string part in pathParts)
                {
                    var newPath = string.IsNullOrWhiteSpace(currentPath)
                        ? part : currentPath + "/" + part;
                    if (!AssetDatabase.IsValidFolder(newPath))
                        AssetDatabase.CreateFolder(currentPath, part);
                    currentPath = newPath;
                }
            }
        }

        public static void CreateAsset(UnityEngine.Object asset, string path)
        {
            CreateFolderIfNotExists(path);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        public static T LoadOrCreateAssetAtPath<T>(string path)
            where T : UnityEngine.Object, new()
        {
            return LoadOrCreateAssetAtPath(path, typeof(T)) as T;
        }

        public static UnityEngine.Object LoadOrCreateAssetAtPath(string path, Type type)
        {
            var asset = AssetDatabase.LoadAssetAtPath(path, type);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type) == false)
                throw new ArgumentException($"type {type.FullName} is a UnityEngine.Object.");

            if (asset == null)
            {
                CreateFolderIfNotExists(path);
                if (typeof(ScriptableObject).IsAssignableFrom(type))
                    asset = ScriptableObject.CreateInstance(type);
                else
                    asset = Activator.CreateInstance(type) as UnityEngine.Object;

                try
                {
                    AssetDatabase.CreateAsset(asset, path);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"建立資源失敗 {new { path, asset }}");
                    throw ex;
                }
                AssetDatabase.SaveAssets();
            }

            return asset;
        }

        #region Object Provider

        public static ObjectProvider GetProvider(UnityEngine.Object obj)
        {
            if (obj == null)
                return null;

            GameObject go = null;
            if (obj is GameObject val)
                go = val;
            else if (obj is Component cmp)
                go = cmp.gameObject;

            if (go == null)
                return null;

            var provider = default(ObjectProvider);

            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
                if (prefab == null)
                    return null;

                provider = prefab.GetOrAddComponent<ObjectProvider>();

                return provider;
            }
            else
            {
                var root = go.scene.GetRootGameObjects()
                    .Select(g => g.GetComponent<ObjectProvider>())
                    .FirstOrDefault(p => p != null);

                if (root == null)
                {
                    var goRoot = new GameObject("[ObjectProvider]");
                    provider = Undo.AddComponent<ObjectProvider>(goRoot);
                }
                else
                {
                    provider = root;
                }
            }

            return provider;
        }

        public const string NAME_OBJECT_PROVIDER = "[ObjectProvider]";

        public static bool TryGetReference(this UnityEngine.Object obj, out SerializableObjectReference r)
        {
            r = default;
            var go = obj is GameObject val ? val
                : obj is Component cmp ? cmp.gameObject : null;
            var rootGo = default(GameObject);
            var rootGUID = "";
            var provider = default(ObjectProvider);
            var rootType = SerializableObjectReference.RootTypes.Scene;

            if (go == null)
                return false;

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            //TEST required
            if (prefabStage != null)
            {
                //Prefab
                rootGo = prefabStage.prefabContentsRoot;
                rootGUID = AssetDatabase.AssetPathToGUID(prefabStage.assetPath);
                rootType = SerializableObjectReference.RootTypes.Prefab;

                provider = rootGo.GetComponent<ObjectProvider>();
                if (provider == false)
                {
                    provider = Undo.AddComponent<ObjectProvider>(rootGo);
                    EditorUtility.SetDirty(provider);
                }
            }
            else
            {
                //Scene
                var scene = go.scene;
                rootType = SerializableObjectReference.RootTypes.Scene;
                rootGUID = AssetDatabase.AssetPathToGUID(scene.path);
                rootGo = GameObject.Find(NAME_OBJECT_PROVIDER);
                if (rootGo == false)
                {
                    rootGo = new GameObject(NAME_OBJECT_PROVIDER);
                    EditorUtility.SetDirty(rootGo);
                }

                provider = rootGo.GetComponent<ObjectProvider>();
                if (provider == false)
                {
                    provider = Undo.AddComponent<ObjectProvider>(rootGo);
                    EditorUtility.SetDirty(provider);
                }
            }

            if (rootGo && provider)
            {
                var id = provider.SetObject(obj);
                r = new SerializableObjectReference(id, rootGUID, rootType, obj);
            }

            return r.IsValid;
        }

        public static UnityEngine.Object GetObject(this SerializableObjectReference reference)
        {
            var rootGo = default(GameObject);
            var rootGUID = "";
            var provider = default(ObjectProvider);
            var rootType = SerializableObjectReference.RootTypes.Scene;

            if (reference.IsValid == false)
                return null;

            if (reference.rootType == SerializableObjectReference.RootTypes.Scene)
            {
                var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                var path = AssetDatabase.GUIDToAssetPath(reference.rootGuid);
                if (currentScene.path != path)
                    return null;

                rootGo = GameObject.Find(NAME_OBJECT_PROVIDER);
                if (rootGo == null)
                    return null;

                provider = rootGo.GetComponent<ObjectProvider>();
                if (provider == null)
                    return null;
            }
            else if (reference.rootType == SerializableObjectReference.RootTypes.Prefab)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(reference.rootGuid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage?.assetPath == assetPath)
                    provider = prefabStage.prefabContentsRoot.GetComponent<ObjectProvider>();
                else
                    provider = prefab.GetComponent<ObjectProvider>();

                if (provider == null)
                    return null;
            }

            return provider.GetObject<UnityEngine.Object>(reference.ID);
        }

        public static SerializableObjectReference RefreshReference(this SerializableObjectReference reference)
        {
            if (reference.IsValid == false
                && reference.Value == true
                && reference.Value.TryGetReference(out var r))
                return r;

            if (reference.IsValid == false)
                return reference;

            var obj = reference.GetObject();
            if (obj == null)
                return reference;

            reference.Value = obj;
            reference.Name = obj.name;
            reference.Type = obj.GetType();

            return reference;
        }

        #endregion
    }
}