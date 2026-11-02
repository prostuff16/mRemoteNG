using mRemoteNG.App;
using mRemoteNG.Messages;
using mRemoteNG.UI.Forms;
using mRemoteNG.UI.Window;
using System;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using mRemoteNG.Resources.Language;

namespace mRemoteNG.UI.Panels
{
    public class PanelAdder
    {
        public ConnectionWindow AddPanel(string title = "")
        {
            try
            {
                var connectionForm = new ConnectionWindow(new DockContent());
                BuildConnectionWindowContextMenu(connectionForm);
                SetConnectionWindowTitle(title, connectionForm);
                ShowConnectionWindow(connectionForm);
                PrepareTabSupport(connectionForm);
                return connectionForm;
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddMessage(MessageClass.ErrorMsg,
                                                    "Couldn\'t add panel" + Environment.NewLine + ex.Message);
                return null;
            }
        }

        public bool DoesPanelExist(string panelName)
        {
            return Runtime.WindowList?.OfType<ConnectionWindow>().Any(w => w.TabText == panelName)
                ?? false;
        }

        private static void ShowConnectionWindow(ConnectionWindow connectionForm)
        {
            connectionForm.Show(FrmMain.Default.pnlDock, DockState.Document);
        }

        private static void PrepareTabSupport(ConnectionWindow connectionForm)
        {
            Runtime.WindowList.Add(connectionForm);
        }

        private static void SetConnectionWindowTitle(string title, ConnectionWindow connectionForm)
        {
            if (string.IsNullOrEmpty(title))
                title = Language.NewPanel;
            connectionForm.SetFormText(title.Replace("&", "&&"));
        }

        private static void BuildConnectionWindowContextMenu(DockContent pnlcForm)
        {
            var cMen = new ContextMenuStrip();
            var cMenRen = CreateRenameMenuItem(pnlcForm);
            var cMenScreens = CreateScreensMenuItem(pnlcForm);
            var cMenClose = CreateCloseMenuItem(pnlcForm);
            cMen.Items.AddRange(new ToolStripItem[] {cMenRen, cMenScreens, cMenClose});
            pnlcForm.TabPageContextMenuStrip = cMen;
        }

        private static ToolStripMenuItem CreateScreensMenuItem(DockContent pnlcForm)
        {
            var cMenScreens = new ToolStripMenuItem
            {
                Text = Language.SendTo,
                Image = Properties.Resources.Monitor_16x,
                Tag = pnlcForm
            };
            cMenScreens.DropDownItems.Add("Dummy");
            cMenScreens.DropDown.ShowItemToolTips = true;
            cMenScreens.DropDownOpening += (sender, e) => PopulateScreensMenu(cMenScreens, pnlcForm);
            cMenScreens.DropDownClosed += (sender, e) => ScreenIdentifier.Hide();
            return cMenScreens;
        }

        private static ToolStripMenuItem CreateRenameMenuItem(DockContent pnlcForm)
        {
            var cMenRen = new ToolStripMenuItem
            {
                Text = Language.Rename,
                Image = Properties.Resources.Rename_16x,
                Tag = pnlcForm
            };
            cMenRen.Click += cMenConnectionPanelRename_Click;
            return cMenRen;
        }

        private static ToolStripMenuItem CreateCloseMenuItem(DockContent pnlcForm)
        {
            var cMenClose = new ToolStripMenuItem
            {
                Text = Language._Close,
                Image = Properties.Resources.Close_16x,
                Tag = pnlcForm
            };
            cMenClose.Click += cMenConnectionPanelClose_Click;
            return cMenClose;
        }

        private static void cMenConnectionPanelRename_Click(object sender, EventArgs e)
        {
            try
            {
                var conW = (ConnectionWindow)((ToolStripMenuItem)sender).Tag;

                using (var newTitle = new FrmInputBox(Language.NewTitle, Language.NewTitle + ":", ""))
                    if (newTitle.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(newTitle.returnValue))
                        conW.SetFormText(newTitle.returnValue.Replace("&", "&&"));
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace("cMenConnectionPanelRename_Click: Caught Exception: ",
                    ex);
            }
        }

        private static void cMenConnectionPanelClose_Click(object sender, EventArgs e)
        {
            try
            {
                var conW = (ConnectionWindow)((ToolStripMenuItem)sender).Tag;
                conW.Close();
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace("cMenConnectionPanelClose_Click: Caught Exception: ",
                    ex);
            }
        }

        private static void PopulateScreensMenu(ToolStripMenuItem cMenScreens, DockContent panel)
        {
            try
            {
                cMenScreens.DropDownItems.Clear();

                var rdpFullscreenByDefault = Properties.Settings.Default.SendToScreenRdpFullscreen;
                var screens = Screen.AllScreens;
                for (var i = 0; i <= screens.Length - 1; i++)
                {
                    var screen = screens[i];
                    var screenNumber = i + 1;
                    var cMenScreen = new ToolStripMenuItem(Language.Screen + " " + Convert.ToString(screenNumber))
                    {
                        Image = Properties.Resources.Monitor_16x,
                        ToolTipText = rdpFullscreenByDefault
                            ? Language.SendToRightClickWindowedHint
                            : Language.SendToRightClickFullscreenHint
                    };
                    // right-click does the opposite of the configured default. Click fires for right-clicks too but
                    // doesn't carry the button, so it is captured on MouseDown (keyboard activation has no MouseDown)
                    var rightClicked = false;
                    cMenScreen.MouseDown += (sender, e) => rightClicked = e.Button == MouseButtons.Right;
                    cMenScreen.MouseEnter += (sender, e) => ScreenIdentifier.Show(screen, screenNumber);
                    cMenScreen.MouseLeave += (sender, e) => ScreenIdentifier.Hide();
                    cMenScreen.Click += (sender, e) =>
                    {
                        var invert = rightClicked;
                        rightClicked = false;
                        cMenConnectionPanelScreen_Click(panel, screen, rdpFullscreenByDefault != invert);
                    };
                    cMenScreens.DropDownItems.Add(cMenScreen);
                }
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace(
                                                                "cMenConnectionPanelScreens_DropDownOpening: Caught Exception: ",
                                                                ex);
            }
        }

        private static void cMenConnectionPanelScreen_Click(DockContent panel, Screen screen, bool rdpFullscreen)
        {
            try
            {
                ScreenIdentifier.Hide();
                Screens.SendPanelToScreen(panel, screen, rdpFullscreen);
            }
            catch (Exception ex)
            {
                Runtime.MessageCollector.AddExceptionStackTrace("cMenConnectionPanelScreen_Click: Caught Exception: ",
                                                                ex);
            }
        }
    }
}