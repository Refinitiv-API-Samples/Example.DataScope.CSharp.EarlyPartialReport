using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommandLineParser;
using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;
using DataScope.Select.Api.Content;
using DataScope.Select.Api.Extractions;
using DataScope.Select.Api.Extractions.ExtractionRequests;
using DataScope.Select.Api.Extractions.SubjectLists;
using DataScope.Select.Api.Extractions.ReportTemplates;
using DataScope.Select.Api.Users;
using DataScope.Select.Api.Extractions.Schedules;
using DataScope.Select.Api.Extractions.ReportExtractions;
using DataScope.Select.Api.Core;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace EarlyPartialReportCSharp
{
    internal class Program
    {
        private CommandLineParser.CommandLineParser cmdParser = new CommandLineParser.CommandLineParser();
        ValueArgument<string> dssUserName = new ValueArgument<string>('u', "username", "DSS Username");
        ValueArgument<string> dssPassword = new ValueArgument<string>('p', "password", "DSS Password");
        Uri dssUri = new Uri("https://selectapi.datascope.refinitiv.com/RestApi/v1/");
        UserPreference userPreference;
        bool SavedPartialEmbargoedReportsEnabled = false;
        bool SavedIntermediateReportsEnabled = false;
        bool SavedDeltaReportsEnabled = false;
          
        ExtractionsContext extractionsContext = null;
        List<InstrumentIdentifier> instrumentIdentifiers = new List<InstrumentIdentifier> 
        {
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "0001.HK"},
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "IBM.N"},
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "JPY="},
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "1579.T"},
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "PTT.BK"},
        };


        List<string> embargoDescFields = new List<string>
        {
            "RIC",
            "Current Embargo Delay",
            "Embargo Times",
            "Embargo Window",
            "Exchange Requiring Embargo",
            "Instrument Snap Time",
            "Last Update Time",
            "Maximum Embargo Delay",
            "Real Time Permitted"
        };

        List<string> intradayFields = new List<string>
        {      
            "RIC",
            "Last Price",
            "Last Update Time",
            "Last Volume",
            "Bid Price",
            "Ask Price",
            "Bid Size",
            "Ask Size"
        };

        List<int> embargoWaitTime = new List<int>();
        List<string> dataFileList = new List<string>();

        string instrumentListName = "EmbargoedTestInstrumentList";
        string reportTemplateName = "EmbargoedTestIntradayPricingTemplate";
        string scheduleName = "EmbargoedTestImmediateSchedule";

        InstrumentList instrumentList = null;
        IntradayPricingReportTemplate reportTemplate = null;
        Schedule schedule = null;
        ReportExtraction reportExtraction = null;
        static void Main(string[] args)
        {
            Program prog = new Program();
            if (prog.init(ref args))
            {
                try
                {
                    prog.run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }

        public Program()
        {
            cmdParser.IgnoreCase = true;
            dssPassword.DefaultValue = "";
            dssUserName.DefaultValue = "";
            cmdParser.Arguments.Add(dssUserName);
            cmdParser.Arguments.Add(dssPassword);

        }
        private void createExtractionsContext()
        {
            Console.WriteLine("DSS Username is " + dssUserName.Value);
            Console.WriteLine("\n1. Request a token");
            Console.WriteLine("==================");
            extractionsContext = new ExtractionsContext(dssUri, dssUserName.Value, dssPassword.Value);

            Console.WriteLine("Token: {0}", extractionsContext.SessionToken);
            
        }
        private void updateUserPreferences()
        {
            Console.WriteLine("\n3. Check the partial embargoed reports settings");
            Console.WriteLine("=================================================");
            UsersContext usersContext = new UsersContext(dssUri, extractionsContext.SessionToken);
           
            userPreference = usersContext.UserPreferenceOperations.Get(int.Parse(dssUserName.Value));

            SavedPartialEmbargoedReportsEnabled = userPreference.ContentSettings.PartialEmbargoedReportsEnabled;
            SavedIntermediateReportsEnabled = userPreference.ContentSettings.IntermediateReportsEnabled;
            SavedDeltaReportsEnabled = userPreference.ContentSettings.DeltaReportsEnabled;

            Console.WriteLine("Current Settings:");
            Console.WriteLine("PartialEmbargoedReportsEnabled: "+userPreference.ContentSettings.PartialEmbargoedReportsEnabled);
            Console.WriteLine("IntermediateReportsEnabled: " + userPreference.ContentSettings.IntermediateReportsEnabled);
            Console.WriteLine("DeltaReportsEnabled: " + userPreference.ContentSettings.DeltaReportsEnabled);
            Console.WriteLine("Date Format: " + userPreference.UiSettings.ShortDateFormatString);
            Console.WriteLine("Time Format: " + userPreference.UiSettings.LongTimeFormatString);

            userPreference.ContentSettings.PartialEmbargoedReportsEnabled = true;
            userPreference.ContentSettings.IntermediateReportsEnabled = true;
            userPreference.ContentSettings.DeltaReportsEnabled = true;

            usersContext.UserPreferenceOperations.Update(userPreference);

            
        }




        private void printDisclaimer()
        {
            Console.WriteLine("\n####################################################################");
            string disclaimer = @"
            Disclaimer:
            The example applications presented here has been written by Refinitiv for the only purpose of illustrating articles published on the Refinitiv Developer Community. These example applications has not been tested for a usage in production environments. Refinitiv cannot be held responsible for any issues that may happen if these example applications or the related source code is used in production or any other client environment.
            ";
            Console.WriteLine(disclaimer);
            Console.WriteLine("####################################################################\n");
        } 
        private void getEmbargoDescription()
        {
            Console.WriteLine("\n2. Get embargo description");
            Console.WriteLine("==========================");
            IntradayPricingExtractionRequest request = new IntradayPricingExtractionRequest { 
                IdentifierList = InstrumentIdentifierList.Create(instrumentIdentifiers.ToArray()),
                ContentFieldNames = embargoDescFields.ToArray(),
                Condition = null
            };

            var result = extractionsContext.ExtractWithNotes(request);
            Console.WriteLine("RIC\t\tCurrent Embargo Delay");
            foreach(var row in result.Contents)
            {
                Console.Write(row.Identifier);
                Console.Write("\t\t");
                if (row.DynamicProperties.ContainsKey("Current Embargo Delay"))
                {
                    Console.Write(row.DynamicProperties["Current Embargo Delay"].ToString() + " minute(s)\n");
                }
                else
                {
                    Console.WriteLine("");
                }
               
            }
            //Console.Write(result.Notes[0].ToString());

        }
        private void getCredential()
        {
            if (dssUserName.Value == "")
            {
                Console.Write("Enter DSS Username: ");
                dssUserName.Value = Console.ReadLine();
            }

            if (dssPassword.Value == "")
            {
                Console.Write("Enter DSS Password: ");
                ConsoleKeyInfo key;

                do
                {
                    key = Console.ReadKey(true);
                    // Backspace Should Not Work
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        dssPassword.Value += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && dssPassword.Value.Length > 0)
                        {
                            dssPassword.Value = dssPassword.Value.Substring(0, (dssPassword.Value.Length - 1));
                            Console.Write("\b \b");
                        }
                    }


                }
                // Stops Receving Keys Once Enter is Pressed
                while (key.Key != ConsoleKey.Enter);

                Console.WriteLine("");
            }

        }

        private void createAnInstrumentList()
        {
            Console.WriteLine(instrumentListName);
            instrumentList = new InstrumentList { Name = instrumentListName };
            extractionsContext.InstrumentListOperations.Create(instrumentList);          
        }

        private void appendInstruments()
        {
            InstrumentsAppendIdentifiersResult appendResult =
               extractionsContext.InstrumentListOperations.AppendIdentifiers(
                   instrumentList, instrumentIdentifiers, false);
            Console.WriteLine($"Appended: {appendResult.AppendResult.AppendedInstrumentCount} instruments.");
            
           
        }

        private void createAnIntradayPricingReportTemplate()
        {
            Console.WriteLine(reportTemplateName);
            reportTemplate = new IntradayPricingReportTemplate
            {
                Name = reportTemplateName,
                ShowColumnHeaders = true
            };
            reportTemplate.ContentFields.AddRange(intradayFields.ToArray());
           

            extractionsContext.ReportTemplateOperations.Create(reportTemplate);
            
        }

        private void scheduleAnImmediatedExtraction()
        {
            Console.WriteLine(scheduleName);
            schedule = new Schedule
            {
                Name = scheduleName,
                TimeZone = TimeZone.CurrentTimeZone.StandardName,
                Recurrence = ScheduleRecurrence.CreateSingleRecurrence(DateTimeOffset.UtcNow, true),
                Trigger = ScheduleTrigger.CreateImmediateTrigger(true),
                ListId = instrumentList.ListId,
                ReportTemplateId = reportTemplate.ReportTemplateId
            };

            extractionsContext.ScheduleOperations.Create(schedule);

            
        }
        private void scheduleImmediatedExtraction()
        {
            Console.WriteLine("\n4. Schedule an immediate extraction");
            Console.WriteLine("=====================================");
            Console.WriteLine("4.1 Create an instrument list");
            createAnInstrumentList();
            Console.WriteLine("\n4.2 Append instruments to an instrument list");
            appendInstruments();
            Console.WriteLine("\n4.3 Create an intraday pricing report template");
            createAnIntradayPricingReportTemplate();
            Console.WriteLine("\n4.4 Schedule an immediate extraction");
            scheduleAnImmediatedExtraction();
        }

        private string getData(ExtractedFile extractedFile) 
        {
            printDisclaimer();
            DssStreamResponse streamResponse = extractionsContext.GetReadStream(extractedFile);
            using (FileStream fileStream = File.Create(extractedFile.ExtractedFileName))
            {
                streamResponse.Stream.CopyTo(fileStream);
            }
            return File.ReadAllText(extractedFile.ExtractedFileName);
        }
        private string getNotes()
        {
          
            while (reportExtraction.NotesFile == null)
            {
                extractionsContext.LoadProperty(reportExtraction, "NotesFile");
                if(reportExtraction.NotesFile == null)
                {
                    System.Threading.Thread.Sleep(5000);
                }
            }
           
            Console.WriteLine("NotesFile ID: " + reportExtraction.NotesFile.ExtractedFileId);
            return getData(reportExtraction.NotesFile);

        }
        private void checkWaitTimes(string notes)
        {
            DateTime processingDateTime;
            string pattern = @"^Processing completed successfully at (.*),";
            string partialFilePattern = @"The file .* will be embargoed until (.*)\.";
            // Create a Regex
            Regex rg = new Regex(pattern, RegexOptions.Multiline);
            Regex partialFileRegularExprssion = new Regex(partialFilePattern, RegexOptions.Multiline);

            var processingTimeMatch = rg.Match(notes);
            if (processingTimeMatch.Success)
            {
                processingDateTime = DateTime.ParseExact(processingTimeMatch.Groups[1].Value, userPreference.UiSettings.ShortDateFormatString + " " + userPreference.UiSettings.LongTimeFormatString, null);
                //Console.WriteLine(processingDateTime.ToString());

                var partialFilesMatches = partialFileRegularExprssion.Matches(notes);
                foreach (Match partialFilesMatch in partialFilesMatches)
                {
                    //Console.WriteLine(partialFilesMatch.Groups[1].Value);
                    var partialFileDateTime = DateTime.ParseExact(partialFilesMatch.Groups[1].Value, userPreference.UiSettings.ShortDateFormatString + " " + userPreference.UiSettings.LongTimeFormatString, null);

                    TimeSpan ts = partialFileDateTime - processingDateTime;                   
                    embargoWaitTime.Add(Convert.ToInt16(ts.TotalSeconds));
                }

                embargoWaitTime.Sort();
                embargoWaitTime = embargoWaitTime.Distinct().ToList();

            }
            else
            {
                throw new Exception("Unable to find Processing completed time");
            }

           
           

        }
        private void checkLatestDataFiles()
        {
            extractionsContext.LoadProperty(reportExtraction, "Files");
            while(reportExtraction.Files == null)
            {
                System.Threading.Thread.Sleep(5000);
                extractionsContext.LoadProperty(reportExtraction, "Files");
            }

            foreach(ExtractedFile file in reportExtraction.Files)
            {
                if(file.FileType == ExtractedFileType.Partial &&
                    !dataFileList.Contains(file.ExtractedFileId))
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": " + file.ExtractedFileName+"\n");
                    Console.WriteLine(getData(file));
                    dataFileList.Add(file.ExtractedFileId);


                }
            }
        }
        private void cleanup()
        {
            Console.WriteLine("\n7. Cleanup");
            Console.WriteLine("############");
            Console.WriteLine($"Delete the schedule: {schedule.Name}");
            extractionsContext.ScheduleOperations.Delete(schedule);

            Console.WriteLine($"Delete the intraday pricing report template: {reportTemplate.Name}");
            extractionsContext.ReportTemplateOperations.Delete(reportTemplate);

            Console.WriteLine($"Delete the instrument list: {instrumentList.Name}");
            extractionsContext.InstrumentListOperations.Delete(instrumentList);

            userPreference.ContentSettings.PartialEmbargoedReportsEnabled = SavedPartialEmbargoedReportsEnabled;
            userPreference.ContentSettings.DeltaReportsEnabled = SavedDeltaReportsEnabled;
            userPreference.ContentSettings.IntermediateReportsEnabled = SavedPartialEmbargoedReportsEnabled;

            Console.WriteLine("Reset the user preferences");

            UsersContext usersContext = new UsersContext(dssUri, extractionsContext.SessionToken);

            usersContext.UserPreferenceOperations.Update(userPreference);
        }
        private void getNotesAndDataFiles()
        {
            int waitTime = 0;
            Console.WriteLine("\n6. Get Notes and Data");
            Console.WriteLine("######################");
            Console.WriteLine("6.1 Get Notes");
            string notes = getNotes();
            Console.WriteLine(notes);
            checkWaitTimes(notes);
            Console.WriteLine("\n6.2 Get Data");
            checkLatestDataFiles();
            if (reportExtraction.Status != ReportExtractionStatus.Completed)
            {
                foreach(var embargoTime in embargoWaitTime)
                {
                    Console.WriteLine($"Wait for Embargo Delay: {embargoTime} seconds");
                    System.Threading.Thread.Sleep(((embargoTime - waitTime)+5)*1000);
                    waitTime = embargoTime;
                    checkLatestDataFiles();
                }
            }

        }
        private void getReportExtraction()
        {
            Console.WriteLine("\n5. Get a report extraction");
            Console.WriteLine("============================");

            extractionsContext.LoadProperty(schedule, "PendingExtractions");
            
            if (schedule.PendingExtractions.Count == 0)
            {
                Console.WriteLine("No Pending Extraction");
                extractionsContext.LoadProperty(schedule, "CompletedExtractions");
                if(schedule.CompletedExtractions.Count == 0)
                {
                    Console.WriteLine("No Completed Extraction");
                    throw new Exception("No Report Extraction");
                }
                else
                {
                     reportExtraction = schedule.PendingExtractions[0];
                }
            }
            else
            {
                reportExtraction = schedule.PendingExtractions[0];
            }

            Console.WriteLine($"Report Extraction ID is {reportExtraction.ReportExtractionId}, Status: {reportExtraction.Status}/{reportExtraction.DetailedStatus}");

           

        }
        public void run()
        {
            printDisclaimer();
            getCredential();
            createExtractionsContext();
            getEmbargoDescription();
            updateUserPreferences();
            printDisclaimer();
            scheduleImmediatedExtraction();
            getReportExtraction();
            getNotesAndDataFiles();
            cleanup();
            printDisclaimer();
        }

        public bool init(ref string[] args)
        {
           
            try
            {
                cmdParser.ParseCommandLine(args);

                if (!cmdParser.ParsingSucceeded)
                {
                    cmdParser.ShowUsage();
                    return false;
                }


              
            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.Message);
                cmdParser.ShowUsage();
                return false;
            }

           
            return true;
        }
    }
}
