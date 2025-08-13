using System;

class P
{
    static void Main()
    {
        // Buraya KENDİ gerçek şifreni yaz:
        var plain = "SeninGizliSifren";
        var hash = BCrypt.Net.BCrypt.HashPassword(plain, workFactor: 11);
        Console.WriteLine(hash);
    }
}
