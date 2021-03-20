using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication.Exceptions.ChangePasswordExceptions
{
    public class PasswordLowSecurityException : ChangePasswordException
    {
        public PasswordLowSecurityException()
        {

        }

        public PasswordLowSecurityException(string message)
            : base(message)
        {

        }
    }
}
