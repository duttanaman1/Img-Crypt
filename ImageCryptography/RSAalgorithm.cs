namespace ImageCrypto
{
  internal static class RSAalgorithm
  {
    private static long Square(long a)
    {
      return a * a;
    }

    public static long BigMod(int b, int p, int m) //b^p%m=?
    {
      if (p == 0)
        return 1;
      if (p % 2 == 0)
        return Square(BigMod(b, p / 2, m)) % m;
      return b % m * BigMod(b, p - 1, m) % m;
    }

    public static int n_value(int prime1, int prime2)
    {
      return prime1 * prime2;
    }

    public static int cal_phi(int prime1, int prime2)
    {
      return (prime1 - 1) * (prime2 - 1);
    }

    public static int cal_privateKey(int phi, int e, int n)
    {
      int d;

      for (d = 1; ; d++)
      {
        var res = d * e % phi;
        if (res == 1) break;
      }

      return d;
    }
  }
}