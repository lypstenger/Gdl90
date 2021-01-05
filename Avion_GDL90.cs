using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Gdl90
{
    public class Avion_GDL90
    {
        public Avion_GDL90()
        {
            //________________________________________________________
            Latitude = 44.54;
            Longitude = -1.122;
            Altitude = 1500;
            ICAO = new byte[] { 0x3B, 0x45, 0x49 };
            VitAsc = 64d;
            TrueTrack = 45;
            //________________________________________________________
            
            TailNumber = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        }

  

        public double VitAsc { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double VitesseX { get; set; }
        public double VitesseY { get; set; }
        public double Altitude { get; set; }
        public byte[] ICAO { get; set; } = new byte[3];
        public byte TrueTrack { get; set; }
        public double Cap { get; set; }
        public byte[] TailNumber { get; set; } = new byte[8];

        public double Distance { get; set; } 
            

        //CREATION DU MESSAGE EN GDL90
        public byte[] createTableau()
        {
            byte[] TrafficReport = new byte[28]; //TrafficReport correspond au tableau d'octet que l'on envoie dans l'avion au format GDL90
            double Vitesse = Math.Sqrt(Math.Pow(VitesseX, 2) + Math.Pow(VitesseY, 2));
            

            CalculTrack(); //Procédure permettant de calculer l'angle


            TrafficReport[0] = 20; //Message ID 0x14 (dans le tableau GDL90 page 25)
            TrafficReport[1] = (0x00 | 0x02); //(s | t) dans la table 12 - traffic report page 25

            Buffer.BlockCopy(ICAO, 0, TrafficReport, 2, 3); //ICAO

            int result1 = (int)(Latitude / (180 / Math.Pow(2, 23))); //Retourne la valeur de la latitude en binaire 24 bits (format GDL90)
            int result2 = (int)(Longitude / (180 / Math.Pow(2, 23))); //idem mais cette fois-ci longitude
            ushort result3 = (ushort)((Altitude + 1000) / 25);

            byte[] tampon_conversion = BitConverter.GetBytes(result1);
            Array.Reverse(tampon_conversion);
            Buffer.BlockCopy(tampon_conversion, 1, TrafficReport, 5, 3); //"Reconstruction" du tableau - traffic report (GDL90 page 25) -- LATITUDE

            tampon_conversion = BitConverter.GetBytes(result2);
            Array.Reverse(tampon_conversion);
            Buffer.BlockCopy(tampon_conversion, 1, TrafficReport, 8, 3); //"Reconstruction" du tableau - traffic report (GDL90 page 25) -- LONGITUDE

            result3 = (ushort)(result3 * 16); //Mise au format pour décalage de 4 bit (et Alicia contente)
            tampon_conversion = BitConverter.GetBytes(result3);
            Array.Reverse(tampon_conversion);
            TrafficReport[11] = tampon_conversion[0]; // "Reconstruction" du tableau -traffic report(GDL90 page 25)-- ALTITUDE

            TrafficReport[12] = (byte)(tampon_conversion[1] | TrafficReport[12]); // "MISCELLANEOUS INDICATORS" (m dans la table 8 page 18)
            TrafficReport[12] = (byte)(tampon_conversion[1] | 0x09); //"MISCELLANEOUS INDICATORS" (m dans la table 8 page 18) //9 car correspond a True Track et Airbnorne (cf. table 9 page 20 doc GDL90)

            TrafficReport[13] = 0xCD; //Correspond "Navigation Integrity & Accuracy" (ia)

            tampon_conversion = BitConverter.GetBytes((short)(Vitesse * 16));
            Array.Reverse(tampon_conversion);
            Buffer.BlockCopy(tampon_conversion, 0, TrafficReport, 14, 2); //"Reconstruction" du tableau - traffic report (GDL90 page 25) -- HORIZONTAL VELOCITY (hhh)

            short result4 = (short)(VitAsc / 64);
            tampon_conversion = BitConverter.GetBytes(result4);
            Array.Reverse(tampon_conversion);

            TrafficReport[15] = (byte)(TrafficReport[15] | tampon_conversion[0]);//"Reconstruction" du tableau - traffic report (GDL90 page 25) -- VERTICAL VELOCITY (v)

            TrafficReport[16] = tampon_conversion[1];//"Reconstruction" du tableau - traffic report (GDL90 page 25) -- VERTICAL VELOCITY (vvv)

            byte result5 = (byte)(Cap / (360d / 256d));
            TrafficReport[17] = result5;

            TrafficReport[18] = 0x01; //Correspond "Emitter Category" (ee dans le tableau 12 page 25) = correspond au type de mobile (parachutiste, paraglider, ballon dirigeable....)

            Buffer.BlockCopy(TailNumber, 0, TrafficReport, 19, 8); //Call sign = IVOL et si rien alors on met que des espaces (0x20)


            TrafficReport[27] = (0x00 | 0x00); //(p | x) dans la table 12 - traffic report page 25
            return TrafficReport;
        }

        private void CalculTrack()
        {

            double angle = Math.Atan(VitesseY / VitesseX);

            Cap = angle * 180 / Math.PI;
            //Calcul du Cap "dans le bon sens" cf. schéma
            if (VitesseX >= 0 && VitesseY >= 0)
            {
                Cap = 90 - Cap;
            }

            if (VitesseX < 0 && VitesseY < 0)
            {
                Cap = 270 - Cap;
            }

            if (VitesseX >= 0 && VitesseY < 0)
            {
                Cap = 90 - Cap;
            }

            if (VitesseX < 0 && VitesseY >= 0)
            {
                Cap = 270 - Cap;
            }
            

        }

        public static UInt16[] cp_Crc16_CCITT =
        {
            0x0000,0x1021, 0x2042,0x3063,0x4084,0x50A5,0x60C6,0x70E7,
            0x8108,0x9129,0xA14A,0xB16B,0xC18C,0xD1AD,0xE1CE,0xF1EF,
            0x1231,0x0210,0x3273,0x2252,0x52B5,0x4294,0x72F7,0x62D6,
            0x9339,0x8318,0xB37B,0xA35A,0xD3BD,0xC39C,0xF3FF,0xE3DE,
            0x2462,0x3443,0x0420,0x1401,0x64E6,0x74C7,0x44A4,0x5485,
            0xA56A,0xB54B,0x8528,0x9509,0xE5EE,0xF5CF,0xC5AC,0xD58D,
            0x3653,0x2672,0x1611,0x0630,0x76D7,0x66F6,0x5695,0x46B4,
            0xB75B,0xA77A,0x9719,0x8738,0xF7DF,0xE7FE,0xD79D,0xC7BC,
            0x48C4,0x58E5,0x6886,0x78A7,0x0840,0x1861,0x2802,0x3823,
            0xC9CC,0xD9ED,0xE98E,0xF9AF,0x8948,0x9969,0xA90A,0xB92B,
            0x5AF5,0x4AD4,0x7AB7,0x6A96,0x1A71,0x0A50,0x3A33,0x2A12,
            0xDBFD,0xCBDC,0xFBBF,0xEB9E,0x9B79,0x8B58,0xBB3B,0xAB1A,
            0x6CA6,0x7C87,0x4CE4,0x5CC5,0x2C22,0x3C03,0x0C60,0x1C41,
            0xEDAE,0xFD8F,0xCDEC,0xDDCD,0xAD2A,0xBD0B,0x8D68,0x9D49,
            0x7E97,0x6EB6,0x5ED5,0x4EF4,0x3E13,0x2E32,0x1E51,0x0E70,
            0xFF9F,0xEFBE,0xDFDD,0xCFFC,0xBF1B,0xAF3A,0x9F59,0x8F78,
            0x9188,0x81A9,0xB1CA,0xA1EB,0xD10C,0xC12D,0xF14E,0xE16F,
            0x1080,0x00A1,0x30C2,0x20E3,0x5004,0x4025,0x7046,0x6067,
            0x83B9,0x9398,0xA3FB,0xB3DA,0xC33D,0xD31C,0xE37F,0xF35E,
            0x02B1,0x1290,0x22F3,0x32D2,0x4235,0x5214,0x6277,0x7256,
            0xB5EA,0xA5CB,0x95A8,0x8589,0xF56E,0xE54F,0xD52C,0xC50D,
            0x34E2,0x24C3,0x14A0,0x0481,0x7466,0x6447,0x5424,0x4405,
            0xA7DB,0xB7FA,0x8799,0x97B8,0xE75F,0xF77E,0xC71D,0xD73C,
            0x26D3,0x36F2,0x0691,0x16B0,0x6657,0x7676,0x4615,0x5634,
            0xD94C,0xC96D,0xF90E,0xE92F,0x99C8,0x89E9,0xB98A,0xA9AB,
            0x5844,0x4865,0x7806,0x6827,0x18C0,0x08E1,0x3882,0x28A3,
            0xCB7D,0xDB5C,0xEB3F,0xFB1E,0x8BF9,0x9BD8,0xABBB,0xBB9A,
            0x4A75,0x5A54,0x6A37,0x7A16,0x0AF1,0x1AD0,0x2AB3,0x3A92,
            0xFD2E,0xED0F,0xDD6C,0xCD4D,0xBDAA,0xAD8B,0x9DE8,0x8DC9,
            0x7C26,0x6C07,0x5C64,0x4C45,0x3CA2,0x2C83,0x1CE0,0x0CC1,
            0xEF1F,0xFF3E,0xCF5D,0xDF7C,0xAF9B,0xBFBA,0x8FD9,0x9FF8,
            0x6E17,0x7E36,0x4E55,0x5E74,0x2E93,0x3EB2,0x0ED1,0x1EF0,
         };

        ushort initialValue = 0;
        public ushort ComputeChecksumme(byte[] bytes)
        {
            ushort crc = initialValue;
            for (int i = 0; i < bytes.Length; i++)
            {
                crc = (ushort)((crc << 8) ^ cp_Crc16_CCITT[((crc >> 8) ^ (0xff & bytes[i]))]);
            }
            return crc;
        }

        public ushort Crc16C_citt(byte[] bytes)
        {
            const ushort poly = 4129;
            ushort[] tables = new ushort[256];
            ushort initialValue = 0;
            ushort temp, a;
            ushort crc = initialValue;
            for (int i = 0; i < tables.Length; ++i)
            {
                temp = 0;
                a = (ushort)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0)
                        temp = (ushort)((temp << 1) ^ poly);
                    else
                        temp <<= 1;
                    a <<= 1;
                }
                tables[i] = temp;
            }
            int LGN = bytes.Length;
            for (int i = 0; i < LGN; i++)
            {
                crc = (ushort)(tables[(crc >> 8)] ^ (crc << 8) ^ bytes[i]);
                byte[] bcrc = BitConverter.GetBytes(crc);
                if (bcrc[0] == 0xAE || bcrc[1] == 0xAE)
                {
                    int deb = 0;
                }
            }
            return crc;
        }
        

    }
}
