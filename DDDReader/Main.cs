using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace DDDReader
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("Enter filename: ");
            var fname = Console.ReadLine();
            var DS = new DataStorage();
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

                ReportActivityVerbose(PL, hold, fname, repStartingDate, repEndingDate);
                //                Report561_2006(PL, hold, fname, repStartingDate, repEndingDate);
            }
            //Output call here!
        }

        private static void ReportActivityVerbose(PrintingLayout pl, PrintingLayout.DriverCard hold, string fname, DateTime repStartDate, DateTime repEndDate)
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

        /*private static void Report561_2006(PrintingLayout pl, PrintingLayout.DriverCard hold, string fname, DateTime repStartDate, DateTime repEndDate)
        {
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
            int shift = (hold.CDA.cardActivityDailyRecord[0].activityRecordDate.ConvertToDateTime() - repStartDate).Days;
            if (shift < 0) shift = 0;
            int ReportTimeSpan = (repEndDate - repStartDate).Days;

            if (ReportTimeSpan > hold.CDA.cardActivityDailyRecord.Length)
                ReportTimeSpan = hold.CDA.cardActivityDailyRecord.Length;

            CardActivityDailyRecord[] cadrHERE = new CardActivityDailyRecord[ReportTimeSpan];

            Array.Copy(hold.CDA.cardActivityDailyRecord,shift,cadrHERE,0,ReportTimeSpan);

            Statistics stat = new Statistics();
            stat.ds = new DailyStatistics[ReportTimeSpan];
            int i=-1;
            int daysCnt =0 , weeksCnt=0;
            bool DrivingWeekly10hrsOne=false, DrivingWeekly10hrsTwo = false;

            foreach (var t in cadrHERE)
            {
                i++;
                daysCnt++;
                var statsvar = stat.ds[i];
                statsvar.vDayTime = t.activityRecordDate;
                statsvar.vCardDailyActivityRecordNumber = t.activityPresenceCounter.dailyPresenceCounter;
                statsvar.vOrderedArrayId = i;
                int isCrew = 0;
                if (t.activityChangeInfo !=null)
                {
                    for (int j = 0; j < t.activityChangeInfo.Length; j++)
                    {
                        var aci = t.activityChangeInfo[j];
                        if (t.activityChangeInfo[j].getVerboseDrivingStatus() == "CREW") isCrew++;
                        else isCrew--;
                        //get the time
                        var aci_timestart = aci.minutesSinceMidnight;
                        int aci_timeend = 1440;
                        if ((j + 1) < t.activityChangeInfo.Length)
                        {
                            aci_timeend = t.activityChangeInfo[j + 1].minutesSinceMidnight;
                        }
                        var eventTime = aci_timeend - aci_timestart;
                        //process if activity is DRIVING
                        if (aci.activityType == "11")
                        {
                            statsvar.vDrivingTimeTotal += eventTime;
                            statsvar.vDrivingTimeSinceLastBreak += eventTime;
                            statsvar.vDrivingTimeWeekly += eventTime;
                            statsvar.v144hours += eventTime;
                        }
                        /* //acquiring driving period
                        if (statsvar.isDrivingPeriodOne && !statsvar.isDrivingPeriodTwo)
                            statsvar.isDrivingPeriodTwo = true;
                        else if (statsvar.isDrivingPeriodOne && statsvar.isDrivingPeriodTwo)
                            statsvar.isDrivingPeriodExtra = true;
                        else statsvar.isDrivingPeriodOne=true;

                        statsvar.vDrivingTimeTotal += eventTime;
                        //check if driving time period is without a break
                        statsvar.vDrivingTime += eventTime;
                        if ((eventTime > 270)||(statsvar.vDrivingTime > 270)) {statsvar.isDrivingTimeNotIncludesShortBreak = true;
                            statsvar.isDrivingPeriodOneExceeded = true;
                            statsvar.vDrivingTime = 0;
                        }
                        else
                        {
                            if (eventTime >= 15)
                            {
                                if (eventTime < 30) statsvar.isShortBreak15min = true;
                                else if (eventTime >= 30)
                                {
                                    statsvar.isShortBreak30min = true;
                                    if (eventTime >= 45 || statsvar.isShortBreak15min)
                                    {
                                        if (statsvar.vDrivingTimeSinceLastBreak > 270)
                                        {
                                            statsvar.vOvertime += statsvar.vDrivingTimeSinceLastBreak - 270;

                                            printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeSinceLastBreak, aci_timeend, "LongDrivingWithoutAShortBreak",
                                                (statsvar.vDrivingTimeSinceLastBreak - 270).ToString("D2") + " min");
                                        }
                                        statsvar.vDrivingTimeSinceLastBreak = 0;
                                        statsvar.isShortBreak15min = false;
                                    }
                                    else statsvar.isShortBreak15min = true;
                                }
                            }

                            if (eventTime >= 180 && (statsvar.isShortBreak15min || statsvar.isShortBreak30min) && !statsvar.isDailyBreak)
                            {
                                statsvar.isDailyBreak = true;
                                statsvar.vDailyBreakTime += eventTime;
                            }
                            else if ((statsvar.isShortBreak15min || statsvar.isShortBreak30min) && !statsvar.isDailyBreak)
                            {
                                statsvar.isDailyBreak = true;
                                statsvar.vDailyBreakTime += eventTime;
                            }
                            else if ((statsvar.isShortBreak15min || statsvar.isShortBreak30min) && statsvar.isDailyBreak)
                            {
                                statsvar.vDailyBreakTime += eventTime;
                            }
                            else if ((!statsvar.isShortBreak15min && !statsvar.isShortBreak30min) && !statsvar.isDailyBreak)
                            {
                                if (eventTime >= 15)
                                {
                                    if (eventTime >= 30)
                                    {
                                        if (eventTime >= 45)
                                        {
                                            statsvar.isShortBreak15min = true;
                                            statsvar.isShortBreak30min = true;
                                        }
                                        else
                                        {
                                            statsvar.isShortBreak30min = true;
                                        }
                                    }
                                    else
                                    {
                                        statsvar.isShortBreak15min = true;
                                    }
                                }
                            }
                        }
                    } //TODO:Crew Driving??
                }

                //if (isCrew>0) if (statsvar.vDrivingTimeTotal>1800)

                //else {
                    if ((statsvar.vDrivingTimeTotal > 540))
                        if (statsvar.vDrivingTimeTotal < 600)
                        {
                            if (DrivingWeekly10hrsOne && DrivingWeekly10hrsTwo)
                            {
                                printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeTotal, 1440, "LongDrivingDuringTheDay", (statsvar.vDrivingTimeTotal - 540).ToString("D2") + " min");
                                DrivingWeekly10hrsOne = false;
                                DrivingWeekly10hrsTwo = false;
                            }
                            else if (DrivingWeekly10hrsOne && !DrivingWeekly10hrsTwo) DrivingWeekly10hrsTwo = true;
                            else if (!DrivingWeekly10hrsOne) DrivingWeekly10hrsOne = true;
                        }
                        else printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeTotal, 1440, "LongDrivingDuringTheDay", (statsvar.vDrivingTimeTotal - 540).ToString("D2") + " min");

                //}
                statsvar.vDrivingTimeTotal = 0;

                if (daysCnt == 7)
                {
                    weeksCnt++;
                    daysCnt = 0;
                    if (statsvar.vDrivingTimeWeekly > 3360) printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeWeekly, 0, "ExcessiveDrivingDuringTheWeek", (statsvar.vDrivingTimeWeekly - 3360).ToString("D2") + " min");
                    statsvar.vDrivingTime2Weeks += statsvar.vDrivingTimeWeekly;
                    if (weeksCnt > 1)
                    {
                        if (statsvar.vDrivingTime2Weeks > 5400) printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeWeekly, 0, "ExcessiveDrivingDuringTwoConsecutiveWeeks", (statsvar.vDrivingTime2Weeks - 5400).ToString("D2") + " min");
                        statsvar.vDrivingTime2Weeks = statsvar.vDrivingTimeWeekly;
                    }
                    statsvar.vDrivingTimeWeekly = 0;
                    DrivingWeekly10hrsOne = false;
                    DrivingWeekly10hrsTwo = false;
                }

                if (t.activityChangeInfo != null)
                if (statsvar.vDailyBreakTime < 540) printfine(xmlWriter, statsvar.vDayTime, statsvar.vDailyBreakTime, 0, "NotEnoughDailyRest", (statsvar.vDailyBreakTime - 540).ToString("D2") + " min");
                statsvar.vDailyBreakTime = 0;
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
        }
        */

        private static void printfine(XmlWriter x, TimeReal statsvarVDayTime, int evtime, int aci_te, string p1, string p2)
        {
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

        public static string getVerboseTimeFromMinutes(int minutesSinceMidnight)
        {
            var n = TimeSpan.FromMinutes(minutesSinceMidnight);
            return n.Hours.ToString("D2") + ":" + n.Minutes.ToString("D2");
        }

        private static void FileDecode(DataStorage ds, int blockCount, PrintingLayout pl, PrintingLayout.DriverCard hold, String fname)
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

            Importing imp = new Importing();
            imp.CardIdentifier(ds.DataArchive[Importing.GetBlockId("EF ICC", ds)].Value, pl.cardICCI, xmlWriter);
            imp.CardChipIdentification(ds.DataArchive[Importing.GetBlockId("EF IC", ds)].Value, pl.cardCI, xmlWriter);
            int CardType = ds.DataArchive[Importing.GetBlockId("EF Application_Identification", ds)].Value[0];
            switch (CardType)
            {
                case 1:
                    {
                        Importing.DriverCardInput(ds, hold, xmlWriter);
                        //XmlOutputClass.PrintoutData(pl, hold);
                        break;
                    }
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
            }

            /*for (int i = 0; i <= blockCount; i++)
            {
            }*/

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            xmlWriter.Close();
        }

        private static int FileOpenRead(string fname, DataStorage ds)
        {
            var fs = new FileStream(fname, FileMode.Open, FileAccess.Read);

            int blockCount = 0;
            var b = ds.DataArchive[blockCount];
            while (fs.Position < fs.Length) //Reading all block by block
            {
                b = ds.DataArchive[blockCount] = new DataStorage.DataBlock();
                fs.Read(b.ID, 0, 3);
                fs.Read(b.Length, 0, 2);
                b.Value = new byte[(int)b.LengthInt()];
                fs.Read(b.Value, 0, (int)b.LengthInt());
                blockCount++;
            }
            fs.Close();
            return blockCount;
        }
    }

    internal class DataStorage
    {
        internal class DataBlock
        {
            private ByteConvert ByteCon = new ByteConvert();
            public byte[] ID = new byte[3];
            public byte[] Length = new byte[2];
            public byte[] Value;

            public int LengthInt()
            {
                return ByteCon.ToInt(Length);
            }
        }

        public DataBlock[] DataArchive = new DataBlock[100];
    }

    internal class PrintingLayout
    {
        public CardIccIdentification cardICCI = new CardIccIdentification();
        public CardChipIdentification cardCI = new CardChipIdentification();

        internal class DriverCard
        {
            public DriverCardApplicationIdentification DCAI = new DriverCardApplicationIdentification();
            public CardCertificate CC = new CardCertificate();
            public MemberStateCertificate MSC = new MemberStateCertificate();
            public CardIdentification CI = new CardIdentification();
            public DriverCardHolderIdentification DCHI = new DriverCardHolderIdentification();
            public LastCardDownload LCD = new LastCardDownload();
            public CardDrivingLicenseInformation CDLI = new CardDrivingLicenseInformation();
            public CardEventData CED = new CardEventData();
            public CardFaultData CFD = new CardFaultData();
            public CardDriverActivity CDA = new CardDriverActivity();
            public CardVehiclesUsed CVU = new CardVehiclesUsed();
            public CardPlaceDailyWorkPeriod CPDWP = new CardPlaceDailyWorkPeriod();
            public CardCurrentUse CCU = new CardCurrentUse();
            public CardControlActivityDataRecord CCADR = new CardControlActivityDataRecord();
            public SpecificConditionRecords SCR = new SpecificConditionRecords();
        }

        /*
        internal class WorkshopCard
        {
            public WorkshopCardApplicationIdentification WCAI = new WorkshopCardApplicationIdentification();
            public CardCertificate CC = new CardCertificate();
            public MemberStateCertificate MSC = new MemberStateCertificate();
            public CardIdentification CI = new CardIdentification();
            public WorkshopCardHolderIdentification WCHI = new WorkshopCardHolderIdentification();
            public NoOfCalibrationsSinceDownload NOCSD = new NoOfCalibrationsSinceDownload();
            public WorkshopCardCalibrationData WCCD = new WorkshopCardCalibrationData();
            public SensorInstallationSecData SISD = new SensorInstallationSecData();
            public CardEventData CED = new CardEventData();
            public CardFaultData CFD = new CardFaultData();
            public CardDriverActivity CDA = new CardDriverActivity();
            public CardVehiclesUsed CVU = new CardVehiclesUsed();
            public CardPlaceDailyWorkPeriod CPDWP = new CardPlaceDailyWorkPeriod();
            public CardCurrentUse CCU = new CardCurrentUse();
            public CardControlActivityDataRecord CCADR = new CardControlActivityDataRecord();
            public SpecificConditionRecords SCR = new SpecificConditionRecords();
        }

        internal class ControlCard
        {
            public ControlCardApplicationIdentification CCAI = new ControlCardApplicationIdentification();
            public CardCertificate CC = new CardCertificate();
            public MemberStateCertificate MSC = new MemberStateCertificate();
            public CardIdentification CI = new CardIdentification();
            public ControlCardHolderIdentification CCHI = new ControlCardHolderIdentification();
            public ControlCardControlActivityData CCCAD = new ControlCardControlActivityData();
        }

        internal class CompanyCard
        {
            public CompanyCardApplicationIdentification ComCAI = new CompanyCardApplicationIdentification();
            public CardCertificate CC = new CardCertificate();
            public MemberStateCertificate MSC = new MemberStateCertificate();
            public CardIdentification CI = new CardIdentification();
            public CompanyCardHolderIdentification ComCHI = new CompanyCardHolderIdentification();
            public CompanyActivityData CAD = new CompanyActivityData();
        }*/
    }

    internal struct ByteConvert
    {
        public uint ToUInt(byte[] b)
        {
            int pos = 0;
            uint res = 0;
            foreach (byte by in b)
            {
                res |= ((uint)by) << pos;
                pos += 8;
            }
            return res;
        }

        public int ToInt(byte[] b)
        {
            int pos = 8 * (b.Length - 1);
            int res = 0;
            foreach (byte by in b)
            {
                res |= by << pos;
                pos -= 8;
            }
            return res;
        }

        public char ToChar(byte b)
        {
            return (char)b;
        }

        public char[] ToCharArray(byte[] b)
        {
            int pos = 0;
            char[] res = new char[b.Length];
            foreach (byte by in b)
            {
                res[pos] = (char)by;
                pos++;
            }
            return res;
        }

        public string ToString(byte[] b)
        {
            return new string(ToCharArray(b));
        }

        //returns a formated string of hex
        //Parameters:
        //{format} - {0 for no dashes, 1 for dashed text}
        public string ToStringOfHex(byte[] b, int format)
        {
            switch (format)
            {
                case 0: return BitConverter.ToString(b).Replace("-", string.Empty);
                case 1:
                    return BitConverter.ToString(b);

                default:
                    return null;
            }
        }

        public string ToBitsString(byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
        }

        public byte GetByte(char c)
        {
            return (byte)c;
        }

        public byte GetByte(int i)
        {
            if (i < 256) return (byte)i;
            else return Byte.MinValue;
        }

        public byte[] GetBytes(int i)
        {
            if (i < 256) { byte[] temp = new byte[1]; temp[0] = (byte)i; return temp; }
            else
            {
                byte[] intBytes = BitConverter.GetBytes(i);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(intBytes);
                return intBytes;
            }
        }

        public byte[] GetByte(char[] i)
        {
            int c = 0;
            byte[] t = new byte[i.Length];
            foreach (char ch in i)
            {
                t[c] = Convert.ToByte(ch);
                c++;
            }
            return t;
        }

        public byte[] GetByte(string i)
        {
            return System.Text.Encoding.Default.GetBytes(i);
        }

        public byte[] GetByteFromHexString(string i)
        {
            int NumberChars = i.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int j = 0; j < NumberChars; j += 2)
                bytes[j / 2] = Convert.ToByte(i.Substring(j, 2), 16);
            return bytes;
        }

        internal string ToStringOfHex(byte[] b)
        {
            return BitConverter.ToString(b);
        }
    }

    internal struct Importing
    {
        public static int GetBlockId(string ID, DataStorage ds)
        {
            byte[,] blockIDs =
            {
                {0x00, 0x02, 0x00},
                {0x00, 0x05, 0x00},
                {0x05, 0x01, 0x00},
                {0xc1, 0x00, 0x00},
                {0xc1, 0x08, 0x00},
                {0x05, 0x20, 0x00},
                {0x05, 0x0E, 0x00},
                {0x05, 0x21, 0x00},
                {0x05, 0x02, 0x00},
                {0x05, 0x03, 0x00},
                {0x05, 0x04, 0x00},
                {0x05, 0x05, 0x00},
                {0x05, 0x06, 0x00},
                {0x05, 0x07, 0x00},
                {0x05, 0x08, 0x00},
                {0x05, 0x22, 0x00},
                {0x05, 0x09, 0x00},
                {0x05, 0x0A, 0x00},
                {0x05, 0x0B, 0x00},
                {0x05, 0x0C, 0x00},
                {0x05, 0x0D, 0x00}
            };
            int[] blockLengths =
            {
                25, 8, 10, 194, 194, 143, 4, 53, 1728, 1152, 13780, 6202, 1121, 19, 46, 280, 2, 9243,
                16, 10582, 10582
            };
            string[] blockNamesConst =
            {
                "EF ICC",
                "EF IC",
                "EF Application_Identification",
                "EF Card_Certificate",
                "EF CA_Certificate",
                "EF Identification",
                "EF Card_Download",
                "EF Driving_License_Info",
                "EF Events_Data",
                "EF Faults_Data",
                "EF Driver_Activity_Data",
                "EF Vehicles_Used",
                "EF Places",
                "EF Current_Usage",
                "EF Control_Activity_Data",
                "EF Specific_Conditions",
                "EF Card_Download1",
                "EF Calibration",
                "EF Sensor_Installation_Date",
                "EF Controller_Activity_Data",
                "EF Company_Activity_Data"
            };
            bool WrongConsistencyAndNoOtherOptionFlag = false;
            int i = 0;
            ByteConvert bc = new ByteConvert();
            int pos = 0;
            int DataArchiveCounter = 0;

            foreach (var s in blockNamesConst)
            {
                if (s == ID) break;
                i++;
            }

            foreach (var d in ds.DataArchive) //count THIS!!!!
            {
                byte[] b = new byte[3];
                if (i < 22)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        b[j] = blockIDs[i, j];
                    }
                }
                if (ByteCompare(d.ID, b, 3))
                {
                    if ((d.ID[0] != 0x05) & (d.ID[1] != (0x0D | 0x0C | 0x02 | 0x03 | 0x04 | 0x05 | 0x06)))
                    {
                        if (bc.ToInt(d.Length) == blockLengths[i])
                        {
                            return DataArchiveCounter;
                        }
                        pos = DataArchiveCounter;
                        WrongConsistencyAndNoOtherOptionFlag = true;
                    }
                    else
                    {
                        return DataArchiveCounter;
                    }
                }
                DataArchiveCounter++;
            }
            if (WrongConsistencyAndNoOtherOptionFlag) return pos;
            return -1;
        }

        public static Boolean ByteCompare(byte[] a, byte[] b, int l)
        {
            int ch = 0;
            for (int i = 0; i < l; i++)
            {
                if (a[i] == b[i]) ch++;
            }
            if (ch == l) return true;
            return false;
        }

        public static string Encode(int codepage, byte[] b)
        {
            try
            {
                return Encoding.GetEncoding(codepage).GetString(b);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bad encoding. Returning to defaults. Error Message: " + ex.Message + ". StackTrace: " +
                            ex.StackTrace);
                return Encoding.Default.GetString(b);
            }
        }

        private static int GetBlockId(byte[] ID, DataStorage ds)
        {
            byte[,] blockIDs =
            {
                {0x00, 0x02},
                {0x00, 0x05},
                {0x05, 0x01},
                {0xc1, 0x00},
                {0xc1, 0x08},
                {0x05, 0x20},
                {0x05, 0x0E},
                {0x05, 0x21},
                {0x05, 0x02},
                {0x05, 0x03},
                {0x05, 0x04},
                {0x05, 0x05},
                {0x05, 0x06},
                {0x05, 0x07},
                {0x05, 0x08},
                {0x05, 0x22},
                {0x05, 0x09},
                {0x05, 0x0A},
                {0x05, 0x0B},
                {0x05, 0x0C},
                {0x05, 0x0D}
            };
            int[] blockLengths =
            {
                25, 8, 10, 194, 194, 143, 4, 53, 1728, 1152, 13780, 6202, 1121, 19, 46, 280, 2, 9243,
                16, 10582, 10582
            };
            string[] blockNamesConst =
            {
                "EF ICC",
                "EF IC",
                "EF Application_Identification",
                "EF Card_Certificate",
                "EF CA_Certificate",
                "EF Identification",
                "EF Card_Download",
                "EF Driving_License_Info",
                "EF Events_Data",
                "EF Faults_Data",
                "EF Driver_Activity_Data",
                "EF Vehicles_Used",
                "EF Places",
                "EF Current_Usage",
                "EF Control_Activity_Data",
                "EF Specific_Conditions",
                "EF Card_Download",
                "EF Calibration",
                "EF Sensor_Installation_Date",
                "EF Controller_Activity_Data",
                "EF Company_Activity_Data"
            };
            //TODO
            return 0;
        }

        public void CardIdentifier(byte[] value, CardIccIdentification cardICCI, XmlWriter xmlWriter)
        {
            int carretPosition = 0;
            ByteConvert bc = new ByteConvert();
            xmlWriter.WriteStartElement("cardICCI");

            cardICCI.clockStop = value[carretPosition];
            carretPosition++;
            xmlWriter.WriteStartElement("clockStop");
            xmlWriter.WriteValue(cardICCI.clockStop);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("cardExtendedSerialNumber");

            var temp = new byte[4];
            Array.Copy(value, carretPosition, temp, 0, 4);
            carretPosition += 4;
            cardICCI.cardExtendedSerialNumber.serialNumber = bc.ToInt(temp);
            xmlWriter.WriteStartElement("serialNumber");
            xmlWriter.WriteValue(cardICCI.cardExtendedSerialNumber.serialNumber);
            xmlWriter.WriteEndElement();

            Array.Copy(value, carretPosition, cardICCI.cardExtendedSerialNumber.date.EncodedDate, 0, 2);
            carretPosition += 2;
            xmlWriter.WriteStartElement("date");
            xmlWriter.WriteAttributeString("encodedDate", bc.ToStringOfHex(cardICCI.cardExtendedSerialNumber.date.EncodedDate, 0));
            xmlWriter.WriteString(cardICCI.cardExtendedSerialNumber.date.ToString());
            xmlWriter.WriteEndElement();

            cardICCI.cardExtendedSerialNumber.equipmentType.equipmentType = value[carretPosition];
            carretPosition++;
            xmlWriter.WriteStartElement("equipmentType");
            xmlWriter.WriteAttributeString("id", cardICCI.cardExtendedSerialNumber.equipmentType.equipmentType.ToString());
            xmlWriter.WriteAttributeString("type", cardICCI.cardExtendedSerialNumber.equipmentType.getVerboseType());
            xmlWriter.WriteEndElement();

            cardICCI.cardExtendedSerialNumber.manufacturerCode.manufacturerCode = value[carretPosition];
            carretPosition++;
            xmlWriter.WriteStartElement("manufacturerCode");
            xmlWriter.WriteAttributeString("id", ((int)cardICCI.cardExtendedSerialNumber.manufacturerCode.manufacturerCode).ToString());
            xmlWriter.WriteString(cardICCI.cardExtendedSerialNumber.manufacturerCode.getVerboseCode());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            temp = new byte[8];
            Array.Copy(value, carretPosition, temp, 0, 8);
            carretPosition += 8;
            cardICCI.cardApprovalNumber = bc.ToStringOfHex(temp, 0);
            xmlWriter.WriteStartElement("cardApprovalNumber");
            xmlWriter.WriteString(cardICCI.cardApprovalNumber);
            xmlWriter.WriteEndElement();

            cardICCI.cardPersonaliserID = value[carretPosition];
            carretPosition++;
            xmlWriter.WriteStartElement("cardPersonaliserID");
            xmlWriter.WriteString(cardICCI.cardPersonaliserID.ToString("X"));
            xmlWriter.WriteEndElement();

            Array.Copy(value, carretPosition, cardICCI.embedderIcAssemblerId, 0, 5);
            carretPosition += 5;
            xmlWriter.WriteStartElement("embedderIcAssemblerID");
            xmlWriter.WriteBinHex(cardICCI.embedderIcAssemblerId, 0, cardICCI.embedderIcAssemblerId.Length);
            xmlWriter.WriteEndElement();

            Array.Copy(value, carretPosition, cardICCI.icIdentifier, 0, 2);
            xmlWriter.WriteStartElement("icIdentifier");
            xmlWriter.WriteBinHex(cardICCI.icIdentifier, 0, cardICCI.icIdentifier.Length);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();
        }

        public void CardChipIdentification(byte[] value, CardChipIdentification cardCI, XmlWriter xmlWriter)
        {
            int carretPosition = 0;
            xmlWriter.WriteStartElement("cardCI");

            Array.Copy(value, carretPosition, cardCI.icSerialNumber, 0, 4);
            carretPosition += 4;
            xmlWriter.WriteStartElement("icSerialNumber");
            xmlWriter.WriteBinHex(cardCI.icSerialNumber, 0, cardCI.icSerialNumber.Length);
            xmlWriter.WriteEndElement();

            Array.Copy(value, carretPosition, cardCI.icManufacturingReference, 0, 4);
            xmlWriter.WriteStartElement("icManufacturingReference");
            xmlWriter.WriteBinHex(cardCI.icManufacturingReference, 0, cardCI.icManufacturingReference.Length);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();
        }

        public static void DriverCardInput(DataStorage ds, PrintingLayout.DriverCard hold, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("DriverCardInput");

            // ***DriverCardIdentification*** Pos=+10=TRUE
            var block = ds.DataArchive[GetBlockId("EF Application_Identification", ds)];
            int CarretPositionUniversal = 0;
            var pr = hold.DCAI;
            ByteConvert bc = new ByteConvert();

            xmlWriter.WriteStartElement("ApplicationIdentification");

            pr.typeOfTachographCardId.equipmentType = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("equipmentType");
            xmlWriter.WriteAttributeString("id", pr.typeOfTachographCardId.equipmentType.ToString());
            xmlWriter.WriteString(pr.typeOfTachographCardId.getVerboseType());
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, pr.cardStructureVersion, 0, 2);
            CarretPositionUniversal += 2;
            xmlWriter.WriteStartElement("cardStructureVersion");
            xmlWriter.WriteBinHex(pr.cardStructureVersion, 0, pr.cardStructureVersion.Length);
            xmlWriter.WriteEndElement();

            pr.noOfEventsPerType = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("noOfEventsPerType");
            xmlWriter.WriteValue(pr.noOfEventsPerType);
            xmlWriter.WriteEndElement();

            pr.noOfFaultsPerType = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("noOfFaultsPerType");
            xmlWriter.WriteValue(pr.noOfFaultsPerType);
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, pr.activityStructureLength, 0, 2);
            CarretPositionUniversal += 2;
            xmlWriter.WriteStartElement("activityStructureLength");
            xmlWriter.WriteString(bc.ToInt(pr.activityStructureLength).ToString("D") + " Bytes");
            xmlWriter.WriteEndElement();

            var temp = new byte[2];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
            pr.noOfCardVehicleRecords = bc.ToInt(temp);
            CarretPositionUniversal += 2;
            xmlWriter.WriteStartElement("noOfCardVehicleRecords");
            xmlWriter.WriteValue(pr.noOfCardVehicleRecords);
            xmlWriter.WriteEndElement();

            pr.noOfCardPlaceRecords = block.Value[CarretPositionUniversal];
            xmlWriter.WriteStartElement("noOfCardPlaceRecords");
            xmlWriter.WriteValue(pr.noOfCardPlaceRecords);
            xmlWriter.WriteEndElement();

            CarretPositionUniversal = 0;
            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            // ***Card Certificate*** Pos=+194=TRUE
            block = ds.DataArchive[GetBlockId("EF Card_Certificate", ds)];
            var pr1 = hold.CC;
            xmlWriter.WriteStartElement("CardCertificate");

            xmlWriter.WriteStartElement("certificate");

            Array.Copy(block.Value, CarretPositionUniversal, pr1.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            xmlWriter.WriteStartElement("Sign");
            xmlWriter.WriteBinHex(pr1.certificate.Sign, 0, pr1.certificate.Sign.Length);
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, pr1.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            xmlWriter.WriteStartElement("Cndash");
            xmlWriter.WriteBinHex(pr1.certificate.Cndash, 0, pr1.certificate.Cndash.Length);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("certificateAuthorityReference");

            pr1.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric =
                block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("nationNumeric");
            xmlWriter.WriteAttributeString("id", pr1.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric.ToString("D"));
            xmlWriter.WriteString(pr1.certificate.certificateAuthorityReferenceerty.nationNumeric.getVerboseNation());
            xmlWriter.WriteEndElement();

            temp = new byte[3];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
            pr1.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha =
                new String(bc.ToCharArray(temp));
            CarretPositionUniversal += 3;
            xmlWriter.WriteStartElement("nationAlpha");
            xmlWriter.WriteAttributeString("id", pr1.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha);
            xmlWriter.WriteString(pr1.certificate.certificateAuthorityReferenceerty.nationAlpha.getVerboseAlpha());
            xmlWriter.WriteEndElement();

            pr1.certificate.certificateAuthorityReferenceerty.keySerialNumber = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("keySerialNumber");
            xmlWriter.WriteValue(pr1.certificate.certificateAuthorityReferenceerty.keySerialNumber);
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal,
                pr1.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            xmlWriter.WriteStartElement("additionalInfo");
            xmlWriter.WriteBinHex(pr1.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, pr1.certificate.certificateAuthorityReferenceerty.additionalInfo.Length);
            xmlWriter.WriteEndElement();

            pr1.certificate.certificateAuthorityReferenceerty.caIdentifier = block.Value[CarretPositionUniversal];
            xmlWriter.WriteStartElement("caIdentifier");
            xmlWriter.WriteValue(pr1.certificate.certificateAuthorityReferenceerty.caIdentifier);
            xmlWriter.WriteEndElement();

            CarretPositionUniversal = 0;
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            // ***Member State Certificate***
            block = ds.DataArchive[GetBlockId("EF CA_Certificate", ds)];
            var pr7 = hold.MSC;
            xmlWriter.WriteStartElement("CACertificate");

            xmlWriter.WriteStartElement("certificate");

            Array.Copy(block.Value, CarretPositionUniversal, pr7.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            xmlWriter.WriteStartElement("Sign");
            xmlWriter.WriteBinHex(pr7.certificate.Sign, 0, pr7.certificate.Sign.Length);
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, pr7.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            xmlWriter.WriteStartElement("Cndash");
            xmlWriter.WriteBinHex(pr7.certificate.Cndash, 0, pr7.certificate.Cndash.Length);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("certificateAuthorityReference");

            pr7.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric =
                block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("nationNumeric");
            xmlWriter.WriteAttributeString("id", pr7.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric.ToString("D"));
            xmlWriter.WriteString(pr7.certificate.certificateAuthorityReferenceerty.nationNumeric.getVerboseNation());
            xmlWriter.WriteEndElement();

            temp = new byte[3];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
            pr7.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = new string(bc.ToCharArray(temp));
            CarretPositionUniversal += 3;
            xmlWriter.WriteStartElement("nationAlpha");
            xmlWriter.WriteAttributeString("id", pr7.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha);
            xmlWriter.WriteString(pr7.certificate.certificateAuthorityReferenceerty.nationAlpha.getVerboseAlpha());
            xmlWriter.WriteEndElement();

            pr7.certificate.certificateAuthorityReferenceerty.keySerialNumber = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("keySerialNumber");
            xmlWriter.WriteValue(pr7.certificate.certificateAuthorityReferenceerty.keySerialNumber);
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal,
                pr7.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            xmlWriter.WriteStartElement("additionalInfo");
            xmlWriter.WriteBinHex(pr7.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, pr7.certificate.certificateAuthorityReferenceerty.additionalInfo.Length);
            xmlWriter.WriteEndElement();

            pr7.certificate.certificateAuthorityReferenceerty.caIdentifier = block.Value[CarretPositionUniversal];
            xmlWriter.WriteStartElement("caIdentifier");
            xmlWriter.WriteValue(pr7.certificate.certificateAuthorityReferenceerty.caIdentifier);
            xmlWriter.WriteEndElement();

            CarretPositionUniversal = 0;
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            // ***Card Identification*** Pos=+65=TRUE
            block = ds.DataArchive[GetBlockId("EF Identification", ds)];

            xmlWriter.WriteStartElement("Identification");

            var pr2 = hold.CI;
            pr2.cardIssuingMemberState.nationNumeric = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("cardIssuingMemberState");
            xmlWriter.WriteAttributeString("id", pr2.cardIssuingMemberState.nationNumeric.ToString("D"));
            xmlWriter.WriteString(pr2.cardIssuingMemberState.getVerboseNation());
            xmlWriter.WriteEndElement();

            temp = new byte[16];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 16);
            CarretPositionUniversal += 16;
            pr2.cardNumber = bc.ToString(temp);
            xmlWriter.WriteStartElement("cardNumber");
            xmlWriter.WriteString(pr2.cardNumber);
            xmlWriter.WriteEndElement();

            pr2.cardIssuingAuthorityName.codePage = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;

            temp = new byte[35];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
            pr2.cardIssuingAuthorityName.name = Encode(pr2.cardIssuingAuthorityName.codePage, temp);
            CarretPositionUniversal += 35;
            xmlWriter.WriteStartElement("cardIssuingAuthorityName");
            xmlWriter.WriteString(pr2.cardIssuingAuthorityName.name);
            xmlWriter.WriteEndElement();

            temp = new byte[4];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
            pr2.cardIssueDate.timeSec = bc.ToInt(temp);
            CarretPositionUniversal += 4;
            xmlWriter.WriteStartElement("cardIssueDate");
            xmlWriter.WriteAttributeString("timeSec", pr2.cardIssueDate.timeSec.ToString("D"));
            xmlWriter.WriteString(pr2.cardIssueDate.ConvertToUTC());
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
            pr2.cardValidityBegin.timeSec = bc.ToInt(temp);
            CarretPositionUniversal += 4;
            xmlWriter.WriteStartElement("cardValidityBegin");
            xmlWriter.WriteAttributeString("timeSec", pr2.cardValidityBegin.timeSec.ToString("D"));
            xmlWriter.WriteString(pr2.cardValidityBegin.ConvertToUTC());
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
            pr2.cardExpiryDate.timeSec = bc.ToInt(temp);
            CarretPositionUniversal += 4;
            xmlWriter.WriteStartElement("cardExpiryDate");
            xmlWriter.WriteAttributeString("timeSec", pr2.cardExpiryDate.timeSec.ToString("D"));
            xmlWriter.WriteString(pr2.cardExpiryDate.ConvertToUTC());
            xmlWriter.WriteEndElement();

            // *** Driver Card Holder Identification*** Pos=+78=TRUE

            var pr3 = hold.DCHI;

            pr3.cardHolderName.holderSurname.codePage = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;

            xmlWriter.WriteStartElement("cardHolderName");

            temp = new byte[35];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
            pr3.cardHolderName.holderSurname.name = Encode(pr3.cardHolderName.holderSurname.codePage, temp);
            CarretPositionUniversal += 35;
            xmlWriter.WriteStartElement("surname");
            xmlWriter.WriteString(pr3.cardHolderName.holderSurname.name);
            xmlWriter.WriteEndElement();

            pr3.cardHolderName.holderFirstNames.codePage = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
            pr3.cardHolderName.holderFirstNames.name = Encode(pr3.cardHolderName.holderFirstNames.codePage, temp);
            CarretPositionUniversal += 35;
            xmlWriter.WriteStartElement("firstNames");
            xmlWriter.WriteString(pr3.cardHolderName.holderFirstNames.name);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, pr3.cardHolderBirthDate.EncodedDate, 0, 4);
            CarretPositionUniversal += 4;
            xmlWriter.WriteStartElement("cardHolderBirthDate");
            xmlWriter.WriteAttributeString("encodedDate", bc.ToStringOfHex(pr3.cardHolderBirthDate.EncodedDate, 0));
            xmlWriter.WriteString(pr3.cardHolderBirthDate.ToString());
            xmlWriter.WriteEndElement();

            temp = new byte[2];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
            pr3.cardHolderPreferredLanguage.PLanguage = bc.ToCharArray(temp);
            CarretPositionUniversal = 0;
            xmlWriter.WriteStartElement("cardHolderPreferredLanguage");
            xmlWriter.WriteString(new string(pr3.cardHolderPreferredLanguage.PLanguage));
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            //*** Last Card Download *** Pos=+4=TRUE
            block = ds.DataArchive[GetBlockId("EF Card_Download", ds)];
            var pr4 = hold.LCD;
            xmlWriter.WriteStartElement("CardDownload");

            temp = new byte[4];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
            pr4.lastCardDownload.timeSec = bc.ToInt(temp);
            CarretPositionUniversal = 0;
            xmlWriter.WriteStartElement("lastCardDownload");
            xmlWriter.WriteAttributeString("timeSec", pr4.lastCardDownload.timeSec.ToString("D"));
            xmlWriter.WriteString(pr4.lastCardDownload.ConvertToUTC());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            //***Car Driving License Info ***
            block = ds.DataArchive[GetBlockId("EF Driving_License_Info", ds)];
            var pr5 = hold.CDLI;

            xmlWriter.WriteStartElement("DrivingLicenseInfo");

            pr5.drivingLicenseIssuingAuthoruty.codePage = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;

            temp = new byte[35];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
            pr5.drivingLicenseIssuingAuthoruty.name = Encode(pr5.drivingLicenseIssuingAuthoruty.codePage, temp);
            CarretPositionUniversal += 35;
            xmlWriter.WriteStartElement("drivingLicenseIssuingAuthoruty");
            xmlWriter.WriteString(pr5.drivingLicenseIssuingAuthoruty.name);
            xmlWriter.WriteEndElement();

            pr5.drivingLicenseIssuingNation.nationNumeric = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("drivingLicenseIssuingNation");
            xmlWriter.WriteAttributeString("id", pr5.drivingLicenseIssuingNation.nationNumeric.ToString("D"));
            xmlWriter.WriteString(pr5.drivingLicenseIssuingNation.getVerboseNation());
            xmlWriter.WriteEndElement();

            temp = new byte[16];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 16);
            pr5.drivingLicenseNumber = bc.ToString(temp);
            CarretPositionUniversal = 0;
            xmlWriter.WriteStartElement("drivingLicenseNumber");
            xmlWriter.WriteString(pr5.drivingLicenseNumber);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            //*** Events Reader ***
            block = ds.DataArchive[GetBlockId("EF Events_Data", ds)];
            var pr6 = hold.CED;

            xmlWriter.WriteStartElement("EventsData");

            for (var i = 0; i < 6; i++)
            {
                pr6.ceRecs[i] = new CardEventRecords { ceRec = new CardEventRecord[pr.noOfEventsPerType] };
            }

            xmlWriter.WriteStartElement("cardEventRecords");
            xmlWriter.WriteAttributeString("NumberPerType", pr.noOfEventsPerType.ToString("D"));

            for (var i = 0; i < 6; i++)
                for (var j = 0; j < pr.noOfEventsPerType; j++) //Pos=+24 each = TRUE
                {
                    xmlWriter.WriteStartElement("event");
                    xmlWriter.WriteAttributeString("Type", (i + 1).ToString("D"));
                    xmlWriter.WriteAttributeString("Number", (j + 1).ToString("D"));

                    if ((block.Value[CarretPositionUniversal + 9] != 0))
                    {
                        pr6.ceRecs[i].ceRec[j] = new CardEventRecord
                        {
                            eventType = { eventFaultType = block.Value[CarretPositionUniversal] }
                        };
                        CarretPositionUniversal++;
                        xmlWriter.WriteStartElement("eventType");
                        xmlWriter.WriteAttributeString("id", pr6.ceRecs[i].ceRec[j].eventType.eventFaultType.ToString("X"));
                        xmlWriter.WriteString(pr6.ceRecs[i].ceRec[j].eventType.getVerboseType());
                        xmlWriter.WriteEndElement();

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        pr6.ceRecs[i].ceRec[j].eventBeginTime.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;
                        xmlWriter.WriteStartElement("eventBeginTime");
                        xmlWriter.WriteAttributeString("timeSec", pr6.ceRecs[i].ceRec[j].eventBeginTime.timeSec.ToString("D"));
                        xmlWriter.WriteString(pr6.ceRecs[i].ceRec[j].eventBeginTime.ConvertToUTC());
                        xmlWriter.WriteEndElement();

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        pr6.ceRecs[i].ceRec[j].eventEndTime.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;
                        xmlWriter.WriteStartElement("eventEndTime");
                        xmlWriter.WriteAttributeString("timeSec", pr6.ceRecs[i].ceRec[j].eventEndTime.timeSec.ToString("D"));
                        xmlWriter.WriteString(pr6.ceRecs[i].ceRec[j].eventEndTime.ConvertToUTC());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("eventVehicleRegistration");

                        pr6.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;
                        xmlWriter.WriteStartElement("vehicleRegistrationNation");
                        xmlWriter.WriteAttributeString("id", pr6.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric.ToString("D"));
                        xmlWriter.WriteString(pr6.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNation.getVerboseNation());
                        xmlWriter.WriteEndElement();

                        temp = new byte[14];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 14);
                        pr6.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage = temp[0];
                        for (var k = 1; k < temp.Length; k++)
                            temp[k - 1] = temp[k];
                        pr6.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                            Encode(pr6.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage, temp);
                        CarretPositionUniversal += 14;
                        xmlWriter.WriteStartElement("vehicleRegistrationNumber");
                        xmlWriter.WriteString(pr6.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteEndElement();
                    }
                    else
                    {
                        temp = new byte[24]; Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 24);
                        CarretPositionUniversal += 24;
                    }

                    xmlWriter.WriteEndElement();
                }

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            CarretPositionUniversal = 0;
            //*** Faults Reader ***
            block = ds.DataArchive[GetBlockId("EF Faults_Data", ds)];

            xmlWriter.WriteStartElement("FaultsData");

            for (var i = 0; i < 2; i++)
            {
                hold.CFD.cardFaultRecords[i] = new CardFaultRecords { cfRec = new CardEventRecord[pr.noOfFaultsPerType] };
            }

            xmlWriter.WriteStartElement("cardFaultRecords");
            xmlWriter.WriteAttributeString("NumberPerType", pr.noOfFaultsPerType.ToString("D"));

            for (var i = 0; i < 2; i++)
                for (var j = 0; j < pr.noOfFaultsPerType; j++) //Pos+=24 each = TRUE
                {
                    xmlWriter.WriteStartElement("event");
                    xmlWriter.WriteAttributeString("Type", (i + 1).ToString("D"));
                    xmlWriter.WriteAttributeString("Number", (j + 1).ToString("D"));

                    if ((block.Value[CarretPositionUniversal + 9] != 0))
                    {
                        hold.CFD.cardFaultRecords[i].cfRec[j] = new CardEventRecord
                        {
                            eventType = { eventFaultType = block.Value[CarretPositionUniversal] }
                        };
                        CarretPositionUniversal++;
                        xmlWriter.WriteStartElement("faultType");
                        xmlWriter.WriteAttributeString("id", hold.CFD.cardFaultRecords[i].cfRec[j].eventType.eventFaultType.ToString("X"));
                        xmlWriter.WriteString(hold.CFD.cardFaultRecords[i].cfRec[j].eventType.getVerboseType());
                        xmlWriter.WriteEndElement();

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.CFD.cardFaultRecords[i].cfRec[j].eventBeginTime.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;
                        xmlWriter.WriteStartElement("eventBeginTime");
                        xmlWriter.WriteAttributeString("timeSec", hold.CFD.cardFaultRecords[i].cfRec[j].eventBeginTime.timeSec.ToString("D"));
                        xmlWriter.WriteString(hold.CFD.cardFaultRecords[i].cfRec[j].eventBeginTime.ConvertToUTC());
                        xmlWriter.WriteEndElement();

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.CFD.cardFaultRecords[i].cfRec[j].eventEndTime.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;
                        xmlWriter.WriteStartElement("eventEndTime");
                        xmlWriter.WriteAttributeString("timeSec", hold.CFD.cardFaultRecords[i].cfRec[j].eventEndTime.timeSec.ToString("D"));
                        xmlWriter.WriteString(hold.CFD.cardFaultRecords[i].cfRec[j].eventEndTime.ConvertToUTC());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("eventVehicleRegistration");

                        hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNation
                                .nationNumeric =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;
                        xmlWriter.WriteStartElement("vehicleRegistrationNation");
                        xmlWriter.WriteAttributeString("id", hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric.ToString("D"));
                        xmlWriter.WriteString(hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNation.getVerboseNation());
                        xmlWriter.WriteEndElement();

                        hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[13];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                        hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber
                                .vehicleRegNumber =
                            Encode(
                                hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber
                                    .codePage,
                                temp);
                        CarretPositionUniversal += 13;
                        xmlWriter.WriteStartElement("vehicleRegistrationNumber");
                        xmlWriter.WriteString(hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteEndElement();
                    }
                    else
                    {
                        temp = new byte[24]; Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 24);
                        CarretPositionUniversal += 24;
                    }

                    xmlWriter.WriteEndElement();
                }

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            CarretPositionUniversal = 0;
            //*** Daily Records ***
            block = ds.DataArchive[GetBlockId("EF Driver_Activity_Data", ds)];

            xmlWriter.WriteStartElement("DriverActivityData");

            var CyclicLongitude = bc.ToInt(hold.DCAI.activityStructureLength);
            hold.CDA.cyclicDataRAW = new byte[CyclicLongitude];

            temp = new byte[2];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
            hold.CDA.activityPointerOldestRecord = bc.ToInt(temp);
            CarretPositionUniversal += 2;
            xmlWriter.WriteStartElement("oldestRecord");
            xmlWriter.WriteValue(hold.CDA.activityPointerOldestRecord);
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
            hold.CDA.activityPointerNewestRecord = bc.ToInt(temp);
            CarretPositionUniversal += 2;
            xmlWriter.WriteStartElement("newestRecord");
            xmlWriter.WriteValue(hold.CDA.activityPointerNewestRecord);
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, hold.CDA.cyclicDataRAW, 0, CyclicLongitude);
            CarretPositionUniversal += CyclicLongitude;

            xmlWriter.WriteStartElement("cardActivityDailyRecords");

            var cadrtemp = hold.CDA.CyclicDataParser(CyclicLongitude, xmlWriter, hold.CDA.activityPointerOldestRecord,
                hold.CDA.activityPointerNewestRecord);

            hold.CDA.cardActivityDailyRecord = cadrtemp;

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            CarretPositionUniversal = 0;
            //*** Vehicle Records ***
            block = ds.DataArchive[GetBlockId("EF Vehicles_Used", ds)];

            xmlWriter.WriteStartElement("VehiclesUsed");

            temp = new byte[2];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
            hold.CVU.vehiclePointerNewestRecord = bc.ToInt(temp);
            CarretPositionUniversal += 2;
            xmlWriter.WriteStartElement("vehiclePointerNewestRecord");
            xmlWriter.WriteValue(hold.CVU.vehiclePointerNewestRecord);
            xmlWriter.WriteEndElement();

            hold.CVU.cVehRecs = new CardVehicleRecord[pr.noOfCardVehicleRecords];

            for (var i = 0; i < pr.noOfCardVehicleRecords; i++) //Pos+=31 = TRUE
            {
                hold.CVU.cVehRecs[i] = new CardVehicleRecord();
                temp = new byte[3];
                Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                CarretPositionUniversal += 3;
                hold.CVU.cVehRecs[i].vehicleOdometerBegin = bc.ToInt(temp);

                Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                CarretPositionUniversal += 3;
                hold.CVU.cVehRecs[i].vehicleOdometerEnd = bc.ToInt(temp);

                temp = new byte[4];
                Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                CarretPositionUniversal += 4;
                hold.CVU.cVehRecs[i].vehicleFirstUse.timeSec = bc.ToInt(temp);

                Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                CarretPositionUniversal += 4;
                hold.CVU.cVehRecs[i].vehicleLastUse.timeSec = bc.ToInt(temp);

                hold.CVU.cVehRecs[i].registration.vehicleRegistrationNation.nationNumeric =
                    block.Value[CarretPositionUniversal];
                CarretPositionUniversal++;

                hold.CVU.cVehRecs[i].registration.vehicleRegistrationNumber.codePage =
                    block.Value[CarretPositionUniversal];
                CarretPositionUniversal++;

                temp = new byte[13];
                Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                CarretPositionUniversal += 13;
                hold.CVU.cVehRecs[i].registration.vehicleRegistrationNumber.vehicleRegNumber = bc.ToString(temp);

                temp = new byte[2];
                Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                CarretPositionUniversal += 2;
                hold.CVU.cVehRecs[i].vuDataBlockCounter.vuDataBlockCounter = bc.ToInt(temp);
            }

            xmlWriter.WriteStartElement("cardVehicleRecords");
            xmlWriter.WriteAttributeString("Number", pr.noOfCardVehicleRecords.ToString("D"));
            int jj = 0;
            for (var i = hold.CVU.vehiclePointerNewestRecord + 1; i < pr.noOfCardVehicleRecords; i++)
            {
                jj++;

                xmlWriter.WriteStartElement("cardVehicleRecord");
                xmlWriter.WriteAttributeString("Number", (jj).ToString("D"));

                if ((hold.CVU.cVehRecs[i].vehicleOdometerBegin == 0) && (hold.CVU.cVehRecs[i].vehicleOdometerEnd == 0) &&
                    (hold.CVU.cVehRecs[i].vehicleFirstUse.timeSec == 0))
                {
                    xmlWriter.WriteEndElement();
                    break;
                }

                xmlWriter.WriteStartElement("vehicleOdometerBegin");
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].vehicleOdometerBegin.ToString("D") + " km");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleOdometerEnd");
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].vehicleOdometerEnd.ToString("D") + " km");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleFirstUse");
                xmlWriter.WriteAttributeString("timeSec", hold.CVU.cVehRecs[i].vehicleFirstUse.timeSec.ToString("D"));
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].vehicleFirstUse.ConvertToUTC());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleLastUse");
                xmlWriter.WriteAttributeString("timeSec", hold.CVU.cVehRecs[i].vehicleLastUse.timeSec.ToString("D"));
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].vehicleLastUse.ConvertToUTC());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("registration");

                xmlWriter.WriteStartElement("vehicleRegistrationNation");
                xmlWriter.WriteAttributeString("id", hold.CVU.cVehRecs[i].registration.vehicleRegistrationNation.nationNumeric.ToString("D"));
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].registration.vehicleRegistrationNation.getVerboseNation());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleRegistrationNumber");
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].registration.vehicleRegistrationNumber.vehicleRegNumber);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vuDataBlockCounter");
                xmlWriter.WriteValue(hold.CVU.cVehRecs[i].vuDataBlockCounter.vuDataBlockCounter);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }

            for (var i = 0; i <= hold.CVU.vehiclePointerNewestRecord; i++)
            {
                xmlWriter.WriteStartElement("cardVehicleRecord");
                xmlWriter.WriteAttributeString("Number", (jj + i).ToString("D"));

                if ((hold.CVU.cVehRecs[i].vehicleOdometerBegin == 0) && (hold.CVU.cVehRecs[i].vehicleOdometerEnd == 0) &&
    (hold.CVU.cVehRecs[i].vehicleFirstUse.timeSec == 0))
                {
                    xmlWriter.WriteEndElement();
                    break;
                }

                xmlWriter.WriteStartElement("vehicleOdometerBegin");
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].vehicleOdometerBegin.ToString("D") + " km");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleOdometerEnd");
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].vehicleOdometerEnd.ToString("D") + " km");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleFirstUse");
                xmlWriter.WriteAttributeString("timeSec", hold.CVU.cVehRecs[i].vehicleFirstUse.timeSec.ToString("D"));
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].vehicleFirstUse.ConvertToUTC());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleLastUse");
                xmlWriter.WriteAttributeString("timeSec", hold.CVU.cVehRecs[i].vehicleLastUse.timeSec.ToString("D"));
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].vehicleLastUse.ConvertToUTC());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("registration");

                xmlWriter.WriteStartElement("vehicleRegistrationNation");
                xmlWriter.WriteAttributeString("id", hold.CVU.cVehRecs[i].registration.vehicleRegistrationNation.nationNumeric.ToString("D"));
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].registration.vehicleRegistrationNation.getVerboseNation());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleRegistrationNumber");
                xmlWriter.WriteString(hold.CVU.cVehRecs[i].registration.vehicleRegistrationNumber.vehicleRegNumber);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vuDataBlockCounter");
                xmlWriter.WriteValue(hold.CVU.cVehRecs[i].vuDataBlockCounter.vuDataBlockCounter);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            CarretPositionUniversal = 0;
            //*** Places ***
            block = ds.DataArchive[GetBlockId("EF Places", ds)];

            xmlWriter.WriteStartElement("Places");

            hold.CPDWP.placePointerNewestRecord = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("placePointerNewestRecord");
            xmlWriter.WriteValue(hold.CPDWP.placePointerNewestRecord);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("placeRecords");
            xmlWriter.WriteAttributeString("noOfCardPlaceRecords", pr.noOfCardPlaceRecords.ToString("D"));

            hold.CPDWP.plRecs = new PlaceRecord[pr.noOfCardPlaceRecords];

            for (var i = 0; i < pr.noOfCardPlaceRecords; i++) //Pos+=10 = TRUE
            {
                hold.CPDWP.plRecs[i] = new PlaceRecord();

                temp = new byte[4];
                Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                CarretPositionUniversal += 4;
                hold.CPDWP.plRecs[i].entryTime.timeSec = bc.ToInt(temp);

                hold.CPDWP.plRecs[i].entryTypeDailyWorkPeriod.entryTypeDailyWorkPeriod =
                    block.Value[CarretPositionUniversal];
                CarretPositionUniversal++;

                hold.CPDWP.plRecs[i].dailyWorkPeriodCountry.nationNumeric = block.Value[CarretPositionUniversal];
                CarretPositionUniversal++;

                hold.CPDWP.plRecs[i].dailyWorkPeriodRegion.regionNumeric = block.Value[CarretPositionUniversal];
                CarretPositionUniversal++;

                temp = new byte[3];
                Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                CarretPositionUniversal += 3;
                hold.CPDWP.plRecs[i].vehicleOdometerValue = bc.ToInt(temp);
            }
            jj = 0;
            for (int i = hold.CPDWP.placePointerNewestRecord + 1; i < pr.noOfCardPlaceRecords; i++)
            {
                jj++;
                xmlWriter.WriteStartElement("PlaceEntry");
                xmlWriter.WriteAttributeString("Number", jj.ToString("D"));

                xmlWriter.WriteStartElement("entryTime");
                xmlWriter.WriteAttributeString("timeSec", hold.CPDWP.plRecs[i].entryTime.timeSec.ToString("D"));
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].entryTime.ConvertToUTC());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("entryTypeDailyWorkPeriod");
                xmlWriter.WriteAttributeString("id", hold.CPDWP.plRecs[i].entryTypeDailyWorkPeriod.entryTypeDailyWorkPeriod.ToString("D"));
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].entryTypeDailyWorkPeriod.getVerboseEntryType());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("dailyWorkPeriodCountry");
                xmlWriter.WriteAttributeString("id", hold.CPDWP.plRecs[i].dailyWorkPeriodCountry.nationNumeric.ToString("D"));
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].dailyWorkPeriodCountry.getVerboseNation());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("dailyWorkPeriodRegion");
                xmlWriter.WriteAttributeString("id", hold.CPDWP.plRecs[i].dailyWorkPeriodRegion.regionNumeric.ToString("D"));
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].dailyWorkPeriodRegion.getVerboseNation());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleOdometerValue");
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].vehicleOdometerValue.ToString("D") + " km");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }
            for (int i = 0; i <= hold.CPDWP.placePointerNewestRecord; i++)
            {
                xmlWriter.WriteStartElement("PlaceEntry");
                xmlWriter.WriteAttributeString("Number", (jj + i).ToString("D"));

                xmlWriter.WriteStartElement("entryTime");
                xmlWriter.WriteAttributeString("timeSec", hold.CPDWP.plRecs[i].entryTime.timeSec.ToString("D"));
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].entryTime.ConvertToUTC());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("entryTypeDailyWorkPeriod");
                xmlWriter.WriteAttributeString("id", hold.CPDWP.plRecs[i].entryTypeDailyWorkPeriod.entryTypeDailyWorkPeriod.ToString("D"));
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].entryTypeDailyWorkPeriod.getVerboseEntryType());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("dailyWorkPeriodCountry");
                xmlWriter.WriteAttributeString("id", hold.CPDWP.plRecs[i].dailyWorkPeriodCountry.nationNumeric.ToString("D"));
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].dailyWorkPeriodCountry.getVerboseNation());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("dailyWorkPeriodRegion");
                xmlWriter.WriteAttributeString("id", hold.CPDWP.plRecs[i].dailyWorkPeriodRegion.regionNumeric.ToString("D"));
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].dailyWorkPeriodRegion.getVerboseNation());
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("vehicleOdometerValue");
                xmlWriter.WriteString(hold.CPDWP.plRecs[i].vehicleOdometerValue.ToString("D") + " km");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            CarretPositionUniversal = 0;
            //*** Current usage *** Pos=+19=TRUE
            block = ds.DataArchive[GetBlockId("EF Current_Usage", ds)];

            xmlWriter.WriteStartElement("CurrentUsage");

            temp = new byte[4];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
            hold.CCU.sessionOpenTime.timeSec = bc.ToInt(temp);
            CarretPositionUniversal += 4;
            xmlWriter.WriteStartElement("sessionOpenTime");
            xmlWriter.WriteAttributeString("timeSec", hold.CCU.sessionOpenTime.timeSec.ToString("D"));
            xmlWriter.WriteString(hold.CCU.sessionOpenTime.ConvertToUTC());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("sessionOpenVehicle");

            hold.CCU.sessionOpenVehicle.vehicleRegistrationNation.nationNumeric = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("vehicleRegistrationNation");
            xmlWriter.WriteAttributeString("id", hold.CCU.sessionOpenVehicle.vehicleRegistrationNation.nationNumeric.ToString("D"));
            xmlWriter.WriteString(hold.CCU.sessionOpenVehicle.vehicleRegistrationNation.getVerboseNation());
            xmlWriter.WriteEndElement();

            hold.CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;

            temp = new byte[13];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
            hold.CCU.sessionOpenVehicle.vehicleRegistrationNumber.vehicleRegNumber =
                Encode(hold.CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage, temp);
            CarretPositionUniversal += 13;
            xmlWriter.WriteStartElement("vehicleRegistrationNumber");
            xmlWriter.WriteString(hold.CCU.sessionOpenVehicle.vehicleRegistrationNumber.vehicleRegNumber[0] !=
                                  (char)0x00
                ? hold.CCU.sessionOpenVehicle.vehicleRegistrationNumber.vehicleRegNumber
                : "undefined");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            CarretPositionUniversal = 0;
            //*** Control Activity Record *** Pos=+46=38?=
            block = ds.DataArchive[GetBlockId("EF Control_Activity_Data", ds)];

            xmlWriter.WriteStartElement("ControlActivityData");

            var t = block.Value[CarretPositionUniversal];
            hold.CCADR.controlType.controlType = bc.ToBitsString(t);
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("controlType");
            xmlWriter.WriteAttributeString("byte", t.ToString("D"));
            xmlWriter.WriteAttributeString("binary", hold.CCADR.controlType.controlType);
            xmlWriter.WriteString(hold.CCADR.controlType.getVerboseControlType());
            xmlWriter.WriteEndElement();

            temp = new byte[4];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
            hold.CCADR.controlTime.timeSec = bc.ToInt(temp);
            CarretPositionUniversal += 4;
            xmlWriter.WriteStartElement("controlTime");
            xmlWriter.WriteAttributeString("timeSec", hold.CCADR.controlTime.timeSec.ToString("D"));
            xmlWriter.WriteString(hold.CCADR.controlTime.ConvertToUTC());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("controlCardNumber");

            hold.CCADR.controlCardNumber.cardType.equipmentType = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("equipmentType");
            xmlWriter.WriteAttributeString("id", hold.CCADR.controlCardNumber.cardType.equipmentType.ToString("D"));
            xmlWriter.WriteString(hold.CCADR.controlCardNumber.cardType.getVerboseType());
            xmlWriter.WriteEndElement();

            hold.CCADR.controlCardNumber.cardIssuingMemberState.nationNumeric = block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("cardIssuingMemberState");
            xmlWriter.WriteAttributeString("id", hold.CCADR.controlCardNumber.cardIssuingMemberState.nationNumeric.ToString("D"));
            xmlWriter.WriteString(hold.CCADR.controlCardNumber.cardIssuingMemberState.getVerboseNation());
            xmlWriter.WriteEndElement();

            temp = new byte[16];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 16);
            CarretPositionUniversal += 16;

            hold.CCADR.controlCardNumber.cardNumber.cardConsecutiveIndex.index = (char)temp[13];
            hold.CCADR.controlCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex = (char)temp[15];
            hold.CCADR.controlCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex = (char)temp[14];
            var tem = new byte[13];
            Array.Copy(temp, 0, tem, 0, 13);
            hold.CCADR.controlCardNumber.cardNumber.driverIdentification = bc.ToString(tem);
            xmlWriter.WriteStartElement("cardNumber");
            xmlWriter.WriteString(hold.CCADR.controlCardNumber.cardNumber.driverIdentification +
                hold.CCADR.controlCardNumber.cardNumber.cardConsecutiveIndex.index +
                hold.CCADR.controlCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex +
                hold.CCADR.controlCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("controlVehicleRegistration");

            hold.CCADR.controlVehicleRegistration.vehicleRegistrationNation.nationNumeric =
                block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;
            xmlWriter.WriteStartElement("vehicleRegistrationNation");
            xmlWriter.WriteAttributeString("id", hold.CCADR.controlVehicleRegistration.vehicleRegistrationNation.nationNumeric.ToString("D"));
            xmlWriter.WriteString(hold.CCADR.controlVehicleRegistration.vehicleRegistrationNation.getVerboseNation());
            xmlWriter.WriteEndElement();

            hold.CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage =
                block.Value[CarretPositionUniversal];
            CarretPositionUniversal++;

            Array.Copy(block.Value, CarretPositionUniversal, tem, 0, 13);
            hold.CCADR.controlVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                Encode(hold.CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage, tem);
            CarretPositionUniversal += 13;
            xmlWriter.WriteStartElement("vehicleRegistrationNumber");
            xmlWriter.WriteString(hold.CCADR.controlVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            temp = new byte[4];
            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
            hold.CCADR.controlDownloadPeriodBegin.timeSec = bc.ToInt(temp);
            CarretPositionUniversal += 4;
            xmlWriter.WriteStartElement("controlDownloadPeriodBegin");
            xmlWriter.WriteAttributeString("timeSec", hold.CCADR.controlDownloadPeriodBegin.timeSec.ToString("D"));
            xmlWriter.WriteString(hold.CCADR.controlDownloadPeriodBegin.ConvertToUTC());
            xmlWriter.WriteEndElement();

            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
            hold.CCADR.controlDownloadPeriodEnd.timeSec = bc.ToInt(temp);
            xmlWriter.WriteStartElement("controlDownloadPeriodEnd");
            xmlWriter.WriteAttributeString("timeSec", hold.CCADR.controlDownloadPeriodEnd.timeSec.ToString("D"));
            xmlWriter.WriteString(hold.CCADR.controlDownloadPeriodEnd.ConvertToUTC());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            CarretPositionUniversal = 0;
            //*** Specific Conditions *** Pos+=280=TRUE
            block = ds.DataArchive[GetBlockId("EF Specific_Conditions", ds)];

            xmlWriter.WriteStartElement("SpecificConditions");

            xmlWriter.WriteStartElement("specificConditionRecords");
            xmlWriter.WriteAttributeString("Number", "56");

            hold.SCR.scRec = new SpecificConditionRecord[56];
            for (var i = 0; i < 56; i++)
            {
                xmlWriter.WriteStartElement("SpecificConditionRecord");
                xmlWriter.WriteAttributeString("Number", (i + 1).ToString("D"));

                hold.SCR.scRec[i] = new SpecificConditionRecord();

                temp = new byte[4];
                Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                CarretPositionUniversal += 4;
                hold.SCR.scRec[i].entryTime.timeSec = bc.ToInt(temp);

                hold.SCR.scRec[i].specificConditionType.specificConditionType = block.Value[CarretPositionUniversal];
                CarretPositionUniversal++;

                if (hold.SCR.scRec[i].entryTime.timeSec != 0)
                {
                    xmlWriter.WriteStartElement("entryTime");
                    xmlWriter.WriteAttributeString("timeSec", hold.SCR.scRec[i].entryTime.timeSec.ToString("D"));
                    xmlWriter.WriteString(hold.SCR.scRec[i].entryTime.ConvertToUTC());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("specificConditionType");
                    xmlWriter.WriteAttributeString("id", hold.SCR.scRec[i].specificConditionType.specificConditionType.ToString("D"));
                    xmlWriter.WriteString(hold.SCR.scRec[i].specificConditionType.getVerboseType());
                    xmlWriter.WriteEndElement();
                }
                else
                    xmlWriter.WriteString("Undefined");

                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();

            xmlWriter.WriteEndElement();

            xmlWriter.Flush();
        }

        /*        public static void WorkshopCardInput(DataStorage ds, PrintingLayout.WorkshopCard hold)
                {
                    // ***WorkshopCardIdentification*** Pos=+11=TRUE
                    var block = ds.DataArchive[GetBlockId("EF Application_Identification", ds)];
                    int CarretPositionUniversal = 0;
                    var pr = hold.WCAI;
                    ByteConvert bc = new ByteConvert();

                    pr.typeOfTachographId.equipmentType = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal, pr.cardStructureVersion, 0, 2);
                    CarretPositionUniversal += 2;
                    pr.noOfEventsPerType = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    pr.noOfFaultsPerType = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    var temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    pr.activityStructureLength = bc.ToInt(temp);
                    CarretPositionUniversal += 2;
                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    CarretPositionUniversal += 2;
                    pr.noOfCardVehicleRecords = bc.ToInt(temp);
                    pr.noOfCardPlaceRecords = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    pr.noOfCalibrationRecords = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal = 0;

                    // ***Card Certificate*** Pos=+194=TRUE
                    block = ds.DataArchive[GetBlockId("EF Card_Certificate", ds)];
                    var pr1 = hold.CC;

                    Array.Copy(block.Value, CarretPositionUniversal, pr1.certificate.Sign, 0, 128);
                    CarretPositionUniversal += 128;
                    Array.Copy(block.Value, CarretPositionUniversal, pr1.certificate.Cndash, 0, 58);
                    CarretPositionUniversal += 58;
                    pr1.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric =
                        block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    temp = new byte[3];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                    pr1.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = bc.ToCharArray(temp).ToString();
                    CarretPositionUniversal += 3;
                    pr1.certificate.certificateAuthorityReferenceerty.keySerialNumber = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal,
                        pr1.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
                    CarretPositionUniversal += 2;
                    pr1.certificate.certificateAuthorityReferenceerty.caIdentifier = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal = 0;

                    // ***Member State Certificate***
                    block = ds.DataArchive[GetBlockId("EF CA_Certificate", ds)];
                    var pr7 = hold.MSC;

                    Array.Copy(block.Value, CarretPositionUniversal, pr7.certificate.Sign, 0, 128);
                    CarretPositionUniversal += 128;
                    Array.Copy(block.Value, CarretPositionUniversal, pr7.certificate.Cndash, 0, 58);
                    CarretPositionUniversal += 58;
                    pr7.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric =
                        block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    temp = new byte[3];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                    pr7.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = bc.ToCharArray(temp).ToString();
                    CarretPositionUniversal += 3;
                    pr7.certificate.certificateAuthorityReferenceerty.keySerialNumber = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal,
                        pr7.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
                    CarretPositionUniversal += 2;
                    pr7.certificate.certificateAuthorityReferenceerty.caIdentifier = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal = 0;

                    // ***Card Identification*** Pos=+65=TRUE
                    block = ds.DataArchive[GetBlockId("EF Identification", ds)];
                    var pr2 = hold.CI;
                    pr2.cardIssuingMemberState.nationNumeric = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    temp = new byte[16];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 16);
                    CarretPositionUniversal += 16;
                    pr2.cardNumber = bc.ToStringOfHex(temp, 0);

                    pr2.cardIssuingAuthorityName.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    temp = new byte[35];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    pr2.cardIssuingAuthorityName.name = Encode(pr2.cardIssuingAuthorityName.codePage, temp);
                    CarretPositionUniversal += 35;

                    temp = new byte[4];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    pr2.cardIssueDate.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    pr2.cardValidityBegin.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    pr2.cardExpiryDate.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    // *** Workshop Card Holder Identification*** Pos=+146=TRUE
                    hold.WCHI.workshopName.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    temp = new byte[35];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.WCHI.workshopName.name = Encode(hold.WCHI.workshopName.codePage, temp);
                    CarretPositionUniversal += 35;

                    hold.WCHI.workshopAddress.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.WCHI.workshopAddress.address = Encode(hold.WCHI.workshopAddress.codePage, temp);
                    CarretPositionUniversal += 35;

                    hold.WCHI.cardHolderName.holderSurname.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.WCHI.cardHolderName.holderSurname.name = Encode(hold.WCHI.cardHolderName.holderSurname.codePage, temp);
                    CarretPositionUniversal += 35;

                    hold.WCHI.cardHolderName.holderFirstNames.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.WCHI.cardHolderName.holderFirstNames.name = Encode(hold.WCHI.cardHolderName.holderFirstNames.codePage, temp);
                    CarretPositionUniversal += 35;

                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.WCHI.cardHolderPreferredLanguage.PLanguage = bc.ToCharArray(temp);
                    CarretPositionUniversal =0;

                    //*** Calibratoin Number Since Download *** Pos=+2=TRUE
                    block = ds.DataArchive[GetBlockId("EF Card_Download1", ds)];
                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.NOCSD.noOfCalibrationsSinceDownload = bc.ToInt(temp);
                    CarretPositionUniversal =0;

                    //*** Calibration Records ***
                    block = ds.DataArchive[GetBlockId("EF Calibration", ds)];
                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.WCCD.calibrationTotalNumber = bc.ToInt(temp);
                    CarretPositionUniversal += 2;

                    hold.WCCD.calibrationPointerNewestRecord = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    hold.WCCD.wshopCCR = new WorkshopCardCalibrationRecord[hold.WCAI.noOfCalibrationRecords];

                    for (var i = 1; i < hold.WCAI.noOfCalibrationRecords; i++) //Pos=+105=TRUE
                    {
                        hold.WCCD.wshopCCR[i].calibrationPurpose.calibrationPurpose = block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[17];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 17);
                        hold.WCCD.wshopCCR[i].VIN.vehicleIdentificationNumber = bc.ToStringOfHex(temp, 0);
                        CarretPositionUniversal += 17;

                        hold.WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNation.nationNumeric =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNumber.codePage =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[13];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                        hold.WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                            Encode(hold.WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNumber.codePage, temp);
                        CarretPositionUniversal += 13;

                        temp = new byte[2];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                        hold.WCCD.wshopCCR[i].wVehicleCharacteristicConstant = bc.ToInt(temp);
                        CarretPositionUniversal += 2;

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                        hold.WCCD.wshopCCR[i].kConstantOfRecordingEquipment = bc.ToInt(temp);
                        CarretPositionUniversal += 2;

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                        hold.WCCD.wshopCCR[i].lTyreCircumference = bc.ToInt(temp);
                        CarretPositionUniversal += 2;

                        temp = new byte[15];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 15);
                        hold.WCCD.wshopCCR[i].tyreSize.tyreSize = bc.ToString(temp);
                        CarretPositionUniversal += 15;

                        hold.WCCD.wshopCCR[i].authorisedSpeed = block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[3];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                        hold.WCCD.wshopCCR[i].oldOdometerValue = bc.ToInt(temp);
                        CarretPositionUniversal += 3;

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                        hold.WCCD.wshopCCR[i].newOdometerValue = bc.ToInt(temp);
                        CarretPositionUniversal += 3;

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.WCCD.wshopCCR[i].oldTimeValue.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.WCCD.wshopCCR[i].newTimeValue.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 17);
                        hold.WCCD.wshopCCR[i].nextCalibrationDate.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;

                        temp = new byte[16];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 16);
                        hold.WCCD.wshopCCR[i].vuPartNumber = bc.ToStringOfHex(temp,0);
                        CarretPositionUniversal += 16;

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.WCCD.wshopCCR[i].VuSerialNumber.serialNumber = bc.ToInt(temp);
                        CarretPositionUniversal += 4;
                        Array.Copy(block.Value, CarretPositionUniversal, hold.WCCD.wshopCCR[i].VuSerialNumber.date.EncodedDate, 0, 2);
                        CarretPositionUniversal += 2;
                        hold.WCCD.wshopCCR[i].VuSerialNumber.equipmentType.equipmentType = block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;
                        hold.WCCD.wshopCCR[i].VuSerialNumber.manufacturerCode.manufacturerCode =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.serialNumber = bc.ToInt(temp);
                        CarretPositionUniversal += 4;
                        Array.Copy(block.Value, CarretPositionUniversal, hold.WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.date.EncodedDate, 0, 2);
                        CarretPositionUniversal += 2;
                        hold.WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.equipmentType.equipmentType =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;
                        hold.WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.manufacturerCode.manufacturerCode =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;
                    }

                    CarretPositionUniversal = 0;
                    //*** Sensor Instalation Data *** //Pos=+16=TRUE
                    block = ds.DataArchive[GetBlockId("EF Sensor_Installation_Date", ds)];
                    temp = new byte[8];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 8);
                    hold.SISD.sensorInstallationSecData.tDesKeyA = bc.ToStringOfHex(temp, 0);
                    CarretPositionUniversal += 8;

                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 8);
                    hold.SISD.sensorInstallationSecData.tDesKeyB = bc.ToStringOfHex(temp, 0);
                    CarretPositionUniversal =0;

                    //*** Events Reader ***
                    block = ds.DataArchive[GetBlockId("EF Events_Data", ds)];
                    for (var i = 0; i <= 5; i++)
                        hold.CED.ceRecs[i].ceRec = new CardEventRecord[2];

                    for (var i = 0; i <= 5; i++)
                        for (var j = 0; j <= 2; j++) //Pos=+24 each = TRUE
                        {
                            hold.CED.ceRecs[i].ceRec[j].eventType.eventFaultType = block.Value[CarretPositionUniversal];
                            CarretPositionUniversal++;

                            temp = new byte[4];
                            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                            hold.CED.ceRecs[i].ceRec[j].eventBeginTime.timeSec = bc.ToInt(temp);
                            CarretPositionUniversal += 4;

                            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                            hold.CED.ceRecs[i].ceRec[j].eventEndTime.timeSec = bc.ToInt(temp);
                            CarretPositionUniversal += 4;

                            hold.CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric =
                                block.Value[CarretPositionUniversal];
                            CarretPositionUniversal++;

                            hold.CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage =
                                block.Value[CarretPositionUniversal];
                            CarretPositionUniversal++;

                            temp = new byte[13];
                            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                            hold.CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                                Encode(hold.CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage, temp);
                            CarretPositionUniversal += 13;
                        }

                    CarretPositionUniversal = 0;
                    //*** Faults Reader ***
                    block = ds.DataArchive[GetBlockId("EF Faults_Data", ds)];
                    for (var i = 0; i <= 1; i++)
                        hold.CFD.cardFaultRecords[i].cfRec = new CardEventRecord[5];

                    for (var i = 0; i <= 1; i++)
                        for (var j = 0; j <= 5; j++) //Pos+=24 each = TRUE
                        {
                            hold.CFD.cardFaultRecords[i].cfRec[j].eventType.eventFaultType =
                                block.Value[CarretPositionUniversal];
                            CarretPositionUniversal++;

                            temp = new byte[4];
                            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                            hold.CFD.cardFaultRecords[i].cfRec[j].eventBeginTime.timeSec = bc.ToInt(temp);
                            CarretPositionUniversal += 4;

                            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                            hold.CFD.cardFaultRecords[i].cfRec[j].eventEndTime.timeSec = bc.ToInt(temp);
                            CarretPositionUniversal += 4;

                            hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNation
                                    .nationNumeric =
                                block.Value[CarretPositionUniversal];
                            CarretPositionUniversal++;

                            hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage =
                                block.Value[CarretPositionUniversal];
                            CarretPositionUniversal++;

                            temp = new byte[13];
                            Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                            hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                                Encode(
                                    hold.CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage,
                                    temp);
                            CarretPositionUniversal += 13;
                        }

                    CarretPositionUniversal = 0;
                    //*** Daily Records ***
                    block = ds.DataArchive[GetBlockId("EF Driver_Activity_Data", ds)];
                    var CyclicLongitude = hold.WCAI.activityStructureLength;
                    hold.CDA.cyclicData = new byte[CyclicLongitude - 1];

                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.CDA.activityPointerOldestRecord = bc.ToInt(temp);
                    CarretPositionUniversal += 2;

                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.CDA.activityPointerNewestRecord = BitConverter.ToInt16(temp, 0);
                    CarretPositionUniversal += 2;

                    Array.Copy(block.Value,CarretPositionUniversal, hold.CDA.cyclicData, 0, CyclicLongitude);

                    hold.CDA.CyclicDataParser(CyclicLongitude / 5544);

                    CarretPositionUniversal = 0;
                    //*** Vehicle Records ***
                    block = ds.DataArchive[GetBlockId("EF Vehicles_Used", ds)];

                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.CVU.vehiclePointerNewestRecord = bc.ToInt(temp);
                    CarretPositionUniversal += 2;

                    hold.CVU.cVehRecs = new CardVehicleRecord[hold.WCAI.noOfCardVehicleRecords - 1];

                    for (var i = 0; i < hold.WCAI.noOfCardVehicleRecords; i++) //Pos+=31 = TRUE
                    {
                        temp = new byte[3];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                        CarretPositionUniversal += 3;
                        hold.CVU.cVehRecs[i].vehicleOdometerBegin = bc.ToInt(temp);

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                        CarretPositionUniversal += 3;
                        hold.CVU.cVehRecs[i].vehicleOdometerEnd = bc.ToInt(temp);

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        CarretPositionUniversal += 4;
                        hold.CVU.cVehRecs[i].vehicleFirstUse.timeSec = bc.ToInt(temp);

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        CarretPositionUniversal += 4;
                        hold.CVU.cVehRecs[i].vehicleLastUse.timeSec = bc.ToInt(temp);

                        hold.CVU.cVehRecs[i].registration.vehicleRegistrationNation.nationNumeric =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.CVU.cVehRecs[i].registration.vehicleRegistrationNumber.codePage =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[13];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                        CarretPositionUniversal += 13;
                        hold.CVU.cVehRecs[i].registration.vehicleRegistrationNumber.vehicleRegNumber =
                            Encode(hold.CVU.cVehRecs[i].registration.vehicleRegistrationNumber.codePage, temp);

                        temp = new byte[2];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                        CarretPositionUniversal += 2;
                        hold.CVU.cVehRecs[i].vuDataBlockCounter.vuDataBlockCounter = bc.ToInt(temp);
                    }

                    CarretPositionUniversal = 0;
                    //*** Places ***
                    block = ds.DataArchive[GetBlockId("EF Places", ds)];

                    hold.CPDWP.placePointerNewestRecord = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    hold.CPDWP.plRecs = new PlaceRecord[hold.WCAI.noOfCardPlaceRecords - 1];

                    for (var i = 0; i < hold.WCAI.noOfCardPlaceRecords; i++) //Pos+=10 = TRUE
                    {
                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        CarretPositionUniversal += 4;
                        hold.CPDWP.plRecs[i].entryTime.timeSec = bc.ToInt(temp);

                        hold.CPDWP.plRecs[i].entryTypeDailyWorkPeriod.entryTypeDailyWorkPeriod =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.CPDWP.plRecs[i].dailyWorkPeriodCountry.nationNumeric = block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.CPDWP.plRecs[i].dailyWorkPeriodRegion.regionNumeric = block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[3];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                        CarretPositionUniversal += 3;
                        hold.CPDWP.plRecs[i].vehicleOdometerValue = bc.ToInt(temp);
                    }

                    CarretPositionUniversal = 0;
                    //*** Current usage *** Pos=+19=TRUE
                    block = ds.DataArchive[GetBlockId("EF Current_Usage", ds)];

                    temp = new byte[4];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    hold.CCU.sessionOpenTime.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    hold.CCU.sessionOpenVehicle.vehicleRegistrationNation.nationNumeric = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    hold.CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    temp = new byte[13];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                    hold.CCU.sessionOpenVehicle.vehicleRegistrationNumber.vehicleRegNumber =
                        Encode(hold.CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage, temp);

                    CarretPositionUniversal = 0;
                    //*** Control Activity Record *** Pos=+46=38?=46=TRUE
                    block = ds.DataArchive[GetBlockId("EF Control_Activity_Data", ds)];

                    byte t = block.Value[CarretPositionUniversal];
                    hold.CCADR.controlType.controlType = t.ToString();
                    CarretPositionUniversal++;

                    temp = new byte[4];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    hold.CCADR.controlTime.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    hold.CCADR.controlCardNumber.cardType.equipmentType = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    hold.CCADR.controlCardNumber.cardIssuingMemberState.nationNumeric = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal ++;

                    temp = new byte[16];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 16);
                    CarretPositionUniversal += 16;
                    hold.CCADR.controlCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex = (char)temp[15];
                    hold.CCADR.controlCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex = (char)temp[14];
                    var tem = new byte[13];
                    Array.Copy(temp, 0, tem, 0, 14);
                    hold.CCADR.controlCardNumber.cardNumber.driverIdentification = bc.ToString(tem);

                    hold.CCADR.controlVehicleRegistration.vehicleRegistrationNation.nationNumeric =
                        block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    hold.CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage =
                        block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    Array.Copy(block.Value, CarretPositionUniversal, tem, 0, 13);
                    CarretPositionUniversal += 13;
                    hold.CCADR.controlVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                        Encode(hold.CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage, tem);

                    temp = new byte[4];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    hold.CCADR.controlDownloadPeriodBegin.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    hold.CCADR.controlDownloadPeriodEnd.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal = 0;

                    //*** Specific Conditions *** Pos+=280=TRUE
                    block = ds.DataArchive[GetBlockId("EF Specific_Conditions", ds)];

                    hold.SCR.scRec = new SpecificConditionRecord[1];
                    for (var i = 0; i < 2; i++)
                    {
                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        CarretPositionUniversal += 4;
                        hold.SCR.scRec[i].entryTime.timeSec = bc.ToInt(temp);

                        hold.SCR.scRec[i].specificConditionType.specificConditionType = block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;
                    }
                }

                public static void ControlCardInput(DataStorage ds, PrintingLayout.ControlCard hold)
                {
                    var block = ds.DataArchive[GetBlockId("EF Application_Identification", ds)];
                    int CarretPositionUniversal = 0;
                    var pr = hold.CCAI;
                    ByteConvert bc = new ByteConvert();

                    // ***ControlCardIdentification*** Pos=+5=TRUE
                    pr.typeOfTachographCardId.equipmentType = 1;
                    CarretPositionUniversal++;

                    Array.Copy(block.Value, CarretPositionUniversal, pr.cardStructureVersion, 0, 2);
                    CarretPositionUniversal += 2;

                    var temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    CarretPositionUniversal = 0;
                    pr.noOfControlActivityRecords = bc.ToInt(temp);

                    // ***Card Certificate*** Pos=+194=TRUE
                    block = ds.DataArchive[GetBlockId("EF Card_Certificate", ds)];
                    var pr1 = hold.CC;

                    Array.Copy(block.Value, CarretPositionUniversal, pr1.certificate.Sign, 0, 128);
                    CarretPositionUniversal += 128;
                    Array.Copy(block.Value, CarretPositionUniversal, pr1.certificate.Cndash, 0, 58);
                    CarretPositionUniversal += 58;
                    pr1.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric =
                        block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    temp = new byte[3];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                    pr1.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = bc.ToCharArray(temp).ToString();
                    CarretPositionUniversal += 3;
                    pr1.certificate.certificateAuthorityReferenceerty.keySerialNumber = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal,
                        pr1.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
                    CarretPositionUniversal += 2;
                    pr1.certificate.certificateAuthorityReferenceerty.caIdentifier = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal = 0;

                    // ***Member State Certificate***
                    block = ds.DataArchive[GetBlockId("EF CA_Certificate", ds)];
                    var pr7 = hold.MSC;

                    Array.Copy(block.Value, CarretPositionUniversal, pr7.certificate.Sign, 0, 128);
                    CarretPositionUniversal += 128;
                    Array.Copy(block.Value, CarretPositionUniversal, pr7.certificate.Cndash, 0, 58);
                    CarretPositionUniversal += 58;
                    pr7.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric =
                        block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    temp = new byte[3];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                    pr7.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = bc.ToCharArray(temp).ToString();
                    CarretPositionUniversal += 3;
                    pr7.certificate.certificateAuthorityReferenceerty.keySerialNumber = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal,
                        pr7.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
                    CarretPositionUniversal += 2;
                    pr7.certificate.certificateAuthorityReferenceerty.caIdentifier = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal = 0;

                    // ***Card Identification*** Pos=+65=TRUE
                    block = ds.DataArchive[GetBlockId("EF Identification", ds)];
                    var pr2 = hold.CI;
                    pr2.cardIssuingMemberState.nationNumeric = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    temp = new byte[16];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 16);
                    CarretPositionUniversal += 16;
                    pr2.cardNumber = bc.ToStringOfHex(temp, 0);

                    pr2.cardIssuingAuthorityName.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    temp = new byte[35];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    pr2.cardIssuingAuthorityName.name = Encode(pr2.cardIssuingAuthorityName.codePage, temp);
                    CarretPositionUniversal += 35;

                    temp = new byte[4];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    pr2.cardIssueDate.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    pr2.cardValidityBegin.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    pr2.cardExpiryDate.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    // *** Control Card Holder Identification *** Pos=+146=TRUE
                    hold.CCHI.controlBodyName.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    temp = new byte[35];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.CCHI.controlBodyName.name = Importing.Encode(hold.CCHI.controlBodyName.codePage, temp);
                    CarretPositionUniversal += 35;

                    hold.CCHI.controlBodyAddress.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.CCHI.controlBodyAddress.address = Importing.Encode(hold.CCHI.controlBodyAddress.codePage, temp);
                    CarretPositionUniversal += 35;

                    hold.CCHI.cardHolderName.holderSurname.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.CCHI.cardHolderName.holderSurname.name = Importing.Encode(hold.CCHI.cardHolderName.holderSurname.codePage, temp);
                    CarretPositionUniversal += 35;

                    hold.CCHI.cardHolderName.holderFirstNames.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.CCHI.cardHolderName.holderFirstNames.name = Importing.Encode(hold.CCHI.cardHolderName.holderFirstNames.codePage, temp);
                    CarretPositionUniversal += 35;

                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.CCHI.cardHolderPreferredLanguage.PLanguage = bc.ToCharArray(temp);
                    CarretPositionUniversal =0;

                    //*** Control Card Control Activity Data ***
                    block = ds.DataArchive[GetBlockId("EF Controller_Activity_Data",ds)];
                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.CCCAD.controlPointerNewestRecord = bc.ToInt(temp);
                    CarretPositionUniversal += 2;

                    hold.CCCAD.conActRecs = new ControlActivityRecord[hold.CCAI.noOfControlActivityRecords - 1];

                    for (var i = 0; i < hold.CCAI.noOfControlActivityRecords; i++) //Pos=+105=TRUE
                    {
                        hold.CCCAD.conActRecs[i].controlType.controlType = block.Value[CarretPositionUniversal].ToString();
                        CarretPositionUniversal++;

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.CCCAD.conActRecs[i].controlTime.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;

                        hold.CCCAD.conActRecs[i].controlledCardNumber.cardType.equipmentType =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.CCCAD.conActRecs[i].controlledCardNumber.cardIssuingMemberState.nationNumeric =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[13];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                        hold.CCCAD.conActRecs[i].controlledCardNumber.cardNumber.ownerIdentification = bc.ToString(temp);
                        CarretPositionUniversal += 13;

                        hold.CCCAD.conActRecs[i].controlledCardNumber.cardNumber.cardConsecutiveIndex.index =
                            (char) block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.CCCAD.conActRecs[i].controlledCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex =
                            (char) block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.CCCAD.conActRecs[i].controlledCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex =
                            (char) block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.CCCAD.conActRecs[i].controlledVehicleRegistration.vehicleRegistrationNation.nationNumeric =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.CCCAD.conActRecs[i].controlledVehicleRegistration.vehicleRegistrationNumber.codePage =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[13];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                        hold.CCCAD.conActRecs[i].controlledVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                            bc.ToString(temp);
                        CarretPositionUniversal += 13;

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.CCCAD.conActRecs[i].controlDownloadPeriodBegin.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.CCCAD.conActRecs[i].controlDownloadPeriodEnd.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;
                    }
                }

                public static void CompanyCardInput(DataStorage ds, PrintingLayout.CompanyCard hold)
                {
                    var block = ds.DataArchive[GetBlockId("EF Application_Identification", ds)];
                    int CarretPositionUniversal = 0;
                    var pr = hold.ComCAI;
                    ByteConvert bc = new ByteConvert();

                    // ***CompanyCardIdentification*** Pos=+5=TRUE
                    pr.typeOfTachographCardId.equipmentType = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    Array.Copy(block.Value, CarretPositionUniversal, pr.cardStructureVersion, 0, 4);
                    CarretPositionUniversal += 2;

                    var temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    CarretPositionUniversal += 2;
                    pr.noOfCompanyActivityRecords = bc.ToInt(temp);

                    // ***Card Certificate*** Pos=+194=TRUE
                    block = ds.DataArchive[GetBlockId("EF Card_Certificate", ds)];
                    var pr1 = hold.CC;

                    Array.Copy(block.Value, CarretPositionUniversal, pr1.certificate.Sign, 0, 128);
                    CarretPositionUniversal += 128;
                    Array.Copy(block.Value, CarretPositionUniversal, pr1.certificate.Cndash, 0, 58);
                    CarretPositionUniversal += 58;
                    pr1.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric =
                        block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    temp = new byte[3];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                    pr1.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = bc.ToCharArray(temp).ToString();
                    CarretPositionUniversal += 3;
                    pr1.certificate.certificateAuthorityReferenceerty.keySerialNumber = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal,
                        pr1.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
                    CarretPositionUniversal += 2;
                    pr1.certificate.certificateAuthorityReferenceerty.caIdentifier = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal = 0;

                    // ***Member State Certificate***
                    block = ds.DataArchive[GetBlockId("EF CA_Certificate", ds)];
                    var pr7 = hold.MSC;

                    Array.Copy(block.Value, CarretPositionUniversal, pr7.certificate.Sign, 0, 128);
                    CarretPositionUniversal += 128;
                    Array.Copy(block.Value, CarretPositionUniversal, pr7.certificate.Cndash, 0, 58);
                    CarretPositionUniversal += 58;
                    pr7.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric =
                        block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    temp = new byte[3];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 3);
                    pr7.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = bc.ToCharArray(temp).ToString();
                    CarretPositionUniversal += 3;
                    pr7.certificate.certificateAuthorityReferenceerty.keySerialNumber = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal,
                        pr7.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
                    CarretPositionUniversal += 2;
                    pr7.certificate.certificateAuthorityReferenceerty.caIdentifier = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal = 0;

                    // ***Card Identification*** Pos=+65=TRUE
                    block = ds.DataArchive[GetBlockId("EF Identification", ds)];
                    var pr2 = hold.CI;
                    pr2.cardIssuingMemberState.nationNumeric = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    temp = new byte[16];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 16);
                    CarretPositionUniversal += 16;
                    pr2.cardNumber = bc.ToStringOfHex(temp, 0);

                    pr2.cardIssuingAuthorityName.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;

                    temp = new byte[35];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    pr2.cardIssuingAuthorityName.name = Encode(pr2.cardIssuingAuthorityName.codePage, temp);
                    CarretPositionUniversal += 35;

                    temp = new byte[4];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    pr2.cardIssueDate.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    pr2.cardValidityBegin.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                    pr2.cardExpiryDate.timeSec = bc.ToInt(temp);
                    CarretPositionUniversal += 4;

                    // *** Company Card Holder Identification *** Pos=+146=TRUE
                    hold.ComCHI.companyName.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    temp = new byte[35];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.ComCHI.companyName.name = Importing.Encode(hold.ComCHI.companyName.codePage, temp);
                    CarretPositionUniversal += 35;

                    hold.ComCHI.companyAddress.codePage = block.Value[CarretPositionUniversal];
                    CarretPositionUniversal++;
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 35);
                    hold.ComCHI.companyAddress.address = Importing.Encode(hold.ComCHI.companyAddress.codePage, temp);
                    CarretPositionUniversal += 35;

                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.ComCHI.cardHolderPreferredLanguage.PLanguage = bc.ToCharArray(temp);
                    CarretPositionUniversal = 0;

                    //*** Company Card Control Activity Data ***
                    block = ds.DataArchive[GetBlockId("EF Company_Activity_Data", ds)];
                    temp = new byte[2];
                    Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 2);
                    hold.CAD.companyPointerNewestRecord = bc.ToInt(temp);
                    CarretPositionUniversal += 2;

                    hold.CAD.cRecs = new CompanyActivityRecord[hold.ComCAI.noOfCompanyActivityRecords - 1];

                    for (var i = 0; i < hold.ComCAI.noOfCompanyActivityRecords; i++) //Pos=+105=TRUE
                    {
                        hold.CAD.cRecs[i].companyActivityType.companyActivityType = block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.CAD.cRecs[i].companyActivityTime.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;

                        hold.CAD.cRecs[i].cardNumberInformation.cardType.equipmentType = block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        hold.CAD.cRecs[i].cardNumberInformation.cardIssuingMemberState.nationNumeric =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[13];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                        hold.CAD.cRecs[i].cardNumberInformation.cardNumber.ownerIdentification = bc.ToString(temp);
                        CarretPositionUniversal += 13;

                        hold.CAD.cRecs[i].vehicleRegistrationInformation.vehicleRegistrationNumber.codePage =
                            block.Value[CarretPositionUniversal];
                        CarretPositionUniversal++;

                        temp = new byte[13];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 13);
                        hold.CAD.cRecs[i].vehicleRegistrationInformation.vehicleRegistrationNumber.vehicleRegNumber =
                            Encode(hold.CAD.cRecs[i].vehicleRegistrationInformation.vehicleRegistrationNumber.codePage, temp);
                        CarretPositionUniversal += 13;

                        temp = new byte[4];
                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.CAD.cRecs[i].downloadPeriodBegin.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;

                        Array.Copy(block.Value, CarretPositionUniversal, temp, 0, 4);
                        hold.CAD.cRecs[i].downloadPeriodEnd.timeSec = bc.ToInt(temp);
                        CarretPositionUniversal += 4;
                    }
                }
            }

                */

        //All classes are stored in separate file;
    }

    /*class XmlOutputClass
    {
        internal static void PrintoutData(PrintingLayout pl, PrintingLayout.DriverCard hold)
        {
            //0
            //2
            xmlWriter.WriteStartElement("cardICCI");
            //3
            xmlWriter.WriteStartElement("clockStop");
            xmlWriter.WriteValue(pl.cardICCI.clockStop);
            xmlWriter.WriteEndElement();
            //3
            xmlWriter.WriteStartElement("cardExtendedSerialNumber");
            //4
            xmlWriter.WriteStartElement("serialNumber");
            xmlWriter.WriteValue(pl.cardICCI.cardExtendedSerialNumber.serialNumber);
            xmlWriter.WriteEndElement();
            //4
            xmlWriter.WriteStartElement("date");
            xmlWriter.WriteString(pl.cardICCI.cardExtendedSerialNumber.date.ToString());
            xmlWriter.WriteEndElement();
            //4
            xmlWriter.WriteStartElement("equipmentType");
            xmlWriter.WriteAttributeString("id", pl.cardICCI.cardExtendedSerialNumber.equipmentType.equipmentType.ToString());
            xmlWriter.WriteAttributeString("type", pl.cardICCI.cardExtendedSerialNumber.equipmentType.getVerboseType());
            xmlWriter.WriteEndElement();
            //4
            xmlWriter.WriteStartElement("manufacturerCode");
            xmlWriter.WriteString(pl.cardICCI.cardExtendedSerialNumber.manufacturerCode.getVerboseCode());
            xmlWriter.WriteEndElement();
            //3
            xmlWriter.WriteEndElement();
            //3
            xmlWriter.WriteStartElement("cardApprovalNumber");
            xmlWriter.WriteString(pl.cardICCI.cardApprovalNumber);
            xmlWriter.WriteEndElement();
            //3
            xmlWriter.WriteStartElement("cardPersonaliserID");

            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndDocument();
            xmlWriter.Close();

            /*
            public CardIccIdentification cardICCI = new CardIccIdentification();
            public CardChipIdentification cardCI = new CardChipIdentification();
            public DriverCardApplicationIdentification DCAI = new DriverCardApplicationIdentification();
            public CardCertificate CC = new CardCertificate();
            public MemberStateCertificate MSC = new MemberStateCertificate();
            public CardIdentification CI = new CardIdentification();
            public DriverCardHolderIdentification DCHI = new DriverCardHolderIdentification();
            public LastCardDownload LCD = new LastCardDownload();
            public CardDrivingLicenseInformation CDLI = new CardDrivingLicenseInformation();
            public CardEventData CED = new CardEventData();
            public CardFaultData CFD = new CardFaultData();
            public CardDriverActivity CDA = new CardDriverActivity();
            public CardVehiclesUsed CVU = new CardVehiclesUsed();
            public CardPlaceDailyWorkPeriod CPDWP = new CardPlaceDailyWorkPeriod();
            public CardCurrentUse CCU = new CardCurrentUse();
            public CardControlActivityDataRecord CCADR = new CardControlActivityDataRecord();
            public SpecificConditionRecords SCR = new SpecificConditionRecords();

             * "EF ICC",
                "EF IC",
                "EF Application_Identification",
                "EF Card_Certificate",
                "EF CA_Certificate",
                "EF Identification",
                "EF Card_Download",
                "EF Driving_License_Info",
                "EF Events_Data",
                "EF Faults_Data",
                "EF Driver_Activity_Data",
                "EF Vehicles_Used",
                "EF Places",
                "EF Current_Usage",
                "EF Control_Activity_Data",
                "EF Specific_Conditions",
             *
             *
             *
        }
    }
    */
}