using Backend.Data;

namespace Backend.Services;

public interface IWordProcessingService
{
    public void HandleNewWord(WordToken wordToken);
}