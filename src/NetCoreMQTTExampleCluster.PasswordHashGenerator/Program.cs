// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A program to hash passwords for new users.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.PasswordHashGenerator;

/// <summary>
/// A program to hash passwords for new users.
/// </summary>
public static class Program
{
    /// <summary>
    /// The password hasher.
    /// </summary>
    private static readonly IPasswordHasher<User> PasswordHasher = new PasswordHasher<User>();

    /// <summary>
    /// The main method of the program.
    /// </summary>
    public static void Main()
    {
        var hashAnother = new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false);

        while (hashAnother.Key == ConsoleKey.Y)
        {
            Console.WriteLine("Please insert a password to hash and press enter.");
            var password = Console.ReadLine();
            var hashedPassword = HashPassword(password);
            Console.WriteLine($"Your hashed password is: {hashedPassword}");
            Console.WriteLine("Do you want to hash another password? y/n");
            hashAnother = Console.ReadKey();

            // Wait for the enter key to be pressed and add a new line to not overwrite the user input.
            Console.ReadKey();
            Console.WriteLine();
        }

        Console.WriteLine("You can close the window now.");
    }

    /// <summary>
    /// Hashes the given password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A hash representation of the password.</returns>
    private static string HashPassword(string password)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = string.Empty,
            ClientId = string.Empty
        };

        return PasswordHasher.HashPassword(user, password);
    }
}
