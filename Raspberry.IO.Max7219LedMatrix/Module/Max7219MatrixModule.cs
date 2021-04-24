using System;
using System.Linq;
using Raspberry.IO.Max7219LedMatrix.CharactersLibrary;

namespace Raspberry.IO.Max7219LedMatrix.Module
{
    public class Max7219MatrixModule : IMax7219MatrixModule
    {
        public const int NumberOfRows = 8;
        private readonly Action<IMax7219MatrixModule> _preprocessData;

        protected byte[] Data = new byte[NumberOfRows];
        protected IMatrixCharactersLibrary MatrixCharactersLibrary=new MatrixCharactersLibrary();


        /// <summary>
        /// Module number represents in which order are modules connected.
        /// Lowest module number marks first connected module and the highest marks the last.
        /// Display cannot contain duplicate module numbers.
        /// </summary>
        public int ModuleNumber { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleNumber">
        /// Module number represents in which order are modules connected.
        /// Lowest module number marks first connected module and the highest marks the last.
        /// Display cannot contain duplicate module numbers.
        /// </param>
        public Max7219MatrixModule(int moduleNumber)
        {
            ModuleNumber = moduleNumber;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moduleNumber">
        /// Module number represents in which order are modules connected.
        /// Lowest module number marks first connected module and the highest marks the last.
        /// Display cannot contain duplicate module numbers.
        /// </param>
        /// <param name="preprocessData">
        /// Action which will be applied to module before updating display.
        /// </param>
        public Max7219MatrixModule(int moduleNumber, Action<IMax7219MatrixModule> preprocessData)
        {
            _preprocessData = preprocessData;
            ModuleNumber = moduleNumber;
        }

        public virtual Max7219MatrixModule ApplyPreprocessing()
        {
            _preprocessData?.Invoke(this);
            return this;
        }
        public void SetCharacterLibrary(IMatrixCharactersLibrary charactersLibrary)
        {
            MatrixCharactersLibrary = charactersLibrary;
        }

        public virtual IMax7219MatrixModule ShiftModuleRight(uint columnCount)
        {
            Data = Data.Select(b => (byte) (b >> (int) columnCount)).ToArray();
            return this;
        }

        public virtual IMax7219MatrixModule ShiftModuleLeft(uint columnCount)
        {
            Data = Data.Select(b => (byte) (b << (int) columnCount)).ToArray();
            return this;
        }

        public virtual IMax7219MatrixModule ShiftModuleUp(uint rowCount)
        {
            var newScreen = Data.Skip((int) rowCount).ToList();

            while (newScreen.Count < Data.Length) newScreen.Add(0x00);

            Data = newScreen.ToArray();

            return this;
        }

        public virtual IMax7219MatrixModule ShiftModuleDown(uint rowCount)
        {
            var newScreen = Data.ToList();
            for (var i = 0; i < rowCount; i++) newScreen.Insert(0, 0x0);
            Data = newScreen.Take(Data.Length).ToArray();
            return this;
        }

        public virtual IMax7219MatrixModule FlipModuleVertical()
        {
            for (var i = 0; i < Data.Length; i++)
            {
                var b = Data[i];
                Data[i] = ReverseBits(b);
            }

            return this;
        }

        public virtual IMax7219MatrixModule FlipModuleHorizontal()
        {
            Data = Data.Reverse().ToArray();
            return this;
        }

        public virtual IMax7219MatrixModule InvertModule()
        {
            for (var i = 0; i < Data.Length; i++)
            {
                var b = Data[i];
                Data[i] = NegateBits(b);
            }

            return this;
        }

        public virtual IMax7219MatrixModule SetRow(uint rowId, byte value)
        {
            CheckRowId(rowId);
            Data[rowId] = value;
            return this;
        }

        public virtual byte GetRow(uint rowId)
        {
            CheckRowId(rowId);
            return Data[rowId];
        }


        public virtual IMax7219MatrixModule Fill()
        {
            for (var r = 0; r < NumberOfRows; r++) Data[r] = 0xff;
            return this;
        }


        public virtual void Set(byte[] data)
        {
            if (Data.Length != data.Length)
            {
                throw new ArgumentException($"Provided invalid size of array! Expected {data.Length}", nameof(data));
            }

            data.CopyTo(Data, 0);
        }

        public virtual IMax7219MatrixModule SetNumber(int number)
        {
            if (number < 0 || number > 9)
                throw new ArgumentException("Number is out of valid range of 0-9!", nameof(number));

            SetCharacter(number.ToString()[0]);
            return this;
        }

        public virtual IMax7219MatrixModule SetCharacter(char character)
        {
            Set(MatrixCharactersLibrary.GetCharacter(character));
            return this;
        }

        public virtual byte[] Get()
        {
            var data = new byte[NumberOfRows];
            Data.CopyTo(data, 0);
            return data;
        }


        public virtual IMax7219MatrixModule ClearModule()
        {
            for (var r = 0; r < NumberOfRows; r++) Data[r] = 0x0;
            return this;
        }

        public virtual uint GetRowCount()
        {
            return (uint) Data.Length;
        }

        public virtual uint GetColumnCount()
        {
            return GetRowCount();
        }

        public void SetColumn(uint column, byte data)
        {
            CheckColumn(column);

            var moduleData = Get();
            moduleData = RotateRight(moduleData);
            moduleData[column] = data;

            for (int i = 0; i < 3; i++)
            {
                moduleData = RotateRight(moduleData);
            }

            Data = moduleData;
        }

        public byte GetColumn(uint column)
        {
            CheckColumn(column);
            return RotateRight(Data)[column];
        }

        protected static byte ReverseBits(byte b)
        {
            return (byte) ((((b * 0x80200802ul) & 0x0884422110ul) * 0x0101010101ul) >> 32);
        }

        protected static byte NegateBits(byte b)
        {
            return unchecked((byte) ~b);
        }

        private void CheckRowId(uint rowId)
        {
            if (rowId > NumberOfRows)
            {
                throw new ArgumentException($"{nameof(rowId)} must be in range 0 - {NumberOfRows - 1}", nameof(rowId));
            }
        }

        protected static byte[] RotateRight(byte[] data)
        {
            var outData = new byte[NumberOfRows];

            byte val;


            //rotate 90* clockwise
            for (byte i = 0; i < 8; i++)
            {
                for (byte j = 0; j < 8; j++)
                {
                    val = (byte) ((data[i] >> j) & 1); //extract the j-th bit of the i-th element
                    outData[NumberOfRows - 1 - j] |= (byte) (val << i); //set the newJ-th bit of the newI-th element
                }
            }

            return outData;
        }

        private static void CheckColumn(uint column)
        {
            if (column >= NumberOfRows)
            {
                throw new ArgumentException($"Column must be in range 0-{NumberOfRows - 1}.");
            }
        }
    }
}