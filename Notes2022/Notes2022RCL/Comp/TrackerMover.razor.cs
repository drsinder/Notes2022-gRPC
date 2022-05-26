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
// Name: TrackerMover.razor.cs
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
// <copyright file="TrackerMover.razor.cs" company="Notes2022RCL">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>
using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Notes2022RCL.Pages;
using Syncfusion.Blazor.SplitButtons;

namespace Notes2022RCL.Comp
{
    /// <summary>
    /// Class TrackerMover.
    /// Implements the <see cref="ComponentBase" />
    /// </summary>
    /// <seealso cref="ComponentBase" />
    public partial class TrackerMover
    {
        /// <summary>
        /// Who are we
        /// </summary>
        /// <value>The current tracker.</value>

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Parameter]
        public Sequencer CurrentTracker { get; set; }

        /// <summary>
        /// List of trackers
        /// </summary>
        /// <value>The trackers.</value>
        [Parameter]
        public List<Sequencer> Trackers { get; set; }

        /// <summary>
        /// Our container/caller
        /// </summary>
        /// <value>The tracker.</value>
        [Parameter]
        public Tracker Tracker { get; set; }

        /// <summary>
        /// List of items before me
        /// </summary>
        /// <value>The befores.</value>
        List<Sequencer> befores { get; set; }

        /// <summary>
        /// List of items after me
        /// </summary>
        /// <value>The afters.</value>
        List<Sequencer> afters { get; set; }

        /// <summary>
        /// Item just before me
        /// </summary>
        /// <value>The before.</value>
        Sequencer before { get; set; }

        /// <summary>
        /// Item just after me
        /// </summary>
        /// <value>The after.</value>
        Sequencer after { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            // find before and after items
            if (CurrentTracker is not null)
            {
                befores = Trackers.Where(p => p.Ordinal < CurrentTracker.Ordinal).OrderByDescending(p => p.Ordinal).ToList();
                if (befores is not null && befores.Count > 0)
                    before = befores.First();
                afters = Trackers.Where(p => p.Ordinal > CurrentTracker.Ordinal).OrderBy(p => p.Ordinal).ToList();
                if (afters is not null && afters.Count > 0)
                    after = afters.First();
            }
        }

        /// <summary>
        /// Move an item as wished
        /// </summary>
        /// <param name="args">The <see cref="MenuEventArgs" /> instance containing the event data.</param>
        private async Task ItemSelected(MenuEventArgs args)
        {
            switch (args.Item.Text)
            {
                case "Up":
                    if (before is null)
                        return;
                    await Swap(before, CurrentTracker);
                    break;
                case "Down":
                    if (after is null)
                        return;
                    await Swap(after, CurrentTracker);
                    break;
                case "Top":
                    if (before is null)
                        return;
                    await Swap(befores[befores.Count - 1], CurrentTracker);
                    break;
                case "Bottom":
                    if (after is null)
                        return;
                    await Swap(afters[afters.Count - 1], CurrentTracker);
                    break;
                default:
                    break;
            }

            await Tracker.Shuffle();
        }

        /// <summary>
        /// Swap the postion of two trackers
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        private async Task Swap(Sequencer a, Sequencer b)
        {
            int aord = a.Ordinal;
            int bord = b.Ordinal;
            a.Ordinal = bord;
            b.Ordinal = aord;
            await Client.UpdateSequencerOrdinalAsync(a, myState.AuthHeader);
            await Client.UpdateSequencerOrdinalAsync(b, myState.AuthHeader);
        }
    }
}