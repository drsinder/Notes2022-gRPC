// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-29-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 04-30-2022
// ***********************************************************************
// <copyright file="AccessDeleteButton.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Notes2022.Shared;

namespace Notes2022.Client.Dialogs
{
    /// <summary>
    /// Class AccessDeleteButton.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class AccessDeleteButton
    {
        /// <summary>
        /// Gets or sets the note access.
        /// </summary>
        /// <value>The note access.</value>
        [Parameter]
        public NoteAccess noteAccess { get; set; }

        /// <summary>
        /// Gets or sets the on click.
        /// </summary>
        /// <value>The on click.</value>
        [Parameter]
        public EventCallback<string> OnClick { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] Notes2022Server.Notes2022ServerClient Client { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessDeleteButton"/> class.
        /// </summary>
        public AccessDeleteButton() { }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        protected async Task Delete()
        {
            await Client.DeleteAccessItemAsync(noteAccess, myState.AuthHeader);
            await OnClick.InvokeAsync("Delete");
        }
    }
}