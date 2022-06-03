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
// Name: Responses.razor.cs
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
// <copyright file="Responses.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Notes2022RCL.Pages;
using Syncfusion.Blazor.Grids;

namespace Notes2022RCL.Panels
{
    /// <summary>
    /// Class Responses.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class Responses
    {
        /// <summary>
        /// List of response headers
        /// </summary>
        /// <value>The headers.</value>
        [Parameter]
        public List<NoteHeader> Headers { get; set; }

        /// <summary>
        /// Show content for responses
        /// </summary>
        /// <value><c>true</c> if [show content r]; otherwise, <c>false</c>.</value>
        [Parameter]
        public bool ShowContentR { get; set; }

        /// <summary>
        /// Expand all rows
        /// </summary>
        /// <value><c>true</c> if [expand all r]; otherwise, <c>false</c>.</value>
        [Parameter]
        public bool ExpandAllR { get; set; }

        [Parameter]
        public NoteIndex MyNoteIndex { get; set; }

        //[Parameter] public NoteIndex Parent {get; set;}
        //public bool ShowContent { get; set; }
        //public bool ExpandAll { get; set; }
        /// <summary>
        /// Gets or sets the sf grid2.
        /// </summary>
        /// <value>The sf grid2.</value>
        protected SfGrid<NoteHeader> sfGrid2 { get; set; }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject]
        NavigationManager Navigation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Responses" /> class.
        /// </summary>
        public Responses()
        {
        }

        /// <summary>
        /// Copy parameter to local copy
        /// </summary>
         //protected override async Task OnParametersSetAsync()
        //{
        //}
        public void DataBoundHandler()
        {
            // Expand if appropriate
            if (ExpandAllR)
            {
                sfGrid2.ExpandAllDetailRowAsync().GetAwaiter();
            }
        }

        //System.Timers.Timer timer2;
        //protected void ActionCompleteHandler(ActionEventArgs<NoteHeader> action)
        //{
        //    if (action.RequestType == Syncfusion.Blazor.Grids.Action.Filtering)
        //    {
        //        timer2 = new System.Timers.Timer(1000);
        //        timer2.Elapsed += TimerTick2;
        //        timer2.Enabled = true;
        //    }
        //}
        //protected void TimerTick2(Object source, ElapsedEventArgs e)
        //{
        //    timer2.Elapsed -= TimerTick2;
        //    timer2.Stop();
        //    timer2.Enabled = false;
        //    this.StateHasChanged();
        //}
        /// <summary>
        /// GO show the note
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected void DisplayIt(RowSelectEventArgs<NoteHeader> args)
        {
            Navigation.NavigateTo("notedisplay/" + args.Data.Id);
        }
    }
}