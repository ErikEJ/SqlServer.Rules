//------------------------------------------------------------------------------
// <copyright company="Microsoft">
//   Copyright 2013 Microsoft
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.SqlServer.Dac.CodeAnalysis;

namespace TSQLSmellSCA
{
    /// <summary>
    /// This is an example of a localized export attribute. These can be very useful in the case where
    /// you need localized resource strings for things like the display name and description of a rule.
    ///
    /// All of the export attributes provided by the DAC API can be localized, and internally a very
    /// similar structure is used. If you do not need to perform localization of any resources it's easier to use the
    /// <see cref="ExportCodeAnalysisRuleAttribute"/> directly.
    ///
    /// </summary>
    internal sealed class LocalizedExportCodeAnalysisRuleAttribute : ExportCodeAnalysisRuleAttribute
    {
        public string ResourceBaseName { get; }
        public string DisplayNameResourceId { get; }
        public string DescriptionResourceId { get; }

        private ResourceManager _resourceManager;
        private string _displayName;
        private string _descriptionValue;

        /// <summary>
        /// Creates the attribute, with the specified rule ID, the fully qualified
        /// name of the resource file that will be used for looking up display name
        /// and description, and the Ids of those resources inside the resource file.
        /// </summary>
        public LocalizedExportCodeAnalysisRuleAttribute(
            string id,
            string resourceBaseName,
            string displayNameResourceId,
            string descriptionResourceId)
            : base(id, null)
        {
            ResourceBaseName = resourceBaseName;
            DisplayNameResourceId = displayNameResourceId;
            DescriptionResourceId = displayNameResourceId; // descriptionResourceId;
        }

        /// <summary>
        /// Rules in a different assembly would need to overwrite this
        /// </summary>
        /// <returns></returns>
        public Assembly GetAssembly()
        {
            return GetType().Assembly;
        }

        private void EnsureResourceManagerInitialized()
        {
            var resourceAssembly = GetAssembly();

            try
            {
                _resourceManager = new ResourceManager(ResourceBaseName, resourceAssembly);
            }
            catch (Exception ex)
            {
                var msg = String.Format(CultureInfo.CurrentCulture, Resources.CannotCreateResourceManager, ResourceBaseName, resourceAssembly);
                throw new RuleException(msg, ex);
            }
        }

        private string GetResourceString(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                return string.Empty;
            }

            EnsureResourceManagerInitialized();
            return _resourceManager.GetString(resourceId, CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Overrides the standard DisplayName and looks up its value inside a resources file
        /// </summary>
        public override string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    _displayName = GetResourceString(DisplayNameResourceId);
                }

                return _displayName;
            }
        }

        /// <summary>
        /// Overrides the standard Description and looks up its value inside a resources file
        /// </summary>
        public override string Description
        {
            get
            {
                if (_descriptionValue == null)
                {
                    // Using the descriptionResourceId as the key for looking up the description in the resources file.
                    _descriptionValue = GetResourceString(DescriptionResourceId);
                }

                return _descriptionValue;
            }
        }
    }
}
