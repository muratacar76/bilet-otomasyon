using System;

class Program
{
    static void Main()
    {
        string password = "bulut1234";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        Console.WriteLine($"Hashed password: {hashedPassword}");
    }
}