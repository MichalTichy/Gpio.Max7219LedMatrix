using System;
using System.Device.Spi;
using System.Linq;
using System.Threading.Tasks;
using Raspberry.IO.Max7219LedMatrix.CharactersLibrary;
using Raspberry.IO.Max7219LedMatrix.Display;
using Raspberry.IO.Max7219LedMatrix.Module;

namespace Raspberry.IO.Max7219LedMatrix.Example
{
    internal class CodingCharacters : IMatrixCharactersLibrary
    {
        public byte[] GetCharacter(char character)
        {
            switch (character)
            {
                case '1':
                    return new byte[] { 0x00, 0x31, 0x4a, 0x42, 0x42, 0x4a, 0x31, 0x00 };
                case '2':
                    return new byte[] { 0x00, 0x9c, 0x52, 0x52, 0x52, 0x52, 0x9c, 0x00 };
                case '3':
                    return new byte[] { 0x00, 0xa2, 0xb2, 0xba, 0xae, 0xa6, 0xa2, 0x00 };
                case '4':
                    return new byte[] { 0x00, 0x60, 0x90, 0x80, 0xb0, 0x90, 0x60, 0x00 };
                default:
                    throw new ArgumentException("unsupported character");
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 1000000, // Set the SPI bus speed to 1 MHz
                Mode = SpiMode.Mode0 // Set the SPI mode to Mode0
            };
            var device = SpiDevice.Create(settings);

            var modules = new[]
            {
                new Max7219MatrixModule(3, FlipModule),
                new Max7219MatrixModule(2, FlipModule),
                new Max7219MatrixModule(1, FlipModule),
                new Max7219MatrixModule(0, FlipModule)
            };
            var display = new Max7219MatrixDisplay(device, new[] { modules });

            display.Init();
            await Task.Delay(500);

            display.TurnOn();
            await Task.Delay(500);

            display.ClearDisplay();
            display.UpdateScreen();

            await Task.Delay(500);
            display.SetBrightness(5);

            display.SetCharacterLibrary(new CodingCharacters());
            byte brightness = 0xf;
            while (true)
            {
                display.SetText("1234");
                display.UpdateScreen();
                await Task.Delay(100);
                display.SetBrightness(brightness);
                brightness--;
                if (brightness == 0)
                {
                    brightness = 0xf;
                }
            }
        }

        private static byte[] FlipModule(byte[] data)
        {
            data = Max7219MatrixModule.FlipDataHorizontal(data);
            data = Max7219MatrixModule.FlipDataVertical(data);
            return data;
        }
    }
}
