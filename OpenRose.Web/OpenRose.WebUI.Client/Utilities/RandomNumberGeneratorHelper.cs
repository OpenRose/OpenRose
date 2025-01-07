using System;

namespace OpenRose.WebUI.Client.Utilities
{
    public static class RandomNumberGeneratorHelper
    {
        private static readonly Random random = new Random();

        public static int GenerateRandomNumber()
        {
            return random.Next(1, 100001);
        }
    }
}
