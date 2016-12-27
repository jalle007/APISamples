/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using APISamples.Helpers;
using APISamples.Models;
using Resources;
using Microsoft.Graph;

namespace APISamples.Controllers
{
    public class GraphController : Controller
    {
        GraphService graphService = new GraphService();

  

    public ActionResult Index()
        {
            return View();
        }

    [Authorize]
    // Get the current user's email address from their profile.
    public async Task<ActionResult> GetMyEmailAddress() {
      try {

        // Initialize the GraphServiceClient.
        GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

        // Get the current user's email address. 
        ViewBag.Email = await graphService.GetMyEmailAddress(graphClient);
        return View("Index");
      } catch (ServiceException se) {
        if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
        return RedirectToAction("Index", "Error", new { message = Resource.Error_Message + Request.RawUrl + ": " + se.Error.Message });
      }
    }

    [Authorize]
    // Send mail on behalf of the current user.
    public async Task<ActionResult> SendEmail() {
      if (string.IsNullOrEmpty(Request.Form["email-address"])) {
        ViewBag.Message = Resource.Graph_SendMail_Message_GetEmailFirst;
        return View("Index");
      }

      // Build the email message.
      Message message = graphService.BuildEmailMessage(Request.Form["recipients"], Request.Form["subject"]);
      try {

        // Initialize the GraphServiceClient.
        GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

        // Send the email.
        await graphService.SendEmail(graphClient, message);

        // Reset the current user's email address and the status to display when the page reloads.
        ViewBag.Email = Request.Form["email-address"];
        ViewBag.Message = Resource.Graph_SendMail_Success_Result;
        return View("Index");
      } catch (ServiceException se) {
        if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
        return RedirectToAction("Index", "Error", new { message = Resource.Error_Message + Request.RawUrl + ": " + se.Error.Message });
      }
    }

 

  }
}
