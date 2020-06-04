using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BackEnd.Validators
{
    public class TreePhoto : ValidationAttribute
    {
        private static readonly IEnumerable<String> AcceptedObjects = new List<String>() { "tree" };
        private static readonly IEnumerable<String> AcceptedTags = new List<String>() { "tree" };
        private static readonly IEnumerable<String> RejectedTags = new List<String>() { "art", "cartoon", "drawing" };

        private static readonly IList<VisualFeatureTypes> features =
            new List<VisualFeatureTypes>() { VisualFeatureTypes.Adult, VisualFeatureTypes.ImageType, VisualFeatureTypes.Objects, VisualFeatureTypes.Tags };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            IFormFile file = (IFormFile)value;

            // Check that the picture has been provided and has a valid MIME type
            if (file == null || file.Length == 0 || file.ContentType != "image/png")
            {
                return new ValidationResult("A .png file must be attached.");
            }

            if (file.Length > 4 * 1024 * 1024)
            {
                return new ValidationResult("The picture is too big.");
            }
            
            // Chack that the picture actually has content of interest
            ComputerVisionClient computerVisionClient = validationContext.GetService<ComputerVisionClient>();
            ImageAnalysis imageAnalysis = computerVisionClient.AnalyzeImageInStreamAsync(file.OpenReadStream(), features).Result;

            IEnumerable<String> imageTags = from tag in imageAnalysis.Tags
                                            select tag.Name;

            IEnumerable<String> imageObjects = from item in imageAnalysis.Objects
                                               select item.ObjectProperty;

            if (imageAnalysis.Adult.IsAdultContent || imageAnalysis.Adult.IsRacyContent)
            {
                return new ValidationResult("The picture is either racy or contains adult content.");
            }

            if (imageAnalysis.ImageType.ClipArtType != 0 ||
                imageAnalysis.ImageType.LineDrawingType != 0 ||
                imageTags.Intersect(RejectedTags).Any())
            {
                return new ValidationResult("The picture must be a real photo.");
            }

            if (imageTags.Intersect(AcceptedTags).Count() == 0 && imageObjects.Intersect(AcceptedObjects).Count() == 0)
            {
                return new ValidationResult("The picture does not contain a tree.");
            }

            return ValidationResult.Success;
        }
    }
}
