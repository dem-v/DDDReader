using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DDDReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter filename: ");
            string fname = Console.ReadLine();
            Library lib = new Library();
            lib.FileOpenReadDecode(fname);
            //Output call here!
        }
    }

    /// <summary>
    /// Here begins section with datatypes definitions
    /// </summary>

    class ActivityChangeInfo
    {
        public string binaryString;
        public byte[] RAW = new byte[1];
        public int slotStatus;
        public int drivingStatus;
        public int driverCardStatus;
        public string activityType;
        public int minutesSinceMidnight;

        public void ParseIT()
        {
            binaryString = Convert.ToString(RAW[0], 2).PadLeft(8, '0') + Convert.ToString(RAW[1], 2).PadLeft(8, '0');
            slotStatus = binaryString[0];
            drivingStatus = binaryString[1];
            driverCardStatus = binaryString[2];
            activityType = binaryString.Substring(3, 2);
            minutesSinceMidnight = Convert.ToInt32(binaryString.Substring(5, 11), 2);

        }

        public string getVerboseSlotStatus()
        {
            switch (slotStatus)
            {
                case 0: return "DRIVER";
                case 1: return "CO-DRIVER";
                default: return null;
            }

        }
        public string getVerboseDrivingStatus(bool isWorkshopOrDriver)
        {
            if (!isWorkshopOrDriver)
            {
                switch (drivingStatus)
                {
                    case 0: return "SINGLE";
                    case 1: return "CREW";
                    default: return null;
                }
            }
            else
            {
                if (driverCardStatus == 0)
                {
                    switch (drivingStatus)
                    {
                        case 0: return "SINGLE";
                        case 1: return "CREW";
                        default: return null;
                    }
                }
                else if (driverCardStatus == 1)
                {
                    switch (drivingStatus)
                    {
                        case 0: return "UNKNOWN";
                        case 1: return "KNOWN(manual)";
                        default: return null;
                    }
                }
                else { return null; }
            }
        }
        public string getVerboseDriverCardStatus()
        {
            switch (driverCardStatus)
            {
                case 0: return "INSERTED";
                case 1: return "NOT INSERTED";
                default: return null;
            }
        }
        public string getVerboseActivity()
        {
            switch (activityType)
            {
                case "00": return "BREAK/REST";
                case "01": return "AVAILABILITY";
                case "10": return "WORK";
                case "11": return "DRIVING";
                default: return null;
            }
        }
        public string getVerboseTime()
        {
            double n = minutesSinceMidnight / 60;
            return (((int)n).ToString() + ":" + ((int)(n - (int)n) * 60).ToString());
        }
    }
    class EntryTypeDailyWorkPeriod
    {
        public int entryTypeDailyWorkPeriod { get; set; }

        public string getVerboseEntryType()
        {

            switch (entryTypeDailyWorkPeriod)
            {
                case 0: return "Begin, related time=card insertion time or time of entry";
                case 1: return "End, related time=card withdrawal time or time of entry";
                case 2: return "Begin, related time manually entered (start time)";
                case 3: return "End, related time manually entered (end of work period)";
                case 4: return "Begin, related time assumed by VU";
                case 5: return "End, related time assumed by VU";
                default: return null;
            }
        }
    }
    class Address
    {
        public int codePage { get; set; }
        public string address { get; set; }
    }
    class TimeReal
    {
        public int timeSec { get; set; }

        public string ConvertToUTC()
        {
            int t = timeSec;
            DateTime baseDT = new DateTime(1970, 1, 1, 0, 0, 0);
            baseDT.AddSeconds(t);
            return baseDT.ToString("dd-MMM-yyyy HH:mm:ss");
        }
    }
    class BCDDate
    {
        byte[] encodedDate = new byte[3];

        public byte[] EncodedDate
        {
            get { return encodedDate; }
            set { encodedDate = value; }
        }
        
        public override string ToString()
        {
            return BitConverter.ToString(encodedDate);
        }
    }
    class BcdMonthYear
    {
        byte[] encodedDate = new byte[1];

        public byte[] EncodedDate
        {
            get { return encodedDate; }
            set { encodedDate = value; }
        }

        public override string ToString()
        {
            return "20 " + BitConverter.ToString(encodedDate)[1] + " - " + BitConverter.ToString(encodedDate)[0];
        }
    }
    class CardConsecutiveIndex
    {
        public char index { get; set; }
    }
    class Certificate
    {
        byte[] sign = new byte[127];

        public byte[] Sign
        {
            get { return sign; }
            set { sign = value; }
        }
        byte[] cndash = new byte[57];

        public byte[] Cndash
        {
            get { return cndash; }
            set { cndash = value; }
        }
        public CertificateAuthority certificateAuthorityReferenceerty { get; set; }
    }
    class CardSlotsStatus
    {
        public string CoDriverSlot { get; set; }
        public string DriverSlot { get; set; }

        public string getVerbose(string inp)
        {
            switch (inp)
            {
                case "0000": return "no card";
                case "0001": return "a driver card is inserted";
                case "0010": return "a workshop card is inserted";
                case "0011": return "a control card is inserted";
                case "0100": return "a company card is iserted";
                default: return null;
            }
        }
    }
    class CertificateRequestID
    {
        public int requestSerialNumber { get; set; }
        public BcdMonthYear requestMonthYear { get; set; }
        public byte crIdentifier { get; set; }
        public ManufacturerCode manufacturerCode { get; set; }

    }
    class CompanyActivityData
    {
        public int companyPointerNewestRecord { get; set; }
        public CompanyActivityRecord[] cRecs { get; set; }
    }
    class CompanyActivityRecord
    {
        public CompanyActivityType companyActivityType { get; set; }
        public TimeReal companyActivityTime { get; set; }
        public FullCardNumber cardNumberInformation { get; set; }
        public VehicleRegistrationIdentification vehicleRegistrationInformation { get; set; }
        public TimeReal downloadPeriodBegin { get; set; }
        public TimeReal downloadPeriodEnd { get; set; }
    }
    class CompanyActivityType
    {
        public int companyActivityType { get; set; }
        public string getVerboseActivityType()
        {
            switch (companyActivityType)
            {
                case 1: return "card downloading";
                case 2: return "VU downloading";
                case 3: return "VU lock-in";
                case 4: return "VU lock-out";
                default: return null;
            }
        }
    }
    class CompanyCardHolderIdentification
    {
        public Name companyName { get; set; }
        public Address companyAddress { get; set; }
        public Language cardHolderPreferredLanguage { get; set; }
    }
    class ControlCardApplicationIdentification
    {
        public EquipmentType typeOfTachographCardId;
        public int noOfControlActivityRecords;
        public byte[] cardStructureVersion = new byte[1];
    }

    class CompanyCardApplicationIdentification
    {
        public EquipmentType typeOfTachographCardId;
        public int noOfCompanyActivityRecords;
        public byte[] cardStructureVersion = new byte[1];
    }

    class EquipmentType
    {
        public int equipmentType { get; set; }
        public string getVerboseType()
        {
            switch (equipmentType)
            {
                case 0: return "Reserved";
                case 1: return "Driver Card";
                case 2: return "Workshop Card";
                case 3: return "Control Card";
                case 4: return "Company Card";
                case 5: return "Manufacturing Card";
                case 6: return "Vehicle Unit";
                case 7: return "Motion Sensor";
                default: return "RFU";
            }
        }
    }
    class EventFaultType
    {
        public byte eventFaultType { get; set; }
        public string getVerboseType()
        {
            if (eventFaultType >= 0x80 && eventFaultType <= 0xFF)
            {
                return "Manufacturer specific";
            }
            else
            {
                switch (eventFaultType)
                {
                    case 0x00: return "General events. No further details";
                    case 0x01: return "Insertion of a non-valid card";
                    case 0x02: return "Card conflict";
                    case 0x03: return "Time overlap";
                    case 0x04: return "Driving without an appropriate card";
                    case 0x05: return "Card insertion while driving";
                    case 0x06: return "Last card session not correctly closed";
                    case 0x07: return "Overspeeding";
                    case 0x08: return "Power supply interruption";
                    case 0x09: return "Motion data error";
                    case 0x10: return "Vehicle unit related security breach attempt events. No further details";
                    case 0x11: return "Motion sensor authentication failure";
                    case 0x12: return "Tachograph card authentication failure";
                    case 0x13: return "Unauthorised change of motion sensor";
                    case 0x14: return "Card data input integrity error";
                    case 0x15: return "Stored user data integrity error";
                    case 0x16: return "Internal data transfer error";
                    case 0x17: return "Unauthorised case opening";
                    case 0x18: return "Hardware sabotage";
                    case 0x20: return "Sensor related security breach attempt event. No further details";
                    case 0x21: return "Authentication failure";
                    case 0x22: return "Stored data integrity error";
                    case 0x23: return "Internal data transfer error";
                    case 0x24: return "Unauthorized case opening";
                    case 0x25: return "Hardware sabotage";
                    case 0x30: return "Recording equipment faults. No further details";
                    case 0x31: return "VU internal fault";
                    case 0x32: return "Priinter fault";
                    case 0x33: return "Display fault";
                    case 0x34: return "Downloading fault";
                    case 0x35: return "Sensor fault";
                    case 0x40: return "Card faults. No further details";
                    default: return "RFU";
                }
            }
        }
    }
    class EventFaultRecordPurpose
    {
        public byte eventFaultRecordPurpose { get; set; }
        public string getVerbosePurpose()
        {
            if (eventFaultRecordPurpose >= 0x80 & eventFaultRecordPurpose <= 0xFF)
            {
                return "Manufacturer specific";
            }
            else
            {
                switch (eventFaultRecordPurpose)
                {
                    case 0x00: return "One of the 10 most recent (or last) event or faults";
                    case 0x01: return "The longest event for one of the last 10 days of occurrence";
                    case 0x02: return "One of the 5 longest events over the last 365 days";
                    case 0x03: return "The last event for one of the last 10 days of occurrence";
                    case 0x04: return "The most serious event for one of the last 10 days of occurence";
                    case 0x05: return "One of the 5 most serious events over the last 365 days";
                    case 0x06: return "The first event or fault having occurred after the last calibration";
                    case 0x07: return "an active/on-going event or fault";
                    default: return "RFU";
                }
            }
        }
    }
    class ControlCardControlActivityData
    {
        public int controlPointerNewestRecord;
        public ControlActivityRecord[] conActRecs;

    }
    class ControlActivityRecord
    {
        public ControlType controlType { get; set; }
        public TimeReal controlTime { get; set; }
        public FullCardNumber controlledCardNumber { get; set; }
        public VehicleRegistrationIdentification controlledVehicleRegistration { get; set; }
        public TimeReal controlDownloadPeriodBegin { get; set; }
        public TimeReal controlDownloadPeriodEnd { get; set; }
    }
    class ControlCardHolderIdentification
    {
        public Name controlBodyName { get; set; }
        public Address controlBodyAddress { get; set; }
        public HolderName cardHolderName { get; set; }
        public Language cardHolderPreferredLanguage { get; set; }
    }
    class ControlType
    {
        public string controlType { get; set; }
        public string getVerboseControlType()
        {
            string s;
            if (controlType[0] == '1') { s = "Card downloaded during this control activity. "; } else { s = "Card not downloaded during this control activity. "; };
            if (controlType[1] == '1') { s += "VU downloaded during this control activity. "; } else { s += "VU not downloaded during this control activity. "; };
            if (controlType[2] == '1') { s += "Printing done during this control activity. "; } else { s += "No pringting done during this control activity. "; };
            if (controlType[3] == '1') { s += "Display used during this control activity. "; } else { s += "No display used during this control activity. "; };
            return s;
        }
    }
    class CurrentDateTime
    {
        public TimeReal currentDateTime { get; set; }
    }
    class DailyPresenceCounter
    {
        public int dailyPresenceCounter { get; set; }
    }
    class Datef
    {
        public string year { get; set; }
        public string month { get; set; }
        public string day { get; set; }
    }
    class Distance
    {
        public int distance;
    }
    class KeyIdentifier
    {
        public ExtendedSerialNumber extendedSerialNumber { get; set; }
        public CertificateRequestID certificateRequestID { get; set; }
        public CertificateAuthority certificateAuthorityKID { get; set; }

        public object getActiveChoice()
        {
            if (extendedSerialNumber != null) { return extendedSerialNumber.GetType(); }
            else if (certificateRequestID != null) { return certificateRequestID.GetType(); }
            else if (certificateAuthorityKID != null) { return certificateAuthorityKID.GetType(); }
            else { throw new ArgumentNullException("KeyIdentifier is Null!"); }
        }
    }
    class Language
    {
        char[] language = new char[1];

        public char[] PLanguage
        {
            get { return language; }
            set { language = value; }
        }
    }
    class CertificateHolderAuthorization
    {
        byte[] tachographApplicationId = new byte[5];

        public byte[] TachographApplicationId
        {
            get { return tachographApplicationId; }
            set { tachographApplicationId = value; }
        }
        public EquipmentType equipmentType { get; set; }


    }
    class CardNumber
    {
        public string driverIdentification { get; set; }
        public CardReplacementIndex cardReplacementIndex { get; set; }
        public CardRenewalIndex cardRenewalIndex { get; set; }
        //Should programmly change, which to use!!!
        public string ownerIdentification { get; set; }
        public CardConsecutiveIndex cardConsecutiveIndex { get; set; }
    }
    class ManualInputFlag
    {
        public int manualInputFlag { get; set; }

        public string getVerboseFlag()
        {
            switch (manualInputFlag)
            {
                case 0: return "noEntry";
                case 1: return "manualEntry";
                default: return null;
            }
        }
    }
    class FullCardNumber
    {
        public EquipmentType cardType { get; set; }
        public NationNumeric cardIssuingMemberState { get; set; }
        public CardNumber cardNumber { get; set; }

    }
    class HolderName
    {
        public Name holderSurname { get; set; }
        public Name holderFirstNames { get; set; }
    }
    class ManufacturerCode
    {
        public byte manufacturerCode { get; set; }
        public string getVerboseCode()
        {
            switch (manufacturerCode)
            {
                case 0x00: return "No information available";
                case 0x01: return "Reserved value";
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
                case 0x0F: return "Reserved for Future Use";
                case 0x10: return "ACTIA";
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17: return "Reserved for manufacturers which name begins with 'A'";
                case 0x18:
                case 0x19:
                case 0x1a:
                case 0x1b:
                case 0x1c:
                case 0x1d:
                case 0x1e:
                case 0x1f: return "Reserved for manufacturers which name begins with 'B'";
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27: return "Reserved for manufacturers which name begins with 'C'";
                case 0x28:
                case 0x29:
                case 0x2a:
                case 0x2b:
                case 0x2c:
                case 0x2d:
                case 0x2e:
                case 0x2f: return "Reserved for manufacturers which name begins with 'D'";
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                case 0x36:
                case 0x37: return "Reserved for manufacturers which name begins with 'E'";
                case 0x38:
                case 0x39:
                case 0x3a:
                case 0x3b:
                case 0x3c:
                case 0x3d:
                case 0x3e:
                case 0x3f: return "Reserved for manufacturers which name begins with 'F'";
                case 0x40: return "Giesecke & Devrient GmbH";
                case 0x41: return "GEM plus";
                case 0x42:
                case 0x43:
                case 0x44:
                case 0x45:
                case 0x46:
                case 0x47: return "Reserved for manufacturers which name begins with 'G'";
                case 0x48:
                case 0x49:
                case 0x4a:
                case 0x4b:
                case 0x4c:
                case 0x4d:
                case 0x4e:
                case 0x4f: return "Reserved for manufacturers which name begins with 'H'";
                case 0x50:
                case 0x51:
                case 0x52:
                case 0x53:
                case 0x54:
                case 0x55:
                case 0x56:
                case 0x57: return "Reserved for manufacturers which name begins with 'I'";
                case 0x58:
                case 0x59:
                case 0x5a:
                case 0x5b:
                case 0x5c:
                case 0x5d:
                case 0x5e:
                case 0x5f: return "Reserved for manufacturers which name begins with 'J'";
                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x66:
                case 0x67: return "Reserved for manufacturers which name begins with 'K'";
                case 0x68:
                case 0x69:
                case 0x6a:
                case 0x6b:
                case 0x6c:
                case 0x6d:
                case 0x6e:
                case 0x6f: return "Reserved for manufacturers which name begins with 'L'";
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x76:
                case 0x77: return "Reserved for manufacturers which name begins with 'M'";
                case 0x78:
                case 0x79:
                case 0x7a:
                case 0x7b:
                case 0x7c:
                case 0x7d:
                case 0x7e:
                case 0x7f: return "Reserved for manufacturers which name begins with 'N'";
                case 0x80: return "OSCARD";
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x86:
                case 0x87: return "Reserved for manufacturers which name begins with 'O'";
                case 0x88:
                case 0x89:
                case 0x8a:
                case 0x8b:
                case 0x8c:
                case 0x8d:
                case 0x8e:
                case 0x8f: return "Reserved for manufacturers which name begins with 'P'";
                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x96:
                case 0x97: return "Reserved for manufacturers which name begins with 'Q'";
                case 0x98:
                case 0x99:
                case 0x9a:
                case 0x9b:
                case 0x9c:
                case 0x9d:
                case 0x9e:
                case 0x9f: return "Reserved for manufacturers which name begins with 'R'";
                case 0xa0: return "SETEC";
                case 0xa1: return "SIEMENS VDO";
                case 0xa2: return "STONERIDGE";
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9: return "Reserved for manufacturers which name begins with 'S'";
                case 0xaa: return "TACHOCONTROL";
                case 0xab:
                case 0xac:
                case 0xad:
                case 0xae:
                case 0xaf: return "Reserved for manufacturers which name begins with 'T'";
                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7: return "Reserved for manufacturers which name begins with 'U'";
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf: return "Reserved for manufacturers which name begins with 'V'";
                case 0xc0:
                case 0xc1:
                case 0xc2:
                case 0xc3:
                case 0xc4:
                case 0xc5:
                case 0xc6:
                case 0xc7: return "Reserved for manufacturers which name begins with 'W'";
                case 0xc8:
                case 0xc9:
                case 0xca:
                case 0xcb:
                case 0xcc:
                case 0xcd:
                case 0xce:
                case 0xcf: return "Reserved for manufacturers which name begins with 'X'";
                case 0xd0:
                case 0xd1:
                case 0xd2:
                case 0xd3:
                case 0xd4:
                case 0xd5:
                case 0xd6:
                case 0xd7: return "Reserved for manufacturers which name begins with 'Y'";
                case 0xd8:
                case 0xd9:
                case 0xda:
                case 0xdb:
                case 0xdc:
                case 0xdd:
                case 0xde:
                case 0xdf: return "Reserved for manufacturers which name begins with 'Z'";
                default: return null;
            }
        }
    }
    class MemberStatePublicKey
    {
        public PublicKey memberStatePublicKey { get; set; }
    }
    class Name
    {
        public int codePage { get; set; }
        public string name { get; set; }
    }
    class NationAlpha
    {
        public string nationAlpha { get; set; }
        public string getVerboseAlpha()
        {
            switch (nationAlpha)
            {
                case "A": return " Austria";
                case "AL": return " Albania";
                case "ARM": return " Armenia";
                case "AND": return " Andorra";
                case "AZ": return " Azerbaijan";
                case "B": return " Belgium";
                case "BG": return " Bulgaria";
                case "BIH": return " Bosnia and Herzegovina";
                case "BY": return " Belarus";
                case "CH": return "  witzerland";
                case "CY": return " Cyprus";
                case "CZ": return " Czech Republic";
                case "D": return " Germany";
                case "DK": return " Denmark";
                case "E": return " Spain";
                case "EST": return " Estonia";
                case "F": return " France";
                case "FIN": return " Finland";
                case "FL": return " Liechtenstein";
                case "UK": return " United Kingdom, Alderney, Guernsey, Jersey, Isle of Man, Gibraltar";
                case "GE": return " Georgia";
                case "GR": return " Greece";
                case "H": return " Hungary";
                case "HR": return " Croatia";
                case "I": return " Italy";
                case "IRL": return " Ireland";
                case "IS": return " Iceland";
                case "KZ": return " Kazakhstan";
                case "L": return " Luxembourg";
                case "LT": return " Lithuania";
                case "LV": return " Latvia";
                case "M": return " Malta";
                case "MC": return " Monaco";
                case "MD": return " Republic of Moldova";
                case "MK": return " Macedonia";
                case "N": return " Norway";
                case "NL": return " Netherlands";
                case "P": return " Portugal";
                case "PL": return " Poland";
                case "RO": return " Romania";
                case "RSM": return " San Marino";
                case "RUS": return " Russia";
                case "S": return " Sweden";
                case "SK": return " Slovakia";
                case "SLO": return " Slovenia";
                case "TM": return " Turkmenistan";
                case "TR": return " Turkey";
                case "UA": return " Ukraine";
                case "V": return " Vatican City";
                case "YU": return " Yugoslavia";
                case "UNK": return " Unknown";
                case "EC": return " European Community";
                case "EUR": return " Rest of Europe";
                case "WLD": return " Rest of the world";
                default: return "No information available";
            }
        }
    }
    class NationNumeric
    {
        public int nationNumeric { get; set; }
        public string getVerboseNation()
        {
            switch (nationNumeric)
            {
                case 0x01: return " Austria";
                case 0x02: return " Albania";
                case 0x03: return " Armenia";
                case 0x04: return " Andorra";
                case 0x05: return " Azerbaijan";
                case 0x06: return " Belgium";
                case 0x07: return " Bulgaria";
                case 0x08: return " Bosnia and Herzegovina";
                case 0x09: return " Belarus";
                case 0x0a: return "  witzerland";
                case 0x0b: return " Cyprus";
                case 0x0c: return " Czech Republic";
                case 0x0d: return " Germany";
                case 0x0e: return " Denmark";
                case 0x0f: return " Spain";
                case 0x10: return " Estonia";
                case 0x11: return " France";
                case 0x12: return " Finland";
                case 0x13: return " Liechtenstein";
                case 0x14: return " Faeroe Islands";
                case 0x15: return " United Kingdom";
                case 0x16: return " Georgia";
                case 0x17: return " Greece";
                case 0x18: return " Hungary";
                case 0x19: return " Croatia";
                case 0x1a: return " Italy";
                case 0x1b: return " Ireland";
                case 0x1c: return " Iceland";
                case 0x1d: return " Kazakhstan";
                case 0x1e: return " Luxembourg";
                case 0x1f: return " Lithuania";
                case 0x20: return " Latvia";
                case 0x21: return " Malta";
                case 0x22: return " Monaco";
                case 0x23: return " Republic of Moldova";
                case 0x24: return " Macedonia";
                case 0x25: return " Norway";
                case 0x26: return " Netherlands";
                case 0x27: return " Portugal";
                case 0x28: return " Poland";
                case 0x29: return " Romania";
                case 0x2a: return " San Marino";
                case 0x2b: return " Russia";
                case 0x2c: return " Sweden";
                case 0x2d: return " Slovakia";
                case 0x2e: return " Slovenia";
                case 0x2f: return " Turkmenistan";
                case 0x30: return " Turkey";
                case 0x31: return " Ukraine";
                case 0x32: return " Vatican City";
                case 0x33: return " Yugoslavia";
                case 0x00: return " No information available";
                case 0xFD: return " European Community";
                case 0xFE: return " Rest of Europe";
                case 0xFF: return " Rest of the world";
                default: return "RFU";
            }
        }
    }
    class CardActivityDailyRecord
    {
        public int activityRecordPreviousLength;
        public int activityRecordLength;
        public TimeReal activityRecordDate;
        public DailyPresenceCounter activityPresenceCounter;
        public Distance activityDayDistance;
        public ActivityChangeInfo[] activityChangeInfo;
        ///ActivityChange should be counted from activityRecordLength.
        ///  (which is total in bytes of this record)

    }
    class CardPrivateKey
    {
        public RSAKeyPrivateExponent cardPrivateKey;
    }
    class CardPublicKey
    {
        public PublicKey cardPublicKey { get; set; }
    }
    class EuropeanPublicKey
    {
        public PublicKey europeanPublicKey { get; set; }
    }
    class CardRenewalIndex
    {
        public char cardRenewalIndex { get; set; }
    }
    class CardReplacementIndex
    {
        public char cardReplacementIndex { get; set; }
    }
    class CardEventRecord
    {
        public EventFaultType eventType { get; set; }
        public TimeReal eventBeginTime { get; set; }
        public TimeReal eventEndTime { get; set; }
        public VehicleRegistrationIdentification eventVehicleRegistration { get; set; }

    }
    class SpecificConditionRecord
    {
        public TimeReal entryTime { get; set; }
        public SpecificConditionType specificConditionType { get; set; }

    }
    class VuCompanyLocksRecord
    {
        public TimeReal lockInTime { get; set; }
        public TimeReal lockOutTime { get; set; }
        public Name companyName { get; set; }
        public Address companyAddress { get; set; }
        public FullCardNumber companyCardNumber { get; set; }

    }
    class VuCompanyLocksData
    {
        public int noOfLocks { get; set; }
        public VuCompanyLocksRecord[] vuCLR { get; set; }
    }
    class VuControlActivityRecord
    {
        public ControlType controlType { get; set; }
        public TimeReal controlTime { get; set; }
        public FullCardNumber controlCardNumber { get; set; }
        public TimeReal downloadPeriodBeginTime { get; set; }
        public TimeReal downloadPeriodEndTime { get; set; }

    }
    class VuControlActivityData
    {
        public int noOfControls;
        public VuControlActivityRecord[] vuCAR;
    }
    class VehicleRegistrationIdentification
    {
        public NationNumeric vehicleRegistrationNation;
        public VehicleRegistrationNumber vehicleRegistrationNumber;

    }
    class PlaceRecord
    {
        public TimeReal entryTime;
        public EntryTypeDailyWorkPeriod entryTypeDailyWorkPeriod;
        public NationNumeric dailyWorkPeriodCountry;
        public RegionNumeric dailyWorkPeriodRegion;
        public int vehicleOdometerValue;

    }
    class PreviousVehicleInfo
    {
        public VehicleRegistrationIdentification vehicleRegistrationIdentification;
        public TimeReal cardWithdrawalTime;
    }
    class CardVehicleRecord
    {
        public int vehicleOdometerBegin;
        public int vehicleOdometerEnd;
        public TimeReal vehicleFirstUse;
        public TimeReal vehicleLastUse;
        public VehicleRegistrationIdentification registration;
        public VuDataBlockCounter vuDataBlockCounter;

    }
    class VuDataBlockCounter
    {
        public int vuDataBlockCounter;
    }
    class RSAKeyPublicExponent
    {
        public string rsaKeyPublicExponent;
    }
    class ExtendedSerialNumber
    {
        public int serialNumber;
        public BcdMonthYear date;
        public EquipmentType equipmentType;
        public ManufacturerCode manufacturerCode;

    }
    class Block11Record
    {
        public FullCardNumber cardNumber;
        public TimeSpan time;
        public int sometimesDuration;
        public byte[] payloadData = new byte[29];
    }
    class VuCalibrationData
    {
        public int noOfVuCalibrationRecords;
        public VuCalibrationRecord[] vuCalRecs;
    }
    class VuCalibrationRecord
    {
        public CalibrationPurpose calibrationPurpose;
        public Name workshopName;
        public Address workshopAddress;
        public FullCardNumber workshopCardNumber;
        public TimeReal workshopCardExpiryDate;
        public VehicleIdentificationNumber vehicleIdentificationNumber;
        public VehicleRegistrationIdentification vehicleRegistrationIdentification;
        public int wVehicleCharacteristicConstant;
        public int kConstantOfRecordingEquipment;
        public int lTyreCircumference;
        public TyreSize tyreSize;
        public int authorisedSpeed;
        public int oldOdometerValue;
        public int newOdometerValue;
        public TimeReal oldTimeValue;
        public TimeReal newTimeValue;
        public TimeReal nextCalibrationDate;

    }
    class WorkshopCardHolderIdentification
    {
        public Name workshopName;
        public Address workshopAddress;
        public HolderName cardHolderName;
        public Language cardHolderPreferredLanguage;
    }
    class WorkshopCardPIN
    {
        public string workshopCardPIN;
    }
    class CalibrationPurpose
    {
        public int calibrationPurpose;
        public string calibrationPurposeIdentification()
        {
            switch (calibrationPurpose)
            {
                case 0: return "Reserved";
                case 1: return "Activation";
                case 2: return "First installation after activation";
                case 3: return "First installation in this vehicle";
                case 4: return "Periodic inspection";
                default: return null;
            }
        }
    }
    class VuTimeAdjustmentRecord
    {
        public TimeReal oldTimeValue;
        public TimeReal newTimeValue;
        public Name workshopName;
        public Address workshopAddress;
        public FullCardNumber workshopCardNumber;
    }
    class VuTimeAdjustmentData
    {
        public int noOfVuTimeAdjRecords;
        public VuTimeAdjustmentRecord[] vuTAR;
    }
    class CertificateAuthority
    {
        public NationNumeric nationNumeric;
        public NationAlpha nationAlpha;
        public int keySerialNumber;
        public byte[] additionalInfo = new byte[1];
        public int caIdentifier;

    }
    class VuCardIWRecord
    {
        public Name cardHolderName;
        public FullCardNumber cardNumber;
        public TimeReal cardExpiryDate;
        public TimeReal cardInsertionTime;
        public int vehicleOdometerValueAtInsertion;
        public CardSlotsStatus cardSlotNumber;
        public TimeReal cardWithdrawalTime;
        public int vehicleOdometerValueAtWithdrawal;
        public VehicleRegistrationIdentification previousVehicleRegistration;
        public TimeReal previousCardWithdrawalTime;
        public ManualInputFlag manualInputFlag;
    }
    class VuCardIWData
    {
        public int noOfIWRecords;
        public VuCardIWRecord[] VuCIWR;
    }
    class VuPlaceDailyWorkPeriodRecord
    {
        public FullCardNumber fullCardNumber;
        public PlaceRecord placeRecord;
    }
    class VuPlaceDailyWorkPeriodData
    {
        public int noOfPlaceRecords;
        public VuPlaceDailyWorkPeriodRecord[] vuPDWPR;
    }
    class VuPrivateKey
    {
        public RSAKeyPrivateExponent vuPublicKey;
    }
    class VuPublicKey
    {
        public PublicKey vuPublicKey;
    }
    class VuSoftwareIdentification
    {
        public string vuSoftwareVersion;
        public TimeReal vuSoftInstallationDate;
    }
    class VuSpecificConditionData
    {
        public int noOfSpecificConditionRecords;
        public SpecificConditionRecord[] specificConditionRecords;
    }
    class VuDetailedSpeedBlock
    {
        public TimeReal speedBlockBeginDate;
        public byte[] speedsPerSecond = new byte[60];
    }
    class VuDetailedSpeedData
    {
        public int noOfSpeedBlocks;
        public VuDetailedSpeedBlock[] vuDSB;
    }
    class CardSlots
    {
        public FullCardNumber cardNumberDriverSlotBegin;
        public FullCardNumber cardNumberCoDriverSlotBegin;
        public FullCardNumber cardNumberDriverSlotEnd;
        public FullCardNumber cardNumberCoDriverSlotEnd;
    }
    class VuFaultRecord
    {
        public EventFaultType eventType;
        public EventFaultRecordPurpose eventRecordPurpose;
        public TimeReal faultBeginTime;
        public TimeReal faultEndTime;
        public CardSlots cardSlots;

    }
    class VuFaultData
    {
        public int noOfVuFaults;
        public VuFaultRecord[] vuFR;
    }
    class VuEventData
    {
        public int noOfVuEvents;
        public VuEventRecord[] vuER;
    }
    class VuEventRecord
    {
        public EventFaultType eventType;
        public EventFaultRecordPurpose eventRecordPurpose;
        public TimeReal eventBeginTime;
        public TimeReal eventEndTime;
        public CardSlots cardSlots;
        public int similarEventsNumber;

    }
    class VuOverSpeedingControlData
    {
        public TimeReal lastOverspeedControlTime;
        public TimeReal firstOverspeedSince;
        public int numberOfOverspeedSince;
    }
    class VuOverSpeedingEventData
    {
        public int noOfVuOverSpeedingEvents;
        public VuOverSpeedingEventRecord[] vuOSER;
    }
    class VuOverSpeedingEventRecord
    {
        public EventFaultType eventType;
        public EventFaultRecordPurpose eventRecordPurpose;
        public TimeReal eventBeginTime;
        public TimeReal eventEndTime;
        public int maxSpeedValue;
        public int averageSpeedValue;
        public FullCardNumber cardNumberDriverSlotBegin;
        public int similarEventsNumber;

    }
    class VuIdentification
    {
        public Name vuManufacturerName;
        public Address vuManufacturerAddress;
        public string vuPartNumber;
        public ExtendedSerialNumber vuSerialNumber;
        public VuSoftwareIdentification vuSoftwareIdentification;
        public TimeReal vuManufacturingDate;
        public VuApprovalNumber vuApprovalNumber;
    }
    class WorkshopCardApplicationIdentification
    {
        public EquipmentType typeOfTachographId;
        public byte[] cardStructureVersion = new byte[1];
        public int noOfEventsPerType;
        public int noOfFaultsPerType;
        public int activityStructureLength;
        public int noOfCardVehicleRecords;
        public int noOfCardPlaceRecords;
        public int noOfCalibrationRecords;
    }
    class WorkshopCardCalibrationData
    {
        public int calibrationTotalNumber;
        public int calibrationPointerNewestRecord;
        public WorkshopCardCalibrationRecord[] wshopCCR;
    }
    class WorkshopCardCalibrationRecord
    {
        public CalibrationPurpose calibrationPurpose;
        public VehicleIdentificationNumber VIN;
        public VehicleRegistrationIdentification vehicleRegistration;
        public int wVehicleCharacteristicConstant;
        public int kConstantOfRecordingEquipment;
        public int lTyreCircumference;
        public TyreSize tyreSize;
        public int authorisedSpeed;
        public int oldOdometerValue;
        public int newOdometerValue;
        public TimeReal oldTimeValue;
        public TimeReal newTimeValue;
        public TimeReal nextCalibrationDate;
        public string vuPartNumber;
        public ExtendedSerialNumber VuSerialNumber;
        public SensorSerialNumber sensorSerialNumber;
    }
    class CertificateContent
    {
        public int certificateProfileIdentifier;
        public CertificateAuthority certificateAuthorityReference;
        public CertificateHolderAuthorization certificateHolderAuthorization;
        public TimeReal endOfValidity;
        public KeyIdentifier certificateHolderReference;
        public PublicKey rsaPublicKey;
    }
    class PlainCertificate
    {
        public CertificateAuthority keyIdentifier;
        public PublicKey rsaPublicKey;
    }
    class PublicKey
    {
        public RSAKeyModulus rsaKeyModulus;
        public RSAKeyPublicExponent rsaKeyPublicExponent;
    }
    class RegionAlpha
    {
        public string regionAlpha;
        public string getVerboseAlpha()
        {
            switch (regionAlpha)
            {
                case "AN": return " Andalucia";
                case "AR": return " Aragon";
                case "AST": return " Asturias";
                case "C": return " Cantabria";
                case "CAT": return " Cataluna";
                case "CL": return " Castilla-Leon";
                case "CM": return " Castilla-La-Mancha";
                case "CV": return " Valencia";
                case "EXT": return " Extremadura";
                case "G": return " Galicia";
                case "IB": return " Baleares";
                case "IC": return " Canarias";
                case "LR": return " La Rioja";
                case "M": return " Madrid";
                case "MU": return " Murcia";
                case "NA": return " Navarra";
                case "PV": return " Pais Vasco";
                default: return "No information available";
            }
        }
    }
    class RegionNumeric
    {
        public int regionNumeric;
        public string getVerboseNation()
        {
            switch (regionNumeric)
            {
                case 0x01: return " Andalucia";
                case 0x02: return " Aragon";
                case 0x03: return " Asturias";
                case 0x04: return " Cantabria";
                case 0x05: return " Cataluna";
                case 0x06: return " Castilla-Leon";
                case 0x07: return " Castilla-La-Mancha";
                case 0x08: return " Valencia";
                case 0x09: return " Extremadura";
                case 0x0a: return " Galicia";
                case 0x0b: return " Baleares";
                case 0x0c: return " Canarias";
                case 0x0d: return " La Rioja";
                case 0x0e: return " Madrid";
                case 0x0f: return " Murcia";
                case 0x10: return " Navarra";
                case 0x11: return " Pais Vasco";
                default: return " No information available";
            }
        }
    }
    class RSAKeyModulus
    {
        public string rsaKeyModulus;
    }
    class RSAKeyPrivateExponent
    {
        public string rsaKeyPrivateExponent;
    }
    class SensorApprovalNumber
    {
        public string sensorApprovalNumber;
    }
    class SensorIdentification
    {
        public SensorSerialNumber sensorSN;
        public SensorApprovalNumber sensorAN;
        public SensorSCIdentifier sensorSCI;
        public SensorOSIdentifier sensorOSI;
    }
    class SensorInstallation
    {
        public SensorPairingDate sensorPairingDateFirst;
        public VuApprovalNumber firstVuAN;
        public ExtendedSerialNumber firstVuSN;
        public SensorPairingDate sensorPairingDateCurrent;
        public VuApprovalNumber currentVuAN;
        public ExtendedSerialNumber currentVuSN;
    }
    class SensorInstallationSecData
    {
        public TDesSessionKey sensorInstallationSecData;
    }
    class SensorOSIdentifier
    {
        public string sensorOSIdentifier;
    }
    class SensorPaired
    {
        public SensorSerialNumber sensorSN;
        public SensorApprovalNumber sensorAN;
        public SensorPairingDate sensorPairingDateFirst;
    }
    class SensorPairingDate
    {
        public TimeReal sensorPairingDate;
    }
    class SensorSerialNumber
    {
        public ExtendedSerialNumber sensorSN;
    }
    class SensorSCIdentifier
    {
        public string sensorSCI;
    }
    class Signature
    {
        public string signature;
    }
    class SimilarEventsNumber
    {
        public int similarEventsNumber;
    }
    class SpecificConditionType
    {
        public byte specificConditionType;
        public string getVerboseType()
        {
            switch (specificConditionType)
            {
                default: return "RFU";
                case 0x01: return "Out of scope - Begin";
                case 0x02: return "Out of scope - End";
                case 0x03: return "Ferry/Train crossing";
            }
        }
    }
    class TDesSessionKey
    {
        public string tDesKeyA;
        public string tDesKeyB;
    }
    class TyreSize
    {
        public string tyreSize;
    }
    class VehicleIdentificationNumber
    {
        public string vehicleIdentificationNumber;
    }
    class VehicleRegistrationNumber
    {
        public int codePage;
        public string vehicleRegNumber;
    }
    /// <summary>
    /// Here ends type definition Block;
    /// 
    /// Here begins card block
    /// </summary>
    class DriverCardApplicationIdentification
    {
        public EquipmentType typeOfTachographCardId;
        public byte[] cardStructureVersion = new byte[1];
        public int noOfEventsPerType;
        public int noOfFaultsPerType;
        public byte[] activityStructureLength = new byte[1];
        public int noOfCardVehicleRecords;
        public int noOfCardPlaceRecords;


    }
    class DriverCardHolderIdentification
    {
        public HolderName cardHolderName;
        public BCDDate cardHolderBirthDate;
        public Language cardHolderPreferredLanguage;
    }
    class LastCardDownload
    {
        public TimeReal lastCardDownload;

        public override string ToString()
        {
            return lastCardDownload.ConvertToUTC();
        }
    }
    class CardDrivingLicenseInformation
    {
        public Name drivingLicenseIssuingAuthoruty;
        public NationNumeric drivingLicenseIssuingNation;
        public string drivingLicenseNumber;
    }
    class CardIdentification
    {
        public NationNumeric cardIssuingMemberState;
        public string cardNumber;
        public Name cardIssuingAuthorityName;
        public TimeReal cardIssueDate;
        public TimeReal cardValidityBegin;
        public TimeReal cardExpiryDate;
    }
    class CardCurrentUse
    {
        public TimeReal sessionOpenTime;
        public VehicleRegistrationIdentification sessionOpenVehicle;
    }
    class CardChipIdentification
    {
        public byte[] icSerialNumber = new byte[3];
        public byte[] icManufacturingReference = new byte[3];
    }
    class CardIccIdentification
    {
        public int clockStop;
        public ExtendedSerialNumber cardExtendedSerialNumber;
        public string cardApprovalNumber;
        public byte cardPesonaliserID;
        public byte[] embedderIcAssemblerId = new byte[4];
        public byte[] icIdentifier = new byte[1];
    }
    class CardControlActivityDataRecord
    {
        public ControlType controlType;
        public TimeReal controlTime;
        public FullCardNumber controlCardNumber;
        public VehicleRegistrationIdentification controlVehicleRegistration;
        public TimeReal controlDownloadPeriodBegin;
        public TimeReal controlDownloadPeriodEnd;
    }
    class CardDriverActivity
    {
        public int activityPointerOldestRecord;
        public int activityPointerNewestRecord;
        public byte[] cyclicData;

        public CardActivityDailyRecord[] cardActivityDailyRecord;

        public void CyclicDataParser(int NumberOfCycles)
        {
            cardActivityDailyRecord = new CardActivityDailyRecord[NumberOfCycles - 1];
            int pos = 0;
            byte[] t;
            for (int i = 0; i < NumberOfCycles; i++)
            {
                t = new byte[1];
                Array.Copy(cyclicData, pos, t, 0, 2);
                cardActivityDailyRecord[i].activityRecordPreviousLength = BitConverter.ToInt32(t, 0);
                pos += 2;

                Array.Copy(cyclicData, pos, t, 0, 2);
                cardActivityDailyRecord[i].activityRecordLength = BitConverter.ToInt32(t, 0);
                pos += 2;

                t = new byte[3];
                Array.Copy(cyclicData, pos, t, 0, 4);
                cardActivityDailyRecord[i].activityRecordDate.timeSec = BitConverter.ToInt32(t, 0);
                pos += 4;

                t = new byte[1];
                Array.Copy(cyclicData, pos, t, 0, 2);
                cardActivityDailyRecord[i].activityPresenceCounter.dailyPresenceCounter = BitConverter.ToInt32(t, 0);
                pos += 2;

                Array.Copy(cyclicData, pos, t, 0, 2);
                cardActivityDailyRecord[i].activityDayDistance.distance = BitConverter.ToInt32(t, 0);
                pos += 2;

                int NumberOfThings = (int)((cardActivityDailyRecord[i].activityRecordLength - 12) / 2);
                cardActivityDailyRecord[i].activityChangeInfo = new ActivityChangeInfo[NumberOfThings - 1];

                for (int j = 0; i < NumberOfThings; i++)
                {
                    Array.Copy(cyclicData, pos, cardActivityDailyRecord[i].activityChangeInfo[j].RAW, 0, 2);
                    pos += 2;
                    cardActivityDailyRecord[i].activityChangeInfo[j].ParseIT();
                }

            }
        }
    }
    class CardEventData
    {
        public CardEventRecords[] ceRecs = new CardEventRecords[5];
    }
    class CardEventRecords
    {
        public CardEventRecord[] ceRec;
    }
    class CardFaultData
    {
        public CardFaultRecords[] cardFaultRecords = new CardFaultRecords[1];
    }
    class CardFaultRecords
    {
        public CardEventRecord[] cfRec;
    }
    class SpecificConditions
    {
        public SpecificConditionRecord[] scRec = new SpecificConditionRecord[55];
    }
    class MemberStateCertificate
    {
        public Certificate certificate;
    }
    class CardCertificate
    {
        public Certificate certificate;
    }
    class CardPlaceDailyWorkPeriod
    {
        public int placePointerNewestRecord;
        public PlaceRecord[] plRecs;
    }
    class CardVehiclesUsed
    {
        public int vehiclePointerNewestRecord;
        public CardVehicleRecord[] cVehRecs;
    }
    /// <summary>
    /// Here ends CardBlocks
    /// 
    /// Here happens VuBlock
    /// </summary>
    class VuActivityDailyData
    {
        public int noOfActivityChanges;
        public ActivityChangeInfo activityChangeInfos;
    }
    class VuOverview
    {
        public Certificate memberStateCertificate;
        public Certificate vuCertificate;
        public string vehicleIdentificationNumber;
        public VehicleRegistrationIdentification vehicleRegistrationIdentification;
        public TimeReal currentDateTime;
        public VuDownloadablePeriod vuDownloadablePeriod;
        public CardSlotsStatus cardSlotsStatus;
        public TimeReal downloadingTime;
        public FullCardNumber cardNumber;
        public string companyOrWorkshopName;
        public VuCompanyLocksRecord[] cLocksRec;
        public int cLocksRecCNT;
        public VuControlActivityRecord[] cActivRec;
        public int cActivRecCNT;
    }
    class VuDownloadablePeriod
    {
        public TimeReal minDownloadableTime;
        public TimeReal maxDownloadableTime;
    }
    class VuDownloadActivityData
    {
        public TimeReal downloadingTime;
        public FullCardNumber fullCardNumber;
        public Name companyOrWorkshopName;
    }
    class VuTechnical
    {
        public string vuManufacturerName;
        public string vuManufacturerAddress;
        public string vuPartNumber;
        public ExtendedSerialNumber vuSerialNumber;
        public string vuSoftwareVersion;
        public TimeReal vuSoftInstallationDate;
        public TimeReal vuManufacturingDate;
        public VuApprovalNumber vuApprovalNumber;
        public ExtendedSerialNumber sensorSerialNumber;
        public string sensorApprovalNumber;
        public TimeReal sensorPairingDateFirst;
        public VuCalibrationRecord[] calRecs;
        public int calRecsCNT;
    }
    class VuApprovalNumber
    {
        public string vuApprovalNumber;
    }
    class VuCardData
    {
        //should be saved to a separate file;
    }
    class Block11
    {
        public byte[] header = new byte[14];
        public Block11Record[] bl11Recs;
        public int bl11RecsCNT;
    }
    class Block13
    {
        public byte[] header = new byte[29];
        public Block11Record[] bl13Recs;
        public int bl13RecsCNT;
    }
    class VuEventsFaults
    {
        public VuFaultRecord[] flRecs;
        public int flRecsCNT;
        public VuEventRecord[] evRecs;
        public int evRecsCNT;
        public TimeReal lastOverspeedControlTime;
        public TimeReal firstOverspeedSince;
        public int numberOfOverspeedSince;
        public VuOverSpeedingEventRecord[] ovSpdRecs;
        public int ovSpdRecsCNT;
        public VuTimeAdjustmentRecord tmAdjRecs;
        public int tmAdjRecsCNT;

    }
    class VuActivities
    {
        public TimeReal timeReal;
        public int odometerValueMidnight;
        public VuCardIWRecord[] cIWRecs;
        public int cIWRecsCNT;
        public ActivityChangeInfo[] acChInfo;
        public int acChInfoCNT;
        public VuPlaceDailyWorkPeriodRecord[] dWPRecs;
        public int dWPRecsCNT;
        public SpecificConditionRecord[] spCondRecs;
        public int spCondRecsCNT;
    }

    class SpecificConditionRecords
    {
        public SpecificConditionRecord[] scRec;
    }
    class NoOfCalibrationsSinceDownload
    {
        public int noOfCalibrationsSinceDownload;
    }

    class Library
    {
        public static int CarretPositionUniversal = 0;

        public void FileOpenReadDecode(string FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            CardIccIdentification cardICCI = new CardIccIdentification();
            CardIdentifier(fs, cardICCI);
            CardChipIdentification cardCI = new CardChipIdentification();
            CardChipIdentification(fs, cardCI);
            int Detect = fs.ReadByte();
            switch (Detect)
            {
                case 1:
                    {
                        DriverCardApplicationIdentification DCAI = new DriverCardApplicationIdentification();
                        CardCertificate CC = new CardCertificate();
                        MemberStateCertificate MSC = new MemberStateCertificate();
                        CardIdentification CI = new CardIdentification();
                        DriverCardHolderIdentification DCHI = new DriverCardHolderIdentification();
                        LastCardDownload LCD = new LastCardDownload();
                        CardDrivingLicenseInformation CDLI = new CardDrivingLicenseInformation();
                        CardEventData CED = new CardEventData();
                        CardFaultData CFD = new CardFaultData();
                        CardDriverActivity CDA = new CardDriverActivity();
                        CardVehiclesUsed CVU = new CardVehiclesUsed();
                        CardPlaceDailyWorkPeriod CPDWP = new CardPlaceDailyWorkPeriod();
                        CardCurrentUse CCU = new CardCurrentUse();
                        CardControlActivityDataRecord CCADR = new CardControlActivityDataRecord();
                        SpecificConditionRecords SCR = new SpecificConditionRecords();
                        DriverCardInput(fs, DCAI, CC, MSC, CI, DCHI, LCD, CDLI, CED, CFD, CDA, CVU, CPDWP, CCU, CCADR, SCR);
                        //TODO: Output designer;
                        break;
                    }
                case 2:
                    {
                        WorkshopCardApplicationIdentification WCAI = new WorkshopCardApplicationIdentification();
                        CardCertificate CC = new CardCertificate();
                        MemberStateCertificate MSC = new MemberStateCertificate();
                        CardIdentification CI = new CardIdentification();
                        WorkshopCardHolderIdentification WCHI = new WorkshopCardHolderIdentification();
                        NoOfCalibrationsSinceDownload NOCSD = new NoOfCalibrationsSinceDownload();
                        WorkshopCardCalibrationData WCCD = new WorkshopCardCalibrationData();
                        SensorInstallationSecData SISD = new SensorInstallationSecData();
                        CardEventData CED = new CardEventData();
                        CardFaultData CFD = new CardFaultData();
                        CardDriverActivity CDA = new CardDriverActivity();
                        CardVehiclesUsed CVU = new CardVehiclesUsed();
                        CardPlaceDailyWorkPeriod CPDWP = new CardPlaceDailyWorkPeriod();
                        CardCurrentUse CCU = new CardCurrentUse();
                        CardControlActivityDataRecord CCADR = new CardControlActivityDataRecord();
                        SpecificConditionRecords SCR = new SpecificConditionRecords();
                        WorkshopCardInput(fs, WCAI, CC, MSC, CI, WCHI, NOCSD, WCCD, SISD, CED, CFD, CDA, CVU, CPDWP, CCU, CCADR, SCR);
                        //TODO:Design output
                        break;
                    }
                case 3:
                    {
                        ControlCardApplicationIdentification CCAI = new ControlCardApplicationIdentification();
                        CardCertificate CC = new CardCertificate();
                        MemberStateCertificate MSC = new MemberStateCertificate();
                        CardIdentification CI = new CardIdentification();
                        ControlCardHolderIdentification CCHI = new ControlCardHolderIdentification();
                        ControlCardControlActivityData CCCAD = new ControlCardControlActivityData();
                        ControlCardInput(fs, CCAI, CC, MSC, CI, CCHI, CCCAD);
                        //TODO:Output
                        break;
                    }
                case 4:
                    {
                        CompanyCardApplicationIdentification CCAI = new CompanyCardApplicationIdentification();
                        CardCertificate CC = new CardCertificate();
                        MemberStateCertificate MSC = new MemberStateCertificate();
                        CardIdentification CI = new CardIdentification();
                        CompanyCardHolderIdentification CCHI = new CompanyCardHolderIdentification();
                        CompanyActivityData CAD = new CompanyActivityData();
                        CompanyCardInput(fs, CCAI, CC, MSC, CI, CCHI, CAD);
                        //TODO:Output
                        break;
                    }
                case 5:
                    {

                        break;
                    }
                default: throw new FileLoadException("The specified filename doesn't contain a tachograph card info or something went wrong...", FileName);
            }
        }


        public void CardIdentifier(FileStream fs, CardIccIdentification cardICCI)
        {
            cardICCI.clockStop = fs.ReadByte();
            CarretPositionUniversal++;
            byte[] temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CarretPositionUniversal += 4;
            cardICCI.cardExtendedSerialNumber.serialNumber = BitConverter.ToInt32(temp, 0);
            fs.Read(cardICCI.cardExtendedSerialNumber.date.EncodedDate, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            cardICCI.cardExtendedSerialNumber.equipmentType.equipmentType = fs.ReadByte();
            CarretPositionUniversal++;
            cardICCI.cardExtendedSerialNumber.manufacturerCode.manufacturerCode = (byte)fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[7];
            fs.Read(temp, CarretPositionUniversal, 8);
            CarretPositionUniversal += 8;
            cardICCI.cardApprovalNumber = BitConverter.ToString(temp);
            cardICCI.cardPesonaliserID = BitConverter.GetBytes(fs.ReadByte())[0];
            CarretPositionUniversal++;
            fs.Read(cardICCI.embedderIcAssemblerId, CarretPositionUniversal, 5);
            CarretPositionUniversal += 5;
            fs.Read(cardICCI.icIdentifier, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
        } //Pos= +25 = TRUE

        public void CardChipIdentification(FileStream fs, CardChipIdentification cardCI)
        {
            fs.Read(cardCI.icSerialNumber, CarretPositionUniversal, 4);
            CarretPositionUniversal += 4;
            fs.Read(cardCI.icManufacturingReference, CarretPositionUniversal, 4);
            CarretPositionUniversal += 4;
        } //Pos= +8 = TRUE

        public void DriverCardInput(FileStream fs, DriverCardApplicationIdentification DCAI, CardCertificate CC, MemberStateCertificate MSC, CardIdentification CI, DriverCardHolderIdentification DCHI, LastCardDownload LCD, CardDrivingLicenseInformation CDLI, CardEventData CED, CardFaultData CFD, CardDriverActivity CDA, CardVehiclesUsed CVU, CardPlaceDailyWorkPeriod CPDWP, CardCurrentUse CCU, CardControlActivityDataRecord CCADR, SpecificConditionRecords SCR)
        {
            // ***DriverCardIdentification*** Pos=+10=TRUE
            DCAI.typeOfTachographCardId.equipmentType = 1;
            CarretPositionUniversal++;
            fs.Read(DCAI.cardStructureVersion, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            DCAI.noOfEventsPerType = fs.ReadByte();
            DCAI.noOfFaultsPerType = fs.ReadByte();
            CarretPositionUniversal += 2;
            fs.Read(DCAI.activityStructureLength, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            byte[] temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            DCAI.noOfCardVehicleRecords = BitConverter.ToInt32(temp, 0);
            DCAI.noOfCardPlaceRecords = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Certificate*** Pos=+194=TRUE
            fs.Read(CC.certificate.Sign, CarretPositionUniversal, 128);
            CarretPositionUniversal += 128;
            fs.Read(CC.certificate.Cndash, CarretPositionUniversal, 58);
            CarretPositionUniversal += 58;
            CC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[2];
            fs.Read(temp, CarretPositionUniversal, 3);
            CC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            CC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(CC.certificate.certificateAuthorityReferenceerty.additionalInfo, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            CC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Member State Certificate*** Pos=+194=TRUE
            fs.Read(MSC.certificate.Sign, CarretPositionUniversal, 128);
            CarretPositionUniversal += 128;
            fs.Read(MSC.certificate.Cndash, CarretPositionUniversal, 58);
            CarretPositionUniversal += 58;
            MSC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[2];
            fs.Read(temp, CarretPositionUniversal, 3);
            MSC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            MSC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(MSC.certificate.certificateAuthorityReferenceerty.additionalInfo, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            MSC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Identification*** Pos=+65=TRUE
            CI.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[15];
            fs.Read(temp, CarretPositionUniversal, 16);
            CarretPositionUniversal += 16;
            CI.cardNumber = BitConverter.ToString(temp);

            CI.cardIssuingAuthorityName.codePage = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[34];
            fs.Read(temp, CarretPositionUniversal, 35);
            CI.cardIssuingAuthorityName.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardIssueDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardValidityBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardExpiryDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            // *** Driver Card Holder Identification*** Pos=+78=TRUE
            DCHI.cardHolderName.holderSurname.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[34];
            fs.Read(temp, CarretPositionUniversal, 35);
            DCHI.cardHolderName.holderSurname.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;
            DCHI.cardHolderName.holderFirstNames.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 35);
            DCHI.cardHolderName.holderFirstNames.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;
            temp = new byte[3];
            fs.Read(DCHI.cardHolderBirthDate.EncodedDate, CarretPositionUniversal, 4);
            CarretPositionUniversal += 4;
            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            DCHI.cardHolderPreferredLanguage.PLanguage = BitConverter.ToString(temp).ToCharArray();
            CarretPositionUniversal += 2;

            //*** Last Card Download *** Pos=+4=TRUE
            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            LCD.lastCardDownload.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            //*** Events Reader ***
            for (int i = 0; i <= 5; i++)
            {
                CED.ceRecs[i].ceRec = new CardEventRecord[DCAI.noOfEventsPerType];
            }

            for (int i = 0; i <= 5; i++)
            {
                for (int j = 0; j <= DCAI.noOfEventsPerType; j++) //Pos=+24 each = TRUE
                {
                    CED.ceRecs[i].ceRec[j].eventType.eventFaultType = (byte)fs.ReadByte();
                    CarretPositionUniversal++;

                    temp = new byte[3];
                    fs.Read(temp, CarretPositionUniversal, 4);
                    CED.ceRecs[i].ceRec[j].eventBeginTime.timeSec = BitConverter.ToInt32(temp, 0);
                    CarretPositionUniversal += 4;

                    fs.Read(temp, CarretPositionUniversal, 4);
                    CED.ceRecs[i].ceRec[j].eventEndTime.timeSec = BitConverter.ToInt32(temp, 0);
                    CarretPositionUniversal += 4;

                    CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                    CarretPositionUniversal++;

                    temp = new byte[13];
                    fs.Read(temp, CarretPositionUniversal, 14);
                    CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage = (int)temp[0];
                    CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp, 1);
                    CarretPositionUniversal += 14;

                }
            }

            //*** Faults Reader ***

            for (int i = 0; i <= 5; i++)
            {
                CFD.cardFaultRecords[i].cfRec = new CardEventRecord[DCAI.noOfFaultsPerType];
            }

            for (int i = 0; i <= 5; i++)
            {
                for (int j = 0; j <= DCAI.noOfFaultsPerType; j++) //Pos+=24 each = TRUE
                {
                    CFD.cardFaultRecords[i].cfRec[j].eventType.eventFaultType = (byte)fs.ReadByte();
                    CarretPositionUniversal++;

                    temp = new byte[3];
                    fs.Read(temp, CarretPositionUniversal, 4);
                    CFD.cardFaultRecords[i].cfRec[j].eventBeginTime.timeSec = BitConverter.ToInt32(temp, 0);
                    CarretPositionUniversal += 4;

                    fs.Read(temp, CarretPositionUniversal, 4);
                    CFD.cardFaultRecords[i].cfRec[j].eventEndTime.timeSec = BitConverter.ToInt32(temp, 0);
                    CarretPositionUniversal += 4;

                    CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                    CarretPositionUniversal++;

                    temp = new byte[13];
                    fs.Read(temp, CarretPositionUniversal, 14);
                    CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage = (int)temp[0];
                    CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp, 1);
                    CarretPositionUniversal += 14;

                }
            }

            //*** Daily Records *** 
            int CyclicLongitude = BitConverter.ToInt32(DCAI.activityStructureLength, 0);
            CDA.cyclicData = new byte[CyclicLongitude - 1];

            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CDA.activityPointerOldestRecord = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            fs.Read(temp, CarretPositionUniversal, 2);
            CDA.activityPointerNewestRecord = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            fs.Read(CDA.cyclicData, CarretPositionUniversal, CyclicLongitude);
            CarretPositionUniversal += CyclicLongitude;

            CDA.CyclicDataParser((int)(CyclicLongitude / 5544));

            //*** Vehicle Records ***
            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CVU.vehiclePointerNewestRecord = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            CVU.cVehRecs = new CardVehicleRecord[DCAI.noOfCardVehicleRecords - 1];

            for (int i = 0; i < DCAI.noOfCardVehicleRecords; i++) //Pos+=31 = TRUE
            {

                temp = new byte[2];
                fs.Read(temp, CarretPositionUniversal, 3);
                CarretPositionUniversal += 3;
                CVU.cVehRecs[i].vehicleOdometerBegin = BitConverter.ToInt32(temp, 0);

                fs.Read(temp, CarretPositionUniversal, 3);
                CarretPositionUniversal += 3;
                CVU.cVehRecs[i].vehicleOdometerEnd = BitConverter.ToInt32(temp, 0);

                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CarretPositionUniversal += 4;
                CVU.cVehRecs[i].vehicleFirstUse.timeSec = BitConverter.ToInt32(temp, 0);

                fs.Read(temp, CarretPositionUniversal, 4);
                CarretPositionUniversal += 4;
                CVU.cVehRecs[i].vehicleLastUse.timeSec = BitConverter.ToInt32(temp, 0);

                CVU.cVehRecs[i].registration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CVU.cVehRecs[i].registration.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[12];
                fs.Read(temp, CarretPositionUniversal, 13);
                CarretPositionUniversal += 13;
                CVU.cVehRecs[i].registration.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp, 0);

                temp = new byte[1];
                fs.Read(temp, CarretPositionUniversal, 2);
                CarretPositionUniversal += 2;
                CVU.cVehRecs[i].vuDataBlockCounter.vuDataBlockCounter = BitConverter.ToInt32(temp, 0);
            }

            //*** Places *** 
            CPDWP.placePointerNewestRecord = fs.ReadByte();
            CarretPositionUniversal++;

            CPDWP.plRecs = new PlaceRecord[DCAI.noOfCardPlaceRecords - 1];

            for (int i = 0; i < DCAI.noOfCardPlaceRecords; i++) //Pos+=10 = TRUE
            {
                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CarretPositionUniversal += 4;
                CPDWP.plRecs[i].entryTime.timeSec = BitConverter.ToInt32(temp, 0);

                CPDWP.plRecs[i].entryTypeDailyWorkPeriod.entryTypeDailyWorkPeriod = fs.ReadByte();
                CarretPositionUniversal++;

                CPDWP.plRecs[i].dailyWorkPeriodCountry.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CPDWP.plRecs[i].dailyWorkPeriodRegion.regionNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[2];
                fs.Read(temp, CarretPositionUniversal, 3);
                CarretPositionUniversal += 3;
                CPDWP.plRecs[i].vehicleOdometerValue = BitConverter.ToInt32(temp, 0);
            }

            //*** Current usage *** Pos=+19=TRUE
            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CCU.sessionOpenTime.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;
            CCU.sessionOpenVehicle.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[13];
            fs.Read(temp, CarretPositionUniversal, 14);
            CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage = (int)temp[0];
            CCU.sessionOpenVehicle.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp, 1);
            CarretPositionUniversal += 14;

            //*** Control Activity Record *** Pos=+46=38?=
            byte t;
            t = (byte)fs.ReadByte();
            CCADR.controlType.controlType = t.ToString();
            CarretPositionUniversal++;
            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CCADR.controlTime.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;
            CCADR.controlCardNumber.cardType.equipmentType = fs.ReadByte();
            CCADR.controlCardNumber.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal += 2;
            temp = new byte[15];
            fs.Read(temp, CarretPositionUniversal, 16);
            CarretPositionUniversal += 16;
            CCADR.controlCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex = (char)temp[15];
            CCADR.controlCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex = (char)temp[14];
            byte[] tem = new byte[13];
            Array.Copy(temp, 0, tem, 0, 14);
            CCADR.controlCardNumber.cardNumber.driverIdentification = BitConverter.ToString(tem);

            CCADR.controlVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(tem, CarretPositionUniversal, 14);
            CarretPositionUniversal += 14;
            CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage = (int)tem[0];
            CCADR.controlVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber = tem.ToString();

            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CCADR.controlDownloadPeriodBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CCADR.controlDownloadPeriodEnd.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            //*** Specific Conditions *** Pos+=280=TRUE
            SCR.scRec = new SpecificConditionRecord[55];
            for (int i = 0; i < 56; i++)
            {
                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CarretPositionUniversal += 4;
                SCR.scRec[i].entryTime.timeSec = BitConverter.ToInt32(temp, 0);

                SCR.scRec[i].specificConditionType.specificConditionType = (byte)fs.ReadByte();
                CarretPositionUniversal++;
            }

        }

        public void WorkshopCardInput(FileStream fs, WorkshopCardApplicationIdentification WCAI, CardCertificate CC, MemberStateCertificate MSC, CardIdentification CI, WorkshopCardHolderIdentification WCHI, NoOfCalibrationsSinceDownload NOCSD, WorkshopCardCalibrationData WCCD, SensorInstallationSecData SISD, CardEventData CED, CardFaultData CFD, CardDriverActivity CDA, CardVehiclesUsed CVU, CardPlaceDailyWorkPeriod CPDWP, CardCurrentUse CCU, CardControlActivityDataRecord CCADR, SpecificConditionRecords SCR)
        {
            // ***WorkshopCardIdentification*** Pos=+11=TRUE
            WCAI.typeOfTachographId.equipmentType = 2;
            CarretPositionUniversal++;
            fs.Read(WCAI.cardStructureVersion, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            WCAI.noOfEventsPerType = fs.ReadByte();
            WCAI.noOfFaultsPerType = fs.ReadByte();
            CarretPositionUniversal += 2;
            byte[] temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            WCAI.activityStructureLength = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;
            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            WCAI.noOfCardVehicleRecords = BitConverter.ToInt32(temp, 0);
            WCAI.noOfCardPlaceRecords = fs.ReadByte();
            CarretPositionUniversal++;
            WCAI.noOfCalibrationRecords = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Certificate*** Pos=+194=TRUE
            fs.Read(CC.certificate.Sign, CarretPositionUniversal, 128);
            CarretPositionUniversal += 128;
            fs.Read(CC.certificate.Cndash, CarretPositionUniversal, 58);
            CarretPositionUniversal += 58;
            CC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            temp = new byte[2];
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 3);
            CC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            CC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(CC.certificate.certificateAuthorityReferenceerty.additionalInfo, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            CC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Member State Certificate***
            fs.Read(MSC.certificate.Sign, CarretPositionUniversal, 128);
            CarretPositionUniversal += 128;
            fs.Read(MSC.certificate.Cndash, CarretPositionUniversal, 58);
            CarretPositionUniversal += 58;
            MSC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            temp = new byte[2];
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 3);
            MSC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            MSC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(MSC.certificate.certificateAuthorityReferenceerty.additionalInfo, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            MSC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Identification*** Pos=+65=TRUE
            CI.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[15];
            fs.Read(temp, CarretPositionUniversal, 16);
            CarretPositionUniversal += 16;
            CI.cardNumber = BitConverter.ToString(temp);

            CI.cardIssuingAuthorityName.codePage = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[34];
            fs.Read(temp, CarretPositionUniversal, 35);
            CI.cardIssuingAuthorityName.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardIssueDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardValidityBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardExpiryDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;


            // *** Driver Card Holder Identification*** Pos=+146=TRUE
            WCHI.workshopName.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[34];
            fs.Read(temp, CarretPositionUniversal, 35);
            WCHI.workshopName.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            WCHI.workshopAddress.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 35);
            WCHI.workshopAddress.address = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            WCHI.cardHolderName.holderSurname.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 35);
            WCHI.cardHolderName.holderSurname.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            WCHI.cardHolderName.holderFirstNames.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 35);
            WCHI.cardHolderName.holderFirstNames.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            WCHI.cardHolderPreferredLanguage.PLanguage = BitConverter.ToString(temp).ToCharArray();
            CarretPositionUniversal += 2;

            //*** Calibratoin Number Since Download *** Pos=+2=TRUE
            temp = new byte[2];
            fs.Read(temp, CarretPositionUniversal, 2);
            NOCSD.noOfCalibrationsSinceDownload = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            //*** Calibration Records ***
            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            WCCD.calibrationTotalNumber = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            WCCD.calibrationPointerNewestRecord = fs.ReadByte();
            CarretPositionUniversal++;

            WCCD.wshopCCR = new WorkshopCardCalibrationRecord[WCAI.noOfCalibrationRecords - 1];

            for (int i = 0; i < WCAI.noOfCalibrationRecords; i++) //Pos=+105=TRUE
            {
                WCCD.wshopCCR[i].calibrationPurpose.calibrationPurpose = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[16];
                fs.Read(temp, CarretPositionUniversal, 17);
                WCCD.wshopCCR[i].VIN.vehicleIdentificationNumber = BitConverter.ToString(temp);
                CarretPositionUniversal += 17;

                WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[12];
                fs.Read(temp, CarretPositionUniversal, 13);
                WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp);
                CarretPositionUniversal += 13;

                temp = new byte[1];
                fs.Read(temp, CarretPositionUniversal, 2);
                WCCD.wshopCCR[i].wVehicleCharacteristicConstant = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 2;

                fs.Read(temp, CarretPositionUniversal, 2);
                WCCD.wshopCCR[i].kConstantOfRecordingEquipment = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 2;

                fs.Read(temp, CarretPositionUniversal, 2);
                WCCD.wshopCCR[i].lTyreCircumference = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 2;

                temp = new byte[14];
                fs.Read(temp, CarretPositionUniversal, 5);
                WCCD.wshopCCR[i].tyreSize.tyreSize = BitConverter.ToString(temp, 0);
                CarretPositionUniversal += 15;

                WCCD.wshopCCR[i].authorisedSpeed = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[2];
                fs.Read(temp, CarretPositionUniversal, 3);
                WCCD.wshopCCR[i].oldOdometerValue = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 3;

                fs.Read(temp, CarretPositionUniversal, 3);
                WCCD.wshopCCR[i].newOdometerValue = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 3;

                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                WCCD.wshopCCR[i].oldTimeValue.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, CarretPositionUniversal, 4);
                WCCD.wshopCCR[i].newTimeValue.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, CarretPositionUniversal, 4);
                WCCD.wshopCCR[i].nextCalibrationDate.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                temp = new byte[15];
                fs.Read(temp, CarretPositionUniversal, 16);
                WCCD.wshopCCR[i].vuPartNumber = BitConverter.ToString(temp, 0);
                CarretPositionUniversal += 16;

                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                WCCD.wshopCCR[i].VuSerialNumber.serialNumber = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;
                temp = new byte[1];
                fs.Read(WCCD.wshopCCR[i].VuSerialNumber.date.EncodedDate, CarretPositionUniversal, 2);
                CarretPositionUniversal += 2;
                WCCD.wshopCCR[i].VuSerialNumber.equipmentType.equipmentType = fs.ReadByte();
                CarretPositionUniversal++;
                WCCD.wshopCCR[i].VuSerialNumber.manufacturerCode.manufacturerCode = (byte)fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.serialNumber = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;
                temp = new byte[1];
                fs.Read(WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.date.EncodedDate, CarretPositionUniversal, 2);
                CarretPositionUniversal += 2;
                WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.equipmentType.equipmentType = fs.ReadByte();
                CarretPositionUniversal++;
                WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.manufacturerCode.manufacturerCode = (byte)fs.ReadByte();
                CarretPositionUniversal++;

            }

            //*** Sensor Instalation Data *** //Pos=+16=TRUE
            temp = new byte[7];
            fs.Read(temp, CarretPositionUniversal, 8);
            SISD.sensorInstallationSecData.tDesKeyA = BitConverter.ToString(temp, 0);
            CarretPositionUniversal += 8;

            fs.Read(temp, CarretPositionUniversal, 8);
            SISD.sensorInstallationSecData.tDesKeyB = BitConverter.ToString(temp, 0);
            CarretPositionUniversal += 8;

            //*** Events Reader ***
            for (int i = 0; i <= 5; i++)
            {
                CED.ceRecs[i].ceRec = new CardEventRecord[2];
            }

            for (int i = 0; i <= 5; i++)
            {
                for (int j = 0; j <= 2; j++) //Pos=+24 each = TRUE
                {
                    CED.ceRecs[i].ceRec[j].eventType.eventFaultType = (byte)fs.ReadByte();
                    CarretPositionUniversal++;

                    temp = new byte[3];
                    fs.Read(temp, CarretPositionUniversal, 4);
                    CED.ceRecs[i].ceRec[j].eventBeginTime.timeSec = BitConverter.ToInt32(temp, 0);
                    CarretPositionUniversal += 4;

                    fs.Read(temp, CarretPositionUniversal, 4);
                    CED.ceRecs[i].ceRec[j].eventEndTime.timeSec = BitConverter.ToInt32(temp, 0);
                    CarretPositionUniversal += 4;

                    CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                    CarretPositionUniversal++;

                    temp = new byte[13];
                    fs.Read(temp, CarretPositionUniversal, 14);
                    CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage = (int)temp[0];
                    CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp, 1);
                    CarretPositionUniversal += 14;

                }
            }

            //*** Faults Reader ***

            for (int i = 0; i <= 1; i++)
            {
                CFD.cardFaultRecords[i].cfRec = new CardEventRecord[5];
            }

            for (int i = 0; i <= 1; i++)
            {
                for (int j = 0; j <= 5; j++) //Pos+=24 each = TRUE
                {
                    CFD.cardFaultRecords[i].cfRec[j].eventType.eventFaultType = (byte)fs.ReadByte();
                    CarretPositionUniversal++;

                    temp = new byte[3];
                    fs.Read(temp, CarretPositionUniversal, 4);
                    CFD.cardFaultRecords[i].cfRec[j].eventBeginTime.timeSec = BitConverter.ToInt32(temp, 0);
                    CarretPositionUniversal += 4;

                    fs.Read(temp, CarretPositionUniversal, 4);
                    CFD.cardFaultRecords[i].cfRec[j].eventEndTime.timeSec = BitConverter.ToInt32(temp, 0);
                    CarretPositionUniversal += 4;

                    CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                    CarretPositionUniversal++;

                    temp = new byte[13];
                    fs.Read(temp, CarretPositionUniversal, 14);
                    CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage = (int)temp[0];
                    CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp, 1);
                    CarretPositionUniversal += 14;

                }
            }

            //*** Daily Records *** 
            int CyclicLongitude = WCAI.activityStructureLength;
            CDA.cyclicData = new byte[CyclicLongitude - 1];

            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CDA.activityPointerOldestRecord = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            fs.Read(temp, CarretPositionUniversal, 2);
            CDA.activityPointerNewestRecord = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            fs.Read(CDA.cyclicData, CarretPositionUniversal, CyclicLongitude);
            CarretPositionUniversal += CyclicLongitude;

            CDA.CyclicDataParser((int)(CyclicLongitude / 5544));

            //*** Vehicle Records ***
            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CVU.vehiclePointerNewestRecord = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            CVU.cVehRecs = new CardVehicleRecord[WCAI.noOfCardVehicleRecords - 1];

            for (int i = 0; i < WCAI.noOfCardVehicleRecords; i++) //Pos+=31 = TRUE
            {

                temp = new byte[2];
                fs.Read(temp, CarretPositionUniversal, 3);
                CarretPositionUniversal += 3;
                CVU.cVehRecs[i].vehicleOdometerBegin = BitConverter.ToInt32(temp, 0);

                fs.Read(temp, CarretPositionUniversal, 3);
                CarretPositionUniversal += 3;
                CVU.cVehRecs[i].vehicleOdometerEnd = BitConverter.ToInt32(temp, 0);

                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CarretPositionUniversal += 4;
                CVU.cVehRecs[i].vehicleFirstUse.timeSec = BitConverter.ToInt32(temp, 0);

                fs.Read(temp, CarretPositionUniversal, 4);
                CarretPositionUniversal += 4;
                CVU.cVehRecs[i].vehicleLastUse.timeSec = BitConverter.ToInt32(temp, 0);

                CVU.cVehRecs[i].registration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CVU.cVehRecs[i].registration.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[12];
                fs.Read(temp, CarretPositionUniversal, 13);
                CarretPositionUniversal += 13;
                CVU.cVehRecs[i].registration.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp, 0);

                temp = new byte[1];
                fs.Read(temp, CarretPositionUniversal, 2);
                CarretPositionUniversal += 2;
                CVU.cVehRecs[i].vuDataBlockCounter.vuDataBlockCounter = BitConverter.ToInt32(temp, 0);
            }

            //*** Places *** 
            CPDWP.placePointerNewestRecord = fs.ReadByte();
            CarretPositionUniversal++;

            CPDWP.plRecs = new PlaceRecord[WCAI.noOfCardPlaceRecords - 1];

            for (int i = 0; i < WCAI.noOfCardPlaceRecords; i++) //Pos+=10 = TRUE
            {
                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CarretPositionUniversal += 4;
                CPDWP.plRecs[i].entryTime.timeSec = BitConverter.ToInt32(temp, 0);

                CPDWP.plRecs[i].entryTypeDailyWorkPeriod.entryTypeDailyWorkPeriod = fs.ReadByte();
                CarretPositionUniversal++;

                CPDWP.plRecs[i].dailyWorkPeriodCountry.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CPDWP.plRecs[i].dailyWorkPeriodRegion.regionNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[2];
                fs.Read(temp, CarretPositionUniversal, 3);
                CarretPositionUniversal += 3;
                CPDWP.plRecs[i].vehicleOdometerValue = BitConverter.ToInt32(temp, 0);
            }

            //*** Current usage *** Pos=+19=TRUE
            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CCU.sessionOpenTime.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;
            CCU.sessionOpenVehicle.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[13];
            fs.Read(temp, CarretPositionUniversal, 14);
            CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage = (int)temp[0];
            CCU.sessionOpenVehicle.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp, 1);
            CarretPositionUniversal += 14;

            //*** Control Activity Record *** Pos=+46=38?=46=TRUE
            byte t;
            t = (byte)fs.ReadByte();
            CCADR.controlType.controlType = t.ToString();
            CarretPositionUniversal++;
            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CCADR.controlTime.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;
            CCADR.controlCardNumber.cardType.equipmentType = fs.ReadByte();
            CCADR.controlCardNumber.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal += 2;
            temp = new byte[15];
            fs.Read(temp, CarretPositionUniversal, 16);
            CarretPositionUniversal += 16;
            CCADR.controlCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex = (char)temp[15];
            CCADR.controlCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex = (char)temp[14];
            byte[] tem = new byte[13];
            Array.Copy(temp, 0, tem, 0, 14);
            CCADR.controlCardNumber.cardNumber.driverIdentification = BitConverter.ToString(tem);

            CCADR.controlVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(tem, CarretPositionUniversal, 14);
            CarretPositionUniversal += 14;
            CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage = (int)tem[0];
            CCADR.controlVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber = tem.ToString();

            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CCADR.controlDownloadPeriodBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CCADR.controlDownloadPeriodEnd.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            //*** Specific Conditions *** Pos+=280=TRUE
            SCR.scRec = new SpecificConditionRecord[1];
            for (int i = 0; i < 2; i++)
            {
                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CarretPositionUniversal += 4;
                SCR.scRec[i].entryTime.timeSec = BitConverter.ToInt32(temp, 0);

                SCR.scRec[i].specificConditionType.specificConditionType = (byte)fs.ReadByte();
                CarretPositionUniversal++;
            }


        }

        public void ControlCardInput(FileStream fs, ControlCardApplicationIdentification CCAI, CardCertificate CC, MemberStateCertificate MSC, CardIdentification CI, ControlCardHolderIdentification CCHI, ControlCardControlActivityData CCCAD)
        {
            // ***DriverCardIdentification*** Pos=+5=TRUE
            CCAI.typeOfTachographCardId.equipmentType = 1;
            CarretPositionUniversal++;

            fs.Read(CCAI.cardStructureVersion, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;

            byte[] temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            CCAI.noOfControlActivityRecords = BitConverter.ToInt32(temp, 0);

            // ***Card Certificate*** Pos=+194=TRUE
            fs.Read(CC.certificate.Sign, CarretPositionUniversal, 128);
            CarretPositionUniversal += 128;
            fs.Read(CC.certificate.Cndash, CarretPositionUniversal, 58);
            CarretPositionUniversal += 58;
            CC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[2];
            fs.Read(temp, CarretPositionUniversal, 3);
            CC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            CC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(CC.certificate.certificateAuthorityReferenceerty.additionalInfo, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            CC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Member State Certificate*** Pos=+194=TRUE
            fs.Read(MSC.certificate.Sign, CarretPositionUniversal, 128);
            CarretPositionUniversal += 128;
            fs.Read(MSC.certificate.Cndash, CarretPositionUniversal, 58);
            CarretPositionUniversal += 58;
            MSC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[2];
            fs.Read(temp, CarretPositionUniversal, 3);
            MSC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            MSC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(MSC.certificate.certificateAuthorityReferenceerty.additionalInfo, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            MSC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Identification*** Pos=+65=TRUE
            CI.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[15];
            fs.Read(temp, CarretPositionUniversal, 16);
            CarretPositionUniversal += 16;
            CI.cardNumber = BitConverter.ToString(temp);

            CI.cardIssuingAuthorityName.codePage = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[34];
            fs.Read(temp, CarretPositionUniversal, 35);
            CI.cardIssuingAuthorityName.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardIssueDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardValidityBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardExpiryDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            // *** Control Card Holder Identification *** Pos=+146=TRUE
            CCHI.controlBodyName.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[34];
            fs.Read(temp, CarretPositionUniversal, 35);
            CCHI.controlBodyName.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            CCHI.controlBodyAddress.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 35);
            CCHI.controlBodyAddress.address = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            CCHI.cardHolderName.holderSurname.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 35);
            CCHI.cardHolderName.holderSurname.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            CCHI.cardHolderName.holderFirstNames.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 35);
            CCHI.cardHolderName.holderFirstNames.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CCHI.cardHolderPreferredLanguage.PLanguage = BitConverter.ToString(temp).ToCharArray();
            CarretPositionUniversal += 2;

            //*** Control Card Control Activity Data ***
            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CCCAD.controlPointerNewestRecord = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            CCCAD.conActRecs = new ControlActivityRecord[CCAI.noOfControlActivityRecords - 1];

            for (int i = 0; i < CCAI.noOfControlActivityRecords; i++) //Pos=+105=TRUE
            {
                CCCAD.conActRecs[i].controlType.controlType = BitConverter.GetBytes(fs.ReadByte()).ToString();
                CarretPositionUniversal++;

                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CCCAD.conActRecs[i].controlTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                CCCAD.conActRecs[i].controlledCardNumber.cardType.equipmentType = fs.ReadByte();
                CarretPositionUniversal++;

                CCCAD.conActRecs[i].controlledCardNumber.cardIssuingMemberState.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[12];
                fs.Read(temp, CarretPositionUniversal, 13);
                CCCAD.conActRecs[i].controlledCardNumber.cardNumber.ownerIdentification = BitConverter.ToString(temp);
                CarretPositionUniversal += 13;
                CCCAD.conActRecs[i].controlledCardNumber.cardNumber.cardConsecutiveIndex.index = (char)fs.ReadByte();
                CarretPositionUniversal++;
                CCCAD.conActRecs[i].controlledCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex = (char)fs.ReadByte();
                CarretPositionUniversal++;
                CCCAD.conActRecs[i].controlledCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex = (char)fs.ReadByte();
                CarretPositionUniversal++;

                CCCAD.conActRecs[i].controlledVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CCCAD.conActRecs[i].controlledVehicleRegistration.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[12];
                fs.Read(temp, CarretPositionUniversal, 13);
                CCCAD.conActRecs[i].controlledVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp);
                CarretPositionUniversal += 13;

                

                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CCCAD.conActRecs[i].controlDownloadPeriodBegin.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, CarretPositionUniversal, 4);
                CCCAD.conActRecs[i].controlDownloadPeriodEnd.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;


            }
        }

        private void CompanyCardInput(FileStream fs, CompanyCardApplicationIdentification CCAI, CardCertificate CC, MemberStateCertificate MSC, CardIdentification CI, CompanyCardHolderIdentification CCHI, CompanyActivityData CAD)
        {
            // ***DriverCardIdentification*** Pos=+5=TRUE
            CCAI.typeOfTachographCardId.equipmentType = 1;
            CarretPositionUniversal++;

            fs.Read(CCAI.cardStructureVersion, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;

            byte[] temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            CCAI.noOfCompanyActivityRecords = BitConverter.ToInt32(temp, 0);

            // ***Card Certificate*** Pos=+194=TRUE
            fs.Read(CC.certificate.Sign, CarretPositionUniversal, 128);
            CarretPositionUniversal += 128;
            fs.Read(CC.certificate.Cndash, CarretPositionUniversal, 58);
            CarretPositionUniversal += 58;
            CC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[2];
            fs.Read(temp, CarretPositionUniversal, 3);
            CC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            CC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(CC.certificate.certificateAuthorityReferenceerty.additionalInfo, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            CC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Member State Certificate*** Pos=+194=TRUE
            fs.Read(MSC.certificate.Sign, CarretPositionUniversal, 128);
            CarretPositionUniversal += 128;
            fs.Read(MSC.certificate.Cndash, CarretPositionUniversal, 58);
            CarretPositionUniversal += 58;
            MSC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[2];
            fs.Read(temp, CarretPositionUniversal, 3);
            MSC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            MSC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(MSC.certificate.certificateAuthorityReferenceerty.additionalInfo, CarretPositionUniversal, 2);
            CarretPositionUniversal += 2;
            MSC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Identification*** Pos=+65=TRUE
            CI.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[15];
            fs.Read(temp, CarretPositionUniversal, 16);
            CarretPositionUniversal += 16;
            CI.cardNumber = BitConverter.ToString(temp);

            CI.cardIssuingAuthorityName.codePage = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[34];
            fs.Read(temp, CarretPositionUniversal, 35);
            CI.cardIssuingAuthorityName.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            temp = new byte[3];
            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardIssueDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardValidityBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, CarretPositionUniversal, 4);
            CI.cardExpiryDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            // *** Company Card Holder Identification *** Pos=+146=TRUE
            CCHI.companyName.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[34];
            fs.Read(temp, CarretPositionUniversal, 35);
            CCHI.companyName.name = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            CCHI.companyAddress.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, CarretPositionUniversal, 35);
            CCHI.companyAddress.address = BitConverter.ToString(temp);
            CarretPositionUniversal += 35;

            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CCHI.cardHolderPreferredLanguage.PLanguage = BitConverter.ToString(temp).ToCharArray();
            CarretPositionUniversal += 2;

            //*** Company Card Control Activity Data ***
            temp = new byte[1];
            fs.Read(temp, CarretPositionUniversal, 2);
            CAD.companyPointerNewestRecord = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 2;

            CAD.cRecs = new CompanyActivityRecord[CCAI.noOfCompanyActivityRecords - 1];

            for (int i = 0; i < CCAI.noOfCompanyActivityRecords; i++) //Pos=+105=TRUE
            {
                CAD.cRecs[i].companyActivityType.companyActivityType = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CAD.cRecs[i].companyActivityTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                CAD.cRecs[i].cardNumberInformation.cardType.equipmentType = fs.ReadByte();
                CarretPositionUniversal++;

                CAD.cRecs[i].cardNumberInformation.cardIssuingMemberState.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[12];
                fs.Read(temp, CarretPositionUniversal, 13);
                CAD.cRecs[i].cardNumberInformation.cardNumber.ownerIdentification = BitConverter.ToString(temp);
                CarretPositionUniversal += 13;
                
                CAD.cRecs[i].vehicleRegistrationInformation.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[12];
                fs.Read(temp, CarretPositionUniversal, 13);
                CAD.cRecs[i].vehicleRegistrationInformation.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp);
                CarretPositionUniversal += 13;

                temp = new byte[3];
                fs.Read(temp, CarretPositionUniversal, 4);
                CAD.cRecs[i].downloadPeriodBegin.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, CarretPositionUniversal, 4);
                CAD.cRecs[i].downloadPeriodEnd.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

            }
        }

        //int readBigEndianInt1( DataPointer start) {
        //    return start[0];
        //}

        //int readBigEndianInt2(const DataPointer& start) {
        //    return (start[0] << 8) + start[1];
        //}


        //int readBigEndianInt3(const DataPointer& start) {
        //    return (start[0] << 16) + (start[1] << 8) + start[2];
        //}

        //int readBigEndianInt4(const DataPointer& start) {
        //    return (start[0] << 24) + (start[1] << 16) + (start[2] << 8) + start[3];
        //}

        //QString bcdbyte(unsigned char start) {
        //    return hexByte(start);
        //}

        //QString hexByte(unsigned char start) {
        //    return QString("%1").arg(start, 2, 16, QChar('0'));
        //}

    }
}
