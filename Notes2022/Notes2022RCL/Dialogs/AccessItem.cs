// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-29-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 04-29-2022
// ***********************************************************************
// <copyright file="AccessItem.cs" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
/*--------------------------------------------------------------------------
    **
    ** Copyright © 2022, Dale Sinder
    **
    ** Name: AccessItem.cs
    **
    ** Description:
    **      USed for managing access
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
    **  If not, see<http://www.gnu.org/licenses/gpl-3.0.txt>.
    **
    **--------------------------------------------------------------------------*/


using Notes2022.Proto;

namespace Notes2022RCL.Dialogs
{
    /// <summary>
    /// Enum AccessX
    /// </summary>
    public enum AccessX
    {
        /// <summary>
        /// The read access
        /// </summary>
        ReadAccess,
        /// <summary>
        /// The respond
        /// </summary>
        Respond,
        /// <summary>
        /// The write
        /// </summary>
        Write,
        /// <summary>
        /// The set tag
        /// </summary>
        SetTag,
        /// <summary>
        /// The delete edit
        /// </summary>
        DeleteEdit,
        /// <summary>
        /// The view access
        /// </summary>
        ViewAccess,
        /// <summary>
        /// The edit access
        /// </summary>
        EditAccess
    }

    /// <summary>
    /// Used for editing an access token segment (one flag)
    /// </summary>
    public class AccessItem
    {
        /// <summary>
        /// The whole token
        /// </summary>
        /// <value>The item.</value>
        public NoteAccess Item { get; set; }

        /// <summary>
        /// Indicates which segment we are dealing with
        /// </summary>
        /// <value>The which.</value>
        public AccessX which { get; set; }

        /// <summary>
        /// Is it currently checked?
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        public bool isChecked { get; set; }

        /// <summary>
        /// Can current user change it?
        /// </summary>
        /// <value><c>true</c> if this instance can edit; otherwise, <c>false</c>.</value>
        public bool canEdit { get; set; }
    }

}
