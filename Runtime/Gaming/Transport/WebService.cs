namespace Gaming.Transport
{
    using System;
    using System.Collections.Generic;
    using UnityEngine.Networking;
    using System.Collections;
    using System.Text;
    using UnityEngine.Scripting;
    using Gaming.Config;
    using Gaming.Avatar;

    public static class WebService
    {
        public class RequestCreateFileData
        {
            public string sid;
            public string name;
            public string md5;
            public int size;
            public string type;
            public string audit_status;
            [Preserve]
            public RequestCreateFileData()
            {

            }
            [Preserve]
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
            public string id;
            public string sid;
            public string md5;
            public string name;
            public string type;
            public int size;
            public string audit_status;
            public string @object;
            public string url;
            [Preserve]
            public Matter() { }
        }

        public class Data
        {
            public int code;
            public string msg;
            public Matter matter;
            public Dictionary<string, List<string>> headers;
            public string up_link;
            [Preserve]
            public Data() { }
        }

        public class ResponseCreateFile
        {
            public int code;
            public string msg;
            public Data data;

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
            [Preserve]
            public ResponseCreateFile() { }
        }

        public class UploadData
        {
            public string id;
            public string sid;
            public string md5;
            public string name;
            public string type;
            public int size;
            public string audit_status;
            public string @object;
            public string url;
            [Preserve]
            public UploadData() { }
        }

        public class UploadAssetResponse
        {
            public int code;
            public string msg;
            public UploadData data;
            [Preserve]
            public UploadAssetResponse() { }
        }

        public class CreateElementData
        {
            public int id;
            [Preserve]
            public CreateElementData() { }
        }

        public class CreateElementResponse
        {
            public int code;
            public string msg;
            public Data data;
            [Preserve]
            public CreateElementResponse() { }
        }

        public class ServerElementData
        {
            public int id;
            public int element;
            public int organization_id;
            public string name;
            public string icon;
            public string icon_hash;
            public int publish_status;
            public int price;
            public string model;
            public string texture;
            public string texture_hash;
            [Preserve]
            public ServerElementData() { }
        }

        public class ElementDataList
        {
            public List<ServerElementData> list;
            public int max_count;
            [Preserve]
            public ElementDataList() { }
        }

        public class ResponsePublishingElementDatas
        {
            public int code;
            public string msg;
            public ElementDataList data;
            [Preserve]
            public ResponsePublishingElementDatas() { }
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

        }

        public static IEnumerator UploadAsset(string address, RequestCreateFileData requestCreate, byte[] bytes, Action<UploadAssetResponse, Exception> callback)
        {
            string postData = Newtonsoft.Json.JsonConvert.SerializeObject(requestCreate);
            UnityWebRequest request = UnityWebRequest.Post(new Uri(address + "avatar/resource/v1/matter/create"), postData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.SetRequestHeader("Access-Control-Request-Method", "*");
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (!request.isDone || request.result != UnityWebRequest.Result.Success)
            {
                callback(null, new Exception(request.error + "\n" + request.downloadHandler.text));
                yield break;
            }

            ResponseCreateFile responseCreateFile = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseCreateFile>(request.downloadHandler.text);
            request = UnityWebRequest.Put(responseCreateFile.data.up_link, bytes);
            request.SetRequestHeaders(responseCreateFile.data.headers);
            yield return request.SendWebRequest();
            if (!request.isDone || request.result != UnityWebRequest.Result.Success)
            {
                callback(null, new Exception(request.error + "\n" + request.downloadHandler.text));
                yield break;
            }

            request = UnityWebRequest.Post(new Uri(address + "avatar/resource/v1/matter/" + requestCreate.name + "/done"), string.Empty);
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
    }
}
