using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; 
using UnityEngine;
using UnityEngine.Networking; 

namespace DVAH
{
    public class RequestBase
    {
 
        protected UnityWebRequest request;

        public RequestBase(string endPoin)
        {
            this.request = UnityWebRequest.Get(endPoin);
        }

        public RequestBase(string endPoin, string postData, RequestType type = RequestType.POST)
        {
            if (!string.IsNullOrEmpty(postData))
            {

                this.request = new UnityWebRequest(endPoin, type.ToString());
                byte[] bodyRaw = Encoding.UTF8.GetBytes(postData);
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            }
            else
                this.request = UnityWebRequest.Post(endPoin, "");

        }


        public RequestBase setHeader(string name, string value)
        {
            this.request.SetRequestHeader(name, value);
            return this;
        }


        public async Task<RequestBase> Send(System.Action<RequestBase> onDone = null, System.Action<RequestBase> onFail = null)
        {
            //Debug.LogError(this.request.uri+ " ---- ");

            this.request.SetRequestHeader("Content-Type", "application/json");
            this.request.SetRequestHeader("Accept", "application/json");

            this.request.SendWebRequest();

            float timer = 0;
            while (!this.request.isDone && timer < 240000)
            {
                await Task.Delay(10);
                timer += 10;
            }
             

            if (this.request.result == UnityWebRequest.Result.Success)
            {
                if (onDone != null)
                {
                    try
                    {
                        onDone(this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("request--> " + this.request.uri + "---- invoke done error: \n " + e);
                    }

                }
            }
            else
            {
                Debug.LogError("send request--> " + this.request.uri
                    + "---- FAIL!!! \n " + this.request.result.ToString() + "\n " + this.request.error);

                if (onFail != null)
                {
                    try
                    {
                        onFail(this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("request--> " + this.request.uri + "---- invoke fail error: \n " + e);
                    }

                }
            }

            return this;
        }

        public string getResHeader(string key) => this.request.GetResponseHeader(key);

        public bool isDone => this.request.isDone;

        public float progress => this.request.downloadProgress;

        public string access_token => this.getResHeader("x-access-token");

        public string response => this.request.downloadHandler.text;

        public long responseCode => this.request.responseCode;

        public enum RequestType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
    }

}