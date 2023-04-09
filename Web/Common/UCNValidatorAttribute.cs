﻿using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Web.Common
{
    public class UCNValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var ucn = value?.ToString()?.ToCharArray();
            if ((ucn?.Any(x => !char.IsDigit(x))?? true) || (ucn?.Length ?? 0) != 10) 
                return new ValidationResult("Invalid UCN");

            var coefficients = new int[] { 2, 4, 8, 5, 10, 9, 7, 3, 6 };
            var sum = 0;

            for (var i = 0; i < 9; i++)
                sum += int.Parse(ucn[i].ToString()) * coefficients[i];

            return sum % 11 != int.Parse(ucn[9].ToString()) ? new ValidationResult("Invalid UCN") : ValidationResult.Success;
        }
    }
}
