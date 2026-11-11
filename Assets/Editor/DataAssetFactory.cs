using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NightBufe.EditorTools
{
    /// <summary>
    /// Gives ScriptableObjects by path. The editor version loads or creates real
    /// assets (keeping GUIDs stable across rebuilds); the in-memory version is used by tests.
    /// </summary>
    public interface IDataAssetFactory
    {
        T Get<T>(string path) where T : ScriptableObject;
    }

    public sealed class InMemoryAssetFactory : IDataAssetFactory
    {
        private readonly Dictionary<string, ScriptableObject> _objects = new Dictionary<string, ScriptableObject>();

        public T Get<T>(string path) where T : ScriptableObject
        {
            if (_objects.TryGetValue(path, out var existing)) return (T)existing;
            var created = ScriptableObject.CreateInstance<T>();
            created.name = Path.GetFileNameWithoutExtension(path);
            _objects[path] = created;
            return created;
        }

        public void DestroyAll()
        {
            foreach (var obj in _objects.Values)
                if (obj != null) Object.DestroyImmediate(obj);
            _objects.Clear();
        }
    }

    public sealed class AssetDatabaseFactory : IDataAssetFactory
    {
        private readonly List<Object> _touched = new List<Object>();

        public T Get<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            _touched.Add(asset);
            return asset;
        }

        public void SaveAll()
        {
            foreach (var obj in _touched)
                if (obj != null) EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
    }
}
