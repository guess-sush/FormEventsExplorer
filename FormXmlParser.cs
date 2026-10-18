using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FormEventExplorerXrmToolBox
{
    internal static class FormXmlParser
    {
        public static List<EventRow> Parse(string xml)
        {
            var output = new List<EventRow>();
            if (string.IsNullOrWhiteSpace(xml)) return output;
            var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

            var libraries = doc.Descendants()
                .Where(x => Eq(x.Name.LocalName, "Library"))
                .Select(x => new
                {
                    Id = NormaliseGuid(Attr(x, "libraryUniqueId", "libraryuniqueid")),
                    Name = Attr(x, "name")
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g => g.First().Name, StringComparer.OrdinalIgnoreCase);

            foreach (var ev in doc.Descendants().Where(x => Eq(x.Name.LocalName, "event")))
            {
                var rawName = Attr(ev, "name");
                if (string.IsNullOrWhiteSpace(rawName)) continue;
                var eventName = CanonicalEvent(rawName);
                if (eventName == null) continue;

                var field = Attr(ev, "attribute", "field", "datafieldname");
                var control = string.Empty;
                if (eventName == "OnChange")
                {
                    var owner = ev.Ancestors().FirstOrDefault(x =>
                        Eq(x.Name.LocalName, "control") || Eq(x.Name.LocalName, "attribute"));
                    if (owner != null && Eq(owner.Name.LocalName, "control"))
                    {
                        control = Attr(owner, "id", "name");
                        if (string.IsNullOrWhiteSpace(field)) field = Attr(owner, "datafieldname");
                    }
                    else if (owner != null && string.IsNullOrWhiteSpace(field))
                    {
                        field = Attr(owner, "name", "id");
                    }
                    if (string.IsNullOrWhiteSpace(field)) field = "Unknown field";
                }

                foreach (var handler in ev.Descendants().Where(x => Eq(x.Name.LocalName, "Handler")))
                {
                    var libraryToken = Attr(handler, "libraryUniqueId", "libraryuniqueid", "libraryName", "libraryname");
                    var key = NormaliseGuid(libraryToken);
                    var library = libraries.TryGetValue(key, out var mapped) ? mapped : libraryToken;
                    output.Add(new EventRow
                    {
                        Event = eventName,
                        Field = eventName == "OnChange" ? field : "Form",
                        Control = control,
                        Function = Attr(handler, "functionName", "functionname"),
                        Library = string.IsNullOrWhiteSpace(library) ? "Not specified" : library,
                        Enabled = !string.Equals(Attr(handler, "enabled"), "false", StringComparison.OrdinalIgnoreCase),
                        PassContext = string.Equals(Attr(handler, "passExecutionContext", "passexecutioncontext"), "true", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }

            return output.GroupBy(x => string.Join("|", x.Event, x.Field, x.Control, x.Function, x.Library), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First()).OrderBy(x => x.Event).ThenBy(x => x.Field).ThenBy(x => x.Function).ToList();
        }

        private static string CanonicalEvent(string value)
        {
            if (Eq(value, "onload")) return "OnLoad";
            if (Eq(value, "onsave")) return "OnSave";
            if (Eq(value, "onchange")) return "OnChange";
            return null;
        }
        private static string Attr(XElement e, params string[] names)
        {
            foreach (var name in names)
            {
                var a = e.Attributes().FirstOrDefault(x => Eq(x.Name.LocalName, name));
                if (a != null) return a.Value;
            }
            return string.Empty;
        }
        private static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        private static string NormaliseGuid(string value) => (value ?? string.Empty).Trim().Trim('{', '}').ToLowerInvariant();
    }
}
