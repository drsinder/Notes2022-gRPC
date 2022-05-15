// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-29-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-06-2022
// ***********************************************************************
// <copyright file="ShowNote.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022.Client.Pages
{
    /// <summary>
    /// Class ShowNote.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class ShowNote
    {
        /// <summary>
        /// Gets or sets the note identifier.
        /// </summary>
        /// <value>The note identifier.</value>
        [Parameter] public long NoteId { get; set; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>The file identifier.</value>
        public int FileId { get; set; }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject] NavigationManager Navigation { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] Notes2022Server.Notes2022ServerClient Client { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowNote"/> class.
        /// </summary>
        public ShowNote()
        {
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            bool x = myState.IsAuthenticated;
            if (!x)
            {
                await myState.GetLoginReplyAsync();
                if (!myState.IsAuthenticated)
                {
                    Globals.returnUrl = Navigation.Uri;
                    Navigation.NavigateTo("authentication/login");
                }
            }
            // find the file id for this note - get note header
            FileId = (await Client.GetHeaderForNoteIdAsync(new NoteId() { Id = NoteId }, myState.AuthHeader)).NoteFileId;

            Globals.GotoNote = NoteId;
            Navigation.NavigateTo("noteindex/" + FileId);
        }

    }

}
