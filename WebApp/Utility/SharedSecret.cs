
/* This generates a secure random per execution of the server
 * A server restart, will generate a new key, making all existing tokens invalid
 * For production (and if a load-balancer is used) come up with a persistent key strategy */
using System.Security.Cryptography;
using System.Threading;

public class SharedSecret : BackgroundService
{
    private static byte[] secret;
    private static TimeSpan PERIODIC_TIMER = TimeSpan.FromHours(24); // 24 hours
    private static PeriodicTimer timer = new PeriodicTimer(PERIODIC_TIMER);

    public static byte[] GetSharedKey()
    {
        if (secret == null)
        {
            // first time generate
            GenerateNewKey();
        }
        return secret;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Repeating
        while (await timer.WaitForNextTickAsync() && !stoppingToken.IsCancellationRequested)
        {
            GenerateNewKey();
        }
    }

    private static byte[] GenerateNewKey()
    {
        secret = new byte[32];
        var rand = RandomNumberGenerator.Create();
        rand.GetBytes(secret);
        return secret;
    }
}