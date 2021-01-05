# Gdl90
 Création d'un message traffic report (GDL90)


utilisation
    
    
           Avion_GDL90 calcul_test = new Avion_GDL90();
           byte[] montableau = calcul_test.createTableau();

           
           byte[] bgdl90 = new byte[b.Length+4]; //Ajout de 4 octets pour avoir le 7E, CRC..

            ushort res_crc =   calcul_crc.Crc16C_citt(b);
            Buffer.BlockCopy(b, 0, bgdl90, 1, taille);

            bgdl90[0] = 0x7E;
            byte[] crc = BitConverter.GetBytes(res_crc);


            bgdl90[bgdl90.Length - 3] = crc[0];
            bgdl90[bgdl90.Length - 2] = crc[1];

            bgdl90[bgdl90.Length - 1] = 0x7E;


penser à rajouter l' echappement 0x7D si nécessaire
