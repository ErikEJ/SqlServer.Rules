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

        private ResourceManager resourceManager;
        private string displayName;
        private string descriptionValue;

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

        public Assembly GetAssembly()
        {
            return GetType().Assembly;
        }

        private void EnsureResourceManagerInitialized()
        {
            var resourceAssembly = GetAssembly();

            try
            {
                resourceManager = new ResourceManager(ResourceBaseName, resourceAssembly);
            }
            catch (Exception ex)
            {
                var msg = string.Format(CultureInfo.CurrentCulture, Resources.CannotCreateResourceManager, ResourceBaseName, resourceAssembly);
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
            return resourceManager.GetString(resourceId, CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Overrides the standard DisplayName and looks up its value inside a resources file
        /// </summary>
        public override string DisplayName
        {
            get
            {
                if (displayName == null)
                {
                    displayName = GetResourceString(DisplayNameResourceId);
                }

                return displayName;
            }
        }

        /// <summary>
        /// Overrides the standard Description and looks up its value inside a resources file
        /// </summary>
        public override string Description
        {
            get
            {
                if (descriptionValue == null)
                {
                    // Using the descriptionResourceId as the key for looking up the description in the resources file.
                    descriptionValue = GetResourceString(DescriptionResourceId);
                }

                return descriptionValue;
            }
        }
    }
}
