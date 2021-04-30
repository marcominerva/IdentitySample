using System.Collections.Generic;

namespace IdentitySample.Shared.Models
{
    public class RegisterResponse
    {
        public bool Succeeded { get; set; }

        public IEnumerable<string> Errors { get; set; }
    }
}
