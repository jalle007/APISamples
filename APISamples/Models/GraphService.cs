﻿/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace APISamples.Models
{
    public class GraphService
    {

    // Get the current user's email address from their profile.
    public async Task<string> GetMyEmailAddress(GraphServiceClient graphClient) {

      // Get the current user. 
      // The app only needs the user's email address, so select the mail and userPrincipalName properties.
      // If the mail property isn't defined, userPrincipalName should map to the email for all account types. 
      User me = await graphClient.Me.Request().Select("mail,userPrincipalName").GetAsync();
      return me.Mail ?? me.UserPrincipalName;
    }

    // Send an email message from the current user.
    public async Task SendEmail(GraphServiceClient graphClient, Message message) {
      await graphClient.Me.SendMail(message, true).Request().PostAsync();
    }

    // Create the email message.
    public Message BuildEmailMessage(string recipients, string subject) {

      // Prepare the recipient list.
      string[] splitter = { ";" };
      string[] splitRecipientsString = recipients.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
      List<Recipient> recipientList = new List<Recipient>();
      foreach (string recipient in splitRecipientsString) {
        recipientList.Add(new Recipient {
          EmailAddress = new EmailAddress {
            Address = recipient.Trim()
          }
        });
      }

      // Build the email message.
      Message email = new Message {
        Body = new ItemBody {
          Content = "Graph_SendMail_Body_Content",
          ContentType = BodyType.Html,
        },
        Subject = subject,
        ToRecipients = recipientList
      };
      return email;
    }
  }
}
