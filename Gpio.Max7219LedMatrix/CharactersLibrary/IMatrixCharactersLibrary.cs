namespace Gpio.Max7219LedMatrix.CharactersLibrary
{
    public interface IMatrixCharactersLibrary
    {
        byte[] GetCharacter(char character);
    }
}
