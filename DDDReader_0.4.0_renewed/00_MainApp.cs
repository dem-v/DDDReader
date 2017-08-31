using System;
using System.Globalization;
using static DDDReader_0._4._0_renewed.DeclarationOfClassesUsed;
using static DDDReader_0._4._0_renewed.Xml_DDDFileHandling;

namespace DDDReader_0._4._0_renewed
{
    internal class MainApp
    {       //This class is a starting one. It takes all the organization of the workflow and user-app interaction.
        private static void Main(string[] args)
        {
            Console.Write("Enter filename: ");
            var fname = Console.ReadLine();
            var DS = new InputRAWDataStorage();
            var PL = new PrintingLayout();
            PrintingLayout.DriverCard hold = new PrintingLayout.DriverCard();

            if (fname != null)
                /*try
                  {
                      FileDecode(DS, FileOpenRead(fname, DS), PL, fname);
                  }
                  catch (Exception e)
                  {
                      Console.WriteLine(e.Message, "\n\r", e.Source, "\n\r", e.StackTrace, "\n\r", e.TargetSite);
                      Console.ReadKey();
                  }
              */
                FileDecode(DS, FileOpenRead(fname, DS), PL, hold, fname);
            else
            {
                Console.WriteLine("ERROR: No input!");
                Console.ReadKey();
            }

            Console.WriteLine();
            Console.WriteLine();
            //            Console.WriteLine("If you require report of 561/2006 Regulations violations, please input any char (else press enter): ");
            //            var report = Console.ReadLine();
            var report = 'g';
            if (report != null)
            {
                var dateBegin =
                    hold.CDA.cardActivityDailyRecord[0].activityRecordDate.ConvertToDateTime().ToString("dd-MM-yyyy");
                var dateEnd =
                    hold.CDA.cardActivityDailyRecord[hold.CDA.cardActivityDailyRecord.Length - 1].activityRecordDate
                        .ConvertToDateTime().ToString("dd-MM-yyyy");
                if (dateEnd == "01-01-1970") dateEnd =
                    hold.CDA.cardActivityDailyRecord[hold.CDA.cardActivityDailyRecord.Length - 2].activityRecordDate
                        .ConvertToDateTime().ToString("dd-MM-yyyy");
                Console.WriteLine("Available Dates are {0} - {1}", dateBegin, dateEnd);
                Console.WriteLine("Please, specify the starting date of report 'DD-MM-YYYY' (default = " + dateBegin + "): ");
                var str1 = Console.ReadLine();
                bool wholeRep = false;
                if (str1 == "")
                {
                    str1 = dateBegin;
                    wholeRep = true;
                }

                DateTimeFormatInfo dtfi = new DateTimeFormatInfo { ShortDatePattern = "dd-MM-yyyy" };
                var repStartingDate = DateTime.Parse(str1, dtfi);

                Console.WriteLine("Please, specify the final date of the report 'DD-MM-YYYY'(default: " + (wholeRep ? dateEnd : str1) + "): ");
                var str2 = Console.ReadLine();
                if (str2 == "") str2 = wholeRep ? dateEnd : str1;
                dtfi = new DateTimeFormatInfo();
                dtfi.ShortDatePattern = "dd-MM-yyyy";
                var repEndingDate = DateTime.Parse(str2, dtfi);

                //ReportActivityVerbose(PL, hold, fname, repStartingDate, repEndingDate);
                Report561_2006(PL, hold, fname, repStartingDate, repEndingDate);

                Console.WriteLine("Report successfully generated. Press any key to exit...");
                Console.ReadKey();
            }
            //Output call here!
        }
    }
}