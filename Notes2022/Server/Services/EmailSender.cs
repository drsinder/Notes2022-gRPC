// ***********************************************************************
// Assembly         : Notes2022.Server
// Author           : Dale Sinder
// Created          : 05-02-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 11-14-2021
// ***********************************************************************
// <copyright file="EmailSender.cs" company="Notes2022.Server">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
/*--------------------------------------------------------------------------
    **
    ** Copyright © 2022, Dale Sinder
    **
    ** Name: EmailSender.cs
    **
    ** Description:
    **      Send Email sometimes with attachment
    **
    ** This program is free software: you can redistribute it and/or modify
    ** it under the terms of the GNU General Public License version 3 as
    ** published by the Free Software Foundation.   
    **
    ** This program is distributed in the hope that it will be useful,
    ** but WITHOUT ANY WARRANTY; without even the implied warranty of
    ** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    ** GNU General Public License version 3 for more details.
    **
    **  You should have received a copy of the GNU General Public License
    **  version 3 along with this program in file "license-gpl-3.0.txt".
    **  If not, see<http://www.gnu.org/licenses/gpl-3.0.txt>.
    **
    **--------------------------------------------------------------------------*/


using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Notes2022.Server.Services
{
    /// <summary>
    /// Class EmailSender.
    /// Implements the <see cref="IEmailSender" />
    /// </summary>
    /// <seealso cref="IEmailSender" />
    public class EmailSender : IEmailSender
    {
        //public StreamWriter StreamWriter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        /// <param name="optionsAccessor">The options accessor.</param>
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender"/> class.
        /// </summary>
        public EmailSender()
        {
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        /// <summary>
        /// Send email as an asynchronous operation.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var apiKey = Globals.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(Globals.SendGridEmail, Globals.SendGridName);
            var to = new EmailAddress(email);
            var htmlStart = "<!DOCTYPE html>";
            var isHtml = message.StartsWith(htmlStart);

            SendGridMessage msg;

            if (email.Contains(';')) // multiple targets
            {
                string[] who = email.Split(';');

                List<EmailAddress> addresses = new List<EmailAddress>();
                foreach (string a in who)
                {
                    addresses.Add(new EmailAddress(a.Trim()));
                }
                msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, addresses, subject, isHtml ? "See Html Attachment." : message, isHtml ? "See Html Attachment." : message);
            }
            else // single target
            {
                msg = MailHelper.CreateSingleEmail(from, to, subject, isHtml ? "See Html Attachment." : message, isHtml ? "See Html Attachment." : message);
            }

            if (isHtml)
            {
                MemoryStream ms = new();
                StreamWriter sw = new(ms);
                await sw.WriteAsync(message);
                await sw.FlushAsync();
                ms.Seek(0, SeekOrigin.Begin);
                await msg.AddAttachmentAsync(subject + ".html", ms);
                ms.Dispose();
            }

            await client.SendEmailAsync(msg);
        }


    }

    /// <summary>
    /// Class AuthMessageSenderOptions.
    /// </summary>
    public class AuthMessageSenderOptions
    {
        /// <summary>
        /// Gets or sets the send grid key.
        /// </summary>
        /// <value>The send grid key.</value>
        public string SendGridKey { get; set; }
    }
}
