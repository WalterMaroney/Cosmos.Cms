using System;
using System.Security.Cryptography;

namespace CDT.Cosmos.Cms.Services
{
    /// <summary>
    /// Random number generator
    /// </summary>
    public class RNGCryptoRandomGenerator
    {
        readonly RNGCryptoServiceProvider csp;

        /// <summary>
        /// Constructor
        /// </summary>
        public RNGCryptoRandomGenerator()
        {
            csp = new RNGCryptoServiceProvider();
        }

        /// <summary>
        /// Get next random number
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxExclusiveValue"></param>
        /// <returns></returns>
        public int Next(int minValue, int maxExclusiveValue)
        {
            if (minValue >= maxExclusiveValue)
                throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

            long diff = (long)maxExclusiveValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;

            uint ui;
            do
            {
                ui = GetRandomUInt();
            } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        private uint GetRandomUInt()
        {
            var randomBytes = GenerateRandomBytes(sizeof(uint));
            return BitConverter.ToUInt32(randomBytes, 0);
        }

        private byte[] GenerateRandomBytes(int bytesNumber)
        {
            byte[] buffer = new byte[bytesNumber];
            csp.GetBytes(buffer);
            return buffer;
        }
    }
}
