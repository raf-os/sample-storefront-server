namespace SampleStorefront.Services;

public class PasswordService
{
    public string? HashPassword(string password)
    {
        try
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            return hashedPassword;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public bool CheckHashedPassword(string password, string hash)
    {
        try
        {
            bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
            return isValid;
        }
        catch (Exception)
        {
            return false;
        }
    }
}