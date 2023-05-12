
/* This generates a secure random per execution of the server
 * A server restart, will generate a new key, making all existing tokens invalid
 * For production (and if a load-balancer is used) come up with a persistent key strategy */
using System.Security.Cryptography;

public static class SharedSecret
{
    private static byte[] secret;
    public static byte[] GetSharedKey()
    {
        if (secret == null)
        {
            byte[] secret = new byte[32];
            var rand = RandomNumberGenerator.Create();
            rand.GetBytes(secret);
        }
        return secret;
    }
}