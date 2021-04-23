namespace Raspberry.IO.Max7219LedMatrix.CharactersLibrary
{
    public interface IMatrixCharactersLibrary
    {
        byte[] GetCharacter(char character);
    }
}