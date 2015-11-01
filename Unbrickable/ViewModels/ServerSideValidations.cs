using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class ServerSideValidations
    {
        public class NameFormat : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value != null)
                {
                    String name_val = value as String;
                    if (name_val.Length <= 50 && Regex.IsMatch(name_val, @"[ A-Za-z0-9\-]$"))
                    {
                        return ValidationResult.Success;
                    }
                }                
                return new ValidationResult(this.ErrorMessage, new[] { validationContext.MemberName });
            }
        }

        public class Over18 : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value != null)
                {
                    int year = Convert.ToInt32(value);
                    int year_now = DateTime.Now.Year;

                    if (year_now - year >= 18)
                    {
                        return ValidationResult.Success;
                    }
                }                
                return new ValidationResult(this.ErrorMessage, new[] { validationContext.MemberName });
            }
        }

        public class UsernameFormat : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value != null)
                {
                    String username_val = value as String;
                    if (username_val.Length <= 50 && Regex.IsMatch(username_val, @"[A-Za-z0-9\-_]$"))
                    {
                        return ValidationResult.Success;
                    }
                }                
                return new ValidationResult(this.ErrorMessage, new[] { validationContext.MemberName });
            }
        }

    }
}