using BraveHaxvius;
using BraveHaxvius.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DataExtractor
{
    class Program
    {
        static Dictionary<String, object> fileJson = JsonConvert.DeserializeObject<Dictionary<String, object>>(Encoding.ASCII.GetString(Properties.Resources.files));
        static Dictionary<String, object> responseJson = JsonConvert.DeserializeObject<Dictionary<String, object>>(Encoding.ASCII.GetString(Properties.Resources.response));
        static Dictionary<String, object> networkJson = JsonConvert.DeserializeObject<Dictionary<String, object>>(Encoding.ASCII.GetString(Properties.Resources.network));
        static Dictionary<String, object> variableJson = JsonConvert.DeserializeObject<Dictionary<String, object>>(Encoding.ASCII.GetString(Properties.Resources.variables));
        public class Mst
        {
            public String TypeName { get; set; }
            public String Id { get; set; }
            public String English { get; set; }
        }
        static WebClient Downloader = new WebClient();
        static string fileHeader = @"using System;
using System.Collections.Generic;

namespace BraveHaxvius.Data
{
    public class CLASSNAME
    {";
        static string fileMiddle = @"
		public static readonly List<CLASSNAME> CLASSNAMEs = new List<CLASSNAME>
		{";
        static string fileFooter = @"		};
    }
}";
        static Regex varRegEx = new Regex(@"([a-zA-Z]+[a-zA-Z0-9_]*)");
        static List<String> DecodeFile(JToken rawFile)
        {

            var name = rawFile[Variable.KeyName].ToString();
            var version = rawFile[Variable.Value].ToString();
            var loc = "mst";
            if (name.Contains("F_TEXT"))
                loc = "localized_texts/en";
            var nameKey = (JToken)fileJson.FirstOrDefault(r => r.Key == name).Value;
            var datContent = Downloader.DownloadString("http://lapis-dlc.gumi.sg/dlc_assets_prod/" + loc + "/Ver" + version + "_" + nameKey["Name"] + ".dat");
            var datLines = datContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var entireFile = new List<String>();
            var decryptChosen = false;
            foreach (var base64 in datLines)
            {
                var decryptedMst = "";
                try
                {
                    if (decryptChosen is true)
                        throw new Exception();
                    decryptedMst = Crypto.Decrypt(base64, nameKey["Key"].ToString());
                    if (decryptedMst.All(c => c > 31 || c < 14) is false)
                        throw new Exception();
                }
                catch (Exception)
                {
                    decryptChosen = true;
                    decryptedMst = Crypto.OldDecrypt(base64, nameKey["Key"].ToString());
                }
                var decryptedEntries = decryptedMst.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                entireFile.AddRange(decryptedEntries);
            }
            return entireFile;
        }
        static List<String> DecodeFile(String name, String version)
        {

            var loc = "mst";
            if (name.Contains("F_TEXT"))
                loc = "localized_texts/en";
            var nameKey = (JToken)fileJson.FirstOrDefault(r => r.Key == name).Value;
            var datContent = "";
            try { datContent = Downloader.DownloadString("http://lapis-dlc.gumi.sg/dlc_assets_prod/" + loc + "/Ver" + version + "_" + nameKey["Name"] + ".dat"); }
            catch { return DecodeFile(name, (int.Parse(version) + 1).ToString()); }
            var datLines = datContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var entireFile = new List<String>();
            var decryptChosen = false;
            foreach (var base64 in datLines)
            {
                var decryptedMst = "";
                try
                {
                    if (decryptChosen is true)
                        throw new Exception();
                    decryptedMst = Crypto.Decrypt(base64, nameKey["Key"].ToString());
                    if (decryptedMst.All(c => c > 31 || c < 14) is false)
                        throw new Exception();
                }
                catch (Exception)
                {
                    decryptChosen = true;
                    decryptedMst = Crypto.OldDecrypt(base64, nameKey["Key"].ToString());
                }
                var decryptedEntries = decryptedMst.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                entireFile.AddRange(decryptedEntries);                
            }           
            
            return entireFile;
        }
        static void DecodeOne(string name, string version)
        {
            version = (int.Parse(version) + 1).ToString();
            var loc = "mst";
            if (name.Contains("F_TEXT"))
                loc = "localized_texts/en";
            var nameKey = (JToken)fileJson.FirstOrDefault(r => r.Key == name).Value;
            var datContent = Downloader.DownloadString("http://lapis-dlc.gumi.sg/dlc_assets_prod/" + loc + "/Ver" + version + "_" + nameKey["Name"] + ".dat");
            var datLines = datContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var entireFile = new List<String>();
            var decryptChosen = false;
            foreach (var base64 in datLines)
            {
                var decryptedMst = "";
                try
                {
                    if (decryptChosen is true)
                        throw new Exception();
                    decryptedMst = Crypto.Decrypt(base64, nameKey["Key"].ToString());
                    if (decryptedMst.All(c => c > 31 || c < 14) is false)
                        throw new Exception();
                }
                catch (Exception)
                {
                    decryptChosen = true;
                    decryptedMst = Crypto.OldDecrypt(base64, nameKey["Key"].ToString());
                }
                var decryptedEntries = decryptedMst.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                entireFile.AddRange(decryptedEntries);
            }
        File.WriteAllLines("..\\mst\\" + name + "_" + version, entireFile);
            if (loc == "mst")
            {
                StringBuilder sb = new StringBuilder();
                var keys = (JObject)JsonConvert.DeserializeObject(entireFile.First());
                foreach (var key in keys)
                {
                    if (Variable.Variables.ContainsKey(key.Key))
                        sb.Append(Variable.Variables.FirstOrDefault(k => k.Key == key.Key).Value + " (" + key.Key + "),");
                    else
                        sb.Append(key.Key + ",");
                }
                sb.AppendLine();
                foreach (var line in entireFile)
                {
                    var obj = (JObject)JsonConvert.DeserializeObject(line);
                    foreach (var o in obj)
                    {
                        if (o.Key == Variable.Description)
                        {
                            try { sb.Append(Unit.Units.First(m => m.UnitId == obj[Variable.UnitId].ToString()).Name.Replace(",", "|") + ","); } catch { sb.Append(o.Value + ","); }
                        }
                        else
                            sb.Append(o.Value.ToString().Replace(",", "|") + ",");
                    }
                    sb.AppendLine();
                }
                File.WriteAllText("..\\mst\\" + name + ".csv", sb.ToString());
            }
        }
        static void DecodeAll()
        {
            var bot = new BraveExvius
            {
                MstVersion = "0"
            };
            var init = bot.Network.SendPacket(Request.Initialize);
            var newMsts = init[GameObject.VersionInfo];
            var F_LOCALIZED_TEXT_MST = DecodeFile(newMsts.First(m => m[Variable.KeyName].ToString() == "F_LOCALIZED_TEXT_MST"));
            F_LOCALIZED_TEXT_MST.ForEach(l =>
            {
                newMsts.Last.AddAfterSelf(new JObject(
                new JProperty(Variable.KeyName, ((JObject)JsonConvert.DeserializeObject(l))[Variable.KeyName].ToString()),
                new JProperty(Variable.Value, ((JObject)JsonConvert.DeserializeObject(l))["00zAYGYS"].ToString())));
            });
            foreach (var mst in newMsts)
            {
                var name = mst[Variable.KeyName].ToString();
                Logger.Out(name);
                //if (!name.Contains("ANIMATION"))
                //    continue;
                var version = mst[Variable.Value].ToString();
                //version = (int.Parse(version) + 1).ToString();
                var loc = "mst";
                if (name.Contains("F_TEXT"))
                    loc = "localized_texts/en";
                var nameKey = (JToken)fileJson.FirstOrDefault(r => r.Key == name).Value;
                if (nameKey == null)
                {
                    Logger.Out(name + " Failed");
                    continue;
                    /*
                    nameKey = (JToken)fileJson.First().Value;
                    nameKey["Name"] = "6DDX7Kbk";
                    nameKey["Key"] = "TPWnfEAt";*/

                }
                try
                {
                    var datContent = Downloader.DownloadString("http://lapis-dlc.gumi.sg/dlc_assets_prod/" + loc + "/Ver" + version + "_" + nameKey["Name"] + ".dat");
                    var datLines = datContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var entireFile = new List<String>();
                    var decryptChosen = false;
                    foreach (var base64 in datLines)
                    {
                        var decryptedMst = "";
                        try
                        {
                            if (decryptChosen is true)
                                throw new Exception();
                            decryptedMst = Crypto.Decrypt(base64, nameKey["Key"].ToString());
                            if (decryptedMst.All(c => c > 31 || c < 14) is false)
                                throw new Exception();
                        }
                        catch (Exception)
                        {
                            decryptChosen = true;
                            decryptedMst = Crypto.OldDecrypt(base64, nameKey["Key"].ToString());
                        }
                        var decryptedEntries = decryptedMst.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        entireFile.AddRange(decryptedEntries);
                    }

                File.WriteAllLines("..\\mst\\" + name + "_" + version, entireFile);
                    if (loc == "mst33")
                    {
                        StringBuilder sb = new StringBuilder();
                        var keys = (JObject)JsonConvert.DeserializeObject(entireFile.First());
                        foreach (var key in keys)
                        {
                            if (Variable.Variables.ContainsKey(key.Key))
                                sb.Append(Variable.Variables.FirstOrDefault(k => k.Key == key.Key).Value + " (" + key.Key + "),");
                            else
                                sb.Append(key.Key + ",");
                        }
                        sb.AppendLine();
                        foreach (var line in entireFile)
                        {
                            var obj = (JObject)JsonConvert.DeserializeObject(line);
                            foreach (var o in obj)
                            {
                                if (o.Key == Variable.Description)
                                {
                                    try { sb.Append(Unit.Units.First(m => m.UnitId == obj[Variable.UnitId].ToString()).Name.Replace(",", "|") + ","); } catch { sb.Append(o.Value + ","); }
                                }
                                else
                                    sb.Append(o.Value.ToString().Replace(",", "|") + ",");
                            }
                            sb.AppendLine();
                        }
                        File.WriteAllText("..\\mst\\" + name + "_" + version + ".csv", sb.ToString());
                    }
                }
                catch { Logger.Out(name + " Failed"); }
            }
        }
        static void DumpHardcoded(String className, Dictionary<String, object> json, List<String> members = null)
        {

            var translations = new List<String>();
            var definitions = new StringBuilder();
            var definitionsList = new StringBuilder();
            var addedUnitIds = new List<String>();
            var classVars = "";
            var fileMiddleFixed = "";
            if (members != null)
            {
                fileMiddleFixed = fileMiddle.Replace("CLASSNAME", className);
                classVars = Environment.NewLine + "        public String Name { get; set; }" + Environment.NewLine;
                foreach (var member in members)
                    classVars += "        public String " + member + " { get; set; }" + Environment.NewLine;

                foreach (var obj in json)
                {
                    definitions.Append("        public static readonly " + className + " " + obj.Key + " = new " + className + " { ");
                    List<String> initClass = new List<string> { "Name = \"" + obj.Key + "\"" };
                    foreach (var member in members)
                        initClass.Add(member + " = \"" + ((JToken)obj.Value)[member].ToString() + "\"");
                    definitions.Append(String.Join(", ", initClass));
                    definitions.AppendLine("};");
                    definitionsList.AppendLine($"           {obj.Key},");
                }
            }
            else
            {
                fileMiddleFixed = fileMiddle.Replace("List<CLASSNAME>", "Dictionary<String, String>").Replace("CLASSNAME", className);
                foreach (var obj in json)
                {
                    var objectName = obj.Value;
                    if (json.Count(o => o.Value.ToString() == obj.Value.ToString()) > 1)
                        objectName += "_" + obj.Key;
                    definitions.AppendLine($"        public static readonly String {objectName} = \"{obj.Key}\";");
                    definitionsList.AppendLine($"            {{{objectName}, \"{objectName}\"}},");
                }
            }
            var file = new StringBuilder();
            file.Append(fileHeader.Replace("CLASSNAME", className));
            file.AppendLine(classVars);
            file.Append(definitions.ToString());
            file.AppendLine(fileMiddleFixed);
            file.Append(definitionsList.ToString());
            file.Append(fileFooter);
            File.WriteAllText(@"..\BraveHaxvius\Data\" + className + ".cs", file.ToString());
        }
        static void DumpTranslation(JToken msts, String translation, String mst, String className, Dictionary<String, String> members = null)
        {
            var translations = DecodeFile(msts.First(m => m[Variable.KeyName].ToString() == translation));
            var json = DecodeFile(msts.First(m => m[Variable.KeyName].ToString() == mst));
            DumpTranslation(translations, json, translation, mst, className, members);

        }


        static void DumpTranslation(String translationVersion, String jsonVersion, String translation, String mst, String className, Dictionary<String, String> members = null)
        {
            var translations = DecodeFile(translation, translationVersion);
            var json = DecodeFile(mst, jsonVersion);
            DumpTranslation(translations, json, translation, mst, className, members);
        }

        static void DumpTranslation(List<String> translations, List<String> json, String translation, String mst, String className, Dictionary<String, String> members = null)
        {

            string updateFile = @"..\BraveHaxvius\Data\" + className + ".cs";
            string text = "";
            string versiontoUpdate = "";
            bool IsUpdateting = false;
            if (File.Exists(updateFile))
            {
                text = System.IO.File.ReadAllText(updateFile);
                IsUpdateting = text.Contains("---VersionControl");
            }

            if (IsUpdateting)
                versiontoUpdate = text.Split(new string[] { "---VersionControl" }, StringSplitOptions.None)[0].Remove(0, 2);



            if (members == null)
                members = new Dictionary<string, string>();
            var mstList = json.Select(m => (JObject)JsonConvert.DeserializeObject(m)).ToList();
            var definitions = new StringBuilder();
            var definitionsList = new StringBuilder();
            var classVars = new StringBuilder();
            classVars.Append($"{Environment.NewLine}        public String Name {{ get; set; }} {Environment.NewLine}");
            //classVars += "        public String Id { get; set; }" + Environment.NewLine;
            var variables = mstList.FirstOrDefault();
            var idKey = "";
	    var idvalue = "";
            foreach (var v in variables)
            {
                var variable = Variable.Variables.FirstOrDefault(v2 => v2.Key == v.Key);
                if (!variable.Equals(default(KeyValuePair<String, String>)))
                {
                    members.Add(variable.Value, variable.Key);
                    if (idKey == "" && translation.Replace("_", "").ToLower().Contains(variable.Value.ToLower().Replace("id", "").Replace("_", "")))
                         {
                        idKey = variable.Key;
                        idvalue = variable.Value;
                    }
                }
            }
            foreach (var member in members)
                classVars.Append($"        public String { member.Key } {{ get; set; }} { Environment.NewLine}");
            var lastTranslation = translations.Last();
            translations.RemoveRange(0, translations.IndexOf(versiontoUpdate) + 1);
            var t = IsUpdateting ? "Updating" : "Building";
            Logger.Out($"{t} {className}.cs,  Adding {translations.Count}");
            if (translations.Count < 1)
                return;
            //var tmpList = JsonConvert.SerializeObject(Type.GetType($"BraveHaxvius.Data.{className}, BraveHaxvius")?.GetField($"{className}s")?.GetValue(null) ?? "").ToLower() ;
            foreach (var line in translations)
            {
                var id = line.Split(new char[1] { '^' })[0].Split(new char[1] { '_' }).Last();
                if (text.ToLower().Contains($"{className.ToLower()}id\":\"{id}") == true)
			continue;
                var name = line.Split(new char[1] { '^' })[1]?.Replace("\"", "");
                //Console.WriteLine(translations.IndexOf(line));
                var varName = new StringBuilder();
                varName.Append(String.Join("", varRegEx.Matches(name).Cast<Match>().Select(o => o.Value.ToString())));
                var count = translations.Count(m => String.Join("", varRegEx.Matches(m.Split(new char[1] { '^' })[1]).Cast<Match>().Select(o => o.Value.ToString())) == varName.ToString());
                if (IsUpdateting || count > 1)
                    varName.Append($"_{id}" + (IsUpdateting ? translations.IndexOf(line).ToString() : ""));
                if (definitions.ToString().Contains($" readonly { className } { varName } = new"))
                    continue;
                definitions.Append($"        public static readonly { className } { varName } = new { className } {{ ");
                List<String> initClass = new List<string>
                {
                    "Name = \"" + name + "\""
                };
		initClass.Add(idvalue + " = \"" + id + "\"");
                members.Remove(idvalue);
                //initClass.Add("Id = \"" + id + "\"");
                foreach (var member in members)
                {
                    if (idKey == "")
                        continue;
                    var value = (mstList.FirstOrDefault(m => m[idKey].ToString() == id))?[member.Value]?.ToString()?.Replace("\"", "");
                    if (value != null)
                        initClass.Add($"{ member.Key } = \"{ value }\"");
                }
                definitions.Append(String.Join(", ", initClass));
                definitions.AppendLine("};");
                definitionsList.AppendLine($"           {varName},");
            }
            var file = new StringBuilder();
            if (!IsUpdateting)
            {
                file.Append("//" + lastTranslation + "---VersionControl" + Environment.NewLine + fileHeader.Replace("CLASSNAME", className));
                file.AppendLine(classVars.ToString());
                file.Append(definitions.ToString() + "/*VersionControldefinitions*/" + Environment.NewLine);
                file.AppendLine(fileMiddle.Replace("CLASSNAME", className));
                file.Append(definitionsList.ToString() + "/*VersionControldefinitionsList*/" + Environment.NewLine);
                file.Append(fileFooter);
                File.WriteAllText(@"..\BraveHaxvius\Data\" + className + ".cs", file.ToString());
            }
            else
            {
                text = text.Replace(versiontoUpdate, lastTranslation);
                text = text.Replace(@"/*VersionControldefinitions*/", definitions.ToString() + "/*VersionControldefinitions*/" + Environment.NewLine);
                text = text.Replace(@"/*VersionControldefinitionsList*/", definitionsList.ToString() + "/*VersionControldefinitionsList*/" + Environment.NewLine);
                File.WriteAllText(@"..\BraveHaxvius\Data\" + className + ".cs", text);
            }

        }

        static void Main(string[] args)
        {
            /*DumpTranslation("87", "65", "F_TEXT_UNITS_NAME", "F_UNIT_MST", "Unit", new Dictionary<String, String>  {
                   { "UniqueUnitId", "" },
                   { "Level", "" },
                   { "Tmr", "" },
                   { "Equip", "" } });*/
            /*DumpTranslation("108", "58", "F_TEXT_ITEM_EQUIP_NAME", "F_EQUIP_ITEM_MST", "Equipment");
            DumpTranslation("88", "58", "F_TEXT_ITEM_NAME", "F_ITEM_MST", "Item");
            DumpTranslation("87", "44", "F_TEXT_MATERIA_NAME", "F_MATERIA_MST", "Materia");
            for (int i = 61; i < 150; i++)
            { try { DecodeOne("F_UNIT_MST", i.ToString()); } catch { } }
            for (int i = 80; i < 150; i++)
            { try { DecodeOne("F_TEXT_UNITS_NAME", i.ToString()); } catch { } }

            for (int i = 48; i < 150; i++)
            { try { DecodeOne("F_ITEM_MST", i.ToString()); } catch { } }
            for (int i = 81; i < 150; i++)
            { try { DecodeOne("F_TEXT_ITEM_NAME", i.ToString()); } catch { } }

            for (int i = 57; i < 150; i++)
            { try { DecodeOne("F_EQUIP_ITEM_MST", i.ToString()); } catch { } }
            for (int i = 100; i < 150; i++)
            { try { DecodeOne("F_TEXT_ITEM_EQUIP_NAME", i.ToString()); } catch { } }

            for (int i = 41; i < 150; i++)
            { try { DecodeOne("F_MATERIA_MST", i.ToString()); } catch { } }
            for (int i = 80; i < 150; i++)
            { try { DecodeOne("F_TEXT_MATERIA_NAME", i.ToString()); } catch { } }*/
            //DecodeAll();
            DumpHardcoded("Request", networkJson, new List<String> { "Url", "EncodeKey", "RequestID" });
            DumpHardcoded("GameObject", responseJson);
            DumpHardcoded("Variable", variableJson);

            var bot = new BraveExvius
            {
                MstVersion = "0",
                AppVersion = "99999"
            };
            var init = bot.Network.SendPacket(Request.Initialize);
            var newMsts = init[GameObject.VersionInfo];
            var F_LOCALIZED_TEXT_MST = DecodeFile(newMsts.First(m => m[Variable.KeyName].ToString() == "F_LOCALIZED_TEXT_MST"));
            F_LOCALIZED_TEXT_MST.ForEach(l =>
            {
                newMsts.Last.AddAfterSelf(new JObject(
                new JProperty(Variable.KeyName, ((JObject)JsonConvert.DeserializeObject(l))[Variable.KeyName].ToString()),
                new JProperty(Variable.Value, ((JObject)JsonConvert.DeserializeObject(l))["00zAYGYS"].ToString())));
            });
            {
                var switchJson = DecodeFile(newMsts.First(m => m[Variable.KeyName].ToString() == "F_SUBLIMATION_RECIPE_MST"));
                var switchMst = switchJson.Select(m => (JObject)JsonConvert.DeserializeObject(m)).ToList();
                var definitions = new StringBuilder();
                var definitionsList = new StringBuilder();
                var className = "Sublimation";
                var classVars = @"
        public String SublimationId { get; set; }
        public String UnitId { get; set; }
        public String Items { get; set; }
";
                foreach (var switchObj in switchMst)
                {
                    var id = switchObj[Variable.SublimationId].ToString();
                    var unitId = switchObj[Variable.SublimationUnitId].ToString();
                    var items = switchObj[Variable.SublimationItems].ToString();
                    var varName = "Sublimation_" + id;
                    definitions.AppendLine($"        public static readonly {className} {varName} = new {className} {{ SublimationId = \"{id}\", UnitId = \"{unitId}\", Items = \"{items}\"}};");
                    definitionsList.AppendLine($"           {varName},");
                }
                var file = new StringBuilder();
                file.Append(fileHeader.Replace("CLASSNAME", className));
                file.AppendLine(classVars);
                file.Append(definitions.ToString());
                file.AppendLine(fileMiddle.Replace("CLASSNAME", className));
                file.Append(definitionsList.ToString());
                file.Append(fileFooter);
                File.WriteAllText(@"..\BraveHaxvius\Data\" + className + ".cs", file.ToString());
            }
            DumpTranslation(newMsts, "F_TEXT_ITEM_EQUIP_NAME", "F_EQUIP_ITEM_MST", "Equipment");
            DumpTranslation(newMsts, "F_TEXT_UNITS_NAME", "F_UNIT_MST", "Unit", new Dictionary<String, String>  {
                { "UniqueUnitId", "" },
                { "Level", "" },
                { "Tmr", "" },
                { "Equip", "" } });
			DumpTranslation(newMsts, "F_TEXT_VISION_CARD_NAME", "F_VISION_CARD_MST", "VisionCard");
            DumpTranslation(newMsts, "F_TEXT_MISSION", "F_MISSION_MST", "Mission");
            DumpTranslation(newMsts, "F_TEXT_IMPORTANT_ITEM_NAME", "F_IMPORTANT_ITEM_MST", "ImportantItem");
            DumpTranslation(newMsts, "F_TEXT_IMPORTANT_ITEM_NAME", "F_TICKET_MST", "Ticket");
            DumpTranslation(newMsts, "F_TEXT_ITEM_NAME", "F_ITEM_MST", "Item");
            DumpTranslation(newMsts, "F_TEXT_RECIPE_BOOK_NAME", "F_RECIPE_BOOK_MST", "Recipe");
            DumpTranslation(newMsts, "F_TEXT_MATERIA_NAME", "F_MATERIA_MST", "Materia");
            DumpTranslation(newMsts, "F_TEXT_CHALLENGE_NAME", "F_CHALLENGE_MST", "Challenge");
            DumpTranslation(newMsts, "F_TEXT_ABILITY_NAME", "F_ABILITY_MST", "Ability");
            DumpTranslation(newMsts, "F_TEXT_BEAST_NAME", "F_BEAST_MST", "Beast");
            DumpTranslation(newMsts, "F_TEXT_SHOP", "F_SHOP_MST", "Shop");
            {
                var translations = DecodeFile(newMsts.First(m => m[Variable.KeyName].ToString() == "F_TEXT_TEXT_EN"));
                var definitions = new StringBuilder();
                var definitionsList = new StringBuilder();
                var className = "Text";
                var names = new List<String>();
                foreach (var line in translations)
                {
                    var name = line.Split(new char[1] { '^' })[0];
                    var message = line.Split(new char[1] { '^' })[1].Replace("\\", "\\\\").Replace("\"", "\\\"");
                    if (names.Contains(name))
                    {
                        name += message + "_";
                    }
                    names.Add(name);
                    definitionsList.AppendLine($"           {{\"{name}\", \"{message}\"}},");
                }
                var file = new StringBuilder();
                file.Append(fileHeader.Replace("CLASSNAME", className));
                file.AppendLine(fileMiddle.Replace("List<CLASSNAME>", "Dictionary<String, String>").Replace("CLASSNAME", className));
                file.Append(definitionsList.ToString());
                file.Append(fileFooter);
                File.WriteAllText(@"..\BraveHaxvius\Data\" + className + ".cs", file.ToString());
            }
            {
                var switchJson = DecodeFile(newMsts.First(m => m[Variable.KeyName].ToString() == "F_SWITCH_MST"));
                var switchMst = switchJson.Select(m => (JObject)JsonConvert.DeserializeObject(m)).ToList();
                var definitions = new StringBuilder();
                var definitionsList = new StringBuilder();
                var className = "Switch";
                var classVars = @"
        public String SwitchId { get; set; }
        public String SwitchType { get; set; }
        public String SwitchBit { get; set; }
";
                foreach (var switchObj in switchMst)
                {
                    var id = switchObj["8vAYn3IX"].ToString();
                    var type = switchObj[Variable.SwitchType].ToString();
                    var bit = switchObj[Variable.BitNumber].ToString();
                    var varName = "Switch_" + id;
                    definitions.AppendLine($"        public static readonly {className} {varName} = new {className} {{ SwitchId = \"{id}\", SwitchType = \"{type}\", SwitchBit = \"{bit}\"}};");
                    definitionsList.AppendLine($"           {varName},");
                }
                var file = new StringBuilder();
                file.Append(fileHeader.Replace("CLASSNAME", className));
                file.AppendLine(classVars);
                file.Append(definitions.ToString());
                file.AppendLine(fileMiddle.Replace("CLASSNAME", className));
                file.Append(definitionsList.ToString());
                file.Append(fileFooter);
                File.WriteAllText(@"..\BraveHaxvius\Data\" + className + ".cs", file.ToString());
            }
            {
                var switchJson = DecodeFile(newMsts.First(m => m[Variable.KeyName].ToString() == "F_UNIT_CLASS_UP_MST"));
                var switchMst = switchJson.Select(m => (JObject)JsonConvert.DeserializeObject(m)).ToList();
                var definitions = new StringBuilder();
                var definitionsList = new StringBuilder();
                var className = "ClassUp";
                var classVars = @"
        public String UnitId { get; set; }
        public String Items { get; set; }
";
                foreach (var switchObj in switchMst)
                {
                    var id = switchObj[Variable.UnitId].ToString();
                    var items = switchObj[Variable.ClassUpItems].ToString();
                    var varName = "Unit_" + id;
                    definitions.AppendLine($"        public static readonly {className} {varName} = new {className} {{ UnitId = \"{id}\", Items = \"{items}\"}};");
                    definitionsList.AppendLine($"           {varName},");
                }
                var file = new StringBuilder();
                file.Append(fileHeader.Replace("CLASSNAME", className));
                file.AppendLine(classVars);
                file.Append(definitions.ToString());
                file.AppendLine(fileMiddle.Replace("CLASSNAME", className));
                file.Append(definitionsList.ToString());
                file.Append(fileFooter);
                File.WriteAllText(@"..\BraveHaxvius\Data\" + className + ".cs", file.ToString());
            }
        }
    }
}
