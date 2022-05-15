// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-29-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 04-30-2022
// ***********************************************************************
// <copyright file="Versions.razor.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
/*--------------------------------------------------------------------------
    **
    ** Copyright © 2022, Dale Sinder
    **
    ** Name: Versions.razor.cs
    **
    ** Description:
    **      Displays Versions content
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
    **  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
    **
    **--------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Notes2022.Shared;
using System.Net.Http.Json;

namespace Notes2022.Client.Panels
{
    /// <summary>
    /// Displays versions for edited notes
    /// </summary>
    public partial class Versions
    {
        /// <summary>
        /// These four parameters identify the note
        /// </summary>
        /// <value>The file identifier.</value>
        [Parameter] public int FileId { get; set; }
        /// <summary>
        /// Gets or sets the note ordinal.
        /// </summary>
        /// <value>The note ordinal.</value>
        [Parameter] public int NoteOrdinal { get; set; }
        /// <summary>
        /// Gets or sets the response ordinal.
        /// </summary>
        /// <value>The response ordinal.</value>
        [Parameter] public int ResponseOrdinal { get; set; }
        /// <summary>
        /// Gets or sets the arc identifier.
        /// </summary>
        /// <value>The arc identifier.</value>
        [Parameter] public int ArcId { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>The headers.</value>
        protected NoteHeaderList Headers { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject] Notes2022Server.Notes2022ServerClient Client { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Versions"/> class.
        /// </summary>
        public Versions()
        {
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            Headers = await Client.GetVersionsAsync(new GetVersionsRequest() 
                { FileId=FileId, NoteOrdinal=NoteOrdinal, ResponseOrdinal=ResponseOrdinal, ArcId=ArcId },
                myState.AuthHeader);
        }
    }
}
