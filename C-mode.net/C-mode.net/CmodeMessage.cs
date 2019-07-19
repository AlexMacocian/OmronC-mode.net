using System;
using System.Collections.Generic;
using System.Text;

namespace C_mode.net
{
    struct CmodeMessage
    {
        #region Fields
        private static char beginChar = '@';
        private static string terminatorChars = "*\r";
        private StringBuilder buffer;
        private byte fcs;
        #endregion
        #region Properties
        /// <summary>
        /// Unit number. Length is 2 BCD characters.
        /// </summary>
        public string UnitNo
        {
            get => buffer.ToString().Substring(0, 2);
            set
            {
                buffer[0] = value[0];
                buffer[1] = value[1];
            }
        }
        /// <summary>
        /// Header code. Length is 2 characters.
        /// </summary>
        public string HeaderCode
        {
            get => buffer.ToString().Substring(2, 2);
            set
            {
                buffer[2] = value[0];
                buffer[3] = value[1];
            }
        }
        /// <summary>
        /// End code. Length is 2 hexadecimal characters.
        /// </summary>
        public string EndCode
        {
            get => buffer.ToString().Substring(4, 2);
            set
            {
                buffer[4] = value[0];
                buffer[5] = value[1];
            }
        }
        /// <summary>
        /// Address. Length is 4 BCD characters.
        /// </summary>
        public string Address
        {
            get => buffer.ToString().Substring(4, 4);
            set
            {
                for (int i = 4; i < 8; i++)
                {
                    buffer[i] = value[i - 4];
                }
            }
        }
        /// <summary>
        /// Text of a command message.
        /// </summary>
        public string CommandText
        {
            get => buffer.ToString().Substring(8, buffer.Length - 8);
            set
            {
                buffer.Remove(8, buffer.Length - 8);
                buffer.Insert(8, value);
            }
        }
        /// <summary>
        /// Text of a response message. Used only for a response read.
        /// Values are in sets of 4 hexadecimal characters.
        /// </summary>
        public string ResponseText
        {
            get => buffer.ToString().Substring(6, buffer.Length - 6);
            set
            {
                buffer.Remove(6, buffer.Length - 6);
                buffer.Insert(6, value);
            }
        }
        #endregion
        #region Constructors
        public CmodeMessage(string message)
        {
            buffer = new StringBuilder(message);
            string fcs = buffer.ToString(buffer.Length - 2, 2);
            buffer = buffer.Remove(buffer.Length - 2, 2);
            this.fcs = (byte)HexaToInt(fcs);
        }

        public CmodeMessage(string unitNo, string headerCode, string endCode = "", string address = "", string text = "")
        {
            buffer = new StringBuilder();
            buffer.Append(unitNo);
            buffer.Append(headerCode);
            buffer.Append(address);
            buffer.Append(endCode);
            buffer.Append(text);
            fcs = CalculateFCS(buffer);
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Calculate the FCS and return the built message.
        /// </summary>
        /// <returns>Message in string format.</returns>
        public string GetMessage()
        {
            fcs = CalculateFCS(buffer);
            StringBuilder sb = new StringBuilder(buffer.ToString());
            sb.Insert(0, beginChar);
            sb.Append(IntToHexa(fcs));
            sb.Append('*');
            string ret = sb.ToString();
            ret += '\r';
            return ret;
        }
        /// <summary>
        /// Checks if the current fcs matches the content of the message.
        /// </summary>
        /// <returns>True if the FCS is correct.</returns>
        public bool CheckFCS()
        {
            byte currentFcs = CalculateFCS(buffer);
            if (this.fcs == currentFcs)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Calculate and set FCS according to current contents.
        /// </summary>
        public void RecalculateFCS()
        {
            this.fcs = CalculateFCS(buffer);
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// Calculate FCS value for given buffer.
        /// </summary>
        /// <param name="buffer">String containing the FCS value</param>
        /// <returns>FCS value in byte format</returns>
        public static byte CalculateFCS(StringBuilder buffer)
        {
            byte[] byteArray = ASCIIEncoding.ASCII.GetBytes(buffer.ToString());
            byte fcs = 0 ^ 64;
            for (int i = 0; i < byteArray.Length; i++)
            {
                fcs ^= byteArray[i];
            }
            return fcs;
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
        #endregion
    }
}
