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
// Name: Clock.razor.cs
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
// <copyright file="Clock.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using System.Timers;

namespace Notes2022RCL.Comp
{
    /// <summary>
    /// Class Clock.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class Clock
    {
        /// <summary>
        /// Gets or sets the interval.
        /// </summary>
        /// <value>The interval.</value>
        [Parameter]
        public int Interval { get; set; } = 1000;
#pragma warning disable IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets the timer2.
        /// </summary>
        /// <value>The timer2.</value>
        private System.Timers.Timer timer2 { get; set; }

        /// <summary>
        /// Gets or sets the mytime.
        /// </summary>
        /// <value>The mytime.</value>
        private DateTime mytime { get; set; } = DateTime.Now;
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Method invoked after each time the component has been rendered.
        /// </summary>
        /// <param name="firstRender">Set to <c>true</c> if this is the first time <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> has been invoked
        /// on this component instance; otherwise <c>false</c>.</param>
        /// <remarks>The <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRender(System.Boolean)" /> and <see cref="M:Microsoft.AspNetCore.Components.ComponentBase.OnAfterRenderAsync(System.Boolean)" /> lifecycle methods
        /// are useful for performing interop, or interacting with values received from <c>@ref</c>.
        /// Use the <paramref name="firstRender" /> parameter to ensure that initialization work is only performed
        /// once.</remarks>
        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                mytime = DateTime.Now;
                timer2 = new System.Timers.Timer(Interval);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

                timer2.Elapsed += TimerTick2;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

                timer2.Enabled = true;
            }
        }

        /// <summary>
        /// Timers the tick2.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs" /> instance containing the event data.</param>
        protected void TimerTick2(Object source, ElapsedEventArgs e)
        {
            mytime = DateTime.Now;
            //StateHasChanged();
            try
            {
                this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }
    }
}