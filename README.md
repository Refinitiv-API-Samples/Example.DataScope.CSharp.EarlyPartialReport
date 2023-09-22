# [How to Retrieve Early Partial Delivery of Embargoed Reports via DataScope Select REST API](https://developers.refinitiv.com/en/article-catalog/article/how-to-retrieve-early-partial-delivery-of-embargoed-reports-via-0)

The C# .NET Framework example demonstrates how to use the DSS .NET SDK to perform the immediate schedule Intraday Pricing extraction with the PartialEmbargoedReportsEnabled options enabled. The example can be downloaded from GitHub. You can run the example with the following parameters.

```
Usage:
        -u, --username[optional]... DSS Username

        -p, --password[optional]... DSS Password
```

The list of instruments and content fields can be modified in the code.

```
        List<InstrumentIdentifier> instrumentIdentifiers = new List<InstrumentIdentifier> 
        {
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "0001.HK"},
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "IBM.N"},
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "JPY="},
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "1579.T"},
            new InstrumentIdentifier{IdentifierType = IdentifierType.Ric, Identifier = "PTT.BK"},
        };


...

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

```
The output is:
```
####################################################################

            Disclaimer:
            The example applications presented here has been written by Refinitiv for the only purpose of illustrating articles published on the Refinitiv Developer Community. These example applications has not been tested for a usage in production environments. Refinitiv cannot be held responsible for any issues that may happen if these example applications or the related source code is used in production or any other client environment.

####################################################################

DSS Username is 9008895

1. Request a token
==================
Token: _9G4ZgssAuJFKNnxxx

2. Get embargo description
==========================
RIC             Current Embargo Delay
0001.HK         10 minute(s)
IBM.N           0 minute(s)
JPY=            0 minute(s)
1579.T          20 minute(s)
PTT.BK          15 minute(s)

3. Check the partial embargoed reports settings
=================================================
Current Settings:
PartialEmbargoedReportsEnabled: False
IntermediateReportsEnabled: False
DeltaReportsEnabled: False
Date Format: MM/dd/yyyy
Time Format: tt hh:mm:ss


4. Schedule an immediated extraction
=====================================
4.1 Create an instrument list
EmbargoedTestInstrumentList

4.2 Append instruments to an instrument list
Appended: 5 instruments.

4.3 Create an intraday pricing report template
EmbargoedTestIntradayPricingTemplate

4.4 Schedule an immediate extraction
EmbargoedTestImmediateSchedule

5. Get a report extraction
============================
Report Extraction ID is 2000000608361977, Status: Pending/Queued

6. Get Notes and Data
######################
6.1 Get Notes
NotesFile ID: VjF8fDEyNDM1MzU0ODI


Extraction Services Version 17.3.1.46134 (701464ae16af), Built Aug  1 2023 16:17:07
User has overridden estimates broker entitlements.
Processing started at 09/07/2023 AM 11:27:14.
User ID: 9008895
Extraction ID: 2000000608361977
Correlation ID: CiD/9007633/udU_bQ.089831168d190800/EQM/ED.0x089ded7e607913d9.0
Schedule: EmbargoedTestImmediateSchedule (ID = 0x089ded7e606913d9)
Input List (5 items): EmbargoedTestInstrumentList (ID = 0x089de707dde913d5) Created: 09/07/2023 AM 11:27:00 Last Modified: 09/07/2023 AM 11:27:04
Report Template (8 fields): EmbargoedTestIntradayPricingTemplate (ID = 0x089deff1346913db) Created: 09/07/2023 AM 11:27:08 Last Modified: 09/07/2023 AM 11:27:08
Schedule dispatched via message queue (0x089ded7e607913d9)
Schedule Time: 09/07/2023 AM 11:27:10
Temporary Integration Test Checkpoint 22
Successful operation - data received from RDP
Real-time data was snapped at the following times:
   09/07/2023 AM 11:27:10
   09/07/2023 AM 11:27:14 for data scheduled to snap at 09/07/2023 AM 11:27:10.
Processing completed successfully at 09/07/2023 AM 11:27:15, taking 0.792 Secs.
Extraction finished at 09/07/2023 AM 04:27:15 UTC, with servers: xc08bdm2Q23, QSDHA1 (0.0 secs), QSHC17 (0.4 secs)
Columns for (RIC,IBM.N,NYS) suppressed due to trade date other than today
Embargo delay of 20 minutes required by [ TYO (TOKYO STOCK EXCHANGE), TYM (TOKYO SE FLEX FULL OPEN MKT MODEL) ] for quotes from TYO
Embargo delay of 15 minutes required by [ SET (STOCK EXCHANGE OF THAILAND), ST2 (STOCK EXCHANGE OF THAILAND L1 L2) ] for quotes from SET
The last report will be embargoed until 09/07/2023 AM 11:47:14 (20 minutes) due to quote: RIC,1579.T,TYO - Last Update Time: 09/07/2023 AM 11:27:14.
Usage Summary for User 9008895, Client 65507, Template Type Intraday Pricing
Base Usage
        Instrument                          Instrument                   Terms          Price
  Count Type                                Subtype                      Source         Source
------- ----------------------------------- ---------------------------- -------------- ----------------------------------------
      4 Equities                                                         N/A            N/A
      1 Money Market                                                     N/A            N/A
-------
      5 Total instruments charged.
      0 Instruments with no reported data.
=======
      5 Instruments in the input list.
No Evaluated Pricing Service complex usage to report -- 5 Instruments in the input list had no reported data.
The file 9008895.EmbargoedTestImmediateSchedule.20230907.112714.2000000608361977.xc08bdm2Q23.0min.csv will be available immediately.
The file 9008895.EmbargoedTestImmediateSchedule.20230907.112714.2000000608361977.xc08bdm2Q23.15min.csv will be embargoed until 09/07/2023 AM 11:42:04.
The file 9008895.EmbargoedTestImmediateSchedule.20230907.112714.2000000608361977.xc08bdm2Q23.20min.csv will be embargoed until 09/07/2023 AM 11:47:14.
Writing RIC maintenance report.


6.2 Get Data
9/7/2023 11:27:24 AM: 9008895.EmbargoedTestImmediateSchedule.20230907.112714.2000000608361977.xc08bdm2Q23.0min.csv

RIC,Last Price,Last Update Time,Last Volume,Bid Price,Ask Price,Bid Size,Ask Size
0001.HK,41.5,09/07/2023 AM 11:05:53,500,41.5,41.55,"10,500","49,000"
IBM.N,,09/07/2023 AM 11:02:25,,,,0,0
JPY=,147.57,09/07/2023 AM 11:27:14,,147.57,147.58,,

Wait for Embargo Delay: 889 seconds
9/7/2023 11:42:21 AM: 9008895.EmbargoedTestImmediateSchedule.20230907.112714.2000000608361977.xc08bdm2Q23.15min.csv

RIC,Last Price,Last Update Time,Last Volume,Bid Price,Ask Price,Bid Size,Ask Size
PTT.BK,35.5,09/07/2023 AM 11:27:04,"1,700",35.25,35.5,"4,893,900","12,203,500"

Wait for Embargo Delay: 1199 seconds
9/7/2023 11:47:38 AM: 9008895.EmbargoedTestImmediateSchedule.20230907.112714.2000000608361977.xc08bdm2Q23.20min.csv

RIC,Last Price,Last Update Time,Last Volume,Bid Price,Ask Price,Bid Size,Ask Size
1579.T,"22,025",09/07/2023 AM 11:27:14,50,"22,025","22,030",70,650


7. Cleanup
############
Delete the schedule: EmbargoedTestImmediateSchedule
Delete the intraday pricing report template: EmbargoedTestIntradayPricingTemplate
Delete the instrument list: EmbargoedTestInstrumentList
Reset the user preferences

Press any key to continue . . .

```
