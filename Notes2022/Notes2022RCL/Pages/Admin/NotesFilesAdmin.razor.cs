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
// Name: NotesFilesAdmin.razor.cs
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
// <copyright file="NotesFilesAdmin.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Notes2022RCL.Dialogs;
using W8lessLabs.Blazor.LocalFiles;

namespace Notes2022RCL.Pages.Admin
{
    /// <summary>
    /// Class NotesFilesAdmin.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class NotesFilesAdmin
    {
#pragma warning disable IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets the todo.
        /// </summary>
        /// <value>The todo.</value>
        private List<string> todo { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        private NoteFileList files { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        private HomePageModel model { get; set; }

        /// <summary>
        /// Gets or sets the u list.
        /// </summary>
        /// <value>The u list.</value>
        private List<GAppUser> uList { get; set; }

#pragma warning restore IDE1006 // Naming Styles

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
        /// Gets or sets the modal.
        /// </summary>
        /// <value>The modal.</value>
        [Inject]
        IModalService Modal { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Initializes a new instance of the <see cref="NotesFilesAdmin" /> class.
        /// </summary>
        public NotesFilesAdmin()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        {
        }

        /// <summary>
        /// Reloads this instance.
        /// </summary>
        public async Task Reload()
        {
            await GetStuff();
            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            await GetStuff();
        }

        /// <summary>
        /// Gets the stuff.
        /// </summary>
        protected async Task GetStuff()
        {
            model = await Client.GetAdminPageModelAsync(new NoRequest(), myState.AuthHeader);
            uList = GetApplicationUsers(model.UserDataList);
            uList = uList is not null ? uList : new();
            todo = new List<string> { "announce", "pbnotes", "noteshelp", "pad", "homepagemessages" };
            foreach (NoteFile file in model.NoteFiles.List)
            {
                if (file.NoteFileName == "announce")
                    todo.Remove("announce");
                if (file.NoteFileName == "pbnotes")
                    todo.Remove("pbnotes");
                if (file.NoteFileName == "noteshelp")
                    todo.Remove("noteshelp");
                if (file.NoteFileName == "pad")
                    todo.Remove("pad");
                if (file.NoteFileName == "homepagemessages")
                    todo.Remove("homepagemessages");
            }

            files = model.NoteFiles;
        }

        /// <summary>
        /// Creates the announce.
        /// </summary>
        private async Task CreateAnnounce()
        {
            await Client.CreateNoteFileAsync(new NoteFile()
            { NoteFileName = "announce", NoteFileTitle = "Notes 2022 Announcements" }, myState.AuthHeader);
            await Reload();
            Navigation.NavigateTo("admin/notefilelist");
        }

        /// <summary>
        /// Creates the pbnotes.
        /// </summary>
        private async Task CreatePbnotes()
        {
            await Client.CreateNoteFileAsync(new NoteFile()
            { NoteFileName = "pbnotes", NoteFileTitle = "Public Notes" }, myState.AuthHeader);
            await Reload();
            Navigation.NavigateTo("admin/notefilelist");
        }

        /// <summary>
        /// Creates the notes help.
        /// </summary>
        private async Task CreateNotesHelp()
        {
            await Client.CreateNoteFileAsync(new NoteFile()
            { NoteFileName = "noteshelp", NoteFileTitle = "Help with Notes 2022" }, myState.AuthHeader);
            await Reload();
            Navigation.NavigateTo("admin/notefilelist");
        }

        /// <summary>
        /// Creates the pad.
        /// </summary>
        private async Task CreatePad()
        {
            await Client.CreateNoteFileAsync(new NoteFile()
            { NoteFileName = "pad", NoteFileTitle = "Traditional Pad" }, myState.AuthHeader);
            await Reload();
            Navigation.NavigateTo("admin/notefilelist");
        }

        /// <summary>
        /// Creates the home page messages.
        /// </summary>
        private async Task CreateHomePageMessages()
        {
            await Client.CreateNoteFileAsync(new NoteFile()
            { NoteFileName = "homepagemessages", NoteFileTitle = "Home Page Messages" }, myState.AuthHeader);
            await Reload();
            Navigation.NavigateTo("admin/notefilelist");
        }

        /// <summary>
        /// Creates the note file.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        async void CreateNoteFile(int Id)
        {
            try
            {
                this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
            var parameters = new ModalParameters();
            parameters.Add("FileId", Id);
            var xModal = Modal.Show<Dialogs.CreateNoteFile>("Create Notefile", parameters);
            var result = await xModal.Result;
            await Reload();
            if (!result.Cancelled)
                Navigation.NavigateTo("admin/notefilelist");
        }

        /// <summary>
        /// Gets the application users.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>List&lt;GAppUser&gt;.</returns>
        private List<GAppUser> GetApplicationUsers(GAppUserList other)
        {
            List<GAppUser> list = new List<GAppUser>();
            foreach (GAppUser user in other.List)
            {
                list.Add(user);
            }

            return list;
        }

        /// <summary>
        /// Deletes the note file.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        async void DeleteNoteFile(int Id)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

            NoteFile file = files.List.ToList().Find(x => x.Id == Id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
            var parameters = new ModalParameters();
            parameters.Add("FileId", Id);
#pragma warning disable CS8602 // Dereference of a possibly null reference.

            parameters.Add("FileName", file.NoteFileName);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            parameters.Add("FileTitle", file.NoteFileTitle);
            IModalReference? xModal = Modal.Show<Dialogs.DeleteNoteFile>("Delete", parameters);
            var result = await xModal.Result;
            if (!result.Cancelled)
            {
                await Reload();
                Navigation.NavigateTo("admin/notefilelist");
            }
        }

        /// <summary>
        /// Notes the file details.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        async void NoteFileDetails(int Id)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

            NoteFile file = files.List.ToList().Find(x => x.Id == Id);
            var parameters = new ModalParameters();
            parameters.Add("FileId", Id);
#pragma warning disable CS8602 // Dereference of a possibly null reference.

            parameters.Add("FileName", file.NoteFileName);
            parameters.Add("FileTitle", file.NoteFileTitle);
            //parameters.Add("LastEdited", file.LastEdited);
            parameters.Add("NumberArchives", file.NumberArchives);
            parameters.Add("Owner", model.UserDataList.List.ToList().Find(p => p.Id == file.OwnerId).DisplayName);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            var xModal = Modal.Show<Dialogs.NoteFileDetails>("Details", parameters);
            await xModal.Result;
        }

        /// <summary>
        /// Edits the note file.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        async void EditNoteFile(int Id)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

            NoteFile file = files.List.ToList().Find(x => x.Id == Id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            var parameters = new ModalParameters();
            parameters.Add("FileId", Id);
#pragma warning disable CS8602 // Dereference of a possibly null reference.

            parameters.Add("FileName", file.NoteFileName);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            parameters.Add("FileTitle", file.NoteFileTitle);
            parameters.Add("LastEdited", file.LastEdited);
            parameters.Add("NumberArchives", file.NumberArchives);
            parameters.Add("Owner", file.OwnerId);
            var xModal = Modal.Show<Dialogs.EditNoteFile>("Edit Notefile", parameters);
            var result = await xModal.Result;
            if (!result.Cancelled)
            {
                await Reload();
                Navigation.NavigateTo("admin/notefilelist");
            }
        }

        //        /// <summary>
        //        /// Imports the note file.
        //        /// </summary>
        //        /// <param name="Id">The identifier.</param>
        //        async Task ImportNoteFile(int Id)
        //        {
        //            var parameters = new ModalParameters();
        //            var xModal = Modal.Show<Dialogs.Upload1>("Upload1", parameters);
        //            var result = await xModal.Result;
        //            if (result.Cancelled)
        //            {
        //                return;
        //            }
        //            else
        //            {
        //#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        //                NoteFile file = files.List.ToList().Find(x => x.Id == Id);
        //#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        //                string filename = (string)result.Data;
        //                parameters = new ModalParameters();
        //                parameters.Add("UploadFile", filename);
        //#pragma warning disable CS8602 // Dereference of a possibly null reference.
        //                parameters.Add("NoteFile", file.NoteFileName);
        //#pragma warning restore CS8602 // Dereference of a possibly null reference.
        //                var yModal = Modal.Show<Dialogs.Upload2>("Upload2", parameters);
        //                await yModal.Result;
        //                Navigation.NavigateTo("noteindex/" + Id);
        //                return;
        //            }
        //        }
        /// <summary>
        /// The file select
        /// </summary>
        protected FileSelect fileSelect;
        /// <summary>
        /// The filename
        /// </summary>
        protected string filename;
        /// <summary>
        /// The file identifier
        /// </summary>
        protected int fileId;
        /// <summary>
        /// Fileses the selected handler.
        /// </summary>
        /// <param name="selectedFiles">The selected files.</param>
        async Task FilesSelectedHandler(SelectedFile[] selectedFiles)
        {
            SelectedFile? selectedFile = selectedFiles[0];
            Stream myFile = await fileSelect.OpenFileStreamAsync(selectedFile.Name); // open a stream on the file
            StreamReader sr = new StreamReader(myFile); // get a reader
            string myText = await sr.ReadToEndAsync(); // read entire file as a string
            ModalParameters? parameters = new ModalParameters();
            parameters.Add("UploadText", myText);
            parameters.Add("NoteFile", filename);
            var yModal = Modal.Show<Dialogs.Upload>("Upload2", parameters);
            await yModal.Result;
            Navigation.NavigateTo("noteindex/" + fileId);
        }

        /// <summary>
        /// Imports the note file2.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        async Task ImportNoteFile2(int Id)
        {
            NoteFile file = files.List.ToList().Single(x => x.Id == Id);
            filename = file.NoteFileName;
            fileId = file.Id;

            if (Globals.IsMaui)
            {
                IModalReference? xx = ShowYesNo("Be sure data to import is in the clipboard!! Ready?");
                ModalResult? res = await xx.Result;
                if (res.Cancelled)
                    return;

                Notes2022MauiLib.MauiFileActions mauiFileActions = new Notes2022MauiLib.MauiFileActions();
                string txt = await mauiFileActions.ReadClipboard();

                if (string.IsNullOrEmpty(txt) || !txt.StartsWith("{"))
                    return;

                ModalParameters? parameters = new ModalParameters();
                parameters.Add("UploadText", txt);
                parameters.Add("NoteFile", filename);
                var yModal = Modal.Show<Upload>("Upload2", parameters);
                await yModal.Result;
                Navigation.NavigateTo("noteindex/" + fileId);

                return;
            }

            fileSelect.SelectFiles();
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="message">The message.</param>
        private IModalReference? ShowYesNo(string message)
        {
            var parameters = new ModalParameters();
            parameters.Add("MessageInput", message);
            IModalReference?  retval = Modal.Show<YesNo>("", parameters);
            return retval;
        }

    }
}
