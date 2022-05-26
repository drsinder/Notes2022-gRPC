using Notes2022.Proto;

namespace Notes2022RCL.Pages
{
    public partial class Tracker
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the stuff.
        /// </summary>
        /// <value>The stuff.</value>
        private List<NoteFile> stuff { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        private List<NoteFile> files { get; set; }

        /// <summary>
        /// Gets or sets the trackers.
        /// </summary>
        /// <value>The trackers.</value>
        private List<Sequencer> trackers { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            trackers = (await Client.GetSequencerAsync(new NoRequest(), myState.AuthHeader)).List.ToList();
            HomePageModel model = await Client.GetHomePageModelAsync(new NoRequest(), myState.AuthHeader);
            stuff = model.NoteFiles.List.OrderBy(p => p.NoteFileName).ToList();
            await Shuffle();
        }

        /// <summary>
        /// Shuffles this instance.
        /// </summary>
        public async Task Shuffle()
        {
            files = new List<NoteFile>();
            trackers = (await Client.GetSequencerAsync(new NoRequest(), myState.AuthHeader)).List.ToList();
            if (trackers is not null)
            {
                trackers = trackers.OrderBy(p => p.Ordinal).ToList();
                foreach (var tracker in trackers)
                {
#pragma warning disable CS8604 // Possible null reference argument.

                    files.Add(stuff.Find(p => p.Id == tracker.NoteFileId));
#pragma warning restore CS8604 // Possible null reference argument.

                }
            }

            if (stuff is not null && stuff.Count > 0)
            {
                foreach (var s in stuff)
                {
                    if (files.Find(p => p.Id == s.Id) is null)
                        files.Add(s);
                }
            }

            try
            {
                await this.InvokeAsync(() => this.StateHasChanged());
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        private void Cancel()
        {
            NavMan.NavigateTo("");
        }
    }
}