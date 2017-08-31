using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using static DDDReader_0._4._0_renewed.DeclarationOfClassesUsed;

namespace DDDReader_0._4._0_renewed
{ //This is an analytical module. It performs ananlysis of 561/2006 Decrete violations from recieved array.
  //Output is also a class.
    internal class QueueManager
    {
        private Queue functionsQueue;

        public bool IsEmpty
        {
            get
            {
                if (functionsQueue.Count == 0)
                    return true;
                return false;
            }
        }

        public QueueManager()
        {
            functionsQueue = new Queue();
        }

        public QueueManager(int size)
        {
            functionsQueue = new Queue(size);
        }

        public bool Contains(Action action)
        {
            if (functionsQueue.Contains(action))
                return true;
            else
                return false;
        }

        public int Count()
        {
            return functionsQueue.Count;
        }

        public Action Pop()
        {
            return functionsQueue.Dequeue() as Action;
        }

        public void Add(Action function)
        {
            functionsQueue.Enqueue(function);
        }
    }

    internal class _561_2006Control
    {   //TODO: Rebuild!!!
        public static void GetWeeklyViolations(List<ViolationRecord> weeklyViolations, int ReportTimeSpan, CardActivityDailyRecord[] cadrHERE, Statistics stat)
        {
            stat.ds = new DailyStatistics[ReportTimeSpan];
            int i = -1;
            int daysCnt = 0, weeksCnt = 0;
            bool DrivingWeekly10hrsOne = false, DrivingWeekly10hrsTwo = false;

            foreach (var t in cadrHERE)
            {
                i++;
                daysCnt++;
                var statsvar = stat.ds[i];
                statsvar.vDayTime = t.activityRecordDate;
                statsvar.vCardDailyActivityRecordNumber = t.activityPresenceCounter.dailyPresenceCounter;
                statsvar.vOrderedArrayId = i;
                int isCrew = 0;
                if (t.activityChangeInfo != null)
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
                        //acquiring driving period
                        if (statsvar.isDrivingPeriodOne && !statsvar.isDrivingPeriodTwo)
                            statsvar.isDrivingPeriodTwo = true;
                        else if (statsvar.isDrivingPeriodOne && statsvar.isDrivingPeriodTwo)
                            statsvar.isDrivingPeriodExtra = true;
                        else statsvar.isDrivingPeriodOne = true;

                        statsvar.vDrivingTimeTotal += eventTime;
                        //check if driving time period is without a break
                        statsvar.vDrivingTimeSinceLastBreak += eventTime;
                        /*if ((eventTime > 270)||(statsvar.vDrivingTimeSinceLastBreak > 270)) {statsvar.isDrivingTimeNotIncludesShortBreak = true;
                            statsvar.isDrivingPeriodOneExceeded = true;
                            statsvar.vDrivingTimeSinceLastBreak = 0;
                        }
                        else*/
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

                                            //  printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeSinceLastBreak, aci_timeend, "LongDrivingWithoutAShortBreak",
                                            //  (statsvar.vDrivingTimeSinceLastBreak - 270).ToString("D2") + " min");
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
                            //printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeTotal, 1440, "LongDrivingDuringTheDay", (statsvar.vDrivingTimeTotal - 540).ToString("D2") + " min");
                            DrivingWeekly10hrsOne = false;
                            DrivingWeekly10hrsTwo = false;
                        }
                        else if (DrivingWeekly10hrsOne && !DrivingWeekly10hrsTwo) DrivingWeekly10hrsTwo = true;
                        else if (!DrivingWeekly10hrsOne) DrivingWeekly10hrsOne = true;
                    }
                    else //printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeTotal, 1440, "LongDrivingDuringTheDay", (statsvar.vDrivingTimeTotal - 540).ToString("D2") + " min");

                        //}
                        statsvar.vDrivingTimeTotal = 0;

                if (daysCnt == 7)
                {
                    weeksCnt++;
                    daysCnt = 0;
                    if (statsvar.vDrivingTimeWeekly > 3360)
                        weeklyViolations.Add(new ViolationRecord
                        {
                            amount = statsvar.vDrivingTimeWeekly - 3360,
                            VIOLATION_TYPE = "ExcessiveDrivingDuringTheWeek",
                            ViolationDateTime = statsvar.vDayTime.ConvertToDateTime(),
                            ViolationText = "Exceeding driving time in a single week for " +
                                            (statsvar.vDrivingTimeWeekly - 3360).ToString("D2") + " min"
                        });
                    //printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeWeekly, 0, "ExcessiveDrivingDuringTheWeek", (statsvar.vDrivingTimeWeekly - 3360).ToString("D2") + " min");
                    statsvar.vDrivingTime2Weeks += statsvar.vDrivingTimeWeekly;
                    if (weeksCnt > 1)
                    {
                        if (statsvar.vDrivingTime2Weeks > 5400)
                            weeklyViolations.Add(new ViolationRecord
                            {
                                amount = statsvar.vDrivingTime2Weeks - 5400,
                                VIOLATION_TYPE = "ExcessiveDrivingDuringTwoConsecutiveWeeks",
                                ViolationDateTime = statsvar.vDayTime.ConvertToDateTime(),
                                ViolationText = "Exceeding driving time in two consecutive weeks for " +
                                                (statsvar.vDrivingTime2Weeks - 5400).ToString("D2") + " min"
                            });
                        //printfine(xmlWriter, statsvar.vDayTime, statsvar.vDrivingTimeWeekly, 0, "ExcessiveDrivingDuringTwoConsecutiveWeeks", (statsvar.vDrivingTime2Weeks - 5400).ToString("D2") + " min");
                        statsvar.vDrivingTime2Weeks = statsvar.vDrivingTimeWeekly;
                    }
                    statsvar.vDrivingTimeWeekly = 0;
                    DrivingWeekly10hrsOne = false;
                    DrivingWeekly10hrsTwo = false;
                }

                /*if (t.activityChangeInfo != null)
                if (statsvar.vDailyBreakTime < 540) printfine(xmlWriter, statsvar.vDayTime, statsvar.vDailyBreakTime, 0, "NotEnoughDailyRest", (statsvar.vDailyBreakTime - 540).ToString("D2") + " min");
                statsvar.vDailyBreakTime = 0;*/
            }
        }

        public static List<ActivityChangeInfoExtended> aFaultProofListOfEvents(CardActivityDailyRecord[] cadrHERE)
        {
            List<ActivityChangeInfoExtended> listoq = new List<ActivityChangeInfoExtended>();
            for (int i = 0; i < cadrHERE.Length; i++)
            {
                DateTime dayOfRecord = cadrHERE[i].activityRecordDate.ConvertToDateTime();
                if (cadrHERE[i].activityChangeInfo != null)
                {
                    var activityArray = cadrHERE[i].activityChangeInfo;
                    for (int j = 0; j < activityArray.Length; j++)
                    {
                        var A = new ActivityChangeInfoExtended();
                        A.StartingDateTime = dayOfRecord.AddMinutes(activityArray[j].minutesSinceMidnight);
                        A.EndingDateTime = j + 1 < activityArray.Length
                            ? A.StartingDateTime.AddMinutes(activityArray[j + 1].minutesSinceMidnight - activityArray[j].minutesSinceMidnight - 1)
                            : A.StartingDateTime.AddMinutes(1439 - activityArray[j].minutesSinceMidnight);
                        A.Duration = (int)A.EndingDateTime.Subtract(A.StartingDateTime).TotalMinutes;
                        A.ActivityType = activityArray[j].getVerboseActivity();
                        A.DriverCardStatus = activityArray[j].getVerboseDriverCardStatus();
                        A.DrivingStatus = activityArray[j].getVerboseDrivingStatus();
                        A.SlotStatus = activityArray[j].getVerboseSlotStatus();
                        listoq.Add(A);
                    }
                }
            }
            //Check and delete duplicates
            for (int i = 0; i < listoq.Count; i++)
            {
                if (i + 1 < listoq.Count)
                {
                    if (listoq[i].ActivityType == listoq[i + 1].ActivityType)
                    {
                        if (listoq[i].DriverCardStatus == listoq[i + 1].DriverCardStatus)
                        {
                            if (listoq[i].DrivingStatus == listoq[i + 1].DrivingStatus)
                            {
                                if (listoq[i].SlotStatus == listoq[i + 1].SlotStatus)
                                {
                                    listoq[i].EndingDateTime = listoq[i + 1].EndingDateTime;
                                    listoq[i].Duration = (int)listoq[i].EndingDateTime.Subtract(listoq[i].StartingDateTime)
                                        .TotalMinutes;
                                    listoq.RemoveAt(i + 1);
                                }
                            }
                        }
                    }
                }
            }

            return listoq;
        }

        public static List<ShiftStats> ShiftFinesCounting(CardActivityDailyRecord[] cadr)
        {
            //for debugging only!!
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = ("\t"),
                OmitXmlDeclaration = true
            };

            XmlWriter xml = XmlWriter.Create("_561 - 2006ReportDEBUG.xml", settings);
            xml.WriteStartDocument();
            xml.WriteStartElement("ShiftsDebugging");
            xml.WriteStartElement("ShiftsDebugging_DateAndTime");
            //end of debugging block

            var c = new Control();
            List<ActivityChangeInfoExtended> acie_in = aFaultProofListOfEvents(cadr);

            QueueManager queue = new QueueManager(acie_in.Count * 13 + 1);

            //Queue queue = new Queue(acie_in.Count*4+1); //output queue for debugging

            int recordsShiftCounter = 0;
            List<ShiftStats> ss = new List<ShiftStats>();
            int i = 0;
            while (acie_in.Count > 0)
            {
                i++;
                var t = c.GetOneShift(acie_in);

                PrintDebugShifts(t, queue, xml, i);//debugging

                recordsShiftCounter = t.Count+1;
                ss.Add(c.GetStatsForSingleShift(t, i));
                var aaa = acie_in.Count - recordsShiftCounter;
                acie_in = aaa > 0 ? acie_in.GetRange(recordsShiftCounter - 1, aaa) : acie_in.GetRange(acie_in.Count - 1, 0);
            }

            //debugging block
            xml.WriteEndElement();
            xml.WriteStartElement("ShiftEvents");
            for (int x = 0; x < queue.Count(); x++)
            {
                queue.Pop();
            }
            xml.WriteEndElement();
            xml.WriteEndElement();
            xml.WriteEndDocument();
            xml.Flush();
            //end debugging block

            return ss;
        }

        private static void PrintDebugShifts(List<ActivityChangeInfoExtended> ss, QueueManager queue, XmlWriter xml, int shiftcounter)
        {
            var A = ss.First().StartingDateTime;
            var B = ss.Last().EndingDateTime;

            xml.WriteStartElement("SingleShift");

            xml.WriteAttributeString("ShiftNumber", shiftcounter.ToString("D"));
            xml.WriteAttributeString("StartingDateTime", A.ToString("f"));
            xml.WriteAttributeString("EndingDateTime", B.ToString("f"));
            xml.WriteAttributeString("ShiftDuration", B.Subtract(A).TotalMinutes.ToString("######"));
            xml.WriteAttributeString("ShiftDurationVerbose", Math.Truncate(B.Subtract(A).TotalHours).ToString("####") + ":" + B.Subtract(A).Minutes.ToString("D"));

            xml.WriteEndElement();

                xml.WriteStartElement("SingleShiftEvents");
                xml.WriteAttributeString("ShiftID", shiftcounter.ToString("D"));

            foreach (var s in ss)
            {

                xml.WriteStartElement("Event");
                xml.WriteAttributeString("StartingDateTime", s.StartingDateTime.ToString("f"));
                xml.WriteAttributeString("EndingDateTime", s.EndingDateTime.ToString("f"));
                xml.WriteAttributeString("Duration", s.Duration.ToString("D"));
                xml.WriteAttributeString("ActivityType", s.ActivityType);
                xml.WriteAttributeString("DriverCardStatus", s.DriverCardStatus);
                xml.WriteAttributeString("DrivingStatus", s.DrivingStatus);
                xml.WriteAttributeString("SlotStatus", s.SlotStatus);
                xml.WriteEndElement();
                xml.Flush();
            }
                xml.WriteEndElement();
        }
    }

    internal struct Control
    {
        /*
                private void DailyShiftWorkControl(List<EventInfoRoute> eir, List<FinesRecord> outputList)
                {
                    bool shiftStart = false;
                    bool shiftEnd = false;
                    bool dailyRestStart = false;
                    bool dailyRestEnd = false;
                    bool dailyRestFull = false;
                    bool dailyRestIncomplete = false;

                    int shiftRestCounter = 0;
                    int activityNow = 0;
                    EventInfoRoute eiritem = new EventInfoRoute();
                    FinesRecord FR = new FinesRecord();

                    foreach (var e in eir)
                    {
                        FR.DateTime = e.EventStart;
                        FR.Duration = e.DurationOfEventInMinutes;

                        if (e.EventType=="BREAK/REST")
                        {
                            shiftRestCounter += e.DurationOfEventInMinutes;
                        }
                    }
                }

                private void DailyShiftRestControl(List<EventInfoRoute> eir, List<FinesRecord> outputList)
                {
                    throw new NotImplementedException();
                }

                private void WeeklyShiftWorkControl(List<EventInfoRoute> eir, List<FinesRecord> outputList)
                {
                    throw new NotImplementedException();
                }

                private void WeeklyShiftRestControl(List<EventInfoRoute> eir, List<FinesRecord> outputList)
                {
                    throw new NotImplementedException();
                }
        */

        public List<ActivityChangeInfoExtended> ConvertArrayToExtendedList(CardActivityDailyRecord[] cadr)
        {
            //Input must be an array, reduced to the required interval
            List<ActivityChangeInfoExtended> acie_array = new List<ActivityChangeInfoExtended>();
            ActivityChangeInfoExtended acie_array_elem;
            //acie_array = new ActivityChangeInfoExtended[cadr.Length];
            int i = 0;
            int counterForActivityChanges = 0;
            foreach (var f in cadr)
            {
                i++;
                var thisdate = f.activityRecordDate.ConvertToDateTime();
                if (f.activityChangeInfo != null)
                {
                    for (int j = 0; j < f.activityChangeInfo.Length; j++)
                    {
                        counterForActivityChanges++;
                        var aci = f.activityChangeInfo[j];
                        var endtime = (j + 2 > f.activityChangeInfo.Length)
                            ? 1440
                            : f.activityChangeInfo[j + 1].minutesSinceMidnight;

                        acie_array_elem = new ActivityChangeInfoExtended();
                        acie_array_elem.StartingDateTime = thisdate;
                        acie_array_elem.StartingDateTime =
                            acie_array_elem.StartingDateTime.AddMinutes(aci.minutesSinceMidnight);
                        acie_array_elem.EndingDateTime = thisdate;
                        acie_array_elem.EndingDateTime =
                            acie_array_elem.EndingDateTime.AddMinutes(endtime);
                        acie_array_elem.Duration = endtime - aci.minutesSinceMidnight;
                        acie_array_elem.ActivityType = aci.getVerboseActivity();
                        acie_array_elem.DriverCardStatus = aci.getVerboseDriverCardStatus();
                        acie_array_elem.DrivingStatus = aci.getVerboseDrivingStatus();
                        acie_array_elem.SlotStatus = aci.getVerboseSlotStatus();

                        acie_array.Add(acie_array_elem);
                    }
                }
            }

            return acie_array;
        }

        public List<ActivityChangeInfoExtended> GetOneShift(List<ActivityChangeInfoExtended> acie_array)
        {
            var singleShift = new List<ActivityChangeInfoExtended>();
            var i = 0;
//while (i < acie_array.Count - 1 && acie_array[i].ActivityType == "BREAK/REST") i++;
            var scoreForCrew = 0;

            #region CrewIdentification

            if (acie_array[i].DrivingStatus == "CREW") scoreForCrew++;
            else scoreForCrew--;
            if (acie_array.Count > 1)
                if (acie_array[i + 1].DrivingStatus == "CREW") scoreForCrew++;
                else scoreForCrew--;
            if (acie_array.Count > 2)
                if (acie_array[i + 2].DrivingStatus == "CREW") scoreForCrew++;
                else scoreForCrew--;
            if (acie_array.Count > 3)
                if (acie_array[i + 3].DrivingStatus == "CREW") scoreForCrew++;
                else scoreForCrew--;
            if (acie_array.Count > 4)
                if (acie_array[i + 4].DrivingStatus == "CREW") scoreForCrew++;
                else scoreForCrew--;
            if (acie_array.Count > 5)
                if (acie_array[i + 5].DrivingStatus == "CREW") scoreForCrew++;
                else scoreForCrew--;
            if (acie_array.Count > 6)
                if (acie_array[i + 6].DrivingStatus == "CREW") scoreForCrew++;
                else scoreForCrew--;

            #endregion CrewIdentification

            var startingCarretPosition = i;
            var breaktimesum = 0;
            int FullRestArrayPos = -1;
            int ReducedRestArrayPos = -1;
            int _3hRestPos = -1;
            int _9hRestPos = -1;

            if (scoreForCrew > 0)
            {
                while (i <= acie_array.Count - 1 && acie_array[i].EndingDateTime
                           .Subtract(acie_array[startingCarretPosition].StartingDateTime)
                           .TotalMinutes <= 1800)
                {
                    singleShift.Add(acie_array[i]);
                    //алгоритм начинается тут

                    #region alfa

                    /* if (acie_array[i].ActivityType == "BREAK/REST")
                    if (acie_array[i].Duration > 540 || breaktimesum > 540)
                        {
                            startingCarretPosition = i;
                            breaktimesum = 0;
                            break;
                        }
                        else if (acie_array[i].Duration > 120)
                        {
                            breaktimesum += acie_array[i].Duration;
                            if (i < acie_array.Count())
                                if (acie_array[i + 1].ActivityType == "BREAK/REST" && acie_array[i + 1].Duration > 120)
                                {
                                    breaktimesum += acie_array[i + 1].Duration;
                                    singleShift.Add(acie_array[i + 1]);
                                    if (breaktimesum < 540)
                                        if (i < acie_array.Count() - 1)
                                            if (acie_array[i + 2].ActivityType == "BREAK/REST" &&
                                                acie_array[i + 2].Duration > 120)
                                            {
                                                breaktimesum += acie_array[i + 2].Duration;
                                                singleShift.Add(acie_array[i + 2]);
                                                if (breaktimesum > 540)
                                                {
                                                    startingCarretPosition = i + 2;
                                                    breaktimesum = 0;
                                                    break;
                                                }
                                                i++;
                                            }
                                    i++;
                                }
                        }

                    i++;*/

                    #endregion alfa

                    singleShift.Add(acie_array[i]);
                    if (acie_array[i].ActivityType == "BREAK/REST")
                        if (acie_array[i].Duration >= 660)
                        {
                            startingCarretPosition = i;
                            FullRestArrayPos = i;
                            break;
                        }
                        else if (acie_array[i].Duration >= 540)
                        {
                            ReducedRestArrayPos = i;
                            break;
                        }
                        else
                        {
                            if (acie_array[i].Duration >= 180)
                            {
                                _3hRestPos = i;
                                if (i < acie_array.Count())
                                {
                                    if (acie_array[i + 1].ActivityType == "BREAK/REST" && (acie_array[i].Duration + acie_array[i + 1].Duration <= 1800))
                                    {
                                        if (acie_array[i + 1].Duration >= 540)
                                        {
                                            FullRestArrayPos = i + 1;
                                            _9hRestPos = i + 1;
                                            break;
                                        }
                                        if (i + 1 < acie_array.Count())
                                        {
                                            if (acie_array[i + 2].ActivityType == "BREAK/REST" &&
                                                (acie_array[i].Duration + acie_array[i + 1].Duration + acie_array[i + 2].Duration <= 1800))
                                            {
                                                if (acie_array[i + 2].Duration >= 540)
                                                {
                                                    FullRestArrayPos = i + 2;
                                                    _9hRestPos = i + 2;
                                                    break;
                                                }
                                                if (i + 2 < acie_array.Count())
                                                {
                                                    if (acie_array[i + 3].ActivityType == "BREAK/REST" &&
                                                        (acie_array[i].Duration + acie_array[i + 1].Duration +
                                                         acie_array[i + 2].Duration + acie_array[i + 3].Duration <=
                                                         1800))
                                                    {
                                                        if (acie_array[i + 2].Duration >= 540)
                                                        {
                                                            FullRestArrayPos = i + 2;
                                                            _9hRestPos = i + 2;
                                                            break;
                                                        }
                                                    }
                                                    i++;
                                                }
                                            }
                                            i++;
                                        }
                                    }
                                    i++;
                                }
                            }
                        }

                    i++;
                }
                return singleShift;
            }

            while (i <= acie_array.Count - 1 && acie_array[i].EndingDateTime
                       .Subtract(acie_array[startingCarretPosition].StartingDateTime)
                       .TotalMinutes <= 1440)
            {
                singleShift.Add(acie_array[i]);
                //алгоритм начинается тут
                if (acie_array[i].Duration < 2)
                {
                    i++;
                    continue;
                }
                if (acie_array[i].ActivityType == "BREAK/REST")
                    if (acie_array[i].Duration >= 660)
                    {
                        startingCarretPosition = i;
                        FullRestArrayPos = i;
                        break;
                    }
                    else if (acie_array[i].Duration >= 540)
                    {
                        ReducedRestArrayPos = i;
                        break;
                    }
                    else
                    {
                        if (acie_array[i].Duration >= 180)
                        {
                            _3hRestPos = i;
                            if (i < acie_array.Count())
                            {
                                if (acie_array[i + 1].ActivityType == "BREAK/REST" && (acie_array[i].Duration + acie_array[i + 1].Duration <= 1440))
                                {
                                    if (acie_array[i + 1].Duration >= 540)
                                    {
                                        FullRestArrayPos = i + 1;
                                        _9hRestPos = i + 1;
                                        break;
                                    }
                                    if (i + 1 < acie_array.Count())
                                    {
                                        if (acie_array[i + 2].ActivityType == "BREAK/REST" &&
                                            (acie_array[i].Duration + acie_array[i + 1].Duration + acie_array[i + 2].Duration <= 1440))
                                        {
                                            if (acie_array[i + 2].Duration >= 540)
                                            {
                                                FullRestArrayPos = i + 2;
                                                _9hRestPos = i + 2;
                                                break;
                                            }
                                            if (i + 2 < acie_array.Count())
                                            {
                                                if (acie_array[i + 3].ActivityType == "BREAK/REST" &&
                                                    (acie_array[i].Duration + acie_array[i + 1].Duration +
                                                     acie_array[i + 2].Duration + acie_array[i + 3].Duration <=
                                                     1440))
                                                {
                                                    if (acie_array[i + 2].Duration >= 540)
                                                    {
                                                        FullRestArrayPos = i + 2;
                                                        _9hRestPos = i + 2;
                                                        break;
                                                    }
                                                }
                                                i++;
                                            }
                                        }
                                        i++;
                                    }
                                }
                                i++;
                            }
                        }
                    }

                i++;
            }

            #region beta

            /* while (i <= acie_array.Count - 1 && acie_array[i].StartingDateTime
                        .Subtract(acie_array[startingCarretPosition].StartingDateTime)
                        .TotalMinutes < 1440)
             {
                 singleShift.Add(acie_array[i]);
                 if (acie_array[i].ActivityType == "BREAK/REST")
                     if (acie_array[i].Duration > 540 || breaktimesum > 540)
                     {
                         startingCarretPosition = i;
                         breaktimesum = 0;
                         break;
                     }
                     else if (acie_array[i].Duration > 120)
                     {
                         breaktimesum += acie_array[i].Duration;
                         if (i < acie_array.Count())
                             if (acie_array[i + 1].ActivityType == "BREAK/REST" && acie_array[i + 1].Duration > 120)
                             {
                                 breaktimesum += acie_array[i + 1].Duration;
                                 singleShift.Add(acie_array[i + 1]);
                                 if (breaktimesum < 540)
                                     if (i < acie_array.Count() - 1)
                                         if (acie_array[i + 2].ActivityType == "BREAK/REST" &&
                                             acie_array[i + 2].Duration > 120)
                                         {
                                             breaktimesum += acie_array[i + 2].Duration;
                                             singleShift.Add(acie_array[i + 2]);
                                             if (breaktimesum > 540)
                                             {
                                                 startingCarretPosition = i + 2;
                                                 breaktimesum = 0;
                                                 break;
                                             }
                                             i++;
                                         }
                                 i++;
                             }
                     }

                 i++;
             }*/

            #endregion beta

            if (i < acie_array.Count)
            {
                if (acie_array[i].EndingDateTime
                        .Subtract(acie_array[startingCarretPosition].StartingDateTime)
                        .TotalMinutes > 1440)
                {
                    singleShift.Add(acie_array[i]);
                    ActivityChangeInfoExtended A = new ActivityChangeInfoExtended();
                    A = acie_array[i];
                    ActivityChangeInfoExtended B = new ActivityChangeInfoExtended();
                    B = acie_array[i];
                    int C = (int)acie_array[i].EndingDateTime
                                .Subtract(acie_array[startingCarretPosition].StartingDateTime)
                                .TotalMinutes - 1440;
                    A.Duration = 0;
                    A.EndingDateTime = A.EndingDateTime.Subtract(new TimeSpan(0, C, 0));

                    B.Duration = C;
                    B.StartingDateTime = A.EndingDateTime.Add(new TimeSpan(0, 1, 0));
                    singleShift[singleShift.Count-1] = A;
                    acie_array.Insert(i+1,B);
                    
                    return singleShift;
                }
            }
            return singleShift;
        }

        public ShiftStats GetStatsForSingleShift(List<ActivityChangeInfoExtended> acie, int shiftID)
        {
            ShiftStats ss = new ShiftStats();

            ss.shiftID = shiftID;

            ViolationRecord svtemp = new ViolationRecord();

            int i = -1;
            if (acie.Count != 0)
                foreach (var ac in acie)
                {
                    i++;
                    if (ac.DriverCardStatus == "NOT INSERTED" || ac.ActivityType == null) ac.ActivityType = "BREAK/REST";
                    switch (ac.ActivityType)
                    {
                        case "DRIVING":
                        case "AVAILABILITY":
                        case "WORKING":
                            {
                                ss.workingMinCounter += ac.Duration;
                                if (ss.workingMinCounter <= 270 && ss.dailyBreakMinCounter >= 15 && ss.shiftBreakMinCounter == 0 &&
                                    !ss.dailyBreakComplete && !ss.shiftBreakComplete)
                                {
                                    ss.dailyBreakComplete = true;
                                }
                                else if (ss.workingMinCounter > 270)
                                {
                                    ss.dailyBreakTimeLimitExceeded = true;

                                    svtemp.ViolationDateTime = ac.EndingDateTime;
                                    svtemp.amount = ss.workingMinCounter - 270;
                                    svtemp.VIOLATION_TYPE = "LongDrivingWithoutABreak";
                                    svtemp.ViolationText = "Driving time without short break exceeded for " +
                                                           svtemp.amount + " minutes.";
                                    ss.sv.Add(svtemp);

                                    svtemp = new ViolationRecord();
                                }
                                break;
                            }
                        case "BREAK/REST":
                            {
                                if (ac.Duration > 5)
                                {
                                    if (ac.Duration >= 540)
                                    {
                                        ss.shiftBreakMinCounter += ac.Duration;
                                        ss.shiftBreakComplete = true;

                                        if (ss.dailyBreakMinCounter < 45)
                                        {
                                            ss.dailyBreakTimeReduced = true;

                                            svtemp.ViolationDateTime = ac.EndingDateTime;
                                            svtemp.amount = 45 - ss.dailyBreakMinCounter;
                                            svtemp.VIOLATION_TYPE = "NotEnoughRestDuringTheDay";
                                            svtemp.ViolationText = "Short daily break was reduced for " +
                                                                   svtemp.amount + " minutes.";
                                            ss.sv.Add(svtemp);

                                            svtemp = new ViolationRecord();
                                        }
                                        if (ss.workingMinCounter >= 540 && ss.workingMinCounter <= 600)
                                            ss.dailyDrivingTimeExceeded_Warning = true;
                                        else if (ss.workingMinCounter > 600)
                                        {
                                            ss.dailyDrivingTimeExceeded_Fault = true;

                                            svtemp.ViolationDateTime = ac.EndingDateTime;
                                            svtemp.amount = ss.dailyBreakMinCounter - 600;
                                            svtemp.VIOLATION_TYPE = "DailyDrivingTimeExceeded";
                                            svtemp.ViolationText = "Daily driving time was exceeded for " +
                                                                   svtemp.amount + " minutes.";
                                            ss.sv.Add(svtemp);

                                            svtemp = new ViolationRecord();
                                        }
                                    }
                                    else if (ss.dailyBreakMinCounter > 585)
                                    {
                                        ss.shiftBreakMinCounter += ss.dailyBreakMinCounter;
                                        ss.shiftBreakComplete = true;
                                        ss.dailyBreakComplete = true;
                                        break;
                                    }
                                    if (ss.shiftBreakMinCounter == 0) ss.dailyBreakMinCounter += ac.Duration;
                                }
                                else
                                {
                                    ss.workingMinCounter += ac.Duration;
                                }
                                break;
                            }
                    }
                    if (ss.shiftBreakComplete) break;
                }
            if (!ss.shiftBreakComplete && acie.Count > 0)
            {
                svtemp.ViolationDateTime = acie.Last().EndingDateTime;
                svtemp.amount = 540 - ss.shiftBreakMinCounter;
                svtemp.VIOLATION_TYPE = "DailyRestWasIncomplete";
                svtemp.ViolationText = "Daily break time was reduced for " +
                                       svtemp.amount + " minutes.";
                ss.sv.Add(svtemp);
            }

            return ss;
        }

        /*        public void GetStatsForCrewShift(List<ActivityChangeInfoExtended> acie)
                {
                    int workingMinCounter = 0;
                    int dailyBreakMinCounter = 0;
                    int shiftBreakMinCounter = 0;
                    bool dailyBreakComplete = false;
                    bool dailyBreakTimeReduced = false;
                    bool dailyBreakTimeLimitExceeded = false;
                    bool shiftBreakComplete = false;
                    bool dailyDrivingTimeExceeded_Fault = false;
                    bool dailyDrivingTimeExceeded_Warning = false;
                    int i = -1;

                    foreach (var ac in acie)
                    {
                        i++;
                        switch (ac.ActivityType)
                        {
                            case "DRIVING":
                            case "AVAILABILITY":
                            case "WORKING":
                            {
                                workingMinCounter += ac.Duration;
                                if (workingMinCounter <= 270 && dailyBreakMinCounter >= 15 && shiftBreakMinCounter == 0 &&
                                    !dailyBreakComplete && !shiftBreakComplete)
                                {
                                    dailyBreakComplete = true;
                                }
                                else if (workingMinCounter > 270) dailyBreakTimeLimitExceeded = true;
                                break;
                            }
                            case "BREAK/REST":
                            {
                                if (ac.Duration > 5)
                                {
                                    if (ac.Duration >= 540)
                                    {
                                        shiftBreakMinCounter += ac.Duration;
                                        shiftBreakComplete = true;
                                        if (dailyBreakMinCounter < 45) dailyBreakTimeReduced = true;
                                        if (workingMinCounter >= 540 && workingMinCounter <= 600)
                                            dailyDrivingTimeExceeded_Warning = true;
                                        else if (workingMinCounter > 600) dailyDrivingTimeExceeded_Fault = true;
                                    }
                                    if (shiftBreakMinCounter == 0) dailyBreakMinCounter += ac.Duration;
                                }
                                else
                                {
                                    workingMinCounter += ac.Duration;
                                }
                                break;
                            }
                        }
                    }
                }*/
    }
}