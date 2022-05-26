// ***********************************************************************
// Assembly         : Notes2022RCL
// Author           : Dale Sinder
// Created          : 05-24-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-25-2022
//
// Copyright © 2022, Dale Sinder
//
// Name: ConfirmEmail.razor.cs
//
// Description:
//      TODO
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 3 as
// published by the Free Software Foundation.   
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License version 3 for more details.
//
//  You should have received a copy of the GNU General Public License
//  version 3 along with this program in file "license-gpl-3.0.txt".
//  If not, see<http://www.gnu.org/licenses/gpl-3.0.txt>.
// ***********************************************************************
// <copyright file="ConfirmEmail.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using System.Text.Json;

namespace Notes2022RCL.Pages.Authentication
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