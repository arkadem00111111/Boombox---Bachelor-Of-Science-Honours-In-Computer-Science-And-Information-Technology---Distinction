using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Security.Cryptography;
using System.Net;

using NAudio.Wave;

using Newtonsoft.Json.Linq;

using Un4seen.Bass;
using Un4seen.Bass.Misc;


namespace Boombox
{
    public partial class Form_Raw_Metadata_Processing : Form
    {
        private int _handle;
        private int _pos;
        private Visuals _visuals = new Visuals();

        String strFilePath = "";


        WaveIn waveInStream;
        WaveFileWriter writer;


        public Form_Raw_Metadata_Processing()
        {
            InitializeComponent();
        }

        private void Form_Raw_Metadata_Processing_FormClosed(object sender, FormClosedEventArgs e)
        {
            Bass.BASS_Free();
            var Main_Menu = (Form_Main_Menu)Tag;
            Main_Menu.Show();
        }





        private void set_FilePath()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";

                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    strFilePath = openFileDialog.FileName;
                }
            }

        }




        private void create_Graphics(String where)
        {
            _pos = 0;
            timer1.Stop();
            Bass.BASS_Stop();
            Bass.BASS_Free();
            _visuals.ClearPeaks();
            pictureBox1.Image = null;
            pictureBox2.Image = null;

            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, Handle))
            {
                _handle = Bass.BASS_StreamCreateFile(where, 0, 0, BASSFlag.BASS_DEFAULT);

                if (Bass.BASS_ChannelPlay(_handle, false))
                {
                    _visuals = new Visuals();

                    _visuals.ScaleFactorLinear = 20;
                    _visuals.ScaleFactorSqr = 4;
                    timer1.Start();

                }
            }

        }


        private void extract_Metadata(string json)
        {
            // in this method we utilise the Newtonsoft JSON framework to effectively and efficiently deserialize the original JSON metadata provided by ACRCloud
            //The JSON data is very fragmented. It's dangerous to go alone! Take this: https://www.newtonsoft.com/json/help/html/SerializingJSONFragments.htm
            JObject raw = JObject.Parse(json);

            string toMusic = (string)raw["metadata"]["music"].ToString();

            JArray musicArray = JArray.Parse(toMusic);


            //The metadata provided by ACRCloud is contained within multiple arrays and sub arrays.
            //Not all songs have the same ammount of metadata. Thus, we shall work with what is provided.
            //Here we disect the metadata provided.
            //The following information will be attempted to be determined
            string idScore = "";
            string artistsName = "";
            string titleName = "";
            string genreName = "";
            string originalDuration = "";
            string releaseDate = "";

            try
            {
                JObject metadataArray1 = JObject.Parse(musicArray[0].ToString());
                textBox2.Text = metadataArray1.ToString();

                
                try//Determine the audio's ID score
                {
                    idScore = (string)metadataArray1["score"];
                }
                catch (Exception)
                {
                    idScore = "Not Obtained";
                }


                try//Determine the artist's names
                {
                    JArray artistsArray1 = (JArray)metadataArray1["artists"];

                    foreach (JObject artist in artistsArray1)
                    {
                        artistsName = artistsName + artist.GetValue("name").ToString() + ", ";
                    }
                }
                catch (Exception)
                {
                     artistsName = "Not Obtained";
                }


                try//Determine the audio's title
                {
                    titleName = (string)metadataArray1["title"];
                }
                catch (Exception)
                {
                    titleName = "Not Obtained";
                }


                try//Determine the audio's genre(s)
                {
                    JArray genreArray1 = (JArray)metadataArray1["genres"];

                    foreach (JObject genre in genreArray1)
                    {
                        genreName = genreName + genre.GetValue("name").ToString() + ", ";
                    }
                }
                catch (Exception)
                {
                    genreName = "Not Obtained";
                }


                try//Determine the origional audio's duration
                {
                    double time = Convert.ToDouble( (string)metadataArray1["duration_ms"]);
                    TimeSpan t = TimeSpan.FromMilliseconds(time);

                    originalDuration = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Milliseconds);
                }
                catch (Exception)
                {
                    originalDuration = "Not Obtained";
                }


                try//Determine the audio's release date
                {
                    releaseDate = (string)metadataArray1["release_date"];
                }
                catch (Exception)
                {
                    releaseDate = "Not Obtained";
                }



                lbl_IDScore.Visible = true;
                lbl_Title.Visible = true;
                lbl_Artists.Visible = true;
                lbl_Genre.Visible = true;
                lbl_OriginalTrack.Visible = true;
                lbl_ReleaseDate.Visible = true;

                pictureBox3.Visible = true;
                pictureBox4.Visible = true;
                pictureBox5.Visible = true;
                pictureBox6.Visible = true;
                pictureBox7.Visible = true;
                pictureBox8.Visible = true;


                lbl_IDScore.Text = "ID Score: " + idScore + "\r\n";
               lbl_Title.Text = "Title: " + titleName + "\r\n";
               lbl_Artists.Text = "Artist(s): " + artistsName + "\r\n";
               lbl_Genre.Text = "Genre(s): " + genreName + "\r\n";
               lbl_OriginalTrack.Text = "Original track's duration: " + originalDuration + "\r\n";
               lbl_ReleaseDate.Text = "Release Date: " + releaseDate + "\r\n";


            }
            catch (Exception )
            {
                lbl_IDScore.Visible = true;
                pictureBox3.Visible = true;

                lbl_IDScore.Text = "Audio Not Identified";
                lbl_Title.Text = "";
                lbl_Artists.Text = "";
                lbl_Genre.Text = "";
                lbl_OriginalTrack.Text = "";
                lbl_ReleaseDate.Text = "";

                textBox2.Text = "No Data";
            }


        }






        private void btn_Test_Click(object sender, EventArgs e)
        {
            Bass.BASS_Free();
            set_FilePath();

            var config = new Dictionary<string, object>();
            // Replace "XXXXXXXX" below with your project's host, access_key and access_secret
            config.Add("host", "identify-eu-west-1.acrcloud.com");
            config.Add("access_key", "5ba2c776a34e3f69d73603688e9e1f4c");
            config.Add("access_secret", "qXHaJEvNFjMwXMdcioXz2oA0FjITufaCEQUh4W12");
            config.Add("timeout", 10); // seconds
            config.Add("rec_type", ACRCloudRecognizer.RECOGNIZER_TYPE.acr_rec_type_audio);

            ACRCloudRecognizer re = new ACRCloudRecognizer(config);

            string result = "";

            try
            {
                using (FileStream fs = new FileStream(strFilePath, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        byte[] datas = reader.ReadBytes((int)fs.Length);
                        // It will skip 0 seconds from the beginning of datas.
                        result = re.RecognizeByFileBuffer(datas, datas.Length, 0);
                        // This result is written in a JSON (JavaScript Object Notation) data interchange language.
                 
                        try
                        {
                            //Here we transform that result into a more readable format.
                            extract_Metadata(result);

                            create_Graphics(strFilePath);

                            textBox1.Text = result;
                        }
                        catch (Exception)
                        {
                            lbl_IDScore.Visible = true;
                            pictureBox3.Visible = true;

                            textBox1.Text = "No Results Found";

                            lbl_IDScore.Text = "Audio Not Identified";
                            lbl_Title.Text = "";
                            lbl_Artists.Text = "";
                            lbl_Genre.Text = "";
                            lbl_OriginalTrack.Text = "";
                            lbl_ReleaseDate.Text = "";

                            textBox2.Text = "No Data";
                        }
                    }
                }
            }
            catch (Exception)
            {
                textBox1.Text = "The file could not be accessed";
            }
        }



        void waveInStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            writer.WriteData(e.Buffer, 0, e.BytesRecorded);
        }



        private void btn_Lstn_Test_Click(object sender, EventArgs e)
        {
            Bass.BASS_Free();
            btn_Stop.Visible = true;
            btn_Lstn_Test.Visible = false;
            waveInStream = new WaveIn();
            writer = new WaveFileWriter(@"recorded.wav", waveInStream.WaveFormat);
            waveInStream.DataAvailable += new EventHandler<WaveInEventArgs>(waveInStream_DataAvailable);
            waveInStream.StartRecording();
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            waveInStream.StopRecording();
            waveInStream.Dispose();
            waveInStream = null;
            writer.Close();
            writer = null;

            btn_Stop.Visible = false;
            btn_Lstn_Test.Visible = true;

            var config = new Dictionary<string, object>();
            // Replace "XXXXXXXX" below with your project's host, access_key and access_secret
            config.Add("host", "identify-eu-west-1.acrcloud.com");
            config.Add("access_key", "5ba2c776a34e3f69d73603688e9e1f4c");
            config.Add("access_secret", "qXHaJEvNFjMwXMdcioXz2oA0FjITufaCEQUh4W12");
            config.Add("timeout", 10); // seconds
            config.Add("rec_type", ACRCloudRecognizer.RECOGNIZER_TYPE.acr_rec_type_audio);

            ACRCloudRecognizer re = new ACRCloudRecognizer(config);

            string result = "";



            try
            {
                result = re.RecognizeByFile(@"recorded.wav", 0);
                try
                {
                    //Here we transform that result into a more readable format.
                    extract_Metadata(result);

                    create_Graphics(@"recorded.wav");

                    textBox1.Text = result;
                    //Console.WriteLine(result);
                }
                catch (Exception)
                {
                    textBox1.Text = "No Results Found";

                    lbl_IDScore.Visible = true;
                    pictureBox3.Visible = true;

                    lbl_IDScore.Text = "Audio Not Identified";
                    lbl_Title.Text = "";
                    lbl_Artists.Text = "";
                    lbl_Genre.Text = "";
                    lbl_OriginalTrack.Text = "";
                    lbl_ReleaseDate.Text = "";


                    textBox2.Text = "No Data";
                }
            }
            catch (Exception)
            {
                textBox1.Text = "The file could not be accessed";
            }




        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bool spectrum3DVoicePrint = _visuals.CreateSpectrum3DVoicePrint(_handle, pictureBox1.CreateGraphics(),
                                                                           pictureBox1.Bounds, Color.Black, Color.White,
                                                                           _pos, false, false);

            spectrum3DVoicePrint = _visuals.CreateSpectrum3DVoicePrint(_handle, pictureBox2.CreateGraphics(),
                                                                          pictureBox2.Bounds, Color.Black, Color.White,
                                                                          _pos, true, false);
            _pos++;
            if (_pos >= pictureBox1.Width)
            {
                _pos = 0;
            }
        }
    }
}
