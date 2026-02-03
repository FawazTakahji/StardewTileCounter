namespace TileCounter;

public static class SafeFireAndForgetExtensions
{
    public static async void SafeFireAndForget(this Task task, Action<Exception> onException)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            onException(ex);
        }
    }
}