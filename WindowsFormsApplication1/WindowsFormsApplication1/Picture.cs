﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Oracle.DataAccess.Client; // 오라클파라미터 쓸라면 추가해야함 왠만하면 DBConnection 으로 하고싶지만

namespace WindowsFormsApplication1
{

    public partial class Picture : Form
    {
        bool isZoomBtn;

        DBConnection db = Program.DB;
        DataSet PictureDS = null;
        DataTable PictureDT = null;
        DateTime preDate;
        bool isLastPictue;

        int pictureLocation;
        int rowNum;
        int currWheel;

        Panel insidePan;

        public Picture()
        {
            InitializeComponent();
            // 생성시 초기화
            pictureLocation = 0;
            rowNum = 0;
            currWheel = 0;
            isLastPictue = false;
            isZoomBtn = true;
            // 패널 안에 똑같은 패널을 만든다 
            // why? : 바깥 패널 안에있는 패널에 사진을넣고 위치값을 바꿀경우 위치가 변해도 바깥 패널밖으로 나가지않음
            insidePan = new Panel();
            insidePan.Size = new Size(Picture_pan.Size.Width, 999999);
            insidePan.Location = new Point(0, 0);
            // 안 패널을 바깥패널에 넣는다
            Picture_pan.Controls.Add(insidePan);
        }

        private void Picture_Load(object sender, EventArgs e)
        { // 실행했을때 바로 사진이 띄우는 메서드실행
            PictureLoad();
            preDate = new DateTime(); // PictureShow 에서 preDate로 현재날짜와 전날짜를 비교한다
            PictureShow();
        }

        private void FileSave(string filePath)
        {
            // 입력받은 파일을 바이트 배열에 저장
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] b = new byte[fs.Length - 1];
            fs.Read(b, 0, b.Length);
            fs.Close();

            // 배열에 있는 데이터를 보낼 정보를 담는다
            OracleParameter op = new OracleParameter();
            op.ParameterName = ":BINARYFILE";
            op.OracleDbType = Oracle.DataAccess.Client.OracleDbType.Blob;
            op.Direction = ParameterDirection.Input;
            op.Size = b.Length;
            op.Value = b;
            db.Command.CommandType = CommandType.Text;
            db.Command.Parameters.Add(op); // 커맨드에 이 파라미터를 추가시켜서 db에 보낼때 같이 보낼수있게 함

            db.ExecuteReader("select TO_CHAR(SEQ_PICCD.nextval) from dual"); // 다음 시퀀스 값 불러오기
            string currSeq = null;
            if (db.Reader.Read())
                currSeq = db.Reader.GetString(0);

            db.ExecuteNonQuery("insert into PICTURE_TB values('P'||SEQ_PICCD.currval, '1', sysdate, '" + db.UR_CD + "', null, :BINARYFILE)");
            db.Command.Parameters.Remove(op); // 삭제를 꼭 시켜야한다 안하면 사진생성을 두번이상 실행안됨
        }


        public void PictureSave()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "사진추가";
            ofd.Filter = "모든 사진파일|*.jpg;*.png|JPG 파일 (*.jpg)|*.jpg|PNG 파일 (*.png)|*.png"; // 필터 형식

            DialogResult dr = ofd.ShowDialog();

            if (dr == DialogResult.OK) // 대화상자에서 OK를 누르면
            {
                try
                {
                    // 파일명 설정 추가해야함
                    string file = @ofd.FileName; // 눌러진 사진의 path
                    FileInfo info = new FileInfo(file);

                    if (info.Length > 2000000) // 1당 byte, 바이트가 너무 큰 사진은 막음
                    {
                        MessageBox.Show("사진 용량이 너무 큽니다 \n" + info.Length.ToString() + " byte > " + "1000000 byte");
                        return;
                    }

                    if (MessageBox.Show("사진을 올리시겠습니까?", "사진등록", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                    {
                        return;
                    }

                    FileSave(file);
                    MessageBox.Show("사진이 등록되었습니다", "사진등록", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    PictureClear();
                    PictureLoad();
                    PictureShow();


                }
                catch (Exception e)
                {
                    MessageBox.Show("파일을 불러오는데 실패하였습니다\n" + e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PictureClear() // 사진 위치나 휠, 페널 위치 초기화 -> 안해주면 그 전 위치들이랑 꼬이게된다
        {
            insidePan.Controls.Clear();
            insidePan.Location = new Point(0, 0);
            pictureLocation = 0;
            rowNum = 0;
            currWheel = 0;
            isLastPictue = false;
            preDate = new DateTime();
        }
        private void PictureLoad() // 폼이 처음 실행되거나 사진추가했을때 사진목록을 불러옴
        {
            db.AdapterOpen("select * from PICTURE_TB where PIC_UR_FK = '" + db.UR_CD + "' ORDER BY PIC_DT DESC");
            PictureDS = new DataSet();
            db.Adapter.Fill(PictureDS, "PICTURE_TB");
            PictureDT = PictureDS.Tables["PICTURE_TB"];
        }

        private void PictureShow() // 사진목록을 받아서 채워진 Table을 한줄씩 받아 사진을 만듦
        {
            if (isZoomBtn) // 크게보기
            {
                int i = 0;
                while (i < 10 && rowNum < PictureDT.Rows.Count)
                {
                    DataRow currRow = PictureDT.Rows[rowNum++]; // 행 하나씩 받아옴
                    string date = currRow["PIC_DT"].ToString();
                    int year = Convert.ToInt32(date.Substring(0, 4));
                    int month = Convert.ToInt32(date.Substring(5, 2));
                    int day = Convert.ToInt32(date.Substring(8, 2));
                    DateTime currDate = new DateTime(year, month, day);

                    if (!preDate.Equals(currDate)) // 전에 설정해둔 날짜와 현재날짜가 같지않으면 새 날짜 레이블 생성
                    {
                        preDate = currDate;
                        Label lb = new Label();
                        insidePan.Controls.Add(lb);
                        lb.Text = preDate.ToString("yyyy년 MM월 dd일");
                        lb.Location = new Point(10, pictureLocation);
                        lb.AutoSize = true;
                        lb.Size = new System.Drawing.Size(60, 24);
                        lb.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        lb.Show();

                        pictureLocation += 34; // 라벨 y값 + y 10 띄우기 해서 34
                    }

                    // 사진을 2진데이터화 하고 Image 클래스로 만든다
                    Byte[] b = (Byte[])(currRow["PIC_DATA"]);
                    MemoryStream stmBlobData = new MemoryStream(b);
                    Image img = Image.FromStream(stmBlobData);

                    // 다음 사진이 들어갈 위치와 이미지를 인자로 보내 사진을 만들고 그 다음 사진 위치를 받음
                    pictureLocation = CreateZoomPicure(pictureLocation, img);
                    i++;
                }
            }
            else // 목록보기
            {
                int i = 0;
                int pictureCount = 0;
                int maxHeight = 0;
                while (i < 40 && rowNum < PictureDT.Rows.Count)
                {
                    DataRow currRow = PictureDT.Rows[rowNum++]; // 행 하나씩 받아옴
                    string date = currRow["PIC_DT"].ToString();
                    int year = Convert.ToInt32(date.Substring(0, 4));
                    int month = Convert.ToInt32(date.Substring(5, 2));
                    int day = Convert.ToInt32(date.Substring(8, 2));
                    DateTime currDate = new DateTime(year, month, day);

                    if (!preDate.Equals(currDate)) // 전에 설정해둔 Month와 비교
                    {
                        pictureLocation = maxHeight;
                        preDate = currDate;
                        Label lb = new Label();
                        insidePan.Controls.Add(lb);
                        lb.Text = preDate.ToString("yyyy년 MM월");
                        lb.Location = new Point(10, pictureLocation);
                        lb.AutoSize = true;
                        lb.Size = new System.Drawing.Size(60, 24);
                        lb.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        lb.Show();
                        pictureCount = 0;

                        pictureLocation += 34; // 라벨 y값 + y 10 띄우기 해서 34
                    }

                    // 사진을 2진데이터화 하고 Image 클래스로 만든다
                    Byte[] b = (Byte[])(currRow["PIC_DATA"]);
                    MemoryStream stmBlobData = new MemoryStream(b);
                    Image img = Image.FromStream(stmBlobData);

                    // 다음 사진이 들어갈 위치와 이미지를 인자로 보내 사진을 만들고 그 다음 사진 위치를 받음
                    int currHeight = CreateSmallPicure(pictureLocation, img, pictureCount++);
                    maxHeight = maxHeight < currHeight ? currHeight : maxHeight;
                    if (pictureCount > 2)
                    {
                        pictureCount = 0;
                        pictureLocation = maxHeight;
                    }
                    i++;
                }
                if (rowNum >= PictureDT.Rows.Count) // 사진이 모두 출력되면
                    isLastPictue = true;
            }
        }
        private int CreateSmallPicure(int location, Image img, int count)
        {
            PictureBox pb = new PictureBox();
            insidePan.Controls.Add(pb); // 페널 안에 넣는다

            // 사진 비율 조정 : made by seungbeen
            float percent = (float)img.Width / 120;
            int imgHeight = (int)((float)img.Height / percent);
            pb.Size = new Size(120, imgHeight);
            pb.Location = new Point(count > 0 ? (count * 125) : 0, location);
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            //pb.BorderStyle = BorderStyle.FixedSingle;
            pb.Image = img;
            pb.Show();
            return location + imgHeight + 10; // 기본 y + 사진 y + 띄울공간 10
        }

        private int CreateZoomPicure(int location, Image img)
        {
            PictureBox pb = new PictureBox();
            insidePan.Controls.Add(pb); // 페널 안에 넣는다

            // 사진 비율 조정 : made by seungbeen
            float percent = (float)img.Width / 365;
            int imgHeight = (int)((float)img.Height / percent);

            pb.Size = new Size(365, imgHeight);
            pb.Location = new Point(0, location);
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            //pb.BorderStyle = BorderStyle.FixedSingle;
            pb.Image = img;
            pb.Show();
            return location + imgHeight + 10; // 기본 y + 사진 y + 띄울공간 10
        }

        private void label4_DoubleClick(object sender, EventArgs e)
        {
            PictureSave();
        }

        private void Picture_pan_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) // 마우스 휠을 올릴때
            {
                if (currWheel >= 0) // 휠을 끝까지 올렸을때 더이상 이동하면 안됨
                    return;
                currWheel += 10; // 10칸씩 증가시킨후 페널을 10씩 + 한다
                insidePan.Location = new Point(insidePan.Location.X, insidePan.Location.Y + 10);

            }
            else // 마우스 휠을 내렸을때
            {
                if (isZoomBtn)
                {
                    if (currWheel <= (pictureLocation * -1) + Picture_pan.Height) // 마지막 사진까지 오면 더이상 내릴수없음
                        return;
                }
                else
                {
                    if (currWheel <= (pictureLocation * -1) + Picture_pan.Height - 70) // 마지막 사진까지 오면 더이상 내릴수없음
                        return;
                }
                currWheel -= 10;
                insidePan.Location = new Point(insidePan.Location.X, insidePan.Location.Y - 10);

            }
            if ((currWheel <= (pictureLocation * -1) + Picture_pan.Height + 50) && !isLastPictue)
            // 끝까지 가기 50전이거나 마지막사진이 아니면 남아있는 사진들 더 출력함 (사진은 10장기준으로 끊킴)
            {
                PictureShow();
            }
            label7.Text = pictureLocation.ToString();
            label5.Text = currWheel.ToString();
            label6.Text = ((pictureLocation * -1) + Picture_pan.Height + 50).ToString();
        }

        private void m_Small_btn_Click(object sender, EventArgs e)
        {
            if (isZoomBtn) // 줌버튼 클릭시
            {
                Label temp = new Label();
                temp.BackColor = m_Zoom_btn.BackColor;
                temp.ForeColor = m_Zoom_btn.ForeColor;
                m_Zoom_btn.BackColor = m_Small_btn.BackColor;
                m_Zoom_btn.ForeColor = m_Small_btn.ForeColor;
                m_Small_btn.BackColor = temp.BackColor;
                m_Small_btn.ForeColor = temp.ForeColor;
                PictureClear();
                isZoomBtn = false;
                PictureLoad();
                preDate = new DateTime(); // PictureShow 에서 preDate로 현재날짜와 전날짜를 비교한다
                PictureShow();
            }
        }

        private void m_Zoom_btn_Click(object sender, EventArgs e)
        {
            if (!isZoomBtn) // 목록보기 버튼 클릭시
            {
                Label temp = new Label();
                temp.BackColor = m_Zoom_btn.BackColor;
                temp.ForeColor = m_Zoom_btn.ForeColor;
                m_Zoom_btn.BackColor = m_Small_btn.BackColor;
                m_Zoom_btn.ForeColor = m_Small_btn.ForeColor;
                m_Small_btn.BackColor = temp.BackColor;
                m_Small_btn.ForeColor = temp.ForeColor;
                PictureClear();
                isZoomBtn = true;
                PictureLoad();
                preDate = new DateTime(); // PictureShow 에서 preDate로 현재날짜와 전날짜를 비교한다
                PictureShow();
            }
        }
    }

}