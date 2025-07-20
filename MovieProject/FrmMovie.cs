using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace MovieProject
{
    public partial class FrmMovie : Form
    {
        // สร้างตัวแปรเก็บรูปภาพของภาพยนต์
        string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=movie_collection_db;Trusted_Connection=True";

        // สร้างตัวแปรเก็บรูปที่แปลงเป็น byte array ลง DB
        byte[] movieImage;
        byte[] movieDirectorImage;

        int movieId = 0; // Variable to member ID

        public FrmMovie()
        {
            InitializeComponent();
        }

        private void resetForm()
        {
            
            btDeleteMovie.Enabled = false;
            btUpdateMovie.Enabled = false;
            lbMovieId.Text = "";
            tbMovieName.Text = "";
            tbMovieDetail.Text = "";
            dtpMovieDate.Value = DateTime.Now;
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
            cbbMovieType.SelectedIndex = 0; // ล้างการเลือกประเภทภาพยนต์
            pcbMovieImage.Image = null; // ล้างรูปภาพของภาพยนต์
            pcbMovieDirectorImage.Image = null; // ล้างรูปภาพของผู้กำกับภาพยนต์


        }
        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }

        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        private void getAllMoiveToListView()
        {
            // Connect String เพื่อเชื่อมต่อฐานข้อมูล ตามยี่ห้อของฐานข้อมูลที่ใช้
            //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True";
            // Create connection object ไปยังฐานข้อมูลที่ต้องการ
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open(); // เปิดการเชื่อมต่อกับฐานข้อมูล

                    // SELECT, INSERT, UPDATE, DELETE
                    // สร้างคำสั่ง SQL เพื่อดึงข้อมูลจากตาราง product_tb
                    string strSQL = "SELECT movieId, movieImage, movieName, movieDetail, movieDate, movieType FROM movie_tb";

                    // สร้าง SqlCommand เพื่อรันคำสั่ง SQL
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        // สร้าง DataTable แปลงจากเป็นก้อนมาเป็นตาราง
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // ตั้งค่าทั่วไปของ All Movie ListView
                        lvShowAllMovie.Items.Clear(); // ล้างข้อมูลเก่าใน ListView
                        lvShowAllMovie.Columns.Clear(); // ล้างคอลัมน์เก่าใน ListView
                        lvShowAllMovie.FullRowSelect = true; // เลือกแถวทั้งหมดเมื่อคลิกที่แถวใดแถวหนึ่ง
                        lvShowAllMovie.View = View.Details; // ตั้งค่าให้แสดงผลแบบรายละเอียด

                        // ตั้งค่าทั่วไปของ Search Movie ListView
                        lvShowSearchMovie.Items.Clear(); // ล้างข้อมูลเก่าใน ListView
                        lvShowSearchMovie.Columns.Clear(); // ล้างคอลัมน์เก่าใน ListView
                        lvShowSearchMovie.FullRowSelect = true; // เลือกแถวทั้งหมดเมื่อคลิกที่แถวใดแถวหนึ่ง
                        lvShowSearchMovie.View = View.Details; // ตั้งค่าให้แสดงผลแบบรายละเอียด

                        // ตั้งค่าการแสดงรูปใน ListView
                        if (lvShowAllMovie.SmallImageList == null)
                        {
                            lvShowAllMovie.SmallImageList = new ImageList();
                            lvShowAllMovie.SmallImageList.ImageSize = new Size(40, 40); // กำหนดขนาดของรูปภาพ
                            lvShowAllMovie.SmallImageList.ColorDepth = ColorDepth.Depth32Bit; // กำหนดความลึกของสี
                        }
                        lvShowAllMovie.SmallImageList.Images.Clear(); // ล้างรูปภาพเก่าใน ImageList

                        // กำหนดรายละเอียดของ Column ใน ListView
                        lvShowAllMovie.Columns.Add("รูปภาพยนต์", 100, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowAllMovie.Columns.Add("ฃื่อภาพยนต์", 200, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowAllMovie.Columns.Add("รายละเอียดหนัง", 150, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowAllMovie.Columns.Add("วันที่เข้าฉาย", 100, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowAllMovie.Columns.Add("ประเภทภาพยนต์", 100, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่

                        // กำหนดรายละเอียดของ Column ใน Search ListView
                        lvShowSearchMovie.Columns.Add("รหัสภาพยนต์", 80, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowSearchMovie.Columns.Add("ฃื่อภาพยนต์", 200, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่


                        // LOOP เพื่อเพิ่มข้อมูลจาก DataTable ลงใน ListView
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem(); // สร้าง item เก็บข้อมูลแต่ละรายการ

                            Image movieImage = null; // ตัวแปรสำหรับเก็บรูปภาพ
                            if (dataRow["movieImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["movieImage"];
                                // แปลงข้อมูลรูปภาพจากฐานข้อมูลเป็น byte array
                                movieImage = convertByteArrayToImage(imgByte); // แปลง byte array เป็น Image
                            }

                            string imagekey = null;// ตัวแปรสำหรับเก็บ key ของรูปภาพ
                            if (movieImage != null)
                            {
                                imagekey = $"movie_{dataRow["movieId"]}"; // สร้าง key สำหรับรูปภาพ
                                lvShowAllMovie.SmallImageList.Images.Add(imagekey, movieImage); // เพิ่มรูปภาพลงใน ImageList
                                item.ImageKey = imagekey; // กำหนด key ของรูปภาพให้กับ item
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }
                            //เพิ่มรายการลงใน item ตามข้อมูลใน DataRow

                            item.SubItems.Add(dataRow["movieName"].ToString());
                            item.SubItems.Add(dataRow["movieDetail"].ToString());
                            item.SubItems.Add(Convert.ToDateTime(dataRow["movieDate"]).ToString("dd/MM/yyyy"));
                            item.SubItems.Add(dataRow["movieType"].ToString());

                            // เพิ่ม item ลงใน ListView
                            lvShowAllMovie.Items.Add(item);

                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                }
            }
        }

        private void FrmMovie_Load(object sender, System.EventArgs e)
        {
            resetForm();
            getAllMoiveToListView(); // เรียกใช้เมธอดเพื่อดึงข้อมูลภาพยนต์ทั้งหมดมาแสดงใน ListView
        }

        private void showWarningMessage(string message)
        {
            MessageBox.Show(message, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btSaveMovie_Click(object sender, EventArgs e)
        {
            if (tbMovieName.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกชื่อภาพยนต์");
            }
            else if (tbMovieDetail.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกรายละเอียดภาพยนต์");
            }
            else if (nudMovieHour.Value == 0 && nudMovieMinute.Value == 0)
            {
                showWarningMessage("กรุณากรอกเวลาความยาวของภาพยนต์");
            }
            else if (pcbMovieImage == null)
            {
                showWarningMessage("กรุณาเลือกรูปภาพของภาพยนต์");
            }
            else if (pcbMovieDirectorImage == null)
            {
                showWarningMessage("กรุณาเลือกรูปภาพของผู้กำกับภาพยนต์");
            }
            else
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    try
                    {
                        sqlConnection.Open(); // เปิดการเชื่อมต่อกับฐานข้อมูล

                        // For Insert, Update, Delete
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSQL = "INSERT INTO movie_tb (movieName, movieDetail, movieDate, movieHour, movieMinute, movieType, movieImage, movieDirectorImage) " +
                                        "VALUES (@movieName, @movieDetail, @movieDate, @movieHour, @movieMinute, @movieType, @movieImage, @movieDirectorImage)";

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@movieName", SqlDbType.NVarChar, 150).Value = tbMovieName.Text;
                            sqlCommand.Parameters.Add("@movieDetail", SqlDbType.NVarChar, 500).Value = tbMovieDetail.Text;
                            sqlCommand.Parameters.Add("@movieDate", SqlDbType.Date).Value = dtpMovieDate.Value;
                            sqlCommand.Parameters.Add("@movieHour", SqlDbType.Int).Value = nudMovieHour.Value;
                            sqlCommand.Parameters.Add("@movieMinute", SqlDbType.Int).Value = nudMovieMinute.Value;
                            sqlCommand.Parameters.Add("@movieType", SqlDbType.NVarChar, 150).Value = cbbMovieType.SelectedItem;
                            sqlCommand.Parameters.Add("@movieImage", SqlDbType.Image).Value = movieImage;
                            sqlCommand.Parameters.Add("@movieDirectorImage", SqlDbType.Image).Value = movieDirectorImage;

                            // รันคำสั่ง SQL
                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show("บันทึกข้อมูลภาพยนต์เรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMoiveToListView();
                            resetForm();
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถบันทึกข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                    }
                }


            }
        }

        private void btMovieImage_Click(object sender, EventArgs e)
        {
            // open file dialog เพื่อเลือกไฟล์รูปภาพ jpg, png
            // ถ้าเลือกไฟล์ได้ ให้แสดงรูปภาพใน pcbMovieImage
            // แปลงเป็น byte array เก็บไว้ในตัวแปรเพื่อใช้ในการบันทึกฐานข้อมูล
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\\"; // กำหนดโฟลเดอร์เริ่มต้น Drive C
            openFileDialog.Filter = "Image Files|*.jpg;*.png;";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // แสดงรูปภาพใน PictureBox
                pcbMovieImage.Image = Image.FromFile(openFileDialog.FileName);

                // ตรวจสอบ Formant ของรูปภาพ แล้วแปลงเป็น byte array
                if (pcbMovieImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    movieImage = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    movieImage = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Png);
                }
            }
        }

        private void btMovieDirectorImage_Click(object sender, EventArgs e)
        {
            // open file dialog เพื่อเลือกไฟล์รูปภาพ jpg, png
            // ถ้าเลือกไฟล์ได้ ให้แสดงรูปภาพใน pcbMovieDirectorImage
            // แปลงเป็น byte array เก็บไว้ในตัวแปรเพื่อใช้ในการบันทึกฐานข้อมูล
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\\"; // กำหนดโฟลเดอร์เริ่มต้น Drive C
            openFileDialog.Filter = "Image Files|*.jpg;*.png;";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // แสดงรูปภาพใน PictureBox
                pcbMovieDirectorImage.Image = Image.FromFile(openFileDialog.FileName);

                // ตรวจสอบ Formant ของรูปภาพ แล้วแปลงเป็น byte array
                if (pcbMovieDirectorImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    movieDirectorImage = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    movieDirectorImage = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Png);
                }
            }
        }

        private void btSearchMovie_Click(object sender, EventArgs e)
        {

        }
    }
}
