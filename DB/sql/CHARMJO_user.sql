-- USER_TB ?��?�� --

-- UR_CD ?��?�� seq_urcd ?��???�� ?��?�� --
create sequence seq_urcd
start with 100000
increment by 1;

-- field ?��?�� --
insert into USER_TB values('U'||to_char(seq_urcd.NEXTVAL), 'testuser1', '1234', 'ȫ�浿');
insert into USER_TB values('U'||to_char(seq_urcd.NEXTVAL), 'testuser2', '1234', '��浿');
insert into USER_TB values('U'||to_char(seq_urcd.NEXTVAL), 'testuser3', '1234', '�ֱ浿');
insert into USER_TB values('U'||to_char(seq_urcd.NEXTVAL), 'testuser4', '1234', '���浿');