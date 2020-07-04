using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using ImageCrypto;
using ImageCryptography.Properties;

namespace ImageCryptography
{
  [SuppressMessage("ReSharper", "LocalizableElement")]
  public partial class Form1 : Form
  {
    private static int _rsaP;
    private static int _rsaQ;
    private static int _rsaE;
    private static int _d;
    private static int _n;
    private static string _loadImage = "";
    private static string _loadcipher = "";

    public Form1()
    {
      InitializeComponent();
    }

    private static string GetIpAddress()
    {
      var host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (var ip in host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork))
        return ip.ToString();

      throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      groupBox6.Hide();
      Button_send.Enabled = true;
      Selector_saveImage.Enabled = true;
      txt_strDec.Enabled = true;
      Button_encryptImage.Enabled = true;
      Selector_decrypt.Enabled = true;
    }

    // Function to encrypt the image
    private string EncryptImage(string imageHexToEncrypt)
    {
      MessageBox.Show("RSA_E = " + _rsaE + "\nn = " + _n);
      var imageHex = imageHexToEncrypt;
      var imageHexArray = imageHex.ToCharArray();
      var cond = "";
      Progressbar_encryptImage.Maximum = imageHexArray.Length;
      for (var i = 0; i < imageHexArray.Length; i++)
      {
        Application.DoEvents();
        Progressbar_encryptImage.Value = i;
        if (cond == "")
          cond = cond + RSAalgorithm.BigMod(imageHexArray[i], _rsaE, _n);
        else
          cond = cond + "-" + RSAalgorithm.BigMod(imageHexArray[i], _rsaE, _n);
      }

      return cond;
    }

    // Function to decrypt the image
    private string DecryptImage(string imageToDecryptHex)
    {
      var ImageHex = imageToDecryptHex.ToCharArray();
      var i = 0;
      var decryptResponse = "";
      ProgressBar_decrypt.Maximum = ImageHex.Length;
      try
      {
        for (; i < ImageHex.Length; i++)
        {
          Application.DoEvents();
          var c = "";
          ProgressBar_decrypt.Value = i;
          int temp;
          for (temp = i; ImageHex[temp] != '-'; temp++) c = c + ImageHex[temp];
          i = temp;
          var xx = Convert.ToInt32(c);
          decryptResponse = decryptResponse + (char)RSAalgorithm.BigMod(xx, _d, _n);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      return decryptResponse;
    }

    private void SendKeyButtonOnClick(object sender, EventArgs e)
    {
      var ip = GetIpAddress();
      Selector_decrypt.Enabled = true;
      if (Button_sendPublicKey.Text == "Send Public Key")
      {
        if (prime1TextBox.Text == "" || prime2TextBox.Text == "" || ETextBox.Text == "")
        {
          MessageBox.Show("Enter Valid Details For RSA", "ERROR");
        }
        else
        {
          if (HelperLibrary.IsPrime(Convert.ToInt16(prime1TextBox.Text)))
          {
            _rsaP = Convert.ToInt16(prime1TextBox.Text);
          }
          else
          {
            prime1TextBox.Text = "";
            MessageBox.Show("Enter Prime Number");
            return;
          }

          if (HelperLibrary.IsPrime(Convert.ToInt16(prime2TextBox.Text)))
          {
            _rsaQ = Convert.ToInt16(prime2TextBox.Text);
          }
          else
          {
            prime2TextBox.Text = "";
            MessageBox.Show("Enter Prime Number");
            return;
          }

          _rsaE = Convert.ToInt16(ETextBox.Text);

          // Help taken from SO
          //  Calculating Private Key
          _n = RSAalgorithm.n_value(_rsaP, _rsaQ);
          var phi = RSAalgorithm.cal_phi(_rsaP, _rsaQ);
          _d = RSAalgorithm.cal_privateKey(phi, _rsaE, _n);
          MessageBox.Show(
              "Please Connect to the server IP : " + ip + "\nPublic Key = (" + _rsaE + " ," + _n +
              ")\nPrivate Key = (" + _d + "," + _n + ")", "Alert");
          Button_sendPublicKey.Text = "Receive";

          // Sending Public key
          Console.WriteLine("Sending public key");
          File.WriteAllText("Key.txt", _rsaE + "+" + _n);
          MessageBox.Show("Sending Public key");
        }
      }
      else
      {
        MessageBox.Show("Public Key integrity check passed.");
      }
    }

    private void EncryptImageButtonOnClick(object sender, EventArgs e)
    {
      try
      {
        Selector_source.Enabled = false;
        Disable_all();
        var encryptedImageRes = EncryptImage(_loadImage);
        File.WriteAllText(saveEncryptedFileTextBox.Text, encryptedImageRes);
        MessageBox.Show("Encryption Done");
        Selector_source.Enabled = true;
        Button_send.Enabled = true;
        Enable_all();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void DecryptButtonOnClick(object sender, EventArgs e)
    {
      Disable_all();
      try
      {
        var de = DecryptImage(_loadcipher);
        pictureBox1.Image = HelperLibrary.ConvertByteToImage(HelperLibrary.DecodeHex(de));
        var fi = new FileInfo(txt_strDec.Text);
        label9.Text = "File Name: " + fi.Name;
        label10.Text = "Image Resolution: " + pictureBox1.Image.PhysicalDimension.Height + " X " +
                       pictureBox1.Image.PhysicalDimension.Width;
        pictureBox1.Image.Save(txt_strDec.Text, ImageFormat.Jpeg);
        double imageMb = fi.Length / 1024f / 1024f;
        label11.Text = "Image Size: " + imageMb.ToString(".##") + "MB";
        MessageBox.Show("Image decrypted and Saved");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
        Console.WriteLine(ex.Message);
      }
    }

    private void LoadImageToEncryptButtonOnClick(object sender, EventArgs e)
    {
      _loadImage = BitConverter.ToString(HelperLibrary.ConvertImageToByte(pictureBox1.Image));
      MessageBox.Show("Image Load Successfully");
      groupBox4.Enabled = true;
      Selector_saveImage.Enabled = true;
      Button_encryptImage.Enabled = true;
    }

    private void SaveImageSelectorClick(object sender, EventArgs e)
    {
      var save1 = new SaveFileDialog { Filter = "TEXT|*.txt" };
      if (save1.ShowDialog() == DialogResult.OK)
      {
        saveEncryptedFileTextBox.Text = save1.FileName;
        Button_encryptImage.Enabled = true;
      }
      else
      {
        saveEncryptedFileTextBox.Text = "";
        Button_encryptImage.Enabled = false;
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      var open1 = new OpenFileDialog();
      open1.Filter = "JPG|*.JPG";
      if (open1.ShowDialog() == DialogResult.OK)
      {
        sourceTextBox.Text = open1.FileName;
        pictureBox1.Image = Image.FromFile(sourceTextBox.Text);
        loadImageToEncryptButton.Enabled = true;
        var fi = new FileInfo(sourceTextBox.Text);
        label9.Text = "File Name: " + fi.Name;
        label10.Text = "Image Resolution: " + pictureBox1.Image.PhysicalDimension.Height + " X " +
                       pictureBox1.Image.PhysicalDimension.Width;
        double imageMb = fi.Length / 1024f / 1024f;
        label11.Text = "Image Size: " + imageMb.ToString(".##") + "MB";
      }
      else
      {
        sourceTextBox.Text = "";
        label9.Text = "File Name: ";
        label10.Text = "Image Resolution: ";
        label11.Text = "Image Size: ";
        pictureBox1.Image = Resources.blank;
        loadImageToEncryptButton.Enabled = false;
      }
    }

    private void Disable_all()
    {
      loadImageToEncryptButton.Enabled = false;
      decryptButtonLoader.Enabled = false;
      saveAtSelector.Enabled = false;
      DecryptButton.Enabled = false;
    }

    private void Enable_all()
    {
      Selector_source.Enabled = true;
      loadImageToEncryptButton.Enabled = true;
      groupBox4.Enabled = true;
      Selector_saveImage.Enabled = true;
      Button_encryptImage.Enabled = true;
      Button_send.Enabled = true;
    }

    private void DecryptSelectorClick(object sender, EventArgs e)
    {
      var open1 = new OpenFileDialog { Filter = "TEXT|*.txt" };
      if (open1.ShowDialog() == DialogResult.OK)
      {
        textBox7.Text = open1.FileName;
        sourceTextBox.Text = open1.FileName;
        decryptButtonLoader.Enabled = true;
      }
      else
      {
        textBox7.Text = "";
        decryptButtonLoader.Enabled = false;
      }
    }

    private void DecryptButtonLoaderClick(object sender, EventArgs e)
    {
      _loadcipher = File.ReadAllText(sourceTextBox.Text);
      MessageBox.Show("Load Cipher Successfully");
      groupBox5.Enabled = true;
      groupBox4.Enabled = true;
      saveAtSelector.Enabled = true;
    }

    private void saveAtSelectorClick(object sender, EventArgs e)
    {
      var save1 = new SaveFileDialog();
      save1.Filter = "JPG|*.JPG";
      if (save1.ShowDialog() == DialogResult.OK)
      {
        txt_strDec.Text = save1.FileName;
        DecryptButton.Enabled = true;
      }
      else
      {
        txt_strDec.Text = "";
        DecryptButton.Enabled = false;
      }
    }

    private void SendButtonClick(object sender, EventArgs e)
    {
      try
      {
        MessageBox.Show("Sent successfully to the server.");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
    }

    private void button6_Click_1(object sender, EventArgs e)
    {
      try
      {
        MessageBox.Show("Reading public keys that was recieved.");
        var keys = File.ReadAllText("Key.txt");
        MessageBox.Show(keys, "Keys");
        var pubKeys = keys.Split('+');
        int.TryParse(pubKeys[0], out _rsaE);
        int.TryParse(pubKeys[1], out _n);
        lbl_pubKey.Text = "(" + _rsaE + "," + _n + ")";
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void getPublicKeyTextBoxClick(object sender, MouseEventArgs e)
    {
      getPublicKeyTextBox.Text = "";
    }

    private void creditToolStripMenuItem_Click(object sender, EventArgs e)
    {
      groupBox6.Show();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Application.Exit();
    }

    private void button3_Click_1(object sender, EventArgs e)
    {
      groupBox6.Hide();
    }

    private void PictureBox1_Click(object sender, EventArgs e)
    {
      // Lets not focus on zooming for now
    }
  }
}