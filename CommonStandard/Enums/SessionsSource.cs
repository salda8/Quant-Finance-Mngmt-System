using System;
using System.ComponentModel;

namespace Common.Enums
{
    [Serializable]
    public enum SessionsSource : int
    {


        /// <summary>
        /// Template
        /// </summary>
        [Description("Template")]
        Template = 0,

        /// <summary>
        /// Custom
        /// </summary>
        [Description("Custom")]
        Custom = 1,
    }
}