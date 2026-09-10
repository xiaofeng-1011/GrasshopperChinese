using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Collections;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using GrasshopperChinese;

class ToolLabelTests
{
    static void Equal(string expected, string actual) { if (expected != actual) throw new Exception("Expected " + expected + ", got " + actual); }
    static int Main()
    {
        try
        {
            var assembly = typeof(NativeTools).Assembly;
            var runtime = assembly.GetType("GrasshopperChinese.Runtime");
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var c = Catalog.Parse("[{\"name\":\"Circle\",\"category\":\"Params\",\"translation\":\"圆\"}]");
            runtime.GetField("catalog", flags).SetValue(null, c);
            runtime.GetField("Enabled", flags).SetValue(null, true);
            runtime.GetField("Bilingual", flags).SetValue(null, false);
            var circle = new Param_Circle();
            var proxy = (IGH_ObjectProxy)FormatterServices.GetUninitializedObject(typeof(IGH_ObjectProxy).Assembly.GetType("Grasshopper.Kernel.GH_CompiledObjectProxy"));
            typeof(IGH_ObjectProxy).Assembly.GetType("Grasshopper.Kernel.GH_CompiledObjectProxy").GetField("m_id", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(proxy, circle.ComponentGuid);
            var recordType = assembly.GetType("GrasshopperChinese.OriginalDescription");
            var record = Activator.CreateInstance(recordType, true);
            foreach (var pair in new[] { new[] { "Name", "Circle" }, new[] { "Nickname", circle.NickName }, new[] { "Category", "Params" } })
                recordType.GetField(pair[0], BindingFlags.Instance | BindingFlags.NonPublic).SetValue(record, pair[1]);
            ((IDictionary)runtime.GetField("originals", flags).GetValue(null)).Add(circle.ComponentGuid, record);
            using (var label = new Label { Text = "  Circle", Tag = proxy })
            {
                var translate = runtime.GetMethod("TranslateToolLabels", flags);
                translate.Invoke(null, new object[] { label }); Equal("  圆", label.Text);
                runtime.GetField("Bilingual", flags).SetValue(null, true);
                translate.Invoke(null, new object[] { label }); Equal("  圆 · Circle", label.Text);
                runtime.GetField("Enabled", flags).SetValue(null, false);
                translate.Invoke(null, new object[] { label }); Equal("  Circle", label.Text);
            }
            using (new TextScope(() => circle.Name, value => circle.Name = value, "圆")) Equal("圆", circle.Name);
            Equal("Circle", circle.Name);
            Console.WriteLine("PASS real control/proxy dropdown Chinese, bilingual, restore; real Circle name restoration");
            return 0;
        }
        catch (Exception e) { Console.WriteLine(e); return 1; }
    }
}
