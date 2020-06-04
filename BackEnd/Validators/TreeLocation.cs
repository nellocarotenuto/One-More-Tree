using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

using AzureMapsToolkit;
using AzureMapsToolkit.Search;
using Microsoft.Extensions.DependencyInjection;

namespace BackEnd.Validators
{
    public class TreeLocation : ValidationAttribute
    {
        private static readonly Regex CoordinatesRegex =
            new Regex(@"^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?),\s*[-+]?(180(\.0+)?|((1[0-7]\d)|([1-9]?\d))(\.\d+)?)$");

        private readonly string _cityField;
        private readonly string _stateField;

        public TreeLocation(string cityField, string stateField)
        {
            _cityField = cityField;
            _stateField = stateField;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string coordinates = (string) value;

            if (!CoordinatesRegex.IsMatch(coordinates))
            {
                return new ValidationResult("The coordinates format isn't valid.");
            }
            
            // Check that the coordinates correspond to a valid location
            AzureMapsServices mapsClient = validationContext.GetService<AzureMapsServices>();
            Response<SearchAddressReverseResponse> searchAddressReverseResponse = 
                mapsClient.GetSearchAddressReverse(new SearchAddressReverseRequest { Query = coordinates, Language = "en-US" }).Result;
            
            if (searchAddressReverseResponse.Error != null)
            {
                return new ValidationResult("Unable to perform location validation with Azure Maps.");
            }

            SearchResultAddress searchResultAddress =
                searchAddressReverseResponse.Result.Addresses[0].Address;
            
            if (searchResultAddress.Municipality == null || searchResultAddress.Municipality == string.Empty)
            {
                return new ValidationResult("These coordinates do not match any city.");
            }

            // Set the additional info about the location
            PropertyInfo cityFieldInfo = validationContext.ObjectType.GetProperty(_cityField);
            cityFieldInfo.SetValue(validationContext.ObjectInstance, searchResultAddress.Municipality);

            PropertyInfo stateFieldInfo = validationContext.ObjectType.GetProperty(_stateField);
            stateFieldInfo.SetValue(validationContext.ObjectInstance, searchResultAddress.Country);

            return ValidationResult.Success;
        }
    }
}
