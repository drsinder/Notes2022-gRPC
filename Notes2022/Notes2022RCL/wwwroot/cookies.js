// ***********************************************************************
// Assembly         : Notes2022.Client
// Author           : Dale Sinder
// Created          : 04-30-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-09-2022
//
// Copyright © 2022, Dale Sinder
//
// Name: cookies.js
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
// <copyright file="cookies.js" company="Notes2022.Client">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>

export function CreateCookie(name, value, hours) {
    var expires;
    if (hours) {
            /// <var>The date</var>
            var date = new Date();
            date.setTime(date.getTime() + (hours * 60 * 60 * 1000));
            expires = "; Expires=" + date.toUTCString();
        }
        else {
            expires = "";
        }
        document.cookie = name + "=" + value + expires + "; path=/";
    }

export function ReadCookie(cname) {
    var name = cname + "=";
    /// <var>The decoded cookie</var>
    var decodedCookie = decodeURIComponent(document.cookie);
    /// <var>The ca</var>
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        /// <var>The c</var>
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}
