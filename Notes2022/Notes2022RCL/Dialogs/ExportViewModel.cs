// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 05-03-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-25-2022
//
// Copyright © 2022, Dale Sinder
//
// Name: ExportViewModel.cs
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
// <copyright file="ExportViewModel.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Notes2022.Proto;
using Notes2022RCL.Menus;

namespace Notes2022RCL.Dialogs
{
    /// <summary>
    /// Class ExportViewModel.
    /// </summary>
    public class ExportViewModel
    {
        /// <summary>
        /// Notefile we are exporting from
        /// </summary>
        /// <value>The note file.</value>
        public NoteFile NoteFile { get; set; }

        /// <summary>
        /// Possible non 0 archive
        /// </summary>
        /// <value>The archive number.</value>
        public int ArchiveNumber { get; set; }

        /// <summary>
        /// Is it to be in html format?
        /// </summary>
        /// <value><c>true</c> if this instance is HTML; otherwise, <c>false</c>.</value>
        public bool isHtml { get; set; }

        /// <summary>
        /// Collapsible or "flat"
        /// </summary>
        /// <value><c>true</c> if this instance is collapsible; otherwise, <c>false</c>.</value>
        public bool isCollapsible { get; set; }

        /// <summary>
        /// Direct output or destination collected via a dailog?
        /// </summary>
        /// <value><c>true</c> if this instance is direct output; otherwise, <c>false</c>.</value>
        public bool isDirectOutput { get; set; }

        //public bool isOnPage { get; set; }

        /// <summary>
        /// NoteOrdinal to export - 0 for all notes
        /// </summary>
        /// <value>The note ordinal.</value>
        public int NoteOrdinal { get; set; }

        /// <summary>
        /// "Marks" to limit scope of notes exportes the a specific set
        /// selected by user "Marked"
        /// </summary>
        /// <value>The marks.</value>
        public List<Mark> Marks { get; set; }

        /// <summary>
        /// Email address if being mailed to someone
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets my menu.
        /// </summary>
        /// <value>My menu.</value>
        public ListMenu myMenu { get; set; }
    }

    /// <summary>
    /// Class Mark.
    /// </summary>
    public class Mark
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the note file identifier.
        /// </summary>
        /// <value>The note file identifier.</value>
        public int NoteFileId { get; set; }

        /// <summary>
        /// Gets or sets the archive identifier.
        /// </summary>
        /// <value>The archive identifier.</value>
        public int ArchiveId { get; set; }

        /// <summary>
        /// Gets or sets the mark ordinal.
        /// </summary>
        /// <value>The mark ordinal.</value>
        public int MarkOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the note ordinal.
        /// </summary>
        /// <value>The note ordinal.</value>
        public int NoteOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the note header identifier.
        /// </summary>
        /// <value>The note header identifier.</value>
        public long NoteHeaderId { get; set; }

        /// <summary>
        /// Gets or sets the response ordinal.
        /// </summary>
        /// <value>The response ordinal.</value>
        public int ResponseOrdinal { get; set; }  // -1 == whole string, 0 base note only, > 0 Response
    }

}

