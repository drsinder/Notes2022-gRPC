using Microsoft.AspNetCore.Components;
using Notes2022.Proto;

namespace Notes2022RCL.Panels
{
    public partial class Versions
    {
        /// <summary>
        /// These four parameters identify the note
        /// </summary>
        /// <value>The file identifier.</value>
        [Parameter]
        public int FileId { get; set; }

        /// <summary>
        /// Gets or sets the note ordinal.
        /// </summary>
        /// <value>The note ordinal.</value>
        [Parameter]
        public int NoteOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the response ordinal.
        /// </summary>
        /// <value>The response ordinal.</value>
        [Parameter]
        public int ResponseOrdinal { get; set; }

        /// <summary>
        /// Gets or sets the arc identifier.
        /// </summary>
        /// <value>The arc identifier.</value>
        [Parameter]
        public int ArcId { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>The headers.</value>
        protected NoteHeaderList Headers { get; set; }

        /// <summary>
        /// Gets or sets the client.
        /// </summary>
        /// <value>The client.</value>
        [Inject]
        Notes2022Server.Notes2022ServerClient Client { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref = "Versions"/> class.
        /// </summary>
        public Versions()
        {
        }

        /// <summary>
        /// On parameters set as an asynchronous operation.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task OnParametersSetAsync()
        {
            Headers = await Client.GetVersionsAsync(new GetVersionsRequest()
            { FileId = FileId, NoteOrdinal = NoteOrdinal, ResponseOrdinal = ResponseOrdinal, ArcId = ArcId }, myState.AuthHeader);
        }
    }
}