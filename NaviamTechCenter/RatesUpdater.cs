using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NBRBService.NBRBServiceReference;
using System.Data;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace NaviamTechCenter
{
    class RatesUpdater
    {
        private const int COUNTRY_ID = 1;
        private object lockObject = new object();

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static ManualResetEvent allDone2 = new ManualResetEvent(false);
        const int DefaultTimeout = 2 * 60 * 1000; // 2 minutes timeout
        const int BUFFER_SIZE = 1024;

        private static log4net.ILog logger = log4net.LogManager.GetLogger("navTechCenter");

        private static List<string> neededCurrenciesCodes = new List<string> { "840", "978", "985", "428", "440", "643" };

        public RatesUpdater()
        {

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

                // Begin the Reading of the contents of the HTML page and print it to the console.
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
                        GetRates(stringContent);
                    }
                    responseStream.Close();
                }
            }
            catch (WebException e)
            {
                logger.Error("RespCallback Exception raised!", e);
            }
            allDone.Set();
        }
        private static void RespCallbackSendResult(IAsyncResult asynchronousResult)
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

                // Begin the Reading of the contents of the HTML page and print it to the console.
                IAsyncResult asynchronousInputRead = responseStream.BeginRead(requestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBackSendResult), requestState);
                return;
            }
            catch (WebException e)
            {
                logger.Error("RespCallbackSendResult Exception raised!", e);
            }
            allDone2.Set();
        }
        private static void ReadCallBackSendResult(IAsyncResult asyncResult)
        {
            try
            {
                RequestState requestState = (RequestState)asyncResult.AsyncState;
                Stream responseStream = requestState.streamResponse;
                int read = responseStream.EndRead(asyncResult);
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
                        logger.Info(string.Format("ReadCallBackSendResult = {0}", stringContent));
                    }
                    responseStream.Close();
                }
            }
            catch (WebException e)
            {
                logger.Error("RespCallback Exception raised!", e);
            }
            allDone2.Set();
        }
        //This method is called by the timer delegate.
        public void Update(Object stateInfo)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(@"http://localhost:54345/Tech/ListDates?daysCount=10&countryId=1");

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
            //Console.WriteLine("{0} Update rate status {1}.", DateTime.Now.ToString("h:mm:ss.fff"), date.ToString());
        }

        public static void GetRates(string absentDays)
        {
            ExRatesSoapClient client = null;
            DataSet cursies = null;

            List<DateTime> dates = new List<DateTime>();
            dates = (List<DateTime>)SerializationHelper.FromXmlString(typeof(List<DateTime>), absentDays);
            if (dates != null && dates.Count > 0)
            {
                client = new ExRatesSoapClient();
                List<CurrRate> result = new List<CurrRate>();
                foreach (DateTime date in dates)
                {
                    cursies = client.ExRatesDaily(date);
                    if (cursies != null)
                    {
                        foreach (DataRow row in cursies.Tables[0].Rows)
                        {
                            string cuurCode = row[3] as string;
                            if (neededCurrenciesCodes.Contains(cuurCode))
                            {
                                result.Add(new CurrRate(date, cuurCode, (decimal)row[2], COUNTRY_ID));
                            }
                        }
                    }
                }
                client.Close();
                SendResult(result);
            }
        }

        public static void SendResult(List<CurrRate> rates)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(@"http://localhost:54345/Tech/UpdateRates");
            JavaScriptSerializer js = new JavaScriptSerializer();
            String content = js.Serialize(rates);
            httpWebRequest.ContentType = "application/json";// "application/xml; charset=utf-8";
            httpWebRequest.Method = "POST";
            byte[] byteData = UTF8Encoding.UTF8.GetBytes(content);
            httpWebRequest.ContentLength = byteData.Length;
            using (Stream postStream = httpWebRequest.GetRequestStream())
            {
                postStream.Write(byteData, 0, byteData.Length);
            }

            RequestState requestState = new RequestState();
            requestState.request = httpWebRequest;
            IAsyncResult result = (IAsyncResult)httpWebRequest.BeginGetResponse(new AsyncCallback(RespCallbackSendResult), requestState);
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), httpWebRequest, DefaultTimeout, true);

            // The response came in the allowed time. The work processing will happen in the callback function.
            allDone2.WaitOne();

            // Release the HttpWebResponse resource.
            if (requestState != null && requestState.response != null)
                requestState.response.Close();
        }

    }

    public class RequestState
    {
        // This class stores the State of the request.
        const int BUFFER_SIZE = 1024;
        public StringBuilder requestData;
        public byte[] BufferRead;
        public HttpWebRequest request;
        public HttpWebResponse response;
        public Stream streamResponse;
        public RequestState()
        {
            BufferRead = new byte[BUFFER_SIZE];
            requestData = new StringBuilder("");
            request = null;
            streamResponse = null;
        }
    }

    [Serializable]
    public class CurrRate
    {
        public CurrRate()
        {

        }
        public CurrRate(DateTime date, string currCode, decimal rateVal, int countryId)
        {
            Date = date;
            CurrCode = currCode;
            RateVal = rateVal;
            CountryId = countryId;
        }

        public DateTime Date { get; set; }
        public string CurrCode { get; set; }
        public decimal RateVal { get; set; }
        public int CountryId { get; set; }
    }
}
