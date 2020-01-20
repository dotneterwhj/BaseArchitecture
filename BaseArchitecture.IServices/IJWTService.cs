using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace BaseArchitecture.IServices
{
    public interface IJWTService
    {
        string GetToken(Claim[] claims);
    }
}
