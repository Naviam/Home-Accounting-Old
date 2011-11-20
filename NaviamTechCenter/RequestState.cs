using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace NaviamTechCenter
{
    /// <summary>
    /// This class stores the State of the request.
    /// </summary>
    public class RequestState
    {

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
}
