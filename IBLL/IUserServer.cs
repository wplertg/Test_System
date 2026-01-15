using Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IUserServer
    {
        UserLoginResultDTO Login(LoginDTO loginDTO);
    }
}
