using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class SemanticFact
    {
        public string Subject { get; set; } = string.Empty;
        public string Verb { get; set; } = string.Empty;
        public string Object { get; set; } = string.Empty;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SemanticFact()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public SemanticFact(SemanticFact src)
        {
            if (src != null)
            {
                Subject = src.Subject;
                Verb = src.Verb;
                Object = src.Object;
            }
        }
    }
}
