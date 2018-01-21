using System;
using System.Collections.Generic;
using System.Xml;
using static DDDReader_0._4._0_renewed.AnyAdditionalAndSupportiveFunctions;

namespace DDDReader_0._4._0_renewed
{
    /* IMPORTANT NOTE
     * ************************************************ *
     * Please, note. All data types were defined as pre *
     * sented in 561/2006 Protocol. This project consis *
     * ts of Basic Types definition and Complex Types d *
     * efinitions. Also they some functions inside.     *
     * ************************************************ *
     */
    internal class DeclarationOfClassesUsed
    {
        #region DataTypes
        //!!!DATA TYPES!!!
        //****************
        
        //****************
        //!!!DATA TYPES!!!

        internal class ActivityChangeInfo
        {
            public string activityType;
            public string binaryString;
            public int driverCardStatus;
            public int drivingStatus;
            public int minutesSinceMidnight;
            public byte[] RAW = new byte[2];
            public int slotStatus;

            public string getVerboseActivity()
            {
                switch (activityType)
                {
                    case "00":
                        return "BREAK/REST";

                    case "01":
                        return "AVAILABILITY";

                    case "10":
                        return "WORK";

                    case "11":
                        return "DRIVING";

                    default:
                        return null;
                }
            }

            public string getVerboseDriverCardStatus()
            {
                switch (driverCardStatus)
                {
                    case 0:
                        return "INSERTED";

                    case 1:
                        return "NOT INSERTED";

                    default:
                        return null;
                }
            }

            public string getVerboseDrivingStatus()
            {
                if (driverCardStatus == 0)
                    switch (drivingStatus)
                    {
                        case 0:
                            return "SINGLE";

                        case 1:
                            return "CREW";

                        default:
                            return null;
                    }
                if (driverCardStatus == 1)
                    switch (drivingStatus)
                    {
                        case 0:
                            return "UNKNOWN";

                        case 1:
                            return "KNOWN(manual)";

                        default:
                            return null;
                    }
                return null;
            }

            public string getVerboseSlotStatus()
            {
                switch (slotStatus)
                {
                    case 0:
                        return "DRIVER";

                    case 1:
                        return "CO-DRIVER";

                    default:
                        return null;
                }
            }

            public string getVerboseTime()
            {
                var n = TimeSpan.FromMinutes(minutesSinceMidnight);
                return n.Hours.ToString("D2") + ":" + n.Minutes.ToString("D2");
            }

            public void ParseIT()
            {
                binaryString = Convert.ToString(RAW[0], 2).PadLeft(8, '0') +
                    Convert.ToString(RAW[1], 2).PadLeft(8, '0');
                slotStatus = (int)Char.GetNumericValue(binaryString[0]);
                drivingStatus = (int)Char.GetNumericValue(binaryString[1]);
                driverCardStatus = (int)Char.GetNumericValue(binaryString[2]);
                activityType = binaryString.Substring(3, 2);
                minutesSinceMidnight = Convert.ToInt32(binaryString.Substring(5, 11), 2);
            }
        }

        internal class Address
        {
            public string address { get; set; }
            public int codePage { get; set; }
        }

        internal class BCDDate
        {
            private byte[] encodedDate = new byte[4];

            public byte[] EncodedDate
            {
                get { return encodedDate; }
                set { encodedDate = value; }
            }

            private ByteConvert bc = new ByteConvert();

            public override string ToString()
            {
                return encodedDate[0].ToString("X2") + encodedDate[1].ToString("X2") + "-" +
                encodedDate[2].ToString("X2") + "-" + encodedDate[3].ToString("X2");
            }
        }

        internal class BcdMonthYear
        {
            private byte[] encodedDate = new byte[2];

            public byte[] EncodedDate
            {
                get { return encodedDate; }
                set { encodedDate = value; }
            }

            private ByteConvert bc = new ByteConvert();

            public override string ToString()
            {
                return "20" + encodedDate[1].ToString("X") + " - " + encodedDate[0].ToString("X");
            }
        }

        internal class CardChipIdentification
        {
            public byte[] icManufacturingReference = new byte[4];
            public byte[] icSerialNumber = new byte[4];
        }

        internal class CardConsecutiveIndex
        {
            public char index { get; set; }
        }

        internal class CalibrationPurpose
        {
            public int calibrationPurpose;

            public string calibrationPurposeIdentification()
            {
                switch (calibrationPurpose)
                {
                    case 0:
                        return "Reserved";

                    case 1:
                        return "Activation";

                    case 2:
                        return "First installation after activation";

                    case 3:
                        return "First installation in this vehicle";

                    case 4:
                        return "Periodic inspection";

                    default:
                        return null;
                }
            }
        }

        internal class CardRenewalIndex
        {
            public char cardRenewalIndex { get; set; }
        }

        internal class CardReplacementIndex
        {
            public char cardReplacementIndex { get; set; }
        }

        internal class CardSlotsStatus
        {
            public string CoDriverSlot { get; set; }
            public string DriverSlot { get; set; }

            public string getVerbose(string inp)
            {
                switch (inp)
                {
                    case "0000":
                        return "no card";
                    case "0001":
                        return "a driver card is inserted";
                    case "0010":
                        return "a workshop card is inserted";
                    case "0011":
                        return "a control card is inserted";
                    case "0100":
                        return "a company card is iserted";
                    default:
                        return null;
                }
            }
        }

        internal class CompanyActivityType
        {
            public int companyActivityType { get; set; }

            public string getVerboseActivityType()
            {
                switch (companyActivityType)
                {
                    case 1:
                        return "card downloading";
                    case 2:
                        return "VU downloading";
                    case 3:
                        return "VU lock-in";
                    case 4:
                        return "VU lock-out";
                    default:
                        return null;
                }
            }
        }

        internal class ControlType
        {
            public string controlType { get; set; }

            public string getVerboseControlType()
            {
                string s = controlType[0] == '1'
                    ? "Card downloaded during this control activity. "
                    : "Card not downloaded during this control activity. ";

                s += controlType[1] == '1'
                    ? "VU downloaded during this control activity. "
                    : "VU not downloaded during this control activity. ";

                s += controlType[2] == '1'
                    ? "Printing done during this control activity. "
                    : "No pringting done during this control activity. ";

                s += controlType[3] == '1'
                    ? "Display used during this control activity. "
                    : "No display used during this control activity. ";

                return s;
            }
        }

        internal class Distance
        {
            public int distance;
        }

        internal class EntryTypeDailyWorkPeriod
        {
            public int entryTypeDailyWorkPeriod { get; set; }

            public string getVerboseEntryType()
            {
                switch (entryTypeDailyWorkPeriod)
                {
                    case 0:
                        return "Begin, related time=card insertion time or time of entry";
                    case 1:
                        return "End, related time=card withdrawal time or time of entry";
                    case 2:
                        return "Begin, related time manually entered (start time)";
                    case 3:
                        return "End, related time manually entered (end of work period)";
                    case 4:
                        return "Begin, related time assumed by VU";
                    case 5:
                        return "End, related time assumed by VU";
                    default:
                        return null;
                }
            }
        }

        internal class EquipmentType
        {
            public int equipmentType { get; set; }

            public string getVerboseType()
            {
                switch (equipmentType)
                {
                    case 0:
                        return "Reserved";
                    case 1:
                        return "Driver Card";
                    case 2:
                        return "Workshop Card";
                    case 3:
                        return "Control Card";
                    case 4:
                        return "Company Card";
                    case 5:
                        return "Manufacturing Card";
                    case 6:
                        return "Vehicle Unit";
                    case 7:
                        return "Motion Sensor";
                    default:
                        return "RFU";
                }
            }
        }

        internal class EventFaultRecordPurpose
        {
            public byte eventFaultRecordPurpose { get; set; }

            public string getVerbosePurpose()
            {
                if ((eventFaultRecordPurpose >= 0x80) & (eventFaultRecordPurpose <= 0xFF))
                    return "Manufacturer specific";
                switch (eventFaultRecordPurpose)
                {
                    case 0x00:
                        return "One of the 10 most recent (or last) event or faults";
                    case 0x01:
                        return "The longest event for one of the last 10 days of occurrence";
                    case 0x02:
                        return "One of the 5 longest events over the last 365 days";
                    case 0x03:
                        return "The last event for one of the last 10 days of occurrence";
                    case 0x04:
                        return "The most serious event for one of the last 10 days of occurence";
                    case 0x05:
                        return "One of the 5 most serious events over the last 365 days";
                    case 0x06:
                        return "The first event or fault having occurred after the last calibration";
                    case 0x07:
                        return "an active/on-going event or fault";
                    default:
                        return "RFU";
                }
            }
        }

        internal class EventFaultType
        {
            public byte eventFaultType { get; set; }

            public string getVerboseType()
            {
                if (eventFaultType >= 0x80 && eventFaultType <= 0xFF)
                    return "Manufacturer specific";
                switch (eventFaultType)
                {
                    case 0x00:
                        return "General events. No further details";
                    case 0x01:
                        return "General events: Insertion of a non-valid card";
                    case 0x02:
                        return "General events: Card conflict";
                    case 0x03:
                        return "General events: Time overlap";
                    case 0x04:
                        return "General events: Driving without an appropriate card";
                    case 0x05:
                        return "General events: Card insertion while driving";
                    case 0x06:
                        return "General events: Last card session not correctly closed";
                    case 0x07:
                        return "General events: Overspeeding";
                    case 0x08:
                        return "General events: Power supply interruption";
                    case 0x09:
                        return "General events: Motion data error";
                    case 0x10:
                        return "Vehicle unit related security breach attempt events. No further details";
                    case 0x11:
                        return "Vehicle unit related Security breach attempt: Motion sensor authentication failure";
                    case 0x12:
                        return "Vehicle unit related Security breach attempt: Tachograph card authentication failure";
                    case 0x13:
                        return "Vehicle unit related Security breach attempt: Unauthorized change of motion sensor";
                    case 0x14:
                        return "Vehicle unit related Security breach attempt: Card data input integrity error";
                    case 0x15:
                        return "Vehicle unit related Security breach attempt: Stored user data integrity error";
                    case 0x16:
                        return "Vehicle unit related Security breach attempt: Internal data transfer error";
                    case 0x17:
                        return "Vehicle unit related Security breach attempt: Unauthorized case opening";
                    case 0x18:
                        return "Vehicle unit related Security breach attempt: Hardware sabotage";
                    case 0x20:
                        return "Sensor related security breach attempt event. No further details";
                    case 0x21:
                        return "Sensor related security breach attempt: Authentication failure";
                    case 0x22:
                        return "Sensor related security breach attempt: Stored data integrity error";
                    case 0x23:
                        return "Sensor related security breach attempt: Internal data transfer error";
                    case 0x24:
                        return "Sensor related security breach attempt: Unauthorized case opening";
                    case 0x25:
                        return "Sensor related security breach attempt: Hardware sabotage";
                    case 0x30:
                        return "Recording equipment faults. No further details";
                    case 0x31:
                        return "Recording equipment faults: VU internal fault";
                    case 0x32:
                        return "Recording equipment faults: Printer fault";
                    case 0x33:
                        return "Recording equipment faults: Display fault";
                    case 0x34:
                        return "Recording equipment faults: Downloading fault";
                    case 0x35:
                        return "Recording equipment faults: Sensor fault";
                    case 0x40:
                        return "Card faults. No further details";
                    default:
                        return "RFU";
                }
            }
        }

        internal class Language
        {
            private char[] language = new char[1];

            public char[] PLanguage
            {
                get { return language; }
                set { language = value; }
            }
        }

        internal class ManualInputFlag
        {
            public int manualInputFlag { get; set; }

            public string getVerboseFlag()
            {
                switch (manualInputFlag)
                {
                    case 0:
                        return "noEntry";
                    case 1:
                        return "manualEntry";
                    default:
                        return null;
                }
            }
        }

        internal class ManufacturerCode
        {
            public byte manufacturerCode { get; set; }

            public string getVerboseCode()
            {
                switch (manufacturerCode)
                {
                    case 0x00:
                        return "No information available";
                    case 0x01:
                        return "Reserved value";
                    case 0x02:
                    case 0x03:
                    case 0x04:
                    case 0x05:
                    case 0x06:
                    case 0x07:
                    case 0x08:
                    case 0x09:
                    case 0x0A:
                    case 0x0B:
                    case 0x0C:
                    case 0x0D:
                    case 0x0E:
                    case 0x0F:
                        return "Reserved for Future Use";
                    case 0x10:
                        return "ACTIA";
                    case 0x11:
                    case 0x12:
                    case 0x13:
                    case 0x14:
                    case 0x15:
                    case 0x16:
                    case 0x17:
                        return "Reserved for manufacturers which name begins with 'A'";
                    case 0x18:
                    case 0x19:
                    case 0x1a:
                    case 0x1b:
                    case 0x1c:
                    case 0x1d:
                    case 0x1e:
                    case 0x1f:
                        return "Reserved for manufacturers which name begins with 'B'";
                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x25:
                    case 0x26:
                    case 0x27:
                        return "Reserved for manufacturers which name begins with 'C'";
                    case 0x28:
                    case 0x29:
                    case 0x2a:
                    case 0x2b:
                    case 0x2c:
                    case 0x2d:
                    case 0x2e:
                    case 0x2f:
                        return "Reserved for manufacturers which name begins with 'D'";
                    case 0x30:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                    case 0x34:
                    case 0x35:
                    case 0x36:
                    case 0x37:
                        return "Reserved for manufacturers which name begins with 'E'";
                    case 0x38:
                    case 0x39:
                    case 0x3a:
                    case 0x3b:
                    case 0x3c:
                    case 0x3d:
                    case 0x3e:
                    case 0x3f:
                        return "Reserved for manufacturers which name begins with 'F'";
                    case 0x40:
                        return "Giesecke & Devrient GmbH";
                    case 0x41:
                        return "GEM plus";
                    case 0x42:
                    case 0x43:
                    case 0x44:
                    case 0x45:
                    case 0x46:
                    case 0x47:
                        return "Reserved for manufacturers which name begins with 'G'";
                    case 0x48:
                    case 0x49:
                    case 0x4a:
                    case 0x4b:
                    case 0x4c:
                    case 0x4d:
                    case 0x4e:
                    case 0x4f:
                        return "Reserved for manufacturers which name begins with 'H'";
                    case 0x50:
                    case 0x51:
                    case 0x52:
                    case 0x53:
                    case 0x54:
                    case 0x55:
                    case 0x56:
                    case 0x57:
                        return "Reserved for manufacturers which name begins with 'I'";
                    case 0x58:
                    case 0x59:
                    case 0x5a:
                    case 0x5b:
                    case 0x5c:
                    case 0x5d:
                    case 0x5e:
                    case 0x5f:
                        return "Reserved for manufacturers which name begins with 'J'";
                    case 0x60:
                    case 0x61:
                    case 0x62:
                    case 0x63:
                    case 0x64:
                    case 0x65:
                    case 0x66:
                    case 0x67:
                        return "Reserved for manufacturers which name begins with 'K'";
                    case 0x68:
                    case 0x69:
                    case 0x6a:
                    case 0x6b:
                    case 0x6c:
                    case 0x6d:
                    case 0x6e:
                    case 0x6f:
                        return "Reserved for manufacturers which name begins with 'L'";
                    case 0x70:
                    case 0x71:
                    case 0x72:
                    case 0x73:
                    case 0x74:
                    case 0x75:
                    case 0x76:
                    case 0x77:
                        return "Reserved for manufacturers which name begins with 'M'";
                    case 0x78:
                    case 0x79:
                    case 0x7a:
                    case 0x7b:
                    case 0x7c:
                    case 0x7d:
                    case 0x7e:
                    case 0x7f:
                        return "Reserved for manufacturers which name begins with 'N'";
                    case 0x80:
                        return "OSCARD";
                    case 0x81:
                    case 0x82:
                    case 0x83:
                    case 0x84:
                    case 0x85:
                    case 0x86:
                    case 0x87:
                        return "Reserved for manufacturers which name begins with 'O'";
                    case 0x88:
                    case 0x89:
                    case 0x8a:
                    case 0x8b:
                    case 0x8c:
                    case 0x8d:
                    case 0x8e:
                    case 0x8f:
                        return "Reserved for manufacturers which name begins with 'P'";
                    case 0x90:
                    case 0x91:
                    case 0x92:
                    case 0x93:
                    case 0x94:
                    case 0x95:
                    case 0x96:
                    case 0x97:
                        return "Reserved for manufacturers which name begins with 'Q'";
                    case 0x98:
                    case 0x99:
                    case 0x9a:
                    case 0x9b:
                    case 0x9c:
                    case 0x9d:
                    case 0x9e:
                    case 0x9f:
                        return "Reserved for manufacturers which name begins with 'R'";
                    case 0xa0:
                        return "SETEC";
                    case 0xa1:
                        return "SIEMENS VDO";
                    case 0xa2:
                        return "STONERIDGE";
                    case 0xa3:
                    case 0xa4:
                    case 0xa5:
                    case 0xa6:
                    case 0xa7:
                    case 0xa8:
                    case 0xa9:
                        return "Reserved for manufacturers which name begins with 'S'";
                    case 0xaa:
                        return "TACHOCONTROL";
                    case 0xab:
                    case 0xac:
                    case 0xad:
                    case 0xae:
                    case 0xaf:
                        return "Reserved for manufacturers which name begins with 'T'";
                    case 0xb0:
                    case 0xb1:
                    case 0xb2:
                    case 0xb3:
                    case 0xb4:
                    case 0xb5:
                    case 0xb6:
                    case 0xb7:
                        return "Reserved for manufacturers which name begins with 'U'";
                    case 0xb8:
                    case 0xb9:
                    case 0xba:
                    case 0xbb:
                    case 0xbc:
                    case 0xbd:
                    case 0xbe:
                    case 0xbf:
                        return "Reserved for manufacturers which name begins with 'V'";
                    case 0xc0:
                    case 0xc1:
                    case 0xc2:
                    case 0xc3:
                    case 0xc4:
                    case 0xc5:
                    case 0xc6:
                    case 0xc7:
                        return "Reserved for manufacturers which name begins with 'W'";
                    case 0xc8:
                    case 0xc9:
                    case 0xca:
                    case 0xcb:
                    case 0xcc:
                    case 0xcd:
                    case 0xce:
                    case 0xcf:
                        return "Reserved for manufacturers which name begins with 'X'";
                    case 0xd0:
                    case 0xd1:
                    case 0xd2:
                    case 0xd3:
                    case 0xd4:
                    case 0xd5:
                    case 0xd6:
                    case 0xd7:
                        return "Reserved for manufacturers which name begins with 'Y'";
                    case 0xd8:
                    case 0xd9:
                    case 0xda:
                    case 0xdb:
                    case 0xdc:
                    case 0xdd:
                    case 0xde:
                    case 0xdf:
                        return "Reserved for manufacturers which name begins with 'Z'";
                    default:
                        return null;
                }
            }
        }

        internal class Name
        {
            public int codePage { get; set; }
            public string name { get; set; }
        }

        internal class NationAlpha
        {//nation name signed with letters
            public string nationAlpha { get; set; }

            public string getVerboseAlpha()
            {
                switch (nationAlpha)
                {
                    case "A  ":
                        return " Austria";
                    case "AL ":
                        return " Albania";
                    case "ARM":
                        return " Armenia";
                    case "AND":
                        return " Andorra";
                    case "AZ ":
                        return " Azerbaijan";
                    case "B  ":
                        return " Belgium";
                    case "BG ":
                        return " Bulgaria";
                    case "BIH":
                        return " Bosnia and Herzegovina";
                    case "BY ":
                        return " Belarus";
                    case "CH ":
                        return " Switzerland";
                    case "CY ":
                        return " Cyprus";
                    case "CZ ":
                        return " Czech Republic";
                    case "D  ":
                        return " Germany";
                    case "DK ":
                        return " Denmark";
                    case "E  ":
                        return " Spain";
                    case "EST":
                        return " Estonia";
                    case "F  ":
                        return " France";
                    case "FIN":
                        return " Finland";
                    case "FL ":
                        return " Liechtenstein";
                    case "UK ":
                        return " United Kingdom, Alderney, Guernsey, Jersey, Isle of Man, Gibraltar";
                    case "GE ":
                        return " Georgia";
                    case "GR ":
                        return " Greece";
                    case "H  ":
                        return " Hungary";
                    case "HR ":
                        return " Croatia";
                    case "I  ":
                        return " Italy";
                    case "IRL":
                        return " Ireland";
                    case "IS ":
                        return " Iceland";
                    case "KZ ":
                        return " Kazakhstan";
                    case "L  ":
                        return " Luxembourg";
                    case "LT ":
                        return " Lithuania";
                    case "LV ":
                        return " Latvia";
                    case "M  ":
                        return " Malta";
                    case "MC ":
                        return " Monaco";
                    case "MD ":
                        return " Republic of Moldova";
                    case "MK ":
                        return " Macedonia";
                    case "N  ":
                        return " Norway";
                    case "NL ":
                        return " Netherlands";
                    case "P  ":
                        return " Portugal";
                    case "PL ":
                        return " Poland";
                    case "RO ":
                        return " Romania";
                    case "RSM":
                        return " San Marino";
                    case "RUS":
                        return " Russia";
                    case "S  ":
                        return " Sweden";
                    case "SK ":
                        return " Slovakia";
                    case "SLO":
                        return " Slovenia";
                    case "TM ":
                        return " Turkmenistan";
                    case "TR ":
                        return " Turkey";
                    case "UA ":
                        return " Ukraine";
                    case "V  ":
                        return " Vatican City";
                    case "YU ":
                        return " Yugoslavia";
                    case "UNK":
                        return " Unknown";
                    case "EC ":
                        return " European Community";
                    case "EUR":
                        return " Rest of Europe";
                    case "WLD":
                        return " Rest of the world";
                    default:
                        return "No information available";
                }
            }
        }

        internal class NationNumeric
        {//Nation defined by country code
            public int nationNumeric { get; set; }

            public string getVerboseNation()
            {
                switch (nationNumeric)
                {
                    case 0x01:
                        return " Austria";
                    case 0x02:
                        return " Albania";
                    case 0x03:
                        return " Armenia";
                    case 0x04:
                        return " Andorra";
                    case 0x05:
                        return " Azerbaijan";
                    case 0x06:
                        return " Belgium";
                    case 0x07:
                        return " Bulgaria";
                    case 0x08:
                        return " Bosnia and Herzegovina";
                    case 0x09:
                        return " Belarus";
                    case 0x0a:
                        return "  witzerland";
                    case 0x0b:
                        return " Cyprus";
                    case 0x0c:
                        return " Czech Republic";
                    case 0x0d:
                        return " Germany";
                    case 0x0e:
                        return " Denmark";
                    case 0x0f:
                        return " Spain";
                    case 0x10:
                        return " Estonia";
                    case 0x11:
                        return " France";
                    case 0x12:
                        return " Finland";
                    case 0x13:
                        return " Liechtenstein";
                    case 0x14:
                        return " Faeroe Islands";
                    case 0x15:
                        return " United Kingdom";
                    case 0x16:
                        return " Georgia";
                    case 0x17:
                        return " Greece";
                    case 0x18:
                        return " Hungary";
                    case 0x19:
                        return " Croatia";
                    case 0x1a:
                        return " Italy";
                    case 0x1b:
                        return " Ireland";
                    case 0x1c:
                        return " Iceland";
                    case 0x1d:
                        return " Kazakhstan";
                    case 0x1e:
                        return " Luxembourg";
                    case 0x1f:
                        return " Lithuania";
                    case 0x20:
                        return " Latvia";
                    case 0x21:
                        return " Malta";
                    case 0x22:
                        return " Monaco";
                    case 0x23:
                        return " Republic of Moldova";
                    case 0x24:
                        return " Macedonia";
                    case 0x25:
                        return " Norway";
                    case 0x26:
                        return " Netherlands";
                    case 0x27:
                        return " Portugal";
                    case 0x28:
                        return " Poland";
                    case 0x29:
                        return " Romania";
                    case 0x2a:
                        return " San Marino";
                    case 0x2b:
                        return " Russia";
                    case 0x2c:
                        return " Sweden";
                    case 0x2d:
                        return " Slovakia";
                    case 0x2e:
                        return " Slovenia";
                    case 0x2f:
                        return " Turkmenistan";
                    case 0x30:
                        return " Turkey";
                    case 0x31:
                        return " Ukraine";
                    case 0x32:
                        return " Vatican City";
                    case 0x33:
                        return " Yugoslavia";
                    case 0x00:
                        return " No information available";
                    case 0xFD:
                        return " European Community";
                    case 0xFE:
                        return " Rest of Europe";
                    case 0xFF:
                        return " Rest of the world";
                    default:
                        return "RFU";
                }
            }
        }

        internal class NoOfCalibrationsSinceDownload
        {
            public int noOfCalibrationsSinceDownload;
        }

        internal class RegionAlpha
        {
            public string regionAlpha = "";

            public string getVerboseAlpha()
            {
                switch (regionAlpha)
                {
                    case "AN":
                        return " Andalucia";
                    case "AR":
                        return " Aragon";
                    case "AST":
                        return " Asturias";
                    case "C":
                        return " Cantabria";
                    case "CAT":
                        return " Cataluna";
                    case "CL":
                        return " Castilla-Leon";
                    case "CM":
                        return " Castilla-La-Mancha";
                    case "CV":
                        return " Valencia";
                    case "EXT":
                        return " Extremadura";
                    case "G":
                        return " Galicia";
                    case "IB":
                        return " Baleares";
                    case "IC":
                        return " Canarias";
                    case "LR":
                        return " La Rioja";
                    case "M":
                        return " Madrid";
                    case "MU":
                        return " Murcia";
                    case "NA":
                        return " Navarra";
                    case "PV":
                        return " Pais Vasco";
                    default:
                        return "No information available";
                }
            }
        }

        internal class RegionNumeric
        {
            public int regionNumeric;

            public string getVerboseNation()
            {
                switch (regionNumeric)
                {
                    case 0x01:
                        return " Andalucia";
                    case 0x02:
                        return " Aragon";
                    case 0x03:
                        return " Asturias";
                    case 0x04:
                        return " Cantabria";
                    case 0x05:
                        return " Cataluna";
                    case 0x06:
                        return " Castilla-Leon";
                    case 0x07:
                        return " Castilla-La-Mancha";
                    case 0x08:
                        return " Valencia";
                    case 0x09:
                        return " Extremadura";
                    case 0x0a:
                        return " Galicia";
                    case 0x0b:
                        return " Baleares";
                    case 0x0c:
                        return " Canarias";
                    case 0x0d:
                        return " La Rioja";
                    case 0x0e:
                        return " Madrid";
                    case 0x0f:
                        return " Murcia";
                    case 0x10:
                        return " Navarra";
                    case 0x11:
                        return " Pais Vasco";
                    default:
                        return " No information available";
                }
            }
        }

        internal class RSAKeyModulus
        {
            public string rsaKeyModulus = "";
        }

        internal class RSAKeyPrivateExponent
        {
            public string rsaKeyPrivateExponent = "";
        }

        internal class RSAKeyPublicExponent
        {
            public string rsaKeyPublicExponent = "";
        }

        internal class SensorApprovalNumber
        {
            public string sensorApprovalNumber = "";
        }

        internal class SensorOSIdentifier
        {
            public string sensorOSIdentifier = "";
        }

        internal class SensorSCIdentifier
        {
            public string sensorSCI = "";
        }

        internal class Signature
        {
            public string signature = "";
        }

        internal class SimilarEventsNumber
        {
            public int similarEventsNumber = 0;
        }

        internal class SpecificConditionType
        {
            public byte specificConditionType;

            public string getVerboseType()
            {
                switch (specificConditionType)
                {
                    default:
                        return "RFU";
                    case 0x01:
                        return "Out of scope - Begin";
                    case 0x02:
                        return "Out of scope - End";
                    case 0x03:
                        return "Ferry/Train crossing";
                }
            }
        }

        internal class TDesSessionKey
        {
            public string tDesKeyA;
            public string tDesKeyB;
        }

        internal class TimeReal
        {//time in seconds since 1/1/1970 00:00:00
            public int timeSec { get; set; }

            public string ConvertToUTC()
            {
                var t = timeSec;
                var baseDT = new DateTime(1970, 1, 1, 0, 0, 0);
                var j = baseDT.AddSeconds(t);
                return timeSec == 0 ? "undefined" : j.ToString("dd-MMM-yyyy HH:mm:ss");
            }

            public DateTime ConvertToDateTime()
            {
                var t = timeSec;
                var baseDT = new DateTime(1970, 1, 1, 0, 0, 0);
                var j = baseDT.AddSeconds(t);
                return j;
            }
        }

        internal class TyreSize
        {
            public string tyreSize;
        }

        internal class VehicleIdentificationNumber
        {
            public string vehicleIdentificationNumber;
        }

        internal class VehicleRegistrationNumber
        {
            public int codePage;
            public string vehicleRegNumber;
        }

        internal class VuApprovalNumber
        {
            public string vuApprovalNumber = "";
        }

        internal class VuDataBlockCounter
        {
            public int vuDataBlockCounter;
        }

        internal class WorkshopCardPIN
        {
            public string workshopCardPIN = "";
        }



        #endregion DataTypes

        #region DataBundles
        //!!!DATA BUNDLES!!!
        //******************
        //******************
        //!!!DATA BUNDLES!!!

        internal class Block11
        {//I don't know what is this record for
            public Block11Record[] bl11Recs;
            public int bl11RecsCNT = 0;
            public byte[] header = new byte[14];
        }

        internal class Block11Record
        {//I don't know what is this record for
            public FullCardNumber cardNumber = new FullCardNumber();
            public byte[] payloadData = new byte[30];
            public int sometimesDuration = 0;
            public TimeSpan time = new TimeSpan();
        }

        internal class Block13
        {//I don't know what is this record for
            public Block11Record[] bl13Recs;
            public int bl13RecsCNT = 0;
            public byte[] header = new byte[29];
        }

        internal class CardActivityDailyRecord
        {
            public ActivityChangeInfo[] activityChangeInfo;
            public Distance activityDayDistance = new Distance();
            public DailyPresenceCounter activityPresenceCounter = new DailyPresenceCounter();
            public TimeReal activityRecordDate = new TimeReal();
            public int activityRecordLength;

            public int activityRecordPreviousLength;
            //ActivityChange should be counted from activityRecordLength.
            // (which is total in bytes of this record)
        }

        internal class CardCertificate
        {
            public Certificate certificate = new Certificate();
        }

        internal class CardControlActivityDataRecord
        {
            public FullCardNumber controlCardNumber = new FullCardNumber();
            public TimeReal controlDownloadPeriodBegin = new TimeReal();
            public TimeReal controlDownloadPeriodEnd = new TimeReal();
            public TimeReal controlTime = new TimeReal();
            public ControlType controlType = new ControlType();
            public VehicleRegistrationIdentification controlVehicleRegistration = new VehicleRegistrationIdentification();
        }

        internal class CardCurrentUse
        {
            public TimeReal sessionOpenTime = new TimeReal();
            public VehicleRegistrationIdentification sessionOpenVehicle = new VehicleRegistrationIdentification();
        }

        internal class CardDriverActivity //TODO:Rebuild code in order to separate Definition, Parsing and Xml in different functions
        {
            public int activityPointerNewestRecord;
            public int activityPointerOldestRecord;
            public CardActivityDailyRecord[] cardActivityDailyRecord;
            public byte[] cyclicDataRAW;
            public byte[] cyclicDataOrdered;

            public CardActivityDailyRecord[] CyclicDataParser(int length, XmlWriter xmlWriter, int oldest, int newest)
            {
                var output = new CardActivityDailyRecord[3000];
                for (var i = 0; i < 3000; i++) { output[i] = new CardActivityDailyRecord(); }

                cyclicDataOrdered = new byte[length];
                Array.Copy(cyclicDataRAW, oldest, cyclicDataOrdered, 0, length - oldest);
                if (oldest > 0) Array.Copy(cyclicDataRAW, 0, cyclicDataOrdered, length - oldest, oldest);

                var cnt = 0;
                var bc = new ByteConvert();
                var pos = 0;
                var NumOfTh = 0;
                byte[] t;

                while (pos < length)
                {
                    t = new byte[2];
                    Array.Copy(cyclicDataOrdered, pos + 2, t, 0, 2);
                    if (pos + bc.ToInt(t) >= length)
                        break;

                    cnt++;

                    t = new byte[2];
                    Array.Copy(cyclicDataOrdered, pos, t, 0, 2);
                    output[cnt - 1].activityRecordPreviousLength = bc.ToInt(t);
                    pos += 2;

                    Array.Copy(cyclicDataOrdered, pos, t, 0, 2);
                    output[cnt - 1].activityRecordLength = bc.ToInt(t);
                    pos += 2;

                    t = new byte[4];
                    Array.Copy(cyclicDataOrdered, pos, t, 0, 4);
                    output[cnt - 1].activityRecordDate.timeSec = bc.ToInt(t);
                    pos += 4;

                    t = new byte[2];
                    Array.Copy(cyclicDataOrdered, pos, t, 0, 2);
                    output[cnt - 1].activityPresenceCounter.dailyPresenceCounter = bc.ToInt(t);
                    pos += 2;

                    if ((output[cnt - 1].activityRecordPreviousLength == 0) && (output[cnt - 1].activityRecordLength == 0) &&
                        (output[cnt - 1].activityRecordDate.timeSec == 0) && (output[cnt - 1].activityPresenceCounter.dailyPresenceCounter == 0))
                        break;


                    Array.Copy(cyclicDataOrdered, pos, t, 0, 2);
                    output[cnt - 1].activityDayDistance.distance = bc.ToInt(t);
                    pos += 2;

                    var NumberOfThings = (output[cnt - 1].activityRecordLength - 12) / 2;
                    if (NumberOfThings <= 0) NumberOfThings = 0;

                    output[cnt - 1].activityChangeInfo = new ActivityChangeInfo[NumberOfThings];

                    for (var j = 0; j < NumberOfThings; j++)
                    {
                        output[cnt - 1].activityChangeInfo[j] = new ActivityChangeInfo();
                        Array.Copy(cyclicDataOrdered, pos, output[cnt - 1].activityChangeInfo[j].RAW, 0, 2);
                        pos += 2;
                        output[cnt - 1].activityChangeInfo[j].ParseIT();
                    }
                }

                for (int i = 0; i < cnt; i++)
                {
                    if ((output[i].activityRecordPreviousLength == 0) && (output[i].activityRecordLength == 0) &&
                        (output[i].activityRecordDate.timeSec == 0))
                        break;
                    xmlWriter.WriteStartElement("Activities");
                    xmlWriter.WriteAttributeString("Number", i.ToString("D"));

                    xmlWriter.WriteStartElement("activityRecordPreviousLength");
                    xmlWriter.WriteString(output[i].activityRecordPreviousLength.ToString("D") + " Bytes");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("activityRecordLength");
                    xmlWriter.WriteString(output[i].activityRecordLength.ToString("D") + " Bytes");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("activityRecordDate");
                    xmlWriter.WriteAttributeString("timeSec", output[i].activityRecordDate.timeSec.ToString("D"));
                    xmlWriter.WriteString(output[i].activityRecordDate.ConvertToUTC());
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("activityPresenceCounter");
                    xmlWriter.WriteValue(output[i].activityPresenceCounter.dailyPresenceCounter);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("activityDayDistance");
                    xmlWriter.WriteString(output[i].activityDayDistance.distance.ToString("D") + " km");
                    xmlWriter.WriteEndElement();

                    NumOfTh = (output[i].activityRecordLength - 12) / 2;

                    xmlWriter.WriteStartElement("activityChangeInfos");
                    xmlWriter.WriteAttributeString("Number", NumOfTh.ToString("D"));

                    for (var j = 0; j < NumOfTh; j++)
                    {
                        xmlWriter.WriteStartElement("activityChangeInfo");
                        xmlWriter.WriteAttributeString("Number", (j + 1).ToString("D"));

                        xmlWriter.WriteStartElement("activity");
                        xmlWriter.WriteAttributeString("id", output[i].activityChangeInfo[j].activityType);
                        xmlWriter.WriteString(output[i].activityChangeInfo[j].getVerboseActivity());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("time");

                        xmlWriter.WriteStartElement("startTime");
                        if (j > 0)
                        {
                            xmlWriter.WriteAttributeString("minutesSinceMidnight", output[i].activityChangeInfo[j].minutesSinceMidnight.ToString("D"));
                            xmlWriter.WriteString(output[i].activityChangeInfo[j].getVerboseTime());
                        }
                        else
                        {
                            xmlWriter.WriteAttributeString("minutesSinceMidnight", "0");
                            xmlWriter.WriteString("00:00");
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("endTime");
                        if (j + 1 < NumOfTh)
                        {
                            xmlWriter.WriteAttributeString("minutesSinceMidnight",
                                output[i].activityChangeInfo[j + 1].minutesSinceMidnight.ToString("D"));
                            xmlWriter.WriteString(output[i].activityChangeInfo[j + 1].getVerboseTime());
                            xmlWriter.WriteEndElement();
                        }
                        else
                        {
                            xmlWriter.WriteAttributeString("minutesSinceMidnight", "1440");
                            xmlWriter.WriteString("24:00");
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("slotStatus");
                        xmlWriter.WriteString(output[i].activityChangeInfo[j].getVerboseDriverCardStatus() + ", " + output[i].activityChangeInfo[j].getVerboseSlotStatus() + ", " + output[i].activityChangeInfo[j].getVerboseDrivingStatus());
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("RAW");
                        xmlWriter.WriteRaw(Convert.ToString(output[i].activityChangeInfo[j].RAW[0], 2).PadLeft(8, '0') + Convert.ToString(output[i].activityChangeInfo[j].RAW[1], 2).PadLeft(8, '0'));
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();

                }
                /*if (oldest!=0)
                {
                    while (pos < oldest)
                    {
                        cnt++;

                        xmlWriter.WriteStartElement("Activities");
                        xmlWriter.WriteAttributeString("Number", cnt.ToString("D"));

                        byte[] t = new byte[2];
                        Array.Copy(cyclicDataRAW, pos, t, 0, 2);
                        output[cnt - 1].activityRecordPreviousLength = bc.ToInt(t);
                        pos += 2;
                        xmlWriter.WriteStartElement("activityRecordPreviousLength");
                        xmlWriter.WriteString(output[cnt - 1].activityRecordPreviousLength.ToString("D") + " Bytes");
                        xmlWriter.WriteEndElement();

                        Array.Copy(cyclicDataRAW, pos, t, 0, 2);
                        output[cnt - 1].activityRecordLength = bc.ToInt(t);
                        pos += 2;
                        xmlWriter.WriteStartElement("activityRecordLength");
                        xmlWriter.WriteString(output[cnt - 1].activityRecordLength.ToString("D") + " Bytes");
                        xmlWriter.WriteEndElement();

                        t = new byte[4];
                        Array.Copy(cyclicDataRAW, pos, t, 0, 4);
                        output[cnt - 1].activityRecordDate.timeSec = bc.ToInt(t);
                        pos += 4;
                        xmlWriter.WriteStartElement("activityRecordDate");
                        xmlWriter.WriteAttributeString("timeSec", output[cnt - 1].activityRecordDate.timeSec.ToString("D"));
                        xmlWriter.WriteString(output[cnt - 1].activityRecordDate.ConvertToUTC());
                        xmlWriter.WriteEndElement();

                        t = new byte[2];
                        Array.Copy(cyclicDataRAW, pos, t, 0, 2);
                        output[cnt - 1].activityPresenceCounter.dailyPresenceCounter = bc.ToInt(t);
                        pos += 2;
                        xmlWriter.WriteStartElement("activityPresenceCounter");
                        xmlWriter.WriteValue(output[cnt - 1].activityPresenceCounter.dailyPresenceCounter);
                        xmlWriter.WriteEndElement();

                        Array.Copy(cyclicDataRAW, pos, t, 0, 2);
                        output[cnt - 1].activityDayDistance.distance = bc.ToInt(t);
                        pos += 2;
                        xmlWriter.WriteStartElement("activityDayDistance");
                        xmlWriter.WriteString(output[cnt - 1].activityDayDistance.distance.ToString("D") + " km");
                        xmlWriter.WriteEndElement();

                        var NumberOfThings = (output[cnt - 1].activityRecordLength - 12) / 2;
                        output[cnt - 1].activityChangeInfo = new ActivityChangeInfo[NumberOfThings];
                        xmlWriter.WriteStartElement("activityChangeInfos");
                        xmlWriter.WriteAttributeString("Number", NumberOfThings.ToString("D"));

                        for (var j = 0; j < NumberOfThings; j++)
                        {
                            xmlWriter.WriteStartElement("activityChangeInfo");
                            xmlWriter.WriteAttributeString("Number", (j + 1).ToString("D"));

                            output[cnt - 1].activityChangeInfo[j] = new ActivityChangeInfo();
                            Array.Copy(cyclicDataRAW, pos, output[cnt - 1].activityChangeInfo[j].RAW, 0, 2);
                            pos += 2;
                            output[cnt - 1].activityChangeInfo[j].ParseIT();

                            xmlWriter.WriteStartElement("activity");
                            xmlWriter.WriteAttributeString("id", output[cnt - 1].activityChangeInfo[j].activityType);
                            xmlWriter.WriteString(output[cnt - 1].activityChangeInfo[j].getVerboseActivity());
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteStartElement("time");

                            xmlWriter.WriteStartElement("startTime");
                            if (j>0)
                            {
                                xmlWriter.WriteAttributeString("minutesSinceMidnight", output[cnt - 1].activityChangeInfo[j-1].minutesSinceMidnight.ToString("D"));
                                xmlWriter.WriteString(output[cnt - 1].activityChangeInfo[j-1].getVerboseTime());
                            }
                            else
                            {
                                xmlWriter.WriteAttributeString("minutesSinceMidnight", "0");
                                xmlWriter.WriteString("00:00");
                            }
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteStartElement("endTime");
                            xmlWriter.WriteAttributeString("minutesSinceMidnight", output[cnt - 1].activityChangeInfo[j].minutesSinceMidnight.ToString("D"));
                            xmlWriter.WriteString(output[cnt - 1].activityChangeInfo[j].getVerboseTime());
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteStartElement("slotStatus");
                            xmlWriter.WriteString(output[cnt - 1].activityChangeInfo[j].getVerboseDriverCardStatus() + ", " + output[cnt - 1].activityChangeInfo[j].getVerboseSlotStatus() + ", " + output[cnt - 1].activityChangeInfo[j].getVerboseDrivingStatus(false));
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteStartElement("RAW");
                            xmlWriter.WriteRaw(Convert.ToString(output[cnt - 1].activityChangeInfo[j].RAW[0], 2).PadLeft(8, '0') + Convert.ToString(output[cnt - 1].activityChangeInfo[j].RAW[1], 2).PadLeft(8, '0'));
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteEndElement();

                    }
                }
                */
                var cardActivityDailyRecords = new CardActivityDailyRecord[cnt];
                Array.Copy(output, cardActivityDailyRecords, cnt);
                xmlWriter.Flush();
                return cardActivityDailyRecords;
            }
        }

        internal class CardDrivingLicenseInformation
        {
            public Name drivingLicenseIssuingAuthoruty = new Name();
            public NationNumeric drivingLicenseIssuingNation = new NationNumeric();
            public string drivingLicenseNumber = "";
        }

        internal class CardEventData
        {
            public CardEventRecords[] ceRecs = new CardEventRecords[6];
        }

        internal class CardEventRecord
        {
            public TimeReal eventBeginTime = new TimeReal();
            public TimeReal eventEndTime = new TimeReal();
            public EventFaultType eventType = new EventFaultType();
            public VehicleRegistrationIdentification eventVehicleRegistration = new VehicleRegistrationIdentification();
        }

        internal class CardEventRecords
        {
            public CardEventRecord[] ceRec = new CardEventRecord[1];
        }

        internal class CardFaultData
        {
            public CardFaultRecords[] cardFaultRecords = new CardFaultRecords[2];
        }

        internal class CardFaultRecords
        {
            public CardEventRecord[] cfRec = new CardEventRecord[1];
        }

        internal class CardIccIdentification
        {
            public string cardApprovalNumber;
            public ExtendedSerialNumber cardExtendedSerialNumber = new ExtendedSerialNumber();
            public byte cardPersonaliserID;
            public int clockStop;
            public byte[] embedderIcAssemblerId = new byte[5];
            public byte[] icIdentifier = new byte[2];
        }

        internal class CardIdentification
        {
            public TimeReal cardExpiryDate = new TimeReal();
            public TimeReal cardIssueDate = new TimeReal();
            public Name cardIssuingAuthorityName = new Name();
            public NationNumeric cardIssuingMemberState = new NationNumeric();
            public string cardNumber;
            public TimeReal cardValidityBegin = new TimeReal();
        }

        internal class CardNumber
        {
            public CardConsecutiveIndex cardConsecutiveIndex = new CardConsecutiveIndex();
            public CardRenewalIndex cardRenewalIndex = new CardRenewalIndex();
            public CardReplacementIndex cardReplacementIndex = new CardReplacementIndex();
            public string driverIdentification { get; set; }
            //Should programmly change, which to use!!!
            public string ownerIdentification { get; set; }
        }

        internal class CardPlaceDailyWorkPeriod
        {
            public int placePointerNewestRecord;
            public PlaceRecord[] plRecs;
        }

        internal class CardPrivateKey
        {
            public RSAKeyPrivateExponent cardPrivateKey = new RSAKeyPrivateExponent();
        }

        internal class CardPublicKey
        {
            public PublicKey cardPublicKey = new PublicKey();
        }

        internal class CardSlots
        {
            public FullCardNumber cardNumberCoDriverSlotBegin = new FullCardNumber();
            public FullCardNumber cardNumberCoDriverSlotEnd = new FullCardNumber();
            public FullCardNumber cardNumberDriverSlotBegin = new FullCardNumber();
            public FullCardNumber cardNumberDriverSlotEnd = new FullCardNumber();
        }

        internal class CardVehicleRecord
        {
            public VehicleRegistrationIdentification registration = new VehicleRegistrationIdentification();
            public TimeReal vehicleFirstUse = new TimeReal();
            public TimeReal vehicleLastUse = new TimeReal();
            public int vehicleOdometerBegin;
            public int vehicleOdometerEnd;
            public VuDataBlockCounter vuDataBlockCounter = new VuDataBlockCounter();
        }

        internal class CardVehiclesUsed
        {
            public CardVehicleRecord[] cVehRecs;
            public int vehiclePointerNewestRecord;
        }

        internal class Certificate
        {
            private byte[] cndash = new byte[58];
            private byte[] sign = new byte[128];

            public CertificateAuthority certificateAuthorityReferenceerty = new CertificateAuthority();

            public byte[] Cndash
            {
                get { return cndash; }
                set { cndash = value; }
            }

            public byte[] Sign
            {
                get { return sign; }
                set { sign = value; }
            }
        }

        internal class CertificateAuthority
        {
            public byte[] additionalInfo = new byte[2];
            public int caIdentifier;
            public int keySerialNumber;
            public NationAlpha nationAlpha = new NationAlpha();
            public NationNumeric nationNumeric = new NationNumeric();
        }

        internal class CertificateContent
        {
            public CertificateAuthority certificateAuthorityReference = new CertificateAuthority();
            public CertificateHolderAuthorization certificateHolderAuthorization = new CertificateHolderAuthorization();
            public KeyIdentifier certificateHolderReference = new KeyIdentifier();
            public int certificateProfileIdentifier = 0;
            public TimeReal endOfValidity = new TimeReal();
            public PublicKey rsaPublicKey = new PublicKey();
        }

        internal class CertificateHolderAuthorization
        {
            private byte[] tachographApplicationId = new byte[6];

            public EquipmentType equipmentType = new EquipmentType();

            public byte[] TachographApplicationId
            {
                get { return tachographApplicationId; }
                set { tachographApplicationId = value; }
            }
        }

        internal class CertificateRequestID
        {
            public byte crIdentifier { get; set; }
            public ManufacturerCode manufacturerCode = new ManufacturerCode();
            public BcdMonthYear requestMonthYear = new BcdMonthYear();
            public int requestSerialNumber { get; set; }
        }

        internal class CompanyActivityData
        {
            public int companyPointerNewestRecord { get; set; }
            public CompanyActivityRecord[] cRecs { get; set; }
        }

        internal class CompanyActivityRecord
        {
            public FullCardNumber cardNumberInformation = new FullCardNumber();
            public TimeReal companyActivityTime = new TimeReal();
            public CompanyActivityType companyActivityType = new CompanyActivityType();
            public TimeReal downloadPeriodBegin = new TimeReal();
            public TimeReal downloadPeriodEnd = new TimeReal();

            public VehicleRegistrationIdentification vehicleRegistrationInformation =
                new VehicleRegistrationIdentification();
        }

        internal class CompanyCardApplicationIdentification
        {
            public byte[] cardStructureVersion = new byte[2];
            public int noOfCompanyActivityRecords;
            public EquipmentType typeOfTachographCardId = new EquipmentType();
        }

        internal class CompanyCardHolderIdentification
        {
            public Language cardHolderPreferredLanguage = new Language();
            public Address companyAddress = new Address();
            public Name companyName = new Name();
        }

        internal class ControlActivityRecord
        {
            public TimeReal controlDownloadPeriodBegin = new TimeReal();
            public TimeReal controlDownloadPeriodEnd = new TimeReal();
            public FullCardNumber controlledCardNumber = new FullCardNumber();
            public VehicleRegistrationIdentification controlledVehicleRegistration = new VehicleRegistrationIdentification();
            public TimeReal controlTime = new TimeReal();
            public ControlType controlType = new ControlType();
        }

        internal class ControlCardApplicationIdentification
        {
            public byte[] cardStructureVersion = new byte[2];
            public int noOfControlActivityRecords;
            public EquipmentType typeOfTachographCardId = new EquipmentType();
        }

        internal class ControlCardControlActivityData
        {
            public ControlActivityRecord[] conActRecs;
            public int controlPointerNewestRecord;
        }

        internal class ControlCardHolderIdentification
        {
            public HolderName cardHolderName = new HolderName();
            public Language cardHolderPreferredLanguage = new Language();
            public Address controlBodyAddress = new Address();
            public Name controlBodyName = new Name();
        }

        internal class CurrentDateTime
        {
            public TimeReal currentDateTime = new TimeReal();
        }

        internal class DailyPresenceCounter
        {
            public int dailyPresenceCounter { get; set; }
        }

        internal class Datef
        {
            public string day { get; set; }
            public string month { get; set; }
            public string year { get; set; }
        }

        internal class DriverCardApplicationIdentification
        {
            public byte[] activityStructureLength = new byte[2];
            public byte[] cardStructureVersion = new byte[2];
            public int noOfCardPlaceRecords;
            public int noOfCardVehicleRecords;
            public int noOfEventsPerType;
            public int noOfFaultsPerType;
            public EquipmentType typeOfTachographCardId = new EquipmentType();
        }

        internal class DriverCardHolderIdentification
        {
            public BCDDate cardHolderBirthDate = new BCDDate();
            public HolderName cardHolderName = new HolderName();
            public Language cardHolderPreferredLanguage = new Language();
        }

        internal class EuropeanPublicKey
        {
            public PublicKey europeanPublicKey = new PublicKey();
        }

        internal class ExtendedSerialNumber
        {
            public BcdMonthYear date = new BcdMonthYear();
            public EquipmentType equipmentType = new EquipmentType();
            public ManufacturerCode manufacturerCode = new ManufacturerCode();
            public int serialNumber;
        }

        internal class FullCardNumber
        {
            public NationNumeric cardIssuingMemberState = new NationNumeric();
            public CardNumber cardNumber = new CardNumber();
            public EquipmentType cardType = new EquipmentType();
        }

        internal class HolderName
        {
            public Name holderFirstNames = new Name();
            public Name holderSurname = new Name();
        }

        internal class KeyIdentifier //TODO: What was it for?
        {
            public CertificateAuthority certificateAuthorityKID = new CertificateAuthority();
            public CertificateRequestID certificateRequestID = new CertificateRequestID();
            public ExtendedSerialNumber extendedSerialNumber = new ExtendedSerialNumber();

            public object getActiveChoice()
            {
                if (extendedSerialNumber != null) return extendedSerialNumber.GetType();
                if (certificateRequestID != null) return certificateRequestID.GetType();
                if (certificateAuthorityKID != null) return certificateAuthorityKID.GetType();
                throw new ArgumentNullException("", "KeyIdentifier is Null!");
            }
        }

        internal class LastCardDownload
        {
            public TimeReal lastCardDownload = new TimeReal();

            public override string ToString()
            {
                return lastCardDownload.ConvertToUTC();
            }
        }

        internal class MemberStateCertificate
        {
            public Certificate certificate = new Certificate();
        }

        internal class MemberStatePublicKey
        {
            public PublicKey memberStatePublicKey = new PublicKey();
        }

        internal class PlaceRecord
        {
            public NationNumeric dailyWorkPeriodCountry = new NationNumeric();
            public RegionNumeric dailyWorkPeriodRegion = new RegionNumeric();
            public TimeReal entryTime = new TimeReal();
            public EntryTypeDailyWorkPeriod entryTypeDailyWorkPeriod = new EntryTypeDailyWorkPeriod();
            public int vehicleOdometerValue;
        }

        internal class PlainCertificate
        {
            public CertificateAuthority keyIdentifier = new CertificateAuthority();
            public PublicKey rsaPublicKey = new PublicKey();
        }

        internal class PreviousVehicleInfo
        {
            public TimeReal cardWithdrawalTime = new TimeReal();

            public VehicleRegistrationIdentification vehicleRegistrationIdentification =
                new VehicleRegistrationIdentification();
        }

        internal class PublicKey
        {
            public RSAKeyModulus rsaKeyModulus = new RSAKeyModulus();
            public RSAKeyPublicExponent rsaKeyPublicExponent = new RSAKeyPublicExponent();
        }

        internal class SensorIdentification
        {
            public SensorApprovalNumber sensorAN = new SensorApprovalNumber();
            public SensorOSIdentifier sensorOSI = new SensorOSIdentifier();
            public SensorSCIdentifier sensorSCI = new SensorSCIdentifier();
            public SensorSerialNumber sensorSN = new SensorSerialNumber();
        }

        internal class SensorInstallation
        {
            public VuApprovalNumber currentVuAN = new VuApprovalNumber();
            public ExtendedSerialNumber currentVuSN = new ExtendedSerialNumber();
            public VuApprovalNumber firstVuAN = new VuApprovalNumber();
            public ExtendedSerialNumber firstVuSN = new ExtendedSerialNumber();
            public SensorPairingDate sensorPairingDateCurrent = new SensorPairingDate();
            public SensorPairingDate sensorPairingDateFirst = new SensorPairingDate();
        }

        internal class SensorInstallationSecData
        {
            public TDesSessionKey sensorInstallationSecData = new TDesSessionKey();
        }

        internal class SensorPaired
        {
            public SensorApprovalNumber sensorAN = new SensorApprovalNumber();
            public SensorPairingDate sensorPairingDateFirst = new SensorPairingDate();
            public SensorSerialNumber sensorSN = new SensorSerialNumber();
        }

        internal class SensorPairingDate
        {
            public TimeReal sensorPairingDate = new TimeReal();
        }

        internal class SensorSerialNumber
        {
            public ExtendedSerialNumber sensorSN = new ExtendedSerialNumber();
        }

        internal class SpecificConditionRecord
        {
            public TimeReal entryTime = new TimeReal();
            public SpecificConditionType specificConditionType = new SpecificConditionType();
        }

        internal class SpecificConditionRecords
        {
            public SpecificConditionRecord[] scRec;
        }

        internal class SpecificConditions
        {
            public SpecificConditionRecord[] scRec = new SpecificConditionRecord[56];
        }

        internal class VehicleRegistrationIdentification
        {
            public NationNumeric vehicleRegistrationNation = new NationNumeric();
            public VehicleRegistrationNumber vehicleRegistrationNumber = new VehicleRegistrationNumber();
        }

        internal class VuActivities
        {
            public ActivityChangeInfo[] acChInfo;
            public int acChInfoCNT = 0;
            public VuCardIWRecord[] cIWRecs;
            public int cIWRecsCNT = 0;
            public VuPlaceDailyWorkPeriodRecord[] dWPRecs;
            public int dWPRecsCNT = 0;
            public int odometerValueMidnight = 0;
            public SpecificConditionRecord[] spCondRecs;
            public int spCondRecsCNT = 0;
            public TimeReal timeReal = new TimeReal();
        }

        internal class VuActivityDailyData
        {
            public ActivityChangeInfo activityChangeInfos = new ActivityChangeInfo();
            public int noOfActivityChanges;
        }

        internal class VuCalibrationData
        {
            public int noOfVuCalibrationRecords = 0;
            public VuCalibrationRecord[] vuCalRecs;
        }

        internal class VuCalibrationRecord
        {
            public int authorisedSpeed = 0;
            public CalibrationPurpose calibrationPurpose = new CalibrationPurpose();
            public int kConstantOfRecordingEquipment = 0;
            public int lTyreCircumference = 0;
            public int newOdometerValue = 0;
            public TimeReal newTimeValue = new TimeReal();
            public TimeReal nextCalibrationDate = new TimeReal();
            public int oldOdometerValue = 0;
            public TimeReal oldTimeValue = new TimeReal();
            public TyreSize tyreSize = new TyreSize();
            public VehicleIdentificationNumber vehicleIdentificationNumber = new VehicleIdentificationNumber();

            public VehicleRegistrationIdentification vehicleRegistrationIdentification =
                new VehicleRegistrationIdentification();

            public Address workshopAddress = new Address();
            public TimeReal workshopCardExpiryDate = new TimeReal();
            public FullCardNumber workshopCardNumber = new FullCardNumber();
            public Name workshopName = new Name();
            public int wVehicleCharacteristicConstant = 0;
        }

        internal class VuCardData
        {
            //should be saved to a separate file;
        }

        internal class VuCardIWData
        {
            public int noOfIWRecords;
            public VuCardIWRecord[] VuCIWR;
        }

        internal class VuCardIWRecord
        {
            public TimeReal cardExpiryDate = new TimeReal();
            public Name cardHolderName = new Name();
            public TimeReal cardInsertionTime = new TimeReal();
            public FullCardNumber cardNumber = new FullCardNumber();
            public CardSlotsStatus cardSlotNumber = new CardSlotsStatus();
            public TimeReal cardWithdrawalTime = new TimeReal();
            public ManualInputFlag manualInputFlag = new ManualInputFlag();
            public TimeReal previousCardWithdrawalTime = new TimeReal();
            public VehicleRegistrationIdentification previousVehicleRegistration = new VehicleRegistrationIdentification();
            public int vehicleOdometerValueAtInsertion = 0;
            public int vehicleOdometerValueAtWithdrawal = 0;
        }

        internal class VuCompanyLocksData
        {
            public int noOfLocks { get; set; }
            public VuCompanyLocksRecord[] vuCLR { get; set; }
        }

        internal class VuCompanyLocksRecord
        {
            public Address companyAddress = new Address();
            public FullCardNumber companyCardNumber = new FullCardNumber();
            public Name companyName = new Name();
            public TimeReal lockInTime = new TimeReal();
            public TimeReal lockOutTime = new TimeReal();
        }

        internal class VuControlActivityData
        {
            public int noOfControls = 0;
            public VuControlActivityRecord[] vuCAR;
        }

        internal class VuControlActivityRecord
        {
            public FullCardNumber controlCardNumber = new FullCardNumber();
            public TimeReal controlTime = new TimeReal();
            public ControlType controlType = new ControlType();
            public TimeReal downloadPeriodBeginTime = new TimeReal();
            public TimeReal downloadPeriodEndTime = new TimeReal();
        }

        internal class VuDetailedSpeedBlock
        {
            public TimeReal speedBlockBeginDate = new TimeReal();
            public byte[] speedsPerSecond = new byte[60];
        }

        internal class VuDetailedSpeedData
        {
            public int noOfSpeedBlocks;
            public VuDetailedSpeedBlock[] vuDSB;
        }

        internal class VuDownloadablePeriod
        {
            public TimeReal maxDownloadableTime = new TimeReal();
            public TimeReal minDownloadableTime = new TimeReal();
        }

        internal class VuDownloadActivityData
        {
            public Name companyOrWorkshopName = new Name();
            public TimeReal downloadingTime = new TimeReal();
            public FullCardNumber fullCardNumber = new FullCardNumber();
        }

        internal class VuEventData
        {
            public int noOfVuEvents;
            public VuEventRecord[] vuER;
        }

        internal class VuEventRecord
        {
            public CardSlots cardSlots = new CardSlots();
            public TimeReal eventBeginTime = new TimeReal();
            public TimeReal eventEndTime = new TimeReal();
            public EventFaultRecordPurpose eventRecordPurpose = new EventFaultRecordPurpose();
            public EventFaultType eventType = new EventFaultType();
            public int similarEventsNumber = 0;
        }

        internal class VuEventsFaults
        {
            public VuEventRecord[] evRecs;
            public int evRecsCNT = 0;
            public TimeReal firstOverspeedSince = new TimeReal();
            public VuFaultRecord[] flRecs;
            public int flRecsCNT = 0;
            public TimeReal lastOverspeedControlTime = new TimeReal();
            public int numberOfOverspeedSince = 0;
            public VuOverSpeedingEventRecord[] ovSpdRecs;
            public int ovSpdRecsCNT = 0;
            public VuTimeAdjustmentRecord tmAdjRecs;
            public int tmAdjRecsCNT = 0;
        }

        internal class VuFaultData
        {
            public int noOfVuFaults;
            public VuFaultRecord[] vuFR;
        }

        internal class VuFaultRecord
        {
            public CardSlots cardSlots = new CardSlots();
            public EventFaultRecordPurpose eventRecordPurpose = new EventFaultRecordPurpose();
            public EventFaultType eventType = new EventFaultType();
            public TimeReal faultBeginTime = new TimeReal();
            public TimeReal faultEndTime = new TimeReal();
        }

        internal class VuIdentification
        {
            public VuApprovalNumber vuApprovalNumber = new VuApprovalNumber();
            public Address vuManufacturerAddress = new Address();
            public Name vuManufacturerName = new Name();
            public TimeReal vuManufacturingDate = new TimeReal();
            public string vuPartNumber = "";
            public ExtendedSerialNumber vuSerialNumber = new ExtendedSerialNumber();
            public VuSoftwareIdentification vuSoftwareIdentification = new VuSoftwareIdentification();
        }

        internal class VuOverSpeedingControlData
        {
            public TimeReal firstOverspeedSince = new TimeReal();
            public TimeReal lastOverspeedControlTime = new TimeReal();
            public int numberOfOverspeedSince = 0;
        }

        internal class VuOverSpeedingEventData
        {
            public int noOfVuOverSpeedingEvents;
            public VuOverSpeedingEventRecord[] vuOSER;
        }

        internal class VuOverSpeedingEventRecord
        {
            public int averageSpeedValue = 0;
            public FullCardNumber cardNumberDriverSlotBegin = new FullCardNumber();
            public TimeReal eventBeginTime = new TimeReal();
            public TimeReal eventEndTime = new TimeReal();
            public EventFaultRecordPurpose eventRecordPurpose = new EventFaultRecordPurpose();
            public EventFaultType eventType = new EventFaultType();
            public int maxSpeedValue = 0;
            public int similarEventsNumber = 0;
        }

        internal class VuOverview
        {
            public VuControlActivityRecord[] cActivRec;
            public int cActivRecCNT = 0;
            public FullCardNumber cardNumber = new FullCardNumber();
            public CardSlotsStatus cardSlotsStatus = new CardSlotsStatus();
            public VuCompanyLocksRecord[] cLocksRec;
            public int cLocksRecCNT = 0;
            public string companyOrWorkshopName = "";
            public TimeReal currentDateTime = new TimeReal();
            public TimeReal downloadingTime = new TimeReal();
            public Certificate memberStateCertificate = new Certificate();
            public string vehicleIdentificationNumber = "";

            public VehicleRegistrationIdentification vehicleRegistrationIdentification =
                new VehicleRegistrationIdentification();

            public Certificate vuCertificate = new Certificate();
            public VuDownloadablePeriod vuDownloadablePeriod = new VuDownloadablePeriod();
        }

        internal class VuPlaceDailyWorkPeriodData
        {
            public int noOfPlaceRecords;
            public VuPlaceDailyWorkPeriodRecord[] vuPDWPR;
        }

        internal class VuPlaceDailyWorkPeriodRecord
        {
            public FullCardNumber fullCardNumber = new FullCardNumber();
            public PlaceRecord placeRecord = new PlaceRecord();
        }

        internal class VuPrivateKey
        {
            public RSAKeyPrivateExponent vuPublicKey = new RSAKeyPrivateExponent();
        }

        internal class VuPublicKey
        {
            public PublicKey vuPublicKey = new PublicKey();
        }

        internal class VuSoftwareIdentification
        {
            public TimeReal vuSoftInstallationDate = new TimeReal();
            public string vuSoftwareVersion = "";
        }

        internal class VuSpecificConditionData
        {
            public int noOfSpecificConditionRecords;
            public SpecificConditionRecord[] specificConditionRecords;
        }

        internal class VuTechnical
        {
            public VuCalibrationRecord[] calRecs;
            public int calRecsCNT = 0;
            public string sensorApprovalNumber = "";
            public TimeReal sensorPairingDateFirst = new TimeReal();
            public ExtendedSerialNumber sensorSerialNumber = new ExtendedSerialNumber();
            public VuApprovalNumber vuApprovalNumber = new VuApprovalNumber();
            public string vuManufacturerAddress = "";
            public string vuManufacturerName = "";
            public TimeReal vuManufacturingDate = new TimeReal();
            public string vuPartNumber = "";
            public ExtendedSerialNumber vuSerialNumber = new ExtendedSerialNumber();
            public TimeReal vuSoftInstallationDate = new TimeReal();
            public string vuSoftwareVersion = "";
        }

        internal class VuTimeAdjustmentData
        {
            public int noOfVuTimeAdjRecords = 0;
            public VuTimeAdjustmentRecord[] vuTAR;
        }

        internal class VuTimeAdjustmentRecord
        {
            public TimeReal newTimeValue = new TimeReal();
            public TimeReal oldTimeValue = new TimeReal();
            public Address workshopAddress = new Address();
            public FullCardNumber workshopCardNumber = new FullCardNumber();
            public Name workshopName = new Name();
        }

        internal class WorkshopCardApplicationIdentification
        {
            public int activityStructureLength;
            public byte[] cardStructureVersion = new byte[2];
            public int noOfCalibrationRecords;
            public int noOfCardPlaceRecords;
            public int noOfCardVehicleRecords;
            public int noOfEventsPerType;
            public int noOfFaultsPerType;
            public EquipmentType typeOfTachographId = new EquipmentType();
        }

        internal class WorkshopCardCalibrationData
        {
            public int calibrationPointerNewestRecord;
            public int calibrationTotalNumber;
            public WorkshopCardCalibrationRecord[] wshopCCR;
        }

        internal class WorkshopCardCalibrationRecord
        {
            public int authorisedSpeed;
            public CalibrationPurpose calibrationPurpose = new CalibrationPurpose();
            public int kConstantOfRecordingEquipment;
            public int lTyreCircumference;
            public int newOdometerValue;
            public TimeReal newTimeValue = new TimeReal();
            public TimeReal nextCalibrationDate = new TimeReal();
            public int oldOdometerValue;
            public TimeReal oldTimeValue = new TimeReal();
            public SensorSerialNumber sensorSerialNumber = new SensorSerialNumber();
            public TyreSize tyreSize = new TyreSize();
            public VehicleRegistrationIdentification vehicleRegistration = new VehicleRegistrationIdentification();
            public VehicleIdentificationNumber VIN = new VehicleIdentificationNumber();
            public string vuPartNumber;
            public ExtendedSerialNumber VuSerialNumber = new ExtendedSerialNumber();
            public int wVehicleCharacteristicConstant;
        }

        internal class WorkshopCardHolderIdentification
        {
            public HolderName cardHolderName = new HolderName();
            public Language cardHolderPreferredLanguage = new Language();
            public Address workshopAddress = new Address();
            public Name workshopName = new Name();
        }

        #endregion DataBundles

        #region AdditionalDataTypes
        //!!!ADDITIONAL DATA TYPES!!!
        //***************************
        //***************************
        //!!!ADDITIONAL DATA TYPES!!!

        internal class Statistics
        {
            public DailyStatistics[] ds;
        }

        internal struct DailyStatistics
        {//Was intended to be a universal struct/class to include all possible counters for driving and 
         //non-driving activities and faults
            public TimeReal vDayTime;
            public int vCardDailyActivityRecordNumber;
            public int vOrderedArrayId;

            public int vDrivingTimeTotal, vAvailableTimeTotal, vWorkingTimeTotal, vOtherTimeTotal;
            public int vDrivingTimeSinceLastBreak, vOtherTime, vOvertime, vDrivingTimeWeekly, vDrivingTime2Weeks, v144hours;
            public int vDrivingCrewTimeCurrent;
            public int vDailyBreakTime, vBreakTime;

            public bool

                isDrivingPeriodOne,
                isDrivingPeriodTwo,
                isShortBreakWhole,
                isShortBreakPartial,


                isDrivingPeriodOneCompleted,
                isDrivingPeriodOneExceeded,
                isDrivingPeriodOneShort,
                isDrivingPeriodOneBrokenForLessThanFiveMins,
                isDrivingPeriodTwoCompleted,
                isDrivingPeriodTwoExceeded,
                isDrivingPeriodTwoShort,
                isDrivingPeriodTwoBrokenForLessThanFiveMins,
                isDrivingPeriodExtra,

                isDrivingTimeNotIncludesShortBreak,
                isDailyDrivingTimeNotExceeded,
                isDailyDrivingTimeExceededForOneHour_FirstTime,
                isDailyDrivingTimeExceededForOneHour_SecondTime,
                isWeeklyDrivingTimeNotExceeded,
                isTwoConsecutiveWeeksDrivingSumBelow90Hours,
                isCrewToday,
                isSingleToday,
                isCrewNow,
                isSingleNow,
                isSingleForAMoment,

                isShortBreak15min, isShortBreakCompleted, isShortBreak30min,

                isShortBreak,
                isShortBreakDivided,
                isShortBreakDividedCompleted,
                isShortBreakTooShortPartOne,
                isShortBreakTooShortPartTwo,
                isDailyBreak,
                isDailyBreakDivided,
                isDailyBreakCompleted,
                isDailyBreakShort_First,
                isDailyBreakShort_Second,
                isDailyBreakShort_Third,
                isDailyBreakCompensated,
                isDailyBreakTooShort,
                isDailyBreakShortTooShort,
                isCrewDailyBreakCompleted,
                isCrewDailyBreakTooShort,
                isWeeklyBreak,
                isWeeklyBreakShort,
                isWeeklyBreakCompensated;
        }

        internal class InputRAWDataStorage
        {//this block is used to store raw hex input, separated by block ID, its length (set by 
         //law) and hex byte array
            internal class DataBlock
            {
                private ByteConvert ByteCon = new ByteConvert();
                public byte[] ID = new byte[3];
                public byte[] Length = new byte[2];
                public byte[] Value;

                public int LengthInt()
                {//returns defined length in form of integer
                    return ByteCon.ToInt(Length);
                }
            }

            public DataBlock[] DataArchive = new DataBlock[100];
        }

        internal class PrintingLayout
        {//Global variable, that collects all parts of a card information, and stores it as a universal class
            //First block: Card Identifiers.
            public CardIccIdentification cardICCI = new CardIccIdentification();
            public CardChipIdentification cardCI = new CardChipIdentification();

            //Second block: Driver card information. It includes all parts that the driver card is designed to
            //include.
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
        }

        internal class FinesRecord
        {//class to contain a single fine record.
            public DateTime DateTime;
            public int Duration;
            public string FaultText;
        }

        internal class ShiftTimeCounter
        {//was intended to be a counter of all work and rest activities throughout the driver card record.
            public int[] Work;
            public int[] FullRest;
        }

        internal class EventInfoRoute
        {//was replaced by activityChangeInfo
            public DateTime EventStart { get; internal set; }
            public int DurationOfEventInMinutes { get; internal set; }
            public DateTime EventEnd { get; internal set; }
            public string EventType { get; set; }
            public string DrivingStatus { get; set; }
            public string DriverCardStatus { get; set; }
            public string CardSlotStatus { get; set; }
        }

        internal class ActivityChangeInfoExtended
        {
            public DateTime StartingDateTime;
            public int Duration;
            public DateTime EndingDateTime;
            public string ActivityType, DriverCardStatus, DrivingStatus, SlotStatus;

        }

        internal class ShiftStats
        {//statistical class for whole shift
            public int shiftID = 0;
            public int workingMinCounter = 0;
            public int dailyBreakMinCounter = 0;
            public int shiftBreakMinCounter = 0;
            public bool dailyBreakComplete = false;
            public bool dailyBreakTimeReduced = false;
            public bool dailyBreakTimeLimitExceeded = false;
            public bool shiftBreakComplete = false;
            public bool dailyDrivingTimeExceeded_Fault = false;
            public bool dailyDrivingTimeExceeded_Warning = false;
            public List<ViolationRecord> sv = new List<ViolationRecord>();
        }

        internal class ViolationRecord
        {//This is the actual class that should contain violation record.
            public DateTime ViolationDateTime;
            public int amount;
            public string VIOLATION_TYPE;
            public string ViolationText;
        }

        #endregion AdditionalDataTypes
    }
}