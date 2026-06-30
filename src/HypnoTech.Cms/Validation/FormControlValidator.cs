using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HypnoTech.Cms.FormMemberImplementations;
using HypnoTech.Cms.Forms.Controls;

namespace HypnoTech.Cms.Validation
{
    public static class FormControlValidator
    {
        public static bool Validate(this FormControl control, List<string> addedFiles)
        {
            var valid = ValidateRequired(control, addedFiles);
            if (!valid)
            {
                return false;
            }

            valid = ValidateUrl(control, addedFiles);
            if (!valid)
            {
                return false;
            }

            valid = ValidateCustom(control, addedFiles);
            if (!valid)
            {
                return false;
            }

            return true;
        }

        private static bool ValidateRequired(FormControl control, List<string> addedFiles)
        {
            if (control.Mapping.IsRequired)
            {
                if (control.Mapping is IFileUpload && control is IHasValue fileUploadShouldHaveValue)
                {
                    if (!fileUploadShouldHaveValue.HasValue && !addedFiles.Contains(control.Name))
                    {
                        control.ValidationMessage = control.Mapping.NameIsPlural ? $"{control.Label} are required." : $"{control.Label} is required.";
                        return false;
                    }
                }
                else if (control is IHasValue shouldHaveValue)
                {
                    if (!shouldHaveValue.HasValue)
                    {
                        control.ValidationMessage = control.Mapping.NameIsPlural ? $"{control.Label} are required." : $"{control.Label} is required.";
                        return false;
                    }
                }
                else if (control is NestedSelectListInputFormControl nested)
                {
                    if (string.IsNullOrEmpty(nested.Value))
                    {
                        control.ValidationMessage = control.Mapping.NameIsPlural ? $"{control.Label} are required." : $"{control.Label} is required.";
                        return false;
                    }

                    if (string.IsNullOrEmpty(nested.ChildValue))
                    {
                        control.ValidationMessage = control.Mapping.NameIsPlural ? $"{nested.ChildLabel} are required." : $"{nested.ChildLabel} is required.";
                        return false;
                    }
                }
                else if (control is PhotoGalleryFormControl gallery)
                {
                    if (!gallery.HasImages(addedFiles))
                    {
                        control.ValidationMessage = control.Mapping.NameIsPlural ? $"{control.Label} are required." : $"{control.Label} is required.";
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ValidateUrl(FormControl control, List<string> addedFiles)
        {
            if (control.Mapping.ValidateUrl)
            {
                if (control is IHasValue shouldHaveValue)
                {
                    if (shouldHaveValue.HasValue)
                    {
                        var regex = new Regex(
                            @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
                            RegexOptions.Compiled);

                        if (!regex.IsMatch(shouldHaveValue.Value.ToString()))
                        {
                            control.ValidationMessage = "Not a valid Url";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool ValidateCustom(FormControl control, List<string> addedFiles)
        {
            if (control.Mapping.CustomStringValidator != null && control is IHasValue stringValue)
            {
                var result = control.Mapping.CustomStringValidator.Invoke(stringValue.Value?.ToString());
                if (!string.IsNullOrEmpty(result))
                {
                    control.ValidationMessage = result;
                    return false;
                }
            }

            if (control.Mapping.CustomDateValidator != null && control is IHasValue dateValue)
            {
                string result;
                if (dateValue.Value is string dateString)
                {
                    if (DateTime.TryParse(dateString, out var dateTime))
                    {
                        result = control.Mapping.CustomDateValidator.Invoke(dateTime);
                    }
                    else
                    {
                        result = $"Can't parse {dateString} as a Date";
                    }
                }
                else
                {
                    result = control.Mapping.CustomDateValidator.Invoke((DateTime)dateValue.Value);
                }

                if (!string.IsNullOrEmpty(result))
                {
                    control.ValidationMessage = result;
                    return false;
                }
            }

            if (control.Mapping.CustomNumberValidator != null && control is IHasValue numberValue)
            {
                string result;
                if (numberValue.Value is string numberString)
                {
                    if (decimal.TryParse(numberString, out var number))
                    {
                        result = control.Mapping.CustomNumberValidator.Invoke(number);
                    }
                    else
                    {
                        result = $"Can't parse '{numberString}' as a number";
                    }
                }
                else
                {
                    result = control.Mapping.CustomNumberValidator.Invoke((decimal)numberValue.Value);
                }

                if (!string.IsNullOrEmpty(result))
                {
                    control.ValidationMessage = result;
                    return false;
                }
            }

            return true;
        }
    }
}
