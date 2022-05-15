// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 05-03-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-03-2022
// ***********************************************************************
// <copyright file="Email.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace Notes2022.Client.Dialogs
{
    /// <summary>
    /// Class Email.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class Email
    {
        /// <summary>
        /// Gets or sets the modal instance.
        /// </summary>
        /// <value>The modal instance.</value>
        [CascadingParameter]
        public BlazoredModalInstance ModalInstance { get; set; }

        /// <summary>
        /// Gets or sets the emailaddr.
        /// </summary>
        /// <value>The emailaddr.</value>
        public string emailaddr { get; set; }

        /// <summary>
        /// Oks this instance.
        /// </summary>
        private void Ok()
        {
            ModalInstance.CloseAsync(ModalResult.Ok(emailaddr));
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            ModalInstance.CancelAsync();
        }
    }
}