using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryEnforcer
{
    public static class StringHelper
    {

        /// <summary>
        /// String extension method that returns a substring starting after the first instance of the specified search text.
        /// </summary>
        /// <param name="str">String instance on which this method is being called.</param>
        /// <param name="searchText">Search text.</param>
        /// <returns>Substring after the first instance of the specified search text, or null if the search text was not found.</returns>
        public static string AllAfter(this string str, string searchText)
        {
            int index = str.IndexOf(searchText);

            if (index == -1)
            {
                return null;
            }
            else if (index == str.Length - searchText.Length)
            {
                return null;
            }
            else
            {
                return str.Substring(index + searchText.Length);
            }
        }

        /// <summary>
        /// String extension method that returns a copy of itself truncated at the specified search text.
        /// </summary>
        /// <param name="str">String instance on which this method is being called.</param>
        /// <param name="searchText">Search text.</param>
        /// <returns>Truncated string, or null if the search text was not found.</returns>
        public static string TruncateAt(this string str, string searchText)
        {
            int index = str.IndexOf(searchText);

            if (index == -1)
            {
                return str;
            }
            else if (index == 0)
            {
                return null;
            }
            else
            {
                return str.Substring(0, index);
            }
        }

        /// <summary>
        /// String extension method that returns a copy of itself truncated at the last instance of the specified search text.
        /// </summary>
        /// <param name="str">String instance on which this method is being called.</param>
        /// <param name="searchText">Search text.</param>
        /// <returns>Truncated string, or null if the search text was not found.</returns>
        public static string TruncateAtLast(this string str, string searchText)
        {
            int index = str.LastIndexOf(searchText);

            if (index == -1)
            {
                return str;
            }
            else if (index == 0)
            {
                return null;
            }
            else
            {
                return str.Substring(0, index);
            }
        }
    }
}
