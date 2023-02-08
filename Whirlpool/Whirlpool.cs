using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Whirlpool
{
    public class Whirlpool
    {
        private FileStream infile, outfile;
        private string filename;
        public Whirlpool(string infile, string outfile)
        {
            this.filename = infile;
            this.infile = new FileStream(infile, FileMode.Open, FileAccess.Read);
            this.outfile = new FileStream(outfile, FileMode.Create);
        }

        private void MessageComletion()
        {
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
            infile.Close();

            long arrBitSize = zeroCount + 1 + 256;
            long arrByteSize = arrBitSize / 8;

            byte[] arr = new byte[arrByteSize]; //дополнение сообщения
            //int lastByteLenth = (zeroCount + 1) % 8; //количество нулей в последнем байте
            //int indexLastByte;

            //формируем первый байт
            arr[0] |= (byte)(1 << 7); //первая 1
            for (int i = 1; i < 8; i++)
                arr[0] &= (byte)~(1 << 7 - i);
            //дописываем нули
            for (long i = 1; i < arrByteSize - 24; i++)
                arr[i] = 0;
            //дописываем длину сообщения 
            byte[] mybyt = BitConverter.GetBytes(fileSize);
            for (int i = 0; i < 8; i++)
                arr[arrByteSize - 8 + i] = mybyt[i];

            infile = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None);
            using (var bw = new BinaryWriter(infile)) 
                bw.Write(arr);

            infile.Close();
        }

        private void bufInitialize()
        {
            byte[] H = new byte[512];
            for (int i = 0; i < 512; i++)
                H[i] = 0;
        }

        private void mainFunction()
        {

        }
    }
}