#pragma warning disable SA1516 // Elements should be separated by blank line
using System;
using System.IO;
using System.Linq;
using System.Text;

var ignoreArray = new[]
{
    @"layout\activity_main.xml",
    @"navigation\mobile_navigation.xml",
};

var ignoreBoundLayoutArray = new[]
{
    @"layout\activity_guide_ca_cert.xml",
    @"layout\activity_toolbar_webview.xml",
};

const string Mark = "<!--ST.Tools.AndroidResourceLink-->";
var resPath = ProjectPathUtil.projPath + Path.DirectorySeparatorChar + "src" + Path.DirectorySeparatorChar + string.Join(Path.DirectorySeparatorChar, new[] { "ST.Client.Mobile.Droid.Design", "ui", "src", "main", "res" });
var androidProjPath = ProjectPathUtil.projPath + Path.DirectorySeparatorChar + "src" + Path.DirectorySeparatorChar + "ST.Client.Android";
var androidProjFilePath = androidProjPath + Path.DirectorySeparatorChar + "ST.Client.Android.csproj";

var csprojContent = File.ReadAllText(androidProjFilePath);
var csprojContentLines = csprojContent.Split(Environment.NewLine);
var index = Array.FindIndex(csprojContentLines, x => x.Trim() == Mark);
if (index < 0)
{
    Console.WriteLine("Cannot find mark.");
    Console.ReadKey();
    return;
}
var top = csprojContentLines.Take(index).ToArray();
var bottom = csprojContentLines.Skip(index).ToArray();
index = Array.FindIndex(bottom, x => x.Trim() == "</ItemGroup>");
bottom = bottom.Skip(index + 1).ToArray();

var files = Directory.GetDirectories(resPath).Select(Directory.GetFiles).SelectMany(x => x).Where(x => !x.Contains("__dont_link")).ToArray();
var sb = new StringBuilder();
Array.ForEach(top, x => sb.AppendLine(x));
sb.Append("  ").AppendLine(Mark).AppendLine("  <ItemGroup>");
foreach (var file in files)
{
    var include = Path.GetRelativePath(androidProjPath, file);
    var link = Path.GetRelativePath(androidProjPath, resPath);
    link = "Resources" + include[link.Length..];
    var isContinue = false;
    foreach (var ignoreItem in ignoreArray)
    {
        if (link.EndsWith(ignoreItem, StringComparison.OrdinalIgnoreCase))
        {
            isContinue = true;
            break;
        }
    }
    if (isContinue) continue;
    var tag = IsBoundLayout(link) ? "AndroidBoundLayout" : "AndroidResource";
    sb.AppendFormat("    <{1} Include=\"{0}\">", include, tag).AppendLine()
        .AppendFormat("      <Link>{0}</Link>", link).AppendLine()
        .AppendFormat("    </{0}>", tag).AppendLine();
}
sb.AppendLine("  </ItemGroup>");
Array.ForEach(bottom, x => sb.AppendLine(x));
csprojContent = sb.ToString().Trim();
File.WriteAllText(androidProjFilePath, csprojContent);

bool IsBoundLayout(string link)
{
    foreach (var item in ignoreBoundLayoutArray)
    {
        if (link.EndsWith(item)) return false;
    }
    return link.Contains("\\layout") && /*!link.Contains("\\shared_") && !link.Contains("_content.xml") &&*/ !link.Contains("_not_binding.xml");
}

Console.WriteLine("OK");
#pragma warning restore SA1516 // Elements should be separated by blank line