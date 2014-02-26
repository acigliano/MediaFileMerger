<%@ WebService Language="C#" Class="WebService1" %>
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Services;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class WebService1 : System.Web.Services.WebService 
{
    public UpdaterService()
    {
    }

    [WebMethod]
    public string Update(string appName, Int32 presentationID, string fileLocation, Boolean bSuccess)
    {
        return bSuccess.ToString();
    }
}
