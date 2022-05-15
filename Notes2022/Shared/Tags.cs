using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notes2022.Proto
{
    public partial class Tags
    {
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        //public override string? ToString()
        //{
        //    return Tag;
        //}

        /// <summary>
        /// Lists to string.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>System.String.</returns>
        public static string ListToString(List<Tags> list)
        {
            string s = string.Empty;
            if (list is null || list.Count < 1)
                return s;

            foreach (Tags tag in list)
            {
                s += tag.Tag + " ";
            }

            return s.TrimEnd(' ');
        }

        /// <summary>
        /// Strings to list.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>List&lt;Tags&gt;.</returns>
        public static List<Tags> StringToList(string s)
        {
            List<Tags> list = new();

            if (string.IsNullOrEmpty(s) || s.Length < 1)
                return list;

            string[] tags = s.Split(',', ';', ' ');

            if (tags is null || tags.Length < 1)
                return list;

            foreach (string t in tags)
            {
                string r = t.Trim().ToLower();
                list.Add(new Tags() { Tag = r });
            }

            return list;
        }

        /// <summary>
        /// Strings to list.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="hId">The h identifier.</param>
        /// <param name="fId">The f identifier.</param>
        /// <param name="arcId">The arc identifier.</param>
        /// <returns>List&lt;Tags&gt;.</returns>
        public static List<Tags> StringToList(string s, long hId, int fId, int arcId)
        {
            List<Tags> list = new();

            if (string.IsNullOrEmpty(s) || s.Length < 1)
                return list;

            string[] tags = s.Split(',', ';', ' ');

            if (tags is null || tags.Length < 1)
                return list;

            foreach (string t in tags)
            {
                string r = t.Trim().ToLower();
                list.Add(new Tags() { Tag = r, NoteHeaderId = hId, NoteFileId = fId, ArchiveId = arcId });
            }

            return list;
        }

        /// <summary>
        /// Gets the tags list.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>List&lt;Tags&gt;.</returns>
        public static List<Tags> GetTagsList(TagsList other)
        {
            List<Tags> list = new();
            foreach (Tags t in other.List)
            {
                list.Add((t));
            }
            return list;
        }

        /// <summary>
        /// Gets the g tags list.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>GTagsList.</returns>
        public static TagsList GetGTagsList(List<Tags> other)
        {
            TagsList list = new();
            foreach (Tags t in other)
            {
                list.List.Add(t);            }
            return list;
        }


    }
}
