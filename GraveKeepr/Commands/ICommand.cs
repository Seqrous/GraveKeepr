namespace GraveKeeper.Commands;

public interface ICommand
{
    Task ExecuteAsync();
}