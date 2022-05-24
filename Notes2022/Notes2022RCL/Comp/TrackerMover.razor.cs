using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Notes2022RCL;
using Notes2022.Proto;
using Blazored;
using Blazored.Modal;
using Blazored.Modal.Services;
using W8lessLabs.Blazor.LocalFiles;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.LinearGauge;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.SplitButtons;
using Syncfusion.Blazor.Calendars;
using Notes2022RCL.Pages;

namespace Notes2022RCL.Comp
{
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
        /// <param name = "args">The <see cref = "MenuEventArgs"/> instance containing the event data.</param>
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
        /// <param name = "a">a.</param>
        /// <param name = "b">The b.</param>
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