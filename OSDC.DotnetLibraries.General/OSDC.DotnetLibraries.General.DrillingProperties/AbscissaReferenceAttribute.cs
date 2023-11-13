﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class AbscissaReferenceAttribute : ReferenceAttribute
    {
        public CommonProperty.AbscissaReferenceType ReferenceType { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="val"></param>
        public AbscissaReferenceAttribute(CommonProperty.AbscissaReferenceType val)
        {
            this.ReferenceType = val;
        }

    }
}
