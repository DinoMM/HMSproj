using HMSModels.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMSModels
{
    public class LoginRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public IdentityUserOwn User { get; set; } = default;
        public List<RolesOwn> Roles { get; set; } = new();
    }

    public class UpdateRequest<T>
    {
        public object Id { get; set; }
        public T Entity { get; set; }
    }

    public class ValueResponse 
    {
        public object? Value { get; set; }
        public string? TypeOfValue { get; set; }
    }
}
