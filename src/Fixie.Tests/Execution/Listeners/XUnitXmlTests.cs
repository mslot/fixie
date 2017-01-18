﻿namespace Fixie.Tests.Execution.Listeners
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Assertions;
    using Fixie.Execution;
    using Fixie.Execution.Listeners;

    public class XUnitXmlTests : MessagingTests
    {
        public void ShouldProduceValidXmlDocument()
        {
            XDocument actual = null;

            var listener = new ReportListener<XUnitXml>(assembly => actual = new XUnitXml().Transform(assembly));

            using (var console = new RedirectedConsole())
            {
                Run(listener);

                console.Lines()
                    .ShouldEqual(
                        "Console.Out: Fail",
                        "Console.Error: Fail",
                        "Console.Out: FailByAssertion",
                        "Console.Error: FailByAssertion",
                        "Console.Out: Pass",
                        "Console.Error: Pass");
            }

            XsdValidate(actual);
            CleanBrittleValues(actual.ToString(SaveOptions.DisableFormatting)).ShouldEqual(ExpectedReport);
        }

        static void XsdValidate(XDocument doc)
        {
#if NET45
            var schemaSet = new XmlSchemaSet();
            using (var xmlReader = XmlReader.Create(Path.Combine("Execution", Path.Combine("Listeners", "XUnitXmlReport.xsd"))))
            {
                schemaSet.Add(null, xmlReader);
            }

            doc.Validate(schemaSet, null);
#endif
        }

        static string CleanBrittleValues(string actualRawContent)
        {
            //Avoid brittle assertion introduced by system date.
            var cleaned = Regex.Replace(actualRawContent, @"run-date=""\d\d\d\d-\d\d-\d\d""", @"run-date=""YYYY-MM-DD""");

            //Avoid brittle assertion introduced by system time.
            cleaned = Regex.Replace(cleaned, @"run-time=""\d\d:\d\d:\d\d""", @"run-time=""HH:MM:SS""");

            //Avoid brittle assertion introduced by .NET version.
            cleaned = Regex.Replace(cleaned, @"environment=""[^""]*""", @"environment=""00-bit .NET 1.2.3.4""");

            //Avoid brittle assertion introduced by fixie version.
            cleaned = Regex.Replace(cleaned, @"test-framework=""Fixie \d+(\.\d+)*(\-[^""]+)?""", @"test-framework=""Fixie 1.2.3.4""");

            //Avoid brittle assertion introduced by test duration.
            cleaned = Regex.Replace(cleaned, @"time=""[\d\.]+""", @"time=""1.234""");

            //Avoid brittle assertion introduced by stack trace line numbers.
            cleaned = cleaned.CleanStackTraceLineNumbers();

            return cleaned;
        }

        string ExpectedReport
        {
            get
            {
                var assemblyLocation = GetType().Assembly().Location;
                var configLocation =
#if NET45
                    AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#else
                    "Unknown Configuration File";
#endif
                var fileLocation = TestClassPath();
                return XDocument.Parse(File.ReadAllText(Path.Combine("Execution", Path.Combine("Listeners", "XUnitXmlReport.xml"))))
                                .ToString(SaveOptions.DisableFormatting)
                                .Replace("[assemblyLocation]", assemblyLocation)
                                .Replace("[configLocation]", configLocation)
                                .Replace("[fileLocation]", fileLocation)
                                .Replace("[testClass]", TestClass)
                                .Replace("[testClassForStackTrace]", TestClass.Replace("+", "."));
            }
        }
    }
}