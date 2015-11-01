using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Unbrickable.Controllers
{
    public class CSVTestWriter
    {
        public CSVTestWriter()
        { }

        public string WriteRowString(CSVRowModel row)
        {
            StringBuilder builder = new StringBuilder();
            bool firstColumn = true;
            foreach (string value in row)
            {
                // Add separator if this isn't the first value
                if (!firstColumn)
                    builder.Append(',');

                // Implement special handling for values that contain comma or quote
                // Enclose in quotes and double up any double quotes
                if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                    builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                else
                    builder.Append(value);

                firstColumn = false;
            }

            return builder.ToString();
        }
    }
}