create schema `exam-security`;
use `exam-security`;

create table db_user (
	id bigint NOT NULL AUTO_INCREMENT,
    username varchar(255) NOT NULL UNIQUE,
    passwd varchar(255) NOT NULL,
    role_Id bigint NOT NULL REFERENCES db_role(id),
    user_image varchar(260), # Note: We may need to increase this field if filepaths get really long.
	PRIMARY KEY (id)
);

create table db_login_attempts (
	user_id bigint NOT NULL REFERENCES db_user(id),
    username varchar(255) NOT NULL REFERENCES db_user(username),
    attempts int,
    captcha varchar(255)
);

create table db_role (
	id bigint NOT NULL AUTO_INCREMENT,
    rolename varchar(255) NOT NULL UNIQUE,
	PRIMARY KEY (id)
);
insert into db_role (rolename) values('user');
insert into db_role (rolename) values('admin');

create table db_forum_thread (
	id bigint NOT NULL AUTO_INCREMENT,
    title varchar(255) NOT NULL,
    content varchar(255) NOT NULL,
    user_Id bigint NOT NULL REFERENCES db_user(id),
    PRIMARY KEY (id)
);

create table db_forum_thread_post (
	id bigint NOT NULL AUTO_INCREMENT,
    content varchar(255) NOT NULL,
    user_Id bigint not null REFERENCES db_user(id),
    forum_thread_Id bigint NOT NULL REFERENCES db_forum_thread(id),
    PRIMARY KEY (id)
);

create table db_refresh_token (
	user_Id bigint NOT NULL REFERENCES db_user(id),
    token varchar(255) NOT NULL
);