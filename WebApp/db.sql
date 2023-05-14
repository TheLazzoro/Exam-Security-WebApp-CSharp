create schema `exam-security`;
use `exam-security`;

create table db_user (
	id bigint NOT NULL AUTO_INCREMENT,
    username varchar(255) NOT NULL UNIQUE,
    passwd varchar(255) NOT NULL,
    user_role varchar(64),
	PRIMARY KEY (id)
);

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