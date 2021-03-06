﻿using System;
using System.IO;
using DissertationOriginal.Classes;
using DissertationOriginal.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace DissertationOriginal.Tests.Classes
{
    [TestClass()]
    public class UtilitiesTests
    {
        [TestMethod()]
        public void RemoveLast3Cahracters()
        {
            const string test = "Hello World@en";
            const string expected = "Hello World";

            string output = Utilities.RemoveLast3Cahracters(test);

            Assert.AreEqual(expected, output);
        }

        [TestMethod()]
        public void DateCreatorTest()
        {
            const string test = "08/06/1996^^Date";
            const string expected = "08/06/1996";

            string output = Utilities.DateCreator(test);

            Assert.AreEqual(expected, output);
        }

        [TestMethod()]
        public void ExtractLuisDataTest()
        {
            string limit = $"LIMIT({10})";
            string genreMatch = $"FILTER ( regex (str(?genre), '{"crime"}', 'i'))";
            string dateMatch = String.Format("FILTER ((?releaseDate >= '{0}-01-01'^^xsd:date) && (?releaseDate < '{0}-12-31'^^xsd:date))", 2012);

            string queryPattern =
                "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#> " +
                "PREFIX db: <http://dbpedia.org/ontology/> " +
                "PREFIX prop: <http://dbpedia.org/property/> " +
                "SELECT ?movieLink ?title ?genreLink ?genre ?releaseDate " +
                "WHERE {{ " +
                    "?movieLink rdf:type db:Film; " +
                               "foaf:name ?title. " +
                    "OPTIONAL {{ ?movieLink prop:genre ?genreLink. " +
                               "?genreLink rdfs:label ?genre. " +
                               "FILTER(lang(?genre) = 'en') }}. " +
                    "OPTIONAL {{ ?movieLink <http://dbpedia.org/ontology/releaseDate> ?releaseDate }}. " +

                    "{0}" +
                    "{1}" +
                    "FILTER(lang(?title) = 'en') " +
                "}}" +
                "ORDER BY DESC(?releaseDate)" +
                "{2}";
            string expected = String.Format(queryPattern, genreMatch, dateMatch, limit);

            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var file = $"{directory}\\Classes\\TestItems\\test2LuisData.json";

            LuisJsonModel data = JsonConvert.DeserializeObject<LuisJsonModel>(File.ReadAllText(file));

            string output = Utilities.ExtractLuisData(data);

            Assert.AreEqual(expected, output);
        }
    }
}