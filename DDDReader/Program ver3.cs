using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DDDReader
{
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

        public string getVerboseDrivingStatus(bool isWorkshopOrDriver)
        {
            if (!isWorkshopOrDriver)
                switch (drivingStatus)
                {
                    case 0:
                        return "SINGLE";
                    case 1:
                        return "CREW";
                    default:
                        return null;
                }
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
            var n = minutesSinceMidnight / 60;
            return n + ":" + (minutesSinceMidnight % 60) * 60;
        }

        public void ParseIT()
        {
            binaryString = Convert.ToString(RAW[0], 2).PadLeft(8, '0') + Convert.ToString(RAW[1], 2).PadLeft(8, '0');
            slotStatus = binaryString[0];
            drivingStatus = binaryString[1];
            driverCardStatus = binaryString[2];
            activityType = binaryString.Substring(3, 2);
            minutesSinceMidnight = Convert.ToInt32(binaryString.Substring(5, 11), 2);
        }
    }

    /// <summary>
    ///     Here begins section with datatypes definitions
    /// </summary>
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

//        public override string ToString()
//        {
//            return BitConverter.ToString(encodedDate);
//        }
    }

    internal class BcdMonthYear
    {
        private byte[] encodedDate = new byte[2];

        public byte[] EncodedDate
        {
            get { return encodedDate; }
            set { encodedDate = value; }
        }

//        public override string ToString()
//        {
//            return "20" + BitConverter.ToString(encodedDate)[1] + " - " + BitConverter.ToString(encodedDate)[0];
//        }
    }

    internal class Block11
    {
        public Block11Record[] bl11Recs;
        public int bl11RecsCNT = 0;
        public byte[] header = new byte[14];
    }

    internal class Block11Record
    {
        public FullCardNumber cardNumber = new FullCardNumber();
        public byte[] payloadData = new byte[30];
        public int sometimesDuration = 0;
        public TimeSpan time = new TimeSpan();
    }

    internal class Block13
    {
        public Block11Record[] bl13Recs;
        public int bl13RecsCNT = 0;
        public byte[] header = new byte[29];
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

    internal class CardChipIdentification
    {
        public byte[] icManufacturingReference = new byte[4];
        public byte[] icSerialNumber = new byte[4];
    }

    internal class CardConsecutiveIndex
    {
        public char index { get; set; }
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

    internal class CardDriverActivity
    {
        public int activityPointerNewestRecord;
        public int activityPointerOldestRecord;
        public CardActivityDailyRecord[] cardActivityDailyRecord;
        public byte[] cyclicData;

        public void CyclicDataParser(int NumberOfCycles)
        {
            cardActivityDailyRecord = new CardActivityDailyRecord[NumberOfCycles - 1];
            var pos = 0;
            for (var i = 0; i < NumberOfCycles; i++)
            {
                byte[] t = new byte[2];
                Array.Copy(cyclicData, pos, t, 0, 2);
                cardActivityDailyRecord[i].activityRecordPreviousLength = BitConverter.ToInt16(t, 0);
                pos += 2;

                Array.Copy(cyclicData, pos, t, 0, 2);
                cardActivityDailyRecord[i].activityRecordLength = BitConverter.ToInt16(t, 0);
                pos += 2;

                t = new byte[4];
                Array.Copy(cyclicData, pos, t, 0, 4);
                cardActivityDailyRecord[i].activityRecordDate.timeSec = BitConverter.ToInt32(t, 0);
                pos += 4;

                t = new byte[2];
                Array.Copy(cyclicData, pos, t, 0, 2);
                cardActivityDailyRecord[i].activityPresenceCounter.dailyPresenceCounter = BitConverter.ToInt16(t, 0);
                pos += 2;

                Array.Copy(cyclicData, pos, t, 0, 2);
                cardActivityDailyRecord[i].activityDayDistance.distance = BitConverter.ToInt16(t, 0);
                pos += 2;

                var NumberOfThings = (cardActivityDailyRecord[i].activityRecordLength - 12) / 2;
                cardActivityDailyRecord[i].activityChangeInfo = new ActivityChangeInfo[NumberOfThings - 1];

                for (var j = 0; i <= NumberOfThings; i++)
                {
                    Array.Copy(cyclicData, pos, cardActivityDailyRecord[i].activityChangeInfo[j].RAW, 0, 2);
                    pos += 2;
                    cardActivityDailyRecord[i].activityChangeInfo[j].ParseIT();
                }
            }
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
        public byte cardPesonaliserID;
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

    internal class CardRenewalIndex
    {
        public char cardRenewalIndex { get; set; }
    }

    internal class CardReplacementIndex
    {
        public char cardReplacementIndex { get; set; }
    }

    internal class CardSlots
    {
        public FullCardNumber cardNumberCoDriverSlotBegin = new FullCardNumber();
        public FullCardNumber cardNumberCoDriverSlotEnd = new FullCardNumber();
        public FullCardNumber cardNumberDriverSlotBegin = new FullCardNumber();
        public FullCardNumber cardNumberDriverSlotEnd = new FullCardNumber();
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

    internal class Distance
    {
        public int distance;
    }

    /// <summary>
    ///     Here ends type definition Block;
    ///     Here begins card block
    /// </summary>
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

    internal class EuropeanPublicKey
    {
        public PublicKey europeanPublicKey = new PublicKey();
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
                    return "Insertion of a non-valid card";
                case 0x02:
                    return "Card conflict";
                case 0x03:
                    return "Time overlap";
                case 0x04:
                    return "Driving without an appropriate card";
                case 0x05:
                    return "Card insertion while driving";
                case 0x06:
                    return "Last card session not correctly closed";
                case 0x07:
                    return "Overspeeding";
                case 0x08:
                    return "Power supply interruption";
                case 0x09:
                    return "Motion data error";
                case 0x10:
                    return "Vehicle unit related security breach attempt events. No further details";
                case 0x11:
                    return "Motion sensor authentication failure";
                case 0x12:
                    return "Tachograph card authentication failure";
                case 0x13:
                    return "Unauthorized change of motion sensor";
                case 0x14:
                    return "Card data input integrity error";
                case 0x15:
                    return "Stored user data integrity error";
                case 0x16:
                    return "Internal data transfer error";
                case 0x17:
                    return "Unauthorized case opening";
                case 0x18:
                    return "Hardware sabotage";
                case 0x20:
                    return "Sensor related security breach attempt event. No further details";
                case 0x21:
                    return "Authentication failure";
                case 0x22:
                    return "Stored data integrity error";
                case 0x23:
                    return "Internal data transfer error";
                case 0x24:
                    return "Unauthorized case opening";
                case 0x25:
                    return "Hardware sabotage";
                case 0x30:
                    return "Recording equipment faults. No further details";
                case 0x31:
                    return "VU internal fault";
                case 0x32:
                    return "Printer fault";
                case 0x33:
                    return "Display fault";
                case 0x34:
                    return "Downloading fault";
                case 0x35:
                    return "Sensor fault";
                case 0x40:
                    return "Card faults. No further details";
                default:
                    return "RFU";
            }
        }
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

    internal class KeyIdentifier
    {
        public CertificateAuthority certificateAuthorityKID = new CertificateAuthority();
        public CertificateRequestID certificateRequestID = new CertificateRequestID();
        public ExtendedSerialNumber extendedSerialNumber = new ExtendedSerialNumber();

        public object getActiveChoice()
        {
            if (extendedSerialNumber != null) return extendedSerialNumber.GetType();
            if (certificateRequestID != null) return certificateRequestID.GetType();
            if (certificateAuthorityKID != null) return certificateAuthorityKID.GetType();
            throw new ArgumentNullException("","KeyIdentifier is Null!");
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

    internal class LastCardDownload
    {
        public TimeReal lastCardDownload = new TimeReal();

        public override string ToString()
        {
            return lastCardDownload.ConvertToUTC();
        }
    }

    internal class Library
    {
        public static int CarretPositionUniversal;

        public string Encode(int codepage, byte[] b)
        {
            try
            {
                return Encoding.GetEncoding(codepage).GetString(b);
            }
            catch (Exception ex)
            {
                Debug.Print("Bad encoding. Returning to defaults. Error Message: "+ex.Message+". StackTrace: "+ex.StackTrace);
                return Encoding.Default.GetString(b);
            }
        }

        public void CardChipIdentification(FileStream fs, CardChipIdentification cardCI)
        {
            fs.Read(cardCI.icSerialNumber, 0, 4);
            CarretPositionUniversal += 4;
            fs.Read(cardCI.icManufacturingReference, 0, 4);
            CarretPositionUniversal += 4;
        }

        public void CardIdentifier(FileStream fs, CardIccIdentification cardICCI)
        {
            //byte[] b = new byte[207];
            //fs.Read(b, 0, 207);
            //CarretPositionUniversal += 207;

            cardICCI.clockStop = fs.ReadByte();
            CarretPositionUniversal++;
            var temp = new byte[4];
            fs.Read(temp, 0, 4);
            CarretPositionUniversal += 4;
            cardICCI.cardExtendedSerialNumber.serialNumber = BitConverter.ToInt32(temp, 0);
            fs.Read(cardICCI.cardExtendedSerialNumber.date.EncodedDate, 0, 2);
            CarretPositionUniversal += 2;
            cardICCI.cardExtendedSerialNumber.equipmentType.equipmentType = fs.ReadByte();
            CarretPositionUniversal++;
            cardICCI.cardExtendedSerialNumber.manufacturerCode.manufacturerCode = (byte) fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[8];
            fs.Read(temp, 0, 8);
            CarretPositionUniversal += 8;
            cardICCI.cardApprovalNumber = BitConverter.ToString(temp);
            cardICCI.cardPesonaliserID = BitConverter.GetBytes(fs.ReadByte())[0];
            CarretPositionUniversal++;
            fs.Read(cardICCI.embedderIcAssemblerId, 0, 5);
            CarretPositionUniversal += 5;
            fs.Read(cardICCI.icIdentifier, 0, 2);
            CarretPositionUniversal += 2;
        }

        public void ControlCardInput(FileStream fs, ControlCardApplicationIdentification CCAI, CardCertificate CC,
            MemberStateCertificate MSC, CardIdentification CI, ControlCardHolderIdentification CCHI,
            ControlCardControlActivityData CCCAD)
        {
            // ***DriverCardIdentification*** Pos=+5=TRUE
            CCAI.typeOfTachographCardId.equipmentType = 1;
            CarretPositionUniversal++;

            fs.Read(CCAI.cardStructureVersion, 0, 2);
            CarretPositionUniversal += 2;

            var temp = new byte[2];
            fs.Read(temp, 0, 2);
            CarretPositionUniversal += 2;
            CCAI.noOfControlActivityRecords = BitConverter.ToInt16(temp, 0);

            // ***Card Certificate*** Pos=+194=TRUE
            fs.Read(CC.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            fs.Read(CC.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            CC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[3];
            fs.Read(temp, 0, 3);
            CC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            CC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(CC.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            CC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Member State Certificate*** Pos=+194=TRUE
            fs.Read(MSC.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            fs.Read(MSC.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            MSC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[3];
            fs.Read(temp, 0, 3);
            MSC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            MSC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(MSC.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            MSC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Identification*** Pos=+65=TRUE
            CI.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[16];
            fs.Read(temp, 0, 16);
            CarretPositionUniversal += 16;
            CI.cardNumber = BitConverter.ToString(temp);

            CI.cardIssuingAuthorityName.codePage = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[35];
            fs.Read(temp, 0, 35);
            CI.cardIssuingAuthorityName.name =
                Encode(CI.cardIssuingAuthorityName.codePage, temp);
            CarretPositionUniversal += 35;

            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CI.cardIssueDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CI.cardValidityBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CI.cardExpiryDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            // *** Control Card Holder Identification *** Pos=+146=TRUE
            CCHI.controlBodyName.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[35];
            fs.Read(temp, 0, 35);
            CCHI.controlBodyName.name = Encode(CCHI.controlBodyName.codePage, temp);
            CarretPositionUniversal += 35;

            CCHI.controlBodyAddress.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, 0, 35);
            CCHI.controlBodyAddress.address = Encode(CCHI.controlBodyAddress.codePage, temp);
            CarretPositionUniversal += 35;

            CCHI.cardHolderName.holderSurname.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, 0, 35);
            CCHI.cardHolderName.holderSurname.name = Encode(CCHI.cardHolderName.holderSurname.codePage, temp);
            CarretPositionUniversal += 35;

            CCHI.cardHolderName.holderFirstNames.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, 0, 35);
            CCHI.cardHolderName.holderFirstNames.name = Encode(CCHI.cardHolderName.holderFirstNames.codePage, temp);
            CarretPositionUniversal += 35;

            temp = new byte[2];
            fs.Read(temp, 0, 2);
            CCHI.cardHolderPreferredLanguage.PLanguage = BitConverter.ToString(temp).ToCharArray();
            CarretPositionUniversal += 2;

            //*** Control Card Control Activity Data ***
            temp = new byte[2];
            fs.Read(temp, 0, 2);
            CCCAD.controlPointerNewestRecord = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;

            CCCAD.conActRecs = new ControlActivityRecord[CCAI.noOfControlActivityRecords - 1];

            for (var i = 0; i < CCAI.noOfControlActivityRecords; i++) //Pos=+105=TRUE
            {
                CCCAD.conActRecs[i].controlType.controlType = BitConverter.GetBytes(fs.ReadByte()).ToString();
                CarretPositionUniversal++;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CCCAD.conActRecs[i].controlTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                CCCAD.conActRecs[i].controlledCardNumber.cardType.equipmentType = fs.ReadByte();
                CarretPositionUniversal++;

                CCCAD.conActRecs[i].controlledCardNumber.cardIssuingMemberState.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[13];
                fs.Read(temp, 0, 13);
                CCCAD.conActRecs[i].controlledCardNumber.cardNumber.ownerIdentification = BitConverter.ToString(temp);
                CarretPositionUniversal += 13;
                CCCAD.conActRecs[i].controlledCardNumber.cardNumber.cardConsecutiveIndex.index = (char) fs.ReadByte();
                CarretPositionUniversal++;
                CCCAD.conActRecs[i].controlledCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex =
                    (char) fs.ReadByte();
                CarretPositionUniversal++;
                CCCAD.conActRecs[i].controlledCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex =
                    (char) fs.ReadByte();
                CarretPositionUniversal++;

                CCCAD.conActRecs[i].controlledVehicleRegistration.vehicleRegistrationNation.nationNumeric =
                    fs.ReadByte();
                CarretPositionUniversal++;

                CCCAD.conActRecs[i].controlledVehicleRegistration.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[13];
                fs.Read(temp, 0, 13);
                CCCAD.conActRecs[i].controlledVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                    BitConverter.ToString(temp);
                CarretPositionUniversal += 13;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CCCAD.conActRecs[i].controlDownloadPeriodBegin.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, 0, 4);
                CCCAD.conActRecs[i].controlDownloadPeriodEnd.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;
            }
        }

        public void DriverCardInput(FileStream fs, DriverCardApplicationIdentification DCAI, CardCertificate CC,
            MemberStateCertificate MSC, CardIdentification CI, DriverCardHolderIdentification DCHI, LastCardDownload LCD,
            CardDrivingLicenseInformation CDLI, CardEventData CED, CardFaultData CFD, CardDriverActivity CDA,
            CardVehiclesUsed CVU, CardPlaceDailyWorkPeriod CPDWP, CardCurrentUse CCU,
            CardControlActivityDataRecord CCADR, SpecificConditionRecords SCR)
        {
            var bID = new BlockIDReader();
            // ***DriverCardIdentification*** Pos=+10=TRUE
            DCAI.typeOfTachographCardId.equipmentType = 1;
            CarretPositionUniversal++;
            fs.Read(DCAI.cardStructureVersion, 0, 2);
            CarretPositionUniversal += 2;
            DCAI.noOfEventsPerType = fs.ReadByte();
            DCAI.noOfFaultsPerType = fs.ReadByte();
            CarretPositionUniversal += 2;
            fs.Read(DCAI.activityStructureLength, 0, 2);
            CarretPositionUniversal += 2;
            var temp = new byte[2];
            fs.Read(temp, 0, 2);
            DCAI.noOfCardVehicleRecords = (temp[1] << 0) | (temp[0] << 8);
            DCAI.noOfCardPlaceRecords = fs.ReadByte();
            CarretPositionUniversal+=2;

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            // ***Card Certificate*** Pos=+194=TRUE
            fs.Read(CC.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            fs.Read(CC.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            CC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[3];
            fs.Read(temp, 0, 3);
            CC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            CC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(CC.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            CC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            // ***Member State Certificate*** Pos=+194=TRUE
            fs.Read(MSC.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            fs.Read(MSC.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            MSC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[3];
            fs.Read(temp, 0, 3);
            MSC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            MSC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(MSC.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            MSC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            // ***Card Identification*** Pos=+65=TRUE
            CI.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[16];
            fs.Read(temp, 0, 16);
            CarretPositionUniversal += 16;
            CI.cardNumber = BitConverter.ToString(temp);

            CI.cardIssuingAuthorityName.codePage = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[35];
            fs.Read(temp, 0, 35);
            CI.cardIssuingAuthorityName.name = Encode(CI.cardIssuingAuthorityName.codePage, temp);
            CarretPositionUniversal += 35;

            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CI.cardIssueDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CI.cardValidityBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CI.cardExpiryDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            // *** Driver Card Holder Identification*** Pos=+78=TRUE
            DCHI.cardHolderName.holderSurname.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[35];
            fs.Read(temp, 0, 35);
            DCHI.cardHolderName.holderSurname.name = Encode(DCHI.cardHolderName.holderSurname.codePage, temp);
            CarretPositionUniversal += 35;
            DCHI.cardHolderName.holderFirstNames.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, 0, 35);
            DCHI.cardHolderName.holderFirstNames.name = Encode(DCHI.cardHolderName.holderFirstNames.codePage, temp);
            CarretPositionUniversal += 35;
            fs.Read(DCHI.cardHolderBirthDate.EncodedDate, 0, 4);
            CarretPositionUniversal += 4;
            temp = new byte[2];
            fs.Read(temp, 0, 2);
            DCHI.cardHolderPreferredLanguage.PLanguage = BitConverter.ToString(temp).ToCharArray();
            CarretPositionUniversal += 2;

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            //*** Last Card Download *** Pos=+4=TRUE
            temp = new byte[4];
            fs.Read(temp, 0, 4);
            LCD.lastCardDownload.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            //*** Events Reader ***
            for (var i = 0; i < 6; i++)
            {
                CED.ceRecs[i] = new CardEventRecords {ceRec = new CardEventRecord[DCAI.noOfEventsPerType]};
            }
            for (var i = 0; i < 6; i++)
            for (var j = 0; j < DCAI.noOfEventsPerType; j++) //Pos=+24 each = TRUE
            {
                CED.ceRecs[i].ceRec[j] = new CardEventRecord {eventType = {eventFaultType = (byte) fs.ReadByte()}};
                CarretPositionUniversal++;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CED.ceRecs[i].ceRec[j].eventBeginTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, 0, 4);
                CED.ceRecs[i].ceRec[j].eventEndTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[14];
                fs.Read(temp, 0, 14);
                CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage = temp[0];
                for (var k = 1; k < temp.Length; k++)
                    temp[k - 1] = temp[k];
                CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                    Encode(CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage, temp);
                CarretPositionUniversal += 14;
            }

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            //*** Faults Reader ***

            for (var i = 0; i < 2; i++)
            {
                CFD.cardFaultRecords[i] = new CardFaultRecords {cfRec = new CardEventRecord[DCAI.noOfFaultsPerType]};
            }
            for (var i = 0; i < 2; i++)
            for (var j = 0; j < DCAI.noOfFaultsPerType; j++) //Pos+=24 each = TRUE
            {
                CFD.cardFaultRecords[i].cfRec[j] = new CardEventRecord {eventType = {eventFaultType = (byte) fs.ReadByte()}};
                CarretPositionUniversal++;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CFD.cardFaultRecords[i].cfRec[j].eventBeginTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, 0, 4);
                CFD.cardFaultRecords[i].cfRec[j].eventEndTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric =
                    fs.ReadByte();
                CarretPositionUniversal++;


                CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage =
                    fs.ReadByte();
                temp = new byte[13];
                fs.Read(temp, 0, 13);
                CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                    Encode(
                        CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage,
                        temp);
                CarretPositionUniversal += 14;
            }

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            //*** Daily Records ***
            var CyclicLongitude = BitConverter.ToInt16(DCAI.activityStructureLength, 0);
            CDA.cyclicData = new byte[CyclicLongitude - 1];

            temp = new byte[2];
            fs.Read(temp, 0, 2);
            CDA.activityPointerOldestRecord = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;

            fs.Read(temp, 0, 2);
            CDA.activityPointerNewestRecord = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;

            fs.Read(CDA.cyclicData, 0, CyclicLongitude-1);
            CarretPositionUniversal += CyclicLongitude;

            CDA.CyclicDataParser(CyclicLongitude / 5544);

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            //*** Vehicle Records ***
            temp = new byte[2];
            fs.Read(temp, 0, 2);
            CVU.vehiclePointerNewestRecord = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;

            CVU.cVehRecs = new CardVehicleRecord[DCAI.noOfCardVehicleRecords - 1];

            for (var i = 1; i < DCAI.noOfCardVehicleRecords; i++) //Pos+=31 = TRUE
            {
                temp = new byte[3];
                fs.Read(temp, 0, 3);
                CarretPositionUniversal += 3;
                CVU.cVehRecs[i].vehicleOdometerBegin = (temp[2] << 0) | (temp[1] << 8) | (temp[0] << 16);

                fs.Read(temp, 0, 3);
                CarretPositionUniversal += 3;
                CVU.cVehRecs[i].vehicleOdometerEnd = (temp[2] << 0) | (temp[1] << 8) | (temp[0] << 16);

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CarretPositionUniversal += 4;
                CVU.cVehRecs[i].vehicleFirstUse.timeSec = BitConverter.ToInt32(temp, 0);

                fs.Read(temp, 0, 4);
                CarretPositionUniversal += 4;
                CVU.cVehRecs[i].vehicleLastUse.timeSec = BitConverter.ToInt32(temp, 0);

                CVU.cVehRecs[i].registration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CVU.cVehRecs[i].registration.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[13];
                fs.Read(temp, 0, 13);
                CarretPositionUniversal += 13;
                CVU.cVehRecs[i].registration.vehicleRegistrationNumber.vehicleRegNumber = BitConverter.ToString(temp, 0);

                temp = new byte[2];
                fs.Read(temp, 0, 2);
                CarretPositionUniversal += 2;
                CVU.cVehRecs[i].vuDataBlockCounter.vuDataBlockCounter = BitConverter.ToInt16(temp, 0);
            }

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            //*** Places ***
            CPDWP.placePointerNewestRecord = fs.ReadByte();
            CarretPositionUniversal++;

            CPDWP.plRecs = new PlaceRecord[DCAI.noOfCardPlaceRecords - 1];

            for (var i = 0; i < DCAI.noOfCardPlaceRecords; i++) //Pos+=10 = TRUE
            {
                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CarretPositionUniversal += 4;
                CPDWP.plRecs[i].entryTime.timeSec = BitConverter.ToInt32(temp, 0);

                CPDWP.plRecs[i].entryTypeDailyWorkPeriod.entryTypeDailyWorkPeriod = fs.ReadByte();
                CarretPositionUniversal++;

                CPDWP.plRecs[i].dailyWorkPeriodCountry.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CPDWP.plRecs[i].dailyWorkPeriodRegion.regionNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[3];
                fs.Read(temp, 0, 3);
                CarretPositionUniversal += 3;
                CPDWP.plRecs[i].vehicleOdometerValue = (temp[2] << 0) | (temp[1] << 8) | (temp[0] << 16);
            }

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            //*** Current usage *** Pos=+19=TRUE
            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CCU.sessionOpenTime.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;
            CCU.sessionOpenVehicle.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage = fs.ReadByte();
            temp = new byte[13];
            fs.Read(temp, 0, 13);
            CCU.sessionOpenVehicle.vehicleRegistrationNumber.vehicleRegNumber =
                Encode(CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage, temp);
            CarretPositionUniversal += 14;

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            //*** Control Activity Record *** Pos=+46=38?=
            var t = (byte) fs.ReadByte();
            CCADR.controlType.controlType = t.ToString();
            CarretPositionUniversal++;
            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CCADR.controlTime.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;
            CCADR.controlCardNumber.cardType.equipmentType = fs.ReadByte();
            CCADR.controlCardNumber.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal += 2;
            temp = new byte[16];
            fs.Read(temp, 0, 16);
            CarretPositionUniversal += 16;
            CCADR.controlCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex = (char) temp[15];
            CCADR.controlCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex = (char) temp[14];
            var tem = new byte[13];
            Array.Copy(temp, 0, tem, 0, 14);
            CCADR.controlCardNumber.cardNumber.driverIdentification = BitConverter.ToString(tem);

            CCADR.controlVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage = fs.ReadByte();
            fs.Read(tem, 0, 13);
            CCADR.controlVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                Encode(CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage, temp);
            CarretPositionUniversal += 14;

            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CCADR.controlDownloadPeriodBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CCADR.controlDownloadPeriodEnd.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            //*** Specific Conditions *** Pos+=280=TRUE
            SCR.scRec = new SpecificConditionRecord[56];
            for (var i = 1; i <= 56; i++)
            {
                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CarretPositionUniversal += 4;
                SCR.scRec[i].entryTime.timeSec = BitConverter.ToInt32(temp, 0);

                SCR.scRec[i].specificConditionType.specificConditionType = (byte) fs.ReadByte();
                CarretPositionUniversal++;
            }
        }
        //TODO:5 bytes before each block!!!!!!!!!
        //TODO:Redesign total, to search for blocks!!!!!!!!!!
        public void FileOpenReadDecode(string FileName)
        {
            var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);

            var bID = new BlockIDReader();

            var cardICCI = new CardIccIdentification();
            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            CardIdentifier(fs, cardICCI);
            
            var cardCI = new CardChipIdentification();
            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            CardChipIdentification(fs, cardCI);

            bID.BlockIDRead(fs);
            CarretPositionUniversal += 5;
            var Detect = fs.ReadByte();
            switch (Detect)
            {
                case 1:
                {
                    var DCAI = new DriverCardApplicationIdentification();
                    var CC = new CardCertificate();
                    var MSC = new MemberStateCertificate();
                    var CI = new CardIdentification();
                    var DCHI = new DriverCardHolderIdentification();
                    var LCD = new LastCardDownload();
                    var CDLI = new CardDrivingLicenseInformation();
                    var CED = new CardEventData();
                    var CFD = new CardFaultData();
                    var CDA = new CardDriverActivity();
                    var CVU = new CardVehiclesUsed();
                    var CPDWP = new CardPlaceDailyWorkPeriod();
                    var CCU = new CardCurrentUse();
                    var CCADR = new CardControlActivityDataRecord();
                    var SCR = new SpecificConditionRecords();
                    DriverCardInput(fs, DCAI, CC, MSC, CI, DCHI, LCD, CDLI, CED, CFD, CDA, CVU, CPDWP, CCU, CCADR, SCR);
                    //TODO: Output designer;
                    break;
                }
                case 2:
                {
                    var WCAI = new WorkshopCardApplicationIdentification();
                    var CC = new CardCertificate();
                    var MSC = new MemberStateCertificate();
                    var CI = new CardIdentification();
                    var WCHI = new WorkshopCardHolderIdentification();
                    var NOCSD = new NoOfCalibrationsSinceDownload();
                    var WCCD = new WorkshopCardCalibrationData();
                    var SISD = new SensorInstallationSecData();
                    var CED = new CardEventData();
                    var CFD = new CardFaultData();
                    var CDA = new CardDriverActivity();
                    var CVU = new CardVehiclesUsed();
                    var CPDWP = new CardPlaceDailyWorkPeriod();
                    var CCU = new CardCurrentUse();
                    var CCADR = new CardControlActivityDataRecord();
                    var SCR = new SpecificConditionRecords();
                    WorkshopCardInput(fs, WCAI, CC, MSC, CI, WCHI, NOCSD, WCCD, SISD, CED, CFD, CDA, CVU, CPDWP, CCU,
                        CCADR, SCR);
                    //TODO:Design output
                    break;
                }
                case 3:
                {
                    var CCAI = new ControlCardApplicationIdentification();
                    var CC = new CardCertificate();
                    var MSC = new MemberStateCertificate();
                    var CI = new CardIdentification();
                    var CCHI = new ControlCardHolderIdentification();
                    var CCCAD = new ControlCardControlActivityData();
                    ControlCardInput(fs, CCAI, CC, MSC, CI, CCHI, CCCAD);
                    //TODO:Output
                    break;
                }
                case 4:
                {
                    var CCAI = new CompanyCardApplicationIdentification();
                    var CC = new CardCertificate();
                    var MSC = new MemberStateCertificate();
                    var CI = new CardIdentification();
                    var CCHI = new CompanyCardHolderIdentification();
                    var CAD = new CompanyActivityData();
                    CompanyCardInput(fs, CCAI, CC, MSC, CI, CCHI, CAD);
                    //TODO:Output
                    break;
                }
                case 5:
                {
                    break;
                }
                default:
                    throw new FileLoadException(
                        "The specified filename doesn't contain a tachograph card info or something went wrong...",
                        FileName);
            }
        }

        //Pos= +25 = TRUE

        //Pos= +8 = TRUE
        public void WorkshopCardInput(FileStream fs, WorkshopCardApplicationIdentification WCAI, CardCertificate CC,
            MemberStateCertificate MSC, CardIdentification CI, WorkshopCardHolderIdentification WCHI,
            NoOfCalibrationsSinceDownload NOCSD, WorkshopCardCalibrationData WCCD, SensorInstallationSecData SISD,
            CardEventData CED, CardFaultData CFD, CardDriverActivity CDA, CardVehiclesUsed CVU,
            CardPlaceDailyWorkPeriod CPDWP, CardCurrentUse CCU, CardControlActivityDataRecord CCADR,
            SpecificConditionRecords SCR)
        {
            // ***WorkshopCardIdentification*** Pos=+11=TRUE
            WCAI.typeOfTachographId.equipmentType = 2;
            CarretPositionUniversal++;
            fs.Read(WCAI.cardStructureVersion, 0, 2);
            CarretPositionUniversal += 2;
            WCAI.noOfEventsPerType = fs.ReadByte();
            WCAI.noOfFaultsPerType = fs.ReadByte();
            CarretPositionUniversal += 2;
            var temp = new byte[2];
            fs.Read(temp, 0, 2);
            WCAI.activityStructureLength = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;
            temp = new byte[2];
            fs.Read(temp, 0, 2);
            CarretPositionUniversal += 2;
            WCAI.noOfCardVehicleRecords = BitConverter.ToInt16(temp, 0);
            WCAI.noOfCardPlaceRecords = fs.ReadByte();
            CarretPositionUniversal++;
            WCAI.noOfCalibrationRecords = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Certificate*** Pos=+194=TRUE
            fs.Read(CC.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            fs.Read(CC.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            CC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            temp = new byte[3];
            CarretPositionUniversal++;
            fs.Read(temp, 0, 3);
            CC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            CC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(CC.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            CC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Member State Certificate***
            fs.Read(MSC.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            fs.Read(MSC.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            MSC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            temp = new byte[3];
            CarretPositionUniversal++;
            fs.Read(temp, 0, 3);
            MSC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            MSC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(MSC.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            MSC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Identification*** Pos=+65=TRUE
            CI.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[16];
            fs.Read(temp, 0, 16);
            CarretPositionUniversal += 16;
            CI.cardNumber = BitConverter.ToString(temp);

            CI.cardIssuingAuthorityName.codePage = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[35];
            fs.Read(temp, 0, 35);
            CI.cardIssuingAuthorityName.name =
                Encode(CI.cardIssuingAuthorityName.codePage, temp);
            CarretPositionUniversal += 35;

            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CI.cardIssueDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CI.cardValidityBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CI.cardExpiryDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            // *** Driver Card Holder Identification*** Pos=+146=TRUE
            WCHI.workshopName.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[35];
            fs.Read(temp, 0, 35);
            WCHI.workshopName.name = Encode(WCHI.workshopName.codePage, temp);
            CarretPositionUniversal += 35;

            WCHI.workshopAddress.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, 0, 35);
            WCHI.workshopAddress.address = Encode(WCHI.workshopAddress.codePage, temp);
            CarretPositionUniversal += 35;

            WCHI.cardHolderName.holderSurname.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, 0, 35);
            WCHI.cardHolderName.holderSurname.name = Encode(WCHI.cardHolderName.holderSurname.codePage, temp);
            CarretPositionUniversal += 35;

            WCHI.cardHolderName.holderFirstNames.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, 0, 35);
            WCHI.cardHolderName.holderFirstNames.name = Encode(WCHI.cardHolderName.holderFirstNames.codePage, temp);
            CarretPositionUniversal += 35;

            temp = new byte[2];
            fs.Read(temp, 0, 2);
            WCHI.cardHolderPreferredLanguage.PLanguage = BitConverter.ToString(temp).ToCharArray();
            CarretPositionUniversal += 2;

            //*** Calibratoin Number Since Download *** Pos=+2=TRUE
            temp = new byte[3];
            fs.Read(temp, 0, 2);
            NOCSD.noOfCalibrationsSinceDownload = (temp[2] << 0) | (temp[1] << 8) | (temp[0] << 16);
            CarretPositionUniversal += 2;

            //*** Calibration Records ***
            temp = new byte[2];
            fs.Read(temp, 0, 2);
            WCCD.calibrationTotalNumber = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;

            WCCD.calibrationPointerNewestRecord = fs.ReadByte();
            CarretPositionUniversal++;

            WCCD.wshopCCR = new WorkshopCardCalibrationRecord[WCAI.noOfCalibrationRecords];

            for (var i = 1; i < WCAI.noOfCalibrationRecords; i++) //Pos=+105=TRUE
            {
                WCCD.wshopCCR[i].calibrationPurpose.calibrationPurpose = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[17];
                fs.Read(temp, 0, 17);
                WCCD.wshopCCR[i].VIN.vehicleIdentificationNumber = BitConverter.ToString(temp);
                CarretPositionUniversal += 17;

                WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[13];
                fs.Read(temp, 0, 13);
                WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                    Encode(WCCD.wshopCCR[i].vehicleRegistration.vehicleRegistrationNumber.codePage, temp);
                CarretPositionUniversal += 13;

                temp = new byte[2];
                fs.Read(temp, 0, 2);
                WCCD.wshopCCR[i].wVehicleCharacteristicConstant = BitConverter.ToInt16(temp, 0);
                CarretPositionUniversal += 2;

                fs.Read(temp, 0, 2);
                WCCD.wshopCCR[i].kConstantOfRecordingEquipment = BitConverter.ToInt16(temp, 0);
                CarretPositionUniversal += 2;

                fs.Read(temp, 0, 2);
                WCCD.wshopCCR[i].lTyreCircumference = BitConverter.ToInt16(temp, 0);
                CarretPositionUniversal += 2;

                temp = new byte[15];
                fs.Read(temp, 0, 5);
                WCCD.wshopCCR[i].tyreSize.tyreSize = BitConverter.ToString(temp, 0);
                CarretPositionUniversal += 15;

                WCCD.wshopCCR[i].authorisedSpeed = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[3];
                fs.Read(temp, 0, 3);
                WCCD.wshopCCR[i].oldOdometerValue = (temp[2] << 0) | (temp[1] << 8) | (temp[0] << 16);
                CarretPositionUniversal += 3;

                fs.Read(temp, 0, 3);
                WCCD.wshopCCR[i].newOdometerValue = (temp[2] << 0) | (temp[1] << 8) | (temp[0] << 16);
                CarretPositionUniversal += 3;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                WCCD.wshopCCR[i].oldTimeValue.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, 0, 4);
                WCCD.wshopCCR[i].newTimeValue.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, 0, 4);
                WCCD.wshopCCR[i].nextCalibrationDate.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                temp = new byte[16];
                fs.Read(temp, 0, 16);
                WCCD.wshopCCR[i].vuPartNumber = BitConverter.ToString(temp, 0);
                CarretPositionUniversal += 16;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                WCCD.wshopCCR[i].VuSerialNumber.serialNumber = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;
                fs.Read(WCCD.wshopCCR[i].VuSerialNumber.date.EncodedDate, 0, 2);
                CarretPositionUniversal += 2;
                WCCD.wshopCCR[i].VuSerialNumber.equipmentType.equipmentType = fs.ReadByte();
                CarretPositionUniversal++;
                WCCD.wshopCCR[i].VuSerialNumber.manufacturerCode.manufacturerCode = (byte) fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.serialNumber = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;
                fs.Read(WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.date.EncodedDate, 0, 2);
                CarretPositionUniversal += 2;
                WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.equipmentType.equipmentType = fs.ReadByte();
                CarretPositionUniversal++;
                WCCD.wshopCCR[i].sensorSerialNumber.sensorSN.manufacturerCode.manufacturerCode = (byte) fs.ReadByte();
                CarretPositionUniversal++;
            }

            //*** Sensor Instalation Data *** //Pos=+16=TRUE
            temp = new byte[8];
            fs.Read(temp, 0, 8);
            SISD.sensorInstallationSecData.tDesKeyA = BitConverter.ToString(temp, 0);
            CarretPositionUniversal += 8;

            fs.Read(temp, 0, 8);
            SISD.sensorInstallationSecData.tDesKeyB = BitConverter.ToString(temp, 0);
            CarretPositionUniversal += 8;

            //*** Events Reader ***
            for (var i = 0; i <= 5; i++)
                CED.ceRecs[i].ceRec = new CardEventRecord[2];

            for (var i = 0; i <= 5; i++)
            for (var j = 0; j <= 2; j++) //Pos=+24 each = TRUE
            {
                CED.ceRecs[i].ceRec[j].eventType.eventFaultType = (byte) fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CED.ceRecs[i].ceRec[j].eventBeginTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, 0, 4);
                CED.ceRecs[i].ceRec[j].eventEndTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage = fs.ReadByte();
                temp = new byte[13];
                fs.Read(temp, 0, 13);
                CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                    Encode(CED.ceRecs[i].ceRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage, temp);
                CarretPositionUniversal += 14;
            }

            //*** Faults Reader ***

            for (var i = 0; i <= 1; i++)
                CFD.cardFaultRecords[i].cfRec = new CardEventRecord[5];

            for (var i = 0; i <= 1; i++)
            for (var j = 0; j <= 5; j++) //Pos+=24 each = TRUE
            {
                CFD.cardFaultRecords[i].cfRec[j].eventType.eventFaultType = (byte) fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CFD.cardFaultRecords[i].cfRec[j].eventBeginTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, 0, 4);
                CFD.cardFaultRecords[i].cfRec[j].eventEndTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNation.nationNumeric =
                    fs.ReadByte();
                CarretPositionUniversal++;

                CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage =
                    fs.ReadByte();
                temp = new byte[13];
                fs.Read(temp, 0, 13);
                CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                    Encode(
                        CFD.cardFaultRecords[i].cfRec[j].eventVehicleRegistration.vehicleRegistrationNumber.codePage,
                        temp);
                CarretPositionUniversal += 14;
            }

            //*** Daily Records ***
            var CyclicLongitude = WCAI.activityStructureLength;
            CDA.cyclicData = new byte[CyclicLongitude - 1];

            temp = new byte[2];
            fs.Read(temp, 0, 2);
            CDA.activityPointerOldestRecord = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;

            fs.Read(temp, 0, 2);
            CDA.activityPointerNewestRecord = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;

            fs.Read(CDA.cyclicData, 0, CyclicLongitude);
            CarretPositionUniversal += CyclicLongitude;

            CDA.CyclicDataParser(CyclicLongitude / 5544);

            //*** Vehicle Records ***
            temp = new byte[2];
            fs.Read(temp, 0, 2);
            CVU.vehiclePointerNewestRecord = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;

            CVU.cVehRecs = new CardVehicleRecord[WCAI.noOfCardVehicleRecords - 1];

            for (var i = 0; i < WCAI.noOfCardVehicleRecords; i++) //Pos+=31 = TRUE
            {
                temp = new byte[3];
                fs.Read(temp, 0, 3);
                CarretPositionUniversal += 3;
                CVU.cVehRecs[i].vehicleOdometerBegin = (temp[2] << 0) | (temp[1] << 8) | (temp[0] << 16);

                fs.Read(temp, 0, 3);
                CarretPositionUniversal += 3;
                CVU.cVehRecs[i].vehicleOdometerEnd = (temp[2] << 0) | (temp[1] << 8) | (temp[0] << 16);

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CarretPositionUniversal += 4;
                CVU.cVehRecs[i].vehicleFirstUse.timeSec = BitConverter.ToInt32(temp, 0);

                fs.Read(temp, 0, 4);
                CarretPositionUniversal += 4;
                CVU.cVehRecs[i].vehicleLastUse.timeSec = BitConverter.ToInt32(temp, 0);

                CVU.cVehRecs[i].registration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CVU.cVehRecs[i].registration.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[13];
                fs.Read(temp, 0, 13);
                CarretPositionUniversal += 13;
                CVU.cVehRecs[i].registration.vehicleRegistrationNumber.vehicleRegNumber =
                    Encode(CVU.cVehRecs[i].registration.vehicleRegistrationNumber.codePage, temp);


                temp = new byte[2];
                fs.Read(temp, 0, 2);
                CarretPositionUniversal += 2;
                CVU.cVehRecs[i].vuDataBlockCounter.vuDataBlockCounter = BitConverter.ToInt16(temp, 0);
            }

            //*** Places ***
            CPDWP.placePointerNewestRecord = fs.ReadByte();
            CarretPositionUniversal++;

            CPDWP.plRecs = new PlaceRecord[WCAI.noOfCardPlaceRecords - 1];

            for (var i = 0; i < WCAI.noOfCardPlaceRecords; i++) //Pos+=10 = TRUE
            {
                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CarretPositionUniversal += 4;
                CPDWP.plRecs[i].entryTime.timeSec = BitConverter.ToInt32(temp, 0);

                CPDWP.plRecs[i].entryTypeDailyWorkPeriod.entryTypeDailyWorkPeriod = fs.ReadByte();
                CarretPositionUniversal++;

                CPDWP.plRecs[i].dailyWorkPeriodCountry.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                CPDWP.plRecs[i].dailyWorkPeriodRegion.regionNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[3];
                fs.Read(temp, 0, 3);
                CarretPositionUniversal += 3;
                CPDWP.plRecs[i].vehicleOdometerValue = (temp[2] << 0) | (temp[1] << 8) | (temp[0] << 16);
            }

            //*** Current usage *** Pos=+19=TRUE
            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CCU.sessionOpenTime.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;
            CCU.sessionOpenVehicle.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage = fs.ReadByte();
            temp = new byte[13];
            fs.Read(temp, 0, 13);
            CCU.sessionOpenVehicle.vehicleRegistrationNumber.vehicleRegNumber =
                Encode(CCU.sessionOpenVehicle.vehicleRegistrationNumber.codePage, temp);
            CarretPositionUniversal += 14;

            //*** Control Activity Record *** Pos=+46=38?=46=TRUE
            byte t = (byte) fs.ReadByte();
            CCADR.controlType.controlType = t.ToString();
            CarretPositionUniversal++;
            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CCADR.controlTime.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;
            CCADR.controlCardNumber.cardType.equipmentType = fs.ReadByte();
            CCADR.controlCardNumber.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal += 2;
            temp = new byte[16];
            fs.Read(temp, 0, 16);
            CarretPositionUniversal += 16;
            CCADR.controlCardNumber.cardNumber.cardRenewalIndex.cardRenewalIndex = (char) temp[15];
            CCADR.controlCardNumber.cardNumber.cardReplacementIndex.cardReplacementIndex = (char) temp[14];
            var tem = new byte[13];
            Array.Copy(temp, 0, tem, 0, 14);
            CCADR.controlCardNumber.cardNumber.driverIdentification = BitConverter.ToString(tem);

            CCADR.controlVehicleRegistration.vehicleRegistrationNation.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage = fs.ReadByte();
            fs.Read(tem, 0, 13);
            CarretPositionUniversal += 14;
            CCADR.controlVehicleRegistration.vehicleRegistrationNumber.vehicleRegNumber =
                Encode(CCADR.controlVehicleRegistration.vehicleRegistrationNumber.codePage, tem);

            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CCADR.controlDownloadPeriodBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CCADR.controlDownloadPeriodEnd.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            //*** Specific Conditions *** Pos+=280=TRUE
            SCR.scRec = new SpecificConditionRecord[1];
            for (var i = 0; i < 2; i++)
            {
                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CarretPositionUniversal += 4;
                SCR.scRec[i].entryTime.timeSec = BitConverter.ToInt32(temp, 0);

                SCR.scRec[i].specificConditionType.specificConditionType = (byte) fs.ReadByte();
                CarretPositionUniversal++;
            }
        }

        private void CompanyCardInput(FileStream fs, CompanyCardApplicationIdentification CCAI, CardCertificate CC,
            MemberStateCertificate MSC, CardIdentification CI, CompanyCardHolderIdentification CCHI,
            CompanyActivityData CAD)
        {
            // ***DriverCardIdentification*** Pos=+5=TRUE
            CCAI.typeOfTachographCardId.equipmentType = 1;
            CarretPositionUniversal++;

            fs.Read(CCAI.cardStructureVersion, 0, 2);
            CarretPositionUniversal += 2;

            var temp = new byte[2];
            fs.Read(temp, 0, 2);
            CarretPositionUniversal += 2;
            CCAI.noOfCompanyActivityRecords = BitConverter.ToInt16(temp, 0);

            // ***Card Certificate*** Pos=+194=TRUE
            fs.Read(CC.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            fs.Read(CC.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            CC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[3];
            fs.Read(temp, 0, 3);
            CC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            CC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(CC.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            CC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Member State Certificate*** Pos=+194=TRUE
            fs.Read(MSC.certificate.Sign, 0, 128);
            CarretPositionUniversal += 128;
            fs.Read(MSC.certificate.Cndash, 0, 58);
            CarretPositionUniversal += 58;
            MSC.certificate.certificateAuthorityReferenceerty.nationNumeric.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[3];
            fs.Read(temp, 0, 3);
            MSC.certificate.certificateAuthorityReferenceerty.nationAlpha.nationAlpha = BitConverter.ToString(temp);
            CarretPositionUniversal += 3;
            MSC.certificate.certificateAuthorityReferenceerty.keySerialNumber = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(MSC.certificate.certificateAuthorityReferenceerty.additionalInfo, 0, 2);
            CarretPositionUniversal += 2;
            MSC.certificate.certificateAuthorityReferenceerty.caIdentifier = fs.ReadByte();
            CarretPositionUniversal++;

            // ***Card Identification*** Pos=+65=TRUE
            CI.cardIssuingMemberState.nationNumeric = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[16];
            fs.Read(temp, 0, 16);
            CarretPositionUniversal += 16;
            CI.cardNumber = BitConverter.ToString(temp);

            CI.cardIssuingAuthorityName.codePage = fs.ReadByte();
            CarretPositionUniversal++;

            temp = new byte[35];
            fs.Read(temp, 0, 35);
            CI.cardIssuingAuthorityName.name = Encode(CI.cardIssuingAuthorityName.codePage, temp);
            CarretPositionUniversal += 35;

            temp = new byte[4];
            fs.Read(temp, 0, 4);
            CI.cardIssueDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CI.cardValidityBegin.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            fs.Read(temp, 0, 4);
            CI.cardExpiryDate.timeSec = BitConverter.ToInt32(temp, 0);
            CarretPositionUniversal += 4;

            // *** Company Card Holder Identification *** Pos=+146=TRUE
            CCHI.companyName.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            temp = new byte[35];
            fs.Read(temp, 0, 35);
            CCHI.companyName.name = Encode(CCHI.companyName.codePage, temp);
            CarretPositionUniversal += 35;

            CCHI.companyAddress.codePage = fs.ReadByte();
            CarretPositionUniversal++;
            fs.Read(temp, 0, 35);
            CCHI.companyAddress.address = Encode(CCHI.companyAddress.codePage, temp);
            CarretPositionUniversal += 35;

            temp = new byte[2];
            fs.Read(temp, 0, 2);
            CCHI.cardHolderPreferredLanguage.PLanguage = BitConverter.ToString(temp).ToCharArray();
            CarretPositionUniversal += 2;

            //*** Company Card Control Activity Data ***
            temp = new byte[2];
            fs.Read(temp, 0, 2);
            CAD.companyPointerNewestRecord = BitConverter.ToInt16(temp, 0);
            CarretPositionUniversal += 2;

            CAD.cRecs = new CompanyActivityRecord[CCAI.noOfCompanyActivityRecords - 1];

            for (var i = 0; i < CCAI.noOfCompanyActivityRecords; i++) //Pos=+105=TRUE
            {
                CAD.cRecs[i].companyActivityType.companyActivityType = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CAD.cRecs[i].companyActivityTime.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                CAD.cRecs[i].cardNumberInformation.cardType.equipmentType = fs.ReadByte();
                CarretPositionUniversal++;

                CAD.cRecs[i].cardNumberInformation.cardIssuingMemberState.nationNumeric = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[13];
                fs.Read(temp, 0, 13);
                CAD.cRecs[i].cardNumberInformation.cardNumber.ownerIdentification = BitConverter.ToString(temp);
                CarretPositionUniversal += 13;

                CAD.cRecs[i].vehicleRegistrationInformation.vehicleRegistrationNumber.codePage = fs.ReadByte();
                CarretPositionUniversal++;

                temp = new byte[13];
                fs.Read(temp, 0, 13);
                CAD.cRecs[i].vehicleRegistrationInformation.vehicleRegistrationNumber.vehicleRegNumber =
                    Encode(CAD.cRecs[i].vehicleRegistrationInformation.vehicleRegistrationNumber.codePage, temp);
                CarretPositionUniversal += 13;

                temp = new byte[4];
                fs.Read(temp, 0, 4);
                CAD.cRecs[i].downloadPeriodBegin.timeSec = BitConverter.ToInt32(temp, 0);
                CarretPositionUniversal += 4;

                fs.Read(temp, 0, 4);
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

    internal class _BLOCK
    {
        public byte[] ID = new byte[2];
        public int Length = 0;

        public void SetLength(byte[] bytes)
        {
            Length = (bytes[2] << 0) | (bytes[1] << 8) | (bytes[0] << 16);
        }

        private string[] blockHexConst = new[] { "00-02", "00-05", "05-01", "C1-00", "C1-08", "05-20", "05-0E", "05-21", "05-02", "05-03", 
            "05-04", "05-05", "05-06", "05-07", "05-08", "05-22" };
        private int[] blockLengthConst = new[] { 25, 8, 10, 194, 194, 143, 4, 53, 1728, 1152, 13780, 6202, 1121, 19, 46, 280 };
        private string[] blockNamesConst = new[] { "EF ICC", "EF IC", "EF Application_Identification", "EF Card_Certificate", 
            "EF CA_Certificate", "EF Identification", "EF Card_Download", "EF Driving_License_Info", "EF Events_Data", "EF Faults_Data", 
            "EF Driver_Activity_Data", "EF Vehicles_Used", "EF Places", "EF Current_Usage", "EF Control_Activity_Data", 
            "EF Specific_Conditions" };
        public void isBlockMatch()
        {
            string id = BitConverter.ToString(ID);
            bool broken = false;
            for (int i = 0; i < blockHexConst.Length - 1; i++)
            {
                if (id == blockHexConst[i])
                {
                    if (blockLengthConst[i] != Length)
                    {
                        if (i<9|i>13)
                        {
                            Console.WriteLine("something is not OK with " + blockNamesConst[i] + ". Please, check it.");
                        }
                        else { if (blockLengthConst[i] < Length) { Console.WriteLine("something is not OK with " + blockNamesConst[i] + ". Please, check it."); } }
                    }
                    broken = true;
                    break;
                }
            }
            if (!broken) { Console.WriteLine("No match for the block_id=" + id); }
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

    internal class MemberStateCertificate
    {
        public Certificate certificate = new Certificate();
    }

    internal class MemberStatePublicKey
    {
        public PublicKey memberStatePublicKey = new PublicKey();
    }

    internal class Name
    {
        public int codePage { get; set; }
        public string name { get; set; }
    }

    internal class NationAlpha
    {
        public string nationAlpha { get; set; }

        public string getVerboseAlpha()
        {
            switch (nationAlpha)
            {
                case "A":
                    return " Austria";
                case "AL":
                    return " Albania";
                case "ARM":
                    return " Armenia";
                case "AND":
                    return " Andorra";
                case "AZ":
                    return " Azerbaijan";
                case "B":
                    return " Belgium";
                case "BG":
                    return " Bulgaria";
                case "BIH":
                    return " Bosnia and Herzegovina";
                case "BY":
                    return " Belarus";
                case "CH":
                    return "  witzerland";
                case "CY":
                    return " Cyprus";
                case "CZ":
                    return " Czech Republic";
                case "D":
                    return " Germany";
                case "DK":
                    return " Denmark";
                case "E":
                    return " Spain";
                case "EST":
                    return " Estonia";
                case "F":
                    return " France";
                case "FIN":
                    return " Finland";
                case "FL":
                    return " Liechtenstein";
                case "UK":
                    return " United Kingdom, Alderney, Guernsey, Jersey, Isle of Man, Gibraltar";
                case "GE":
                    return " Georgia";
                case "GR":
                    return " Greece";
                case "H":
                    return " Hungary";
                case "HR":
                    return " Croatia";
                case "I":
                    return " Italy";
                case "IRL":
                    return " Ireland";
                case "IS":
                    return " Iceland";
                case "KZ":
                    return " Kazakhstan";
                case "L":
                    return " Luxembourg";
                case "LT":
                    return " Lithuania";
                case "LV":
                    return " Latvia";
                case "M":
                    return " Malta";
                case "MC":
                    return " Monaco";
                case "MD":
                    return " Republic of Moldova";
                case "MK":
                    return " Macedonia";
                case "N":
                    return " Norway";
                case "NL":
                    return " Netherlands";
                case "P":
                    return " Portugal";
                case "PL":
                    return " Poland";
                case "RO":
                    return " Romania";
                case "RSM":
                    return " San Marino";
                case "RUS":
                    return " Russia";
                case "S":
                    return " Sweden";
                case "SK":
                    return " Slovakia";
                case "SLO":
                    return " Slovenia";
                case "TM":
                    return " Turkmenistan";
                case "TR":
                    return " Turkey";
                case "UA":
                    return " Ukraine";
                case "V":
                    return " Vatican City";
                case "YU":
                    return " Yugoslavia";
                case "UNK":
                    return " Unknown";
                case "EC":
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
    {
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

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Write("Enter filename: ");
            var fname = Console.ReadLine();
            var lib = new Library();
            lib.FileOpenReadDecode(fname);
            //Output call here!
        }
    }

    internal class PublicKey
    {
        public RSAKeyModulus rsaKeyModulus = new RSAKeyModulus();
        public RSAKeyPublicExponent rsaKeyPublicExponent = new RSAKeyPublicExponent();
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

    internal class SensorOSIdentifier
    {
        public string sensorOSIdentifier = "";
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

    internal class SensorSCIdentifier
    {
        public string sensorSCI = "";
    }

    internal class SensorSerialNumber
    {
        public ExtendedSerialNumber sensorSN = new ExtendedSerialNumber();
    }

    internal class Signature
    {
        public string signature = "";
    }

    internal class SimilarEventsNumber
    {
        public int similarEventsNumber = 0;
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
    {
        public int timeSec { get; set; }

        public string ConvertToUTC()
        {
            var t = timeSec;
            var baseDT = new DateTime(1970, 1, 1, 0, 0, 0);
            baseDT.AddSeconds(t);
            return baseDT.ToString("dd-MMM-yyyy HH:mm:ss");
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

    internal class VehicleRegistrationIdentification
    {
        public NationNumeric vehicleRegistrationNation = new NationNumeric();
        public VehicleRegistrationNumber vehicleRegistrationNumber = new VehicleRegistrationNumber();
    }

    internal class VehicleRegistrationNumber
    {
        public int codePage;
        public string vehicleRegNumber;
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

    /// <summary>
    ///     Here ends CardBlocks
    ///     Here happens VuBlock
    /// </summary>
    internal class VuActivityDailyData
    {
        public ActivityChangeInfo activityChangeInfos = new ActivityChangeInfo();
        public int noOfActivityChanges;
    }

    internal class VuApprovalNumber
    {
        public string vuApprovalNumber = "";
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

    internal class VuDataBlockCounter
    {
        public int vuDataBlockCounter;
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

    internal class WorkshopCardPIN
    {
        public string workshopCardPIN = "";
    }

    class BlockIDReader
    {
        public void BlockIDRead(FileStream fs)
        {
            var ba = new _BLOCK();
            fs.Read(ba.ID, 0, 2);
            var b = new byte[3];
            fs.Read(b, 0, 3);
            ba.SetLength(b);
            ba.isBlockMatch();
        }
    }
}