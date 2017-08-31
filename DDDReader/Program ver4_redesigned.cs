using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DDDReader
{
    internal class Program1 { }

    
    internal class Functions
    {
        public byte[] Seeker()
        {
            
        }

        struct ByteConvert
        {
            public ulong ToInt(byte[] b)
            {
                int pos = 0;
                ulong res = 0;
                foreach (byte by in b)
                {
                    res |= ((ulong) by) << pos;
                    pos += 8;
                }
                return res;
            }

            public char ToChar(byte b)
            {
                return (char) b;
            }

            public char[] ToCharArray(byte[] b)
            {
                int pos = 0;
                char[] res={};
                foreach (byte by in b)
                {
                    res[pos] = (char)b[pos];
                    pos++;
                }
                return res;
            }

            public string ToString(byte[] b)
            {
                return ToCharArray(b).ToString();
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
                return (byte) c;
            }
            public byte GetByte(int i)
            {
                if (i < 256) return (byte) i;
                else return Byte.MinValue;
            }
            public byte[] GetBytes(int i)
            {
                if (i < 256) {byte[] temp = new byte[1]; temp[0] = (byte) i; return temp;}
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
                    bytes[j / 2] = Convert.ToByte(i.Substring(i, 2), 16);
                return bytes;
            }
        }
    }
    internal class Blocks
    {
        internal class SeekerBlockInstance
        {
            public byte[] headerBytes = new byte[1];
            public int lengthInBytes = 0;
            public byte[] blockDataBytes;

            
            private string[] blockHexConst = new[] { "00-02", "00-05", "05-01", "C1-00", "C1-08", "05-20", "05-0E", "05-21", "05-02", "05-03", 
            "05-04", "05-05", "05-06", "05-07", "05-08", "05-22" };
            private int[] blockLengthConst = new[] { 25, 8, 10, 194, 194, 143, 4, 53, 1728, 1152, 13780, 6202, 1121, 19, 46, 280 };
            private string[] blockNamesConst = new[] { "EF ICC", "EF IC", "EF Application_Identification", "EF Card_Certificate", 
            "EF CA_Certificate", "EF Identification", "EF Card_Download", "EF Driving_License_Info", "EF Events_Data", "EF Faults_Data", 
            "EF Driver_Activity_Data", "EF Vehicles_Used", "EF Places", "EF Current_Usage", "EF Control_Activity_Data", 
            "EF Specific_Conditions" };

        }


    }


    internal class Library
    {
        public static int CarretPositionUniversal;

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

        




        public void ControlCardInput(FileStream fs, ControlCardApplicationIdentification CCAI, CardCertificate CC,
            MemberStateCertificate MSC, CardIdentification CI, ControlCardHolderIdentification CCHI,
            ControlCardControlActivityData CCCAD)
        {
            
        }

        public void DriverCardInput(FileStream fs, DriverCardApplicationIdentification DCAI, CardCertificate CC,
            MemberStateCertificate MSC, CardIdentification CI, DriverCardHolderIdentification DCHI, LastCardDownload LCD,
            CardDrivingLicenseInformation CDLI, CardEventData CED, CardFaultData CFD, CardDriverActivity CDA,
            CardVehiclesUsed CVU, CardPlaceDailyWorkPeriod CPDWP, CardCurrentUse CCU,
            CardControlActivityDataRecord CCADR, SpecificConditionRecords SCR)
        {
            
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
            
        }

        private void CompanyCardInput(FileStream fs, CompanyCardApplicationIdentification CCAI, CardCertificate CC,
            MemberStateCertificate MSC, CardIdentification CI, CompanyCardHolderIdentification CCHI,
            CompanyActivityData CAD)
        {
            
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



    /// <summary>
    ///     Here ends CardBlocks
    ///     Here happens VuBlock
    /// </summary>


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

namespace DDDReader_t
{



}