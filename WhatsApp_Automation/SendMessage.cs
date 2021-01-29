using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace WhatsApp_Automation
{
    internal class SendMessage
    {
        private int _contactNum;
        private readonly ChromeDriver _webDriver = new ChromeDriver();

        public void Send(ref List<ContactInfo> contactLst, ref System.Windows.Forms.Label lblCounter,ref StreamWriter logFile)
        {
            //general waiting time for each searching for element 
            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            //open WhatsApp web
            _webDriver.Navigate().GoToUrl("https://web.whatsapp.com/");

            //wait till user scan QR code
            tryAgain:
            try
            {
                Thread.Sleep(2000);
                Console.Beep();

                //search for QR code & if not found jump to catch block, not found mean that user scan QR code
                _webDriver.FindElement(By.XPath("//*[@id=\"app\"]/div/div/div[2]/div[1]/div/div[2]/div/canvas"));

                goto tryAgain;
            }

            catch (Exception)
            {
                var verifyContact = false;
                var verifyPhoto = false;

                foreach (var info in contactLst)
                {
                    if (info.ContactName != "" && info.MsgTxt != "")
                    {
                        verifyContact = SendMsgByContact(info.ContactName, info.MsgTxt);
                        if(!verifyContact)
                            logFile.WriteLine(info.ContactName);
                    }

                    if (verifyContact && info.PhotoPath != "")
                    {
                        verifyPhoto = SendPhotoByContact(info.PhotoPath, info.PhotoDesc);
                    }

                    if (verifyContact) lblCounter.Text = ++_contactNum + @" From " + contactLst.Count;
                }
            }
        }


        //method to send message by each person
        private bool SendMsgByContact(string contactName, string msg)
        {
            //refresh page and check connection before start
            if (!RefreshPage())
            {
                MessageBox.Show("Error in internet connection", "Connection error");
                return false;
            }

            try
            {
                //search for name
                var txtSearch = _webDriver.FindElement(By.XPath("//*[@id=\"side\"]/div[1]/div/label/div/div[2]"));
                txtSearch.SendKeys("");
                txtSearch.SendKeys(contactName);
                //press Enter
                txtSearch.SendKeys(Keys.Enter);

                //write message
                var txtMsgBox =
                    _webDriver.FindElement(By.XPath("//*[@id=\"main\"]/footer/div[1]/div[2]/div/div[2]"));
                txtMsgBox.SendKeys(msg);
                //press Enter
                txtMsgBox.SendKeys(Keys.Enter);

                return true;
            }

            catch (Exception)
            {
                //empty search box if contact not found
                _webDriver.FindElement(By.XPath("/html/body/div[1]/div/div/div[3]/div/div[1]/div/span/button/span"))
                    .Click();
                return false;
            }
        }

        //method send photos
        private bool SendPhotoByContact(string photoPath, string photoDesc)
        {
            //check photo path
            if (!File.Exists(photoPath))
            {
                RefreshPage();
                return false;
            }

            try
            {
                //open menu of sending types
                _webDriver.FindElement(By.XPath("//*[@id=\"main\"]/footer/div[1]/div[1]/div[2]/div/div/span")).Click();

                //click on photo send icon
                _webDriver.FindElement(
                        By.XPath("//*[@id=\"main\"]/footer/div[1]/div[1]/div[2]/div/span/div/div/ul/li[1]/button/span"))
                    .Click();

                //select photos by path
                CreateCodeFile(photoPath);

                //add description to photo
                if (photoDesc != "")
                {
                    _webDriver.FindElement(By.XPath(
                            "//*[@id=\"app\"]/div/div/div[2]/div[2]/span/div/span/div/div/div[2]/div[1]/span/div/div[2]/div/div[3]/div[1]/div[2]"))
                        .SendKeys(photoDesc);
                }

                //click send green arrow
                _webDriver.FindElement(
                        By.XPath(
                            "//*[@id=\"app\"]/div/div/div[2]/div[2]/span/div/span/div/div/div[2]/span/div/div/span"))
                    .Click();

                //wait till photo sent
                Thread.Sleep(5000);

                return true;
            }

            catch (Exception)
            {
                //MessageBox.Show($"The path \"{photoDesc}\" is not valid", @"Error");
                return false;
            }
        }

        //method that create file.au3 in run time to handle local popup window that send photo
        private void CreateCodeFile(string path)
        {
            var strCode = new[]
            {
                "ControlFocus(\"Open\", \"\", \"Edit1\")",
                "ControlSetText(\"Open\", \"\", \"Edit1\", \"" + path + "\")",
                "ControlClick(\"Open\", \"\", \"Button1\")"
            };

            try
            {
                //create new file with extension au3
                var fs = new StreamWriter("SendPhoto.au3");

                //write code line by line
                foreach (var s in strCode)
                {
                    fs.WriteLine(s);
                }

                //save file
                fs.Close();

                //run code file
                Thread.Sleep(5000);
                new Process {StartInfo = new ProcessStartInfo("SendPhoto.au3") {UseShellExecute = true}}.Start();

                //delete file after process complete
                Thread.Sleep(5000);
                File.Delete("SendPhoto.au3");
            }

            catch (Exception)
            {
                MessageBox.Show(@"Error while photo sending", @"Error");
            }
        }

        private bool RefreshPage()
        {
            //try to refresh page then check it 5 times { 5 , 4 , 3 , 2 , 1 }
            var tryRefresh = 5;
            _webDriver.Navigate().Refresh();
            
            refresh:
            try
            {
                //verify refreshing
                _webDriver.FindElement(By.XPath("//*[@id=\"app\"]/div/div/div[4]/div/div/div[2]"));
                return true;
            }

            catch (Exception)
            {
                tryRefresh--;
                if (tryRefresh == 0)
                    return false;

                //wait 5 sec then recheck if refresh finish
                Thread.Sleep(3000);

                goto refresh;
            }
        }
    }
}