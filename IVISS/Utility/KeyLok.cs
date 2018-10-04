using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;



namespace IVISS

{
	/// <summary>
	/// A class to interface to the Microcomputer Applications Inc. keylok dongle. This code has only
	/// been tested with the USB version - variations may be needed to work with the parallel port dongle version.
	/// To use this class, the KL2DLL64.DLL file must be somewhere the application's EXE can find it (eg: in the
	/// working directory when the application is run, or in the windows system directory).
	/// </summary>
	public class keylok
	{

		/// <summary>
		/// Platform invoke declaration for keylok interface KFUNC function.
		/// </summary>
		[DllImport("KL2DLL64.DLL", CharSet=CharSet.Auto)]
		private static extern uint KFUNC(int arg1,int arg2,int arg3,int arg4);

        [DllImport("KL2DLL64.DLL", CharSet = CharSet.Auto)]
        private static extern uint KEYBD(int arg1);

        [DllImport("KL2DLL64.DLL", CharSet = CharSet.Ansi)]
        private static extern uint KBLOCK(uint task, uint address, uint byteCount, ushort [] pArray);

        [DllImport("KL2DLL64.DLL", CharSet = CharSet.Ansi)]
        private static extern uint KEXEC(string ExeDir, string ExeFile, string UserPin, StringBuilder InBuffer, short SizeBuffer);

        [DllImport("KL2DLL64.DLL", CharSet = CharSet.Ansi)]
        public static extern void KGETGUSN(IntPtr pArray);

        [DllImport("KL2DLL64.DLL", CharSet = CharSet.Ansi)]
        public static extern UInt32 KGETSNS(IntPtr pSNArray, uint iMaxSN);

        [DllImport("KL2DLL64.DLL", CharSet = CharSet.Ansi)]
        public static extern ulong GETLASTKEYERROR();

        public static ushort LaunchAntiDebugger = 0;
        public static uint MaxSNSupported = 12;

		/// <summary>
		/// Company specific codes go here.
		/// These are demonatration codes and will be replaced when you received your production dongles.
		/// </summary> 
        private static ushort ValidateCode1 = 0x9AB8;		// These three codes passed to the dongle as the first part // 0X488B
        private static ushort ValidateCode2 = 0xB8D0;		// of its authentication sequence.
        private static ushort ValidateCode3 = 0x7A00;

        private static ushort ClientIDCode1 = 0xEAC4;		// These values are returned by the dongle as the result of the authentication 
        private static ushort ClientIDCode2 = 0x6DD3;		// sequence. We should check that the returned value matches these values. 

        private static ushort ReadCode1 = 0x24A0;		// These values must be passed to the dongle to authorize read operations
        private static ushort ReadCode2 = 0xF6A0;		// of its non-volatile RAM.
        private static ushort ReadCode3 = 0x2C6D;

        private static ushort WriteCode1 = 0x2E6;		// These values must be passed to the dongle to authorize write operations
        private static ushort WriteCode2 = 0xF5B;		// to its non-volatile RAM.
        private static ushort WriteCode3 = 0x9ABE;

        //private static ushort ValidateCode1 = 0X488B;		// These three codes passed to the dongle as the first part // 0X488B
        //private static ushort ValidateCode2 = 0XFEE2;		// of its authentication sequence.
        //private static ushort ValidateCode3 = 0XEF90;

        //private static ushort ClientIDCode1 = 0XD882;		// These values are returned by the dongle as the result of the authentication 
        //private static ushort ClientIDCode2 = 0X5E01;		// sequence. We should check that the returned value matches these values. 

        //private static ushort ReadCode1 = 0X1772;		// These values must be passed to the dongle to authorize read operations
        //private static ushort ReadCode2 = 0XC4E6;		// of its non-volatile RAM.
        //private static ushort ReadCode3 = 0XBCF8;

        //private static ushort WriteCode1 = 0X30A0;		// These values must be passed to the dongle to authorize write operations
        //private static ushort WriteCode2 = 0X3C89;		// to its non-volatile RAM.
        //private static ushort WriteCode3 = 0X0A2B;
        
        
        //Global Variables
        private static ulong ReturnValue;
        private static int ReturnValue1;
        private static int ReturnValue2;
        public static int DongleType;
        static Random random = new Random();

        		
		/// <summary>
		/// The following function (operation) codes are used as the first argument to the KFUNC() interface function in the DLL
		/// to specify the desired operation to perform.
		/// </summary>
        private static int TERMINATE = -1;
        private static int KLCHECK = 1;
        private static int READAUTH = 2;
        private static int GETSN = 3;
        private static int GETVARWORD = 4;
        private static int WRITEAUTH = 5;
        private static int WRITEVARWORD = 6;
        private static int DECREMENTMEM = 7;
        private static int GETEXPDATE = 8;
        private static int CKLEASEDATE = 9;
        private static int SETEXPDATE = 10;
        private static int SETMAXUSERS = 11;
        private static int GETMAXUSERS = 12;
        private static int DOREMOTEUPDATE = 21;
        private static int SETLONGSN = 31;
        private static int GETABSOLUTEMAXUSERS = 32;
        private static int GETDONGLETYPE = 33;
        private static int CKREALCLOCK = 82; 
        private static uint  READBLOCK      =   84;
        private static uint  WRITEBLOCK     =   85;
        private static short SELECTTARGETSN =   88;
        private static int GETLONGSN = 89;
        private static int LEDON = 90;
        private static int LEDOFF = 91;



        public const long KEY_ERROR_NOERROR = 0;
        public const long KEY_ERROR_FORTRESS_NOAUTHENTICATION = 536871681; //KEYLOK Authentication has not been performed
        public const long KEY_ERROR_FORTRESS_NOFOLDER = 536871682; //Client subfolder with desired program not found
        public const long KEY_ERROR_FORTRESS_WRONGPIN = 536871683; //Client subfolder with desired program not found
        public const long KEY_ERROR_EXE_ERROR = 536871684; //Client EXE Error
        public const long KEY_ERROR_NO_REALCLOCK = 536870936;  // No RTC on board
        public const long KEY_ERROR_RTC_NO_POWER = 536870937;  // RTC has been powered down (battery has lost power)



        private static int RandomNumber(int min, int max)
        {
            return random.Next(min, max);
        }

		private static ushort RotateLeft(ushort val,int n)
		{
			int ival=((int)val)<<n;
			return (ushort)((ival & 0xffff) | (ival>>16));
		}
		
		/// <summary>
		/// Checks for presence of dongle. Note that using this function resets read/write authorization
		/// state bits in the dongle, so read/write must be re-authorized after calls to this function.
		/// </summary>
		/// <returns>true if the dongle is present and the authorization sequence passes.</returns>
		public static bool IsPresent()
		{
            //KEYBD(LaunchAntiDebugger);

			// First call DLL with 3 validate codes
			uint retval=KFUNC(KLCHECK,ValidateCode1,ValidateCode2,ValidateCode3);
			ushort retLow=(ushort)(retval & 0xffff);
			ushort retHigh=(ushort)(retval >> 16);

			// Next call DLL with first call result processed with canned logic
			retval=KFUNC(
				RotateLeft(retLow,retHigh & 7) ^ ReadCode3 ^ retHigh,
				RotateLeft(retHigh,retLow & 15),
				(ushort)(retLow ^ retHigh),
				0
			);
			retLow=(ushort)(retval & 0xffff);
			retHigh=(ushort)(retval >> 16);

			// If all is well, the returned value will match the client id code.
			if (retLow==ClientIDCode1 && retHigh==ClientIDCode2)
				return true;
			return false;
		}
		public static void Reset()
		{
            uint retval = KFUNC(TERMINATE, 0, 0, 0);
            retval=KFUNC(KLCHECK,0,0,0);
		}
        public static void GetDongleType()
        {
            keylok.DongleType = (int)KFUNC(GETDONGLETYPE, RandomNumber(0, 65535), RandomNumber(0, 65535), RandomNumber(0, 65535));
            return;
        }

		/// Authorizes read operations on the dongle's non-volatile RAM. This must be called after
		/// calling IsPresent() which resets the dongle's internal read authorization state.
		public static void AuthorizeRead()
		{
			KFUNC(READAUTH,ReadCode1,ReadCode2,ReadCode3);
		}
        /// Reads the 16-bit serial number from the dongle
        public static ushort ReadSerialNo()
        {
            uint retval = KFUNC(GETSN, 0, 0, 0);
            return (ushort)retval;
        }
        /// Reads a 16-bit value from one of the 56 registers in the dongle's non-volatile RAM.
        public static ushort ReadRegister(int reg)
        {
            if (reg < 0 && reg > 55)
                return 0;
            uint retval = KFUNC(GETVARWORD, reg, 0, 0);
            return (ushort)retval;
        }
        //Fortress and KEYLOK 3 support a user supplied custom serial number
        public static ulong ReadCustomSerialNo()
        {
            ulong retval = KFUNC(GETLONGSN, 0, 0, 0);
            return retval;
        }

        
        
        
        /// Authorizes write operations on the dongle's non-volatile RAM. This must be called after
		/// calling AuthorizeRead() which resets the dongle's internal write authorization state.
		public static void AuthorizeWrite()
		{
			KFUNC(WRITEAUTH,WriteCode1,WriteCode2,WriteCode3);
		}

        //Fortress and KEYLOK 3 support a user supplied custom serial number
        public static void WriteCustomSerialNo(ulong sn)
        {
            ulong retval = KFUNC(SETLONGSN, (int)(sn>>16), (int)sn, 0);
            return;
        }
		/// Writes a 16-bit value to one of the 56 registers in the dongle's non-volatile RAM.
		public static void WriteRegister(int reg,ushort val)
		{
			if (reg<0 && reg>55)
				return;
			KFUNC(WRITEVARWORD,reg,val,0);
		}
        // Decrement memory decreases the count of a memory address by 1.  Used for licensing and counter purposes.
        public static void DecrementMemory(int reg)
        {
            string Title;
            uint longRV;

            Title = "KEYLOK Decrement Function";
            KTASK(DECREMENTMEM, reg, 0, 0);

            switch (ReturnValue2)
            {
                case 0: //Success
                    if (ReturnValue1 == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("CAUTION: This counter is fully counted down to zero.", Title);
                    }
                    else
                    {
                        if (ReturnValue1 < 0)
                        {
                            longRV = (uint)ReturnValue1 + 2 ^ 16;
                        }
                        else
                        {
                            longRV = (uint)ReturnValue1;
                        }
                        System.Windows.Forms.MessageBox.Show("NOTE: There are " + longRV.ToString() + " decimal count(s) remaining for this usage.", Title);
                    }
                    break;
                case 1: //No counts left
                    System.Windows.Forms.MessageBox.Show("ERROR: This counter is already fully counted down to zero.", Title);
                    break;
                case 2: //address out of range
                    System.Windows.Forms.MessageBox.Show("ERROR: Address is out of range.", Title);
                    break;
                case 3: //no write authorization
                    System.Windows.Forms.MessageBox.Show("ERROR: Memory write has not been authorized.", Title);
                    break;
            }
        }        


        //Network Functions
        public static void SetMaxUsers(int MaxUsers)
        {
            KFUNC(SETMAXUSERS, MaxUsers, 0, 0);
        }

        public static ushort GetMaxUsers()
        {
            uint retval = KFUNC(GETMAXUSERS, 0, 0, 0);
            return (ushort)retval;
        }

        public static ushort GetAbsMaxUsers()
        {
            uint retval = KFUNC(GETABSOLUTEMAXUSERS, 0, 0, 0);
            return (ushort)retval;
        }


        //Remote Update API 
        public static void RemoteUpdate(String strLocation) 
        {
            if (strLocation == "")
            {
                keylok.KTASK(DOREMOTEUPDATE, 0, 0, 0);
            }
            else
            {
                IntPtr strLocationPtr = IntPtr.Zero;
            // use marshaling to copy the managed string data to unmanaged memory
                strLocationPtr = Marshal.StringToHGlobalAnsi(strLocation);

            // convert the pointer to an integer value
                int nStringAddress = strLocationPtr.ToInt32();

            // call the API providing the address of the string as our integer
                keylok.KTASK(DOREMOTEUPDATE, 1357, 0, nStringAddress);

            // don't forget to free the memory
                if (strLocationPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(strLocationPtr);
                }
            }

            if (ReturnValue1 == 0)
                System.Windows.Forms.MessageBox.Show("The dongle has been updated.");
            else
                System.Windows.Forms.MessageBox.Show("Error processing Authorize.dat.  Use GetLastKeyError().");
            return;
        }
        
        
        //Expiration Date functions
        public static void SetExpDate(String MonthValStr, String DayValStr, String YearValStr)
        {
            long WriteDate;
            int WriteDateInt;
            int DayVal;
            int MonthVal;
            int YearVal;
            int BaseYear;
            BaseYear = 1990;
            MonthVal = 0;
            DayVal = 0;
            YearVal = 0;
            try
            {
                MonthVal = System.Convert.ToInt32(MonthValStr);
                if (MonthVal == 0)
                    throw new System.Exception();

                DayVal = System.Convert.ToInt32(DayValStr);
                if (DayVal == 0)
                    throw new System.Exception();
                YearVal = System.Convert.ToInt32(YearValStr);
                if (YearVal == 0)
                    throw new System.Exception();
                WriteDate = 512 * (YearVal - BaseYear) + 32 * MonthVal + DayVal;
                if (WriteDate > 32767) WriteDate = WriteDate - 2 ^ 16;

                WriteDateInt = (int)WriteDate;
                keylok.KFUNC(SETEXPDATE, 0, WriteDateInt, 0);
            }
            catch
            {
                //Code for handling your exception goes here
            }
        }

        public static void CkExpDate()
        {
            //Lease expiration constants
            //const int LEASEOK = 65535;
            const int LEASEEXPIRED = 65534;
            const int SYSDATESETBACK = 65533;
            const int NOLEASEDATE = 65532;
            const int LEASEDATEBAD = 65531;
            const int LASTSYSDATECORRUPT = 65530;
            const String Title = "LEASE DATE CHECK";
            String msg;
            int DateRead;
            String DaysLeft;
            int SystemDay;
            int SystemMonth;
            int SystemYear;
            int ExpirationDay;
            int ExpirationMonth;
            int ExpirationYear;
            int BaseYear = 1990;
            KTASK(CKLEASEDATE, 0, 0, 0);
            switch (ReturnValue2)
            {
                case LEASEEXPIRED: //Lease has expired
                    System.Windows.Forms.MessageBox.Show("The lease associated with the use of this software has expired.", Title);
                    break;
                case SYSDATESETBACK: //System date is earlier than one set in KEY-LOK
                    System.Windows.Forms.MessageBox.Show("WARNING: The system clock has been set back to an earlier date.", Title);
                    break;
                case NOLEASEDATE: //No lease expiration date has been programmed
                    System.Windows.Forms.MessageBox.Show("The KEY-LOK has not been programmed with a lease expiration date.", Title);
                    break;
                case LEASEDATEBAD: //An invalid lease expiration date exists
                    System.Windows.Forms.MessageBox.Show("The programmed lease date is in the past.  Please reprogram with a future date.", Title);
                    break;
                case LASTSYSDATECORRUPT: //Last system date as stored in device has become corrupt
                    System.Windows.Forms.MessageBox.Show("The 'last system date' stored in the KEY-LOK is corrupt.", Title);
                    break;
                default: //Default - Lease has not expired
                    //Lease has not expired
                    //Convert current date returned by security system to readable format
                    DateRead = ReturnValue1; //YYYYYYYM MMMDDDDD
                    DaysLeft = System.Convert.ToString(ReturnValue2);
                    SystemYear = BaseYear + (int)(DateRead / 512);
                    SystemMonth = (int)((DateRead & 480) / 32);
                    SystemDay = DateRead & 31;
                    msg = "The system reports the date as :" + System.Convert.ToString(SystemMonth) + "/" + System.Convert.ToString(SystemDay) + "/" + System.Convert.ToString(SystemYear) + "." + (char)(10) + (char)(13);
                    //Acquire and Convert lease expiration date to readable format from storage format
                    KTASK(GETEXPDATE, 0, 0, 0);
                    DateRead = ReturnValue1; //YYYYYYYM MMMDDDDD
                    ExpirationYear = BaseYear + (int)(DateRead / 512);
                    ExpirationMonth = (int)((DateRead & 480) / 32);
                    ExpirationDay = DateRead & 31;
                    msg = msg + "The lease expiration date is    :" + System.Convert.ToString(ExpirationMonth) + "/" + System.Convert.ToString(ExpirationDay) + "/" + System.Convert.ToString(ExpirationYear) + "." + (char)(10) + (char)(13);
                    //Display approximate number of days until lease expires
                    //The number of days is computed by comparing the aggregate days associated with both
                    //the system date and lease expiration dates.  Aggregate days is computed by
                    //multiplying 365 x & years, plus 30 x & months, plus the day of the month, plus one
                    //day for each quarter within a partial year.  As the two dates come closer together
                    //the accuracy of the computation generally improves.  If more accuracy is required
                    //contact KEYLOK technical support to receive a program
                    //enhancement.
                    System.Windows.Forms.MessageBox.Show(msg + "The software lease will expire in approximately " + System.Convert.ToString(DaysLeft) + " days.", Title);
                    break;

            }
        }

        //Fortress Only Functions
        //Get Global Unique Serial Number
        public static string GetGlobalID()
        {
            String GUSN;
            UInt64 GUSNHi = 0;
            UInt64 GUSNLo = 0;
            Int32[] SNArray = new Int32[3];
            Int32 Size = Marshal.SizeOf(SNArray[0]) * SNArray.Length;

            IntPtr array_addr = Marshal.AllocHGlobal(Size);
            Marshal.Copy(SNArray, 0, array_addr, SNArray.Length);
            keylok.KGETGUSN(array_addr);
            Marshal.Copy(array_addr, SNArray, 0, SNArray.Length);
            if (SNArray[0] < 0)
                GUSNHi = (UInt64)(SNArray[0] + 4294967296);
            else
                GUSNHi = (UInt64)SNArray[0];
            if (SNArray[1] < 0)
                GUSNLo = (UInt64)(SNArray[1] + 4294967296);
            else
                GUSNLo = (UInt64)SNArray[1];

            GUSN = String.Format("{0:x2}", GUSNHi) + (String.Format("{0:x2}", GUSNLo)).PadLeft(8, '0');
            Marshal.FreeHGlobal(array_addr);
            return GUSN;
        }
        //LED API Calls
        public static void LEDOn()
        {
            KFUNC(LEDON, RandomNumber(0, 65535), RandomNumber(0, 65535), RandomNumber(0, 65535));
        }
        public static void LEDOff()
        {
            KFUNC(LEDOFF, RandomNumber(0, 65535), RandomNumber(0, 65535), RandomNumber(0, 65535));
        }
        // Search for Fortress dongles attached to enable communication with a specific dongle
        public static UInt32 GetFortressDongles()
        {
            Int32[] SNArray = new Int32[3];
            Int32 Size = Marshal.SizeOf(SNArray[0]) * SNArray.Length;
            UInt32 DongleCount = 0;
            IntPtr array_addr = Marshal.AllocHGlobal(Size);
            Marshal.Copy(SNArray, 0, array_addr, SNArray.Length);
            DongleCount = keylok.KGETSNS(array_addr, MaxSNSupported);
            Marshal.Copy(array_addr, SNArray, 0, SNArray.Length);
            Marshal.FreeHGlobal(array_addr);
            return DongleCount;
        }
        public static void SelectTargetDongle(int iSNIndex)
        {
            KFUNC(SELECTTARGETSN, iSNIndex, RandomNumber(0, 65535), RandomNumber(0, 65535));
        }
        //Block Read API Call
        public static void KLReadBlock(uint StartAddress, uint Length)
        {
            ushort[] usData = new ushort[Length];
            ushort i = 0;
            StringBuilder sMemContents = new StringBuilder(1000);
            string msg;
            keylok.KBLOCK(READBLOCK, StartAddress, Length, usData);
            while (i < Length)
            {
                sMemContents.Append(usData[i].ToString());
                sMemContents.Append(", ");
                i++;
            }
            msg = "The current content of address is" + Environment.NewLine + sMemContents;
            MessageBox.Show(msg, "KEYLOK Memory Contents");
        }
        //Block Write API Call
        public static void KLWriteBlock(uint StartAddress, uint sWriteBlock)
        {
            ushort[] usData = new ushort [sWriteBlock];
            ushort i=0;
            while (i < sWriteBlock)
            {
                usData[i] = (ushort)(sWriteBlock + i);
                i++;
            }
            keylok.KBLOCK(WRITEBLOCK, StartAddress, sWriteBlock, usData);
        }
        //API to call executable CodeVault code on Fortress dongle
        public static void KLKexec(string sExeFile, string sOrigString)
        {
            //KEXEC constants
            StringBuilder sSortString = new StringBuilder(sOrigString); 
            const string sExeDir = "\\7777";
            const string sUserPin = "12345678";
            long lStatus;
            uint UIStatus;
            string msg;
            short bufferSize;
            bufferSize = (short)sSortString.Length;
            UIStatus = KEXEC(sExeDir, sExeFile, sUserPin, sSortString, bufferSize);
            lStatus = UIStatus;

            switch (lStatus)
            {
                case keylok.KEY_ERROR_NOERROR:
                    msg = "The unsorted string is :" + sOrigString + Environment.NewLine + "The sorted string is :" + sSortString;
                    break;

                case keylok.KEY_ERROR_FORTRESS_NOFOLDER:
                    msg = "Client Folder with desired program not found.";
                    break;

                case keylok.KEY_ERROR_FORTRESS_WRONGPIN:
                    msg = "Client Folder with desired program will not authenticate.";
                    break;

                case keylok.KEY_ERROR_FORTRESS_NOAUTHENTICATION:
                    msg = "Successful authentication is a prerequisite.";
                    break;

                case keylok.KEY_ERROR_EXE_ERROR:
                    msg = "Executable error.";
                    break;

                default:
                    msg = "Unidentified error condition." + lStatus.ToString();
                    break;
            }
            MessageBox.Show(msg,"KEYLOK - Call Client EXE");
        }     
        // Real Time Clock API
        public static void CkRTC()
        {
            UInt64 status;
            Double lRTCSeconds;
            String msg;

            KTASK(CKREALCLOCK, 0, 0, 0);

            status = GETLASTKEYERROR();
            switch (status)
            {
                case KEY_ERROR_NOERROR:
                    lRTCSeconds = ReturnValue2 * 65536 + ReturnValue1;
                    DateTime myDateTime = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
                    myDateTime = myDateTime.AddSeconds(lRTCSeconds);
                    msg = "The RTC reports: " + (myDateTime.ToLocalTime()).ToString();
                    break;
                case KEY_ERROR_NO_REALCLOCK: // No real time clock
                    msg = "No real time clock on this device.";
                    break;
                case KEY_ERROR_RTC_NO_POWER: // RTC has lost power
                    msg = "No real time clock on this device.";
                    break;
                default:
                    msg = "Unrecognized error: " + status.ToString();
                    break;
            }
            MessageBox.Show(msg, "KEYLOK Real-Time Clock");
        }




        //Common function to call KFUNC within DLL
        public static void KTASK(int Arg1, int Arg2, int Arg3, int Arg4)
        {
            //NOTICE: LastDLLError can be used to obtain error code returned by security
            //call.  A list of error codes can be found in the KEYERROR.H file created during
            //the expansion of the WIN32.EXE self-expanding file.
            long STARTANTIDEBUGGER = 0;

            long ReturnValue1Long;
            long LgArg1;
            long LgArg2;
            long LgArg3;
            long LgArg4;

            LgArg1 = Arg1;
            if (LgArg1 < 0)
            {
                LgArg1 = LgArg1 + 2 ^ 16;
            }
            LgArg2 = Arg2;
            if (LgArg2 < 0)
            {
                LgArg2 = LgArg2 + 2 ^ 16;
            }
            LgArg3 = Arg3;
            if (LgArg3 < 0)
            {
                LgArg3 = LgArg3 + 2 ^ 16;
            }
            LgArg4 = Arg4;
            if (LgArg4 < 0)
            {
                LgArg4 = LgArg4 + 2 ^ 16;
            }

            //Activate the next line of code in order to launch the anti-debugging utility PPMON.EXE.
            //You may wish to wait to activate this line until you are through debugging your
            //application from within the VBASIC IDE.
            //KeybdRet = KEYBD(STARTANTIDEBUGGER)
            ReturnValue = KFUNC((int)LgArg1, (int)LgArg2, (int)LgArg3, (int)LgArg4);
            //KTASK = ShowLastKeyError(Err.LastDLLError);
            ReturnValue1Long = (long)ReturnValue % 65536;
            ReturnValue1 = (int)ReturnValue1Long;
            ReturnValue2 = (int)(ReturnValue / 65536);
        }

 
	}
            
}
