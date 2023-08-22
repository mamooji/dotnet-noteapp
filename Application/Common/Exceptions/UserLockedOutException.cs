namespace Application.Common.Exceptions;

public class UserLockedOutException : Exception
{
    public UserLockedOutException(int maximumTries)
        : base(
            $"Your account has been locked due to {maximumTries} invalid attempts. Please contact QUES support for help.")
    {
    }
}