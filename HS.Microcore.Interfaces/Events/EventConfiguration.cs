﻿using System.Text.RegularExpressions;
using HS.Microcore.Interfaces.Configuration;

namespace HS.Microcore.Interfaces.Events
{

    [ConfigurationRoot("logging", RootStrategy.ReplaceClassNameWithPath)]
    public class EventConfiguration : IConfigObject
    {
        /// <summary>
        /// E.g. "^[2-4].*$" will filter 2xx - 4xx error codes and leave stack traces for 5xx errors.
        /// </summary>
        public string ExcludeStackTraceForErrorCodeRegex
        {
            get => ExcludeStackTraceRule.ToString();
            set => ExcludeStackTraceRule = new Regex(value, RegexOptions.Compiled);
        }

        public Regex ExcludeStackTraceRule { get; set; } = new Regex("^$", RegexOptions.Compiled);

        public bool ExcludeParams { get; set; } = false;

        public int ParamTruncateLength { get; set; } = 16 * 1024;
    }

}
