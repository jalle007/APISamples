using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Salesforce.Common;
using Salesforce.Force;
using System.Threading.Tasks;
using System.Dynamic;

namespace APISamples.Controllers
{
    public class SalesforceController : Controller
    {
    private static readonly string SecurityToken = ConfigurationManager.AppSettings["SecurityToken"];
    private static readonly string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
    private static readonly string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
    private static readonly string Username = ConfigurationManager.AppSettings["Username"];
    private static readonly string Password = ConfigurationManager.AppSettings["Password"] + SecurityToken;
    private static readonly string IsSandboxUser = ConfigurationManager.AppSettings["IsSandboxUser"];
      public static String result;

    public ActionResult Index() {
      return View();
    }


    [HttpPost]
    public ActionResult Index(String x) {

      System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

      try
      {
        var task = RunSample();
        task.Wait();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.Message);
        Console.WriteLine(e.StackTrace);
      }

      ViewBag.result = result;
      return View();
    }


    private static async System.Threading.Tasks.Task RunSample() {
      var auth = new AuthenticationClient();
        result = "\nAuthenticating with Salesforce";
      // Authenticate with Salesforce
      var url = IsSandboxUser.Equals("true", StringComparison.CurrentCultureIgnoreCase)
          ? "https://test.salesforce.com/services/oauth2/token"
          : "https://login.salesforce.com/services/oauth2/token";

      await auth.UsernamePasswordAsync(ConsumerKey, ConsumerSecret, Username, Password, url);
      result+= "\nConnected to Salesforce";

      var client = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);

      // retrieve all accounts
      result+= "\nGet Accounts";

      const string qry = "SELECT ID, Name FROM Account";
      var accts = new List<Account>();
      var results = await client.QueryAsync<Account>(qry);
      var totalSize = results.TotalSize;

      result+="\nQueried " + totalSize + " records.";

      accts.AddRange(results.Records);
      var nextRecordsUrl = results.NextRecordsUrl;

      if (!string.IsNullOrEmpty(nextRecordsUrl)) {
        result+="\nFound nextRecordsUrl.";

        while (true) {
          var continuationResults = await client.QueryContinuationAsync<Account>(nextRecordsUrl);
          totalSize = continuationResults.TotalSize;
          result+="\nQueried an additional " + totalSize + " records.";

          accts.AddRange(continuationResults.Records);
          if (string.IsNullOrEmpty(continuationResults.NextRecordsUrl)) break;

          //pass nextRecordsUrl back to client.QueryAsync to request next set of records
          nextRecordsUrl = continuationResults.NextRecordsUrl;
        }
      }
      result+="\nRetrieved accounts = " + accts.Count() + ", expected size = " + totalSize;

      // Create a sample record
      result+="\nCreating test record.";
      var account = new Account { Name = "Test Account" };
      var createAccountResponse = await client.CreateAsync(Account.SObjectTypeName, account);
      account.Id = createAccountResponse.Id;
      if (account.Id == null) {
        result+="\nFailed to create test record.";
        return;
      }

      result+="\nSuccessfully created test record.";

      // Update the sample record
      // Shows that annonymous types can be used as well
      result+="\nUpdating test record.";
      var success = await client.UpdateAsync(Account.SObjectTypeName, account.Id, new { Name = "Test Update" });
      if (!string.IsNullOrEmpty(success.Errors.ToString())) {
        result+="\nFailed to update test record!";
        return;
      }

      result+="\nSuccessfully updated the record.";

      // Retrieve the sample record
      // How to retrieve a single record if the id is known
      result+="\nRetrieving the record by ID.";
      account = await client.QueryByIdAsync<Account>(Account.SObjectTypeName, account.Id);
      if (account == null) {
        result+="\nFailed to retrieve the record by ID!";
        return;
      }

      result+="\nRetrieved the record by ID.";

      // Query for record by name
      result+="\nQuerying the record by name.";
      var accounts = await client.QueryAsync<Account>("SELECT ID, Name FROM Account WHERE Name = '" + account.Name + "'");
      account = accounts.Records.FirstOrDefault();
      if (account == null) {
        result+="\nFailed to retrieve account by query!";
        return;
      }

      result+="\nRetrieved the record by name.";

      // Delete account
      result+="\nDeleting the record by ID.";
      var deleted = await client.DeleteAsync(Account.SObjectTypeName, account.Id);
      if (!deleted) {
        result+="\nFailed to delete the record by ID!";
        return;
      }
      result+="\nDeleted the record by ID.";

      // Selecting multiple accounts into a dynamic
      result+="\nQuerying multiple records.";
      var dynamicAccounts = await client.QueryAsync<dynamic>("SELECT ID, Name FROM Account LIMIT 10");
      foreach (dynamic acct in dynamicAccounts.Records) {
        result+="\nAccount - " + acct.Name;
      }

      // Creating parent - child records using a Dynamic
      result+="\nCreating a parent record (Account)";
      dynamic a = new ExpandoObject();
      a.Name = "Account from .Net Toolkit";
      var createParentAccountResponse = await client.CreateAsync("Account", a);
      a.Id = createParentAccountResponse.Id;
      if (a.Id == null) {
        result+="\nFailed to create parent record.";
        return;
      }

      result+="\nCreating a child record (Contact)";
      dynamic c = new ExpandoObject();
      c.FirstName = "Joe";
      c.LastName = "Blow";
      c.AccountId = a.Id;
      var createContactResponse = await client.CreateAsync("Contact", c);
      c.Id = createContactResponse.Id;
      if (c.Id == null) {
        result+="\nFailed to create child record.";
        return;
      }

      result+="\nDeleting parent and child";

      // Delete account (also deletes contact)
      result+="\nDeleting the Account by Id.";
      deleted = await client.DeleteAsync(Account.SObjectTypeName, a.Id);
      if (!deleted) {
        result+="\nFailed to delete the record by ID!";
        return;
      }
      result+="\nDeleted the Account and Contact.";

    }

  }

  class Account {
    public const String SObjectTypeName = "Account";

    public String Id { get; set; }
    public String Name { get; set; }
  }

}