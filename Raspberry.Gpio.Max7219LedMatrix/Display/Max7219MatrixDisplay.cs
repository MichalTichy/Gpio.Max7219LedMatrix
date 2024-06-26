using System;
using System.Collections.Generic;
using System.Device.Spi;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Raspberry.Gpio.Max7219LedMatrix.CharactersLibrary;
using Raspberry.Gpio.Max7219LedMatrix.Module;

namespace Raspberry.Gpio.Max7219LedMatrix.Display
{
    public class Max7219MatrixDisplay : IMax7219MatrixDisplay
    {
        public IMax7219MatrixModule[][] Modules { get; protected set; }

        protected const byte ShutdownByte = 0x0C;
        protected const byte BrightnessByte = 0x0A;
        protected const byte TestModeByte = 0x0F;
        protected const byte DecodeModeByte = 0x09;
        protected const byte ScanLimitByte = 0x0B;


        private readonly SpiDevice device;
        protected IMax7219MatrixModule[] OrderedModules;

        public Max7219MatrixDisplay(SpiDevice device, int numberOfModules)
        {
            this.device = device;
            InitModules(numberOfModules);
            InitOrderedModules();
        }


        public Max7219MatrixDisplay(SpiDevice device, Max7219MatrixModule[][] modules)
        {
            this.device = device;
            Modules = modules;
            InitOrderedModules();
        }

        public void SetCharacterLibrary(IMatrixCharactersLibrary charactersLibrary)
        {
            ApplyToAllModules(module => module.SetCharacterLibrary(charactersLibrary));
        }


        public virtual IMax7219MatrixDisplay Init()
        {
            SendRaw(new byte[2] { DecodeModeByte, 0x00 }); // set decode mode
            SendRaw(new byte[2] { ScanLimitByte, 0x07 });
            TurnOn();
            TestMode(false);
            ClearDisplay();
            UpdateScreen();
            return this;

        }

        public virtual IMax7219MatrixDisplay UpdateScreen()
        {

            for (uint r = 0; r < Max7219MatrixModule.NumberOfRows; r++)
            {
                
                var data = new List<byte>();

                for (var m = OrderedModules.Length - 1; m >= 0; m--)
                {
                    var module = OrderedModules[m];
                    data.Add((byte)(r + 1));
                    data.Add(module.GetRow(r));
                }

                SendRaw(data.ToArray());
            }

            return this;
        }

        public virtual IMax7219MatrixDisplay SendRaw(byte[] data)
        {
            device.Write(data);
            return this;
        }

        public virtual IMax7219MatrixDisplay SetBrightness(byte brightness)
        {
            var brightnesses = new byte[OrderedModules.Length];
            for (int i = 0; i < OrderedModules.Length; i++)
            {
                brightnesses[i] = brightness;
            }

            return SetBrightness(brightnesses);
        }

        public virtual IMax7219MatrixDisplay SetBrightness(byte[] brightnesses)
        {
            if (brightnesses.Length > OrderedModules.Length)
            {
                throw new ArgumentException("Cannot set brightness to more modules than configured!",
                    nameof(brightnesses));
            }

            foreach (var brightness in brightnesses)
            {
                if (brightness > 0xf)
                    throw new ArgumentException("Maximum allowed brightness is 0xf", nameof(brightnesses));

                SendRaw(new[] {BrightnessByte, brightness});
            }

            return this;
        }

        public virtual IMax7219MatrixDisplay TurnOff()
        {

            ApplyToAllModules(module => SendRaw(new byte[2] { ShutdownByte, 0x00 }));

            return this;
        }

        public virtual IMax7219MatrixDisplay TurnOn()
        {

            ApplyToAllModules(module => SendRaw(new byte[2] { ShutdownByte, 0x01 }));
            return this;
        }

        public virtual IMax7219MatrixDisplay TestMode(bool enabled = true)
        {
            byte enabledByte = 0x01;
            byte disabledByte = 0x00;
            byte enable = enabled ? enabledByte : disabledByte;
            SendRaw(new byte[] { TestModeByte, enable });
            return this;
        }

        public virtual IMax7219MatrixDisplay ApplyToAllModules(Action<IMax7219MatrixModule> action)
        {
            foreach (var module in OrderedModules)
            {
                action(module);
            }

            return this;
        }

        public virtual IMax7219MatrixDisplay InvertScreen()
        {
            ApplyToAllModules(module => module.InvertModule());
            return this;
        }

        public virtual IMax7219MatrixDisplay ShiftScreenUp()
        {
            byte[] topOverFlow;

            foreach (var row in Modules.Reverse())
            {
                topOverFlow = new byte[row.Length];
                for (var i = 0; i < row.Length; i++)
                {
                    var module = row[i];
                    var overflow = topOverFlow[i];

                    topOverFlow[i] = module.GetRow(0);
                    module.ShiftModuleUp(1);
                    module.SetRow(module.GetRowCount() - 1, overflow);
                }
            }

            return this;
        }

        public virtual IMax7219MatrixDisplay ShiftScreenDown()
        {
            byte[] bottomOverFlow;

            foreach (var row in Modules)
            {
                bottomOverFlow = new byte[row.Length];
                for (var i = 0; i < row.Length; i++)
                {
                    var module = row[i];
                    var overflow = bottomOverFlow[i];
                    bottomOverFlow[i] = module.GetRow(module.GetRowCount() - 1);
                    module.ShiftModuleDown(1);
                    module.SetRow(0, overflow);
                }
            }

            return this;
        }

        public virtual IMax7219MatrixDisplay ShiftScreenRight()
        {
            byte rightOverflow = 0x0;

            foreach (var row in Modules)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    var module = row[i];
                    var overflow = rightOverflow;
                    rightOverflow = module.GetColumn(module.GetColumnCount() - 1);
                    module.ShiftModuleRight(1);
                    module.SetColumn(0, overflow);
                }
            }

            return this;
        }

        public virtual IMax7219MatrixDisplay ShiftScreenLeft()
        {
            byte rightOverflow = 0x0;

            foreach (var row in Modules)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    var module = row[i];
                    var overflow = rightOverflow;
                    rightOverflow = module.GetColumn(0);
                    module.ShiftModuleLeft(1);
                    module.SetColumn(module.GetColumnCount() - 1, overflow);
                }
            }

            return this;
        }

        public virtual IMax7219MatrixDisplay ClearDisplay()
        {
            ApplyToAllModules(module => module.ClearModule());
            return this;
        }

        public virtual IMax7219MatrixDisplay CopyModule(IMax7219MatrixModule source, IMax7219MatrixModule target)
        {
            var data = source.Get();
            target.Set(data);

            return this;
        }

        public virtual IMax7219MatrixDisplay FillDisplay()
        {
            ApplyToAllModules(module => module.Fill());
            return this;
        }



        public virtual IMax7219MatrixDisplay Identify()
        {
            ApplyToAllModules(module => SetNumber(module.ModuleNumber));
            return this;
        }

        public virtual IMax7219MatrixDisplay SetText(string text, int row = 0)
        {
            CheckTextLength(text, row);
            for (int i = 0; i < text.Length; i++)
            {
                Modules[row][i].SetCharacter(text[i]);
            }

            return this;
        }

        public IMax7219MatrixDisplay SetNumber(int number, int row = 0)
        {
            return SetText(number.ToString());
        }

        private void InitModules(int numberOfModules)
        {
            Modules = new IMax7219MatrixModule[1][];
            Modules[0] = new IMax7219MatrixModule[numberOfModules];
            for (int i = 0; i < numberOfModules; i++)
            {
                Modules[0][i] = new Max7219MatrixModule(i);
            }
        }

        private void InitOrderedModules()
        {
            if (Modules == null)
            {
                return;
            }

            var d = new Dictionary<int, IMax7219MatrixModule>();
            foreach (var module in Modules.SelectMany(t => t))
            {
                if (d.ContainsKey(module.ModuleNumber))
                {
                    throw new ArgumentException(
                        $"Found two modules with duplicate {nameof(module.ModuleNumber)} {module.ModuleNumber}");
                }

                d[module.ModuleNumber] = module;
            }

            OrderedModules = d.Values.OrderByDescending(t => t.ModuleNumber).ToArray();
        }

        private void CheckTextLength(string text, int row)
        {
            CheckRow(row);
            var rowArray = Modules[row];
            if (rowArray.Length < text.Length)
            {
                throw new ArgumentException(
                    $"Provided text is too long for given row. Maximum length is {rowArray.Length}.");
            }
        }

        private void CheckRow(int row)
        {
            if (row < 0 || row >= Modules.Length)
            {
                throw new ArgumentException($"Row must be in range 0 - {Modules.Length - 1}.", nameof(row));
            }
        }

        public async Task RunScrollingText(string text, int columnsPerSecond, bool loop = false, int row = 0, CancellationToken cancellationToken = default)
        {
            var isFirst = true;
            do
            {
                string modifiedText;
                if (isFirst)
                {
                    modifiedText = text;
                    isFirst = false;
                }
                else
                {
                    modifiedText = text.Trim().PadLeft(text.Length + OrderedModules.Length, ' ');
                }

                await PerformTextScroll(modifiedText, row, columnsPerSecond, cancellationToken);
            } while (loop && !cancellationToken.IsCancellationRequested);
        }

        private async Task PerformTextScroll(string text, int row, int columnsPerSecond, CancellationToken cancellationToken)
        {
            var textLength = text.Length;

            var columnCount = OrderedModules.First().GetColumnCount();

            var modifiedText = new string(text.Reverse().ToArray());
            var virtualDisplay = new Max7219MatrixDisplay(device, textLength);
            virtualDisplay.SetText(modifiedText);

            var delay = 1000 / columnsPerSecond;
            var i = 0;
            while (i++ < textLength * columnCount && !cancellationToken.IsCancellationRequested)
            {
                for (var index = 0; index < Modules[row].Length; index++)
                {
                    var target = Modules[row][index];
                    var source = virtualDisplay.OrderedModules[index];
                    virtualDisplay.CopyModule(source, target);
                }

                UpdateScreen();

                virtualDisplay.ShiftScreenLeft();

                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
