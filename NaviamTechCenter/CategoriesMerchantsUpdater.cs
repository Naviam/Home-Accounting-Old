using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Configuration;
using System.IO;

namespace NaviamTechCenter
{
    class CategoriesMerchantsUpdater
    {
        private static log4net.ILog logger = Program.logger;

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        const int DefaultTimeout = 2 * 60 * 1000; // 2 minutes timeout
        const int BUFFER_SIZE = 1024;
        
        public CategoriesMerchantsUpdater()
        {

        }

        // This method is called by the timer delegate.
        public void Update(Object stateInfo)
        {
            try
            {
                logger.Info(string.Format("start update categories..."));
                string url = ConfigurationManager.AppSettings["cat_update_url"];
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Format(@"{0}", url));

                RequestState requestState = new RequestState();
                requestState.request = httpWebRequest;
                IAsyncResult result = (IAsyncResult)httpWebRequest.BeginGetResponse(new AsyncCallback(RespCallback), requestState);
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), httpWebRequest, DefaultTimeout, true);

                // The response came in the allowed time. The work processing will happen in the callback function.
                allDone.WaitOne();

                // Release the HttpWebResponse resource.
                if (requestState != null && requestState.response != null)
                    requestState.response.Close();

                AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            }
            catch (Exception ex)
            {
                logger.Error("update rates error", ex);
            }
        }

        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                {
                    request.Abort();
                }
            }
        }
        private static void RespCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                // State of request is asynchronous.
                RequestState requestState = (RequestState)asynchronousResult.AsyncState;
                HttpWebRequest httpWebRequest = requestState.request;
                requestState.response = (HttpWebResponse)httpWebRequest.EndGetResponse(asynchronousResult);

                // Read the response into a Stream object.
                Stream responseStream = requestState.response.GetResponseStream();
                requestState.streamResponse = responseStream;

                // Begin the Reading .
                IAsyncResult asynchronousInputRead = responseStream.BeginRead(requestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), requestState);
                return;
            }
            catch (WebException e)
            {
                logger.Error("RespCallback Exception raised!", e);
            }
            allDone.Set();
        }

        private static void ReadCallBack(IAsyncResult asyncResult)
        {
            try
            {
                RequestState requestState = (RequestState)asyncResult.AsyncState;
                Stream responseStream = requestState.streamResponse;
                int read = responseStream.EndRead(asyncResult);
                // Read the HTML page and then print it to the console.
                if (read > 0)
                {
                    requestState.requestData.Append(Encoding.ASCII.GetString(requestState.BufferRead, 0, read));
                    IAsyncResult asynchronousResult = responseStream.BeginRead(requestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), requestState);
                    return;
                }
                else
                {
                    if (requestState.requestData.Length > 1)
                    {
                        string stringContent;
                        stringContent = requestState.requestData.ToString();
                        logger.Info(string.Format("update categories status = {0}", stringContent));
                    }
                    responseStream.Close();
                }
            }
            catch (WebException e)
            {
                logger.Error("update categories RespCallback exception raised!", e);
            }
            allDone.Set();
        }
    }
}
