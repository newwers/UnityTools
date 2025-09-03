/*
 StreamingAssets
 会将里面文件复制到打包应用下,但是在安卓平台中,只能读,不能写

persistentDataPath
在安卓平台上，persistentDataPath默认指向应用的内部存储目录，通常是/data/data/your.package.name/files。
可读可写,通常存放配置文件夹,ab资源,下载的热更新资源

PlayerPrefs
pc上储存在注册表中
在安卓平台上，PlayerPrefs将数据存储在 Android的SharedPreferences中。


如果是微信小游戏,需要使用微信官方提供的 PlayerPrefs 进行储存
 在微信小游戏中，PlayerPrefs可能不是最佳选择，因为WebGL平台限制，使用Application.persistentDataPath缓存路径来保存文件可能会失败。
一种简单的方式是使用PlayerPrefs来存储内容，但存在读取速度慢的问题。微信小游戏提供了插件来优化PlayerPrefs的使用

persistentDataPath的替代方案是使用WX.env.USER_DATA_PATH

 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;


/// <summary>
/// 数据接口
/// 在需要保存或加载数据的类中实现此接口
/// </summary>
public interface IDataSaver
{
    void OnSave(SaveData saveData);
    void OnLoad(SaveData saveData);
}

/// <summary>
/// 基础数据结构
/// 需要使用Newtonsoft.Json来处理复杂类型的序列化和反序列化
/// 使用包管理器加载 Newtonsoft.Json  地址 com.unity.nuget.newtonsoft-json
/// 需要自己管理所有实现了IDataSaver接口的类,然后调用实现存档和加载的功能
/// </summary>
[System.Serializable]
public class SaveData
{
    public int version = 1; // 版本控制
    public string PlayerName;
    public Dictionary<string, object> data = new Dictionary<string, object>();//通用数据
    public Dictionary<string, BalloonRuntimeData> balloonData = new Dictionary<string, BalloonRuntimeData>();//气球等级经验数据
    public List<SceneItemSaveData> sceneItemSaveData = new List<SceneItemSaveData>();//物体场景中状态数据
    public List<SceneGoldSaveData> sceneGoldSaveData = new List<SceneGoldSaveData>();//物体场景中状态数据
    public Dictionary<string, ItemUnlockData> itemUnlockData = new Dictionary<string, ItemUnlockData>();//物体解锁状态数据
    public Dictionary<string, object> SettingData = new Dictionary<string, object>();//设置数据



    // 添加类型安全的存取方法
    public void Set<T>(string key, T value, Dictionary<string, object> dic = null)
    {
        if (dic == null)
        {
            dic = data;
        }
        dic[key] = value;
    }

    public T Get<T>(string key, T defaultValue = default, Dictionary<string, object> dic = null)
    {
        if (dic == null)
        {
            dic = data;
        }

        if (dic.TryGetValue(key, out var value))
        {
            //UnityEngine.Debug.Log($"Get key: {key}, value: {value}, type: {value.GetType()}");

            // 特殊处理数组类型
            if (typeof(T).IsArray)
            {
                var elementType = typeof(T).GetElementType();

                // 处理 JArray (JSON 数组)
                if (value is Newtonsoft.Json.Linq.JArray jArray)
                {
                    var array = Array.CreateInstance(elementType, jArray.Count);
                    for (int i = 0; i < jArray.Count; i++)
                    {
                        array.SetValue(Convert.ChangeType(jArray[i], elementType), i);
                    }
                    return (T)(object)array;
                }

                // 处理普通数组
                if (value is Array originalArray)
                {
                    var array = Array.CreateInstance(elementType, originalArray.Length);
                    for (int i = 0; i < originalArray.Length; i++)
                    {
                        array.SetValue(Convert.ChangeType(originalArray.GetValue(i), elementType), i);
                    }
                    return (T)(object)array;
                }
            }

            // 普通类型转换
            return (T)Convert.ChangeType(value, typeof(T));
        }

        return defaultValue;
    }

    public Vector2 GetVector2(string key, Vector2 defaultValue = default)
    {
        float[] array = Get<float[]>(key);
        if (array != null && array.Length >= 2)
        {
            return new Vector2(array[0], array[1]);
        }
        return defaultValue;
    }
    public Vector3 GetVector3(string key, Vector3 defaultValue = default)
    {
        float[] array = Get<float[]>(key);
        if (array != null && array.Length >= 2)
        {
            return new Vector3(array[0], array[1], array[2]);
        }
        return defaultValue;
    }

    public void SetVector2(string key, Vector2 value)
    {
        Set(key, new float[] { value.x, value.y });
    }

    public void SetVector3(string key, Vector3 value)
    {
        Set(key, new float[] { value.x, value.y, value.z });
    }

    public void SetBalloonData(Dictionary<string, BalloonRuntimeData> data)
    {
        balloonData = data;
    }

    public Dictionary<string, BalloonRuntimeData> GetBalloonData()
    {
        return balloonData;
    }

    public void SetItemUnlockSaveData(Dictionary<string, ItemUnlockData> data)
    {
        itemUnlockData = data;
    }

    public Dictionary<string, ItemUnlockData> GetItemUnlockSaveData()
    {
        return itemUnlockData;
    }

    public void SetSceneItemSaveData(List<SceneItemSaveData> data)
    {
        sceneItemSaveData = data;
    }
    public List<SceneItemSaveData> GetSceneItemSaveData()
    {
        return sceneItemSaveData;
    }

    public void SetSceneGoldSaveData(List<SceneGoldSaveData> data)
    {
        sceneGoldSaveData = data;
    }
    public List<SceneGoldSaveData> GetSceneGoldSaveData()
    {
        return sceneGoldSaveData;
    }

}


/// <summary>
/// 储存系统,管理 streamingAssetsPath,persistentDataPath,PlayerPrefs的存储和读取
/// 
/// </summary>
public static class StorageSystem
{
    private static readonly int MAX_BACKUP_COUNT = 10; // 最大备份数量
    private static readonly string BACKUP_FOLDER_NAME = "SaveBackups"; // 备份文件夹名称

    // 加密密钥（可以更复杂）
    private static readonly string encryptionKey = "your-encryption-key";

    public static string streamingAssetsPath
    {
        get
        {
            return Application.streamingAssetsPath;
        }
    }

    public static string persistentDataPath
    {
        get
        {
            return Application.persistentDataPath;
        }
    }




    public static void SaveStringToStreamingAssets(string data, string filename)
    {
        string path = Path.Combine(streamingAssetsPath, filename);
        File.WriteAllText(path, data, Encoding.UTF8);
    }

    public static string LoadStringFromStreamingAssets(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);
        if (File.Exists(path))
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }
        return null;
    }

    public static void SaveStringToPersistentDataPath(string data, string filename)
    {
        // 可选：加密
        //data = EncryptDecrypt(data);

        string path = Path.Combine(Application.persistentDataPath, filename);
        Debug.Log($"Saving data to persistent path: {path}");
        File.WriteAllText(path, data, Encoding.UTF8);
    }

    public static string LoadStringFromPersistentDataPath(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (File.Exists(path))
        {
            var data = File.ReadAllText(path, Encoding.UTF8);
            // 可选：解密
            //jsonData = EncryptDecrypt(jsonData);
            return data;
        }
        return null;
    }


    public static void DeleteFileFromPersistentDataPath(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.Delete(path);
    }

    public static void SaveStringToPlayerPrefs(string data, string key)
    {
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();
    }

    public static string LoadStringFromPlayerPrefs(string key)
    {
        return PlayerPrefs.GetString(key, string.Empty);
    }

    public static void SaveIntToPlayerPrefs(int data, string key)
    {
        PlayerPrefs.SetInt(key, data);
        PlayerPrefs.Save();
    }

    public static int LoadIntFromPlayerPrefs(string key)
    {
        return PlayerPrefs.GetInt(key, 0);
    }

    public static string ObjectToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    public static T JsonToObject<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    public static void SaveStringToResources(string data, string fileName)
    {
        string path = Path.Combine(Application.dataPath + "/Resources/", fileName);
        File.WriteAllText(path, data, Encoding.UTF8);
    }

    public static string LoadStringFromResources(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);
        if (textAsset)
        {
            return textAsset.text;
        }

        return null;
    }

    // 简单的加密解密方法（异或加密）
    private static string EncryptDecrypt(string data)
    {
        char[] dataChars = data.ToCharArray();
        for (int i = 0; i < dataChars.Length; i++)
        {
            dataChars[i] = (char)(dataChars[i] ^ encryptionKey[i % encryptionKey.Length]);
        }
        return new string(dataChars);
    }

    // 5. 加密解密
    private static string Encrypt(string input)
    {
        // AES加密实现
        using var aes = new AesManaged();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32, '*'));
        aes.Mode = CipherMode.CBC;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(input);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    private static string Decrypt(string input)
    {
        // AES解密实现
        byte[] buffer = Convert.FromBase64String(input);
        using var aes = new AesManaged();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32, '*'));
        aes.Mode = CipherMode.CBC;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(buffer);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }

    /// <summary>
    /// 获取场景中所有实现了特定接口的组件
    /// 包括激活和未激活的对象
    /// </summary>
    /// <typeparam name="T">接口类型</typeparam>
    public static List<T> FindImplementers<T>(bool includeInactive = true) where T : class
    {
        // 使用Object.FindObjectsByType替代过时的Object.FindObjectsOfType
        var components = UnityEngine.Object.FindObjectsByType<Component>(
            includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        var result = new List<T>();
        foreach (var component in components)
        {
            // 检查组件是否实现了该接口
            if (component is T implementer)
            {
                result.Add(implementer);
            }
        }
        return result;
    }


    /// <summary>
    /// 将persistentDataPath下的文件夹压缩为log.zip并保存到应用程序根目录，完成后自动打开文件夹并选中文件
    /// 改进：处理文件共享冲突问题
    /// </summary>
    /// <returns>是否压缩成功</returns>
    public static bool CompressPersistentDataToLogZip()
    {
        try
        {
            // 获取源文件夹路径
            string sourcePath = Application.persistentDataPath + "/../";

            // 验证源路径是否存在
            if (!Directory.Exists(sourcePath))
            {
                Debug.LogError($"源文件夹不存在: {sourcePath}");
                return false;
            }

            // 创建临时目录用于处理可能被占用的文件
            string tempPath = Path.Combine(Path.GetTempPath(), "LogCompressorTemp_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);

            try
            {
                // 复制文件到临时目录，跳过无法访问的文件
                CopyDirectoryWithExceptionHandling(sourcePath, tempPath);

                // 获取应用程序根目录路径(Windows平台)
                string targetDirectory = Path.GetDirectoryName(Application.dataPath);
                string targetPath = Path.Combine(targetDirectory, "log.zip");

                // 如果已存在同名压缩文件则删除
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                    Debug.Log("已删除现有log.zip文件");
                }

                // 从临时目录创建压缩文件
                ZipFile.CreateFromDirectory(tempPath, targetPath, System.IO.Compression.CompressionLevel.Optimal, true);

                // 验证压缩文件是否创建成功
                if (File.Exists(targetPath))
                {
                    Debug.Log($"压缩成功，文件保存至: {targetPath}");

                    // 打开文件夹并选中文件
                    OpenFolderAndSelectFile(targetPath);

                    return true;
                }
                else
                {
                    Debug.LogError("压缩失败，未生成log.zip文件");
                    return false;
                }
            }
            finally
            {
                // 清理临时目录
                if (Directory.Exists(tempPath))
                {
                    try
                    {
                        Directory.Delete(tempPath, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"清理临时文件失败: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"压缩过程中发生错误: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 复制目录，处理可能的文件访问异常
    /// </summary>
    private static void CopyDirectoryWithExceptionHandling(string sourceDir, string destDir)
    {
        // 创建目标目录
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        // 复制所有文件
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destDir, fileName);

            try
            {
                // 尝试复制文件，使用FileOptions防止锁定
                File.Copy(file, destFile, true);
                Debug.Log($"已复制文件: {fileName}");
            }
            catch (IOException ex) when (ex.Message.Contains("正由另一进程使用") ||
                                        ex.Message.Contains("Sharing violation"))
            {
                Debug.LogWarning($"文件正被使用，已跳过: {fileName} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"复制文件失败 {fileName}: {ex.Message}");
            }
        }

        // 递归复制子目录
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string subDirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destDir, subDirName);
            CopyDirectoryWithExceptionHandling(subDir, destSubDir);
        }
    }

    /// <summary>
    /// 打开文件夹并选中指定文件
    /// </summary>
    /// <param name="filePath">文件完整路径</param>
    private static void OpenFolderAndSelectFile(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                Debug.LogError("文件路径无效或文件不存在");
                return;
            }

            // Windows资源管理器命令：/select 参数用于选中指定文件
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{filePath}\"",
                UseShellExecute = true
            });

            Debug.Log("已打开文件夹并选中文件");
        }
        catch (Exception e)
        {
            Debug.LogError($"打开文件夹时发生错误: {e.Message}");
        }
    }

    #region 备份

    /// <summary>
    /// 备份文件到备份目录
    /// </summary>
    /// <param name="sourceFilePath">源文件路径</param>
    /// <param name="backupFileName">备份文件名(不包含路径)</param>
    /// <returns>是否备份成功</returns>
    public static bool BackupFile(string sourceFilePath, string backupFileName)
    {
        try
        {
            // 确保源文件存在
            if (!File.Exists(sourceFilePath))
            {
                Debug.LogWarning($"无法备份，源文件不存在: {sourceFilePath}");
                return false;
            }

            // 创建备份目录
            string backupDir = Path.Combine(persistentDataPath, BACKUP_FOLDER_NAME);
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            // 生成带时间戳的备份文件名
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFilePath = Path.Combine(backupDir, $"{Path.GetFileNameWithoutExtension(backupFileName)}_{timestamp}{Path.GetExtension(backupFileName)}.backup");

            // 复制文件到备份目录
            File.Copy(sourceFilePath, backupFilePath, true);
            Debug.Log($"已创建备份: {backupFilePath}");

            // 清理旧备份
            CleanupOldBackups(backupDir);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"备份文件时发生错误: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 清理过多的备份文件，只保留最新的MAX_BACKUP_COUNT个
    /// </summary>
    /// <param name="backupDir">备份目录</param>
    private static void CleanupOldBackups(string backupDir)
    {
        try
        {
            if (!Directory.Exists(backupDir))
                return;

            // 获取所有备份文件并按创建时间排序
            var backupFiles = Directory.GetFiles(backupDir, "*.backup")
                                       .Select(f => new FileInfo(f))
                                       .OrderByDescending(f => f.CreationTime)
                                       .ToList();

            // 删除超出数量限制的旧备份
            if (backupFiles.Count > MAX_BACKUP_COUNT)
            {
                for (int i = MAX_BACKUP_COUNT; i < backupFiles.Count; i++)
                {
                    backupFiles[i].Delete();
                    Debug.Log($"已删除旧备份: {backupFiles[i].Name}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"清理备份文件时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 从备份恢复文件
    /// </summary>
    /// <param name="backupFilePath">备份文件路径</param>
    /// <param name="targetFilePath">目标文件路径</param>
    /// <returns>是否恢复成功</returns>
    public static bool RestoreFromBackup(string backupFilePath, string targetFilePath)
    {
        try
        {
            if (!File.Exists(backupFilePath))
            {
                Debug.LogError($"备份文件不存在: {backupFilePath}");
                return false;
            }

            // 确保目标目录存在
            string targetDir = Path.GetDirectoryName(targetFilePath);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // 复制备份文件到目标位置
            File.Copy(backupFilePath, targetFilePath, true);
            Debug.Log($"已从备份恢复: {backupFilePath} -> {targetFilePath}");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"恢复备份时发生错误: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 获取所有可用备份文件
    /// </summary>
    /// <returns>备份文件列表</returns>
    public static List<string> GetAvailableBackups()
    {
        string backupDir = Path.Combine(persistentDataPath, BACKUP_FOLDER_NAME);
        if (!Directory.Exists(backupDir))
            return new List<string>();

        return Directory.GetFiles(backupDir, "*.backup")
                       .OrderByDescending(f => new FileInfo(f).CreationTime)
                       .ToList();
    }

    #endregion
}
