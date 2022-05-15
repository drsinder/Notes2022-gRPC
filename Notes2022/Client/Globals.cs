// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-19-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-06-2022
// ***********************************************************************
// <copyright file="Globals.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Grpc.Core;
using Notes2022.Proto;
using Notes2022.Client.Shared;
using Notes2022.Client.Menus;
using Notes2022.Client.Pages.Admin;
using Notes2022.Client.Pages;
using System.Text;

namespace Notes2022.Client
{
    /// <summary>
    /// Class Globals.
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// The login display object
        /// </summary>
        public static LoginDisplay? LoginDisplay = null;
        /// <summary>
        /// The nav menu object
        /// </summary>
        public static NavMenu? NavMenu = null;
        /// <summary>
        /// The notes files admin object
        /// </summary>
        public static NotesFilesAdmin? NotesFilesAdmin = null;
        /// <summary>
        /// Gets the access other identifier.
        /// </summary>
        /// <value>The access other identifier.</value>
        public static string AccessOtherId { get; } = "Other";

        /// <summary>
        /// Gets the cookie name.
        /// </summary>
        /// <value>The cookie.</value>
        public static string Cookie { get; } = "notes2022login";

        /// <summary>
        /// Gets or sets the return URL.
        /// </summary>
        /// <value>The return URL.</value>
        public static string returnUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the goto note.
        /// </summary>
        /// <value>The goto note.</value>
        public static long GotoNote { get; set; } = 0;

        /// <summary>
        /// GMT to Local.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns>DateTime.</returns>
        public static DateTime LocalTimeBlazor(DateTime dt)
        {
            int OHours = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours;
            int OMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Minutes;

            return dt.AddHours(OHours * 2).AddMinutes(OMinutes * 2);    // *2 needed because we go in and out of unix utc time
        }

        /// <summary>
        /// Converts to a Universal time
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns>DateTime.</returns>
        public static DateTime UTimeBlazor(DateTime dt)
        {
            int OHours = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours;
            int OMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Minutes;

            return dt.AddHours(-OHours).AddMinutes(-OMinutes);    // *2 needed because we go in and out of unix utc time
        }


        /// <summary>
        /// Base64 Encodes the plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>System.String.</returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Base64s decodes the encoded string.
        /// </summary>
        /// <param name="encodedString">The encoded string.</param>
        /// <returns>System.String.</returns>
        public static string Base64Decode(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

    }
}
