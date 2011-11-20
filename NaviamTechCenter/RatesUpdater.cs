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
using System.Globalization;
using System.Configuration;

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

        private static log4net.ILog logger = Program.logger;

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
        private static void TimeoutCallbackSendResult(object state, bool timedOut)
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
                    IAsyncResult asynchronousResult = responseStream.BeginRead(requestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBackSendResult), requestState);
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
                logger.Error("update rates RespCallback exception raised!", e);
            }
            allDone2.Set();
        }
        
        //This method is called by the timer delegate.
        public void Update(Object stateInfo)
        {
            try
            {
                logger.Info(string.Format("start update rates..."));
                string url = ConfigurationManager.AppSettings["nbrb_get_absent_dates_url"];
                int days = int.Parse(ConfigurationManager.AppSettings["nbrb_rates_update_days"]);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Format(@"{0}?daysCount={1}&countryId={2}", url, days, COUNTRY_ID));

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

        public static void GetRates(string absentDays)
        {
            //TODO: why get "ok" here?
            ExRatesSoapClient client = null;
            DataSet cursies = null;
            List<string> dates = new List<string>();
            try
            {
                dates = (List<string>)SerializationHelper.FromXmlString(typeof(List<string>), absentDays);
                if (dates != null && dates.Count > 0)
                {
                    logger.Info(string.Format("absent days count {0}", dates.Count));
                    client = new ExRatesSoapClient();
                    List<CurrRate> result = new List<CurrRate>();
                    foreach (string date in dates)
                    {
                        cursies = client.ExRatesDaily(DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture));
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
                else
                    logger.Info(string.Format("absent days count {0}", dates.Count));
            }
            catch (Exception ex)
            {
                logger.Error("get rates error", ex);
            }
        }

        public static void SendResult(List<CurrRate> rates)
        {
            try
            {
                string url = ConfigurationManager.AppSettings["nbrb_get_absent_dates_url"];
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(string.Format(@"{0}",url));
                JavaScriptSerializer js = new JavaScriptSerializer();
                String content = js.Serialize(rates);
                httpWebRequest.ContentType = "application/json";
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
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallbackSendResult), httpWebRequest, DefaultTimeout, true);

                // The response came in the allowed time. The work processing will happen in the callback function.
                allDone2.WaitOne();

                // Release the HttpWebResponse resource.
                if (requestState != null && requestState.response != null)
                    requestState.response.Close();
            }
            catch (Exception ex)
            {
                logger.Error("update database with rates error", ex);
            }
        }

    }

    [Serializable]
    public class CurrRate
    {
        public CurrRate()
        {

        }
        public CurrRate(string date, string currCode, decimal rateVal, int countryId)
        {
            Date = date;
            CurrCode = currCode;
            RateVal = rateVal;
            CountryId = countryId;
        }

        public string Date { get; set; }
        public string CurrCode { get; set; }
        public decimal RateVal { get; set; }
        public int CountryId { get; set; }
    }
}
