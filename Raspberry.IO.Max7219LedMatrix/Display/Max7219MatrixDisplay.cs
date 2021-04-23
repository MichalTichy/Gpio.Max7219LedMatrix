using System;
using System.Collections.Generic;
using System.Linq;
using Raspberry.IO.Max7219LedMatrix.CharactersLibrary;
using Raspberry.IO.Max7219LedMatrix.Module;
using Unosquare.RaspberryIO.Abstractions;

namespace Raspberry.IO.Max7219LedMatrix.Display
{
    public class Max7219MatrixDisplay : IMax7219MatrixDisplay
    {
        protected const byte ShutdownByte = 0x0C;
        protected const byte BrightnessByte = 0x0a;
        protected const byte TestModeByte = 0x0f;
        protected const byte DecodeModeByte = 0x09;
        protected const byte ScanLimitByte = 0x0b;


        protected readonly ISpiChannel _channel;
        private IMax7219MatrixModule[][] _modules;
        private IMax7219MatrixModule[] _orderedModules;
        private IMatrixCharactersLibrary _matrixCharactersLibrary;

        public Max7219MatrixDisplay(ISpiChannel channel, int numberOfModules)
        {
            _channel = channel;
            InitModules(numberOfModules);
            InitOrderedModules();
            _matrixCharactersLibrary = new MatrixCharactersLibrary();
        }


        public Max7219MatrixDisplay(ISpiChannel channel, IMax7219MatrixModule[][] modules)
        {
            _channel = channel;
            _modules = modules;
            InitOrderedModules();
            _matrixCharactersLibrary = new MatrixCharactersLibrary();
        }

        public void SetCharacterLibrary(IMatrixCharactersLibrary charactersLibrary)
        {
            _matrixCharactersLibrary = charactersLibrary;
        }


        public virtual IMax7219MatrixDisplay Init()
        {
            SendRaw(new byte[] {DecodeModeByte, 0x00});
            SendRaw(new byte[] {ScanLimitByte, 0x0f});
            return this;
        }

        public virtual IMax7219MatrixDisplay UpdateScreen()
        {
            foreach (var module in _orderedModules)
            {
                module.ApplyPreprocessing();
                SendRaw(module.Get());
            }

            return this;
        }

        public virtual IMax7219MatrixDisplay SendRaw(byte[] data)
        {
            _channel.Write(data);
            return this;
        }

        public virtual IMax7219MatrixDisplay SetBrightness(byte brightness)
        {
            var brightnesses = new byte[_orderedModules.Length];
            for (int i = 0; i < _orderedModules.Length; i++)
            {
                brightnesses[i] = brightness;
            }

            return SetBrightness(brightnesses);
        }

        public virtual IMax7219MatrixDisplay SetBrightness(byte[] brightnesses)
        {
            if (brightnesses.Length > _orderedModules.Length)
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
            SendRaw(new byte[] {ShutdownByte, 0x00});
            return this;
        }

        public virtual IMax7219MatrixDisplay TurnOn()
        {
            SendRaw(new byte[] {ShutdownByte, 0x01});
            return this;
        }

        public virtual IMax7219MatrixDisplay TestMode(bool enabled = true)
        {
            byte enabledByte = 0x00;
            byte disabledByte = 0x01;
            byte enable = enabled ? enabledByte : disabledByte;
            SendRaw(new byte[] {TestModeByte, enable});
            return this;
        }

        public virtual IMax7219MatrixDisplay ApplyToAllModules(Action<IMax7219MatrixModule> action)
        {
            foreach (var module in _orderedModules)
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

            foreach (var row in _modules.Reverse())
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

            foreach (var row in _modules)
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

            foreach (var row in _modules)
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

            foreach (var row in _modules)
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


        public virtual IMax7219MatrixDisplay SetNumber(IMax7219MatrixModule module, int number)
        {
            if (number < 0 || number > 9)
                throw new ArgumentException("Number is out of valid range of 0-9!", nameof(number));

            SetCharacter(module, number.ToString()[0]);
            return this;
        }

        public virtual IMax7219MatrixDisplay SetCharacter(IMax7219MatrixModule module, char character)
        {
            module.Set(_matrixCharactersLibrary.GetCharacter(character));
            return this;
        }

        public virtual IMax7219MatrixDisplay Identify()
        {
            ApplyToAllModules(module => SetNumber(module, module.ModuleNumber));
            return this;
        }

        public virtual IMax7219MatrixDisplay SetText(string text, int row = 0)
        {
            CheckTextLength(text, row);
            for (int i = 0; i < text.Length; i++)
            {
                SetCharacter(_modules[row][i], text[i]);
            }

            return this;
        }

        public IMax7219MatrixDisplay SetNumber(int number, int row = 0)
        {
            return SetText(number.ToString());
        }

        private void InitModules(int numberOfModules)
        {
            _modules = new IMax7219MatrixModule[1][];
            for (int i = 0; i < numberOfModules; i++)
            {
                _modules[0][i] = new Max7219MatrixModule(i);
            }
        }

        private void InitOrderedModules()
        {
            if (_modules == null)
            {
                return;
            }

            var d = new Dictionary<int, IMax7219MatrixModule>();
            foreach (var module in _modules.SelectMany(t => t))
            {
                if (d.ContainsKey(module.ModuleNumber))
                {
                    throw new ArgumentException(
                        $"Found two modules with duplicate {nameof(module.ModuleNumber)} {module.ModuleNumber}");
                }

                d[module.ModuleNumber] = module;
            }

            _orderedModules = d.Values.OrderBy(t => t.ModuleNumber).ToArray();
        }

        private void CheckTextLength(string text, int row)
        {
            CheckRow(row);
            var rowArray = _modules[row];
            if (rowArray.Length < text.Length)
            {
                throw new ArgumentException(
                    $"Provided text is too long for given row. Maximum length is {rowArray.Length}.");
            }
        }

        private void CheckRow(int row)
        {
            if (row < 0 || _modules.Length >= row)
            {
                throw new ArgumentException($"Row must be in range 0 - {_modules.Length - 1}.", nameof(row));
            }
        }
    }
}