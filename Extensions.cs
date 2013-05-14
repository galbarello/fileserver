using Gate;
using Nancy.Hosting.Owin;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FileServer
{
    public static class NancyExtensions
    {

        public static string RemoveDiacritics(this string s)
        {
            string normalizedString = s.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                Char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        } 

        public static IAppBuilder RunNancy(this IAppBuilder builder)
        {
            return builder.Run(Delegates.ToDelegate(new NancyOwinHost().ProcessRequest));
        }

        public static int GenerateSequence(this string name)
        { 
            return name.GetHashCode()>0? name.GetHashCode(): name.GetHashCode()*-1;
        }

        public static int GetMonth(this string archivo)
        {
            var list = archivo
                .Replace(".xlsx", "")
                .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .AsQueryable<string>();
            var fecha = DateTime.ParseExact(list.ElementAtOrDefault(list.Count() - 2), "MMMM", CultureInfo.CurrentCulture).Month;
            
            return fecha;
        }

        public static int GetYear(this string archivo)
        {
            return int.Parse(archivo
                    .Replace(".xlsx","")
                    .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                    .AsQueryable<string>()
                    .Last());
            
        }

        public static IList<string> Responsable(this string nombre)
        {
            var processed = new List<string>();            

            var grupoSupervision = nombre.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            var r = new Regex("([A-Z]+[a-z]+)");
            

            foreach (var word in grupoSupervision)
            {
                var list = r.Replace(word, m => (m.Value.Length > 3 ? m.Value : m.Value.ToLower()) + " ").Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                processed.AddRange(list); 
            }
            return processed;
        }
    }
}
