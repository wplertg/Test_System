using Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRepository
{
    public interface IUserRepository
    {
        UserLoginResultDTO Login(LoginDTO loginDTO);
    }
}
