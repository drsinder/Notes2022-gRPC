// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-29-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-08-2022
// ***********************************************************************
// <copyright file="NewNote.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Notes2022.Shared;

namespace Notes2022.Client.Pages
{
    /// <summary>
    /// FOr creating a new note
    /// </summary>
    public partial class NewNote
    {
        /// <summary>
        /// Gets or sets the notesfile identifier.
        /// </summary>
        /// <value>The notesfile identifier.</value>
        [Parameter] public int NotesfileId { get; set; }
        /// <summary>
        /// Gets or sets the base note header identifier.
        /// </summary>
        /// <value>The base note header identifier.</value>
        [Parameter] public long BaseNoteHeaderId { get; set; }   //  base note we are responding to
        /// <summary>
        /// Gets or sets the reference identifier.
        /// </summary>
        /// <value>The reference identifier.</value>
        [Parameter] public long RefId { get; set; }   //  what we are responding to

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        protected TextViewModel Model { get; set; } = new TextViewModel();

        /// <summary>
        /// Gets or sets the HTTP.
        /// </summary>
        /// <value>The HTTP.</value>
        [Inject] HttpClient Http { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Initializes a new instance of the <see cref="NewNote"/> class.
        /// </summary>
        public NewNote()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Just fill in a few fields and we are ready...
        /// </summary>
        protected override void OnParametersSet()
        {
            Model.NoteFileID = NotesfileId; // which file?
            Model.NoteID = 0;               // 0 for new note
            Model.BaseNoteHeaderID = BaseNoteHeaderId;  // base note we are responding to
            Model.RefId = RefId;            // note we are responding to
            Model.MyNote = "";
            Model.MySubject = "";
            Model.TagLine = "";
            Model.DirectorMessage = "";
        }
    }
}
