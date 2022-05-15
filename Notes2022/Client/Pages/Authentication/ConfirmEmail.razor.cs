// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 05-02-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-08-2022
// ***********************************************************************
// <copyright file="ConfirmEmail.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using System.Text.Json;

namespace Notes2022.Client.Pages.Authentication
{
    /// <summary>
    /// Class ConfirmEmail.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class ConfirmEmail
    {
        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>The payload.</value>
        [Parameter]
        public string? payload { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        private string? Message { get; set; }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
            ConfirmEmailRequest stuff = JsonSerializer.Deserialize<ConfirmEmailRequest>(Globals.Base64Decode(payload));
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            AuthReply reply = await AuthClient.ConfirmEmailAsync(stuff);

            if (reply != null)
            {
                Message = reply.Message;
            }
            else
            {
                Message = "Confirming email call failed!";
            }
        }
    }
}
