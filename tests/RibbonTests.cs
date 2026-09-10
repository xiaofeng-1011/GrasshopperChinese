using System;
using System.Reflection;
using System.Runtime.Serialization;
using Grasshopper.GUI.Ribbon;
using GrasshopperChinese;

class RibbonTests
{
    static void Equal(string expected, string actual)
    { if (expected != actual) throw new Exception("Expected " + expected + ", got " + actual); }
    static int Main()
    {
        try
        {
            // Use real SDK objects without launching a Rhino application or triggering native services.
            var tab = (GH_RibbonTab)FormatterServices.GetUninitializedObject(typeof(GH_RibbonTab));
            foreach (var name in new[] { "m_nameFull", "m_nameShort", "m_nameSymbol" })
                typeof(GH_RibbonTab).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(tab, name == "m_nameSymbol" ? "P" : "Params");
            var panel = (GH_RibbonPanel)FormatterServices.GetUninitializedObject(typeof(GH_RibbonPanel));
            panel.Name = "Geometry";
            var adapter = new RibbonTranslator();
            adapter.ApplyTab(tab, true, false);
            adapter.ApplyPanel(panel, "Params", true, false);
            Equal("参数", tab.NameFull); Equal("参数", tab.NameShort); Equal("几何", panel.Name);
            adapter.ApplyTab(tab, true, true);
            adapter.ApplyPanel(panel, "Params", true, true);
            Equal("参数 · Params", tab.NameFull); Equal("几何 · Geometry", panel.Name);
            adapter.ApplyTab(tab, true, true);
            Equal("参数 · Params", tab.NameFull);
            adapter.ApplyTab(tab, false, false);
            adapter.ApplyPanel(panel, "Params", false, false);
            Equal("Params", tab.NameFull); Equal("Params", tab.NameShort); Equal("P", tab.NameSymbol); Equal("Geometry", panel.Name);
            var thirdParty = (GH_RibbonPanel)FormatterServices.GetUninitializedObject(typeof(GH_RibbonPanel));
            thirdParty.Name = "Geometry";
            adapter.ApplyPanel(thirdParty, "UnknownPlugin", true, false);
            Equal("Geometry", thirdParty.Name);
            Equal("基本类型", RibbonTranslator.Translate("Primitive", false));
            Equal("输入", RibbonTranslator.Translate("Input", false));
            Equal("工具", RibbonTranslator.Translate("Util", false));
            Equal("公鸡", RibbonTranslator.Translate("公鸡", false));
            Console.WriteLine("PASS real SDK ribbon names: Chinese, bilingual, repeated updates, exact restoration, unknown-plugin isolation");
            return 0;
        }
        catch (Exception e) { Console.WriteLine("FAIL " + e); return 1; }
    }
}
