using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;

namespace TCaptcha
{
    public class CaptchaImageHandler : IHttpHandler
    {
        bool System.Web.IHttpHandler.IsReusable
        {
            get
            {
                return true;
            }
        }

        public CaptchaImageHandler()
        {
        }

        void System.Web.IHttpHandler.ProcessRequest(HttpContext context)
        {
            HttpApplication applicationInstance = context.ApplicationInstance;
            string item = applicationInstance.Request.QueryString["guid"];
            CaptchaImage captchaImage = null;
            if (item != "")
            {
                captchaImage = (!string.IsNullOrEmpty(applicationInstance.Request.QueryString["s"]) ? (CaptchaImage)HttpContext.Current.Session[item] : (CaptchaImage)HttpRuntime.Cache.Get(item));
            }
            if (captchaImage == null)
            {
                applicationInstance.Response.StatusCode = 404;
                context.ApplicationInstance.CompleteRequest();
                return;
            }
            using (Bitmap bitmap = captchaImage.RenderImage())
            {
                bitmap.Save(applicationInstance.Context.Response.OutputStream, ImageFormat.Jpeg);
            }
            applicationInstance.Response.ContentType = "image/jpeg";
            applicationInstance.Response.StatusCode = 200;
            context.ApplicationInstance.CompleteRequest();
        }
    }
}
