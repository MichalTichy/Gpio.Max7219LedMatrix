using System.Threading.Tasks;
using Raspberry.IO.Max7219LedMatrix.Display;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace Raspberry.IO.Max7219LedMatrix.Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Pi.Init<BootstrapWiringPi>();

            var display = new Max7219MatrixDisplay(Pi.Spi.Channel0, 4);

            display.Init();
            await Task.Delay(500);

            display.TurnOn();
            await Task.Delay(500);

            display.TurnOn();

            display.ClearDisplay();
            display.UpdateScreen();

            await Task.Delay(500);
            display.SetBrightness(5);

            for (var i = 0; i < 9999; i++)
            {
                display.SetText(i.ToString().PadLeft(4, '0'));
                display.UpdateScreen();
                await Task.Delay(50);
            }

            display.TurnOff();
        }
    }
}
