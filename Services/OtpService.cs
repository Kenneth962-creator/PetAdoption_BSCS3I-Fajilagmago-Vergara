using System;
using System.Security.Cryptography;

namespace PetAdoptionManagementSystem.Services
{
    public class OtpService
    {
        public string GenerateOtp()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }
    }
}
