using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Comp
{
    public partial class FileButton
    {
        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        [Parameter]
        public NoteFile NoteFile { get; set; }

        /// <summary>
        /// Gets or sets the navigation.
        /// </summary>
        /// <value>The navigation.</value>
        [Inject]
        NavigationManager Navigation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref = "FileButton"/> class.
        /// </summary>
        public FileButton()
        {
        }

        /// <summary>
        /// Called when [click].
        /// </summary>
        protected void OnClick()
        {
            Navigation.NavigateTo("noteindex/" + NoteFile.Id);
        }
    }
}