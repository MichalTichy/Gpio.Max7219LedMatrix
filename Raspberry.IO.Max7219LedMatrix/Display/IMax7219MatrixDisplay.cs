using System;
using Raspberry.IO.Max7219LedMatrix.CharactersLibrary;
using Raspberry.IO.Max7219LedMatrix.Module;

namespace Raspberry.IO.Max7219LedMatrix.Display
{
    public interface IMax7219MatrixDisplay
    {
        void SetCharacterLibrary(IMatrixCharactersLibrary charactersLibrary);
        IMax7219MatrixDisplay Init();
        IMax7219MatrixDisplay UpdateScreen();
        IMax7219MatrixDisplay SendRaw(byte[] data);
        IMax7219MatrixDisplay SetBrightness(byte brightness);
        IMax7219MatrixDisplay SetBrightness(byte[] brightnesses);
        IMax7219MatrixDisplay TurnOff();
        IMax7219MatrixDisplay TurnOn();
        IMax7219MatrixDisplay TestMode(bool enabled = true);
        IMax7219MatrixDisplay ApplyToAllModules(Action<IMax7219MatrixModule> action);
        IMax7219MatrixDisplay InvertScreen();
        IMax7219MatrixDisplay ShiftScreenUp();
        IMax7219MatrixDisplay ShiftScreenDown();
        IMax7219MatrixDisplay ShiftScreenRight();
        IMax7219MatrixDisplay ShiftScreenLeft();
        IMax7219MatrixDisplay ClearDisplay();
        IMax7219MatrixDisplay CopyModule(IMax7219MatrixModule source, IMax7219MatrixModule target);
        IMax7219MatrixDisplay FillDisplay();
        IMax7219MatrixDisplay SetNumber(IMax7219MatrixModule module, int number);
        IMax7219MatrixDisplay SetCharacter(IMax7219MatrixModule module, char character);
        IMax7219MatrixDisplay Identify();
        IMax7219MatrixDisplay SetText(string text, int row = 0);
        IMax7219MatrixDisplay SetNumber(int number, int row = 0);
    }
}