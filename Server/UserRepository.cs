using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class UserRepository : IRepository.IUserRepository
    {
        private readonly AppDbContext _db;
        private readonly Logger<UserRepository> _logger;
        public UserRepository(AppDbContext db,Logger<UserRepository> logger)
        {
            _db = db;
            _logger = logger;
        }
        public UserLoginResultDTO Login(LoginDTO loginDTO)
        {
            throw new NotImplementedException();
        }
    }
}
