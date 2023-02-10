using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Whirlpool
{
    public class Whirlpool
    {
        private FileStream infile, outfile;
        private byte[,] H;
        private byte[] message;
        private byte[] S = new byte[256]
        {
            0x18,  0x23,  0xc6,  0xe8,  0x87,  0xb8,  0x01,  0x4f,  0x36,  0xa6,  0xd2,  0xf5,  0x79,  0x6f,  0x91,  0x52,
            0x60,  0xbc,  0x9b,  0x8e,  0xa3,  0x0c,  0x7b,  0x35,  0x1d,  0xe0,  0xd7,  0xc2,  0x2e,  0x4b,  0xfe,  0x57,
            0x15,  0x77,  0x37,  0xe5,  0x9f,  0xf0,  0x4a,  0xda,  0x58,  0xc9,  0x29,  0x0a,  0xb1,  0xa0,  0x6b,  0x85,
            0xbd,  0x5d,  0x10,  0xf4,  0xcb,  0x3e,  0x05,  0x67,  0xe4,  0x27,  0x41,  0x8b,  0xa7,  0x7d,  0x95,  0xd8,
            0xFb,  0xee,  0x7c,  0x66,  0xdd,  0x17,  0x47,  0x9e,  0xca,  0x2d,  0xbf,  0x07,  0xad,  0x5a,  0x83,  0x33,
            0x63,  0x02,  0xaa,  0x71,  0xc8,  0x19,  0x49,  0xd9,  0xf2,  0xe3,  0x5b,  0x88,  0x9a,  0x26,  0x32,  0xb0,
            0xe9,  0x0f,  0xd5,  0x80,  0xbe,  0xcd,  0x34,  0x48,  0xff,  0x7a,  0x90,  0x5f,  0x20,  0x68,  0x1a,  0xae,
            0xb4,  0x54,  0x93,  0x22,  0x64,  0xf1,  0x73,  0x12,  0x40,  0x08,  0xc3,  0xec,  0xdb,  0xa1,  0x8d,  0x3d,
            0x97,  0x00,  0xcf,  0x2b,  0x76,  0x82,  0xd6,  0x1b,  0xb5,  0xaf,  0x6a,  0x50,  0x45,  0xf3,  0x30,  0xef,
            0x3f,  0x55,  0xa2,  0xea,  0x65,  0xba,  0x2f,  0xc0,  0xde,  0x1c,  0xfd,  0x4d,  0x92,  0x75,  0x06,  0x8a,
            0xb2,  0xe6,  0x0e,  0x1f,  0x62,  0xd4,  0xa8,  0x96,  0xf9,  0xc5,  0x25,  0x59,  0x84,  0x72,  0x39,  0x4c,
            0x5e,  0x78,  0x38,  0x8c,  0xd1,  0xa5,  0xe2,  0x61,  0xb3,  0x21,  0x9c,  0x1e,  0x43,  0xc7,  0xfc,  0x04,
            0x51,  0x99,  0x6d,  0x0d,  0xfa,  0xdf,  0x7e,  0x24,  0x3b,  0xab,  0xce,  0x11,  0x8f,  0x4e,  0xb7,  0xeb,
            0x3c,  0x81,  0x94,  0xf7,  0xb9,  0x13,  0x2c,  0xd3,  0xe7,  0x6e,  0xc4,  0x03,  0x56,  0x44,  0x7f,  0xa9,
            0x2a,  0xbb,  0xc1,  0x53,  0xdc,  0x0b,  0x9d,  0x6c,  0x31,  0x74,  0xf6,  0x46,  0xac,  0x89,  0x14,  0xe1,
            0x16,  0x3a,  0x69,  0x09,  0x70,  0xb6,  0xd0,  0xed,  0xcc,  0x42,  0x98,  0xa4,  0x28,  0x5c,  0xf8,  0x86
        };

        public Whirlpool(string infile, string outfile)
        {
            this.infile = new FileStream(infile, FileMode.Open, FileAccess.Read);
            this.outfile = new FileStream(outfile, FileMode.Create);
        }

        //формирует дополненное сообщение
        private void MessageComletion()
        {
            // к исходному сообщению длиной b бит добавляется один бит «1», а затем добавляются нулевые биты так, чтобы длина полученного
            // сообщения в битах по модулю 512 равнялась 256. Всего добавляется по крайней мере один бит и не более 512 бит. 
            // Таким образом, в результате расширения, сообщению недостаёт 256 бит до длины, кратной 512 битам
            //
            // ---исходное сообщение---дополнение (1000...00)---длина исходного сообщения---
            //          b бит             от 1 до 512 бит                256 бит

            ulong fileSize = (ulong)infile.Length * 8; //размер в битах
            fileSize++; //резервируем место под 1 бит
            int zeroCount; //считаем сколько допишем нулей
            switch (fileSize % 512)
            {
                case 256:
                    zeroCount = 0; break;
                case < 256:
                    zeroCount = 256 - (int)(fileSize % 512); break;
                case > 256:
                    zeroCount = 512 - (int)(fileSize % 512) + 256; break;
            }

            long addByteSize = (zeroCount + 1 + 256) / 8; //Размер дополнения в байтах
            message = new byte[infile.Length + addByteSize]; //массив сообщения
            byte[] add = new byte[addByteSize]; //дополнение сообщения

            //формируем первый байт
            add[0] |= (byte)(1 << 7); //первая 1
            for (int i = 1; i < 8; i++)
                add[0] &= (byte)~(1 << 7 - i);
            //дописываем нули
            for (long i = 1; i < addByteSize - 24; i++)
                add[i] = 0;
            //дописываем длину сообщения 
            byte[] mybyt = BitConverter.GetBytes(fileSize);
            for (int i = 0; i < 8; i++)
                add[addByteSize - 8 + i] = mybyt[i];

            //читаем файл
            while (infile.Length != infile.Position)
                message[infile.Position] = (byte)infile.ReadByte();
            //добавляем 
            add.CopyTo(message, infile.Length);

            infile.Close();

            // File.Copy(infilename, "processing" + infilename);
            // processingfile = new FileStream("processing" + infilename, FileMode.Append, FileAccess.Write, FileShare.None);
            // using (var bw = new BinaryWriter(infile)) 
            //     bw.Write(arr);
            // processingfile.Close();
        }

        //Инициализация буфера
        private void BufInitialize()
        {
            //В процессе вычисления хеша используется 512-битный буфер H, который содержит промежуточное состояние хеша. 
            // Начальное состояние Н0 – 512-битная строка, заполненная «0».
            
            H = new byte[8,8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    H[i,j] = 0;
        }

        //обработка сообщения
        private void Description()
        {
            //Обработка сообщения блоками по 512 бит
            //Сообщение М итерационно обрабатывается функцией сжатия блоками mi по 512 бит
            
            byte[,] m = new byte[8,8]; //Блок сообщения
            long position = 0;

            while (message.Length != position)
            {
                //считываем очередные 512 бит для обработки
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++, position++)
                        m[i,j] = message[position];


                //Whirlpool хранит внутреннее состояние H - матрицу 8x8 байт.
                //В самом начале H=0. Когда приходит очередной 64-байтный блок m, Whirlpool его шифрует с помощью H и получает W(H, m),
                //после чего добавляет (операция xor) к своему состоянию полученное значение и исходный блок: Hi = ϻ(Hi-1, mi) = WHi-1(mi) ^ mi ^ Hi-1.
                //Шифрование W. Функция W(H, m) вычисляется по следующей схеме:
                //W = H ^ m — 0-й раунд
                // for round = 1..10 do — еще 10 раундов
                // H = C(round) ^ MixRow(ShiftColumn(SubBytes(H)))
                // W = H ^ MixRow(ShiftColumn(SubBytes(W)))
                //где C(round) это матрица 8x8 у которой все элементы нули, кроме первой строчки, которая берётся из S-Box: //Cm[0..7] = S-Box[8(m-1)..8m-1].


                


            }
        }

        private byte[,] WFunction(byte[,] m, byte[,] h)
        {
            byte[,] w = new byte[8, 8];

            //0 раунд
            w = AddRoundKey(m, h);


            //10 раундов шифрования
            for (int round = 1; round <= 10; round++)
            {
                //h = AddRoundKey((round, m), MixRow(ShiftColumn(SubBytes(H))));


            }









            return m;
        }

        private byte[,] C(int round, byte[,] m)
        {
            byte[,] c = new byte[8, 8];

            for (int i = 0; i < 8; i++)
                c[0, i] = S[8 * (round - 1) + i];

            for (int i = 1; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    c[i, j] = 0;

            return c;
        }

        private byte[,] SubBytes(byte[,] matr)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    byte a = (byte)((byte)0xf0 & matr[i, j]);
                    byte b = (byte)((byte)0x0f & matr[i, j]);
                    matr[i, j] = S[0x10 * a + b];
                }

            return matr;
        }
        private byte SubBytes(byte n)
        {
            return n;
        }

        private byte[,] ShiftColumn(byte[,] matr)
        {
            return matr;
        }
        private byte[,] MixRow(byte[,] matr)
        {
            return matr;
        }
        private byte[,] AddRoundKey(byte[,] matr, byte[,] key)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    matr[i, j] ^= key[i, j];
            return matr;
        }

    }
}