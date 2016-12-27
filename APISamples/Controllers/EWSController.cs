using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Exchange.WebServices.Data;

namespace APISamples.Controllers
{
    public class EWSController : Controller
    {

      String email1;
      String pass1;

    public ActionResult Index() {
      ViewBag.Email = Environment.GetEnvironmentVariable("email1");
      return View();
    }


    [HttpPost]
    public ActionResult Index(String x) {
      email1 = Environment.GetEnvironmentVariable("email1");
      pass1 = Environment.GetEnvironmentVariable("pass1");

      ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);

      service.Credentials = new WebCredentials(email1, pass1);
      service.UseDefaultCredentials = false;
      //service.TraceEnabled = true;
      //service.TraceFlags = TraceFlags.All;
      service.AutodiscoverUrl(email1, RedirectionUrlValidationCallback);

      sendEmail(service, Request);
      ViewBag.Message = "Email sent.";
      ViewBag.Email = email1;

      return View();
    }
    private static void sendEmail(ExchangeService service, HttpRequestBase Request) {
      EmailMessage email = new EmailMessage(service);
      email.ToRecipients.Add(Request.Form["recipients"]);
      email.Subject =  Request.Form["subject"];
      email.Body = new MessageBody("This is the first email I've sent by using the EWS Managed API.");
      email.Send();
    }

    private static bool RedirectionUrlValidationCallback(string redirectionUrl) {
      // The default for the validation callback is to reject the URL.
      bool result = false;
      Uri redirectionUri = new Uri(redirectionUrl);
      // Validate the contents of the redirection URL. In this simple validation
      // callback, the redirection URL is considered valid if it is using HTTPS
      // to encrypt the authentication credentials. 
      if (redirectionUri.Scheme == "https") {
        result = true;
      }
      return result;
    }

   
  }
}