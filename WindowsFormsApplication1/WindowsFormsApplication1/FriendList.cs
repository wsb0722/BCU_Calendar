﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApplication1
{
    public partial class FriendList : Form
    {
        DBConnection db = Program.DB;
        DataTable friendTable = null;
        DataTable friend_group_tb = null;
        
        Panel[] pan;
        Label[] label;
        Button[] btn;

        int location;
        int panLotaion;
        int[] flag;

        public FriendList()
        {
            InitializeComponent();
            location = 0;
            panLotaion = 0;
        }

        private Control Create_FriendProfile(int i, DataTable dataTable) //친구 프로필 생성
        {
            DataRow currRow = dataTable.Rows[i];
            UserCustomControl.Profile FriendProfile = new UserCustomControl.Profile();          
            FriendProfile.Set_Profile_Size(30, FontStyle.Bold);
            FriendProfile.USERNAME = currRow["UR_NM"].ToString();
            FriendProfile.Location = new System.Drawing.Point(0, 10 + location * 30);
            FriendProfile.Size = new System.Drawing.Size(150, 25);
            FriendProfile.MouseClick += new MouseEventHandler(mouse_MouseClick);
         //   FriendProfile.SendToBack();


            //FriendProfile.TabIndex = i;
            
           location++; // 전역

            return FriendProfile;
        }
        

        private void UploadeList()
        {
            flag = new int[friendTable.Rows.Count + 1];
            label = new Label[friendTable.Rows.Count];
            btn = new Button[friend_group_tb.Rows.Count + 1];
            pan = new Panel[friend_group_tb.Rows.Count + 1];

            //기본적으로 생성해주는 모든친구 버튼과 목록이 저장되는 판넬 생성 
            btn[0] = new Button();
            btn[0].Font = new System.Drawing.Font("함초롬돋움", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            btn[0].Location = new System.Drawing.Point(20, 10);
            btn[0].Name = "btn";
            btn[0].TabIndex = 0;
            btn[0].Size = new System.Drawing.Size(130, 32);
            btn[0].Text = "기본그룹    ▲";
            btn[0].Click += new EventHandler(GroupList_Click);
            panel1.Controls.Add(btn[0]);
            flag[0] = 1;

            pan[0] = new Panel();
            pan[0].AutoSize = true;
            pan[0].Location = new Point(btn[0].Location.X, btn[0].Location.Y + 30);
           
            panel1.Controls.Add(pan[0]);

            for (int i = 0; i < friendTable.Rows.Count; i++)
            {             
                pan[0].Controls.Add(Create_FriendProfile(i , friendTable));
 
            }
           

            for (int i = 0; i < friend_group_tb.Rows.Count; i++) 
            {
                //int location = 0;
                DataRow currRow;
                currRow = friend_group_tb.Rows[i];
                btn[i + 1] = new Button();
                btn[i + 1].Font = new System.Drawing.Font("함초롬돋움", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
                btn[i + 1].Location = new System.Drawing.Point(20, 45 + location * 40);
                btn[i + 1].Name = "btn" + i.ToString();
                btn[i + 1].Size = new System.Drawing.Size(130, 32);
                btn[i + 1].TabIndex = i+1;
                btn[i + 1].Text = currRow["FRGR_NM"].ToString() + "       ▼";
                btn[i + 1].Click += new EventHandler(GroupList_Click);
              
                btn[i + 1].Visible = true;
                panel1.Controls.Add(btn[i + 1]);
                btn[i + 1].BringToFront();

                location++;
                flag[i + 1] = 0;
            }

        }
        private void GetFriendsList() //친구 목록 가져오기
        {
            db.AdapterOpen("select  UR_NM ,UR_CD from USER_TB  WHERE ur_cd  in (select FR_FR_FK from FRIEND_TB where FR_UR_FK = '" + db.UR_CD + "') ORDER BY  UR_NM ASC");

            DataSet DS = new DataSet();
            db.Adapter.Fill(DS, "friend_tb");
            friendTable = DS.Tables["friend_tb"];

            //db.Close();
        }

        private void FriendList_Load(object sender, EventArgs e)
        {
            button1.MouseEnter += new EventHandler(OnTopPanMouseEnter);
            button3.MouseEnter += new EventHandler(OnTopPanMouseEnter);
            button4.MouseEnter += new EventHandler(OnTopPanMouseEnter);

            button1.MouseEnter += new EventHandler(OnTopPanMouseLeave);
            button3.MouseEnter += new EventHandler(OnTopPanMouseLeave);
            button4.MouseEnter += new EventHandler(OnTopPanMouseLeave);

            db.UR_CD = "U100000";
            panel1.HorizontalScroll.Maximum = 0;
            panel1.VerticalScroll.Maximum = 0;
            panel1.AutoScroll = false;
            panel1.VerticalScroll.Visible = false;
            panel1.AutoScroll = true;

            GetFriendsList();
            GetGroupList();
            UploadeList();
            GetGroupMamber();


        }

        private void GetGroupList()
        {
            db.AdapterOpen("select FRGR_CD, FRGR_NM from FRIEND_GROUP_TB WHERE FRGR_UR_FK = '" + db.UR_CD + "' ORDER BY FRGR_NM DESC");
            DataSet ds = new DataSet();
            db.Adapter.Fill(ds, "friend_group_tb");
            friend_group_tb = ds.Tables["friend_group_tb"];
        }

        private void GetGroupMamber() // 그룹 멤버 반환 
        {

            for (int i = 0; i < friend_group_tb.Rows.Count; i++)  //panal 생성구간 create panel 로뺄까...?
            {
                location = 0;
                pan[i + 1] = new Panel();
                panel1.Controls.Add(pan[i + 1]);
                pan[i + 1].Location = new System.Drawing.Point(btn[i + 1].Location.X, btn[i + 1].Location.Y + 32);
                pan[i + 1].AutoSize = true;
                pan[i + 1].Visible = false;
                DataRow currRow = friend_group_tb.Rows[i];
                db.AdapterOpen("select  UR_NM from USER_TB  where UR_CD in (select FR_FR_FK from FRIEND_TB where FR_FRGR_FK = '" + currRow["FRGR_CD"].ToString() + "') ORDER BY  UR_NM ASC");

                DataSet rs = new DataSet();
                db.Adapter.Fill(rs, "groupMemberTb");
                DataTable groupMemberTb = rs.Tables["groupMemberTb"];

                for (int j = 0; j < groupMemberTb.Rows.Count; j++) // 그룹 목록에 그룹원 추가
                {
                    pan[i + 1].Controls.Add( Create_FriendProfile(j, groupMemberTb));
                }
            }
        }

        private void GroupList_Click(object render, EventArgs e)// 위치 조절 함수
        {
            int i = 0;
            Button mybtn = (Button)render;
            i = mybtn.TabIndex ;
            
          
            if (!pan[i].Visible) //i 번쨰리스트가 보이지 않는 상태이면 ( 리스트를 펼칠때)
            {
                if(i == 0)
                {
                    btn[0].Text= "기본그룹    ▲";
                }
                else
                {
                    i -= 1;
                    DataRow currRow = friend_group_tb.Rows[i];
                    btn[i+1].Text = currRow["FRGR_NM"].ToString() + "     ▲";
                    i += 1;
                }
                                //리스트 펼치는 버튼 밑에만 생각 하면됨 
                //리스트판넬의 위치를 잡을떄 목록버튼 밑에다가 둬서 버튼이동만 생각 
                for (int j = i +1 ; j < friend_group_tb.Rows.Count + 1; j++) //최대 그룹리스트 갯수만큼 +1 은 기본그룹
                {
                    btn[j].Location = new Point(btn[j].Location.X, btn[j].Location.Y + pan[i].Size.Height);
                    pan[j].Location = new System.Drawing.Point(btn[j].Location.X, btn[j].Location.Y + 32); //32는 버튼 size
                }
                pan[i].Visible = true;
            }
            else //리스트를 닫을떄
            {
                if (i == 0)
                {
                    btn[0].Text = "기본그룹     ▼";
                }
                else
                {
                    i -= 1;
                    DataRow currRow = friend_group_tb.Rows[i];
                    btn[i+1].Text = currRow["FRGR_NM"].ToString() + "     ▼";
                    i += 1;
                }
                for (int j = i+1; j < friend_group_tb.Rows.Count + 1; j++)
                {
                    btn[j].Location = new Point(btn[j].Location.X, btn[j].Location.Y - pan[i].Size.Height);
                    pan[j].Location = new System.Drawing.Point(btn[j].Location.X, btn[j].Location.Y + 32);
                }
                pan[i].Visible = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
            location = 0;
            bool check = false;

            string sql = "select  UR_NM ,UR_ID from USER_TB  WHERE ur_cd  in (select FR_FR_FK from FRIEND_TB where FR_UR_FK = '" + db.UR_CD + "') and (UR_NM ='" + textBox1.Text + "' OR UR_ID='" + textBox1.Text + "') ORDER BY  UR_NM ASC" ;

            db.ExecuteReader(sql);
            if (db.Reader.Read())
            {
                db.AdapterOpen(sql);
                DataSet rs = new DataSet();
                db.Adapter.Fill(rs, "search");
                DataTable search = rs.Tables["search"];

                FriendSearch friendSearch = new FriendSearch(search);
                friendSearch.ShowDialog();
                check = true;

            }
         
            if (!check)
            {
                panel1.Visible = false;
                panel2.Visible = true;
            }
            textBox1.Text = "";        
        }

        private void button4_Click(object sender, EventArgs e) // 취소
        {
            panel2.Visible = false;
            panel1.Visible = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
            AddFriend addFriend = new AddFriend();
            addFriend.ShowDialog();
          
        }
        private void mouse_MouseClick(object sender, MouseEventArgs e)
        {

                UserCustomControl.Profile myPan = (UserCustomControl.Profile)sender;
                int i = myPan.TabIndex;
                DataRow currRow = friendTable.Rows[i];
                Point mousePoint = new Point(e.X, e.Y);
                FriendModify friendModify = new FriendModify(currRow, friend_group_tb, mousePoint);
                friendModify.ShowDialog();

        }
 

        private void OnTopPanMouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.BackColor = Color.SlateGray;
        }
        private void OnTopPanMouseLeave(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.BackColor = Color.Transparent;
        }


    }
}



