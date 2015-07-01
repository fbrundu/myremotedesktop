using SharedStuff;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    internal partial class ServerWindowSelector : Form
    {
        private ServerState _state;
        private IDictionary<IntPtr, String> _openWindows;

        internal ServerWindowSelector(ServerState pState)
        {
            InitializeComponent();

            this._state = pState;

            this._openWindows = OpenWindowsGetter.GetOpenWindows();

            foreach (var lWindow in this._openWindows)
            {
                this.WindowsListBox.Items.Add(lWindow.Value);
            }
        }

        private void ServerWindowSelector_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.ReleaseResources();
        }

        
        private void UpdateButton_Click(object sender, EventArgs e)
        {
            this._openWindows = OpenWindowsGetter.GetOpenWindows();

            this.WindowsListBox.Items.Clear();
            foreach (var lWindow in this._openWindows)
            {
                this.WindowsListBox.Items.Add(lWindow.Value);
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (this.WindowsListBox.SelectedIndex != -1)
            {
                String lWindowName = (String)this.WindowsListBox.Items[this.WindowsListBox.SelectedIndex];

                foreach (var lWindow in this._openWindows)
                {
                    if (lWindow.Value.Equals(lWindowName))
                    {
                        this._state.hWnd = lWindow.Key;
                        break;
                    }
                }
            }
            this.ReleaseResources();
        }

        private void CancelSelectionButton_Click(object sender, EventArgs e)
        {
            this.ReleaseResources();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            this._state.hWnd = IntPtr.Zero;
            this._state.SelectedWindowOffsetX = 0;
            this._state.SelectedWindowOffsetY = 0;
        }

        private void ReleaseResources()
        {
            this.Dispose();
        }
    }
}
