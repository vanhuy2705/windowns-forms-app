using System;
using System.Windows.Forms;

namespace SportZone.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (LoginForm login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    if (Session.AppSession.Current != null && Session.AppSession.Current.IsAuthenticated)
                    {
                        if (Session.AppSession.Current.BatBuocDoiMatKhau)
                        {
                            MessageBox.Show("Tài khoản chưa hoàn tất đổi mật khẩu bắt buộc.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        Application.Run(new MainForm(login.SelectedRole, login.DisplayName));
                    }
                }
            }
        }
    }
}
