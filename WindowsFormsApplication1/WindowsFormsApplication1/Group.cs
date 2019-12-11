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
    public partial class Group : Form
    {

        DBConnection db = Program.DB;
        string sql;

        public Group()
        {
            InitializeComponent();

            MasterProfile_prof.Set_Profile_Size(FontStyle.Bold);
        }

        public UserCustomControl.Profile MASTERPROFILE
        {
            // Userprofile 프로퍼티 -> login에서 값 넘겨줌
            get { return MasterProfile_prof; }
            set { MasterProfile_prof = value; }
        }

        private void Close_btn_Click(object sender, EventArgs e)
        {
            this.Parent.Visible = false;
            Close();
        }

        private void Mem_prof_Click(object sender, EventArgs e)
        {
            MessageBox.Show(((UserCustomControl.Profile)sender).Name.Substring(10, ((UserCustomControl.Profile)sender).Name.Length - 10));
        }

        private void Set_Groupbs() // 그룹 기본 정보 설정
        {
            sql = "select GR_NM, GR_EX, UR_NM, UR_PIC from USER_TB, GROUP_TB";
            sql += " where UR_CD = (select GR_MST_UR_FK from GROUP_TB where GR_CD = '" + db.GR_CD + "')";
            sql += " and GR_CD = '" + db.GR_CD + "'";
            db.ExecuteReader(sql);
            if (db.Reader.Read())
            {
                GR_nm_lbl.Text = db.Reader["GR_NM"].ToString();

                // GR_EX 값이 null이 아니라면 설명을 가져와 주세요
                if (!(db.Reader["GR_EX"].Equals(System.DBNull.Value))) GR_ex_lbl.Text = db.Reader["GR_EX"].ToString();
                else GR_ex_lbl.Text = "그룹설명이 없습니다.";

                MasterProfile_prof.USERNAME = db.Reader["UR_NM"].ToString();
                // UR_PIC 값이 null이 아니라면 사진을 가져와 주세요
                if (!(db.Reader["UR_PIC"].Equals(System.DBNull.Value))) MasterProfile_prof.USERPIC = Image.FromStream(db.Reader.GetOracleBlob(3));
            }
        }
        
        List<UserCustomControl.Profile> MemProf_lst;
        List<string> MemCD_lst;
        private void Set_GroupMem() // 그룹원 설정
        {
            sql = "select UR_CD, UR_NM, UR_PIC from USER_TB";
            sql += " where UR_CD in ";
            sql += "(select GRMB_MBR_UR_FK from GROUP_MEMBER_TB";
            sql += " where GRMB_FK = '" + db.GR_CD + "')";
            sql += " order by UR_NM ASC";
            db.ExecuteReader(sql);

            MemProf_lst = new List<UserCustomControl.Profile>();
            MemCD_lst = new List<string>();
            UserCustomControl.Profile Mem_prof;
            while (db.Reader.Read())
            {
                MemCD_lst.Add(db.Reader["UR_CD"].ToString());
                Mem_prof = new UserCustomControl.Profile();
                
                Mem_prof.USERNAME = db.Reader["UR_NM"].ToString();
                if (!(db.Reader["UR_PIC"].Equals(System.DBNull.Value))) Mem_prof.USERPIC = Image.FromStream(db.Reader.GetOracleBlob(2));
                Mem_prof.Name = "GRMem_prof" + (MemProf_lst.Count);
                Mem_prof.Size = new Size(GRMember_pan.Width - 10, 28);
                Mem_prof.Location = new Point(5, MemProf_lst.Count * 30 + 5);
                Mem_prof.Set_Profile_Size(FontStyle.Regular);

                Mem_prof.Click += new System.EventHandler(this.Mem_prof_Click);

                MemProf_lst.Add(Mem_prof);
                GRMember_pan.Controls.Add(Mem_prof);
            }
            Member_lbl.Text = "그룹원 총 " + MemProf_lst.Count + "명";
        }



        private void Group_Load(object sender, EventArgs e)
        {
            Set_Groupbs();
            Set_GroupMem();
        }
    }
}
