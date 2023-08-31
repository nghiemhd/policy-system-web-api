using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SingLife.ULTracker.WebAPI.V1
{
    public class ULTrackerSettings
    {
        private readonly IConfiguration configuration;

        public ULTrackerSettings(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public byte[] AccountValuesReportTemplate => ReadFileFromAppSetting("V1:RelativePathToAccountValuesTemplate");

        public byte[] ExportPoliciesTemplateFileContent => ReadFileFromAppSetting("V1:RelativePathToDataTrackerTemplate");

        public byte[] CustomerTemplate => ReadFileFromAppSetting("V1:RelativePathToCustomerTemplate");

        public byte[] TransactionReportTemplate => ReadFileFromAppSetting("V1:RelativePathToTransactionReportTemplate");

        public byte[] ULPolicyDetailsTemplate => ReadFileFromAppSetting("V1:RelativePathToULPolicyDetailsTemplate");

        public byte[] VulPolicyDetailsTemplate => ReadFileFromAppSetting("V1:RelativePathToVULPolicyDetailsTemplate");

        public byte[] UlpbPolicyDetailsTemplate => ReadFileFromAppSetting("V1:RelativePathToULPBPolicyDetailsTemplate");

        public byte[] AccountingReportTemplate => ReadFileFromAppSetting("V1:RelativePathToAccountingReportTemplate");

        public byte[] ValuationReportTemplate => ReadFileFromAppSetting("V1:RelativePathToValuationReportTemplate");

        public byte[] MeReportTemplate => ReadFileFromAppSetting("V1:RelativePathToMeReportTemplate");

        public byte[] NewBusinessReportTemplate => ReadFileFromAppSetting("V1:RelativePathToNewBusinessReportTemplate");

        public byte[] PendingNewBusinessReportTemplate => ReadFileFromAppSetting("V1:RelativePathToPendingNewBusinessReportTemplate");

        public byte[] LiaReportTemplate => ReadFileFromAppSetting("V1:LIAReportTemplate");

        public static ULTrackerSettings Initialize(IConfiguration configuration)
        {
            return new ULTrackerSettings(configuration);
        }

        private byte[] ReadFileFromAppSetting(string relativePathSettingKey)
        {
            var fullPathToFile = Path.Combine(
                AppContext.BaseDirectory,
                configuration[relativePathSettingKey]);

            return File.ReadAllBytes(fullPathToFile);
        }
    }
}