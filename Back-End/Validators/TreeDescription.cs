﻿using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back_End.Validators
{
    public class TreeDescription : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string description = (string) value;

            // Do not perform checks if the value is empty
            if (description == string.Empty)
            {
                return ValidationResult.Success;
            }

            Stream descriptionStream = new MemoryStream(Encoding.UTF8.GetBytes(description));

            // Check that the description isn't inappropriate
            ContentModeratorClient contentModeratorClient = validationContext.GetService<ContentModeratorClient>();
            Screen textScreen = contentModeratorClient.TextModeration.ScreenText("text/plain", descriptionStream);

            if (textScreen.Terms != null)
            {
                return new ValidationResult("The description should not contain profane terms.");
            }

            return ValidationResult.Success;
        }
    }
}