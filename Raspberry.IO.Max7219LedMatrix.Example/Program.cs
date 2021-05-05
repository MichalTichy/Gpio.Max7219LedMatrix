using System;
using System.Threading.Tasks;
using Raspberry.IO.Max7219LedMatrix.Display;
using Unosquare.RaspberryIO;

namespace Raspberry.IO.Max7219LedMatrix.Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var display = new Max7219MatrixDisplay(Pi.Spi.Channel0, 4);

            display.Init();

            display.SetText("TEST");
            display.UpdateScreen();
            display.SetBrightness(5);

            await Task.Delay(500);

            display.TurnOff();
        }
    }
}
