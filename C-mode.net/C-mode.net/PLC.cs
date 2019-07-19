using C_mode.net.Exceptions;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace C_mode.net
{
    /// <summary>
    /// Omron PLC communication using serial COM and Cmode protocol.
    /// </summary>
    public class PLC
    {
        /// <summary>
        /// Enum containing endcodes for responses.
        /// </summary>
        private enum EndCodes
        {
            OK = 0,
            FCS = 13,
            Format = 14,
            EntryNum = 15,
            Length = 18,
            CPUUnit = 21
        }
        /// <summary>
        /// Enum containing header codes.
        /// </summary>
        private enum HeaderCodes
        {
            CIORead = 0,
            LRRead = 1,
            HRRead = 2,
            CNTRead = 3,
            CNTStatusRead = 4,
            DMRead = 5,
            ARRead = 6,
            EMRead = 7,
            CIOWrite = 8,
            LRWrite = 9,
            HRWrite = 10,
            CNTWrite = 11,
            DMWrite = 12,
            ARWrite = 13,
            EMWrite = 14
        }
        /// <summary>
        /// Enum containing the memory areas.
        /// </summary>
        public enum MemoryArea
        {
            AR,
            CIO,
            CNT,
            DM,
            HR,
            TIM,
            TK,
            WR
        }
        private static string[] headers = {
            "RR", "RL", "RH", "RC",
            "RG", "RD", "RJ", "RE",
            "WR", "WL", "WH", "WC",
            "WD", "WJ", "WE"
        };
        #region Fields
        private SerialPort serialPort;
        private int timeout = 500;
        private string address = "";
        #endregion
        #region Properties
        /// <summary>
        /// State of the connection. Returns true if connection exists.
        /// </summary>
        public bool Connected { get => serialPort.IsOpen; }
        /// <summary>
        /// Timeout in milliseconds. Timeout represents the amount of time to 
        /// wait for a response from the PLC.
        /// </summary>
        public int Timeout { get => timeout; set => timeout = value; }
        #endregion
        #region Constructors 
        /// <summary>
        /// Creates a new instance of PLC.
        /// </summary>
        /// <param name="COMPort">COM Port in string format.</param>
        public PLC(string COMPort)
        {
            this.address = COMPort;
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Open a connection channel.
        /// </summary>
        /// <param name="address">Name of the serial port.</param>
        /// <returns>True if the connection was successful.</returns>
        public bool Connect()
        {
            /*
             * If there's already a port open, close it.
             */
            serialPort?.Close();
            /*
             * Configure the serial port and open it. Return the status of the communication.
             */
            serialPort = new SerialPort(address);
            serialPort.DataBits = 7;
            serialPort.BaudRate = 9600;
            serialPort.StopBits = StopBits.Two;
            serialPort.Parity = Parity.Even;
            serialPort.Open();
            return serialPort.IsOpen;
        }
        /// <summary>
        /// Disconnect from the plc.
        /// </summary>
        public void Disconnect()
        {
            serialPort?.Close();
        }
        /// <summary>
        /// Get an array of values from the PLC.
        /// </summary>
        /// <param name="memoryArea">Memory area.</param>
        /// <param name="address">Start address for the read operation.</param>
        /// <param name="count">Count of values to read.</param>
        /// <returns>An array of 16 bit unsigned ints.</returns>
        public ushort[] GetData(MemoryArea memoryArea, ushort address, ushort count = 1)
        {
            /*
             * Build a message based on the given parameters
             */
            CmodeMessage cmodeMessage = new CmodeMessage();
            switch (memoryArea)
            {
                case MemoryArea.AR:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.ARRead], "", FormatUshortToString(address), FormatUshortToString(count));
                    break;
                case MemoryArea.CIO:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.CIORead], "", FormatUshortToString(address), FormatUshortToString(count));
                    break;
                case MemoryArea.CNT:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.CNTRead], "", FormatUshortToString(address), FormatUshortToString(count));
                    break;
                case MemoryArea.DM:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.DMRead], "", FormatUshortToString(address), FormatUshortToString(count));
                    break;
                case MemoryArea.HR:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.HRRead], "", FormatUshortToString(address), FormatUshortToString(count));
                    break;
                case MemoryArea.TIM:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.CNTRead], "", FormatUshortToString(address), FormatUshortToString(count));
                    break;
                case MemoryArea.TK:
                    throw new NotImplementedException("C-mode message format doesn't support TK area reading");
                case MemoryArea.WR:
                    throw new NotImplementedException("C-mode message format doesn't support WR area reading");
            }
            /*
             * Send message to the PLC.
             */
            serialPort.Write(cmodeMessage.GetMessage());
            /*
             * Wait for a response from the PLC. Insert this response into a C-modes message object.
             * Then, check if the FCS value fits.
             * In the last step, check if the PLC returned an error and if not, parse the received
             * message data and return the array of data.
             */
            cmodeMessage = new CmodeMessage(WaitGetMessage());
            if (!cmodeMessage.CheckFCS())
            {
                throw new FCSException("Received message has wrong FCS value");
            }
            EndCodes endCode = (EndCodes)HexaToInt(cmodeMessage.EndCode);
            if (endCode == EndCodes.OK)
            {
                return ParseResponse(cmodeMessage.ResponseText);
            }
            else
            {
                ThrowError(cmodeMessage, endCode);
                return null;
            }
        }
        /// <summary>
        /// Write an array of values onto the PLC.
        /// </summary>
        /// <param name="memoryArea">Memory area.</param>
        /// <param name="address">Start address of writing.</param>
        /// <param name="data">Array of 16 bit unsigned ints.</param>
        public void WriteData(MemoryArea memoryArea, ushort address, ushort[] data)
        {
            CmodeMessage cmodeMessage = new CmodeMessage();
            /*
             * Prepare the values to write in hexadecimal with
             * a length of 4 characters.
             */
            string dataString = "";
            for (int i = 0; i < data.Length; i++)
            {
                string sData = IntToHexa(data[i]);
                for (int j = sData.Length; j < 4; j++)
                {
                    sData = sData.Insert(0, "0");
                }
                dataString += sData;
            }
            /*
             * Build the message based on the given parameters.
             */
            switch (memoryArea)
            {
                case MemoryArea.AR:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.ARWrite], "", FormatUshortToString(address), dataString);
                    break;
                case MemoryArea.CIO:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.CIOWrite], "", FormatUshortToString(address), dataString);
                    break;
                case MemoryArea.CNT:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.CNTWrite], "", FormatUshortToString(address), dataString);
                    break;
                case MemoryArea.DM:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.DMWrite], "", FormatUshortToString(address), dataString);
                    break;
                case MemoryArea.HR:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.HRWrite], "", FormatUshortToString(address), dataString);
                    break;
                case MemoryArea.TIM:
                    cmodeMessage = new CmodeMessage("00", headers[(int)HeaderCodes.CNTWrite], "", FormatUshortToString(address), dataString);
                    break;
                case MemoryArea.TK:
                    throw new NotImplementedException("C-mode message format doesn't support TK area reading");
                case MemoryArea.WR:
                    throw new NotImplementedException("C-mode message format doesn't support WR area reading");
            }
            /*
             * Send the message to the PLC.
             */
            serialPort.Write(cmodeMessage.GetMessage());
            /*
             * Wait for a response from the PLC. Insert this response into a C-modes message object.
             * Then, check if the FCS value fits.
             * In the last step, check if the PLC returned an error and throw the error if that was 
             * the case.
             */
            cmodeMessage = new CmodeMessage(WaitGetMessage());
            if (!cmodeMessage.CheckFCS())
            {
                throw new FCSException("Received message has wrong FCS value");
            }
            EndCodes endCode = (EndCodes)HexaToInt(cmodeMessage.EndCode);
            if (endCode == EndCodes.OK)
            {
                return;
            }
            else
            {
                ThrowError(cmodeMessage, endCode);
            }
        }
        #endregion
        #region Private Methods
        private string WaitGetMessage()
        {
            /*
             * Keep reading bytes until the message start indicator is read.
             * Once reading is started, add to the string until the terminator characters are met.
             * Once terminator characters are met, stop reading and return the string.
             */
            StringBuilder stringBuilder = new StringBuilder();
            bool started = false;
            DateTime startTime = DateTime.Now;
            while (true)
            {
                /*
                 * If connection times out, throw exception.
                 */
                if ((DateTime.Now - startTime).TotalMilliseconds > Timeout)
                {
                    throw new TimeoutException("Timeout exceeded while waiting for message. Current receive buffer: " + stringBuilder.ToString());
                }
                if (serialPort.BytesToRead > 0)
                {
                    byte b = (byte)serialPort.ReadByte();
                    char c = (char)b;
                    if (!started && c == '@')
                    {
                        started = true;
                    }
                    else if (b == 13 && stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] == '*')
                    {
                        /*
                         * If the last byte read is CR and the second to last character read is *
                         * then the message has ended.
                         */
                        stringBuilder = stringBuilder.Remove(stringBuilder.Length - 1, 1);
                        break;
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }
                }
            }
            return stringBuilder.ToString();
        }

        private ushort[] ParseResponse(string response)
        {
            ushort[] values = new ushort[response.Length / 4];
            for (int i = 0; i < response.Length; i += 4)
            {
                string sVal = response.Substring(i, 4);
                values[i / 4] = (ushort)HexaToInt(sVal);
            }
            return values;
        }

        private string FormatUshortToString(ushort value)
        {
            StringBuilder sb = new StringBuilder();
            while (value > 0)
            {
                sb.Insert(0, value % 10);
                value /= 10;
            }
            for (int i = sb.Length; i < 4; i++)
            {
                sb.Insert(0, 0);
            }
            return sb.ToString();
        }

        private static string IntToHexa(int n)
        {
            // char array to store  
            // hexadecimal number 
            StringBuilder sb = new StringBuilder();

            // counter for hexadecimal number array 
            while (n != 0)
            {
                // temporary variable to  
                // store remainder 
                int temp = 0;

                // storing remainder in temp 
                // variable. 
                temp = n % 16;

                // check if temp < 10 
                if (temp < 10)
                {
                    sb.Insert(0, (char)(temp + 48));
                }
                else
                {
                    sb.Insert(0, (char)(temp + 55));
                }
                n = n / 16;
            }
            if (sb.Length == 1)
            {
                sb.Insert(0, 0);
            }
            return sb.ToString();
        }

        private static int HexaToInt(string hexVal)
        {
            int len = hexVal.Length;

            // Initializing base1 value  
            // to 1, i.e 16^0 
            int base1 = 1;

            int dec_val = 0;

            // Extracting characters as 
            // digits from last character 
            for (int i = len - 1; i >= 0; i--)
            {
                // if character lies in '0'-'9',  
                // converting it to integral 0-9  
                // by subtracting 48 from ASCII value 
                if (hexVal[i] >= '0' &&
                    hexVal[i] <= '9')
                {
                    dec_val += (hexVal[i] - 48) * base1;

                    // incrementing base1 by power 
                    base1 = base1 * 16;
                }

                // if character lies in 'A'-'F' ,  
                // converting it to integral  
                // 10 - 15 by subtracting 55  
                // from ASCII value 
                else if (hexVal[i] >= 'A' &&
                         hexVal[i] <= 'F')
                {
                    dec_val += (hexVal[i] - 55) * base1;

                    // incrementing base1 by power 
                    base1 = base1 * 16;
                }
            }
            return dec_val;
        }

        private static void ThrowError(CmodeMessage cmodeMessage, EndCodes endCode)
        {
            switch (endCode)
            {
                case EndCodes.Length:
                    throw new LengthException("The maximum frame length of 131 bytes was exceeded");
                case EndCodes.Format:
                    throw new System.FormatException("The command format was wrong, or a command that cannot be divided has been divided, or the frame length is smaller than the minimum length for the applicable command");
                case EndCodes.FCS:
                    throw new FCSException("FCS calculated value was incorrect");
                case EndCodes.EntryNum:
                    throw new EntryNumberException("The data is outside of the specified range or too long");
                case EndCodes.CPUUnit:
                    throw new CPUUnitException("The command can not be executed because a CPU error has occured on the CPU unit");
                default:
                    throw new UndefinedException("An exception occured on the CPU. End code: " + cmodeMessage.EndCode);
            }
        }
        #endregion
    }
}
