/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments & document reference
 * 
 * docs.microsoft
 * https://docs.microsoft.com/en-us/dotnet/api/system.windows.automation.treescope?view=netframework-4.7.2
 */
using System;

namespace OpenQA.Selenium.Uia
{
    /// <summary>
    /// contains values that specify the scope of elements within the UI Automation tree
    /// </summary>
    [Flags]
    public enum TreeScope
    {
        None = 0,

        /// <summary>
        /// specifies that the search include the element itself
        /// </summary>
        Element = 1,

        /// <summary>
        /// specifies that the search include the element's immediate children
        /// </summary>
        Children = 2,

        /// <summary>
        /// specifies that the search include the element's descendants, including children
        /// </summary>
        Descendants = 4,

        /// <summary>
        /// specifies that the search include the root of the search and all descendants
        /// </summary>
        Subtree = Element | Children | Descendants,

        /// <summary>
        /// specifies that the search include the element's parent. not supported
        /// </summary>
        Parent = 8,

        /// <summary>
        /// specifies that the search include the element's ancestors, including the parent.
        /// not supported
        /// </summary>
        Ancestors = 16
    }
}