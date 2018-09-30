using System;
using System.Collections.Generic;

namespace TelephoneCallsWebApplication.Models.DataGeneration
{
    public class PhoneNumberGeneration
    {
        private static readonly Random getRandom = new Random();
        private const int PHONE_NUMBERS_QUANTITY = 20;

        public static void GenerateInterlocutorsNumbers(out int callerSequenceNumber, out int receiverSequenceNumber)
        {
            do
            {
                callerSequenceNumber = GetRandomNumber(0, PHONE_NUMBERS_QUANTITY);
                receiverSequenceNumber = GetRandomNumber(0, PHONE_NUMBERS_QUANTITY);
            } while (callerSequenceNumber == receiverSequenceNumber);
        }

        public static List<int> GeneratePhoneNumbers()
        {
            List<int> phoneNumbers = new List<int>();
            while (phoneNumbers.Count < PHONE_NUMBERS_QUANTITY)
            {
                int newNumber = GeneratePhoneNumber();
                if (!phoneNumbers.Contains(newNumber))
                {
                    phoneNumbers.Add(newNumber);
                }
            }
            return phoneNumbers;
        }

        private static int GeneratePhoneNumber()
        {
            int numbOfDigits = 6;
            int number = (int)(GetFirstDigit() * Math.Pow(10, numbOfDigits));
            for (int i = numbOfDigits - 1; i >= 0; i--)
            {
                number += (int)(GetRandomNumber(0, 10) * Math.Pow(10, i));
            }
            return number;
        }

        private static int GetFirstDigit()
        {
            int firstDigit = GetRandomNumber(1, 4);
            switch (firstDigit)
            {
                case 1: return 3;
                case 2: return 5;
                case 3: return 8;
            }
            return 3;
        }

        private static int GetRandomNumber(int min, int max)
        {
            lock (getRandom)
            {
                return getRandom.Next(min, max);
            }
        }
    }
}
