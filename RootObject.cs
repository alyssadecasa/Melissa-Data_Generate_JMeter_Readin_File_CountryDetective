/*
 * Generate txt file for JMeter load testing of CountryDetective web API
 * RootObject.cs
 * 
 * Object class that contains the keys and values of the JSON formatted
 * response from a REST GET call to the CountryDetective web API
 * 
 * @author Alyssa House
 */

namespace GenerateCSVforCountryDetectiveJMeterTesting
{
    class RootObject
    {
        public string country { get; set; }
    }
}
