using Framework.Authentication.Exceptions.ChangePasswordExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication.Exceptions
{
    public class PasswordMisMatchException : ChangePasswordException
    {
        public PasswordMisMatchException()
        {

        }

        public PasswordMisMatchException(string message)
            : base(message)
        {

        }
    }
}
