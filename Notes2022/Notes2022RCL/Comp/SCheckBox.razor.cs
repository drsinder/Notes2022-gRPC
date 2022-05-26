using Microsoft.AspNetCore.Components;
using Notes2022.Proto;
using Notes2022RCL.Pages;

namespace Notes2022RCL.Comp
{
    public partial class SCheckBox
    {
        /// <summary>
        /// Gets or sets the tracker.
        /// </summary>
        /// <value>The tracker.</value>
        [Parameter]
        public Tracker Tracker { get; set; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>The file identifier.</value>
        [Parameter]
#pragma warning disable IDE1006 // Naming Styles

        public int fileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value><c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        [Parameter]
        public bool isChecked { get; set; }

#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public SCheckModel Model { get; set; }

        /// <summary>
        /// Method invoked when the component has received parameters from its parent in
        /// the render tree, and the incoming values have been assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            Model = new SCheckModel { IsChecked = isChecked, FileId = fileId };
        }

        /// <summary>
        /// Called when [click].
        /// </summary>
        public async Task OnClick()
        {
            isChecked = !isChecked;
            if (isChecked) // create item
            {
                await Client.CreateSequencerAsync(Model, myState.AuthHeader);
            }
            else // delete it
            {
                await Client.DeleteSequencerAsync(Model, myState.AuthHeader);
            }

            await Tracker.Shuffle();
        }
    }
}