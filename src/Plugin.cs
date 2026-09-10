// UI entry point adapted from MultilingualGH Menu.cs (MIT, Victor Lin).
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Ribbon;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;

[assembly: AssemblyTitle("GrasshopperChinese")]
[assembly: AssemblyVersion("0.3.0.0")]
[assembly: AssemblyFileVersion("0.3.0.0")]

namespace GrasshopperChinese
{
    public sealed class PluginInfo : GH_AssemblyInfo
    {
        public override string Name { get { return "GrasshopperChinese"; } }
        public override string Description { get { return "Grasshopper 简体中文界面预览版"; } }
        public override Guid Id { get { return new Guid("531059ef-a238-48c3-ac68-6827ab8fd66f"); } }
        public override string AuthorName { get { return "GrasshopperChinese contributors"; } }
        public override string AuthorContact { get { return ""; } }
        public override Bitmap Icon { get { return null; } }
    }

    public sealed class Loader : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            if (Rhino.RhinoApp.ExeVersion != 8) return GH_LoadingInstruction.Abort;
            Instances.CanvasCreated += Runtime.Attach;
            if (Instances.ActiveCanvas != null) Runtime.Attach(Instances.ActiveCanvas);
            return GH_LoadingInstruction.Proceed;
        }
    }

    internal sealed class OriginalDescription
    {
        internal IGH_ObjectProxy Proxy;
        internal string Name, Nickname, Category, Description;
    }

    internal static class Runtime
    {
        internal static bool Enabled = true;
        internal static bool Bilingual;
        static Catalog catalog;
        static Catalog toolText;
        static readonly Dictionary<Guid, OriginalDescription> originals = new Dictionary<Guid, OriginalDescription>();
        static readonly Dictionary<ToolStripItem, string> menuOriginals = new Dictionary<ToolStripItem, string>();
        static readonly HashSet<GH_Canvas> canvases = new HashSet<GH_Canvas>();
        static readonly Dictionary<GH_Canvas, List<TextScope>> paintScopes = new Dictionary<GH_Canvas, List<TextScope>>();
        static readonly GH_SettingsServer settings = new GH_SettingsServer("GrasshopperChinese", true);
        static Timer setupTimer;
        static Timer ribbonTimer;
        static readonly RibbonTranslator ribbonTranslator = new RibbonTranslator();
        static GH_Ribbon ribbon;
        static GH_DocumentEditor editor;
        static ToolStripMenuItem enabledItem, bilingualItem;
        static bool initialized;
        static readonly string folder = Path.GetDirectoryName(typeof(Runtime).Assembly.Location);
        static readonly Dictionary<string, string> menuText = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "File", "文件" }, { "Edit", "编辑" }, { "View", "视图" }, { "Display", "显示" },
            { "Solution", "求解" }, { "Help", "帮助" }, { "New Document", "新建文档" },
            { "New", "新建" }, { "Open Document", "打开文档" }, { "Open", "打开" },
            { "Save Document", "保存文档" }, { "Save", "保存" }, { "Save As", "另存为" },
            { "Save Document As", "文档另存为" }, { "Close", "关闭" }, { "Close Document", "关闭文档" },
            { "Undo", "撤销" }, { "Redo", "重做" }, { "Cut", "剪切" }, { "Copy", "复制" },
            { "Paste", "粘贴" }, { "Delete", "删除" }, { "Select All", "全选" },
            { "Preferences", "首选项" }, { "Special Folders", "特殊文件夹" },
            { "Components Folder", "组件文件夹" }, { "Settings Folder", "设置文件夹" },
            { "User Object Folder", "用户对象文件夹" }, { "Export", "导出" },
            { "Preview", "预览" }, { "Draw Icons", "显示图标" }, { "Draw Full Names", "显示完整名称" },
            { "Enable Solver", "启用求解器" }, { "Recompute", "重新计算" },
            { "Disable Preview", "禁用预览" }, { "Wireframe Preview", "线框预览" },
            { "Shaded Preview", "着色预览" }, { "Preview Selected Only", "仅预览所选对象" },
            { "Zoom", "缩放" }, { "Zoom Extents", "缩放至全部对象" }, { "Zoom Selected", "缩放至所选对象" },
            { "Group", "编组" }, { "Ungroup", "取消编组" }, { "Bake", "烘焙" },
            { "About", "关于" }, { "About Grasshopper", "关于 Grasshopper" }
        };

        internal static void Log(Exception exception)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GrasshopperChinese");
                Directory.CreateDirectory(path);
                File.AppendAllText(Path.Combine(path, "errors.log"), DateTime.UtcNow.ToString("o") + " " + exception + Environment.NewLine);
            }
            catch { System.Diagnostics.Debug.WriteLine(exception); }
        }

        internal static void Attach(GH_Canvas canvas)
        {
            if (!canvases.Add(canvas)) return;
            canvas.DocumentChanged += delegate { Observe(canvas); };
            canvas.CanvasPrePaintObjects += BeginDisplayNames;
            canvas.CanvasPaintEnd += EndDisplayNames;
            canvas.Disposed += delegate { EndDisplayNames(canvas); canvases.Remove(canvas); };
            if (setupTimer != null || initialized) { if (initialized) Observe(canvas); return; }
            setupTimer = new Timer { Interval = 250 };
            setupTimer.Tick += delegate
            {
                if (Instances.DocumentEditor == null) return;
                setupTimer.Stop();
                setupTimer.Dispose();
                setupTimer = null;
                try { Initialize(); }
                catch (Exception ex) { Enabled = false; Log(ex); MessageBox.Show("中文界面加载失败，已保留英文界面。\n" + ex.Message, "Grasshopper 中文界面"); }
            };
            setupTimer.Start();
        }

        static void Initialize()
        {
            // Load and validate the complete pack before changing the host UI.
            catalog = Catalog.Parse(File.ReadAllText(Path.Combine(folder, "Languages", "zh-CN.json")));
            toolText = Catalog.Parse(File.ReadAllText(Path.Combine(folder, "Languages", "tool-text.zh-CN.json")));
            Enabled = settings.GetValue("Enabled", true);
            Bilingual = settings.GetValue("Bilingual", false);
            editor = Instances.DocumentEditor;
            var root = new ToolStripMenuItem("中文界面") { Name = "GrasshopperChineseMenu" };
            enabledItem = new ToolStripMenuItem("启用界面汉化") { Checked = Enabled, CheckOnClick = true };
            bilingualItem = new ToolStripMenuItem("中英对照") { Checked = Bilingual, CheckOnClick = true };
            enabledItem.Click += delegate { Enabled = enabledItem.Checked; Update(); };
            bilingualItem.Click += delegate { Bilingual = bilingualItem.Checked; Update(); };
            root.DropDownItems.Add(enabledItem);
            root.DropDownItems.Add(bilingualItem);
            root.DropDownItems.Add("刷新组件目录", null, delegate { CollectProxies(); Update(); });
            root.DropDownItems.Add("导出原生工具及缺译清单", null, delegate { ExportCatalog(); });
            root.DropDownItems.Add(new ToolStripSeparator());
            root.DropDownItems.Add("使用说明", null, delegate
            {
                MessageBox.Show("Grasshopper 中文界面 0.3.0 预览版\n\n" +
                    "汉化已匹配的主菜单、组件目录和标准原生组件标题。\n" +
                    "关闭“显示 > 显示图标 / Display > Draw Icons”可查看文字标题。\n" +
                    "自定义昵称保留。特殊绘制组件、参数说明及动态报错暂未覆盖。\n" +
                    "取消勾选启用界面汉化可恢复英文。\n\n" +
                    "词库条目：" + catalog.Count + "（包含未逐条审核的上游转换译文）。\n" +
                    "基于 MultilingualGH 的 MIT 翻译资源与加载入口开发。",
                    "Grasshopper 中文界面", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            editor.MainMenuStrip.Items.Add(root);
            CollectProxies();
            CaptureMenus(editor.MainMenuStrip.Items);
            initialized = true;
            Update();
            ribbonTimer = new Timer { Interval = 300 };
            ribbonTimer.Tick += delegate { RefreshRibbon(); RefreshDropdowns(); TranslateTooltip(); };
            ribbonTimer.Start();
            editor.Disposed += delegate { ribbonTimer.Stop(); ribbonTimer.Dispose(); };
        }

        static void CollectProxies()
        {
            foreach (IGH_ObjectProxy proxy in Instances.ComponentServer.ObjectProxies)
            {
                if (originals.ContainsKey(proxy.Guid) || proxy.Type == null) continue;
                // Do not apply generic name matches to unrelated third-party components.
                var assemblyName = proxy.Type.Assembly.GetName().Name;
                if (!NativeTools.Contains(assemblyName)) continue;
                originals.Add(proxy.Guid, new OriginalDescription { Proxy = proxy, Name = proxy.Desc.Name, Nickname = proxy.Desc.NickName, Category = proxy.Desc.Category, Description = proxy.Desc.Description });
            }
        }

        static string Format(string chinese, string english) { return Bilingual ? chinese + " · " + english : chinese; }

        internal static bool Lookup(IGH_DocumentObject component, out string translated, out string originalNickname)
        {
            translated = null; originalNickname = null;
            OriginalDescription original;
            if (!Enabled || catalog == null || !originals.TryGetValue(component.ComponentGuid, out original)) return false;
            var text = catalog.Find(original.Name, original.Category);
            if (text == null) return false;
            translated = Format(text, original.Name); originalNickname = original.Nickname; return true;
        }

        static void CaptureMenus(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                if (item.Name == "GrasshopperChineseMenu") continue;
                if (!menuOriginals.ContainsKey(item))
                {
                    menuOriginals.Add(item, item.Text);
                    item.Disposed += delegate { menuOriginals.Remove(item); };
                    var dropdown = item as ToolStripMenuItem;
                    if (dropdown != null) dropdown.DropDownOpening += delegate { CaptureMenus(dropdown.DropDownItems); TranslateMenus(); };
                }
                var menu = item as ToolStripMenuItem;
                if (menu != null) CaptureMenus(menu.DropDownItems);
            }
        }

        static void TranslateMenus()
        {
            foreach (var pair in menuOriginals)
            {
                if (pair.Key.IsDisposed) continue;
                var english = pair.Value;
                string key = english.Replace("&", "").Trim().TrimEnd('.', '…');
                string chinese;
                if (!menuText.TryGetValue(key, out chinese)) continue;
                if (!Enabled) { pair.Key.Text = english; continue; }
                int access = english.IndexOf('&');
                string accelerator = access >= 0 && access + 1 < english.Length ? " (&" + english[access + 1] + ")" : "";
                string ellipsis = english.EndsWith("...") ? "..." : english.EndsWith("…") ? "…" : "";
                pair.Key.Text = Format(chinese, key) + accelerator + ellipsis;
            }
        }

        static void Update()
        {
            try
            {
                foreach (var canvas in canvases) EndDisplayNames(canvas);
                GH_Tooltip.Clear();
                foreach (var original in originals.Values)
                {
                    var translated = Enabled ? catalog.Find(original.Name, original.Category) : null;
                    // Display mode applies to both the catalog and already-created dropdown labels.
                    original.Proxy.Desc.Name = translated == null ? original.Name : Format(translated, original.Name);
                    var description = Enabled ? toolText.Find(original.Description, "description") : null;
                    original.Proxy.Desc.Description = description == null ? original.Description : Format(description, original.Description);
                    // Keep the canonical nickname; existing user-nickname checks depend on it.
                }
                TranslateMenus();
                RefreshRibbon();
                RefreshDropdowns();
                foreach (var canvas in canvases) { Observe(canvas); canvas.Refresh(); }
                editor.Refresh();
                settings.SetValue("Enabled", Enabled);
                settings.SetValue("Bilingual", Bilingual);
                settings.WritePersistentSettings();
            }
            catch (Exception ex) { Log(ex); }
        }

        static void RefreshDropdowns()
        {
            foreach (Form form in Application.OpenForms)
                if (form is GH_RibbonDropdown) TranslateToolLabels(form);
        }

        static void TranslateToolLabels(Control parent)
        {
            var proxy = parent.Tag as IGH_ObjectProxy;
            OriginalDescription original;
            if (proxy != null && originals.TryGetValue(proxy.Guid, out original))
            {
                var text = Enabled ? catalog.Find(original.Name, original.Category) : null;
                string desired = "  " + (text == null ? original.Name : Format(text, original.Name));
                if (parent.Text != desired) { parent.Text = desired; parent.Invalidate(); }
            }
            foreach (Control child in parent.Controls) TranslateToolLabels(child);
        }

        static void BeginDisplayNames(GH_Canvas canvas)
        {
            EndDisplayNames(canvas);
            if (!Enabled || canvas.Document == null) return;
            var scopes = new List<TextScope>();
            paintScopes[canvas] = scopes;
            try
            {
                foreach (var obj in canvas.Document.Objects)
                {
                    string text, nick;
                    if (Lookup(obj, out text, out nick))
                        scopes.Add(new TextScope(() => obj.Name, value => obj.Name = value, text));
                }
            }
            catch (Exception ex) { EndDisplayNames(canvas); Log(ex); }
        }

        static void EndDisplayNames(GH_Canvas canvas)
        {
            List<TextScope> scopes;
            if (!paintScopes.TryGetValue(canvas, out scopes)) return;
            try { for (int i = scopes.Count - 1; i >= 0; i--) scopes[i].Dispose(); }
            finally { paintScopes.Remove(canvas); }
        }

        static void TranslateTooltip()
        {
            if (!Enabled || toolText == null) return;
            try
            {
                var tag = GH_Tooltip.Tag;
                var obj = tag as IGH_DocumentObject;
                var attributes = tag as IGH_Attributes;
                if (obj == null && attributes != null) obj = attributes.DocObject;
                var proxy = tag as IGH_ObjectProxy;
                Guid id = obj != null ? obj.ComponentGuid : proxy != null ? proxy.Guid : Guid.Empty;
                OriginalDescription original;
                if (!originals.TryGetValue(id, out original)) return;
                var tooltip = GH_Tooltip.TooltipForm;
                if (tooltip == null || !tooltip.Visible) return;
                string title = catalog.Find(original.Name, original.Category);
                string description = toolText.Find(original.Description, "description");
                bool changed = false;
                if (title != null && tooltip.TT_Title == original.Name) { tooltip.TT_Title = Format(title, original.Name); changed = true; }
                if (description != null && tooltip.TT_Description == original.Description) { tooltip.TT_Description = Format(description, original.Description); changed = true; }
                if (changed) { GH_Tooltip.Layout(); tooltip.Refresh(); }
            }
            catch (Exception ex) { Log(ex); }
        }

        static void ExportCatalog()
        {
            using (var dialog = new SaveFileDialog { Filter = "JSON|*.json", FileName = "grasshopper-native-coverage.json" })
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;
                var rows = new List<Catalog.ExportEntry>();
                foreach (var original in originals.Values)
                {
                    var row = new Catalog.ExportEntry { Guid = original.Proxy.Guid.ToString(), Name = original.Name, Category = original.Category, Description = original.Description,
                        Translation = catalog.Find(original.Name, original.Category), DescriptionTranslation = toolText.Find(original.Description, "description") };
                    try
                    {
                        var component = original.Proxy.CreateInstance() as IGH_Component;
                        if (component != null)
                        {
                            foreach (var p in component.Params.Input) row.Inputs.Add(p.Name + " | " + p.Description);
                            foreach (var p in component.Params.Output) row.Outputs.Add(p.Name + " | " + p.Description);
                        }
                    }
                    catch (Exception ex) { row.Error = ex.Message; }
                    rows.Add(row);
                }
                using (var stream = File.Create(dialog.FileName)) new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(List<Catalog.ExportEntry>)).WriteObject(stream, rows);
            }
        }

        static GH_Ribbon FindRibbon(Control parent)
        {
            var result = parent as GH_Ribbon;
            if (result != null) return result;
            foreach (Control child in parent.Controls) { result = FindRibbon(child); if (result != null) return result; }
            return null;
        }

        static void RefreshRibbon()
        {
            try
            {
                if (ribbon == null || ribbon.IsDisposed) ribbon = FindRibbon(editor);
                if (ribbon == null) return;
                bool changed = false;
                foreach (var tab in ribbon.Tabs)
                {
                    var category = ribbonTranslator.OriginalCategory(tab);
                    string before = tab.NameFull;
                    ribbonTranslator.ApplyTab(tab, Enabled, Bilingual);
                    changed |= before != tab.NameFull;
                    foreach (var panel in tab.Panels)
                    {
                        before = panel.Name;
                        ribbonTranslator.ApplyPanel(panel, category, Enabled, Bilingual);
                        changed |= before != panel.Name;
                    }
                }
                if (changed) { ribbon.PerformLayout(); ribbon.Refresh(); }
            }
            catch (Exception ex) { Log(ex); if (ribbonTimer != null) ribbonTimer.Stop(); }
        }

        static void Observe(GH_Canvas canvas)
        {
            if (!initialized || canvas.Document == null) return;
            var document = canvas.Document;
            document.SolutionStart -= BeforeSolution;
            document.SolutionStart += BeforeSolution;
            document.ObjectsAdded -= ObjectsAdded;
            document.ObjectsAdded += ObjectsAdded;
            foreach (var obj in document.Objects) Adapt(obj);
        }

        static void BeforeSolution(object sender, GH_SolutionEventArgs args)
        { foreach (var canvas in canvases) EndDisplayNames(canvas); }

        static void ObjectsAdded(object sender, GH_DocObjectEventArgs e)
        { foreach (var obj in e.Objects) Adapt(obj); }

        static void Adapt(IGH_DocumentObject obj)
        {
            var component = obj as GH_Component;
            if (component == null)
            {
                var parameter = obj as IGH_Param;
                if (parameter == null || parameter.Attributes == null) return;
                if (parameter.Attributes is ChineseParameterAttributes) { parameter.Attributes.ExpireLayout(); return; }
                string text, nick;
                if (parameter.Attributes.GetType() != typeof(GH_FloatingParamAttributes) || !Lookup(parameter, out text, out nick)) return;
                var previousParameter = parameter.Attributes;
                parameter.Attributes = new ChineseParameterAttributes(parameter) { Pivot = previousParameter.Pivot, Selected = previousParameter.Selected };
                parameter.Attributes.ExpireLayout();
                return;
            }
            string translated, nickname;
            if (component.Attributes is ChineseAttributes) { component.Attributes.ExpireLayout(); return; }
            if (!Lookup(component, out translated, out nickname) || component.Attributes == null || component.Attributes.GetType() != typeof(GH_ComponentAttributes)) return;
            var previous = component.Attributes;
            component.Attributes = new ChineseAttributes(component) { Pivot = previous.Pivot, Selected = previous.Selected };
            component.Attributes.ExpireLayout();
        }
    }

    internal sealed class ChineseParameterAttributes : GH_FloatingParamAttributes
    {
        internal ChineseParameterAttributes(IGH_Param owner) : base(owner) { }
        void WithText(Action action)
        {
            string text, nick;
            if (!Runtime.Lookup(Owner, out text, out nick)) { action(); return; }
            using (new TextScope(() => Owner.Name, value => Owner.Name = value, text))
            using (new TextScope(() => Owner.NickName, value => Owner.NickName = value, DisplayText.Nickname(Owner.NickName, nick, text))) action();
        }
        protected override void Layout() { WithText(delegate { base.Layout(); }); }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        { WithText(delegate { base.Render(canvas, graphics, channel); }); }
        public override void SetupTooltip(PointF point, GH_TooltipDisplayEventArgs args)
        { WithText(delegate { base.SetupTooltip(point, args); }); }
    }

    // Replaces the standard text rendering itself, not an annotation above it.
    // All persistent component fields are restored before Layout/Render returns.
    internal sealed class ChineseAttributes : GH_ComponentAttributes
    {
        internal ChineseAttributes(GH_Component owner) : base(owner) { }
        void WithTranslatedText(Action action)
        {
            string translated, nickname;
            if (!Runtime.Lookup(Owner, out translated, out nickname)) { action(); return; }
            using (new TextScope(() => Owner.Name, value => Owner.Name = value, translated))
            using (new TextScope(() => Owner.NickName, value => Owner.NickName = value, DisplayText.Nickname(Owner.NickName, nickname, translated)))
                action();
        }
        protected override void Layout() { WithTranslatedText(delegate { base.Layout(); }); }
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        { WithTranslatedText(delegate { base.Render(canvas, graphics, channel); }); }
    }
}
