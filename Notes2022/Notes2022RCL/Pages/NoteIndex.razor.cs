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
// Name: NoteIndex.razor.cs
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
// <copyright file="NoteIndex.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Notes2022.Proto;
using Notes2022RCL.Dialogs;
using Notes2022RCL.Menus;
using Notes2022RCL.Panels;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Notes2022RCL.Pages
{
    /// <summary>
    /// Class NoteIndex.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class NoteIndex
    {
        /// <summary>
        /// For dialogs
        /// </summary>
        /// <value>The modal.</value>
        [CascadingParameter]
        public IModalService Modal { get; set; }

        /// <summary>
        /// The NoteFileId we are using
        /// </summary>
        /// <value>The notesfile identifier.</value>
        [Parameter]
        public int NotesfileId { get; set; }

        /// <summary>
        /// Non zero when viewing a note
        /// </summary>
        /// <value>The current note identifier.</value>
        [Parameter]
        public long CurrentNoteId { get; set; }

        /// <summary>
        /// Reference to the menu so we can talk to it.
        /// </summary>
        /// <value>My menu.</value>
        protected ListMenu MyMenu { get; set; }

        /// <summary>
        /// Gets or sets my note panel.
        /// </summary>
        /// <value>My note panel.</value>
        public NotePanel MyNotePanel { get; set; }

        /// <summary>
        /// Accumulator for the navigation string
        /// </summary>
        /// <value>The nav string.</value>
        public string NavString { get; set; }

        /// <summary>
        /// Our direct navigation typin box
        /// </summary>
        /// <value>The sf text box.</value>
        protected SfTextBox sfTextBox { get; set; }

        /// <summary>
        /// Our index grid
        /// </summary>
        /// <value>The sf grid1.</value>
        public SfGrid<NoteHeader> sfGrid1 { get; set; }

        /// <summary>
        /// Filter setting for the grid
        /// </summary>
        /// <value>The page settings.</value>
        protected GridPageSettings PageSettings { get; set; }

        /// <summary>
        /// Grid page size
        /// </summary>
        /// <value>The size of the page.</value>
        protected int PageSize { get; set; }

        /// <summary>
        /// Current page of grid
        /// </summary>
        /// <value>The current page.</value>
        protected int CurPage { get; set; }

        /// <summary>
        /// Should note body be shown?
        /// </summary>
        /// <value><c>true</c> if [show content]; otherwise, <c>false</c>.</value>
        protected bool ShowContent { get; set; }

        /// <summary>
        /// Should resopnse body be shown?
        /// </summary>
        /// <value><c>true</c> if [show content r]; otherwise, <c>false</c>.</value>
        protected bool ShowContentR { get; set; }

        ///// <summary>
        ///// If the grid expanded fully expanded
        ///// </summary>
        ///// <value><c>true</c> if [expand all]; otherwise, <c>false</c>.</value>
        //protected bool ExpandAll { get; set; }
        /// <summary>
        /// Are we sequencing?
        /// </summary>
        /// <value><c>true</c> if this instance is seq; otherwise, <c>false</c>.</value>
        protected bool IsSeq { get; set; }

        /// <summary>
        /// Model for the index data
        /// </summary>
        /// <value>The model.</value>
        public NoteDisplayIndexModel Model { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject]
        NavigationManager Navigation { get; set; }

        /// <summary>
        /// Gets or sets the session storage.
        /// </summary>
        /// <value>The session storage.</value>
        [Inject]
        Blazored.SessionStorage.ISessionStorageService sessionStorage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteIndex" /> class.
        /// </summary>
        public NoteIndex()
        {
        }

        //protected override void OnParametersSet()
        //{
        //    OnParametersSetAsync().GetAwaiter();    // notified of login status change
        //    StateHasChanged();
        //}
        /// <summary>
        /// Set up and get data
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            try
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

                await sessionStorage.SetItemAsync<bool>("InSearch", false);
                await sessionStorage.RemoveItemAsync("SearchIndex");
                await sessionStorage.RemoveItemAsync("SearchList");
                IsSeq = await sessionStorage.GetItemAsync<bool>("IsSeq");
                if (IsSeq && NotesfileId < 0)
                {
                    NotesfileId = -NotesfileId;
                }

                // Get the notefile data
                Model = await Client.GetNoteFileIndexDataAsync(new NoteFileRequest()
                { NoteFileId = NotesfileId }, myState.AuthHeader);
                if (!string.IsNullOrEmpty(Model.Message))
                    return;
                // Set preferences for user
                PageSize = Model.UserData.Ipref2;
                ShowContent = Model.UserData.Pref7;
                ShowContentR = Model.UserData.Pref5;
                //ExpandAll = false; // Model.UserData.Pref3;
                // restore page
                CurPage = await sessionStorage.GetItemAsync<int>("IndexPage");
                if (IsSeq)
                {
                    await StartSeq();
                    return;
                }

                if (Globals.GotoNote > 0)
                {
                    CurrentNoteId = Globals.GotoNote;
                    Globals.GotoNote = 0;
                }
            }
            catch (Exception ex)
            {
                PageSize = 12;
                CurPage = 1;
                CurrentNoteId = 0;
                Model = new NoteDisplayIndexModel();
                Model.Message = ex.Message;
                if (ex.InnerException != null)
                    Model.Message += ":  " + ex.InnerException.Message;
            }
        }

        /// <summary>
        /// Note selected for display
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected void DisplayIt(RowSelectEventArgs<NoteHeader> args)
        {
            sessionStorage.SetItemAsync("IndexPage", sfGrid1.PageSettings.CurrentPage).GetAwaiter();
            CurrentNoteId = args.Data.Id;
            try
            {
                this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Goto a specific note
        /// </summary>
        /// <param name="Id">The identifier.</param>
        public void GotoNote(long Id)
        {
            CurrentNoteId = Id;
            try
            {
                this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Goto the listing mode from note display mode
        /// </summary>
        public void Listing()
        {
            CurrentNoteId = 0;
            try
            {
                this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Get the next base note header given the current one
        /// </summary>
        /// <param name="oh">The oh.</param>
        /// <returns>System.Int64.</returns>
        public long GetNextBaseNote(NoteHeader oh)
        {
            long newId = 0;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

            NoteHeader nh = Model.Notes.SingleOrDefault(p => p.NoteOrdinal == oh.NoteOrdinal + 1 && p.ResponseOrdinal == 0 && p.Version == 0);
            if (nh is not null)
                newId = nh.Id;
            return newId;
        }

        /// <summary>
        /// Get the nest note given the current one
        /// </summary>
        /// <param name="oh">The oh.</param>
        /// <returns>System.Int64.</returns>
        public long GetNextNote(NoteHeader oh)
        {
            long newId = 0;
            NoteHeader nh = null;
            nh = Model.AllNotes.SingleOrDefault(p => p.NoteOrdinal == oh.NoteOrdinal && p.ResponseOrdinal == (oh.ResponseOrdinal + 1) && p.Version == 0);
            if (nh is null)
                nh = Model.AllNotes.SingleOrDefault(p => p.NoteOrdinal == (oh.NoteOrdinal + 1) && p.ResponseOrdinal == 0 && p.Version == 0);
            if (nh is not null)
                newId = nh.Id;
            return newId;
        }

        /// <summary>
        /// Get the previous base note
        /// </summary>
        /// <param name="oh">The oh.</param>
        /// <returns>System.Int64.</returns>
        public long GetPreviousBaseNote(NoteHeader oh)
        {
            long newId = 0;
            NoteHeader nh = Model.Notes.SingleOrDefault(p => p.NoteOrdinal == oh.NoteOrdinal - 1 && p.ResponseOrdinal == 0 && p.Version == 0);
            if (nh is not null)
                newId = nh.Id;
            return newId;
        }

        /// <summary>
        /// Get the previous note
        /// </summary>
        /// <param name="oh">The oh.</param>
        /// <returns>System.Int64.</returns>
        public long GetPreviousNote(NoteHeader oh)
        {
            long newId = 0;
            NoteHeader nh = null;
            nh = Model.AllNotes.SingleOrDefault(p => p.NoteOrdinal == oh.NoteOrdinal && p.ResponseOrdinal == oh.ResponseOrdinal - 1 && p.Version == 0);
            if (nh is null)
                nh = Model.Notes.SingleOrDefault(p => p.NoteOrdinal == oh.NoteOrdinal - 1 && p.ResponseOrdinal == 0 && p.Version == 0);
            if (nh is not null)
                newId = nh.Id;
            return newId;
        }

        /// <summary>
        /// Get just the response headers for the given noteid
        /// </summary>
        /// <param name="headerId">The header identifier.</param>
        /// <returns>List&lt;NoteHeader&gt;.</returns>
        public List<NoteHeader> GetResponseHeaders(long headerId)
        {
            return Model.AllNotes.Where(p => p.BaseNoteId == headerId && (p.ResponseOrdinal != 0) && p.IsDeleted == false && p.Version == 0).OrderBy(p => p.ResponseOrdinal).ToList();
        }

        /// <summary>
        /// Get the Index model - used by the NotePanel
        /// </summary>
        /// <returns>NoteDisplayIndexModel.</returns>
        public NoteDisplayIndexModel GetModel()
        {
            return Model;
        }

        /// <summary>
        /// Get note header Id given note ordinal and response ordinal
        /// </summary>
        /// <param name="noteOrd">The note ord.</param>
        /// <param name="respOrd">The resp ord.</param>
        /// <returns>System.Int64.</returns>
        public long GetNoteHeaderId(int noteOrd, int respOrd)
        {
            long newId = 0;
            NoteHeader nh;
            nh = Model.AllNotes.SingleOrDefault(p => p.NoteOrdinal == noteOrd && p.ResponseOrdinal == respOrd && p.Version == 0);
            if (nh is null && respOrd > -1) // try next base note -- special case if noteOrd == 0 and ResponseOrd == 0  ==> get first base note in file
            {
                nh = Model.AllNotes.SingleOrDefault(p => p.NoteOrdinal == noteOrd + 1 && p.ResponseOrdinal == 0 && p.Version == 0);
            }
            else if (nh is null) // try previous base note
            {
                nh = Model.AllNotes.SingleOrDefault(p => p.NoteOrdinal == noteOrd - 1 && p.ResponseOrdinal == 0 && p.Version == 0);
            }

            if (nh is not null)
                newId = nh.Id;
            return newId;
        }

        /// <summary>
        /// Search results
        /// </summary>
        /// <value>The results.</value>
        private List<NoteHeader> results { get; set; }

        /// <summary>
        /// Are we searching?
        /// </summary>
        /// <value><c>true</c> if this instance is search; otherwise, <c>false</c>.</value>
        private bool isSearch { get; set; }

        /// <summary>
        /// Temp used for navigation
        /// </summary>
        /// <value>The mode.</value>
        private long mode { get; set; }

        /// <summary>
        /// Enum SearchOption
        /// </summary>
        public enum SearchOption
        {
            /// <summary>
            /// The author
            /// </summary>
            Author,
            /// <summary>
            /// The title
            /// </summary>
            Title,
            /// <summary>
            /// The content
            /// </summary>
            Content,
            /// <summary>
            /// The tag
            /// </summary>
            Tag,
            /// <summary>
            /// The dir mess
            /// </summary>
            DirMess,
            /// <summary>
            /// The time is after
            /// </summary>
            TimeIsAfter,
            /// <summary>
            /// The time is before
            /// </summary>
            TimeIsBefore
        }

        /// <summary>
        /// Class Search.
        /// </summary>
        [DataContract]
        public class Search
        {
            // User doing the search
            /// <summary>
            /// Gets or sets the user identifier.
            /// </summary>
            /// <value>The user identifier.</value>
            [StringLength(450)]
            [DataMember(Order = 1)]
            public string? UserId { get; set; }

            // search specs Option
            /// <summary>
            /// Gets or sets the option.
            /// </summary>
            /// <value>The option.</value>
            [Display(Name = "Search By")]
            [DataMember(Order = 2)]
            public SearchOption Option { get; set; }

            // Text to search for
            /// <summary>
            /// Gets or sets the text.
            /// </summary>
            /// <value>The text.</value>
            [Display(Name = "Search Text")]
            [DataMember(Order = 3)]
            public string? Text { get; set; }

            // DateTime to compare to
            /// <summary>
            /// Gets or sets the time.
            /// </summary>
            /// <value>The time.</value>
            [Display(Name = "Search Date/Time")]
            [DataMember(Order = 4)]
            public DateTime Time { get; set; }

            // current/next info -- where we are in the search
            /// <summary>
            /// Gets or sets the note file identifier.
            /// </summary>
            /// <value>The note file identifier.</value>
            [Column(Order = 0)]
            [DataMember(Order = 5)]
            public int NoteFileId { get; set; }

            /// <summary>
            /// Gets or sets the archive identifier.
            /// </summary>
            /// <value>The archive identifier.</value>
            [Required]
            [Column(Order = 1)]
            [DataMember(Order = 6)]
            public int ArchiveId { get; set; }

            /// <summary>
            /// Gets or sets the base ordinal.
            /// </summary>
            /// <value>The base ordinal.</value>
            [Column(Order = 2)]
            [DataMember(Order = 7)]
            public int BaseOrdinal { get; set; }

            /// <summary>
            /// Gets or sets the response ordinal.
            /// </summary>
            /// <value>The response ordinal.</value>
            [Column(Order = 3)]
            [DataMember(Order = 8)]
            public int ResponseOrdinal { get; set; }

            /// <summary>
            /// Gets or sets the note identifier.
            /// </summary>
            /// <value>The note identifier.</value>
            [Column(Order = 4)]
            [DataMember(Order = 9)]
            public long NoteID { get; set; }
        }

        /// <summary>
        /// Starts the search.
        /// </summary>
        /// <param name="target">The target.</param>
        public async Task StartSearch(Search target)
        {
            //message = "Searching... Please Wait...";
            //StateHasChanged();
#pragma warning disable CS8602 // Dereference of a possibly null reference.

            target.Text = target.Text.ToLower();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            switch (target.Option)
            {
                case SearchOption.Author:
                case SearchOption.Title:
                case SearchOption.TimeIsAfter:
                case SearchOption.TimeIsBefore:
                case SearchOption.DirMess:
                    await SearchHeader(target);
                    break;
                case SearchOption.Content:
                    await SearchContents(target);
                    break;
                case SearchOption.Tag:
                    await SearchTags(target);
                    break;
                default:
                    break;
            }
            //message = null;
            //StateHasChanged();
        }

        /// <summary>
        /// Searches the tags.
        /// </summary>
        /// <param name="target">The target.</param>
        protected async Task SearchTags(Search target)
        {
            if (Model.Tags == null || Model.Tags.Count == 0)
            {
                ShowMessage("Nothing Found.");
                return;
            }

#pragma warning disable CS8604 // Possible null reference argument.

            List<Tags> tags = Model.Tags.Where(p => p.Tag.ToLower().Contains(target.Text)).ToList();
            if (tags == null || tags.Count == 0)
            {
                ShowMessage("Nothing Found.");
                return;
            }

            results = new List<NoteHeader>();
            foreach (Tags tag in tags)
            {
                NoteHeader h = Model.AllNotes.SingleOrDefault(p => p.Id == tag.NoteHeaderId);
                if (h != null)
                    results.Add(h);
            }

            if (results.Count == 0)
            {
                ShowMessage("Nothing Found.");
                return;
            }

            results = results.OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal).ToList();
            mode = results[0].Id;
            isSearch = true;
            await sessionStorage.SetItemAsync<bool>("InSearch", true);
            await sessionStorage.SetItemAsync<int>("SearchIndex", 0);
            await sessionStorage.SetItemAsync<List<NoteHeader>>("SearchList", results);
            CurrentNoteId = mode;
            StateHasChanged();
        }

        /// <summary>
        /// Searches the header.
        /// </summary>
        /// <param name="target">The target.</param>
        protected async Task SearchHeader(Search target)
        {
            results = new List<NoteHeader>();
            List<NoteHeader> lookin = Model.AllNotes.ToList();
            foreach (NoteHeader nh in lookin)
            {
                bool isMatch = false;
                switch (target.Option)
                {
                    case SearchOption.Author:
                        isMatch = nh.AuthorName.ToLower().Contains(target.Text);
                        break;
                    case SearchOption.Title:
                        isMatch = nh.NoteSubject.ToLower().Contains(target.Text);
                        break;
                    case SearchOption.DirMess:
                        if (!string.IsNullOrEmpty(nh.DirectorMessage))
                            isMatch = nh.DirectorMessage.ToLower().Contains(target.Text);
                        break;
                    case SearchOption.TimeIsAfter:
                        isMatch = DateTime.Compare(nh.LastEdited.ToDateTime(), target.Time) > 0;
                        break;
                    case SearchOption.TimeIsBefore:
                        isMatch = DateTime.Compare(nh.LastEdited.ToDateTime(), target.Time) < 0;
                        break;
                }

                if (isMatch)
                    results.Add(nh);
            }

            if (results.Count == 0)
            {
                ShowMessage("Nothing Found.");
                return;
            }

            results = results.OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal).ToList();
            mode = results[0].Id;
            isSearch = true;
            await sessionStorage.SetItemAsync<bool>("InSearch", true);
            await sessionStorage.SetItemAsync<int>("SearchIndex", 0);
            await sessionStorage.SetItemAsync<List<NoteHeader>>("SearchList", results);
            CurrentNoteId = mode;
            StateHasChanged();
        }

        /// <summary>
        /// Searches the contents.
        /// </summary>
        /// <param name="target">The target.</param>
        protected async Task SearchContents(Search target)
        {
            results = new List<NoteHeader>();


            //List<NoteHeader> lookin = Model.AllNotes.ToList();
            //foreach (NoteHeader nh in lookin)
            //{
            //    DisplayModel dm = await Client.GetPartNoteContentAsync(new DisplayModelRequest()
            //    { NoteId = nh.Id }, myState.AuthHeader);
            //    NoteContent nc = dm.Content;
            //    bool isMatch = false;
            //    switch (target.Option)
            //    {
            //        case SearchOption.Content:
            //            isMatch = nc.NoteBody.ToLower().Contains(target.Text);
            //            break;
            //    }

            //    if (isMatch)
            //        results.Add(nh);
            //}


            ContentSearchRequest req = new()
            {
                FileId = Model.NoteFile.Id,
                ArcId = Model.ArcId,
                Target = target.Text
            };
            ContentSearchResponse resp = await Client.SearchNoteContentAsync(req , myState.AuthHeader);

            results = Model.AllNotes.Where(p => resp.List.Contains(p.Id)).OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal).ToList();

             if (results.Count == 0)
            {
                ShowMessage("Nothing Found.");
                return;
            }

            results = results.OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal).ToList();
            mode = results[0].Id;
            await sessionStorage.SetItemAsync<bool>("InSearch", true);
            await sessionStorage.SetItemAsync<int>("SearchIndex", 0);
            await sessionStorage.SetItemAsync<List<NoteHeader>>("SearchList", results);
            CurrentNoteId = mode;
            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        /// <summary>
        /// Starts the seq.
        /// </summary>
        protected async Task StartSeq()
        {
            Sequencer seq = await sessionStorage.GetItemAsync<Sequencer>("SeqItem");
            if (seq is null)
                return;
            List<NoteHeader> noteHeaders1 = Model.AllNotes.ToList().FindAll(p => p.IsDeleted == false && p.Version == 0);
            List<NoteHeader> noteHeaders2 = new List<NoteHeader>();
            foreach (NoteHeader noteHeader in noteHeaders1)
            {
                if (DateTime.Compare(noteHeader.LastEdited.ToDateTime(), seq.LastTime.ToDateTime()) >= 0L)
                {
                    noteHeaders2.Add(noteHeader);
                }
            }

            List<NoteHeader> noteHeaders = noteHeaders2.OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal).ToList();
            if (noteHeaders.Count == 0)
            {
                List<Sequencer> sequencers = await sessionStorage.GetItemAsync<List<Sequencer>>("SeqList");
                int seqIndex = await sessionStorage.GetItemAsync<int>("SeqIndex");
                if (sequencers.Count <= ++seqIndex)
                {
                    await sessionStorage.SetItemAsync("IsSeq", false);
                    await sessionStorage.RemoveItemAsync("SeqList");
                    await sessionStorage.RemoveItemAsync("SeqItem");
                    await sessionStorage.RemoveItemAsync("SeqIndex");
                    await sessionStorage.RemoveItemAsync("SeqHeaders");
                    await sessionStorage.RemoveItemAsync("SeqHeaderIndex");
                    await sessionStorage.RemoveItemAsync("CurrentSeqHeader");
                    ShowMessage("You have seen all the new notes!");
                    Navigation.NavigateTo("");
                    return; // end it all
                }

                Sequencer currSeq = sequencers[seqIndex];
                await sessionStorage.SetItemAsync("SeqIndex", seqIndex);
                Navigation.NavigateTo("noteindex/" + -currSeq.NoteFileId);
                return;
            }

            await sessionStorage.SetItemAsync("SeqHeaders", noteHeaders);
            await sessionStorage.SetItemAsync("SeqHeaderIndex", 0);
            NoteHeader currHeader = noteHeaders[0];
            await sessionStorage.SetItemAsync("CurrentSeqHeader", currHeader);
            seq.Active = true;
            await Client.UpdateSequencerAsync(seq, myState.AuthHeader);
            CurrentNoteId = currHeader.Id;
            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Actions the complete handler.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public async void ActionCompleteHandler(ActionEventArgs<NoteHeader> args)
        {
            await sessionStorage.SetItemAsync("IndexPage", sfGrid1.PageSettings.CurrentPage);
            CurPage = await sessionStorage.GetItemAsync<int>("IndexPage");
        }

        /// <summary>
        /// Potential navigation event when ever a key up occurs
        /// </summary>
        /// <param name="args">The <see cref="KeyboardEventArgs" /> instance containing the event data.</param>
        private async Task KeyUpHandler(KeyboardEventArgs args)
        {
            // handle single key press events
            // call up into the menu to execute
            switch (NavString)
            {
                case "L":
                    await ClearNav();
                    await MyMenu.ExecMenu("ListNoteFiles");
                    return;
                case "N":
                    await ClearNav();
                    await MyMenu.ExecMenu("NewBaseNote");
                    return;
                case "X":
                    await ClearNav();
                    await MyMenu.ExecMenu("eXport");
                    return;
                case "J":
                    await ClearNav();
                    await MyMenu.ExecMenu("JsonExport");
                    return;
                case "m":
                    await ClearNav();
                    await MyMenu.ExecMenu("mailFromIndex");
                    return;
                case "P":
                    await ClearNav();
                    await MyMenu.ExecMenu("PrintFile");
                    return;
                case "Z":
                    await ClearNav();
                    Modal.Show<HelpDialog>();
                    return;
                case "H":
                    await ClearNav();
                    await MyMenu.ExecMenu("HtmlFromIndex");
                    return;
                case "h":
                    await ClearNav();
                    await MyMenu.ExecMenu("htmlFromIndex");
                    return;
                case "R":
                    await ClearNav();
                    await MyMenu.ExecMenu("ReloadIndex");
                    return;
                case "A":
                    await ClearNav();
                    await MyMenu.ExecMenu("AccessControls");
                    return;
                case "S":
                    await ClearNav();
                    await MyMenu.ExecMenu("SearchFromIndex");
                    return;
                default:
                    break;
            }

            // Enter press - look for processing
            // Look at NotePanel documentation for how this is processed...
            // It's more involved there anyway...
            if (args.Key == "Enter")
            {
                if (!string.IsNullOrEmpty(NavString))
                {
                    string stuff = NavString.Replace(";", "").Replace(" ", "");
                    // parse string for # or #.#
                    string[] parts = stuff.Split('.');
                    if (parts.Length > 2)
                    {
                        ShowMessage("Too many '.'s : " + parts.Length);
                    }

                    int noteNum;
                    if (parts.Length == 1)
                    {
                        if (!int.TryParse(parts[0], out noteNum))
                        {
                            ShowMessage("Could not parse : " + parts[0]);
                        }
                        else
                        {
                            long headerId = GetNoteHeaderId(noteNum, 0);
                            if (headerId != 0)
                            {
                                CurrentNoteId = headerId;
                                try
                                {
                                    await this.InvokeAsync(() => this.StateHasChanged());
                                }
                                catch (Exception) { }
                                return;
                            }
                            else
                                ShowMessage("Could not find note : " + stuff);
                        }
                    }
                    else if (parts.Length == 2)
                    {
                        if (!int.TryParse(parts[0], out noteNum))
                        {
                            ShowMessage("Could not parse : " + parts[0]);
                        }

                        int noteRespOrd;
                        if (!int.TryParse(parts[1], out noteRespOrd))
                        {
                            ShowMessage("Could not parse : " + parts[1]);
                        }

                        if (noteNum != 0 && noteRespOrd != 0)
                        {
                            long headerId = GetNoteHeaderId(noteNum, noteRespOrd);
                            if (headerId != 0)
                            {
                                CurrentNoteId = headerId;
                                try
                                {
                                    await this.InvokeAsync(() => this.StateHasChanged());
                                }
                                catch (Exception) { }
                                return;
                            }
                            else
                                ShowMessage("Could not find note : " + stuff);
                        }
                    }

                    await ClearNav();
                }
            }
        }

        /// <summary>
        /// Accumulate input
        /// </summary>
        /// <param name="args">The <see cref="InputEventArgs" /> instance containing the event data.</param>
        private async void NavInputHandler(InputEventArgs args)
        {
            NavString = args.Value;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Clear accumulated input
        /// </summary>
        private async Task ClearNav()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

            NavString = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            await Task.CompletedTask;
        }

        /// <summary>
        /// Handle state change for expand all switch
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowMessage(string message)
        {
            var parameters = new ModalParameters();
            parameters.Add("MessageInput", message);
            Modal.Show<MessageBox>("", parameters);
        }

#pragma warning restore CS8604 // Possible null reference argument.

        /// <summary>
        /// The reloader
        /// </summary>
        private Timer Reloader;
        /// <summary>
        /// On after render as an asynchronous operation.
        /// </summary>
        /// <param name="firstRender">if set to <c>true</c> [first render].</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (!firstRender)
            { // have to wait a bit before putting focus in textbox
                //if (ExpandAll)
                //    await sfGrid1.ExpandAllDetailRowAsync();
                if (sfTextBox is not null)
                {
                    await Task.Delay(300);
                    await sfTextBox.FocusAsync();
                }
            }
            else
            {
                myState.OnChange += OnParametersSet; // get notified of login status changes
                int interval = Globals.Interval;
                if (interval > 0)
                {
                    Reloader = new Timer(interval * 60000); // check for new notes every interval minutes
                    Reloader.Elapsed += Reload;
                    Reloader.Enabled = true;
                    Reloader.Start();
                }
            }
        }

        /// <summary>
        /// Reloads the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        protected async void Reload(Object source, ElapsedEventArgs e)
        {
            Reloader.Enabled = true;
            Reloader.Start();
            int x = Model.AllNotes.Count;
            int y = Model.Notes.Count;
            Model = await Client.GetNoteFileIndexDataAsync(new NoteFileRequest()
            { NoteFileId = NotesfileId }, myState.AuthHeader);
            if (y != Model.Notes.Count)
            {
                ShowMessage("New Note(s) in " + Model.NoteFile.NoteFileTitle);
                try
                {
                    await this.InvokeAsync(() => this.StateHasChanged());
                }
                catch (Exception) { }
            }
            else if (x != Model.AllNotes.Count)
            {
                ShowMessage("New Response(s) in " + Model.NoteFile.NoteFileTitle);
                try
                {
                    await this.InvokeAsync(() => this.StateHasChanged());
                }
                catch (Exception) { }
            }
        }
    }
}