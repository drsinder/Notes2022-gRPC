// ***********************************************************************
// Assembly         : Notes2022.Server
// Author           : Dale Sinder
// Created          : 04-26-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-09-2022
// ***********************************************************************
// <copyright file="Importer.cs" company="Notes2022.Server">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
/*--------------------------------------------------------------------------
**
**  Copyright © 2022, Dale Sinder
**
**  Name: Import.cs
**
**  Description:
**      Notes Import for Notes 2022
**
**  This program is free software: you can redistribute it and/or modify
**  it under the terms of the GNU General Public License version 3 as
**  published by the Free Software Foundation.
**  
**  This program is distributed in the hope that it will be useful,
**  but WITHOUT ANY WARRANTY; without even the implied warranty of
**  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
**  GNU General Public License version 3 for more details.
**  
**  You should have received a copy of the GNU General Public License
**  version 3 along with this program in file "license-gpl-3.0.txt".
**  If not, see <http://www.gnu.org/licenses/gpl-3.0.txt>.
**
**--------------------------------------------------------------------------
*/


using Notes2022.Server.Data;
using Notes2022.Proto;

namespace Notes2022.Server
{
    /// <summary>
    /// Does the import of a text file from an old plato/novanet system notefile
    /// There be messy stuff in here!!!
    /// </summary>
    public partial class Importer
    {
        /// <summary>
        /// Outputs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void Output(string message)
        {
        }

        /// <summary>Imports the given input JsonExport object to the notesfile.</summary>
        /// <param name="_db">The database.</param>
        /// <param name="input">The input json text deserialized to a JsonExport object</param>
        /// <param name="myNotesFile">The notes file. name to import to</param>
        /// <returns>
        ///   <c>true</c> if success, <c>false</c> otherwise.</returns>
        public async Task<bool> Import(NotesDbContext _db, JsonExport input, string myNotesFile)
        {
            if (input is null || input.NoteHeaders is null || input.NoteHeaders.List is null || input.NoteHeaders.List.Count < 1)
                return false;   // nothing to import

            NoteFile noteFile = await NoteDataManager.GetFileByName(_db, myNotesFile);

            if (noteFile is null)
                return false;   // no such note file

            // base note loop
            foreach ( NoteHeader nh in input.NoteHeaders.List)
            {
                string theTags = string.Empty;

                NoteHeader makeHeader = new(nh);                    // make a copy of the note

                makeHeader.NoteFileId = noteFile.Id;                // put it in target file
                makeHeader.ArchiveId = 0;                           // must always import to active file
                makeHeader.AuthorID = Globals.ImportedAuthorId;     // Author may not exist - use "*imported*"
                makeHeader.BaseNoteId = 0;                          // should already be 0 - make sure
                makeHeader.ResponseCount = 0;                       // no responses in target yet.
                makeHeader.ResponseOrdinal = 0;                     // base note
                if (nh.Tags is not null && nh.Tags.List is not null && nh.Tags.List.Count > 0)
                {
                    theTags = Tags.ListToString(nh.Tags.List.ToList()); // convert tag list to a string
                }

                // Create the base note
                NoteHeader baseNoteHeader = await NoteDataManager.CreateNote(_db, makeHeader, nh.Content.NoteBody, theTags, makeHeader.DirectorMessage, false, false);
                long baseNoteHeaderId = baseNoteHeader.BaseNoteId;

                if (nh.Responses is null || nh.Responses.List is null || nh.Responses.List.Count < 1)
                    continue;       // no responses - do next base note

                // used this to seqrch for RefId targets
                List<NoteHeader> currentString = new List<NoteHeader>();
                currentString.Add(nh);                              // add the base note first
                currentString.AddRange(nh.Responses.List);          // then all responses

                // keep track of note string in target file
                List<NoteHeader> newString = new List<NoteHeader>();
                newString.Add(baseNoteHeader);              // start with base note added above

                // response loop
                foreach ( NoteHeader rh in nh.Responses.List )
                {
                    makeHeader = new(rh);                           // make a copy of the response
                    makeHeader.BaseNoteId = baseNoteHeaderId;       // connect it to the target file base note
                    makeHeader.NoteFileId = noteFile.Id;            // put it in the target file
                    makeHeader.ArchiveId = 0;                       // must be imported to active file
                    makeHeader.AuthorID = Globals.ImportedAuthorId; // Author may not exist - use "*imported*"
                    if (rh.Tags is not null && rh.Tags.List is not null && rh.Tags.List.Count > 0)
                    {
                        theTags = Tags.ListToString(rh.Tags.List.ToList()); // convert tag list to a string
                    }

                    if (rh.RefId > 0)                               // if this response references another note find it
                    {
                        // find this Id in currentString
                        NoteHeader? refH = currentString.Find(p => p.Id == rh.RefId);
                        if (refH is not null)
                        {
                            // find the ResponseOrdinal in newString
                            NoteHeader? temp = newString.Find(p => p.ResponseOrdinal == refH.ResponseOrdinal);
                            if (temp is not null)
                            {
                                makeHeader.RefId = temp.Id;         // use the Id in target file
                            }
                        }
                    }

                    // Create the response
                    NoteHeader respHeader = await NoteDataManager.CreateResponse(_db, makeHeader, rh.Content.NoteBody, theTags, makeHeader.DirectorMessage, false, false);
                    newString.Add(respHeader);
                }
            }
            return true;
        }
    }
}
