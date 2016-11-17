//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

//
// The following is based upon code from PowerShell Editor Services
// License: https://github.com/PowerShell/PowerShellEditorServices/blob/develop/LICENSE
//

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SqlTools.ServiceLayer.Connection.Contracts;
using Microsoft.SqlTools.ServiceLayer.Hosting.Protocol;
using Microsoft.SqlTools.ServiceLayer.Hosting.Protocol.Channel;
using Microsoft.SqlTools.ServiceLayer.LanguageServices.Contracts;
using Microsoft.SqlTools.ServiceLayer.QueryExecution.Contracts;

namespace Microsoft.SqlTools.ServiceLayer.TestDriver.Driver
{
    /// <summary>
    /// Test driver for the service host
    /// </summary>
    public class ServiceTestDriver : TestDriverBase
    {

        public const string ServiceCodeCoverageEnvironmentVariable = "SERVICECODECOVERAGE";

        public const string CodeCoverageToolEnvironmentVariable = "CODECOVERAGETOOL";

        public const string CodeCoverageOutputEnvironmentVariable = "CODECOVERAGEOUTPUT";

        public const string ServiceHostEnvironmentVariable = "SQLTOOLSSERVICE_EXE";

        public bool IsCoverageRun { get; set; } 

        public ServiceTestDriver()
        {
            string serviceHostExecutable = Environment.GetEnvironmentVariable(ServiceHostEnvironmentVariable);
            string serviceHostArguments = "--enable-logging";
            if (string.IsNullOrWhiteSpace(serviceHostExecutable))
            {
                // Include a fallback value to for running tests within visual studio
                serviceHostExecutable =
                    @"..\..\src\Microsoft.SqlTools.ServiceLayer\bin\Debug\netcoreapp1.0\win7-x64\Microsoft.SqlTools.ServiceLayer.exe";
            }

            // Make sure it exists before continuing
            if (!File.Exists(serviceHostExecutable))
            {
                throw new FileNotFoundException($"Failed to find Microsoft.SqlTools.ServiceLayer.exe at provided location '{serviceHostExecutable}'. " +
                                                "Please set SQLTOOLSERVICE_EXE environment variable to location of exe");
            }

            //setup the service host for code coverage if the envvar is enabled
            if (Environment.GetEnvironmentVariable(ServiceCodeCoverageEnvironmentVariable) == "True")
            {
                string coverageToolPath = Environment.GetEnvironmentVariable(CodeCoverageToolEnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(coverageToolPath))
                {
                    string serviceHostDirectory = Path.GetDirectoryName(serviceHostExecutable);
                    if (string.IsNullOrWhiteSpace(serviceHostDirectory))
                    {
                        serviceHostDirectory = ".";
                    }

                    string coverageOutput = Environment.GetEnvironmentVariable(CodeCoverageOutputEnvironmentVariable);
                    if (string.IsNullOrWhiteSpace(coverageOutput))
                    {
                        coverageOutput = "coverage.xml";
                    }

                    serviceHostArguments = "-mergeoutput -target:" + serviceHostExecutable + " -targetargs:" + serviceHostArguments 
                        + " -register:user -oldstyle -filter:\"+[Microsoft.SqlTools.*]* -[xunit*]*\" -output:" + coverageOutput + " -searchdirs:" + serviceHostDirectory;
                    serviceHostExecutable = coverageToolPath;

                    this.IsCoverageRun = true;
                }               
            }

            this.clientChannel = new StdioClientChannel(serviceHostExecutable, serviceHostArguments);
            this.protocolClient = new ProtocolEndpoint(clientChannel, MessageProtocolType.LanguageServer);
        }

        /// <summary>
        /// Start the test driver, and launch the sqltoolsservice executable
        /// </summary>
        public async Task Start()
        {
            await this.protocolClient.Start();
            await Task.Delay(1000); // Wait for the service host to start

            // Setup events to queue for testing
            this.QueueEventsForType(ConnectionCompleteNotification.Type);
            this.QueueEventsForType(IntelliSenseReadyNotification.Type);
            this.QueueEventsForType(QueryExecuteCompleteEvent.Type);
            this.QueueEventsForType(PublishDiagnosticsNotification.Type);
        }

        /// <summary>
        /// Stop the test driver, and shutdown the sqltoolsservice executable
        /// </summary>
        public async Task Stop()
        {
            await this.protocolClient.Stop();
        }
    }
}