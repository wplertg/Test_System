using IRepository;
using Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class UserServer : IBLL.IUserServer
    {
        private readonly IUserRepository _userRepository;
        public UserServer(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public UserLoginResultDTO Login(LoginDTO loginDTO)
        {
            return _userRepository.Login(loginDTO);
        }
    }
}
