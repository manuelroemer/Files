﻿using System;

namespace Files
{

    /// <summary>
    ///     Defines whether a path is relative or absolute.
    /// </summary>
    public enum PathKind
    {

        /// <summary>
        ///     The path is an absolute path.
        /// </summary>
        Absolute = UriKind.Absolute,

        /// <summary>
        ///     The path is a relative path.
        /// </summary>
        Relative = UriKind.Relative,

    }

}
