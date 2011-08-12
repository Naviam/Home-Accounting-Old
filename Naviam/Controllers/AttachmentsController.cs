using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Naviam.WebUI.Controllers
{
    public class AttachmentsController : BaseController
    {

        [HttpPost]
        public string UploadStatement()
        {
            //throw new NotImplementedException();
            var result = "error";
            if (Request.Files.Count > 0)
            {
                var file = Request.Files["fileToUpload"];
                if (file != null && !String.IsNullOrEmpty(file.FileName))
                {
                }
            }
            return result;
        }

    }
}
