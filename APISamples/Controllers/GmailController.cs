using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
 


namespace APISamples.Controllers
{
  public class Folder
  {
    public String Label { get; set; }
    public int Count { get; set; }
  }

  public class GmailController : Controller
  {
    static string[] Scopes = { GmailService.Scope.GmailReadonly };
    static string ApplicationName = "Gmail API .NET Quickstart";


    public ActionResult Index()
    {
     var folders = getFolders();
      ViewBag.folders = folders;
      return View();
    }
    [HttpPost]
    public ActionResult Index(String x) {
      var folders = getFolders();
      ViewBag.folders = folders;
      return View();
    }
    static List<Folder> getFolders() {
      UserCredential credential;
      var client_secret = Environment.GetEnvironmentVariable("client_secret.json");

      using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(client_secret))) {
        //use the stream here and don't worry about needing to close it

        string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        credPath = Path.Combine(credPath, ".credentials/gmail-dotnet-quickstart.json");

        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
          GoogleClientSecrets.Load(ms).Secrets,
          Scopes,
          "user",
          CancellationToken.None,
          new FileDataStore(credPath, true)).Result;
        Console.WriteLine("Credential file saved to: " + credPath);

      }
      // Create Gmail API service.
      var service = new GmailService(new BaseClientService.Initializer() {
        HttpClientInitializer = credential,
        ApplicationName = ApplicationName,
      });

      // Define parameters of request.
      UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

      UsersResource.GetProfileRequest req = service.Users.GetProfile("me");
      var msgs = req.Execute();

      // List labels.
      IList<Label> labels = request.Execute().Labels;
      List<Folder> folders = new List<Folder>();
    

      Console.WriteLine("Labels:");
      if (labels != null && labels.Count > 0) {
        foreach (var labelItem in labels) {
          UsersResource.LabelsResource.GetRequest getReq = service.Users.Labels.Get("me", labelItem.Id);
          var getr = getReq.Execute();

          folders.Add(new Folder { Label = labelItem.Name, Count = (int) getr.MessagesTotal});

          Console.WriteLine("{0}({1})", labelItem.Name, getr.MessagesTotal);
        }
      } else {
        Console.WriteLine("No labels found.");
      }

      return  (folders);
    }

  
  }
}