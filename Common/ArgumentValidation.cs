﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jamb.Common
{
    static class ArgumentValidation
    {
        /// <summary>
        /// Validates given arguments agains null values.
        /// </summary>
        public static void ValidateParametersForNullValues(params object[] array)
        {
            foreach (object obj in array)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException(nameof(obj));
                }
            }
        }
    }
}
