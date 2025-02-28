using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TTSDK
{
    public class TTFileSystemManagerWebGL : TTFileSystemManager
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkWriteStringFileSync(string filePath, string data, string encoding);
#else
        private static string StarkWriteStringFileSync(string filePath, string data, string encoding)
        {
            return TTFileSystemManagerDefault.Instance.WriteFileSync(filePath, data, encoding);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkWriteBinFileSync(string filePath, byte[] data, int dataLength);
#else
        private static string StarkWriteBinFileSync(string filePath, byte[] data, int dataLength)
        {
            return TTFileSystemManagerDefault.Instance.WriteFileSync(filePath, data);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkWriteBinFile(string filePath, byte[] data, int dataLength, string s, string f);
#else
        private static void StarkWriteBinFile(string filePath, byte[] data, int dataLength, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.WriteFileSync(filePath, data);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkWriteStringFile(string filePath, string data, string encoding, string s,
            string f);
#else
        private static void StarkWriteStringFile(string filePath, string data, string encoding, string s,
            string f)
        {
            TTFileSystemManagerDefault.Instance.WriteFileSync(filePath, data, encoding);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkReadFile(string filePath, string encoding, string callbackId);
#else
        private static void StarkReadFile(string filePath, string encoding, string callbackId)
        {
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkReadStringFileSync(string filePath, string encoding);
#else
        private static string StarkReadStringFileSync(string filePath, string encoding)
        {
            return TTFileSystemManagerDefault.Instance.ReadFileSync(filePath, encoding);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int StarkReadBinFileSync(string filePath);
#else
        private static int StarkReadBinFileSync(string filePath)
        {
            return 0;
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkShareFileBuffer(byte[] data, string callbackId);
#else
        private static void StarkShareFileBuffer(byte[] data, string callbackId)
        {
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool StarkAccessFileSync(string filePath);
#else
        private static bool StarkAccessFileSync(string filePath)
        {
            return TTFileSystemManagerDefault.Instance.AccessSync(filePath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkAccessFile(string filePath, string s, string f);
#else
        private static void StarkAccessFile(string filePath, string s, string f)
        {
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkCopyFileSync(string srcPath, string destPath);
#else
        private static string StarkCopyFileSync(string srcPath, string destPath)
        {
            return TTFileSystemManagerDefault.Instance.CopyFileSync(srcPath, destPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkRenameFileSync(string srcPath, string destPath);
#else
        private static string StarkRenameFileSync(string srcPath, string destPath)
        {
            return TTFileSystemManagerDefault.Instance.RenameFileSync(srcPath, destPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkCopyFile(string srcPath, string destPath, string s, string f);
#else
        private static void StarkCopyFile(string srcPath, string destPath, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.CopyFileSync(srcPath, destPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkRenameFile(string srcPath, string destPath, string s, string f);
#else
        private static void StarkRenameFile(string srcPath, string destPath, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.RenameFileSync(srcPath, destPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkUnlinkSync(string filePath);
#else
        private static string StarkUnlinkSync(string filePath)
        {
            return TTFileSystemManagerDefault.Instance.UnlinkSync(filePath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkUnlink(string filePath, string s, string f);
#else
        private static void StarkUnlink(string filePath, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.UnlinkSync(filePath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkMkdir(string dirPath, bool recursive, string s, string f);
#else
        private static void StarkMkdir(string dirPath, bool recursive, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.MkdirSync(dirPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkMkdirSync(string dirPath, bool recursive);
#else
        private static string StarkMkdirSync(string dirPath, bool recursive)
        {
            return TTFileSystemManagerDefault.Instance.MkdirSync(dirPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkRmdir(string dirPath, bool recursive, string s, string f);
#else
        private static void StarkRmdir(string dirPath, bool recursive, string s, string f)
        {
            TTFileSystemManagerDefault.Instance.RmdirSync(dirPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkRmdirSync(string dirPath, bool recursive);
#else
        private static string StarkRmdirSync(string dirPath, bool recursive)
        {
            return TTFileSystemManagerDefault.Instance.RmdirSync(dirPath);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkStatSync(string path);
#else
        private static string StarkStatSync(string path)
        {
            return "";
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkStat(string path, string callbackId);
#else
        private static void StarkStat(string path, string callbackId)
        {
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void StarkGetSavedFileList(string callbackId);
#else
        private static void StarkGetSavedFileList(string callbackId)
        {
        }
#endif
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string StarkGetCachedPathForUrl(string url);
#else
        private static string StarkGetCachedPathForUrl(string url)
        {
            return "";
        }
#endif

        public static readonly TTFileSystemManagerWebGL Instance = new TTFileSystemManagerWebGL();

        private static Dictionary<string, ReadFileParam> s_readFileParams = new Dictionary<string, ReadFileParam>();
        private static Dictionary<string, StatParam> s_statParams = new Dictionary<string, StatParam>();

        private static Dictionary<string, GetSavedFileListParam> s_getSavedFileListParams =
            new Dictionary<string, GetSavedFileListParam>();

        private static bool _initialized;

        public TTFileSystemManagerWebGL()
        {
            MigratingData();
            CreateStarkFileSystemHandler();
        }

        private void CreateStarkFileSystemHandler()
        {
            if (!_initialized)
            {
                _initialized = true;
                GameObject obj = new GameObject();
                Object.DontDestroyOnLoad(obj);
                obj.name = "StarkFileSystemManager";
                obj.AddComponent<TTFileSystemHandler>();
            }
        }

        public class TTFileSystemHandler : MonoBehaviour
        {
            public void HandleNativeCallback(string msg)
            {
                Debug.Log($"HandleNativeCallback - {msg}");
                TTCallbackHandler.InvokeResponseCallback<TTBaseResponse>(msg);
            }

            public void HandleReadFileCallback(string msg)
            {
                Debug.Log($"HandleReadFileCallback - {msg}");
                var res = JsonUtility.FromJson<TTReadFileCallback>(msg);
                var conf = s_readFileParams[res.callbackId];
                if (conf == null)
                {
                    Debug.LogWarning($"HandleReadFileCallback - no callback for callbackId: {res.callbackId}");
                    return;
                }

                s_readFileParams.Remove(res.callbackId);

                if (res.errCode == 0)
                {
                    if (string.IsNullOrEmpty(conf.encoding) || conf.encoding.Equals("binary"))
                    {
                        var sharedBuffer = new byte[res.byteLength];
                        StarkShareFileBuffer(sharedBuffer, res.callbackId);
                        var obj = new TTReadFileResponse()
                        {
                            binData = sharedBuffer,
                        };
                        conf.success?.Invoke(obj);
                    }
                    else
                    {
                        var obj = new TTReadFileResponse()
                        {
                            stringData = res.data
                        };
                        conf.success?.Invoke(obj);
                    }
                }
                else
                {
                    var obj = new TTReadFileResponse()
                    {
                        errCode = res.errCode,
                        errMsg = res.errMsg
                    };
                    conf.fail?.Invoke(obj);
                }
            }

            public void HandleStatCallback(string msg)
            {
                Debug.Log($"HandleStatCallback - {msg}");
                TTStatResponse res;
                try
                {
                    res = JsonMapper.ToObject<TTStatResponse>(msg);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"failed to parse json data: {msg}, {exception}");
                    return;
                }
                if (res == null)
                {
                    Debug.LogError("empty response");
                    return;
                }
                var conf = s_statParams[res.callbackId];
                if (conf == null)
                {
                    Debug.LogWarning($"HandleStatCallback - no callback for callbackId: {res.callbackId}");
                    return;
                }

                s_statParams.Remove(res.callbackId);

                if (res.errCode == 0)
                {
                    if (res.stat == null)
                    {
                        Debug.LogWarning("empty stat info");
                        res.stat = new TTStatInfo();
                    }
                    conf.success?.Invoke(res);
                }
                else
                {
                    res.stat = new TTStatInfo();
                    conf.fail?.Invoke(res);
                }
            }

            public void HandleGetSavedFileListCallback(string msg)
            {
                Debug.Log($"HandleGetSavedFileListCallback - {msg}");
                TTGetSavedFileListResponse res;
                try
                {
                    res = JsonMapper.ToObject<TTGetSavedFileListResponse>(msg);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"failed to parse json data: {msg}, {exception}");
                    return;
                }

                if (res == null)
                {
                    Debug.LogError("empty response");
                    return;
                }
                var conf = s_getSavedFileListParams[res.callbackId];
                if (conf == null)
                {
                    Debug.LogWarning($"HandleStatCallback - no callback for callbackId: {res.callbackId}");
                    return;
                }

                s_statParams.Remove(res.callbackId);

                if (res.errCode == 0)
                {
                    if (res.fileList == null)
                    {
                        res.fileList = new TTFileInfo[0];
                    }

                    conf.success?.Invoke(res);
                }
                else
                {
                    res.fileList = new TTFileInfo[0];
                    conf.fail?.Invoke(res);
                }
            }
        }

        private string FixFilePath(string filePath)
        {
            if (filePath.StartsWith(USER_DATA_PATH))
            {
                return filePath;
            }

            if (filePath.StartsWith(Application.persistentDataPath))
            {
                filePath = filePath.Replace(Application.persistentDataPath, USER_DATA_PATH);
            }
            else
            {
                if (filePath.StartsWith("/"))
                {
                    filePath = filePath.Substring(1);
                }

                filePath = $"{USER_DATA_PATH}/{filePath}";
            }

            return filePath;
        }

        /// <summary>
        /// 将字符串写入文件（同步）
        /// </summary>
        /// <param name="filePath">要写入的文件路径</param>
        /// <param name="data">要写入的文本</param>
        /// <param name="encoding">指定写入文件的字符编码</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string WriteFileSync(string filePath, string data, string encoding = "utf8")
        {
            if (string.IsNullOrEmpty(encoding))
            {
                encoding = "utf8";
            }

            return StarkWriteStringFileSync(FixFilePath(filePath), data, encoding);
        }

        /// <summary>
        /// 将二进制写入文件（同步）
        /// </summary>
        /// <param name="filePath">要写入的文件路径</param>
        /// <param name="data">要写入的二进制数据</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string WriteFileSync(string filePath, byte[] data)
        {
            return StarkWriteBinFileSync(FixFilePath(filePath), data, data.Length);
        }

        /// <summary>
        /// 将二进制写入文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void WriteFile(WriteFileParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkWriteBinFile(
                FixFilePath(param.filePath),
                param.data,
                param.data.Length,
                pair.success,
                pair.fail
            );
        }

        /// <summary>
        /// 将字符串写入文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void WriteFile(WriteFileStringParam param)
        {
            if (string.IsNullOrEmpty(param.encoding))
            {
                param.encoding = "utf8";
            }

            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkWriteStringFile(
                FixFilePath(param.filePath),
                param.data,
                param.encoding,
                pair.success,
                pair.fail
            );
        }

        /// <summary>
        /// 读取本地文件内容（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void ReadFile(ReadFileParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            s_readFileParams.Add(key, param);
            StarkReadFile(FixFilePath(param.filePath), param.encoding, key);
        }

        /// <summary>
        /// 从本地文件读取二进制数据数据（同步）
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>字节数据，读取失败返回null</returns>
        public override byte[] ReadFileSync(string filePath)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer ||
                (Application.version.Contains("t") && (int)Application.platform == 0x00000032))
            {
                filePath = FixFilePath(filePath);
                var length = StarkReadBinFileSync(filePath);
                if (length == 0)
                {
                    return null;
                }

                var sharedBuffer = new byte[length];
                StarkShareFileBuffer(sharedBuffer, filePath);
                return sharedBuffer;
            }
            else
            {
                return System.IO.File.ReadAllBytes(filePath);
            }
        }

        /// <summary>
        /// 从本地文件读取字符串数据（同步）
        /// </summary>
        /// <param name="filePath">要读取的文件的路径</param>
        /// <param name="encoding">指定读取文件的字符编码, 不能为空</param>
        /// <returns>字符串数据，读取失败返回null</returns>
        public override string ReadFileSync(string filePath, string encoding)
        {
            return StarkReadStringFileSync(FixFilePath(filePath), encoding);
        }

        /// <summary>
        /// 判断文件/目录是否存在（同步）
        /// </summary>
        /// <param name="path">要判断是否存在的文件/目录路径</param>
        /// <returns>成功返回 true, 失败返回 false</returns>
        public override bool AccessSync(string path)
        {
            return StarkAccessFileSync(FixFilePath(path));
        }

        /// <summary>
        /// 判断文件/目录是否存在（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Access(AccessParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkAccessFile(FixFilePath(param.path), pair.success, pair.fail);
        }

        /// <summary>
        /// 复制文件（同步） 
        /// </summary>
        /// <param name="srcPath">源文件路径</param>
        /// <param name="destPath">目标文件路径</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string CopyFileSync(string srcPath, string destPath)
        {
            return StarkCopyFileSync(FixFilePath(srcPath), FixFilePath(destPath));
        }

        /// <summary>
        /// 复制文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void CopyFile(CopyFileParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkCopyFile(FixFilePath(param.srcPath), FixFilePath(param.destPath), pair.success, pair.fail);
        }

        /// <summary>
        /// 重命名文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void RenameFile(RenameFileParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkRenameFile(FixFilePath(param.srcPath), FixFilePath(param.destPath), pair.success, pair.fail);
        }

        /// <summary>
        /// 重命名文件（同步）
        /// </summary>
        /// <param name="srcPath">源文件路径</param>
        /// <param name="destPath">目标文件路径</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string RenameFileSync(string srcPath, string destPath)
        {
            return StarkRenameFileSync(FixFilePath(srcPath), FixFilePath(destPath));
        }

        /// <summary>
        /// 删除文件（同步）
        /// </summary>
        /// <param name="filePath">源文件路径，支持本地路径</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string UnlinkSync(string filePath)
        {
            return StarkUnlinkSync(FixFilePath(filePath));
        }

        /// <summary>
        /// 删除文件（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Unlink(UnlinkParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkUnlink(FixFilePath(param.filePath), pair.success, pair.fail);
        }

        /// <summary>
        /// 创建目录（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Mkdir(MkdirParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkMkdir(FixFilePath(param.dirPath), param.recursive, pair.success, pair.fail);
        }

        /// <summary>
        /// 创建目录（同步）
        /// </summary>
        /// <param name="dirPath">创建的目录路径</param>
        /// <param name="recursive">是否在递归创建该目录的上级目录后再创建该目录。如果对应的上级目录已经存在，则不创建该上级目录。如 dirPath 为 a/b/c/d 且 recursive 为 true，将创建 a 目录，再在 a 目录下创建 b 目录，以此类推直至创建 a/b/c 目录下的 d 目录。</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string MkdirSync(string dirPath, bool recursive = false)
        {
            return StarkMkdirSync(FixFilePath(dirPath), recursive);
        }

        /// <summary>
        /// 删除目录（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Rmdir(RmdirParam param)
        {
            var pair = TTCallbackHandler.AddPair(param.success, param.fail);
            StarkRmdir(FixFilePath(param.dirPath), param.recursive, pair.success, pair.fail);
        }

        /// <summary>
        /// 删除目录（同步）
        /// </summary>
        /// <param name="dirPath">创建的目录路径</param>
        /// <param name="recursive">是否递归删除目录。如果为 true，则删除该目录和该目录下的所有子目录以及文件	。</param>
        /// <returns>成功返回空字符串，失败返回错误信息</returns>
        public override string RmdirSync(string dirPath, bool recursive = false)
        {
            return StarkRmdirSync(FixFilePath(dirPath), recursive);
        }

        /// <summary>
        /// 读取文件描述信息（同步）
        /// </summary>
        /// <param name="path">文件/目录路径</param>
        /// <param name="recursive">是否递归获取目录下的每个文件的 Stat 信息	</param>
        /// <param name="throwException">是否抛出错误信息，如果抛出错误信息，当文件不存在时则会抛出异常，错误信息从异常中获取。</param>
        /// <returns>返回文件信息，如果访问失败则返回null</returns>
        public override TTStatInfo StatSync(string path, bool throwException = false)
        {
            var info = StarkStatSync(FixFilePath(path));
            try
            {
                return JsonUtility.FromJson<TTStatInfo>(info);
            }
            catch (Exception exception)
            {
                if (throwException)
                {
                    if (string.IsNullOrEmpty(info))
                    {
                        info = "stat failed";
                    }

                    throw new Exception(info);
                }

                return null;
            }
        }

        /// <summary>
        /// 读取文件描述信息（异步）
        /// </summary>
        /// <param name="param"></param>
        public override void Stat(StatParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            s_statParams.Add(key, param);
            StarkStat(FixFilePath(param.path), key);
        }

        /// <summary>
        /// 获取保存的用户目录文件列表
        /// </summary>
        /// <param name="param"></param>
        public override void GetSavedFileList(GetSavedFileListParam param)
        {
            var key = TTCallbackHandler.MakeKey();
            s_getSavedFileListParams.Add(key, param);
            StarkGetSavedFileList(key);
        }

        /// <summary>
        /// 根据url链接获取本地缓存文件路径
        /// </summary>
        /// <param name="url">输入文件下载链接url</param>
        /// <returns>返回本地缓存文件路径，以scfile://user开头的路径，可以直接用这个路径访问该文件</returns>
        public override string GetLocalCachedPathForUrl(string url)
        {
            return StarkGetCachedPathForUrl(url);
        }

        /// <summary>
        /// 判断该url是否有本地缓存文件
        /// </summary>
        /// <param name="url">输入文件下载链接url</param>
        /// <returns>如果存在缓存文件则返回true，不存在缓存文件则返回false</returns>
        public override bool IsUrlCached(string url)
        {
            var path = GetLocalCachedPathForUrl(url);
            return !string.IsNullOrEmpty(path) && AccessSync(path);
        }
    }
}