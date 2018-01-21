using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using static DDDReader_0._4._0_renewed._561_2006Control;
using static DDDReader_0._4._0_renewed.DeclarationOfClassesUsed;
using static DDDReader_0._4._0_renewed.DetectionAndInitialDataBuild;

namespace DDDReader_0._4._0_renewed
{   //This class perfroms most activities on Xml files.
    internal class Xml_DDDFileHandling
    {
        /*       public static void ReportActivityVerbose(PrintingLayout pl, PrintingLayout.DriverCard hold, string fname, DateTime repStartDate, DateTime repEndDate)
               {
                   var settings = new XmlWriterSettings
                   {
                       Indent = true,
                       IndentChars = ("\t"),
                       OmitXmlDeclaration = true
                   };

                   XmlWriter xmlWriter = XmlWriter.Create((Path.GetFileNameWithoutExtension(fname) + "_Activity_DataSheet.xml"), settings);
                   xmlWriter.WriteStartDocument();
                   xmlWriter.WriteStartElement(Path.GetFileNameWithoutExtension(fname));

                   xmlWriter.WriteElementString("ReportStartingDate", repStartDate.ToString("dd-MM-yyyy"));
                   xmlWriter.WriteElementString("ReportEndingDate", repEndDate.ToString("dd-MM-yyyy"));

                   xmlWriter.WriteStartElement("Report");
                   int shift = (hold.CDA.cardActivityDailyRecord[0].activityRecordDate.ConvertToDateTime() - repStartDate).Days;
                   if (shift < 0) shift = 0;
                   int ReportTimeSpan = (repEndDate - repStartDate).Days;

                   if (ReportTimeSpan > hold.CDA.cardActivityDailyRecord.Length)
                       ReportTimeSpan = hold.CDA.cardActivityDailyRecord.Length;

                   CardActivityDailyRecord[] cadrHERE = new CardActivityDailyRecord[ReportTimeSpan];

                   Array.Copy(hold.CDA.cardActivityDailyRecord, shift, cadrHERE, 0, ReportTimeSpan);
                   int i = 0;
                   foreach (var f in cadrHERE)
                   {
                       i++;
                       var thisdate = f.activityRecordDate.ConvertToDateTime().ToString("dd-MM-yyyy");

                       if (f.activityChangeInfo != null)
                       {
                           for (int j = 0; j < f.activityChangeInfo.Length; j++)
                           {
                               xmlWriter.WriteStartElement("Activity");
                               var aci = f.activityChangeInfo[j];
                               var endtime = (j + 2 > f.activityChangeInfo.Length)
                                   ? 1440
                                   : f.activityChangeInfo[j + 1].minutesSinceMidnight;

                               xmlWriter.WriteElementString("RecordDate", thisdate);
                               xmlWriter.WriteElementString("StartTime", getVerboseTimeFromMinutes(aci.minutesSinceMidnight));
                               xmlWriter.WriteElementString("EndTime", getVerboseTimeFromMinutes(endtime));
                               xmlWriter.WriteElementString("EventDuration", getVerboseTimeFromMinutes(endtime - aci.minutesSinceMidnight));
                               xmlWriter.WriteElementString("EventType", aci.getVerboseActivity());
                               xmlWriter.WriteElementString("DrivingStatus", aci.getVerboseDrivingStatus());
                               xmlWriter.WriteElementString("DriverCardStatus", aci.getVerboseDriverCardStatus());
                               xmlWriter.WriteElementString("CardSlotStatus", aci.getVerboseSlotStatus());

                               xmlWriter.WriteEndElement();
                           }
                           xmlWriter.Flush();
                       }
                   }
                   xmlWriter.WriteEndElement();
                   xmlWriter.WriteEndElement();
                   xmlWriter.WriteEndDocument();
                   xmlWriter.Flush();
               }
       */

            //Handler to print infomation about fines
        public static void printfine(XmlWriter x, TimeReal statsvarVDayTime, int evtime, int aci_te, string p1, string p2)
        {//prints a single fine record
            x.WriteStartElement(p1);

            x.WriteStartElement("DayTimeOfEvent");
            x.WriteElementString("DateOfEvent", statsvarVDayTime.ConvertToDateTime().ToString("dd-MM-yyyy"));
            x.WriteElementString("TimeOfEvent", getVerboseTimeFromMinutes(aci_te));
            x.WriteElementString("Duration", getVerboseTimeFromMinutes(evtime));
            x.WriteEndElement();

            x.WriteElementString("Amount", p2);

            x.WriteEndElement();

            x.Flush();
        }

        //Handler override, to print fines information using class ViolationRecord
        public static void printfine(XmlWriter x, ViolationRecord vr)
        {
            x.WriteStartElement(vr.VIOLATION_TYPE);

            x.WriteStartElement("DayTimeOfEvent");
            x.WriteElementString("DateOfEvent", vr.ViolationDateTime.ToString("dd - MM - yyyy HH:MM"));
            x.WriteElementString("Duration", vr.amount.ToString("D"));
            x.WriteEndElement();

            x.WriteElementString("Amount", vr.ViolationText);

            x.WriteEndElement();

            x.Flush();
        }

        //Converter for daily time format (00:00-23:59) to UTC format
        public static string getVerboseTimeFromMinutes(int minutesSinceMidnight)
        {
            var n = TimeSpan.FromMinutes(minutesSinceMidnight);
            return n.Hours.ToString("D2") + ":" + n.Minutes.ToString("D2");
        }

        //Actual decoding of DDD file is done here
        public static void FileDecode(InputRAWDataStorage ds, int blockCount, PrintingLayout pl, PrintingLayout.DriverCard hold, String fname)
        {
            //Setting up synchronous writer
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                OmitXmlDeclaration = true
            };

            XmlWriter xmlWriter = XmlWriter.Create((Path.GetFileNameWithoutExtension(fname) + ".xml"), settings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement(Path.GetFileName(fname));

            #region Draft
                //            Importing imp = new Importing();
                //            imp.CardIdentifier(ds.DataArchive[Importing.GetBlockId("EF ICC", ds)].Value, pl.cardICCI, xmlWriter);
                //            imp.CardChipIdentification(ds.DataArchive[Importing.GetBlockId("EF IC", ds)].Value, pl.cardCI, xmlWriter);
                //            int CardType = ds.DataArchive[Importing.GetBlockId("EF Application_Identification", ds)].Value[0];
            #endregion

            CardIdentifier(ds.DataArchive[GetBlockId("EF ICC", ds)].Value, pl.cardICCI, xmlWriter); //Call for generating Card Identification Information
            CardChipIdentification(ds.DataArchive[GetBlockId("EF IC", ds)].Value, pl.cardCI, xmlWriter); //Call for generating Card CHIP Identification Information
            int CardType = ds.DataArchive[GetBlockId("EF Application_Identification", ds)].Value[0]; //Get card type value
            switch (CardType)
            {
                case 1:
                    {
                        DriverCardInput(ds, hold, xmlWriter); //Call for generating actual Data Card information
                        //XmlOutputClass.PrintoutData(pl, hold);
                        break;
                    }
                    #region Draft 
                                    //Considered creating for multiple types
                    /*case 2:
                    {
                        PrintingLayout.WorkshopCard hold = new PrintingLayout.WorkshopCard();
                        Importing.WorkshopCardInput(ds, hold);

                        break;
                    }
                    case 3:
                    {
                        PrintingLayout.ControlCard hold = new PrintingLayout.ControlCard();
                        Importing.ControlCardInput(ds, hold);

                        break;
                    }
                    case 4:
                    {
                        PrintingLayout.CompanyCard hold = new PrintingLayout.CompanyCard();
                        Importing.CompanyCardInput(ds, hold);

                        break;
                    }*/
                    #endregion
            }

            /*for (int i = 0; i <= blockCount; i++)
            {
            }*/

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            xmlWriter.Close();
        }

        //Open DDD file and read all RAW data.
        public static int FileOpenRead(string fname, InputRAWDataStorage ds)
        {
             var fs = new FileStream(fname, FileMode.Open, FileAccess.Read);

            int blockCount = 0;
            var b = ds.DataArchive[blockCount];
            while (fs.Position < fs.Length) //Reading all block by block
            {
                b = ds.DataArchive[blockCount] = new InputRAWDataStorage.DataBlock();
                fs.Read(b.ID, 0, 3);
                fs.Read(b.Length, 0, 2);
                b.Value = new byte[(int)b.LengthInt()];
                fs.Read(b.Value, 0, (int)b.LengthInt());
                blockCount++;
            }
            fs.Close();
            return blockCount;
        }

        //Starting the actual report 561/2006
        public static void Report561_2006(PrintingLayout pl, PrintingLayout.DriverCard hold, string fname, DateTime repStartDate, DateTime repEndDate)
        {
            List<ViolationRecord> weeklyViolations = new List<ViolationRecord>();

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                OmitXmlDeclaration = true
            };

            XmlWriter xmlWriter = XmlWriter.Create((Path.GetFileNameWithoutExtension(fname) + "_561Report.xml"), settings);
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement(Path.GetFileNameWithoutExtension(fname));

            xmlWriter.WriteElementString("ReportStartingDate", repStartDate.ToString("dd-MM-yyyy"));
            xmlWriter.WriteElementString("ReportEndingDate", repEndDate.ToString("dd-MM-yyyy"));

            xmlWriter.WriteStartElement("Report");

            CardActivityDailyRecord[] cadrHERE = PrepareForCounting(hold, repStartDate, repEndDate);//Preparing the array to report and process
            int ReportTimeSpan = (repEndDate - repStartDate).Days;
            Statistics stat = new Statistics();
            List<ShiftStats> lvr = ShiftFinesCounting(cadrHERE);//Get all shift fines
            GetWeeklyViolations(weeklyViolations, ReportTimeSpan, cadrHERE, stat); //Get weekly violations statistics 
            //printing elements
            foreach (var lvrlvr in lvr)
            {
                foreach (var itemsViolationRecord in lvrlvr.sv)
                {
                    printfine(xmlWriter, itemsViolationRecord); //printing accounted fines
                }
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
        }
    }
}