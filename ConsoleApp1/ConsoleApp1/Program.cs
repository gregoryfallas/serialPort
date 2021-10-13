using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace ConsoleApp1
{
    class Program
    {

        // variable of DNI info
        private static string strCedula;
        private static string strPrimerApellido;
        private static string strSegundoApellido;
        private static string strNombre;
        private static string strGenero;
        private static string strFechaNacimiento;
        private static string strFechaExpiracion;


        // Load TSE library
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate String Decoder(byte[] bytCode);



        static bool _continue;
        static SerialPort _serialPort;


        // Byte Arry for decoding DNI
        private static byte[] keysArray = new byte[]{
            (byte)0x27,
            (byte)0x30,
            (byte)0x04,
            (byte)0xA0,
            (byte)0x00,
            (byte)0x0F,
            (byte)0x93,
            (byte)0x12,
            (byte)0xA0,
            (byte)0xD1,
            (byte)0x33,
            (byte)0xE0,
            (byte)0x03,
            (byte)0xD0,
            (byte)0x00,
            (byte)0xDf,
            (byte)0x00
    };

        // Encoding ISO 8859 1
        public static System.Text.Encoding iso_8859_1 = System.Text.Encoding.GetEncoding("iso-8859-1");

        static void Main(string[] args)
        {
            string name;
            string message;
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);

            
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.Encoding = iso_8859_1;

            
            _serialPort.Open();
            _continue = true;
            readThread.Start();

            Console.Write("Name: ");
            name = Console.ReadLine();

            Console.WriteLine("Type QUIT to exit");

            while (_continue)
            {
                message = Console.ReadLine();

                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                else
                {
                    _serialPort.WriteLine(
                        String.Format("<{0}>: {1}", name, message));
                }
            }

            readThread.Join();
            _serialPort.Close();
        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {

                    // File to save byte array
                    string fileName = "lector.dat";

                    // Read info from Serial Port and save into string
                    string message = _serialPort.ReadLine();

                    // string to byte array with ISO 8859 1 Encoding
                    byte[] bytes = iso_8859_1.GetBytes(message);


                    // Save byte arry in file
                    if (!File.Exists("lector.dat"))
                    {
                        using (FileStream
                        fileStream = new FileStream(fileName, FileMode.Create))

                            for (int i = 0; i < bytes.Length; i++)
                            {
                                fileStream.WriteByte(bytes[i]);
                            }
                    }

                    Console.WriteLine(decriptRaw(bytes));
                }
                catch (TimeoutException) { }
            }
        }

        // Display Port values and prompt user to enter a port.
        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        // Display PortParity values and prompt user to enter a value.
        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        // Display DataBits values and prompt user to enter a value.
        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits.ToUpperInvariant());
        }

        // Display StopBits values and prompt user to enter a value.
        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available StopBits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter StopBits value (None is not supported and \n" +
             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }

        // Decoder DNI 
        private static String decriptRaw(byte[] raw)
        {
            StringBuilder resultValue = new StringBuilder(raw.Length);
            int j = 0;

            for (int i = 0; i < raw.Length; i++)
            {
                string pattern = @"^[a-zA-Z0-9]*$";
                if (j == 17)
                {
                    j = 0;
                }

                char result = (char)(keysArray[j] ^ ((char)(raw[i])));

                Match m = Regex.Match(result.ToString(), pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    resultValue.Append(result);
                }
                else
                {
                    resultValue.Append(' ');
                }

                j++;
            }

            //string finalResult = "Cedula: " + resultValue.ToString().Substring(0, 9).Trim() + "\n" +
            //    "Apellido1: " + resultValue.ToString().Substring(9, 35).Trim() + "\n" +
            //    "Apellido2: " + resultValue.ToString().Substring(35, 61).Trim() + "\n" +
            //    "Nombre1: " + resultValue.ToString().Substring(61, 91).Trim() + "\n" +
            //    "Genero: " + resultValue.ToString()[8] + "\n" +
            //    "Fecha de nacimiento: " + resultValue.ToString().Substring(92, 96).Trim() + " - " + resultValue.ToString().Substring(96, 98).Trim() + " - " + resultValue.ToString().Substring(98, 100).Trim() + "\n" +
            //    "Fecha de expiracion: " + resultValue.ToString().Substring(100, 104).Trim() + " - " + resultValue.ToString().Substring(104, 106).Trim() + " - " + resultValue.ToString().Substring(106, 108).Trim() + "\n";
            return resultValue.ToString();

        }


        // Use TSE library for decoder DNI (Fail, Don't Works)
        private static bool strDecodificacion(byte[] raw)
        {
            IntPtr pDll = LoadLibrary("C:/Program Files (x86)/TSE/Lectura de Cédula de Identidad/TSE.dll");
            //oh dear, error handling here
            if (pDll == IntPtr.Zero)
            {
                Console.WriteLine($"Could not load library: {Marshal.GetLastWin32Error()}");
            }

            IntPtr pAddressOfFunctionToCall = GetProcAddress(pDll, "strDecode");
            //oh dear, error handling here
            if (pAddressOfFunctionToCall == IntPtr.Zero)
            {
                Console.WriteLine($"Could not load method: {Marshal.GetLastWin32Error()}");
                FreeLibrary(pDll);  // unload library
            }

            Decoder decoder = (Decoder)Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, typeof(Decoder));



            String strDecodificacion = decoder(raw);
            if ((strDecodificacion != null) && (!strDecodificacion.Equals("")))
            {
                String[] strPartes = strDecodificacion.Split(',');
                strCedula = strPartes[0];
                strPrimerApellido = strPartes[1];
                strSegundoApellido = strPartes[2];
                strNombre = strPartes[3];
                strGenero = (strPartes[4].Equals("M")) ? "Masculino" : "Femenino";
                strFechaNacimiento = strPartes[5].Substring(6) + "/" + strPartes[5].Substring(4, 6) + "/" + strPartes[5].Substring(0, 4);
                strFechaExpiracion = strPartes[6].Substring(6) + "/" + strPartes[6].Substring(4, 6) + "/" + strPartes[6].Substring(0, 4);

            }
            else
            {
                return false;
            }


            return true;
        }
    }
}