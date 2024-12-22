// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace ItemzApp.API.Helper
{
    public static class HierarchyIdStringHelper
    {
        public static string ManuallyGenerateHierarchyIdNumberString(string originalHierarchyIdString, int diffValue, bool addDecimal = false)
        {
            if (!(
                    (originalHierarchyIdString.Substring(originalHierarchyIdString.Length - 2, 2) == "/\"")
                    ||
                    (originalHierarchyIdString.Substring(originalHierarchyIdString.Length - 1, 1) == "/")
                 ))
            {
                var errorMessage = $"Provided Hierarchy string '{originalHierarchyIdString}' format is not valid as it should end with "
                + "'/'" + " OR " + "'/\"'";
                throw new ArgumentException(errorMessage);
            }

            if (diffValue == 0)
            {
                var errorMessage = $"Provided value of '{diffValue}' for increasing or decreasing HierarchyID has to be NON-ZERO positive or negative integer.";
                throw new ArgumentException(errorMessage);
            }

            var lastSlashPosition = originalHierarchyIdString.LastIndexOf("/");
            var secondLastSlashPosition = originalHierarchyIdString.LastIndexOf("/", lastSlashPosition - 1);

            var splitStringArray = originalHierarchyIdString.Split("/");

            var stringToBeModified = splitStringArray[(splitStringArray.Length - 2)];

            var modifiedString = "";
            if (stringToBeModified.Contains("."))
            {
                var lastDecimalPlace = stringToBeModified.LastIndexOf(".");

                var splitDecimalStringArray = stringToBeModified.Split(".");
                var originalNumber = int.Parse(splitDecimalStringArray[(splitDecimalStringArray.Length - 1)]);
                if (addDecimal)
                {
                    modifiedString = stringToBeModified.Remove(lastDecimalPlace)
                                    .Insert(lastDecimalPlace,
                                    "."
                                    + originalNumber
                                    + "."
                                    + diffValue);
                }
                else
                {
                    modifiedString = stringToBeModified.Remove(lastDecimalPlace)
                                    .Insert(lastDecimalPlace, "." + (originalNumber + diffValue).ToString());
                }
            }
            else
            {
                if (addDecimal)
                {
                    modifiedString = stringToBeModified
                                     + "."
                                     + diffValue;
                }
                else
                {
                    modifiedString = ((int.Parse(stringToBeModified)) + diffValue).ToString();
                }
            }
            var updatedHierarchyIdString = originalHierarchyIdString.Substring(0, secondLastSlashPosition + 1)
                                                + modifiedString
                                                + "/"
                                                + ((originalHierarchyIdString.Substring(0, 1) == "\"") ? "\"" : "");

            return (updatedHierarchyIdString.Replace("-0", "0"));
        }
    }
}
