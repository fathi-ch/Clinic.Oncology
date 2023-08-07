namespace Clinic.Core.Helpers;

public static class Guard
{
    public static void IsNotNull(object obj, string paramName)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(paramName, "A null reference exception for " + paramName);
        }
    }
}