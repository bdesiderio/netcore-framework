using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication.Exceptions.ChangePasswordExceptions
{
    public class ChangePasswordException : Exception
    {
        public ChangePasswordException()
        {

        }

        public ChangePasswordException(string message)
            : base(message)
        {

        }
    }
}
