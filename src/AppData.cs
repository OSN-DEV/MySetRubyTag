using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;

namespace MySetRubyTag {
    public abstract class AppData {

        #region Declaration
        //[NonSerialized()]
        //        protected static T _instance = default(T);
        [Serializable]
        public class Pair<TK, TV> {
            public TK Key;
            public TV Value;

            public Pair() { }
            public Pair(KeyValuePair<TK, TV> pair) {
                Key = pair.Key;
                Value = pair.Value;
            }
        }
        #endregion

        #region Constructor
        public AppData() { }
        #endregion

        #region Public Property
        public Dictionary<string, SaveData> RubyData { set; get; }

        //[System.Xml.Serialization.XmlIgnore]
        //public AppData Instance {
        //    get {
        //        if (_instance == null)
        //            _instance = new AppData();
        //        return _instance;
        //    }
        //    set { _instance = value; }
        //}
        #endregion

        #region Public Method
        /// <summary>
        /// save setting data
        /// </summary>
        public abstract void Save();

        ///// <summary>
        ///// load setting data
        ///// </summary>
        //public abstract void Load();
        #endregion

        #region Protected Method
        /// <summary>
        /// load settings from xml
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="type">type</param>
        /// <returns>instance</returns>
        protected static object LoadFromXml(string filePath, Type type) {
            if (!File.Exists(filePath)) {
                return null;
            }
            object instance = null;
            using (var reader = new StreamReader(filePath, new UTF8Encoding(false))) {
                var serializer = new System.Xml.Serialization.XmlSerializer(type);
                instance = serializer.Deserialize(reader);
            }
            return instance;
        }

        /// <summary>
        /// load from xml
        /// </summary>
        /// <typeparam name="TK">type of key</typeparam>
        /// <typeparam name="TV">type of value</typeparam>
        /// <param name="filePath">file path</param>
        /// <returns>instance</returns>
        public static Dictionary<TK, TV> LoadFromXml<TK, TV>(string filePath) {
            if (!File.Exists(filePath)) {
                return null;
            }

            List<Pair<TK, TV>> obj = null;
                using (var reader = new StreamReader(filePath, new UTF8Encoding(false))) {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Pair<TK, TV>>));
                    obj = (List<Pair<TK, TV>>)serializer.Deserialize(reader);
                }
            return ConvertListToDictionary(obj);
        }

        /// <summary>
        /// save settings to xml
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="instance">instance</param>
        protected static void SaveToXml(string filePath, object instance) {
            using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(false))) {
                var seralizer = new System.Xml.Serialization.XmlSerializer(instance.GetType());
                seralizer.Serialize(writer, instance);
            }
        }

        /// <summary>
        /// save setting to xml
        /// </summary>
        /// <typeparam name="TK">type of key</typeparam>
        /// <typeparam name="TV">type of value</typeparam>
        /// <param name="filePath">file path</param>
        /// <param name="dictionary">save data</param>
        public static void SaveToXml<TK, TV>(string filePath, Dictionary<TK, TV> dictionary) {
            List<Pair<TK, TV>> list = ConvertDictionaryToList(dictionary);
            using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(false))) {
                var seralizer = new System.Xml.Serialization.XmlSerializer(typeof(List<Pair<TK, TV>>));
                seralizer.Serialize(writer, list);
            }
        }

        /// <summary>
        /// load settings from binary
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns>instance</returns>
        protected static object LoadFromBinary(string filePath) {
            object instance = null;
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                var binaryFormatter = new BinaryFormatter();
                instance = binaryFormatter.Deserialize(fileStream);
            }
            return instance;
        }

        /// <summary>
        /// save settings to binary
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <param name="instance">instance</param>
        protected static void SaveToBinary(string filePath, object instance) {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, instance);
            }
        }

        /// <summary>
        /// load settings from registry
        /// </summary>
        /// <param name="key">registry key</param>
        protected static object LoadFromRegistry(RegistryKey key) {
            object instance = null;
            var binaryFormatter = new BinaryFormatter();
            using (var registry = key) {
                byte[] byteData = (byte[])registry.GetValue("");
                using (var stream = new MemoryStream(byteData, false)) {
                    instance = binaryFormatter.Deserialize(stream);
                }
            }
            return instance;
        }

        /// <summary>
        /// save settings to registry
        /// </summary>
        /// <param name="key">registry key</param>
        /// /// <param name="instance">instance</param>
        protected static void SaveToRegistry(RegistryKey key, object instance) {
            using (var stream = new MemoryStream()) {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, instance);
                using (var registry = key) {
                    registry.SetValue("", stream.ToArray());
                }
            }
        }

        ///// <summary>
        ///// get setttings file path
        ///// </summary>
        ///// <returns>file path</returns>
        //protected virtual string GetPath() {
        //    //string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        //    //    Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductName + ".config");
        //    string path = Application.StartupPath + "\\" + Application.ProductName + ".config";
        //    return path;
        //}

        ///// <summary>
        ///// get registry key
        ///// </summary>
        ///// <returns>registry key</returns>
        //protected virtual RegistryKey GetRegistryKey() {
        //    RegistryKey reg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName);
        //    return reg;
        //}

        /// <summary>
        /// convert dictionary to list
        /// </summary>
        /// <typeparam name="TK">type of key</typeparam>
        /// <typeparam name="TV">type of value</typeparam>
        /// <param name="dictionary">convert target</param>
        /// <returns>list</returns>
        public static List<Pair<TK, TV>> ConvertDictionaryToList<TK, TV>(Dictionary<TK, TV> dictionary) {
            List<Pair<TK, TV>> list = new List<Pair<TK, TV>>();
            foreach (KeyValuePair<TK, TV> pair in dictionary) {
                list.Add(new Pair<TK, TV>(pair));
            }
            return list;
        }

        /// <summary>
        /// convert list to dictionary
        /// </summary>
        /// <typeparam name="TK">type of key</typeparam>
        /// <typeparam name="TV">type of value</typeparam>
        /// <param name="list">convert target</param>
        /// <returns>dictionary</returns>
        public static Dictionary<TK, TV>ConvertListToDictionary<TK, TV>(List<Pair<TK, TV>> list) {
            Dictionary<TK, TV> dic = new Dictionary<TK, TV>();
            foreach (Pair<TK, TV> pair in list) {
                dic.Add(pair.Key, pair.Value);
            }
            return dic;
        }
        #endregion
    }
}
