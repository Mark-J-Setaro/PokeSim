using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;


namespace PokeSim
{


    public static class ValidationHelpers
    {

        public static bool isUserAdmin( Controller theController )
        {
            return theController.User.IsInRole(EnumHelpers.ROLE_ADMIN);
        }

        /// <summary>
        /// Pass in the parameter name, the form collection, whether or not we care if the item is in the collection, whether or not we care if the item found in the collection is valid, and the default value to return if something goes wrong.
        /// Throws exception if it must be in the collection and it's not; otherwise returns default value.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsStringEnum<T>(FormCollection collection, string key, bool mustBeInCollection = true, bool mustBeValid = true, int defaultVal = 0) where T: struct, IConvertible
        {
            Dictionary<string, int> stringToInt = EnumHelpers.enumNameToIntDict<T>();
            string stringVal = null;
            try
            {
                stringVal = collection[key].Trim();
            }
            catch (IndexOutOfRangeException)
            {
                if (mustBeInCollection)
                {
                    throw new Exception("(StringEnumCol)Could not find Field '" + key + "' in form collection.");
                }
                else
                {
                    return defaultVal;
                }
            }
            catch (Exception ex)
            {
                //should not reach here.
                throw new Exception("(StringEnumCol)Encountered unexpected exception trying to retrieve Field '" + key + "' from form collection; " + ex.Message);
            }
            return validateAsStringEnum<T>(stringVal, key, mustBeValid, defaultVal);
        }

        /// <summary>
        /// Pass in the string value representing the enum.ToString value, the key name, whether or not we care if the string parses as the enum, and the default value to return if it doesn't.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsStringEnum<T>(string stringVal, string key, bool mustBeValid = true, int defaultVal = 0) where T : struct, IConvertible
        {
            Dictionary<string, int> stringToInt = EnumHelpers.enumNameToIntDict<T>();
            int retVal = defaultVal;
            try
            {
                retVal = stringToInt[stringVal];
            }
            catch (Exception ex)
            {
                if (mustBeValid)
                {
                    StringBuilder sb = new StringBuilder("(StringEnum)Could not parse Enum-String value '" + stringVal + "' for collection Field '" + key + "', reason: " + ex.Message);
                    sb.Append("Expected values: ");
                    foreach (KeyValuePair<string, int> entry in stringToInt)
                    {
                        sb.Append("Key: " + entry.Key + "| Value: " + entry.Value + "\r\n");
                    }
                    throw new Exception(sb.ToString());
                }
            }
            return retVal;
        }

        /// <summary>
        /// Pass in the parameter name, the form collection, whether or not we care if the item is in the collection, whether or not we care if the item found in the collection is valid, and the default value to return if something goes wrong.
        /// Throws exception if it must be in the collection and it's not; otherwise returns default value.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsIntEnum<T>(FormCollection collection, string key, bool mustBeInCollection = true, bool mustBeValid = true, int defaultVal = 0) where T : struct, IConvertible
        {
            Dictionary<string, int> stringToInt = EnumHelpers.enumNameToIntDict<T>();
            string stringVal = null;
            try
            {
                stringVal = collection[key].Trim();
            }
            catch (IndexOutOfRangeException)
            {
                if (mustBeInCollection)
                {
                    throw new Exception("(IntEnumCol)Could not find Field '" + key + "' in form collection.");
                }
                else
                {
                    return defaultVal;
                }
            }
            catch (Exception ex)
            {
                //should not reach here.
                throw new Exception("(IntEnumCol)Encountered unexpected exception trying to retrieve Field '" + key + "' from form collection; " + ex.Message);
            }
            return validateAsIntEnum<T>(stringVal, key, mustBeValid, defaultVal);
        }

        /// <summary>
        /// Pass in the string value representing the enum.ToString value, the key name, whether or not we care if the string parses as the enum, and the default value to return if it doesn't.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsIntEnum<T>(string stringVal, string key, bool mustBeValid = true, int defaultVal = 0) where T : struct, IConvertible
        {
            int retVal = defaultVal;
            try
            {
                int dubiousInt = Convert.ToInt32(stringVal);
                EnumHelpers.enumContainsInt<T>(dubiousInt, true);
                retVal = dubiousInt;
            }
            catch (Exception ex)
            {
                if (mustBeValid)
                {
                    throw new Exception("(IntEnum)Could not parse int value '" + stringVal + "' for collection Field '" + key + "', reason: " + ex.Message);
                }
            }
            return retVal;
        }






        /// <summary>
        /// Pass in the parameter name, the form collection, whether or not we care if the item is in the collection, whether or not we care if the item found in the collection is valid, and the default value to return if something goes wrong.
        /// Throws exception if it must be in the collection and it's not; otherwise returns default value.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsInt(FormCollection collection, string key, bool mustBeInCollection = true, bool mustBeValid = true, int defaultVal = 0)
        {
            string stringVal = null;
            try
            {
                stringVal = collection[key].Trim();
            }
            catch (IndexOutOfRangeException)
            {
                if (mustBeInCollection)
                {
                    throw new Exception("(IntCol)Could not find Field '" + key + "' in form collection.");
                }
                else
                {
                    return defaultVal;
                }
            }
            catch (Exception ex)
            {
                //should not reach here.
                throw new Exception("(IntCol)Encountered unexpected exception trying to retrieve Field '" + key + "' from form collection; " + ex.Message);
            }
            return validateAsInt(stringVal, key, mustBeValid, defaultVal);
        }


        /// <summary>
        /// Pass in the string value, the key name, whether or not the string value must parse as an integer, and the default value to return if it doesn't.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsInt(string stringVal, string key, bool mustBeValid = true, int defaultVal = 0)
        {
            int retVal = defaultVal;
            try
            {
                retVal = Convert.ToInt32(stringVal);
            }
            catch
            {
                if (mustBeValid)
                {
                    throw new Exception("(Int)Could not parse int value '" + stringVal + "' for collection key " + key + " to integer.");
                }
            }
            return retVal;
        }

        /// <summary>
        /// Pass in the Dictionary, the parameter name, the form collection, whether or not we care if the item is in the collection, whether or not we care if the item found in the collection is valid, and the default value to return if something goes wrong.
        /// Throws exception if it must be in the collection and it's not; otherwise returns default value.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsIntDict(Dictionary<int, string> theDict, FormCollection collection, string key, bool mustBeInCollection = true, bool mustBeValid = true, int defaultVal = 0)
        {
            string stringVal = null;
            try
            {
                stringVal = collection[key].Trim();
            }
            catch (IndexOutOfRangeException)
            {
                if (mustBeInCollection)
                {
                    throw new Exception("(IntDictColl)Could not find Field '" + key + "' in form collection.");
                }
                else
                {
                    return defaultVal;
                }
            }
            catch (Exception ex)
            {
                //should not reach here.
                throw new Exception("(IntDictColl)Encountered unexpected exception trying to retrieve Field '" + key + "' from form collection; " + ex.Message);
            }
            return validateAsIntDict(theDict, stringVal, key, mustBeValid, defaultVal);
        }


        /// <summary>
        /// Pass in the Dictionary, string value, the key name, whether or not the string value must parse as an integer, and the default value to return if it doesn't.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsIntDict(Dictionary<int, string> theDict, string stringVal, string key, bool mustBeValid = true, int defaultVal = 0)
        {
            int retVal = defaultVal;
            try
            {
                retVal = Convert.ToInt32(stringVal);
            }
            catch
            {
                if (mustBeValid)
                {
                    throw new Exception("(IntDict)Could not parse int value '" + stringVal + "' for collection key " + key + " to integer.");
                }
            }
            if (mustBeValid && !theDict.ContainsKey( retVal ) )
            {
                throw new Exception("(IntDict)Could match the value/int '" + stringVal + "'/" + retVal +" for collection key " + key + " to dictionary.");
            }
            return retVal;
        }


        /// <summary>
        /// Pass in the Dictionary, the parameter name, the form collection, whether or not we care if the item is in the collection, whether or not we care if the item found in the collection is valid, and the default value to return if something goes wrong.
        /// Throws exception if it must be in the collection and it's not; otherwise returns default value.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsStringDict(Dictionary<string, int> theDict, FormCollection collection, string key, bool mustBeInCollection = true, bool mustBeValid = true, int defaultVal = 0)
        {
            string stringVal = null;
            try
            {
                stringVal = collection[key].Trim();
            }
            catch (IndexOutOfRangeException)
            {
                if (mustBeInCollection)
                {
                    throw new Exception("(StringDictColl)Could not find Field '" + key + "' in form collection.");
                }
                else
                {
                    return defaultVal;
                }
            }
            catch (Exception ex)
            {
                //should not reach here.
                throw new Exception("(StringDictColl)Encountered unexpected exception trying to retrieve Field '" + key + "' from form collection; " + ex.Message);
            }
            return validateAsStringDict(theDict, stringVal, key, mustBeValid, defaultVal);
        }


        /// <summary>
        /// Pass in the Dictionary, string value, the key name, whether or not the string value must parse as an integer, and the default value to return if it doesn't.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static int validateAsStringDict(Dictionary<string, int> theDict, string stringVal, string key, bool mustBeValid = true, int defaultVal = 0)
        {
            int retVal = defaultVal;
            try
            {
                retVal = theDict[stringVal];
            }
            catch
            {
                if (mustBeValid)
                {
                    throw new Exception("(StringDict)Could not parse value '" + stringVal + "' for collection key " + key + " to integer.");
                }
            }
            if (mustBeValid && !theDict.ContainsValue(retVal))
            {
                throw new Exception("(StringDict)Could match the value/int '" + stringVal + "'/" + retVal + " for collection key " + key + " to dictionary.");
            }
            return retVal;
        }




        /// <summary>
        /// Pass in the parameter name, the form collection, whether or not we care if the item is in the collection, whether or not we care if the item found is null/whitespace, and the default value to return if something goes wrong.
        /// Throws exception if it must be in the collection and it's not; otherwise returns default value.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static string validateAsString(FormCollection collection, string key, bool mustBeInCollection = true, bool mustNotBeNullOrWhitespace = true, string defaultVal = "")
        {
            string stringVal = null;
            string retVal = defaultVal;
            try
            {
                stringVal = collection[key];
            }
            catch (IndexOutOfRangeException)
            {
                if (mustBeInCollection)
                {
                    throw new Exception("(StringCol)Could not find Field '" + key + "' in form collection.");
                }
                else
                {
                    return defaultVal;
                }
            }
            catch (Exception ex)
            {
                //should not reach here.
                throw new Exception("Encountered unexpected exception trying to retrieve Field '" + key + "' from form collection; " + ex.Message);
            }

            retVal = validateAsString(stringVal, key, mustNotBeNullOrWhitespace, defaultVal);
            
            return retVal;
        }

        public static string validateAsString(string stringVal, string key, bool mustNotBeNullOrWhitespace = true, string defaultVal = "")
        {
            string retVal = defaultVal;
            if (String.IsNullOrWhiteSpace(stringVal))
            {
                if (mustNotBeNullOrWhitespace)
                {
                    throw new Exception("(String)Field '" + key + "' in contained null or whitespace string, '" + stringVal + "'.");
                }
            }
            else
            {
                retVal = stringVal.Trim();
            }
            return retVal;
        }






        /// <summary>
        /// Pass in the parameter name, the form collection, whether or not we care if the item is in the collection, whether or not we care if the item found is null/whitespace, and the default value to return if something goes wrong.
        /// Throws exception if it must be in the collection and it's not; otherwise returns default value.
        /// Throws exception if it must be a valid value and it's not; otherwise returns default value.
        /// </summary>
        public static bool validateAsBool(FormCollection collection, string key, bool mustBeInCollection = true, bool mustBeValid = false, bool defaultVal = false)
        {
            string stringVal = null;
            try
            {
                stringVal = collection[key].Trim();
            }
            catch (IndexOutOfRangeException)
            {
                if (mustBeInCollection)
                {
                    throw new Exception("(BoolCol)Could not find Field '" + key + "' in form collection.");
                }
                else
                {
                    return defaultVal;
                }
            }
            catch (Exception ex)
            {
                //should not reach here.
                throw new Exception("(BoolCol)Encountered unexpected exception trying to retrieve Field '" + key + "' from form collection; " + ex.Message);
            }
            return validateAsBool(stringVal, key, mustBeValid, defaultVal);
        }

        public static bool validateAsBool(string stringVal, string key, bool mustBeValid = true, bool defaultVal = false)
        {
            bool retVal = defaultVal;
            try
            {
                retVal = Convert.ToBoolean(stringVal);
            }
            catch
            {
                if (mustBeValid)
                {
                    throw new Exception("(Bool)Could not parse bool value '" + stringVal + "' for collection key " + key + " to integer.");
                }
            }
            return retVal;
        }


    }





    public class RangeValueAttribute : ValidationAttribute
    {
        private readonly int _minValue;
        private readonly int _maxValue;

        public RangeValueAttribute(int minValue, int maxValue)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            if ((minValue > maxValue ))
            {
                minValue = maxValue;
            }
        }

        public override bool IsValid(object value)
        {
            return (int)value >= _minValue && (int) value <= _maxValue;
        }

        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }

    public class RangeWithExceptsAttribute : ValidationAttribute
    {
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly int[] _exceptions;

        public RangeWithExceptsAttribute(int minValue, int maxValue, int[] exceptions)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _exceptions = exceptions;
            if (!(minValue <= maxValue))
            {
                minValue = maxValue;
            }
        }

        public override bool IsValid(object value)
        {
            return ( (int)value >= _minValue && (int)value <= _maxValue ) || _exceptions.Contains((int)value);
        }

        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }


    public class MinValAttribute : ValidationAttribute
    {
        private readonly int _minValue;

        public MinValAttribute(int minValue)
        {
            _minValue = minValue;
        }

        public override bool IsValid(object value)
        {
            return (int)value >= _minValue;
        }

        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }

    public class MaxValAttribute : ValidationAttribute
    {
        private readonly int _maxValue;

        public MaxValAttribute(int minValue)
        {
            _maxValue = minValue;
        }

        public override bool IsValid(object value)
        {
            return (int)value >= _maxValue;
        }

        public override string FormatErrorMessage(string name)
        {
            return base.FormatErrorMessage(name);
        }
    }

    
}
