using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using AvatarBuild.Config;
using UnityEngine;
using System.Collections;
using System.Text;
using System.Security.Cryptography;

namespace AvatarBuild
{
    public static class Service
    {
        public class RequestCreateFileData
        {
            /// <summary>
            /// 
            /// </summary>
            public string sid { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string md5 { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int size { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string audit_status { get; set; }

            public RequestCreateFileData()
            {

            }
            public RequestCreateFileData(string name, string md5, string type, string status, int size)
            {
                this.sid = "1";
                this.name = name;
                this.size = size;
                this.type = type;
                this.audit_status = status;
                this.md5 = md5;
            }
        }

        public class Matter
        {
            public string id { get; set; }
            public string sid { get; set; }
            public string md5 { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int size { get; set; }
            public string audit_status { get; set; }
            public string @object { get; set; }
            public string url { get; set; }
        }

        public class Data
        {
            public int code { get; set; }
            public string msg { get; set; }
            public Matter matter { get; set; }
            public Dictionary<string, List<string>> headers { get; set; }
            public string up_link { get; set; }
        }

        public class ResponseCreateFile
        {
            /// <summary>
            /// 
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string msg { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Data data { get; set; }

            public UploadAssetResponse Generic()
            {
                return new UploadAssetResponse()
                {
                    code = 200,
                    msg = string.Empty,
                    data = new UploadData()
                    {
                        name = data.matter.name,
                        sid = data.matter.sid,
                        md5 = data.matter.md5,
                        type = data.matter.type,
                        url = data.matter.url,
                        size = data.matter.size,
                    }
                };
            }
        }

        public class UploadData
        {
            /// <summary>
            /// 
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string sid { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string md5 { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int size { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string audit_status { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string @object { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string url { get; set; }
        }

        public class UploadAssetResponse
        {
            /// <summary>
            /// 
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string msg { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public UploadData data { get; set; }
        }

        public class CreateElementData
        {
            /// <summary>
            /// 
            /// </summary>
            public int id { get; set; }
        }

        public class CreateElementResponse
        {
            /// <summary>
            /// 
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string msg { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Data data { get; set; }
        }

        public class ServerElementData
        {
            /// <summary>
            /// 
            /// </summary>
            public int id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int element { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int organization_id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string icon { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string icon_hash { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int publish_status { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int price { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string model { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string texture { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string texture_hash { get; set; }
        }

        public class ElementDataList
        {
            /// <summary>
            /// 
            /// </summary>
            public List<ServerElementData> list { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int max_count { get; set; }
        }

        public class ResponsePublishingElementDatas
        {
            /// <summary>
            /// 
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string msg { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public ElementDataList data { get; set; }
        }

        public static IEnumerator RequestPublishingElementDatas(string address, Action<List<ElementData>, Exception> callback)
        {
            UnityWebRequest request = UnityWebRequest.Get(address + "avatar/api/v1/items?page=1&page_size=100&type=-1");
            yield return request.SendWebRequest();
            if (!request.isDone || request.result != UnityWebRequest.Result.Success)
            {
                callback(null, new Exception(string.Format("{0}\n{1}\n{2}", request.url, request.error, request.downloadHandler.text)));
            }
            else
            {
                ResponsePublishingElementDatas responsePublishingElementDatas = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponsePublishingElementDatas>(request.downloadHandler.text);
                if (responsePublishingElementDatas.code != 200)
                {
                    throw new Exception(responsePublishingElementDatas.msg);
                }
                List<ElementData> elementDatas = new List<ElementData>();
                if (responsePublishingElementDatas.data.list != null)
                {
                    foreach (var item in responsePublishingElementDatas.data.list)
                    {
                        elementDatas.Add(new ElementData()
                        {
                            icon = item.icon,
                            model = item.model,
                            id = item.id.ToString(),
                            element = (Element)item.element,
                            texture = item.texture
                        });
                    }
                }
                callback(elementDatas, null);
            }
        }

        public static IEnumerator CreateElementRequest(string address, ElementData elementData, Action<CreateElementResponse, Exception> callback)
        {
            string postData = Uri.UnescapeDataString(Newtonsoft.Json.JsonConvert.SerializeObject(elementData));
            UnityWebRequest request = UnityWebRequest.Post(new Uri(address + "avatar/api/v1/item/add"), postData);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Access-Control-Request-Method", "*");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            yield return request.SendWebRequest();
            if (!request.isDone || request.result != UnityWebRequest.Result.Success)
            {
                callback(null, new Exception(string.Format("{0}\n{1}\n{2}", request.url, request.error, request.downloadHandler.text)));
            }
            else
            {
                callback(Newtonsoft.Json.JsonConvert.DeserializeObject<CreateElementResponse>(request.downloadHandler.text), null);
            }
        }
        static IEnumerator RequestCreateFile(string address, RequestCreateFileData data, Action<ResponseCreateFile, Exception> callback)
        {
            string postData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            UnityWebRequest request = UnityWebRequest.Post(new Uri(address + "avatar/resource/v1/matter/create"), postData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Access-Control-Request-Method", "*");
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (!request.isDone || request.result != UnityWebRequest.Result.Success)
            {
                callback(null, new Exception(string.Format("{0}\n{1}\n{2}", request.url, request.error, request.downloadHandler.text)));
            }
            else
            {
                callback(Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseCreateFile>(request.downloadHandler.text), null);
            }
        }
        static IEnumerator RequestUploadAssetDone(string address, string fileName, Action<UploadAssetResponse, Exception> callback)
        {
            UnityWebRequest request = UnityWebRequest.Post(new Uri(address + "avatar/resource/v1/matter/" + fileName + "/done"), string.Empty);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (!request.isDone || request.result != UnityWebRequest.Result.Success)
            {
                callback(null, new Exception(string.Format("{0}\n{1}\n{2}", request.url, request.error, request.downloadHandler.text)));
            }
            else
            {
                callback(Newtonsoft.Json.JsonConvert.DeserializeObject<UploadAssetResponse>(request.downloadHandler.text), null);
            }
        }

        /// <summary>
        /// 上传资源
        /// </summary>
        /// <param name="filePath"></param>
        static IEnumerator Upload(byte[] bytes, string url, Dictionary<string, List<string>> headers, Action<Exception> callback = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                callback(new ArgumentException(url));
                yield break;
            }
            if (bytes == null || bytes.Length <= 0)
            {
                callback(new Exception("cannot be upload empty data"));
                yield break;
            }
            UnityWebRequest request = UnityWebRequest.Put(url, bytes);
            request.SetRequestHeaders(headers);
            yield return request.SendWebRequest();
            if (!request.isDone || request.result != UnityWebRequest.Result.Success)
            {
                callback(new Exception(request.error + "\n" + request.downloadHandler.text));
            }
            else
            {
                callback(null);
            }
        }

        public static IEnumerator UploadAsset(string address, RequestCreateFileData requestCreate, byte[] bytes, Action<UploadAssetResponse, Exception> callback)
        {
            yield return RequestCreateFile(address, requestCreate, (response, exception) =>
            {
                if (exception != null)
                {
                    callback(null, exception);
                    return;
                }
                if (string.IsNullOrEmpty(response.data.up_link))
                {
                    callback(response.Generic(), null);
                    return;
                }
                MonoBehaviourInstance.StartCor(Upload(bytes, response.data.up_link, response.data.headers, exception =>
                {
                    if (exception != null)
                    {
                        callback(null, exception);
                        return;
                    }
                    MonoBehaviourInstance.StartCor(RequestUploadAssetDone(address, requestCreate.name, (response, exception) =>
                    {
                        callback(response, exception);
                    }));
                }));
            });
        }

        public static void SetRequestHeaders(this UnityWebRequest request, Dictionary<string, List<string>> headers)
        {
            if (request == null || headers == null || headers.Count <= 0)
            {
                return;
            }
            if (headers != null && headers.Count > 0)
            {
                foreach (var item in headers)
                {
                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        request.SetRequestHeader(item.Key, item.Value[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string GetFileMd5(byte[] bytes)
        {
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }
    }
    public class MonoBehaviourInstance : MonoBehaviour
    {
        private static MonoBehaviourInstance _instance;
        private static MonoBehaviourInstance instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("Mono").AddComponent<MonoBehaviourInstance>();
                }
                return _instance;
            }
        }

        public static void StartCor(IEnumerator enumerator)
        {
            instance.StartCoroutine(enumerator);
        }
    }
}
