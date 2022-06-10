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


using Notes2022.Proto;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Notes2022.TextToJson.Manager
{
    /// <summary>
    /// Enum FileType.  file sources we can process
    /// </summary>
    enum FileType   
    {
        NovaNet,
        WebNotes,   // not tested in a while
        PlatoNotes
    }

    /// <summary>
    /// Does the conversion of a text file exported from an old 
    /// plato/novanet system notefile to .json format
    /// There be messy stuff in here!!!
    /// </summary>
    public partial class Importer
    {
        /// <summary>
        /// The ff
        /// </summary>
        private const char Ff = (char)(12); //  FF

        private JsonExport outp;

        private long counter = 0;

        private bool flag = true;

        StreamReader? file = null;


        /// <summary>
        /// Import/Convert the file from .txt to .json format.
        /// </summary>
        /// <param name="_db">The database</param>
        /// <param name="file">File name to read from</param>
        /// <param name="myNotesFile">Output notefile</param>
        /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
        public async Task<bool> Import(string filename, string outputNotesFile)
        {
            // get the input file
            FileStream fs = File.OpenRead(filename);
            file = new StreamReader(fs);
            outp = new JsonExport();
            outp.NoteHeaders = new();

            // Some initial setup
            bool isResp = false;
            char[] spaceTrim = new char[] { ' ' };
            char[] slash = new char[] { '/' };
            char[] colon = new char[] { ':' };
            char[] dash = new char[] { '-' };

            string platoBaseYear = "";

            StringBuilder sb = new();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            NoteContent newContent = null;
            NoteHeader makeHeader = null;
            int basenotes = 0;

            FileType filetype = FileType.NovaNet;  // 0= NovaNET | 1 = Notes 3.1 | 2 = plato iv group notes -- we can process three formats

            // Read the file and process it line by line.
            // we first determine the file type
            try
            {
                string line;
                NoteHeader newHeader = new();
                await Task.Delay(200);
                while ((line = await GetLineAsync()) is not null)
                {
                    if (flag)   // first line of input file
                    {
                        if (line.StartsWith("2021 NoteFile ") || line.StartsWith("2022 NoteFile "))  // By this we know it came from Notes Web edition
                        {
                            filetype = FileType.WebNotes;   // Notes 3.1
                            _ = await GetLineAsync();
                            line = await GetLineAsync();
                        }
                        else if (line.StartsWith("+++ plato iv group notes +++"))
                        {
                            filetype = FileType.PlatoNotes;   // plato iv group notes
                            _ = await GetLineAsync();
                            _ = await GetLineAsync();
                            _ = await GetLineAsync();
                            string? platoLine5 = await GetLineAsync();    // has base year
                            _ = await GetLineAsync();
                            _ = await GetLineAsync();

                            _ = await GetLineAsync();
                            _ = await GetLineAsync();
                            line = await GetLineAsync();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                            string[] splits = platoLine5.Split(spaceTrim);
                            platoBaseYear = splits[^1];

                        }   // else we assume it's novanet format = 0

                        flag = false;
                    }

                    if (filetype == FileType.NovaNet)  // Process for NovaNET output
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        line = await CheckFf(line, file);
                        if (line.Length > 52)  // Possible Note Header
                        {
                            string head = line[46..];
                            if (head.StartsWith("  Note ")) //new note found
                            {
                                if (newContent is not null)  // have a note to write
                                {
                                    newContent.NoteBody = sb.ToString();
                                    sb.Clear();  // = new StringBuilder();
                                    newContent.NoteBody = newContent.NoteBody.Replace("\r\n", "<br />");

                                    if (!isResp) // base note
                                    {
                                        basenotes++;
                                        newHeader = CreateNote(makeHeader, newContent.NoteBody);
                                    }
                                    else // resp
                                    {
                                        makeHeader.NoteOrdinal = newHeader.NoteOrdinal;
                                        makeHeader.BaseNoteId = newHeader.Id;  //Fix
                                        CreateResponse(makeHeader, newContent.NoteBody);
                                    }
                                }
                                newContent = new NoteContent();
                                makeHeader = new NoteHeader();

                                // get title at start of line
                                string? title = line[..40].TrimEnd(spaceTrim);
                                makeHeader.NoteSubject = title;
                                isResp = head.Contains("Response");  // is this a response?

                                line = await GetLineAsync();     // get next line  
                                line = await CheckFf(line, file);
                                if (isResp && line.StartsWith("----")) // perhaps a response title
                                {
                                    title = line[4..].TrimEnd(spaceTrim);
                                    makeHeader.NoteSubject = title;
                                    line = await GetLineAsync();  // skip line
                                    await CheckFf(line, file);
                                }
                                else if (isResp)
                                    makeHeader.NoteSubject = "*none*";  // must have a title

                                if (string.IsNullOrEmpty(makeHeader.NoteSubject))  // must have a title
                                    makeHeader.NoteSubject = "*none*";

                                line = await GetLineAsync();  // skip line
                                line = await CheckFf(line, file);

                                // Check for possible director message 
                                if (line.StartsWith("    "))
                                {
                                    makeHeader.DirectorMessage = line.Trim(spaceTrim);
                                    line = await GetLineAsync();  // skip line
                                    await CheckFf(line, file);
                                    line = await GetLineAsync();  // skip line
                                    line = await CheckFf(line, file);
                                }

                                // get date, time, author
                                // first date
                                string date = line[..10].TrimEnd(spaceTrim);
                                string[] x = date.Split(slash);

                                string prefix = "/20";
                                int yearpart = int.Parse(x[2]);
                                if (yearpart > 70)
                                    prefix = "/19";

                                string datetime = (x[0].Length == 1 ? "0" + x[0] : x[0]) + "/" + (x[1].Length == 1 ? "0" + x[1] : x[1]) + prefix + x[2];

                                // now time
                                string time = line.Substring(10, 6);
                                time = time.Trim(spaceTrim);
                                time = time.Replace("am", " ");
                                time = time.Replace("pm", " ");
                                time = time.Replace("a", " ");
                                time = time.Replace("p", " ");
                                time = time.TrimEnd(spaceTrim);

                                string[] y = time.Split(colon);
                                int hour = int.Parse(y[0]);
                                if (line[..23].Contains("pm") && hour < 12)
                                {
                                    hour += 12;
                                }

                                datetime += " " + ((hour < 10) ? "0" + hour.ToString() : hour.ToString()) + ":" + y[1];

                                // Save 
                                makeHeader.LastEdited = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Parse(datetime).ToUniversalTime());

                                // author
                                makeHeader.AuthorName = line[25..].Trim(spaceTrim);
                                makeHeader.AuthorID = "*imported*";   //"imported";
                                line = await GetLineAsync();  // skip line
                                line = await CheckFf(line, file);
                            }
                        }
                        // append line to current note body
                        sb.AppendLine(line);
                    }  // end NovaNET

                    else if (filetype == FileType.WebNotes)  // Process from Notes 3.1 export as text - NOT TESTED IN A LONG TIME!!!
                    {
                        if (line.StartsWith("Note: "))  // possible note header
                        {
                            string[] parts = line.Split(dash);
                            if (parts.Length >= 5)  // looks like the right number of sections > with - in subject grrr
                            {
                                if (newContent is not null)  // have a note to write
                                {
                                    sb.Append(' ');
                                    newContent.NoteBody = sb.ToString();
                                    sb.Clear();  // = new StringBuilder();

                                    if (!isResp) // base note
                                    {
                                        basenotes++;
                                        newHeader = CreateNote(makeHeader, newContent.NoteBody);
                                    }
                                    else // resp
                                    {
                                        makeHeader.NoteOrdinal = newHeader.NoteOrdinal;
                                        makeHeader.BaseNoteId = newHeader.Id;  //Fix
                                        CreateResponse(makeHeader, newContent.NoteBody);
                                    }
                                }
                                newContent = new NoteContent();
                                makeHeader = new NoteHeader();

                                // parse sections, 0 is note number, 1 is subject, 2 is author, 3 is datetime, 4 is number of responses
                                // skip section 0
                                // Get subject

                                string part = parts[1];
                                if (part.StartsWith(" Subject: "))
                                {
                                    if (parts.Length == 5)
                                        makeHeader.NoteSubject = part[9..].Trim() + " ";  //only works if no - in subject grrr
                                    else
                                    {
                                        int subjectindx = line.IndexOf(" - Subject: ", StringComparison.Ordinal);
                                        int authorindx = line.IndexOf(" - Author: ", StringComparison.Ordinal);
                                        makeHeader.NoteSubject = subjectindx < authorindx ?
                                            line.Substring(subjectindx + 12, authorindx - subjectindx - 12).Trim()
                                            : "** Subject Parse Error **";
                                    }
                                }
                                // get author
                                part = parts[^3];
                                if (part.StartsWith(" Author: "))
                                {
                                    makeHeader.AuthorName = part[8..].Trim();
                                    makeHeader.AuthorID = "*imported*";
                                }
                                else
                                {
                                    makeHeader.AuthorName = "** Author Parse Error **";
                                    makeHeader.AuthorID = "*imported*";
                                }
                                part = parts[^2].Trim();

                                DateTime dt = DateTime.Parse(part).ToUniversalTime();
                                makeHeader.LastEdited = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dt);
                                // skip resp

                                line = await GetLineAsync();
                                if (line.StartsWith("Tags -"))
                                {
                                    isResp = false;
                                    makeHeader.DirectorMessage = line[6..].Trim();
                                }
                                else if (line.StartsWith("Base Note Subject: "))
                                {
                                    isResp = true;  // but skip content
                                    line = await GetLineAsync();  // Should be Tag
                                    if (line.StartsWith("Tags -"))
                                    {
                                        makeHeader.DirectorMessage = line[6..].Trim();
                                    }
                                }
                                await GetLineAsync();  //skip a line
                                line = await GetLineAsync();  //first content line
                            }
                        }
                        // append line to current note
                        sb.AppendLine(line);
                    }  // end Notes 3.1

                    else if (filetype == FileType.PlatoNotes)  // PLATO iv group notes
                    {
                        int xflag = 0;
                        if (line.Contains("-------- note "))  //  note header
                            xflag = 1;
                        else if (line.Contains("-------- response "))  //  response header
                            xflag = 2;

                        if (xflag > 0) // we have note to write (maybe)
                        {
                            if (newContent is not null)  // have a note to write
                            {
                                newContent.NoteBody = sb.ToString();
                                sb.Clear();   // = new StringBuilder();
                                newContent.NoteBody = newContent.NoteBody.Replace("\r\n", "<br />");

                                if (!isResp) // base note
                                {
                                    basenotes++;
                                    newHeader = CreateNote(makeHeader, newContent.NoteBody);
                                }
                                else // resp
                                {
                                    makeHeader.NoteOrdinal = newHeader.NoteOrdinal;
                                    makeHeader.BaseNoteId = newHeader.Id;  //Fix
                                    CreateResponse(makeHeader, newContent.NoteBody);
                                }
                            }
                            newContent = new NoteContent();
                            makeHeader = new NoteHeader();

                            // now start with new note proc

                            if (xflag == 1) // get note title
                            {
                                // mark we have a base note / get title

                                line = line[16..];
                                string[] splitsx = line.Split(spaceTrim);
                                makeHeader.NoteSubject = line[splitsx[0].Length..].Trim(spaceTrim);

                                isResp = false;
                            }
                            else if (xflag == 2)
                            {
                                isResp = true;  // mark response
                                makeHeader.NoteSubject = "*none*";  // must have a title
                            }
                            if (string.IsNullOrEmpty(makeHeader.NoteSubject))  // must have a title
                                makeHeader.NoteSubject = "*none*";

                            line = await GetLineAsync(); // move to info header

                            // process header

                            // count the /s to get date format.

                            int cnt = Regex.Matches(line, "/").Count;
                            if (cnt < 1) // try next line grrr.
                            {
                                line = await GetLineAsync(); // move to info header
                                cnt = Regex.Matches(line, "/").Count;
                            }

                            line = " " + line;
                            line = line.Replace("     ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ");
                            line = line.Replace("     ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ");


                            string[] splits = line.Split(' ', '/', '.', ':', ',', ';');

                            string date;
                            if (cnt == 1)
                            {
                                date = splits[1] + "/" + splits[2] + "/" + platoBaseYear + " "
                                    + splits[3] + ":" + splits[4] + ":00";
                            }
                            else
                            {
                                date = splits[1] + "/" + splits[2] + "/19" + splits[3] + " "
                                    + splits[4] + ":" + splits[5] + ":00";
                            }
                            makeHeader.LastEdited = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.Parse(date).ToUniversalTime());

                            string group = " " + splits[^1];
                            string name = "";
                            for (int i = 4 + cnt; i < splits.Length - 1; i++)
                            {
                                name += splits[i] + " ";
                            }

                            makeHeader.AuthorName = name + "/" + group;
                            makeHeader.AuthorID = "*imported*";   //"imported";

                            await GetLineAsync(); // skip lines to get to content
                            line = await GetLineAsync(); // skip lines to get to content
                        }

                        //// pre proc above
                        sb.AppendLine(line); // collect lines of note
                    }  // end PLATO
                }  // end while

                // Cleanup after all lines in input file processed - YUCK!!

                if (filetype == FileType.NovaNet)  // NovaNET
                {
                    if (newContent is not null)  // have a note to write
                    {
                        newContent.NoteBody = sb.ToString();
                        sb.Clear();

                        newContent.NoteBody = newContent.NoteBody.Replace("\r\n", "<br />");

                        if (!isResp) // base note
                        {
                        }
                        else // resp
                        {
                            makeHeader.NoteOrdinal = newHeader.NoteOrdinal;
                            makeHeader.BaseNoteId = newHeader.Id;  //Fix
                            CreateResponse(makeHeader, newContent.NoteBody);
                        }
                    }
                }
                else if (filetype == FileType.WebNotes)  // Notes 3.1
                {
                    if (newContent is not null)  // have a note to write
                    {
                        sb.Append(' ');
                        newContent.NoteBody = sb.ToString();
                        sb.Clear();

                        if (!isResp) // base note
                        {
                            newHeader = CreateNote(makeHeader, newContent.NoteBody);
                        }
                        else // resp
                        {
                            makeHeader.NoteOrdinal = newHeader.NoteOrdinal;
                            makeHeader.BaseNoteId = newHeader.Id;  //Fix
                            CreateResponse(makeHeader, newContent.NoteBody);
                        }
                    }
                }
                else if (filetype == FileType.PlatoNotes)  // PLATO
                {
                    if (newContent is not null)  // have a note to write
                    {
                        newContent.NoteBody = sb.ToString();
                        sb.Clear();
                        newContent.NoteBody = newContent.NoteBody.Replace("\r\n", "<br />");

                        if (!isResp) // base note
                        {
                            newHeader = CreateNote(makeHeader, newContent.NoteBody);
                        }
                        else // resp
                        {
                            makeHeader.NoteOrdinal = newHeader.NoteOrdinal;
                            makeHeader.BaseNoteId = newHeader.Id;  //Fix
                            CreateResponse(makeHeader, newContent.NoteBody);
                        }
                    }

                }
            }
            catch (Exception)
            {
                file.Close();
                 throw;
            }

            file.Close();

            Output("  Basenotes: " + basenotes + "   Completed!!");

            string output = JsonConvert.SerializeObject(outp, Formatting.Indented);

            await File.WriteAllTextAsync(outputNotesFile, output);

            Output($"See Output File: {outputNotesFile}");

            return true;
        }

        /// <summary>
        /// Process NovaNET formfeed - we need to eat some lines
        /// </summary>
        /// <param name="inline">input line</param>
        /// <param name="file">StreamReader for import file</param>
        /// <returns>System.String.</returns>
        public async Task<string> CheckFf(string inline, StreamReader file)
        {
            //if (counter > 321)
            //    ;

            if (inline.Length == 1)
            {
                char[] mychars = inline.ToCharArray();
                if (mychars[0] == Ff)
                {
                    await GetLineAsync();
                    await GetLineAsync();
                    await GetLineAsync();
                    await GetLineAsync();
                    string it = await GetLineAsync();

                    //Console.WriteLine($">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>FormFeed at: {counter}");

                    return string.IsNullOrEmpty(it) ? string.Empty : it;
                }
            }
            return inline;
        }


        /// <summary>
        /// Outputs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void Output(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Get line as an asynchronous operation.
        /// Centralized to for line counting.
        /// </summary>
        /// <returns>Line read from input file.</returns>
        private async Task<string?> GetLineAsync()
        {
            counter++;
            //Output($"Line: {counter}");   // debugging
            return await file.ReadLineAsync();
        }

        private int currentOrd = 0;
        private long currentId = 0;
        private long baseId = 0;


        /// <summary>
        /// Creates the note.  Adds a base note to the NoteHeader.List
        /// </summary>
        /// <param name="makeHeader">The basic header to add.</param>
        /// <param name="NoteBody">The note body.</param>
        /// <returns>NoteHeader.</returns>
        private NoteHeader CreateNote(NoteHeader makeHeader, string NoteBody)
        {
            makeHeader.NoteOrdinal = ++currentOrd;
            makeHeader.Id = baseId = ++currentId;
            makeHeader.BaseNoteId = 0;
            makeHeader.ArchiveId = 0;
            makeHeader.RefId = 0;
            makeHeader.ResponseOrdinal = 0;
            makeHeader.ResponseCount = 0;

            makeHeader.Content = new() { Id = 0, NoteHeaderId = 0, NoteBody = NoteBody };

            outp.NoteHeaders.List.Add(makeHeader);

            NoteHeader ret = new(makeHeader);

            //Output($"-----------------After Base Note = {currentOrd}");

            return ret;
        }

        /// <summary>
        /// Creates the response.  Adds a Response item to the most recent base note.
        /// </summary>
        /// <param name="makeHeader">The make header.</param>
        /// <param name="NoteBody">The note body.</param>
        private void CreateResponse(NoteHeader makeHeader, string NoteBody)
        {
            NoteHeader currbase = outp.NoteHeaders.List.Single(p => p.Id == baseId);
            currbase.ResponseCount++;

            makeHeader.NoteOrdinal = currbase.NoteOrdinal;
            makeHeader.Id = ++currentId;
            makeHeader.BaseNoteId = currbase.Id;
            makeHeader.ArchiveId = 0;
            makeHeader.RefId = 0;
            makeHeader.ResponseOrdinal = currbase.ResponseCount;
            makeHeader.ResponseCount = 0;

            makeHeader.Content = new() { Id = 0, NoteHeaderId = 0, NoteBody = NoteBody };


            if (currbase.Responses is null)
                currbase.Responses = new();

            currbase.Responses.List.Add(makeHeader);

            //Output($"-----------------After Note = {currentOrd}, {currbase.ResponseCount}");
        }
    }

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

}
