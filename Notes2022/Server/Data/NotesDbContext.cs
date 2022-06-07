// ***********************************************************************
// Assembly         : Notes2022.Server
// Author           : Dale Sinder
// Created          : 04-19-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 04-16-2022
// ***********************************************************************
// <copyright file="NotesDbContext.cs" company="Notes2022.Server">
//     Copyright (c) 2022 Dale Sinder. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notes2022.Server.Proto;

namespace Notes2022.Server.Data
{
    /// <summary>
    /// Class NotesDbContext.
    /// Implements the <see cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{Notes2022.Server.Entities.ApplicationUser}" />
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{Notes2022.Server.Entities.ApplicationUser}" />
    public partial class NotesDbContext : IdentityDbContext<ApplicationUser>
    {
        // Select classes from proto messages to be included in database
        // ApplucationUser is included by inheritence.

        /// <summary>
        /// Gets or sets the note file.
        /// </summary>
        /// <value>The note file.</value>
        public DbSet<NoteFile> NoteFile { get; set; }

        /// <summary>
        /// Gets or sets the note header.
        /// </summary>
        /// <value>The note header.</value>
        public DbSet<NoteHeader> NoteHeader { get; set; }

        /// <summary>
        /// Gets or sets the content of the note.
        /// </summary>
        /// <value>The content of the note.</value>
        public DbSet<NoteContent> NoteContent { get; set; }
        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public DbSet<Tags> Tags { get; set; }

        /// <summary>
        /// Gets or sets the note access.
        /// </summary>
        /// <value>The note access.</value>
        public DbSet<NoteAccess> NoteAccess { get; set; }

        /// <summary>
        /// Gets or sets the sequencer.
        /// </summary>
        /// <value>The sequencer.</value>
        public DbSet<Sequencer> Sequencer { get; set; }

        public DbSet<JsonData> JsonData { get; set; }


        public DbSet<ActiveUsers> ActiveUsers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotesDbContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public NotesDbContext(DbContextOptions<NotesDbContext> options) : base(options)
        {

        }

        /// <summary>
        /// Called when [model creating].
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // Add keys and indexes.

            builder.Entity<NoteContent>()
                .HasKey(new string[] { "Id" });

            builder.Entity<NoteContent>()
                .HasIndex(new string[] { "NoteHeaderId" });


            builder.Entity<JsonData>()
                .HasKey(new string[] { "Id" });

            builder.Entity<ActiveUsers>()
                .HasKey(new string[] { "Id" });


            builder.Entity<NoteAccess>()
                .HasKey(new string[] { "UserID", "NoteFileId", "ArchiveId" });

            builder.Entity<NoteHeader>()
                .HasIndex(new string[] { "NoteFileId" });

            builder.Entity<NoteHeader>()
                .HasIndex(new string[] { "NoteFileId", "ArchiveId" });

            builder.Entity<NoteHeader>()
                .HasIndex(new string[] { "LinkGuid" });

            builder.Entity<Tags>()
                .HasKey(new string[] { "Tag", "NoteHeaderId" });

            builder.Entity<Tags>()
                .HasIndex(new string[] { "NoteFileId" });

            builder.Entity<Tags>()
                .HasIndex(new string[] { "NoteFileId", "ArchiveId" });

            builder.Entity<Sequencer>()
                .HasKey(new string[] { "UserId", "NoteFileId" });

            // Relationships

            builder.Entity<NoteHeader>()
                .HasOne<NoteFile>()
                .WithMany()
                .HasForeignKey(x => x.NoteFileId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<NoteContent>()
                .HasOne<NoteHeader>()
                .WithMany()     // really only one but I can't get that to work
                .HasForeignKey(x => x.NoteHeaderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<NoteAccess>()
                .HasOne<NoteFile>()
                .WithMany()
                .HasForeignKey(x => x.NoteFileId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Sequencer>()
                .HasOne<NoteFile>()
                .WithMany()
                .HasForeignKey(x => x.NoteFileId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Tags>()
                .HasOne<NoteFile>()
                .WithMany()
                .HasForeignKey(x => x.NoteFileId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);


            //builder.Entity<NoteFile>()
            //    .HasOne<ApplicationUser>()
            //    .WithMany()
            //    .HasForeignKey(x => x.OwnerId)
            //    .OnDelete(DeleteBehavior.NoAction);


            //builder.Entity<NoteHeader>()
            //    .HasOne<ApplicationUser>()
            //    .WithMany()
            //    .HasForeignKey(x => x.AuthorID)
            //    .OnDelete(DeleteBehavior.NoAction);

            //builder.Entity<Sequencer>()
            //    .HasOne<ApplicationUser>()
            //    .WithMany()
            //    .HasForeignKey(x => x.UserId)
            //    .OnDelete(DeleteBehavior.NoAction);

            //builder.Entity<NoteAccess>()
            //    .HasOne<ApplicationUser>()
            //    .WithMany()
            //    .HasForeignKey(x => x.UserID)
            //    .OnDelete(DeleteBehavior.NoAction);



            // Define conversions to and from Google.Protobuf.WellKnownTypes.Timestamp for DateTime

            builder.Entity<NoteFile>()
              .Property(m => m.LastEdited)
              .HasConversion(v => v.ToDateTimeOffset(), v => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(v));


            builder.Entity<NoteHeader>()
              .Property(m => m.LastEdited)
              .HasConversion(v => v.ToDateTimeOffset(), v => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(v));

            builder.Entity<NoteHeader>()
              .Property(m => m.ThreadLastEdited)
              .HasConversion(v => v.ToDateTimeOffset(), v => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(v));

            builder.Entity<NoteHeader>()
                .Property(m => m.CreateDate)
                .HasConversion(v => v.ToDateTimeOffset(), v => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(v));

            builder.Entity<Sequencer>()
              .Property(m => m.LastTime)
              .HasConversion(v => v.ToDateTimeOffset(), v => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(v));

            builder.Entity<Sequencer>()
              .Property(m => m.StartTime)
              .HasConversion(v => v.ToDateTimeOffset(), v => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(v));

            builder.Entity<ActiveUsers>()
              .Property(m => m.StartTime)
              .HasConversion(v => v.ToDateTimeOffset(), v => Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(v));



            // Exclude three columns from NoteHeader Table in DB but make them available for use in gRPC.

            builder.Entity<NoteHeader>().Ignore(c => c.Responses);
            builder.Entity<NoteHeader>().Ignore(c => c.Content);
            builder.Entity<NoteHeader>().Ignore(c => c.Tags);
        }
    }
}
